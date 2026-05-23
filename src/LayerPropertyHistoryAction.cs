using System;
using System.Drawing;
using System.Collections.Specialized;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerPropertyHistoryAction.
    /// </summary>
    public class LayerPropertyHistoryAction
        : HistoryAction
    {
        private object properties;
        private Layer layer;

        protected override HistoryAction OnUndo()
        {
            HistoryAction ha = new LayerPropertyHistoryAction(Name, Image, layer);
            layer.LoadProperties(properties, true);
            layer.PerformPropertyChanged();
            return ha;
        }

        public LayerPropertyHistoryAction(string name, Image image, Layer layer)
            : base(name, image)
        {
            this.layer = layer;
            this.properties = layer.SaveProperties();
        }
    }
}
