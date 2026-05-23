using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerEventArgs.
    /// </summary>
    ///
    public class LayerEventArgs : EventArgs
    {
        Layer layer;

        public Layer Layer
        {
            get
            {
                return layer;
            }
        }

        public LayerEventArgs(Layer layer)
        {
            this.layer = layer;
        }
    }
}
