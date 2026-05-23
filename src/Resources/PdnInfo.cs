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

        public enum StartupTestType
        {
            None,
            Timed,
            WorkingSet,
        }

        private static StartupTestType startupTest = StartupTestType.None;
        public static StartupTestType StartupTest
        {
            get 
            {
                return startupTest; 
            }

            set 
            {
                startupTest = value; 
            }
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

        private static readonly string appConfig = GetAppConfig();

        private static string GetAppConfig()
        {
            object[] attributes = typeof(PdnInfo).Assembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];
            return aca.Configuration;
        }

        private static readonly bool isFinalBuild = GetIsFinalBuild();

        private static bool GetIsFinalBuild()
        {
            return !(GetAppConfig().IndexOf("Final") == -1);
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

        // Pre-release builds expire after this many days. (debug+"final" also equals expiration)
        public const int BetaExpireTimeDays = 30;

        public static DateTime ExpirationDate
        {
            get
            {
                if (PdnInfo.IsFinalBuild && !IsDebugBuild)
                {
                    return DateTime.MaxValue;
                }
                else
                {
                    return PdnInfo.BuildTime + new TimeSpan(BetaExpireTimeDays, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Checks if the build is expired, and displays a dialog box that takes the user to
        /// the Paint.NET website if necessary.
        /// </summary>
        /// <returns>true if the user should be allowed to continue, false if the build has expired</returns>
        public static bool HandleExpiration()
        {
            if (!PdnInfo.IsFinalBuild || PdnInfo.IsDebugBuild)
            {
                if (DateTime.Now > PdnInfo.ExpirationDate)
                {
                    string expiredMessage = PdnResources.GetString("ExpiredDialog.Message");

                    DialogResult result = MessageBox.Show(expiredMessage, PdnInfo.GetProductName(true),
                        MessageBoxButtons.OKCancel);

                    if (result == DialogResult.OK)
                    {
                        string expiredRedirect = PdnResources.GetString("PdnInfo.ExpiredRedirectPage");
                        PdnInfo.LaunchWebSite(expiredRedirect);
                    }

                    return false;
                }
            }

            return true;
        }

        public static string GetApplicationDir()
        {
            string appPath = Application.ExecutablePath;
            string appDir = Path.GetDirectoryName(appPath);
            return appDir;
        }

        /// <summary>
        /// For final builds, returns a string such as "Paint.NET v2.6"
        /// For non-final builds, returns a string such as "Paint.NET v2.6 Beta 2"
        /// </summary>
        /// <returns></returns>
        public static string GetProductName()
        {
            return GetProductName(!IsFinalBuild);
        }

        public static string GetProductName(bool withTag)
        {
            string bareProductName = GetBareProductName();
            string productNameFormat = PdnResources.GetString("Application.ProductName.Format");
            string tag;

            if (withTag)
            {
                string tagFormat = PdnResources.GetString("Application.ProductName.Tag.Format");
                tag = string.Format(tagFormat, GetAppConfig());
            }
            else
            {
                tag = string.Empty;
            }

            string version = GetVersion().ToString(2);

            string productName = string.Format(
                productNameFormat,
                bareProductName,
                version,
                tag);

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
            string versionText = string.Format(
                versionFormat, 
                GetConfigurationString(), 
                buildType, 
                Application.ProductVersion);

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
        /// Returns the application name, with the version string. i.e., "Paint.NET v2.5 (Beta 2 Debug build 1.0.*.*)"
        /// </summary>
        /// <returns></returns>
        public static string GetFullAppName()
        {
            string fullAppNameFormat = PdnResources.GetString("PdnInfo.FullAppName.Format");
            string fullAppName = string.Format(fullAppNameFormat, PdnInfo.GetProductName(false), GetVersionString());
            return fullAppName;
        }

        /// <summary>
        /// For final builds, this returns PdnInfo.GetProductName() (i.e., "Paint.NET v2.2")
        /// For non-final builds, this returns GetFullAppName()
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            if (PdnInfo.IsFinalBuild && !PdnInfo.IsDebugBuild)
            {
                return PdnInfo.GetProductName(false);
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

        public static string GetNgenPath(bool forceX86)
        {
            string fxDir;

            if (UIntPtr.Size == 8 && !forceX86)
            {
                fxDir = "Framework64";
            }
            else
            {
                fxDir = "Framework";
            }

            string fxPathBase = @"%WINDIR%\Microsoft.NET\" + fxDir + @"\v";
            string fxPath = fxPathBase + Environment.Version.ToString(3) + @"\";
            string fxPathExp = System.Environment.ExpandEnvironmentVariables(fxPath);
            string ngenExe = Path.Combine(fxPathExp, "ngen.exe");

            return ngenExe;
        }
    }
}
