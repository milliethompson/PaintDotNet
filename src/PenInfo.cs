/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    /// <summary>
    /// Carries information about the subset of Pen configuration details that we support.
    /// Does not carry color information.
    /// </summary>
    public struct PenInfo
    {
        private DashStyle dashStyle;
        public DashStyle DashStyle
        {
            get
            {
                return dashStyle;
            }

            set
            {
                dashStyle = value;
            }
        }

        private float width;
        public float Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public Pen CreatePen(BrushInfo brushInfo, Color foreColor, Color backColor)
        {
            if (brushInfo.BrushType == BrushType.None)
            {
                return new Pen(foreColor, width);
            }
            else
            {
                return new Pen(brushInfo.CreateBrush(foreColor, backColor), width);
            }
        }

        public PenInfo(DashStyle dashStyle, float width)
        {
            this.dashStyle = dashStyle;
            this.width = width;
        }
    }
}
