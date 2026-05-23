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
        public const string QueueNgen = "QUEUENGEN";
        public const string TgaEditor = "TGAEDITOR";
        public const string CheckForUpdates = "CHECKFORUPDATES";
        public const string CheckForBetas = "CHECKFORBETAS";
        public const string TargetDir = "TARGETDIR";
        public const string DesktopShortcut = "DESKTOPSHORTCUT";
        public const string PdnUpdating = "PDNUPDATING";
        public const string SkipCleanup = "SKIPCLEANUP";
        public const string ProgramsGroup = "PROGRAMSGROUP";
        public const string UsingWizard = "USINGWIZARD";

        public static string CheckForBetasDefault
        {
            get
            {
                if (PdnInfo.IsFinalBuild)
                {
                    return "0";
                }
                else
                {
                    return "1";
                }
            }
        }

        public static readonly string[] Defaults = new string[] 
        {
            JpgPngBmpEditor, "1",
            TgaEditor, "1",
            CheckForUpdates, "1",
            CheckForBetas, CheckForBetasDefault,
            DesktopShortcut, "1",
            ProgramsGroup, "",
            QueueNgen, "1",
            UsingWizard, "1"
        };

        public static readonly string[] AdGpoDefaults = new string[] 
        {
            JpgPngBmpEditor, "1",
            TgaEditor, "1",
            CheckForUpdates, "0",
            CheckForBetas, "0",
            DesktopShortcut, "1",
            ProgramsGroup, "",
            QueueNgen, "0",
            SkipCleanup, "0",
            UsingWizard, "1"
        };
        
        private PropertyNames()
        {
        }
    }
}
