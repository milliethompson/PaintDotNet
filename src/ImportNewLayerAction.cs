using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.Collections;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for FlattenAction.
	/// </summary>
	public class ImportNewLayerAction
		: DocumentAction
	{
		private FileTypeCollection fileTypes = InitFileTypes();
		private bool flatImage = false;
		private string fileName = "";

		public string FileName
		{
			get
			{
				return fileName;
			}
		}

		public bool FlatImage
		{
			get
			{
				return flatImage;
			}
		}

		private static FileTypeCollection InitFileTypes()
		{
			ArrayList ft = new ArrayList();

			ft.Add(FileTypes.Bmp);
			ft.Add(FileTypes.Pdn);
			ft.Add(FileTypes.Jpeg);
			ft.Add(FileTypes.Png);
			ft.Add(FileTypes.Tiff);
			ft.Add(FileTypes.Gif);

			return new FileTypeCollection(ft);
		}

		private DialogResult ChooseFile(out string fileName)
		{
			OpenFileDialog ofd = new OpenFileDialog();

			ofd.CheckFileExists = true;
			ofd.CheckPathExists = true;
			ofd.Multiselect = false;
			ofd.RestoreDirectory = true;

			ofd.Filter = fileTypes.ToString(true, "All images");
			ofd.FilterIndex = 0;

			DialogResult result = ofd.ShowDialog(Workspace);
			fileName = ofd.FileName;

			return result;
		}

		/// <summary>
		/// Asks the user, and performs a resize oepration
		/// </summary>
		/// <param name="fitToMe">The size you want the document to fit to</param>
		/// <returns>returns yes, if yes clicked (and performs the action), does not perform the action if no and cancel are pressed</returns>
		private DialogResult AskForResize(Size fitToMe)
		{
			DialogResult dr = DialogResult.Cancel;
			
			dr = MessageBox.Show(Workspace, "The image being imported is larger than the image canvas.\nExpand canvas to fit imported image?", PdnInfo.GetAppName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
			int layerIndex = Workspace.Document.Layers.IndexOf(Workspace.ActiveLayer);

			switch (dr)
			{
				case DialogResult.Yes:
					Size newSize = new Size(Math.Max(fitToMe.Width, Workspace.Document.Width),
						Math.Max(fitToMe.Height, Workspace.Document.Height));

					Document newDoc = CanvasSizeAction.ResizeDocument(Workspace, Workspace.Document, newSize, AnchorEdge.TopLeft, Workspace.Environment.BackColor);

					if (newDoc == null)
					{
						return DialogResult.Cancel; // user clicked cancel!
					}
					else
					{
						HistoryAction rdha = new ReplaceDocumentHistoryAction("Canvas Size", null, Workspace);
						Workspace.SetDocument(newDoc);
						Workspace.History.PushNewAction(rdha);
						Workspace.ActiveLayer = (Layer)Workspace.Document.Layers[layerIndex];
					}
					return DialogResult.Yes;


				case DialogResult.No:
					return DialogResult.No;

				case DialogResult.Cancel:
					return DialogResult.Cancel;

				default:
					throw new InvalidEnumArgumentException("Internal error: DialogResult was no Yes, No, or Cancel");
			}
		}

		/// <summary>
		/// Resizes the canvas of a layer
		/// </summary>
		/// <param name="layer">The layer to be modified</param>
		/// <param name="newSize">The new size of the layer's canvas</param>
		/// <returns>A new BitmapLayer, with the new canvas size</returns>
		public BitmapLayer CanvasResizeLayer(BitmapLayer layer, Size newSize)
		{
			BitmapLayer newLayer = new BitmapLayer(newSize.Width, newSize.Height);

			new UnaryPixelOps.SetAlphaChannel(0).Apply(newLayer.Surface, newLayer.Surface.Bounds);  

			int middleX = (newSize.Width - layer.Width) / 2;
			int middleY = (newSize.Height - layer.Height) / 2;

			newLayer.Surface.CopySurface(layer.Surface, new Point(middleX, middleY));
			newLayer.LoadProperties(layer.SaveProperties());
			return newLayer;
		}


		/// <summary>
		/// Resizes a layer
		/// </summary>
		/// <param name="layer">The layer to be resized</param>
		/// <param name="newSize">The new size of the layer</param>
		/// <param name="interpMode">The interpolation mode</param>
		/// <returns>The new BitmapLayer, with the new size</returns>
		private BitmapLayer ResizeLayer(BitmapLayer layer, Size newSize, InterpolationMode interpMode)
		{
			using (RenderArgs sourceArgs = new RenderArgs(layer.Surface))
			{
				Surface surface = new Surface(newSize.Width, newSize.Height);

				using (RenderArgs destArgs = new RenderArgs(surface))
				{
					destArgs.Graphics.InterpolationMode = interpMode;
					destArgs.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
					destArgs.Graphics.DrawImage(sourceArgs.Bitmap, destArgs.Bounds, sourceArgs.Bounds, GraphicsUnit.Pixel);
					sourceArgs.Dispose();
					destArgs.Dispose();

					BitmapLayer newLayer = new BitmapLayer(surface);
					newLayer.LoadProperties(layer.SaveProperties());            
					return newLayer;
				}
			}
		}

		/// <summary>
		/// Imports a multi-layered document to the current document
		/// </summary>
		/// <param name="fileName">File to be imported</param>
		/// <param name="ft">Filetype of the file to be imported</param>
		/// <returns><b>True</b> if successull, <b>false</b> if it is not</returns>
		public HistoryAction DoImportLayersToNewLayer(string fileName)
		{			
			int ftIndex = fileTypes.IndexOfExtension(Path.GetExtension(fileName));

			if (ftIndex == -1)
			{
				Utility.ErrorBox(Workspace, "The image type is not recognized, and can not be opened.");
				return null;
			}

			FileType ft = fileTypes[ftIndex]; // Get the filetype

			// If it supports layers, populate the layer stuff
			if(ft.SupportsLayers == true)
			{
				// init dialog
				ImportAsNewLayersDialog importDialog = new ImportAsNewLayersDialog();
				importDialog.Ft = ft;
				importDialog.FileName = fileName;
				
				DialogResult importResult = DialogResult.Cancel;
				importResult = importDialog.ShowDialog();

				// If we didn't get ok, return false
				if(importResult != DialogResult.OK)
					return null;
			
				DialogResult dr = DialogResult.Yes;

				PaintDotNet.LayerList list = importDialog.LayerList;
				using(Layer tempLayer = (Layer)list[0])
				{
					tempLayer.IsBackground = false;
								
					// Can undo the amount of layers in the image
					HistoryAction[] multipleHistoryAcitons = new HistoryAction[list.Count];

					if(tempLayer.Width > Workspace.Document.Width || tempLayer.Height > Workspace.Document.Height)
						dr = AskForResize(new Size(tempLayer.Width,tempLayer.Height));

					// If the user cancels.. return
					if(dr == DialogResult.Cancel)
						return null;

					// If the user does not want to resize the canvas of the original, the layers must be resized before being applied
					if(dr == DialogResult.No)
					{
						DialogResult contToResize = Utility.AskYesNo(Workspace,"To continue, each layer must be resized to fit your current image");
						if(contToResize == DialogResult.No)
							return null;
					}
				

					int n = 0;
					// Go through the ListLayer
					foreach(Layer layer in list)
					{
						// Grab the latest layer
						//layer = (Layer)list[i];
						BitmapLayer nl;

						// If the user wants the layer imported, and it was marked visible
						if(layer.Visible == true)
						{
							switch(dr)
							{
								case DialogResult.No:
									ScaleFactor scaleFactor;
									System.Drawing.Size fitToMe = Workspace.Document.Size;

									scaleFactor = new ScaleFactor(Math.Min((float)fitToMe.Width / layer.Width, (float)fitToMe.Height / layer.Height));
									System.Drawing.Size thisSize = scaleFactor.ScaleSize(layer.Size);
									
									// Need to resize the layer, then expand its canvas to fit the Workspace document
									InterpolationMode im = InterpolationMode.HighQualityBicubic;
									nl = ResizeLayer((BitmapLayer)layer,thisSize,im);									

									nl = CanvasResizeLayer((BitmapLayer)nl,Workspace.Document.Size);
									try
									{
										Workspace.Document.Layers.Add(nl);								
									}

									catch (OutOfMemoryException)
									{
										Utility.ErrorBox(Workspace, "Not enough memory to create a new layer.");
										return null;
									}

									multipleHistoryAcitons[n++] = new NewLayerHistoryAction("New Layer",null,Workspace,nl);
									break;
								case DialogResult.Yes:
									nl = CanvasResizeLayer((BitmapLayer)layer,Workspace.Document.Size);
									try
									{
										Workspace.Document.Layers.Add(nl);								
									}

									catch (OutOfMemoryException)
									{
										Utility.ErrorBox(Workspace, "Not enough memory to create a new layer.");
										return null;
									}			
									multipleHistoryAcitons[n++] = new NewLayerHistoryAction("New Layer",null,Workspace,nl);
									break;
							}
						}

						//++i;
					} // foreach(Layer layer in list)
					// If we have any history actions in the list
					if(n != 0)
					{
						CompoundHistoryAction cha = new CompoundHistoryAction("Import From File",Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp"),multipleHistoryAcitons);
						return (HistoryAction)cha;
						//Workspace.History.PushNewAction(cha);
					}
				}// using(Layer tempLayer = (Layer)list[0])
			} // if(ft.SupportsLayers == true)
			else
			{
				// Else, its a flat image
				flatImage = true;
				this.fileName = fileName;
			}

			return null;
		}
		
		public override HistoryAction PerformAction()
		{
			HistoryAction ha;

			flatImage = false;
			string fileName;
			DialogResult result = ChooseFile(out fileName);
			
			if(result != DialogResult.OK)
				return null;

			if (fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}
			ha = DoImportLayersToNewLayer(fileName);

			return ha;
		}

		public ImportNewLayerAction(DocumentWorkspace workspace)
			: base(workspace, "Import From File")
		{
		}
	}
}
