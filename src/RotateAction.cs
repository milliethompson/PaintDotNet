using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for RotateAction.
	/// </summary>
	public class RotateAction
		: DocumentAction
	{
		private RotateType rotation;
		
		public override HistoryAction PerformAction()
		{
			int newWidth, newHeight;

			//-----------Get new width and Height
			if(rotation == RotateType.Clockwise90 || rotation == RotateType.Clockwise270)
			{
				newWidth = Workspace.Document.Height;
				newHeight = Workspace.Document.Width;
			}
			else
			{
				newWidth = Workspace.Document.Width;
				newHeight = Workspace.Document.Height;
			}

			//----------Initialize the new Doc
			ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(Name, null, Workspace);
			Document newDoc = new Document(newWidth, newHeight);
			foreach (string key in Workspace.Document.UserMetaData)
			{
				newDoc.UserMetaData.Set(key, Workspace.Document.UserMetaData[key]);
			}
			newDoc.Name = Workspace.Document.Name;

			foreach(Layer l in Workspace.Document.Layers)
			{
				if(l is BitmapLayer)
				{
					Layer nl = RotateLayer((BitmapLayer)l, rotation, newWidth, newHeight);
					newDoc.Layers.Add(nl);
				}
				else
				{
					throw new InvalidOperationException("Cannot Rotate non-Bitmap Layers");
				}
			}

			Workspace.SetDocument(newDoc);
			return rdha;
		}

		private static BitmapLayer RotateLayer(BitmapLayer l, RotateType rotation, int width, int height)
		{
            using (RenderArgs srcArgs = new RenderArgs(l.Surface))
            {
                Surface s = new Surface(width, height);

                using (RenderArgs dstArgs = new RenderArgs(s))
                {

                    if(rotation == RotateType.Clockwise180)
                    {				
                        for(int x = 0; x < width; x++)
                        {
                            for(int y = 0; y < height; y++)
                            {
                                dstArgs.Surface[x,y] = srcArgs.Surface[width - x - 1, height - y - 1];
                            }
                        }
                    }
                    else if(rotation == RotateType.Clockwise270)
                    {
                        for(int x = 0; x < width; x++)
                        {
                            for(int y = 0; y < height; y++)
                            {
                                dstArgs.Surface[x,y] = srcArgs.Surface[height - y - 1, x];
                            }
                        }
                    }
                    else if(rotation == RotateType.Clockwise90)
                    {
                        for(int x = 0; x < width; x++)
                        {
                            for(int y = 0; y < height; y++)
                            {
                                dstArgs.Surface[x,y] = srcArgs.Surface[y, width - 1 - x];
                            }
                        }
                    }
                }

                BitmapLayer returnMe = new BitmapLayer(s);
                returnMe.LoadProperties(l.SaveProperties());			
                return returnMe;
            }
		}

		public RotateAction(DocumentWorkspace workspace, RotateType rotation)
			: base(workspace, "Rotate")
		{
			this.rotation = rotation;
			
		}
	}
}
