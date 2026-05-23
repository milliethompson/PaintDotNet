/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    /// <summary>
    /// Defines the possible results when scanning.
    /// </summary>
    public enum ScanResult
    {
        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// The user cancelled the operation.
        /// </summary>
        UserCancelled,

        /// <summary>
        /// The device was busy or otherwise inaccessible.
        /// </summary>
        DeviceBusy
    }
}
