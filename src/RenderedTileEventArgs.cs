using System;
using System.Drawing;

namespace PaintDotNet
{
    public class RenderedTileEventArgs
        : System.EventArgs
    {
        private Region renderedRegion;
        public Region RenderedRegion
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

        public RenderedTileEventArgs(Region renderedRegion, int tileCount, int tileNumber)
        {
            this.renderedRegion = renderedRegion;
            this.tileCount = tileCount;
            this.tileNumber = tileNumber;
        }
    }
}
