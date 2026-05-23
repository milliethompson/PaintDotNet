using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FlipLayerHistoryAction.
    /// </summary>
    public class FlipLayerHistoryAction
        : HistoryAction
    {
        private Layer layer;
        private FlipType flipType;

        public void Flip(Surface surface, FlipType flipType)
        {
            switch (flipType)
            {
                case FlipType.Horizontal:
                    for (int y = 0; y < surface.Height; ++y)
                    {
                        for (int x = 0; x < surface.Width / 2; ++x)
                        {
                            ColorBgra temp = surface[x, y];
                            surface[x, y] = surface[surface.Width - x - 1, y];
                            surface[surface.Width - x - 1, y] = temp;
                        }
                    }

                    break;

                case FlipType.Vertical:
                    for (int x = 0; x < surface.Width; ++x)
                    {
                        for (int y = 0; y < surface.Height / 2; ++y)
                        {
                            ColorBgra temp = surface[x, y];
                            surface[x, y] = surface[x, surface.Height - y - 1];
                            surface[x, surface.Height - y - 1] = temp;
                        }
                    }

                    break;

                default:
                    throw new InvalidOperationException("FlipType was invalid");
            }

            return;
        }

        protected override HistoryAction OnUndo()
        {
            FlipLayerHistoryAction fha = new FlipLayerHistoryAction(this.Name, this.Image, layer, flipType);
            Flip(((BitmapLayer)layer).Surface, this.flipType);
            layer.Invalidate();
            return fha;
        }

        public FlipLayerHistoryAction(string name, Image image, Layer flipMe, FlipType flipType)
            : base(name, image)
        {
            this.layer = flipMe;
            this.flipType = flipType;
        }
    }
}
