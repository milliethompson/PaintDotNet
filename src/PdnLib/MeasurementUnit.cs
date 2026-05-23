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
    /// Specifies the unit of measure for the given data.
    /// </summary>
    /// <remarks>
    /// These enumeration values correspond to the values used in the EXIF ResolutionUnit tag.
    /// </remarks>
    public enum MeasurementUnit
        : int
    {
        Pixel = 1,
        Inch = 2,
        Centimeter = 3
    }
}
