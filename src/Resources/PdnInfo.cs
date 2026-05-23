/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// A few utility functions specific to PaintDotNet.exe
    /// </summary>
    public sealed class PdnInfo
    {
        private PdnInfo()
        {
        }

        private static bool isTestMode = false;
        public static bool IsTestMode
        {
            get
            {
                return isTestMode;
            }

            set
            {
                isTestMode = value;
            }
        }

        public static DateTime BuildTime
        {
            get
            {
                Version version = GetVersion();

                DateTime time = new DateTime(2000, 1, 1, 0, 0, 0);
                time = time.AddDays(version.Build);
                time = time.AddSeconds(version.Revision * 2);

                return time;
            }
        }

        private static readonly bool isFinalBuild = GetIsFinalBuild();

        private static bool GetIsFinalBuild()
        {
            object[] attributes = typeof(PdnInfo).Assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];
            return !(aca.Configuration.IndexOf("Final") == -1);
        }

        public static bool IsFinalBuild
        {
            get
            {
                return isFinalBuild;
            }
        }

        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        // Pre-release builds expire after this many days.
        public const int BetaExpireTimeDays = 30;

        public static DateTime ExpirationDate
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return DateTime.MaxValue;
                }
                else
                {
                    return PdnInfo.BuildTime + new TimeSpan(BetaExpireTimeDays, 0, 0, 0);
                }
            }
        }

        public static string GetApplicationDir()
        {
            string appPath = Application.ExecutablePath;
            string appDir = Path.GetDirectoryName(appPath);
            return appDir;
        }

        private static string productName = null;
        public static string GetProductName()
        {
            if (productName == null)
            {
                productName = PdnResources.GetString("Application.ProductName");
            }

            return productName;
        }

        public static string GetBareProductName()
        {
            return PdnResources.GetString("Application.ProductName.Bare");
        }

        private static string copyrightString = null;
        public static string GetCopyrightString()
        {
            if (copyrightString == null)
            {
                string format = InvariantStrings.CopyrightFormat;
                string allRightsReserved = PdnResources.GetString("Application.Copyright.AllRightsReserved");
                copyrightString = string.Format(CultureInfo.CurrentCulture, format, allRightsReserved);
            }

            return copyrightString;
        }

        public static Version GetVersion()
        {
            return new Version(Application.ProductVersion);
        }

        private static string GetConfigurationString()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];
            return aca.Configuration;
        }

        /// <summary>
        /// Returns a full version string of the form: ApplicationConfiguration + BuildType + BuildVersion
        /// i.e.: "Beta 2 Debug build 1.0.*.*"
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            string buildType =
#if DEBUG
                "Debug";
#else
                "Release";
#endif
                
            string versionFormat = PdnResources.GetString("PdnInfo.VersionString.Format");
            string versionText = string.Format(versionFormat, GetConfigurationString(), buildType, Application.ProductVersion);
            return versionText;
        }

        /// <summary>
        /// Returns a version string that is presentable without the Paint.NET name. example: "version 2.5 Beta 5"
        /// </summary>
        /// <returns></returns>
        public static string GetFriendlyVersionString()
        {
            Version version = PdnInfo.GetVersion();
            string versionFormat = PdnResources.GetString("PdnInfo.FriendlyVersionString.Format");
            string configFormat = PdnResources.GetString("PdnInfo.FriendlyVersionString.ConfigWithSpace.Format");
            string config = string.Format(configFormat, GetConfigurationString());
            string configText;
            
            if (PdnInfo.IsFinalBuild)
            {
                configText = string.Empty;
            }
            else
            {
                configText = config;
            }

            string versionText = string.Format(versionFormat, version.ToString(2), configText);
            return versionText;
        }

        /// <summary>
        /// Returns the application name, with the version string. i.e., "Paint.NET (Beta 2 Debug build 1.0.*.*)"
        /// </summary>
        /// <returns></returns>
        public static string GetFullAppName()
        {
            string fullAppNameFormat = PdnResources.GetString("PdnInfo.FullAppName.Format");
            string fullAppName = string.Format(fullAppNameFormat, PdnInfo.GetProductName(), GetVersionString());
            return fullAppName;
        }

        /// <summary>
        /// For final builds, this returns PdnInfo.GetProductName() (i.e., "Paint.NET v2.2")
        /// For non-final builds, this returns GetFullAppName()
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            if (PdnInfo.IsFinalBuild)
            {
                return PdnInfo.GetProductName();
            }
            else
            {
                return GetFullAppName();
            }
        }

        public static void LaunchWebSite()
        {
            LaunchWebSite(null);
        }

        public static void LaunchWebSite(string page)
        {
            string webSite = PdnResources.GetString("PdnInfo.WebSiteUrl");

            Uri baseUri = new Uri(webSite);
            Uri uri;

            if (page == null)
            {
                uri = baseUri;
            }
            else
            {
                uri = new Uri(baseUri, page);
            }

            string url = uri.ToString();

            if (url.IndexOf("@") == -1)
            {
                System.Diagnostics.Process.Start(url);
            }
        }
    }
}
