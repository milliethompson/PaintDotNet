/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    public class RenderedTileEventArgs
        : System.EventArgs
    {
        private PdnRegion renderedRegion;
        public PdnRegion RenderedRegion
        {
            get
            {
                return renderedRegion;
            }
        }

        private int tileNumber;
        public int TileNumber
        {
            get
            {
                return tileNumber;
            }
        }

        private int tileCount;
        public int TileCount
        {
            get
            {
                return tileCount;
            }
        }

        public RenderedTileEventArgs(PdnRegion renderedRegion, int tileCount, int tileNumber)
        {
            this.renderedRegion = renderedRegion;
            this.tileCount = tileCount;
            this.tileNumber = tileNumber;
        }
    }
}
