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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Curve control specialization for RGB curves
    /// </summary>
    public class CurveControlRgb
        : CurveControl
    {
        public CurveControlRgb()
            : base(3, 256)
        {
            this.mask = new bool[3] { true, true, true };
            visualColors = new ColorBgra[] {     
                                               ColorBgra.Red,
                                               ColorBgra.Green,
                                               ColorBgra.Blue
                                           };
            channelNames = new string[]{
                PdnResources.GetString("CurveControlRgb.Red"),
                PdnResources.GetString("CurveControlRgb.Green"),
                PdnResources.GetString("CurveControlRgb.Blue")
            };
            ResetControlPoints();
        }

        public override ColorTransferMode ColorTransferMode
        {
            get
            {
                return ColorTransferMode.Rgb;
            }
        }
    }
}
