/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    public sealed class PdnResources
    {
        private static readonly ResourceManager resourceManager;
        private const string ourNamespace = "PaintDotNet";
        private static readonly Assembly ourAssembly;
        private static readonly string[] localeDirs;
        private static readonly CultureInfo pdnCulture;

        private PdnResources()
        {
        }

        static PdnResources()
        {
            resourceManager = CreateResourceManager();
            ourAssembly = Assembly.GetExecutingAssembly();
            pdnCulture = CultureInfo.CurrentUICulture;
            localeDirs = GetLocaleDirs();
        }

        public static string[] GetLocaleNameChain()
        {
            ArrayList names = new ArrayList();
            CultureInfo ci = pdnCulture;

            while (ci.Name != "")
            {
                names.Add(ci.Name);
                ci = ci.Parent;
            }

            return (string[])names.ToArray(typeof(string));
        }

        private static string[] GetLocaleDirs()
        {
            const string rootDirName = "Resources";
            string appDir = PdnInfo.GetApplicationDir();
            string rootDir = Path.Combine(appDir, rootDirName);
            ArrayList dirs = new ArrayList();

            CultureInfo ci = pdnCulture;

            while (ci.Name != "")
            {
                string localeDir = Path.Combine(rootDir, ci.Name);

                if (Directory.Exists(localeDir))
                {
                    dirs.Add(localeDir);
                }

                ci = ci.Parent;
            }

            return (string[])dirs.ToArray(typeof(string));
        }

        private static ResourceManager CreateResourceManager()
        {
            const string stringsFileName = "PaintDotNet.Strings";
            ResourceManager rm = ResourceManager.CreateFileBasedResourceManager(stringsFileName, PdnInfo.GetApplicationDir(), null);
            return rm;
        }

        public static ResourceManager Strings
        {
            get
            {
                return resourceManager;
            }
        }

        public static string GetString(string stringName)
        {
            string theString = resourceManager.GetString(stringName, pdnCulture);

            if (theString == null)
            {
                Debug.WriteLine(stringName + " not found");
            }

            return theString;
        }

        public static Stream GetResourceStream(string fileName)
        {
            Stream stream = null;

            for (int i = 0; i < localeDirs.Length; ++i)
            {
                string filePath = Path.Combine(localeDirs[i], fileName);

                if (File.Exists(filePath))
                {
                    stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    break;
                }
            }

            if (stream == null)
            {
                string fullName = ourNamespace + "." + fileName;
                stream = ourAssembly.GetManifestResourceStream(fullName);
            }

            return stream;
        }

        public static Image GetImage(string fileName)
        {
            Stream stream = GetResourceStream(fileName);
            Image image = Image.FromStream(stream);
            return image;
        }

        public static Icon GetIcon(string fileName)
        {
            Stream stream = GetResourceStream(fileName);
            Icon icon = new Icon(stream);
            return icon;
        }

        public static Icon GetIconFromImage(string fileName)
        {
            Stream stream = GetResourceStream(fileName);
            Image image = Image.FromStream(stream);
            Icon icon = Icon.FromHandle(((Bitmap)image).GetHicon());
            image.Dispose();
            stream.Close();
            return icon;
        }
    }
}
