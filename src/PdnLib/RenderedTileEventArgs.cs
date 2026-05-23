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
