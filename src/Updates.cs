/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;

namespace PaintDotNet
{
    /// <summary>
    /// This class handles our auto update system.
    /// </summary>
    /// <remarks>
    /// versions.txt schema:
    ///     ; This is a comment
    ///     DownloadPageUrl=downloadPageUrl                  // This should link to the main download page
    ///     StableVersions=version1,version2,...,versionN    // A comma-separated list of all available stable versions available for download
    ///     BetaVersions=version1,version2,...,versionN      // A comma-separated list of all available beta/pre-release versions available for download
    ///     version1_Name=name1                              // Friendly name for a given version
    ///     version1_NetFxVersion=netFxVersion1              // What version of .NET does this version require?
    ///     version1_InfoUrl=infoUrl1                        // A URL that contains information about the given version
    ///     version1_ZipUrl=zipUrl1                          // A URL to download a ZIP containing the given version
    ///     version1_ZipUrlSize=zipUrlSize1                  // An integer specifying how large the download is, in bytes.
    ///     version1_FullZipUrl=zipFullUrl1                  // A URL to download a ZIP containing the given version w/ its required version of .NET
    ///     version1_FullZipUrlSize=zipFullUrlSize1          // An integer specifying how large the full download is, in bytes.
    ///     ...
    ///     versionN_Name=name1                              // Friendly name for a given version
    ///     versionN_NetFxVersion=netFxVersionN              // What version of .NET does this version require?
    ///     versionN_InfoUrl=infoUrlN                        // A URL that contains information about the given version
    ///     versionN_ZipUrl=zipUrlN                          // A URL to download a ZIP containing the given version
    ///     versionN_ZipUrlSize=zipUrlSizeN                  // An integer specifying how large the download is, in bytes.
    ///     versionN_FullZipUrl=zipFullUrlN                  // A URL to download a ZIP containing the given version w/ its required version of .NET
    ///     versionN_FullZipUrlSize=zipFullUrlSizeN          // An integer specifying how large the full download is, in bytes.
    ///     
    /// Example:
    ///     ; Paint.NET versions download manifest
    ///     DownloadPageUrl=http://www.eecs.wsu.edu/paint.net/download.htm
    ///     StableVersions=2.1.1958.27164
    ///     BetaVersions=2.5.2013.31044
    ///     
    ///     2.1.1958.27164_Name=Paint.NET v2.1b     
    ///     2.1.1958.27164_InfoUrl=http://www.eecs.wsu.edu/paint.net/roadmap.htm#v2_1
    ///     2.1.1958.27164_NetFxVersion=1.1.4322
    ///     2.1.1958.27164_ZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_1b.zip
    ///     2.1.1958.27164_ZipUrlSize=5398528
    ///     2.1.1958.27164_FullZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_1b_Full.zip
    ///     2.1.1958.27164_FullZipUrlSize=27770728
    ///     
    ///     2.5.2013.31044_Name=Paint.NET v2.5        
    ///     2.5.2013.31044_InfoUrl=http://www.eecs.wsu.edu/paint.net/roadmap.htm#v2_5
    ///     2.5.2013.31044_NetFxVersion=1.1.4322
    ///     2.5.2013.31044_ZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_5.zip
    ///     2.5.2013.31044_ZipUrlSize=5100000
    ///     2.5.2013.31044_FullZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_5_Full.zip
    ///     2.5.2013.31044_FullZipUrlSize=45100000
    ///     
    ///     2.6.2113.23752_Name=Paint.NET v2.6 Beta 1
    ///     2.6.2113.23752_InfoUrl=http://www.eecs.wsu.edu/paint.net/roadmap.htm#v2_6
    ///     2.6.2113.23752_NetFxVersion=2.0.50727
    ///     2.6.2113.23752_ZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_6_Beta1.zip
    ///     2.6.2113.23752_ZipUrlSize=5500000
    ///     2.6.2113.23752_FullZipUrl=http://www.eecs.wsu.edu/paint.net/zip/PaintDotNet_2_5_Beta5_Full.zip
    ///     2.6.2113.23752_FullZipUrlSize=75100000
    ///     
    /// Notes:
    ///     A line may have a comment on it. Just start the line with an asterisk, '*'
    ///     Versions must be formatted in a manner parseable by the System.Version class.
    ///     BetaVersions may be an empty list: "BetaVersions="
    ///     versionN_InfoUrl may not be blank
    ///     versionN_ZipUrl may not be blank
    ///     versionN_ZipUrlSize must be greater than 0.
    ///     If any error is detected while parsing, the entire schema will be declared as invalid and ignored.
    ///     Everything is case-sensitive.
    /// </remarks>
    public class Updates
    {
        // {0} is schema version
        // {1} is platform (x86, x64, ia64)
        private const string versionManifestUrlFormat = "http://www.eecs.wsu.edu/paint.net/updates/versions.{0}.{1}.{2}.txt";
        private const string versionManifestTestUrl = "http://www.eecs.wsu.edu/paint.net/updates/versions.txt.test.txt";
        private const int schemaVersion = 2;

        private static string VersionManifestUrl
        {
            get
            {
                string versionManifestUrl;

                if (PdnInfo.IsTestMode)
                {
                    versionManifestUrl = versionManifestTestUrl;
                }
                else
                {
                    string schemaVersionStr = schemaVersion.ToString(CultureInfo.InvariantCulture);
                    Version osVersion = Environment.OSVersion.Version;
                    ProcessorArchitecture platform = SystemLayer.Processor.Architecture;
                    OSType osType = SystemLayer.OS.Type;

                    // If this is XP x64, we want to fudge the NT version to be 5.1 instead of 5.2
                    // This helps us discern between XP x64 and Server 2003 x64 stats.
                    if (osVersion.Major == 5 && osVersion.Minor == 2 && platform == ProcessorArchitecture.X64 && osType == OSType.Workstation)
                    {
                        osVersion = new Version(5, 1, osVersion.Build, osVersion.Revision);
                    }

                    int osVersionInt = (osVersion.Major * 100) + osVersion.Minor;
                    string osVersionStr = osVersionInt.ToString(CultureInfo.InvariantCulture);
                    string platformStr = platform.ToString().ToLower();
                    versionManifestUrl = string.Format(versionManifestUrlFormat, schemaVersionStr, osVersionStr, platformStr);
                }

                return versionManifestUrl;
            }
        }

        // Beta and alpha builds should check every day
        // Final builds should check every 5 days
        public static int UpdateCheckIntervalDays
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 5;
                }
                else
                {
                    return 1;
                }
            }
        }

        // If the build is final and less than 1 week old, then do NOT auto check for updates, no matter what.
        // This may help alleviate any release-day flooding
        // Pre-release builds have no such minimum time before checking.
        public static int MinBuildAgeForUpdateChecking
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return 7;
                }
                else
                {
                    return 0;
                }
            }
        }

        // If the build is over 2 years old, then cease checking for updates.
        // Either we've stopped putting out new builds, or the user doesn't want to update,
        // or the user hardly ever uses the app anyway.
        // Check Now... will still continue to function.
        public const int MaxBuildAgeForUpdateChecking = 2 * 365;

        private const string downloadPageUrlName = "DownloadPageUrl";
        private const string stableVersionsName = "StableVersions";
        private const string betaVersionsName = "BetaVersions";
        private const string nameNameFormat = "{0}_Name";
        private const string netFxVersionNameFormat = "{0}_NetFxVersion";
        private const string infoUrlNameFormat = "{0}_InfoUrl";
        private const string zipUrlNameFormat = "{0}_ZipUrl";
        private const string zipUrlSizeNameFormat = "{0}_ZipUrlSize";
        private const string fullZipUrlNameFormat = "{0}_FullZipUrl";
        private const string fullZipUrlSizeNameFormat = "{0}_FullZipUrlSize";
        private const char commentChar = ';';

        private static byte[] DownloadSmallFile(Uri uri, WebProxy proxy)
        {
            WebRequest request = WebRequest.Create(uri);

            if (proxy != null)
            {
                request.Proxy = proxy;
            }

            request.Timeout = 5000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

            try
            {
                byte[] buffer = new byte[8192];
                int offset = 0;

                while (offset < buffer.Length)
                {
                    int bytesRead = stream.Read(buffer, offset, buffer.Length - offset);

                    if (bytesRead == 0)
                    {
                        byte[] smallerBuffer = new byte[offset + bytesRead];
                        
                        for (int i = 0; i < offset + bytesRead; ++i)
                        {
                            smallerBuffer[i] = buffer[i];
                        }

                        buffer = smallerBuffer;
                    }

                    offset += bytesRead;
                }

                return buffer;
            }

            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }

                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }        
        }

        /// <summary>
        /// Downloads a small file (max 8192 bytes) and returns it as a byte array.
        /// </summary>
        /// <returns>The contents of the file if downloaded successfully.</returns>
        private static byte[] DownloadSmallFile(Uri uri)
        {
            byte[] bytes = null;
            Exception exception = null;
            WebProxy[] proxies = Network.GetProxyList();

            foreach (WebProxy proxy in proxies)
            {
                try
                {
                    bytes = DownloadSmallFile(uri, proxy);
                    exception = null;
                }

                catch (Exception ex)
                {
                    exception = ex;
                    bytes = null;
                }

                if (bytes != null)
                {
                    break;
                }
            }

            if (exception != null)
            {
                WebException we = exception as WebException;

                if (we != null)
                {
                    throw new WebException(null, we, we.Status, we.Response);
                }
                else
                {
                    throw new ApplicationException("An exception occurred while trying to download '" + uri.ToString() + "'", exception);
                }
            }

            return bytes;
        }

        public static void DownloadFile(Uri uri, Stream output, WebProxy proxy)
        {
            WebRequest request = WebRequest.Create(uri);

            if (proxy != null)
            {
                request.Proxy = proxy;
            }

            request.Timeout = 5000;
            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

            try
            {
                Utility.CopyStream(stream, output, 128 * 1024 * 1024); // cap at 128mb
            }

            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream = null;
                }

                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
        }

        /// <summary>
        /// Download a file (max 128MB) and save it to the given Stream.
        /// </summary>
        public static void DownloadFile(Uri uri, Stream output)
        {
            long startPosition = output.Position;
            Exception exception = null;
            WebProxy[] proxies = Network.GetProxyList();

            foreach (WebProxy proxy in proxies)
            {
                bool success = false;

                try
                {
                    DownloadFile(uri, output, proxy);
                    exception = null;
                    success = true;
                }

                catch (Exception ex)
                {
                    exception = ex;
                }

                // If the output stream was written to, then we know
                // that we were either successful in downloading the
                // file, or there was an error unrelated to using the
                // proxy (maybe they unplugged the network cable, who
                // knows!)
                if (output.Position != startPosition || success)
                {
                    break;
                }                
            }

            if (exception != null)
            {
                WebException we = exception as WebException;

                if (we != null)
                {
                    throw new WebException(null, we, we.Status, we.Response);
                }
                else
                {
                    throw new ApplicationException("An exception occurred while trying to download '" + uri.ToString() + "'", exception);
                }
            }
        }

        private static string[] BreakIntoLines(string text)
        {
            StringReader sr = new StringReader(text);
            List<string> strings = new List<string>();
            string line;

            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] != commentChar)
                {
                    strings.Add(line);
                }
            }

            return strings.ToArray();
        }

        private static void LineToNameValue(string line, out string name, out string value)
        {
            int equalIndex = line.IndexOf('=');

            if (equalIndex == -1)
            {
                throw new FormatException("Line had no equal sign (=) present");
            }

            name = line.Substring(0, equalIndex);

            int valueLength = line.Length - equalIndex - 1;

            if (valueLength == 0)
            {
                value = string.Empty;
            }
            else
            {
                value = line.Substring(equalIndex + 1, line.Length - equalIndex - 1);
            }
        }

        private static NameValueCollection LinesToNameValues(string[] lines)
        {
            NameValueCollection nvc = new NameValueCollection();

            foreach (string line in lines)
            {
                string name;
                string value;

                LineToNameValue(line, out name, out value);
                nvc.Add(name, value);
            }

            return nvc;
        }

        private static Version[] VersionStringToArray(string versions)
        {
            string[] versionStrings = versions.Split(',');

            // For the 'null' case...
            if (versionStrings.Length == 0 ||
                (versionStrings.Length == 1 && versionStrings[0].Length == 0))
            {
                return new Version[0];
            }

            Version[] versionList = new Version[versionStrings.Length];
            
            for (int i = 0; i < versionStrings.Length; ++i)
            {
                versionList[i] = new Version(versionStrings[i]);
            }

            return versionList;
        }

        private static string[] BuildVersionValueMapping(NameValueCollection nameValues, Version[] versions, string secondaryKeyFormat)
        {
            string[] newValues = new string[versions.Length];

            for (int i = 0; i < versions.Length; ++i)
            {
                string versionString = versions[i].ToString();
                string secondaryKey = string.Format(secondaryKeyFormat, versionString);
                string secondaryValue = nameValues[secondaryKey];
                newValues[i] = secondaryValue;
            }

            return newValues;
        }

        public static PdnVersionManifest GetManifest(out Exception exception)
        {
            try
            {
                string versionsUrl = VersionManifestUrl;

                Uri versionsUri = new Uri(versionsUrl);
                byte[] manifestBuffer = DownloadSmallFile(versionsUri);
                string manifestText = System.Text.Encoding.UTF8.GetString(manifestBuffer);
                string[] manifestLines = BreakIntoLines(manifestText);
                NameValueCollection nameValues = LinesToNameValues(manifestLines);
            
                string downloadPageUrl = nameValues[downloadPageUrlName];

                string stableVersionsStrings = nameValues[stableVersionsName];
                Version[] stableVersions = VersionStringToArray(stableVersionsStrings);
                string[] stableNames = BuildVersionValueMapping(nameValues, stableVersions, nameNameFormat);
                string[] stableNetFxVersions = BuildVersionValueMapping(nameValues, stableVersions, netFxVersionNameFormat);
                string[] stableInfoUrls = BuildVersionValueMapping(nameValues, stableVersions, infoUrlNameFormat);
                string[] stableZipUrls = BuildVersionValueMapping(nameValues, stableVersions, zipUrlNameFormat);
                string[] stableZipUrlSizes = BuildVersionValueMapping(nameValues, stableVersions, zipUrlSizeNameFormat);
                string[] stableFullZipUrls = BuildVersionValueMapping(nameValues, stableVersions, fullZipUrlNameFormat);
                string[] stableFullZipUrlSizes = BuildVersionValueMapping(nameValues, stableVersions, fullZipUrlSizeNameFormat);

                string betaVersionsStrings = nameValues[betaVersionsName];
                Version[] betaVersions = VersionStringToArray(betaVersionsStrings);
                string[] betaNames = BuildVersionValueMapping(nameValues, betaVersions, nameNameFormat);
                string[] betaNetFxVersions = BuildVersionValueMapping(nameValues, betaVersions, netFxVersionNameFormat);
                string[] betaInfoUrls = BuildVersionValueMapping(nameValues, betaVersions, infoUrlNameFormat);
                string[] betaZipUrls = BuildVersionValueMapping(nameValues, betaVersions, zipUrlNameFormat);
                string[] betaZipUrlSizes = BuildVersionValueMapping(nameValues, betaVersions, zipUrlSizeNameFormat);
                string[] betaFullZipUrls = BuildVersionValueMapping(nameValues, betaVersions, fullZipUrlNameFormat);
                string[] betaFullZipUrlSizes = BuildVersionValueMapping(nameValues, betaVersions, fullZipUrlSizeNameFormat);

                PdnVersionInfo[] versionInfos = new PdnVersionInfo[betaVersions.Length + stableVersions.Length];

                int cursor = 0;
                for (int i = 0; i < stableVersions.Length; ++i)
                {
                    int size = int.Parse(stableZipUrlSizes[i]);
                    int fullSize = int.Parse(stableFullZipUrlSizes[i]);
                    PdnVersionInfo info = new PdnVersionInfo(stableVersions[i], stableNames[i], new Version(stableNetFxVersions[i]), 
                        stableInfoUrls[i], stableZipUrls[i], size, stableFullZipUrls[i], fullSize, true);
                    versionInfos[cursor] = info;
                    ++cursor;
                }

                for (int i = 0; i < betaVersions.Length; ++i)
                {
                    int size = int.Parse(betaZipUrlSizes[i]);
                    int fullSize = int.Parse(betaFullZipUrlSizes[i]);
                    PdnVersionInfo info = new PdnVersionInfo(betaVersions[i], betaNames[i], new Version(betaNetFxVersions[i]),
                        betaInfoUrls[i], betaZipUrls[i], size, betaFullZipUrls[i], fullSize, false);
                    versionInfos[cursor] = info;
                    ++cursor;
                }

                PdnVersionManifest manifest = new PdnVersionManifest(downloadPageUrl, versionInfos);
                exception = null;
                return manifest;
            }

            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }

        public static bool ShouldCheckForUpdates()
        {
            bool shouldCheckForUpdates;
            bool autoCheckForUpdates = ("1" == Settings.SystemWide.GetString(PdnSettings.AutoCheckForUpdates, "0"));

            TimeSpan minAge = new TimeSpan(MinBuildAgeForUpdateChecking, 0, 0, 0);
            TimeSpan maxAge = new TimeSpan(MaxBuildAgeForUpdateChecking, 0, 0, 0);

            TimeSpan buildAge = (DateTime.Now - PdnInfo.BuildTime);
           
            if (buildAge < minAge || buildAge > maxAge)
            {
                shouldCheckForUpdates = false;
            }
            else if (autoCheckForUpdates)
            {
                try
                {
                    string lastUpdateCheckTimeTicksString = Settings.CurrentUser.GetString(PdnSettings.LastUpdateCheckTimeTicks, null);

                    if (lastUpdateCheckTimeTicksString == null)
                    {
                        shouldCheckForUpdates = true;
                    }
                    else
                    {
                        long lastUpdateCheckTimeTicks = long.Parse(lastUpdateCheckTimeTicksString);
                        DateTime lastUpdateCheckTime = new DateTime(lastUpdateCheckTimeTicks);

                        TimeSpan timeSinceLastCheck = DateTime.Now - lastUpdateCheckTime;

                        shouldCheckForUpdates = (timeSinceLastCheck > new TimeSpan(UpdateCheckIntervalDays, 0, 0, 0));
                    }
                }

                catch
                {
                    shouldCheckForUpdates = true;
                }
            }
            else
            {
                shouldCheckForUpdates = false;
            }

            return shouldCheckForUpdates;
        }

        public static void PingLastUpdateCheckTime()
        {
            Settings.CurrentUser.SetString(PdnSettings.LastUpdateCheckTimeTicks, DateTime.Now.Ticks.ToString());
        }

        private Updates()
        {
        }
    }
}
