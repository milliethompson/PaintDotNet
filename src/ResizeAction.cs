using System;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;


namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ResizeAction.
	/// </summary>
	public class ResizeAction 
		: DocumentAction
	{
	    private static BitmapLayer ResizeLayer(BitmapLayer l, int width, int height, InterpolationMode interpMode)
		{
			RenderArgs sourceArgs = new RenderArgs(l.Surface);
			Surface s = new Surface(width, height);
            RenderArgs destArgs = new RenderArgs(s);

			destArgs.Graphics.InterpolationMode = interpMode;
            destArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            destArgs.Graphics.DrawImage(sourceArgs.Bitmap, destArgs.Bounds, sourceArgs.Bounds, GraphicsUnit.Pixel);
			sourceArgs.Dispose();
			destArgs.Dispose();

			BitmapLayer newLayer = new BitmapLayer(s);
			newLayer.LoadProperties(l.SaveProperties());			
			return newLayer;
		}

		public override HistoryAction PerformAction()
		{
			int newWidth = -1;
			int newHeight = -1;
			InterpolationMode im = InterpolationMode.HighQualityBicubic;
	
			ResizeDialog rd = new ResizeDialog();
			rd.AspectRatio = (double)Workspace.Document.Width / (double)Workspace.Document.Height;
            rd.OriginalSize = Workspace.Document.Size;
			rd.ImageHeight =  Workspace.Document.Height;
			rd.ImageWidth =   Workspace.Document.Width;
			rd.DocumentSize = rd.ImageHeight * rd.ImageWidth * Workspace.Document.Layers.Count * System.Runtime.InteropServices.Marshal.SizeOf(typeof(ColorBgra));
			rd.Layers = Workspace.Document.Layers.Count;
			
			DialogResult result = rd.ShowDialog(Workspace.FindForm());

			if (result == DialogResult.Cancel)
			{
				return null;
			}

            // if the new size equals the old size, there's really no point in doing anything
            if (Workspace.Document.Size == new Size(rd.ImageWidth, rd.ImageHeight))
            {
                return null;
            }

			newWidth = rd.ImageWidth;
			newHeight = rd.ImageHeight;
			im = rd.InterpMode;

            using (new WaitCursorChanger(Workspace))
            {
                try
                {
                    ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(Name, null, Workspace);
                    Document nd = new Document(newWidth, newHeight);

                    foreach (string key in Workspace.Document.UserMetaData)
                    {
                        nd.UserMetaData.Set(key, Workspace.Document.UserMetaData[key]);
                    }

                    nd.Name = Workspace.Document.Name;

                    foreach(Layer l in Workspace.Document.Layers)
                    {
                        if(l is BitmapLayer)
                        {
                            Layer nl = ResizeLayer((BitmapLayer)l, newWidth, newHeight, im);
                            nd.Layers.Add(nl);
                        }
                        else
                        {
                            throw new InvalidOperationException("Resize does not support Layers that are not BitmapLayers");
                        }
                    }

                    Workspace.SetDocument(nd);
                    return rdha;
                }

                catch (OutOfMemoryException)
                {
                    Utility.ErrorBox(Workspace, "Not enough memory to resize the image.");
                    return null;
                }
            }
		}

		public ResizeAction(DocumentWorkspace workspace) 
            : base(workspace, "Resize")
		{
			// Nothing to Construct			
		}
	}
}
