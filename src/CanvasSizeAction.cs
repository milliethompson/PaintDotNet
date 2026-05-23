using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// There are two ways to use this action:
	/// 1. Through the normal "PerformAction" interface provided through DoucmentAction
	/// 2. Through the ResizeCanvas static method
	/// </summary>
	public class CanvasSizeAction
        : DocumentAction
	{
        public static BitmapLayer ResizeLayer(BitmapLayer layer, Size newSize, AnchorEdge anchor, ColorBgra background)
        {
            BitmapLayer newLayer = new BitmapLayer(newSize.Width, newSize.Height);

            // Background = 0xffffffff
            new UnaryPixelOps.Constant(background).Apply(newLayer.Surface, newLayer.Surface.Bounds);

            // non-background = 0x00ffffff (see-through)
            if (!layer.IsBackground)
            {
                new UnaryPixelOps.SetAlphaChannel(0).Apply(newLayer.Surface, newLayer.Surface.Bounds);
            }                

            int topY = 0;
            int leftX = 0;
            int rightX = newSize.Width - layer.Width;
            int bottomY = newSize.Height - layer.Height;
            int middleX = (newSize.Width - layer.Width) / 2;
            int middleY = (newSize.Height - layer.Height) / 2;

            int x = 0;
            int y = 0;

            #region choose x,y from AnchorEdge
            switch (anchor)
            {
                case AnchorEdge.TopLeft:
                    x = leftX;
                    y = topY;
                    break;

                case AnchorEdge.Top:
                    x = middleX;
                    y = topY;
                    break;

                case AnchorEdge.TopRight:
                    x = rightX;
                    y = topY;
                    break;

                case AnchorEdge.Left:
                    x = leftX;
                    y = middleY;
                    break;

                case AnchorEdge.Middle:
                    x = middleX;
                    y = middleY;
                    break;

                case AnchorEdge.Right:
                    x = rightX;
                    y = middleY;
                    break;

                case AnchorEdge.BottomLeft:
                    x = leftX;
                    y = bottomY;
                    break;

                case AnchorEdge.Bottom:
                    x = middleX;
                    y = bottomY;
                    break;

                case AnchorEdge.BottomRight:
                    x = rightX;
                    y = bottomY;
                    break;
            }
            #endregion

            newLayer.Surface.CopySurface(layer.Surface, new Point(x, y));
            newLayer.LoadProperties(layer.SaveProperties());
            return newLayer;
        }

        public static Document ResizeDocument(Document document, Size newSize, AnchorEdge edge, ColorBgra background)
        {
            Document nd = new Document(newSize.Width, newSize.Height);

            foreach (string key in document.UserMetaData)
            {
                nd.UserMetaData.Set(key, document.UserMetaData[key]);
            }

            nd.Name = document.Name;

            foreach (Layer l in document.Layers)
            {
                if (l is BitmapLayer)
                {
                    Layer nl = ResizeLayer((BitmapLayer)l, newSize, edge, background);
                    nd.Layers.Add(nl);
                }
                else
                {
                    throw new InvalidOperationException("Canvas Size does not support Layers that are not BitmapLayers");
                }
            }        

            return nd;
        }

        // returns null to indicate user cancelled, or if initialNewSize = newSize that the user requested, 
        // or if there was an error (out of memory)
        public static Document ResizeDocument(IWin32Window parent, 
                                              Document document, 
                                              Size initialNewSize, 
                                              AnchorEdge initialAnchor, 
                                              ColorBgra background)
        {
            CanvasSizeDialog csd = new CanvasSizeDialog();

            csd.AspectRatio = (double)document.Width / (double)document.Height;
            csd.IsLocked = false;
            csd.OriginalSize = document.Size;
            csd.ImageWidth = initialNewSize.Width;
            csd.ImageHeight = initialNewSize.Height;
            csd.DocumentSize = csd.ImageHeight * csd.ImageWidth * document.Layers.Count * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ColorBgra));
            csd.Layers = document.Layers.Count;
            csd.AnchorEdge = initialAnchor;

            DialogResult result = csd.ShowDialog(parent);
            Size newSize = new Size(csd.ImageWidth, csd.ImageHeight);

            if (result == DialogResult.Cancel ||
                newSize == document.Size)
            {
                return null;
            }

            try
            {
                return ResizeDocument(document, newSize, csd.AnchorEdge, background);
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(parent, "Not enough memory to resize the canvas.");
                return null;
            }
        }

        public override HistoryAction PerformAction()
        {
            Document newDoc = ResizeDocument(Workspace.FindForm(), 
                Workspace.Document, Workspace.Document.Size, AnchorEdge.Middle, Workspace.Environment.BackColor);

            if (newDoc != null)
            {
                ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(Name, null, Workspace);
                Workspace.SetDocument(newDoc);
                return rdha;
            }
            else
            {
                return null;
            }
        }

		public CanvasSizeAction(DocumentWorkspace workspace)
            : base(workspace, "Canvas Size")
		{
		}
	}
}
