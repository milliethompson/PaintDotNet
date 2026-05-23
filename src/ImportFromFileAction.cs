/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ImportFromFileAction.
	/// </summary>
	public class ImportFromFileAction
        : DocumentAction
	{
        private void Rollback(ArrayList historyActions)
        {
            for (int i = historyActions.Count - 1; i >= 0; i--)
            {
                HistoryAction ha = (HistoryAction)historyActions[i];
                ha.PerformUndo();
            }
        }

        private HistoryAction AskForCanvasResize(Size newLayerSize)
        {
            HistoryAction retHA;

            DialogResult dr = MessageBox.Show(this.Workspace, "The image being imported is larger than the image canvas.\nExpand canvas to fit imported image?", 
                PdnInfo.GetAppName(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            int layerIndex = Workspace.Document.Layers.IndexOf(Workspace.ActiveLayer);

            switch (dr)
            {
                case DialogResult.Yes:
                    Size newSize = new Size(Math.Max(newLayerSize.Width, Workspace.Document.Width),
                        Math.Max(newLayerSize.Height, Workspace.Document.Height));

                    Document newDoc;
                    
                    try
                    {
                        using (new WaitCursorChanger(this.Workspace))
                        {
                            newDoc = CanvasSizeAction.ResizeDocument(this.Workspace, Workspace.Document, newSize, 
                                AnchorEdge.TopLeft, Workspace.Environment.BackColor);
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.GCFullCollect();
                        Utility.ErrorBox(this.Workspace, "Ran out of memory while trying to resize the image");
                        newDoc = null;
                    }

                    if (newDoc == null)
                    {
                        retHA = null;
                    }
                    else
                    {
                        retHA = new ReplaceDocumentHistoryAction(string.Empty, null, Workspace);

                        using (new WaitCursorChanger(this.Workspace))
                        {
                            Workspace.SetDocument(newDoc);
                        }

                        Workspace.ActiveLayer = (Layer)Workspace.Document.Layers[layerIndex];
                    }

                    break;

                case DialogResult.No:
                    retHA = new CompoundHistoryAction(string.Empty, null, new HistoryAction[0]);
                    break;

                case DialogResult.Cancel:
                    retHA = null;
                    break;

                default:
                    throw new InvalidEnumArgumentException("Internal error: DialogResult was no Yes, No, or Cancel");
            }

            return retHA;
        }

        private HistoryAction ImportOneLayer(BitmapLayer layer)
        {
            HistoryAction retHA;
            ArrayList historyActions = new ArrayList();
            bool success = true;
            
            if (success)
            {
                if (!Workspace.Environment.IsSelectionEmpty)
                {
                    HistoryAction ha = new DeselectAction(this.Workspace).PerformAction();
                    historyActions.Add(ha);
                }
            }

            if (success)
            {
                if (layer.Width > Workspace.Document.Width ||
                    layer.Height > Workspace.Document.Height)
                {
                    HistoryAction ha = AskForCanvasResize(layer.Size);
                
                    if (ha == null)
                    {
                        success = false;
                    }
                    else
                    {
                        historyActions.Add(ha);
                    }
                }
            }

            if (success)
            {
                if (layer.Size != Workspace.Document.Size)
                {
                    BitmapLayer newLayer;
                    
                    try
                    {
                        using (new WaitCursorChanger(this.Workspace))
                        {
                            newLayer = CanvasSizeAction.ResizeLayer((BitmapLayer)layer, Workspace.Document.Size, 
                                AnchorEdge.TopLeft, ColorBgra.White.NewAlpha(0));
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.GCFullCollect();
                        Utility.ErrorBox(this.Workspace, "Ran out of memory while trying to resize the layer");
                        success = false;
                        newLayer = null;
                    }

                    if (newLayer != null)
                    {
                        layer.Dispose();
                        layer = newLayer;
                    }
                }
            }

            if (success)
            {
                NewLayerHistoryAction nlha = new NewLayerHistoryAction(string.Empty, null, this.Workspace, Workspace.Document.Layers.Count);
                Workspace.Document.Layers.Add(layer);
                historyActions.Add(nlha);
            }

            if (success)
            {
                HistoryAction[] has = (HistoryAction[])historyActions.ToArray(typeof(HistoryAction));
                retHA = new CompoundHistoryAction(string.Empty, null, has);
            }
            else
            {
                Rollback(historyActions);
                retHA = null;
            }

            return retHA;
        }

        /// <summary>
        /// Presents a user interface and performs the operations required for importing an entire document.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        /// <remarks>
        /// This function will take ownership of the Document given to it, and will Dispose() of it.
        /// </remarks>
        private HistoryAction ImportDocument(Document document)
        {
            ArrayList historyActions = new ArrayList();
            bool[] selected;

            if (document.Layers.Count == 1)
            {
                selected = new bool[] { true };
            }
            else
            {
                using (ImportLayersDialog ild = new ImportLayersDialog())
                {
                    ild.RenderSurface = Workspace.ScratchSurface;
                    ild.Document = document;
                    DialogResult result = Utility.ShowDialog(ild, Workspace);

                    if (result != DialogResult.Cancel)
                    {
                        selected = ild.SelectedLayers;
                    }
                    else
                    {
                        selected = null;
                    }
                }
            }

            if (selected != null)
            {
                ArrayList layers = new ArrayList();

                for (int i = 0; i < selected.Length; ++i)
                {
                    if (selected[i])
                    {
                        layers.Add(document.Layers[i]);
                    }
                }

                foreach (Layer layer in layers)
                {
                    document.Layers.Remove(layer);
                }

                document.Dispose();
                document = null;

                foreach (Layer layer in layers)
                {
                    HistoryAction ha = ImportOneLayer((BitmapLayer)layer);

                    if (ha != null)
                    {
                        historyActions.Add(ha);
                    }
                    else
                    {
                        Rollback(historyActions);
                        historyActions.Clear();
                        break;
                    }
                }
            }

            if (document != null)
            {
                document.Dispose();
                document = null;
            }

            if (historyActions.Count > 0)
            {
                HistoryAction[] has = (HistoryAction[])historyActions.ToArray(typeof(HistoryAction));
                return new CompoundHistoryAction(string.Empty, null, has);
            }
            else
            {
                return null;
            }
        }

        private HistoryAction ImportOneFile(string fileName)
        {
            FileType fileType;
            Document document = MainForm.LoadDocument(Workspace, fileName, out fileType);

            if (document != null)
            {
                string name = Path.ChangeExtension(Path.GetFileName(fileName), null);

                foreach (Layer layer in document.Layers)
                {
                    layer.Name = name + ": " + layer.Name;
                    layer.IsBackground = false;
                }

                HistoryAction ha = ImportDocument(document);
                return ha;
            }
            else
            {
                return null;
            }
        }

        public HistoryAction ImportMultipleFiles(string[] fileNames)
        {
            HistoryAction retHA = null;
            ArrayList historyActions = new ArrayList();

            foreach (string fileName in fileNames)
            {
                HistoryAction ha = ImportOneFile(fileName);

                if (ha != null)
                {
                    historyActions.Add(ha);
                }
                else
                {
                    Rollback(historyActions);
                    historyActions.Clear();
                    break;
                }
            }

            if (historyActions.Count > 0)
            {
                HistoryAction[] haArray = (HistoryAction[])historyActions.ToArray(typeof(HistoryAction));
                retHA = new CompoundHistoryAction(this.Name, Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp"), haArray);
            }

            return retHA;
        }

        public override HistoryAction PerformAction()
        {
            string[] fileNames;
            string startingDir = Path.GetDirectoryName(Workspace.DocumentFileName);
            DialogResult result = MainForm.ChooseFiles(this.Workspace, out fileNames, true, startingDir);
            HistoryAction retHA = null;

            if (result == DialogResult.OK)
            {
                retHA = ImportMultipleFiles(fileNames);
            }

            if (retHA != null)
            {
                CompoundHistoryAction cha = new CompoundHistoryAction(this.Name, Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp"), new HistoryAction[] { retHA });
                retHA = cha;
            }

            return retHA;
        }

		public ImportFromFileAction(DocumentWorkspace workspace)
            : base(workspace, "Import From File")
		{
		}
	}
}
