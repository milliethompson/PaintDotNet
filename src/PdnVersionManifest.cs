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

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for PdnVersionManifest.
    /// </summary>
    public class PdnVersionManifest
    {
        private string downloadPageUrl;
        private PdnVersionInfo[] versionInfos;

        public string DownloadPageUrl
        {
            get
            {
                return this.downloadPageUrl;
            }
        }

        public PdnVersionInfo[] VersionInfos
        {
            get
            {
                return (PdnVersionInfo[])this.versionInfos;
            }
        }

        private class PdnVersionInfoComparer
            : IComparer
        {
            public int Compare(object x, object y)
            {
                PdnVersionInfo xpvi = (PdnVersionInfo)x;
                PdnVersionInfo ypvi = (PdnVersionInfo)y;
               
                if (xpvi.Version < ypvi.Version)
                {
                    return -1;
                }
                else if (xpvi.Version == ypvi.Version)
                {
                    return 0;
                }
                else // if (xpvi.Version > ypvi.Version)
                {
                    return +1;
                }
            }
        }

        public int GetLatestBetaVersionIndex()
        {
            PdnVersionInfo[] versions = VersionInfos;
            Array.Sort(versions, new PdnVersionInfoComparer());

            for (int i = versions.Length - 1; i >= 0; --i)
            {
                if (!versions[i].IsFinal)
                {
                    return i;
                }
            }

            return -1;
        }

        public int GetLatestStableVersionIndex()
        {
            PdnVersionInfo[] versions = VersionInfos;
            Array.Sort(versions, new PdnVersionInfoComparer());

            for (int i = versions.Length - 1; i >= 0; --i)
            {
                if (versions[i].IsFinal)
                {
                    return i;
                }
            }

            return -1;
        }

        public PdnVersionManifest(string downloadPageUrl, PdnVersionInfo[] versionInfos)
        {
            this.downloadPageUrl = downloadPageUrl;
            this.versionInfos = (PdnVersionInfo[])versionInfos.Clone();
        }
    }
}
