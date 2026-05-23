using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
    /// <summary>
    /// Carries information about the subset of Brush configuration details that we support.
    /// Does not carry color information.
    /// </summary>
    public struct BrushInfo
    {
        private BrushType brushType;
        public BrushType BrushType
        {
            get
            {
                return brushType;
            }

            set
            {
                brushType = value;
            }
        }

        /// <summary>
        /// If BrushType is equal to BrushType.Hatch, then this info is pertinent.
        /// </summary>
        private HatchStyle hatchStyle;
        public HatchStyle HatchStyle
        {
            get
            {
                return hatchStyle;
            }

            set
            {
                hatchStyle = value;
            }
        }

        public Brush CreateBrush(Color foreColor, Color backColor)
        {
            if (brushType == BrushType.Solid)
            {
                return new SolidBrush(foreColor);
            } 
            else if (brushType == BrushType.Hatch)
            {
                return new HatchBrush(hatchStyle, foreColor, backColor);
            }

            throw new InvalidOperationException("BrushType is invalid");
        }

        public BrushInfo(BrushType brushType, HatchStyle hatchStyle)
        {
            this.brushType = brushType;
            this.hatchStyle = hatchStyle;
        }
    }
}
