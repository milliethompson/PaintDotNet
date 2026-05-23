/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Contains information pertaining to a release of Paint.NET
    /// </summary>
    public class PdnVersionInfo
    {
        private Version version;
        private string friendlyName;
        private Version netFxVersion;
        private string infoUrl;
        private string downloadUrl;
        private int downloadSize;
        private string fullDownloadUrl;
        private int fullDownloadSize;
        private bool isFinal;

        public Version Version
        {
            get
            {
                return this.version;
            }
        }

        public string FriendlyName
        {
            get
            {
                return this.friendlyName;
            }
        }

        public Version NetFxVersion
        {
            get
            {
                return this.netFxVersion;
            }
        }

        public string InfoUrl
        {
            get
            {
                return this.infoUrl;
            }
        }
        
        public string DownloadUrl
        {
            get
            {
                return this.downloadUrl;
            }
        }

        public int DownloadSize
        {
            get
            {
                return this.downloadSize;
            }
        }

        public string FullDownloadUrl
        {
            get
            {
                return this.fullDownloadUrl;
            }
        }

        public int FullDownloadSize
        {
            get
            {
                return this.fullDownloadSize;
            }
        }

        public bool IsFinal
        {
            get
            {
                return this.isFinal;
            }
        }

        public PdnVersionInfo(Version version, string friendlyName, Version netFxVersion, string infoUrl, 
            string downloadUrl, int downloadSize, string fullDownloadUrl, int fullDownloadSize, bool isFinal)
        {
            this.version = version;
            this.friendlyName = friendlyName;
            this.netFxVersion = netFxVersion;
            this.infoUrl = infoUrl;
            this.downloadUrl = downloadUrl;
            this.downloadSize = downloadSize;
            this.fullDownloadUrl = fullDownloadUrl;
            this.fullDownloadSize = fullDownloadSize;
            this.isFinal = isFinal;
        }
    }
}
