using System;
using System.ComponentModel;
using System.Drawing;

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

            // Get new width and Height
            switch (rotation)
            {
                case RotateType.Clockwise90:
                case RotateType.Clockwise270:
                case RotateType.CounterClockwise90:
                case RotateType.CounterClockwise270:
                    newWidth = Workspace.Document.Height;
                    newHeight = Workspace.Document.Width;
                    break;

                case RotateType.Clockwise180:
                case RotateType.CounterClockwise180:
                case RotateType.NoRotation:
                    newWidth = Workspace.Document.Width;
                    newHeight = Workspace.Document.Height;
                    break;

                default:
                    throw new InvalidEnumArgumentException("invalid RotateType");
            }

            // Figure out which icon and text to use
            Image icon;
            string suffix;

            switch (rotation)
            {
                case RotateType.Clockwise180:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate180CWIcon.bmp");
                    suffix = "180° CW";
                    break;

                case RotateType.Clockwise270:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate270CWIcon.bmp");
                    suffix = "270° CW";
                    break;

                case RotateType.Clockwise90:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate90CWIcon.bmp");
                    suffix = "90° CW";
                    break;

                case RotateType.CounterClockwise180:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate180CCWIcon.bmp");
                    suffix = "180° CCW";
                    break;

                case RotateType.CounterClockwise270:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate270CCWIcon.bmp");
                    suffix = "270° CCW";
                    break;

                case RotateType.CounterClockwise90:
                    icon = Utility.GetImageResource("Icons.MenuImageRotate90CCWIcon.bmp");
                    suffix = "90° CCW";
                    break;

                case RotateType.NoRotation:
                    icon = null;
                    suffix = string.Empty;
                    break;

                default:
                    throw new InvalidEnumArgumentException("invalid RotateType");
            }

            // Initialize the new Doc
            ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(Name + " " + suffix, icon, Workspace);
            Document newDoc = new Document(newWidth, newHeight);

            foreach (string key in Workspace.Document.UserMetaData)
            {
                newDoc.UserMetaData.Set(key, Workspace.Document.UserMetaData[key]);
            }

            newDoc.Name = Workspace.Document.Name;

            foreach (Layer layer in Workspace.Document.Layers)
            {
                if (layer is BitmapLayer)
                {
                    Layer nl = RotateLayer((BitmapLayer)layer, rotation, newWidth, newHeight);
                    newDoc.Layers.Add(nl);
                }
                else
                {
                    throw new InvalidOperationException("Cannot Rotate non-BitmapLayers");
                }
            }

            Workspace.SetDocument(newDoc);
            return rdha;
        }

        private static BitmapLayer RotateLayer(BitmapLayer layer, RotateType rotation, int width, int height)
        {
            Surface surface = new Surface(width, height);

            if (rotation == RotateType.Clockwise180 ||
                rotation == RotateType.CounterClockwise180)
            {               
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        surface[x,y] = layer.Surface[width - x - 1, height - y - 1];
                    }
                }
            }
            else if (rotation == RotateType.Clockwise270 ||
                     rotation == RotateType.CounterClockwise90)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        surface[x,y] = layer.Surface[height - y - 1, x];
                    }
                }
            }
            else if (rotation == RotateType.Clockwise90 ||
                     rotation == RotateType.CounterClockwise270)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        surface[x,y] = layer.Surface[y, width - 1 - x];
                    }
                }
            }

            BitmapLayer returnMe = new BitmapLayer(surface);
            returnMe.LoadProperties(layer.SaveProperties());            
            return returnMe;
        }

        public RotateAction(DocumentWorkspace workspace, RotateType rotation)
            : base(workspace, "Rotate")
        {
            this.rotation = rotation;           
        }
    }
}
