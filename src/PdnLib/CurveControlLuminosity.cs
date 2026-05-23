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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Curve control specialized for luminosity
    /// </summary>
    public class CurveControlLuminosity
        : CurveControl
    {
        public CurveControlLuminosity()
            : base(1, 256)
        {
            this.mask = new bool[1]{true};
            visualColors = new ColorBgra[]{     
                                              ColorBgra.Black
                                          };
            channelNames = new string[]{
                        PdnResources.GetString("CurveControlLuminosity.Luminosity")
            };
            ResetControlPoints();
        }

        public override ColorTransferMode ColorTransferMode
        {
            get
            {
                return ColorTransferMode.Luminosity;
            }
        }
    }
}
