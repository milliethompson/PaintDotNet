/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Setup
{
    /// <summary>
    /// Summary description for PropertyNames.
    /// </summary>
    public sealed class PropertyNames
    {
        public const string JpgPngBmpEditor = "JPGPNGBMPEDITOR";
        public const string TgaEditor = "TGAEDITOR";
        public const string CheckForUpdates = "CHECKFORUPDATES";
        public const string CheckForBetas = "CHECKFORBETAS";
        public const string TargetDir = "TARGETDIR";
        public const string DesktopShortcut = "DESKTOPSHORTCUT";
        public const string PdnUpdating = "PDNUPDATING";
        public const string SkipCleanup = "SKIPCLEANUP";
        public const string Pdn25Beta4Plus = "PDN25BETA4PLUS";

        public static readonly string[] Defaults = new string[] {
                                                                    JpgPngBmpEditor, "1",
                                                                    TgaEditor, "1",
                                                                    CheckForUpdates, "1",
                                                                    CheckForBetas, "0",
                                                                    DesktopShortcut, "1"
                                                                };
        
        private PropertyNames()
        {
        }
    }
}
