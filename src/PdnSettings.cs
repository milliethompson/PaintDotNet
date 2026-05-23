/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Symbolic constants for our settings
    /// </summary>
    public sealed class PdnSettings
    {
        public const string Width = "Width";
        public const string Height = "Height";
        public const string Top = "Top";
        public const string Left = "Left";
        public const string MruMax = "MRUMax";
        public const string WindowState = "WindowState";
        public const string AntiAliasing = "Antialias";
        public const string Rulers = "Rulers";
        public const string DrawGrid = "DrawGrid";
        public const string TranslucentWindows = "TranslucentWindows";
        public const string HistoryLimit = "HistoryLimit";
        public const string ToolsFormVisible = "ToolsFormVisible";
        public const string ColorsFormVisible = "ColorsFormVisible";
        public const string HistoryFormVisible = "HistoryFormVisible";
        public const string LayersFormVisible = "LayersFormVisible";
        public const string LastResamplingMethod = "LastResamplingMethod";
        public const string LastFileDialogDirectory = "LastFileDialogDirectory";

        private PdnSettings()
        {
        }
    }
}
