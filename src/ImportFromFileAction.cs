/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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

            DialogResult dr = MessageBox.Show(
                this.Workspace, 
                PdnResources.GetString("ImportFromFileAction.AskForCanvasResize.Confirmation"),
                PdnInfo.GetAppName(), 
                MessageBoxButtons.YesNoCancel, 
                MessageBoxIcon.Question);

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
                            Utility.GCFullCollect();

                            newDoc = CanvasSizeAction.ResizeDocument(this.Workspace, Workspace.Document, newSize, 
                                AnchorEdge.TopLeft, Workspace.Environment.BackColor, false, false);
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this.Workspace, PdnResources.GetString("ImportFromFileAction.AskForCanvasResize.OutOfMemory"));
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
                    throw new InvalidEnumArgumentException("Internal error: DialogResult was not Yes, No, or Cancel");
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
                if (!Workspace.Environment.Selection.IsEmpty)
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
                            Utility.GCFullCollect();

                            newLayer = CanvasSizeAction.ResizeLayer((BitmapLayer)layer, Workspace.Document.Size, 
                                AnchorEdge.TopLeft, ColorBgra.White.NewAlpha(0));
                        }
                    }

                    catch (OutOfMemoryException)
                    {
                        Utility.ErrorBox(this.Workspace, PdnResources.GetString("ImportFromFileAction.ImportOneLayer.OutOfMemory"));
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
        private HistoryAction ImportDocument(Document document, out Rectangle lastLayerBounds)
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

            lastLayerBounds = Rectangle.Empty;

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
                    lastLayerBounds = layer.Bounds;
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
                lastLayerBounds = Rectangle.Empty;
                return null;
            }
        }

        private HistoryAction ImportOneFile(string fileName, out Rectangle lastLayerBounds)
        {
            FileType fileType;
            Document document = MainForm.LoadDocument(Workspace, fileName, out fileType);

            if (document != null)
            {
                string name = Path.ChangeExtension(Path.GetFileName(fileName), null);
                string newLayerNameFormat = PdnResources.GetString("ImportFromFileAction.ImportOneFile.NewLayer.Format");

                foreach (Layer layer in document.Layers)
                {
                    layer.Name = string.Format(newLayerNameFormat, name, layer.Name);
                    layer.IsBackground = false;
                }

                HistoryAction ha = ImportDocument(document, out lastLayerBounds);
                return ha;
            }
            else
            {
                lastLayerBounds = Rectangle.Empty;
                return null;
            }
        }

        public HistoryAction ImportMultipleFiles(string[] fileNames)
        {
            HistoryAction retHA = null;
            ArrayList historyActions = new ArrayList();
            Rectangle lastLayerBounds = Rectangle.Empty;

            foreach (string fileName in fileNames)
            {
                HistoryAction ha = ImportOneFile(fileName, out lastLayerBounds);

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

            if (lastLayerBounds.Width > 0 && lastLayerBounds.Height > 0)
            {
                SelectionHistoryAction sha = new SelectionHistoryAction(null, null, this.Workspace);
                historyActions.Add(sha);
                Workspace.Environment.Selection.PerformChanging();
                Workspace.Environment.Selection.Reset();
                Workspace.Environment.Selection.SetContinuation(lastLayerBounds, System.Drawing.Drawing2D.CombineMode.Replace);
                Workspace.Environment.Selection.CommitContinuation();
                Workspace.Environment.Selection.PerformChanged();
            }

            if (historyActions.Count > 0)
            {
                HistoryAction[] haArray = (HistoryAction[])historyActions.ToArray(typeof(HistoryAction));
                retHA = new CompoundHistoryAction(this.Name, StaticImage, haArray);
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
                CompoundHistoryAction cha = new CompoundHistoryAction(this.Name, StaticImage, new HistoryAction[] { retHA });
                retHA = cha;
            }

            return retHA;
        }

        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("ImportFromFileAction.Name");
            }
        }

        public static Image StaticImage
        {
            get
            {
                return PdnResources.GetImage("Icons.MenuLayersImportFromFileIcon.bmp");
            }
        }

        public ImportFromFileAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
