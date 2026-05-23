using System;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// A few utility functions specific to PaintDotNet.exe
    /// </summary>
    internal sealed class PdnInfo
    {
		private const string webSite = "http://www.eecs.wsu.edu/paint.net";

        private PdnInfo()
        {
        }

        public static string GetCopyrightString()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyCopyrightAttribute aca = (AssemblyCopyrightAttribute)attributes[0];            
            return aca.Copyright;
        }

        /// <summary>
        /// Returns a full version string of the form: ApplicationConfiguration + BuildType + BuildVersion
        /// i.e.: "Beta 2 Debug build 1.0.*.*"
        /// </summary>
        /// <returns></returns>
        public static string GetVersionString()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];

            return aca.Configuration + 
#if DEBUG
                " Debug" +
#else
                " Release" +
#endif
                " build " +
                Application.ProductVersion;
        }

        /// <summary>
        /// Returns the application name, with the version string. i.e., "Paint.NET (Beta 2 Debug build 1.0.*.*)"
        /// </summary>
        /// <returns></returns>
        public static string GetFullAppName()
        {
            return Application.ProductName + " (" + GetVersionString() + ")";
        }

        /// <summary>
        /// For final builds, this returns Application.ProductName (i.e., "Paint.NET")
        /// For non-final builds, this returns GetFullAppName()
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            AssemblyConfigurationAttribute aca = (AssemblyConfigurationAttribute)attributes[0];

            if (aca.Configuration.IndexOf("Final") == -1)
            {
                return GetFullAppName();
            }
            else
            {
                return Application.ProductName;
            }
        }

		public static void LaunchWebSite()
		{
			System.Diagnostics.Process.Start(webSite);
		}
    }
}
