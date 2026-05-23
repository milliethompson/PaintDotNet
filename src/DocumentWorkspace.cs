/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using DotNetWidgets;
using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for DocumentWorkspace.
    /// </summary>
    public class DocumentWorkspace
        : UserControl
    {
        private sealed class OurDocumentView
            : DocumentView
        {
            protected override bool QueryNewZoomCenterPoint(ref Point newCenterPt)
            {
                DocumentWorkspace workspace = this.Parent as DocumentWorkspace;
                DocumentEnvironment env = workspace.Environment;

                if (!env.Selection.IsEmpty) 
                {
                    using (PdnRegion selectedRegion = env.Selection.CreateRegion())
                    {
                        Rectangle selectionBounds = selectedRegion.GetBoundsInt();

                        Point selectionCenter = new Point((selectionBounds.Left + selectionBounds.Right) / 2,
                            (selectionBounds.Top + selectionBounds.Bottom) / 2);

                        newCenterPt = selectionCenter;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private Document document = null;
        private string documentFileName = null;
        private FileType documentFileType = null;
        private SaveConfigToken documentSaveToken = null;

        private Surface scratch = null;
        private HistoryStack history;
        private Layer activeLayer;
        private DocumentEnvironment environment;

        private ToolInfo[] toolInfos;
        private Type[] tools;
        private Type[] effects = null;
        private PenConfigWidget penConfigWidget;
        private ZoomConfigWidget zoomConfigWidget;
        private BrushConfigWidget brushConfigWidget;
        private ShapeDrawTypeConfigWidget shapeDrawTypeConfigWidget;
        private DrawModesConfigWidget drawModesConfigWidget;
        private OurDocumentView documentView;
        private SelectionRenderer selectionRenderer;

        private EventHandler zoomChangedDelegate;

        private EventHandler selectedPathChangingDelegate;
        private EventHandler selectedPathChangedDelegate;

        private EventHandler foreColorChangedDelegate;
        private EventHandler backColorChangedDelegate;
        private EventHandler shapeDrawTypeChangedDelegate;
        private EventHandler alphaBlendingChangedDelegate;

        private IndexEventHandler layerRemovingDelegate;
        private IndexEventHandler layerRemovedDelegate;
        private IndexEventHandler layerInsertedDelegate;
        private PropertyEventHandler layerPropertyChangingDelegate;
        private PropertyEventHandler layerPropertyChangedDelegate;

        private FlowPanel topDock;

        private DocumentWidgets widgets;

        private MainToolBarForm mainToolBarForm;
        private LayerForm layerForm;
        private HistoryForm historyForm;
        private PaintDotNet.TextConfigWidget textConfigWidget;
        private PaintDotNet.WorkspaceOptionsConfigWidget workspaceOptionsConfigWidget;
        private System.Windows.Forms.Timer toolPulseTimer;
        private PaintDotNet.CommonActionsWidget commonActionsWidget;
        private ColorsForm colorsForm;

        // DocumentChanging
        // This event is raised right before the 'document' is changed via the 'Document' property.
        public event EventHandler DocumentChanging;

        protected void OnDocumentChanging()
        {
            if (DocumentChanging != null)
            {
                DocumentChanging(this, EventArgs.Empty);
            }
        }

        // DocumentChanged
        // This event is raised right after the 'document' is changed via the 'Document' property.
        public event EventHandler DocumentChanged;
        protected void OnDocumentChanged()
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(this, EventArgs.Empty);
            }
        }

        // ActiveLayerChanging
        // This event is raised right before the 'Layer' is changed via the 'Layer' property.
        public event EventHandler ActiveLayerChanging;
        protected void OnLayerChanging()
        {
            if (ActiveLayerChanging != null)
            {
                ActiveLayerChanging(this, EventArgs.Empty);
            }
        }

        // ActiveLayerChanged
        // This event is raised right after the 'Layer' is changed via the 'Layer' property.
        public event EventHandler ActiveLayerChanged;
        protected void OnLayerChanged()
        {
            this.Focus();
            if (ActiveLayerChanged != null)
            {
                ActiveLayerChanged(this, EventArgs.Empty);
            }
        }

        [Browsable(false)]
        public Rectangle VisibleDocumentBounds
        {
            get
            {
                Rectangle screen = documentView.VisibleDocumentBounds; // documentView Client coordinates
                return this.RectangleToClient(screen); // this Client coordinates
            }
        }

        [Browsable(false)]
        public Rectangle VisibleDocumentRectangle
        {
            get
            {
                return documentView.VisibleDocumentRectangle;
            }
        }

        [Browsable(false)]
        public string DocumentFileName
        {
            get
            {
                return this.documentFileName;
            }
        }

        /// <summary>
        /// The scratch, stencil, accumulation, whatever buffer. This is used by many parts
        /// of Paint.NET as a temporary area for which to store data.
        /// This surface is 'owned' by any Tool that is active. If you want to use this you
        /// must first deactivate the Tool and then activate it when you are finished.
        /// To enforce this, this property returns null when a tool is active.
        /// Tools should use Tool.ScratchSurface instead.
        /// </summary>
        [Browsable(false)]
        public Surface ScratchSurface
        {
            get
            {
                return scratch;
            }

            set
            {
                this.scratch = value;
            }
        }

        [Browsable(false)]
        public Document Document
        {
            get
            {
                return document;
            }
        }

        public ZoomBasis ZoomBasis 
        {
            get 
            {
                return zoomConfigWidget.ZoomBasis;
            }
            set 
            {
                zoomConfigWidget.ZoomBasis = value;
            }
        }

        [Browsable(false)]
        public bool EnableOutlineAnimation
        {
            get
            {
                return this.selectionRenderer.EnableOutlineAnimation;
            }

            set
            {
                this.selectionRenderer.EnableOutlineAnimation = value;
            }
        }

        [Browsable(false)]
        public bool EnableSelectionOutline
        {
            get
            {
                return this.selectionRenderer.EnableSelectionOutline;
            }

            set
            {
                this.selectionRenderer.EnableSelectionOutline = value;
            }
        }

        [Browsable(false)]
        public bool EnableSelectionTinting
        {
            get
            {
                return this.selectionRenderer.EnableSelectionTinting;
            }

            set
            {
                this.selectionRenderer.EnableSelectionTinting = value;
            }
        }

        public void ResetOutlineWhiteOpacity()
        {
            this.selectionRenderer.ResetOutlineWhiteOpacity();
        }

        /// <summary>
        /// Sets the FileType and SaveConfigToken parameters that are used if the
        /// user chooses "Save" from the File menu. These are not used by the
        /// DocumentWorkspace class and should be used by whoever actually goes
        /// to save the Document instance.
        /// </summary>
        /// <param name="fileType"></param>
        /// <param name="saveParameters"></param>
        public void SetDocumentSaveOptions(string fileName, FileType fileType, SaveConfigToken saveParameters)
        {
            this.documentFileName = fileName;
            this.documentFileType = fileType;

            if (saveParameters == null)
            {
                this.documentSaveToken = null;
            }
            else
            {
                this.documentSaveToken = (SaveConfigToken)saveParameters.Clone();
            }
        }

        [Browsable(false)]
        public FileType DocumentFileType
        {
            get
            {
                return this.documentFileType;
            }
        }

        public void GetDocumentSaveOptions(out string fileName, out FileType fileType, out SaveConfigToken saveParameters)
        {
            fileName = this.documentFileName;
            fileType = this.documentFileType;

            if (this.documentSaveToken == null)
            {
                saveParameters = null;
            }
            else
            {
                saveParameters = (SaveConfigToken)this.documentSaveToken.Clone();
            }
        }

        /// <summary>
        /// Updates any pertinent EXIF tags, such as "Creation Software", to be
        /// relevant or up-to-date.
        /// </summary>
        /// <param name="document"></param>
        private void UpdateExifTags(Document document)
        {
            // We want it to say "Creation Software: Paint.NET vX.Y"
            // I have verified that other image editing software overwrites this tag,
            // and does not just add it when it does not exist.
            PropertyItem pi = Exif.CreateAscii(ExifTagID.Software, Application.ProductName); // LOC? should this be localized? if so, use PdnInfo.GetProductName()
            document.MetaData.ReplaceExifValues(ExifTagID.Software, new PropertyItem[1] { pi });
        }
        
        /// <summary>
        /// Sets the Document instance to be managed by DocumentWorkspace.
        /// Before the document is changed, a DocumentChanging event is raised.
        /// After the document is changed, a DocumentChanged event is raised.
        /// The DocumentView contained in our Workspace is also notified of the
        /// change to the Document instance.
        /// Since the DocumentWorkspace takes ownership of this object (it does 
        /// not copy it), it was decided that a method instead of a property should 
        /// be used for the 'set' behavior.
        /// </summary>
        /// <param name="document"></param>
        public void SetDocument(Document document)
        {
            ZoomBasis savedZb = this.ZoomBasis;
            ScaleFactor savedSf = DocumentView.ScaleFactor;

            UpdateExifTags(document);

            Tool oldTool = Environment.Tool;
            Environment.SetTool(null);
            ActiveLayer = null;

            OnDocumentChanging();

            if (this.Document != null)
            {
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging -= this.layerPropertyChangingDelegate;
                    layer.PropertyChanged -= this.layerPropertyChangedDelegate;
                }
            }

            if (!Environment.Selection.IsEmpty)
            {
                Environment.Selection.Reset();
            }

            if (this.document != null)
            {
                this.document.Layers.RemovingAt -= layerRemovingDelegate;
                this.document.Layers.RemovedAt -= layerRemovedDelegate;
                this.document.Layers.Inserted -= layerInsertedDelegate;
            }

            Document oldDocument = this.document;
            documentView.Document = document;
            this.document = null;

            if (this.scratch != null && this.scratch.Size != document.Size)
            {
                this.scratch.Dispose();
                this.scratch = null;
            }

            if (this.scratch == null)
            {
                this.scratch = new Surface(document.Size);
            }

            this.document = document;
            this.environment.Selection.ClipRectangle = this.document.Bounds;

            if (oldDocument != null)
            {
                oldDocument.Dispose();
            }

            OnDocumentChanged();

            if (this.Document != null)
            {
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging += this.layerPropertyChangingDelegate;
                    layer.PropertyChanged += this.layerPropertyChangedDelegate;
                }
            }

            // if the ActiveLayer is not in this new document, then
            // we try to set ActiveLayer to the first layer in this
            // new document. But if the document contains no layers,
            // or is null, we just null the ActiveLayer.
            if (this.document == null)
            {
                ActiveLayer = null;
            }
            else
            {
                if (!this.document.Layers.Contains(activeLayer))
                {
                    if (this.document.Layers.Count > 0)
                    {
                        ActiveLayer = (Layer)this.document.Layers[0];
                    }
                    else
                    {
                        ActiveLayer = null;
                    }
                }

                this.document.Layers.RemovingAt += layerRemovingDelegate;
                this.document.Layers.RemovedAt += layerRemovedDelegate;
                this.document.Layers.Inserted += layerInsertedDelegate;

                bool oldDirty = this.document.Dirty;
                this.document.Invalidate();
                this.document.Dirty = oldDirty;

                this.ZoomBasis = savedZb;
                if (savedZb == ZoomBasis.Factor)
                {
                    this.DocumentView.ScaleFactor = savedSf;
                }
            }

            Environment.SetTool(oldTool);

            documentView.AutoScrollPosition = new Point(0, 0);

            // we invalidate each layer so that it raises the PreviewChanged event
            foreach (Layer layer in Document.Layers)
            {
                layer.Invalidate();
            }

            Invalidate(true);
        }

        [Browsable(false)]
        public DocumentWidgets Widgets
        {
            get
            {
                return widgets;
            }
        }

        [Browsable(false)]
        public HistoryStack History
        {
            get
            {
                return history;
            }
        }

        [Browsable(false)]
        public DocumentEnvironment Environment
        {
            get
            {
                return environment;
            }
        }

        [Browsable(false)]
        public int ActiveLayerIndex
        {
            get
            {
                return Document.Layers.IndexOf(ActiveLayer);
            }

            set
            {
                this.ActiveLayer = (Layer)Document.Layers[value];
            }
        }

        [Browsable(false)]
        public Layer ActiveLayer
        {
            get
            {
                return activeLayer;
            }

            set
            {
                OnLayerChanging();

                bool deactivateTool;

                if (Environment.Tool != null)
                {
                    deactivateTool = Environment.Tool.DeactivateOnLayerChange;
                }
                else
                {
                    deactivateTool = false;
                }

                Type oldToolType = null;
                if (deactivateTool)
                {
                    toolPulseTimer.Enabled = false;
                    oldToolType = Environment.GetToolType();
                    //Environment.Tool.PerformDeactivate();
                    Environment.SetTool(null);
                }

                // Verify that the layer is in the document (sanity checking)
                if (Document != null)
                {
                    if (value != null && !Document.Layers.Contains(value))
                    {
                        throw new InvalidOperationException("ActiveLayer was changed to a layer that is not contained within the Document");
                    }
                }
                else
                {   // Document == null
                    if (value != null)
                    {
                        throw new InvalidOperationException("ActiveLayer was set to non-null while Document was null");
                    }
                }

                activeLayer = value;

                if (deactivateTool)
                {
                    //Environment.Tool.PerformActivate();
                    Environment.SetTool(oldToolType, this);
                    toolPulseTimer.Enabled = true;
                }

                OnLayerChanged();
            }
        }

        [Browsable(false)]
        public Type[] Effects
        {
            get
            {
                if (effects == null)
                {
                    InitializeEffects();
                }

                return (Type[])effects.Clone();
            }
        }

        [Browsable(false)]
        public Type[] Tools
        {
            get
            {
                return (Type[])tools.Clone();
            }
        }

        [Browsable(false)]
        public ToolInfo[] ToolInfos
        {
            get
            {
                if (toolInfos == null)
                {
                    InitializeToolInfos();
                }

                return (ToolInfo[])toolInfos.Clone();
            }
        }

        [Browsable(false)]
        public DocumentView DocumentView
        {
            get
            {
                return documentView;
            }
        }

        private void HistoryChangedHandler(object sender, EventArgs e)
        {   
            // enable/disable buttons on the CommonActionsWidget
            if (history.UndoStack.Count > 1)
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Undo, true);
            }
            else
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Undo, false);
            }

            if (history.RedoStack.Count > 0)
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Redo, true);
            }
            else
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Redo, false);
            }
        }

        /// <summary>
        /// Initializes a new instance of the DocumentWorkspace class.
        /// </summary>
        public DocumentWorkspace()
        {
            this.document = null;
            this.environment = new DocumentEnvironment();
            this.environment.ToolStatusChanged += new EventHandler(environment_ToolStatusChanged);
            this.activeLayer = null;
            this.tools = null;
            this.history = new HistoryStack(this);

            // initialize!
            InitializeTools();
            InitializeComponent();
            InitializeFloatingForms();
            InitializeToolBars();

            this.historyForm.HistoryControl.HistoryStack = this.history;
            
            history.Changed += new EventHandler(HistoryChangedHandler);
           
            // set the workspace toggle buttons correctly
            this.workspaceOptionsConfigWidget.Units = Environment.Units;
            this.workspaceOptionsConfigWidget.RulersEnabled = documentView.RulersEnabled;
            this.workspaceOptionsConfigWidget.DrawGrid = documentView.DrawGrid;
            this.drawModesConfigWidget.AntiAliasing = Environment.AntiAliasing;

            // hook the DocumentView with its selectedPath ...
            this.selectionRenderer = new SelectionRenderer(this.DocumentView.Renderers, this.environment.Selection, this.DocumentView);
            this.DocumentView.Renderers.Add(this.selectionRenderer, true);
            this.selectionRenderer.EnableOutlineAnimation = true;
            this.selectionRenderer.EnableSelectionTinting = false;
            this.selectionRenderer.EnableSelectionOutline = true;

            // hook into Environment *Changed events
            foreColorChangedDelegate = new EventHandler(ForeColorChangedHandler);
            Environment.ForeColorChanged += foreColorChangedDelegate;
            mainToolBarForm.MainToolBar.ColorDisplay.UserForeColorChanged += foreColorChangedDelegate;

            backColorChangedDelegate = new EventHandler(BackColorChangedHandler);
            Environment.BackColorChanged += backColorChangedDelegate;
            mainToolBarForm.MainToolBar.ColorDisplay.UserBackColorChanged += backColorChangedDelegate;

            mainToolBarForm.MainToolBar.ColorDisplay.UserForeAndBackColorsChanged += new EventHandler(ColorDisplay_UserForeAndBackColorsChanged);

            shapeDrawTypeChangedDelegate = new EventHandler(ShapeDrawTypeChangedHandler);
            Environment.ShapeDrawTypeChanged += shapeDrawTypeChangedDelegate;
            
            Environment.ToleranceChanged += new EventHandler(OnEnvironmentToleranceChanged);
            mainToolBarForm.MainToolBar.ToleranceSlider.ToleranceChanged += new EventHandler(OnToolBarToleranceChanged);

            alphaBlendingChangedDelegate = new EventHandler(AlphaBlendingChangedHandler);
            Environment.AlphaBlendingChanged += alphaBlendingChangedDelegate;
            
            Environment.FontInfo = textConfigWidget.FontInfo;
            Environment.TextAlignment = textConfigWidget.TextAlignment;
            textConfigWidget.TextAlignmentChanged += new EventHandler(textConfigWidget_TextAlignmentChanged);
            textConfigWidget.FontTextChanged += new EventHandler(textConfigWidget_FontTextChanged);
            textConfigWidget.RelinquishFocus += new EventHandler(RelinquishFocusHandler2);
            Environment.AntiAliasingChanged += new EventHandler(Environment_AntiAliasingChanged);
            Environment.UnitsChanged += new EventHandler(Environment_UnitsChanged);
            Environment.FontInfoChanged += new EventHandler(Environment_FontInfoChanged);
            Environment.TextAlignmentChanged += new EventHandler(Environment_TextAlignmentChanged);

            // hook into the SelectedPathChanged event ...
            selectedPathChangingDelegate = new EventHandler(SelectedPathChangingHandler);
            Environment.Selection.Changing += selectedPathChangingDelegate;
            selectedPathChangedDelegate = new EventHandler(SelectedPathChangedHandler);
            Environment.Selection.Changed += selectedPathChangedDelegate;

            // hook into the ZoomChanged event
            zoomChangedDelegate = new EventHandler(ZoomChangedHandler);
            documentView.ScaleFactorChanged += zoomChangedDelegate;
            documentView.Units = Environment.Units;

            // layer events
            layerRemovingDelegate = new IndexEventHandler(LayerRemovingHandler);
            layerRemovedDelegate = new IndexEventHandler(LayerRemovedHandler);
            layerInsertedDelegate = new IndexEventHandler(LayerInsertedHandler);
            layerPropertyChangingDelegate = new PropertyEventHandler(LayerPropertyChangingHandler);
            layerPropertyChangedDelegate = new PropertyEventHandler(LayerPropertyChangedHandler);

            // init the Widgets container
            widgets = new DocumentWidgets(this);
            widgets.TopDock = this.topDock;
            widgets.PenConfigWidget = penConfigWidget;
            widgets.ZoomConfigWidget = zoomConfigWidget;
            widgets.BrushConfigWidget = brushConfigWidget;
            widgets.ShapeDrawTypeConfigWidget = this.shapeDrawTypeConfigWidget;
            widgets.CommonActionsWidget = this.commonActionsWidget;
            widgets.TextConfigWidget = this.textConfigWidget;
            widgets.DrawModesConfigWidget = this.drawModesConfigWidget;
            widgets.MainToolBarForm = mainToolBarForm;
            widgets.LayerForm = layerForm;
            widgets.HistoryForm = historyForm;
            widgets.ColorsForm = colorsForm;

            //
            penConfigWidget.PerformPenChanged();
            brushConfigWidget.PerformBrushChanged();
            shapeDrawTypeConfigWidget.PerformShapeDrawTypeChanged();
            
            // Synchronize
            Environment.PerformAllChanged();

            // PaintBrush tool = the default
            Widgets.MainToolBar.SelectTool(typeof(PaintBrushTool));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.document != null)
                {
                    this.document.Dispose();
                    this.document = null;
                }

                if (this.scratch != null)
                {
                    this.scratch.Dispose();
                    this.scratch = null;
                }
            }

            base.Dispose(disposing);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.DocumentView.Select();
            SelectedPathChangedHandler(this, EventArgs.Empty);
            base.OnLoad(e);
        }

        public void RefreshTool()
        {
            Type toolType = environment.GetToolType();
            Widgets.MainToolBar.SelectTool(toolType);
        }

        /// <summary>
        /// Keeps the Environment's ShapeDrawType and the corresponding widget synchronized
        /// </summary>
        private void ShapeDrawTypeChangedHandler(object sender, EventArgs e)
        {
            if (widgets.ShapeDrawTypeConfigWidget.ShapeDrawType != Environment.ShapeDrawType)
            {
                widgets.ShapeDrawTypeConfigWidget.ShapeDrawType = Environment.ShapeDrawType;
            }
        }

        /// <summary>
        /// Keeps the Environment's Compositing Mode and the corresponding widget synchronized
        /// </summary>
        private void AlphaBlendingChangedHandler(object sender, EventArgs e)
        {
            if (widgets.DrawModesConfigWidget.AlphaBlending != Environment.AlphaBlending)
            {
                widgets.DrawModesConfigWidget.AlphaBlending = Environment.AlphaBlending;
            }
        }

        private void ColorDisplay_UserForeAndBackColorsChanged(object sender, EventArgs e)
        {
            // We need to make sure that we don't change which user color is selected (primary vs. secondary)
            // To do this we choose the ordering based on which one is currently active (primary vs. secondary)
            if (widgets.ColorsForm.WhichUserColor == WhichUserColor.Foreground)
            {
                widgets.ColorsForm.SetColorControlsRedraw(false);
                BackColorChangedHandler(sender, e);
                ForeColorChangedHandler(sender, e);
                widgets.ColorsForm.SetColorControlsRedraw(true);
                widgets.ColorsForm.WhichUserColor = WhichUserColor.Foreground;
            }
            else //if (widgets.ColorsForm.WhichUserColor == WhichUserColor.Background)
            {
                widgets.ColorsForm.SetColorControlsRedraw(false);
                ForeColorChangedHandler(sender, e);
                BackColorChangedHandler(sender, e);
                widgets.ColorsForm.SetColorControlsRedraw(true);
                widgets.ColorsForm.WhichUserColor = WhichUserColor.Background;
            }
        }
 
        /// <summary>
        /// Handles the ForeColorChanged event that is raised by the DocumentEnvironment.
        /// </summary>
        private void ForeColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == environment)
            {
                widgets.ColorDisplayWidget.UserForeColor = Environment.ForeColor;
                widgets.ColorsForm.UserForeColor = Environment.ForeColor;
            }
            else if (sender == widgets.ColorDisplayWidget)
            {
                Environment.ForeColor = widgets.ColorDisplayWidget.UserForeColor;
            }
        }

        /// <summary>
        /// Handles the ToleranceChanged event that is raised by the toolbar
        /// </summary>
        private void OnToolBarToleranceChanged(object sender, EventArgs e)
        {
            Environment.Tolerance = widgets.MainToolBar.ToleranceSlider.Tolerance;
            Settings.CurrentUser.SetSingle(PdnSettings.Tolerance, Environment.Tolerance);
            this.Focus();
        }

        /// <summary>
        /// Handles the ToleranceChanged event that is raised by the DocumentEnviroment
        /// </summary>
        private void OnEnvironmentToleranceChanged(object sender, EventArgs e)
        {
            widgets.MainToolBar.ToleranceSlider.Tolerance = Environment.Tolerance;
            Settings.CurrentUser.SetSingle(PdnSettings.Tolerance, Environment.Tolerance);
            this.Focus();
        }

        /// <summary>
        /// Handles the BackColorChanged event that is raised by the DocumentEnvironment.
        /// </summary>
        private void BackColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == environment)
            {
                widgets.ColorDisplayWidget.UserBackColor = Environment.BackColor;
                widgets.ColorsForm.UserBackColor = Environment.BackColor;
            }
            else if (sender == widgets.ColorDisplayWidget)
            {
                Environment.BackColor = widgets.ColorDisplayWidget.UserBackColor;
            }
        }

        private void RelinquishFocusHandler(object sender, EventArgs e)
        {
            this.Focus();
        }

        private void RelinquishFocusHandler2(object sender, EventArgs e)
        {
            this.documentView.Focus();
        }

        private void colorsForm_UserForeColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm cf = (ColorsForm)sender;
            Environment.ForeColor = e.Color;
            widgets.ColorDisplayWidget.UserForeColor = e.Color;
        }

        private void colorsForm_UserBackColorChanged(object sender, ColorEventArgs e)
        {
            ColorsForm cf = (ColorsForm)sender;
            Environment.BackColor = e.Color;
            widgets.ColorDisplayWidget.UserBackColor = e.Color;
        }

        private void colorDisplay_ForeColorClicked(object sender, System.EventArgs e)
        {
            widgets.ColorsForm.WhichUserColor = WhichUserColor.Foreground;
            widgets.ColorsForm.UserForeColor = Environment.ForeColor;
            widgets.ColorsForm.UserBackColor = Environment.BackColor;
            widgets.ColorsForm.Show();
            widgets.ColorsForm.Focus();
            this.RelinquishFocusHandler(widgets.ColorsForm, EventArgs.Empty);
        }

        private void colorDisplay_BackColorClicked(object sender, System.EventArgs e)
        {
            widgets.ColorsForm.WhichUserColor = WhichUserColor.Background;
            widgets.ColorsForm.UserForeColor = Environment.ForeColor;
            widgets.ColorsForm.UserBackColor = Environment.BackColor;
            widgets.ColorsForm.Show();
            widgets.ColorsForm.Focus();
            this.RelinquishFocusHandler(widgets.ColorsForm, EventArgs.Empty);
        }

        private void LayerRemovingHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)Document.Layers[e.Index];
            layer.PropertyChanging -= layerPropertyChangingDelegate;
            layer.PropertyChanged -= layerPropertyChangedDelegate;

            // pick a new valid layer!
            int newLayerIndex;

            if (e.Index == Document.Layers.Count - 1)
            {
                newLayerIndex = e.Index - 1;
            }
            else
            {
                newLayerIndex = e.Index + 1;
            }

            ActiveLayer = (Layer)Document.Layers[newLayerIndex];
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {   
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)Document.Layers[e.Index];
            ActiveLayer = layer;
            layer.PropertyChanging += layerPropertyChangingDelegate;
            layer.PropertyChanged += layerPropertyChangedDelegate;
        }

        private void LayerPropertyChangingHandler(object sender, PropertyEventArgs e)
        {
            string nameFormat = PdnResources.GetString("DocumentWorkspace.LayerPropertyChangingHandler.HistoryActionNameFormat");
            string haName = string.Format(nameFormat, e.PropertyName);

            LayerPropertyHistoryAction lpha = new LayerPropertyHistoryAction(
                haName, 
                PdnResources.GetImage("Icons.MenuLayersLayerPropertiesIcon.bmp"), 
                this, 
                Document.Layers.IndexOf(sender));

            History.PushNewAction(lpha);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            Layer layer = (Layer)sender;

            if (!layer.Visible && layer == this.ActiveLayer && document.Layers.Count > 1)
            {
                SelectClosestVisibleLayer(layer);
            }
        }

        private void SelectClosestVisibleLayer(Layer layer)
        {
            int oldLayerIndex = document.Layers.IndexOf(layer);
            int newLayerIndex = oldLayerIndex;

            // find the closest layer that is still visible
            for (int i = 0; i < document.Layers.Count; ++i)
            {
                int lower = oldLayerIndex - i;
                int upper = oldLayerIndex + i;

                if (lower >= 0 && lower < document.Layers.Count && ((Layer)document.Layers[lower]).Visible)
                {
                    newLayerIndex = lower;
                    break;
                }

                if (upper >= 0 && upper < document.Layers.Count && ((Layer)document.Layers[upper]).Visible)
                {
                    newLayerIndex = upper;
                    break;
                }
            }

            if (newLayerIndex != oldLayerIndex)
            {
                this.ActiveLayer = (Layer)document.Layers[newLayerIndex];
            }
        }

        private void UpdateRulerSelectionTinting()
        {
            if (this.documentView.RulersEnabled)
            {
                Rectangle bounds = environment.Selection.GetBounds();
                this.documentView.SetHighlightRectangle(bounds);
            }
        }

        /// <summary>
        /// Handles the SelectedPathChanging event that is raised by the DocumentEnvironment.
        /// </summary>
        private void SelectedPathChangingHandler(object sender, EventArgs e)
        {   
        }

        /// <summary>
        /// Handles the SelectedPathChanged event that is raised by the DocumentEnvironment.
        /// </summary>
        private void SelectedPathChangedHandler(object sender, EventArgs e)
        {
            UpdateRulerSelectionTinting();
 
            // set buttons on CommonActionsWidgets
            if (Environment.Selection.IsEmpty)
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Cut, false);
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Copy, false);
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Deselect, false);
            }
            else
            {
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Cut, true);
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Copy, true);
                widgets.CommonActionsWidget.SetButtonEnabled(CommonAction.Deselect, true);
            }
        }
        
        private void ZoomChangedHandler(object sender, EventArgs e)
        {
            zoomConfigWidget.ZoomBasis = ZoomBasis.Factor;
            zoomConfigWidget.ScaleFactor = documentView.ScaleFactor;
        }

        private void InitializeComponent()
        {
            this.documentView = new PaintDotNet.DocumentWorkspace.OurDocumentView();
            this.topDock = new PaintDotNet.FlowPanel();
            this.textConfigWidget = new PaintDotNet.TextConfigWidget();
            this.shapeDrawTypeConfigWidget = new PaintDotNet.ShapeDrawTypeConfigWidget();
            this.drawModesConfigWidget = new PaintDotNet.DrawModesConfigWidget();
            this.penConfigWidget = new PaintDotNet.PenConfigWidget();
            this.brushConfigWidget = new PaintDotNet.BrushConfigWidget();
            this.workspaceOptionsConfigWidget = new PaintDotNet.WorkspaceOptionsConfigWidget();
            this.zoomConfigWidget = new PaintDotNet.ZoomConfigWidget();
            this.commonActionsWidget = new PaintDotNet.CommonActionsWidget();
            this.toolPulseTimer = new System.Windows.Forms.Timer();
            this.topDock.SuspendLayout();
            this.SuspendLayout();
            // 
            // documentView
            // 
            this.documentView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.documentView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.documentView.Document = null;
            this.documentView.DrawGrid = false;
            this.documentView.Location = new System.Drawing.Point(0, 54);
            this.documentView.Name = "documentView";
            this.documentView.PanelAutoScroll = true;
            this.documentView.RulersEnabled = false;
            this.documentView.Size = new System.Drawing.Size(872, 586);
            this.documentView.TabIndex = 0;
            this.documentView.TabStop = false;
            this.documentView.RulersEnabledChanged += new System.EventHandler(this.documentView_RulersEnabledChanged);
            this.documentView.DocumentMouseEnter += new EventHandler(this.DocumentMouseEnterHandler);
            this.documentView.DocumentMouseLeave += new EventHandler(this.DocumentMouseLeaveHandler);
            this.documentView.DocumentMouseMove += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseMoveHandler);
            this.documentView.DocumentMouseDown += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseDownHandler);
            this.documentView.Scroll += new System.EventHandler(this.documentView_Scroll);
            this.documentView.DrawGridChanged += new System.EventHandler(this.documentView_DrawGridChanged);
            this.documentView.DocumentClick += new System.EventHandler(this.DocumentClick);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseUpHandler);
            this.documentView.DocumentKeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DocumentKeyPress);
            this.DocumentView.DocumentKeyUp += new KeyEventHandler(DocumenKeyUp);
            this.DocumentView.DocumentKeyDown += new KeyEventHandler(DocumentKeyDown);
            this.DocumentView.MouseWheel += new MouseEventHandler(DocumentView_MouseWheel);
            // 
            // topDock
            // 
            this.topDock.BackColor = SystemColors.Control;
            this.topDock.Controls.Add(this.textConfigWidget);
            this.topDock.Controls.Add(this.drawModesConfigWidget);
            this.topDock.Controls.Add(this.penConfigWidget);
            this.topDock.Controls.Add(this.shapeDrawTypeConfigWidget);
            this.topDock.Controls.Add(this.brushConfigWidget);
            this.topDock.Controls.Add(this.workspaceOptionsConfigWidget);
            this.topDock.Controls.Add(this.zoomConfigWidget);
            this.topDock.Controls.Add(this.commonActionsWidget);
            this.topDock.Dock = System.Windows.Forms.DockStyle.Top;
            this.topDock.Location = new System.Drawing.Point(0, 0);
            this.topDock.Name = "topDock";
            this.topDock.Size = new System.Drawing.Size(872, 54);
            this.topDock.TabIndex = 3;
            // 
            // textConfigWidget
            // 
            this.textConfigWidget.FontSize = 12F;
            this.textConfigWidget.FontStyle = System.Drawing.FontStyle.Regular;
            this.textConfigWidget.Location = new System.Drawing.Point(78, 27);
            this.textConfigWidget.Name = "textConfigWidget";
            this.textConfigWidget.Size = new System.Drawing.Size(397, 27);
            this.textConfigWidget.TabIndex = 3;
            this.textConfigWidget.TextAlignment = PaintDotNet.TextAlignment.Left;
            // 
            // shapeDrawTypeConfigWidget
            // 
            this.shapeDrawTypeConfigWidget.Location = new System.Drawing.Point(0, 27);
            this.shapeDrawTypeConfigWidget.Name = "shapeDrawTypeConfigWidget";
            this.shapeDrawTypeConfigWidget.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.shapeDrawTypeConfigWidget.Size = new System.Drawing.Size(83, 27);
            this.shapeDrawTypeConfigWidget.TabIndex = 1;
            this.shapeDrawTypeConfigWidget.TabStop = false;
            this.shapeDrawTypeConfigWidget.ShapeDrawTypeChanged += new System.EventHandler(this.shapeDrawTypeConfigWidget_ShapeDrawTypeChanged);
            // 
            // drawModesConfigWidget
            // 
            this.drawModesConfigWidget.Name = "drawModesConfigWidget";
            this.drawModesConfigWidget.Size = new System.Drawing.Size(62, 27);
            this.drawModesConfigWidget.TabIndex = 1;
            this.drawModesConfigWidget.TabStop = false;
            this.drawModesConfigWidget.AlphaBlendingChanged += new EventHandler(drawModesConfigWidget_AlphaBlendingChanged);
            this.drawModesConfigWidget.AntiAliasingChanged += new System.EventHandler(this.drawModesConfigWidget_AntiAliasingChanged);
            // 
            // penConfigWidget
            // 
            this.penConfigWidget.Location = new System.Drawing.Point(659, 0);
            this.penConfigWidget.Name = "penConfigWidget";
            this.penConfigWidget.Size = new System.Drawing.Size(137, 27);
            this.penConfigWidget.TabIndex = 2;
            this.penConfigWidget.PenChanged += new System.EventHandler(this.penConfigWidget_PenChanged);
            // 
            // brushConfigWidget
            // 
            this.brushConfigWidget.Location = new System.Drawing.Point(456, 0);
            this.brushConfigWidget.Name = "brushConfigWidget";
            this.brushConfigWidget.Size = new System.Drawing.Size(213, 27);
            this.brushConfigWidget.TabIndex = 1;
            this.brushConfigWidget.BrushChanged += new System.EventHandler(this.brushConfigWidget_BrushChanged);
            // 
            // workspaceOptionsConfigWidget
            // 
            this.workspaceOptionsConfigWidget.DrawGrid = false;
            this.workspaceOptionsConfigWidget.Location = new System.Drawing.Point(374, 0);
            this.workspaceOptionsConfigWidget.Name = "workspaceOptionsConfigWidget";
            this.workspaceOptionsConfigWidget.RulersEnabled = false;
            this.workspaceOptionsConfigWidget.Size = new System.Drawing.Size(201, 27);
            this.workspaceOptionsConfigWidget.TabIndex = 3;
            this.workspaceOptionsConfigWidget.TabStop = false;
            this.workspaceOptionsConfigWidget.RulersEnabledChanged += new System.EventHandler(this.workspaceOptionsConfigWidget_RulersEnabledChanged);
            this.workspaceOptionsConfigWidget.DrawGridChanged += new System.EventHandler(this.workspaceOptionsConfigWidget_DrawGridChanged);
            this.workspaceOptionsConfigWidget.UnitsChanged += new EventHandler(workspaceOptionsConfigWidget_UnitsChanged);
            // 
            // zoomConfigWidget
            // 
            this.zoomConfigWidget.Location = new System.Drawing.Point(256, 0);
            this.zoomConfigWidget.Name = "zoomConfigWidget";
            this.zoomConfigWidget.Size = new System.Drawing.Size(127, 27);
            this.zoomConfigWidget.TabIndex = 0;
            this.zoomConfigWidget.ZoomBasis = PaintDotNet.ZoomBasis.Window;
            this.zoomConfigWidget.ZoomBasisChanged += new System.EventHandler(this.zoomConfigWidget_ZoomBasisChanged);
            this.zoomConfigWidget.ZoomScaleChanged += new System.EventHandler(this.zoomConfigWidget_ZoomScaleChanged);
            this.zoomConfigWidget.ZoomIn += new EventHandler(zoomConfigWidget_ZoomIn);
            this.zoomConfigWidget.ZoomOut += new EventHandler(zoomConfigWidget_ZoomOut);
            // 
            // commonActionsWidget
            // 
            this.commonActionsWidget.Location = new System.Drawing.Point(0, 0);
            this.commonActionsWidget.Name = "commonActionsWidget";
            this.commonActionsWidget.Size = new System.Drawing.Size(256, 27);
            this.commonActionsWidget.TabIndex = 4;
            this.commonActionsWidget.TabStop = false;
            // 
            // toolPulseTimer
            // 
            this.toolPulseTimer.Interval = 16;
            this.toolPulseTimer.Tick += new EventHandler(this.toolPulseTimer_Tick);
            // 
            // DocumentWorkspace
            // 
            this.Controls.Add(this.documentView);
            this.Controls.Add(this.topDock);
            this.Name = "DocumentWorkspace";
            this.Size = new System.Drawing.Size(872, 640);
            this.topDock.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        // The Document* events are raised by the Document class, handled here,
        // and relayed as necessary. For instance, for the DocumentMouse* events, 
        // these are all relayed to the active tool.

        private void DocumentMouseEnterHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformMouseEnter();
            }
        }

        private void DocumentMouseLeaveHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformMouseLeave();
            }
        }

        private void DocumentMouseUpHandler(object sender, MouseEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformMouseUp(e);
            }
        }

        private void DocumentMouseDownHandler(object sender, MouseEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformMouseDown(e);
            }
        }

        private void DocumentMouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformMouseMove(e);
            }
        }

        private void DocumentClick(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformClick();
            }
        }

        private void DocumentKeyPress(object sender, KeyPressEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformKeyPress(e);
            }
        }

        private void DocumentKeyDown(object sender, KeyEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformKeyDown(e);
            }
        }

        private void DocumenKeyUp(object sender, KeyEventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.PerformKeyUp(e);
            }
        }

        private void InitializeFloatingForms()
        {
            // MainToolBarForm
            mainToolBarForm = new MainToolBarForm();
            mainToolBarForm.MainToolBar.ColorDisplay.UserForeColorClick += new EventHandler(colorDisplay_ForeColorClicked);
            mainToolBarForm.MainToolBar.ColorDisplay.UserBackColorClick += new EventHandler(colorDisplay_BackColorClicked);
            mainToolBarForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            mainToolBarForm.AttachControl = this.DocumentView;

            // LayerForm
            layerForm = new LayerForm();
            layerForm.LayerControl.Workspace = this;
            layerForm.LayerControl.ClickedOnLayer += new LayerEventHandler(layerControl_ClickedOnLayer);
            layerForm.NewLayerButtonClick += new EventHandler(layerForm_NewLayerButtonClicked);
            layerForm.DeleteLayerButtonClick += new EventHandler(layerForm_DeleteLayerButtonClicked);                        
            layerForm.DuplicateLayerButtonClick += new EventHandler(layerForm_DuplicateLayerButtonClick);
            layerForm.MoveLayerUpButtonClick += new EventHandler(layerForm_MoveLayerUpButtonClicked);
            layerForm.MoveLayerDownButtonClick += new EventHandler(layerForm_MoveLayerDownButtonClicked);
            layerForm.PropertiesButtonClick += new EventHandler(layerForm_PropertiesButtonClick);
            layerForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            layerForm.AttachControl = this.DocumentView;
            
            // HistoryForm
            historyForm = new HistoryForm();
            historyForm.ClearHistoryButtonClicked += new EventHandler(historyForm_ClearHistoryButtonClicked);
            historyForm.RewindButtonClicked += new EventHandler(historyForm_RewindButtonClicked);
            historyForm.UndoButtonClicked += new EventHandler(historyForm_UndoButtonClicked);
            historyForm.RedoButtonClicked += new EventHandler(historyForm_RedoButtonClicked);
            historyForm.FastForwardButtonClicked += new EventHandler(historyForm_FastForwardButtonClicked);
            historyForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            historyForm.AttachControl = this.DocumentView;

            // ColorsForm
            colorsForm = new ColorsForm();
            colorsForm.UserForeColor = Environment.ForeColor;
            colorsForm.UserBackColor = Environment.BackColor;
            colorsForm.WhichUserColor = WhichUserColor.Foreground;
            colorsForm.UserForeColorChanged += new ColorEventHandler(colorsForm_UserForeColorChanged);
            colorsForm.UserBackColorChanged += new ColorEventHandler(colorsForm_UserBackColorChanged);
            colorsForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            colorsForm.AttachControl = this.DocumentView;
        }

        protected void InitializeTools()
        {
            // add all the tools
            this.tools = new Type[] {
                                        typeof(RectangleSelectTool),
                                        typeof(MoveTool),
                                        typeof(LassoSelectTool),
                                        typeof(MoveSelectionTool),
                                        //typeof(PanTool),
                                        typeof(EllipseSelectTool),
                                        typeof(ZoomTool),
                                        typeof(MagicWandTool),
                                        typeof(TextTool),
            
                                        typeof(PaintBrushTool),
                                        typeof(EraserTool),
                                        typeof(PencilTool),
                                        typeof(ColorPickerTool),
                                        typeof(CloneStampTool), 
                                        typeof(RecolorTool),
                                        typeof(PaintBucketTool),

                                        typeof(LineTool),
                                        typeof(RectangleTool),
                                        typeof(RoundedRectangleTool),
                                        typeof(EllipseTool),
                                        typeof(FreeformShapeTool),

                                    };
        }

        protected void InitializeToolInfos()
        {
            int i = 0;
            Type[] tools = this.Tools;
            this.toolInfos = new ToolInfo[tools.Length];

            foreach (Type toolType in this.tools)
            {
                using (Tool tool = Tool.CreateTool(toolType, this))
                {
                    toolInfos[i] = tool.Info;
                    ++i;
                }
            }
        }

        private Type[] GetEffectsFromAssembly(Assembly assembly)
        {
            ArrayList effects = new ArrayList();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Effect)) && !type.IsAbstract)
                {
                    effects.Add(type);
                }
            }

            // convert from ArrayList to normal array
            return (Type[])effects.ToArray(typeof(Type));
        }

        private void InitializeEffects()
        {
            ArrayList effectsArrays = new ArrayList();
            string homeDir = PdnInfo.GetApplicationDir();

            try
            {
                Assembly effectsAssembly = Assembly.GetAssembly(typeof(Effect));
                Type[] effectTypes = GetEffectsFromAssembly(effectsAssembly);

                effectsArrays.Add(effectTypes);
            }

            catch (FileNotFoundException)
            {
                //Utility.ErrorBox(this, "PaintDotNet.Effects.dll could not be found -- many Effects will not be available.");
            }

            string effectsDir = Path.Combine(homeDir, "Effects");
            bool dirExists;

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(effectsDir);
                dirExists = dirInfo.Exists;
            }

            catch
            {
                dirExists = false;
            }

            if (dirExists)
            {
                foreach (string fileName in Directory.GetFiles(effectsDir, "*.dll"))
                {
                    bool success = false;
                    Assembly pluginAssembly = null;
                    System.Type[] pluginEffects = null;

                    try
                    {
                        pluginAssembly = Assembly.LoadFrom(fileName);
                        pluginEffects = GetEffectsFromAssembly(pluginAssembly);
                        success = true;
                    }

                    catch (Exception)
                    {
                        string errorFormat = PdnResources.GetString("DocumentWorkspace.InitializeEffects.DllLoadFailed.Format");
                        string errorText = string.Format(errorFormat, fileName);
                        Utility.ErrorBox(this, errorText);
                    }

                    if (success)
                    {
                        effectsArrays.Add(pluginEffects);
                    }
                }
            }

            effectsArrays.Add(GetEffectsFromAssembly(Assembly.GetExecutingAssembly()));

            // convert ArrayList of arrays into one big array
            int count = 0;

            foreach (Type[] typeArray in effectsArrays)
            {
                count += typeArray.Length;
            }

            this.effects = new Type[count];
            int cursor = 0;

            foreach (Type[] typeArray in effectsArrays)
            {
                foreach (Type type in typeArray)
                {
                    this.effects[cursor] = type;
                    ++cursor;
                }
            }
        }

        /// <summary>
        /// Provides a way for you to perform an action by its class name. 
        /// This only works for DocumentWorkspace actions don't take any extra context.
        /// This works for both DocumentAction and WorkspaceAction types.
        /// </summary>
        public void PerformAction(Type actionType, params object[] parameters)
        {
            Type oldToolType = Environment.GetToolType();
            Environment.SetTool(null);

            Update();

            using (new WaitCursorChanger(this))
            {
                ConstructorInfo ci = actionType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
                object action = (DocumentAction)ci.Invoke(new object[] { this });

                if (action is DocumentAction)
                {
                    if (parameters.Length != 0)
                    {
                        throw new ArgumentException("can't specify parameters for DocumentActions");
                    }

                    this.PerformAction((DocumentAction)action);
                }
                else if (action is WorkspaceAction)
                {
                    this.PerformAction((WorkspaceAction)action);
                }
            }

            Environment.SetTool(oldToolType, this);
        }

        /// <summary>
        /// Same as PerformAction(Type) except it lets you rename the HistoryAction's name.
        /// </summary>
        /// <param name="actionType"></param>
        /// <param name="newName"></param>
        public void PerformAction(Type actionType, string newName, Image icon)
        {
            Type oldToolType = Environment.GetToolType();
            Environment.SetTool(null);

            Update();

            using (new WaitCursorChanger(this))
            {
                ConstructorInfo ci = actionType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
                DocumentAction action = (DocumentAction)ci.Invoke(new object[] { this });
                HistoryAction ha = action.PerformAction();

                if (ha != null)
                {
                    ha.Name = newName;
                    ha.Image = icon;
                    History.PushNewAction(ha);
                }
            }

            Environment.SetTool(oldToolType, this);
        }

        public void PerformAction(DocumentAction performMe)
        {
            Type oldToolType = Environment.GetToolType();
            Environment.SetTool(null);

            Update();

            using (new WaitCursorChanger(this))
            {           
                HistoryAction ha = performMe.PerformAction();

                if (ha != null)
                {
                    History.PushNewAction(ha);
                }
            }

            Environment.SetTool(oldToolType, this);
        }

        public void PerformAction(WorkspaceAction performMe, params object[] parameters)
        {
            performMe.PerformAction(parameters);
        }
        
        private void mainToolBar_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            documentView.Focus();
            Environment.SetTool(e.ToolType, this);
        }

        private void ToolChangedHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                documentView.Cursor = Environment.Tool.Cursor;
                Environment.Tool.CursorChanged += new EventHandler(ToolCursorChangedHandler);
                this.toolPulseTimer.Enabled = true;
                widgets.MainToolBar.SelectTool(Environment.GetToolType(), false);
            }
            else
            {
                this.toolPulseTimer.Enabled = false;
            }
        }

        private void ToolChangingHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                Environment.Tool.CursorChanged -= new EventHandler(ToolCursorChangedHandler);
            }
        }

        private void ToolCursorChangedHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                documentView.Cursor = Environment.Tool.Cursor;
            }
        }

        private void InitializeToolBars()
        {
            mainToolBarForm.MainToolBar.SetTools(this.ToolInfos, this);
            mainToolBarForm.MainToolBar.ToolClicked += new ToolClickedEventHandler(this.mainToolBar_ToolClicked);
            Environment.ToolChanging += new EventHandler(this.ToolChangingHandler);
            Environment.ToolChanged += new EventHandler(this.ToolChangedHandler);
        }

        private void workspaceOptionsConfigWidget_DrawGridChanged(object sender, EventArgs e)
        {
            documentView.DrawGrid = ((WorkspaceOptionsConfigWidget)sender).DrawGrid;
        }

        private void drawModesConfigWidget_AntiAliasingChanged(object sender, System.EventArgs e)
        {
            Environment.AntiAliasing = ((DrawModesConfigWidget)sender).AntiAliasing;
        }

        private void penConfigWidget_PenChanged(object sender, System.EventArgs e)
        {
            Environment.PenInfo = penConfigWidget.PenInfo;
        }

        private void brushConfigWidget_BrushChanged(object sender, System.EventArgs e)
        {
            Environment.BrushInfo = brushConfigWidget.BrushInfo;
        }

        private void layerControl_ClickedOnLayer(object sender, PaintDotNet.LayerEventArgs ce)
        {
            if (ce.Layer != ActiveLayer)
            {
                ActiveLayer = ce.Layer;
            }

            this.RelinquishFocusHandler(sender, EventArgs.Empty);
        }

        private void layerForm_NewLayerButtonClicked(object sender, System.EventArgs e)
        {
            try
            {
                AddNewLayerToDocument();
            }

            catch
            {
                Utility.ErrorBox(this, PdnResources.GetString("DocumentWorkspace.NewLayerButtonClicked.OutOfMemory"));
                return;
            }
        }

        public NewLayerHistoryAction AddNewLayerToDocument()
        {
            BitmapLayer newLayer = null;
            newLayer = new BitmapLayer(Document.Width, Document.Height);
            newLayer.Name = "Layer " + (Document.Layers.Count + 1).ToString();

            NewLayerHistoryAction ha = new NewLayerHistoryAction(
                PdnResources.GetString("DocumentWorkspace.AddNewLayerToDocument.NewLayerHistoryActionName"),
                PdnResources.GetImage("Icons.MenuLayersAddNewLayerIcon.bmp"), 
                this, 
                document.Layers.Count);

            document.Layers.Add(newLayer);
            History.PushNewAction(ha);
            return ha;
        }

        private void layerForm_DeleteLayerButtonClicked(object sender, System.EventArgs e)
        {
            if (Document.Layers.Count == 1)
            {
                Utility.ErrorBox(this, PdnResources.GetString("DocumentWorkspace.DeleteLayerButtonClicked.MustHaveOneLayer"));
            }
            else
            {
                if (DialogResult.Yes == Utility.AskYesNo(this, PdnResources.GetString("DocumentWorkspace.DeleteLayerButtonClicked.Confirmation")))
                {
                    DeselectAction action = new DeselectAction(this);
                    HistoryAction ha1 = action.PerformAction();
                    
                    HistoryAction ha2 = new DeleteLayerHistoryAction(string.Empty, null, this, ActiveLayer);
                    Document.Layers.Remove(ActiveLayer);

                    CompoundHistoryAction cha = new CompoundHistoryAction(
                        PdnResources.GetString("DocumentWorkspace.DeleteLayerButtonClicked.DeleteLayerHistoryActionName"),
                        PdnResources.GetImage("Icons.MenuLayersDeleteLayerIcon.bmp"), 
                        new HistoryAction[] { ha1, ha2 });

                    History.PushNewAction(cha);
                }
            }
        }

        private void layerForm_DuplicateLayerButtonClick(object sender, System.EventArgs e)
        {
            Layer newLayer = null;
            Utility.GCFullCollect();

            try
            {
                newLayer = (Layer)ActiveLayer.Clone();
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, PdnResources.GetString("DocumentWorkspace.DuplicateLayerButtonClicked.OutOfMemory"));
                return;
            }

            newLayer.IsBackground = false;
            int newIndex = 1 + Document.Layers.IndexOf(ActiveLayer);

            HistoryAction ha = new NewLayerHistoryAction(
                PdnResources.GetString("DocumentWorkspace.DuplicateLayerButtonClicked.DuplicateLayerHistoryActionName"),
                PdnResources.GetImage("Icons.MenuLayersDuplicateLayerIcon.bmp"), 
                this, 
                newIndex);

            Document.Layers.Insert(newIndex, newLayer);
            History.PushNewAction(ha);
            newLayer.Invalidate();
        }

        private void layerForm_MoveLayerUpButtonClicked(object sender, System.EventArgs e)
        {
            int index = Document.Layers.IndexOf(ActiveLayer);

            if (index == 0)
            {
                return;
            }

            SwapLayerHistoryAction slha = new SwapLayerHistoryAction(PdnResources.GetString("DocumentWorkspace.MoveLayerUpButtonClicked.MoveLayerUpHistoryActionName"),
                                                                     PdnResources.GetImage("Icons.MenuLayersMoveLayerUpIcon.bmp"),
                                                                     this, 
                                                                     index, 
                                                                     index - 1);

            HistoryAction ha = slha.PerformUndo();
            history.PushNewAction(ha);
        }

        private void layerForm_MoveLayerDownButtonClicked(object sender, System.EventArgs e)
        {
            int index = Document.Layers.IndexOf(ActiveLayer);
            
            if (index == Document.Layers.Count - 1)
            {
                return;
            }

            SwapLayerHistoryAction slha = new SwapLayerHistoryAction(PdnResources.GetString("DocumentWorkspace.MoveLayerDownButtonClicked.MoveLayerDownHistoryActionName"),
                                                                     PdnResources.GetImage("Icons.MenuLayersMoveLayerDownIcon.bmp"),
                                                                     this, 
                                                                     index, 
                                                                     index + 1);

            HistoryAction ha = slha.PerformUndo();
            history.PushNewAction(ha);

            this.ActiveLayer = (Layer)Document.Layers[index + 1];
        }

        private void shapeDrawTypeConfigWidget_ShapeDrawTypeChanged(object sender, System.EventArgs e)
        {
            if (Environment.ShapeDrawType != widgets.ShapeDrawTypeConfigWidget.ShapeDrawType)
            {
                Environment.ShapeDrawType = widgets.ShapeDrawTypeConfigWidget.ShapeDrawType;
            }
        }

        private void historyForm_ClearHistoryButtonClicked(object sender, System.EventArgs e)
        {
            if (DialogResult.Yes == Utility.AskYesNo(this, PdnResources.GetString("DocumentWorkspace.ClearHistoryButtonClicked.Confirmation")))
            {
                history.ClearAll();

                history.PushNewAction(new NullHistoryAction(
                    PdnResources.GetString("DocumentWorkspace.ClearHistoryButtonClicked.ClearHistoryActionName"),
                    PdnResources.GetImage("Icons.MenuLayersDeleteLayerIcon.bmp")));
            }
        }

        private void historyForm_UndoButtonClicked(object sender, System.EventArgs e)
        {
            if (History.UndoStack.Count > 0)
            {
                if (!(History.UndoStack[History.UndoStack.Count - 1] is NullHistoryAction))
                {
                    using (new WaitCursorChanger(this))
                    {
                        History.StepBackward();
                        Update();
                    }
                }

                Utility.GCFullCollect();
            }
        }

        private void historyForm_RedoButtonClicked(object sender, System.EventArgs e)
        {
            if (History.RedoStack.Count > 0)
            {
                if (!(History.RedoStack[History.RedoStack.Count - 1] is NullHistoryAction))
                {
                    using (new WaitCursorChanger(this))
                    {
                        History.StepForward();
                        Update();
                    }
                }

                Utility.GCFullCollect();
            }
        }

        private void workspaceOptionsConfigWidget_RulersEnabledChanged(object sender, System.EventArgs e)
        {
            documentView.RulersEnabled = workspaceOptionsConfigWidget.RulersEnabled;
        }

        private void historyForm_RewindButtonClicked(object sender, EventArgs e)
        {
            DateTime lastUpdate = DateTime.Now;

            while (History.UndoStack.Count > 1)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepBackward();

                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 500)
                    {
                        Update();
                        lastUpdate = DateTime.Now;
                    }
                }
            }

            Utility.GCFullCollect();
            document.Invalidate();
            Update();
        }

        private void historyForm_FastForwardButtonClicked(object sender, EventArgs e)
        {
            DateTime lastUpdate = DateTime.Now;

            while (History.RedoStack.Count > 0)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepForward();

                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 500)
                    {
                        Update();
                        lastUpdate = DateTime.Now;
                    }                
                }
            }

            Utility.GCFullCollect();
            document.Invalidate();
            Update();
        }

        private void layerForm_PropertiesButtonClick(object sender, EventArgs e)
        {
            using (Form lpd = ActiveLayer.CreateConfigDialog())
            {
                DialogResult result = Utility.ShowDialog(lpd, FindForm());
            }
        }

        private void Environment_FontInfoChanged(object sender, EventArgs e)
        {
            widgets.TextConfigWidget.FontInfo = Environment.FontInfo;
        }

        private void Environment_TextAlignmentChanged(object sender, EventArgs e)
        {
            widgets.TextConfigWidget.TextAlignment = Environment.TextAlignment;
        }

        private void textConfigWidget_TextAlignmentChanged(object sender, EventArgs e)
        {
            Environment.TextAlignment = widgets.TextConfigWidget.TextAlignment;
        }

        private void textConfigWidget_FontTextChanged(object sender, EventArgs e)
        {
            Environment.FontInfo = widgets.TextConfigWidget.FontInfo;
        }

        private void toolPulseTimer_Tick(object sender, EventArgs e)
        {
            if (ParentForm == null || ParentForm.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            if (Environment.Tool != null && Environment.Tool.Active)
            {
                Environment.Tool.PerformPulse();
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            zoomConfigWidget_ZoomBasisChanged(this, EventArgs.Empty);
            base.OnLayout (levent);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (ParentForm != null)
            {
                if (ParentForm.WindowState == FormWindowState.Minimized)
                {
                    toolPulseTimer.Enabled = false;
                }
                else
                {
                    toolPulseTimer.Enabled = true;
                    zoomConfigWidget_ZoomBasisChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler Scroll;
        protected virtual void OnScroll()
        {
            if (Scroll != null)
            {
                Scroll(this, EventArgs.Empty);
            }
        }

        private void documentView_Scroll(object sender, System.EventArgs e)
        {
            OnScroll();
        }

        public void ZoomIn()
        {
            documentView.ZoomIn();
        }

        public void ZoomOut()
        {
            documentView.ZoomOut();
        }

        public void ZoomToWindow()
        {
            documentView.ZoomToWindow();
        }

        public void ZoomToRectangle(Rectangle selectionBounds)
        {
            Point selectionCenter = new Point((selectionBounds.Left + selectionBounds.Right + 1) / 2,
                (selectionBounds.Top + selectionBounds.Bottom + 1) / 2);

            Point cornerPosition;

            ScaleFactor zoom = ScaleFactor.Min(documentView.ClientRectantangleMin.Width, selectionBounds.Width + 2,
                                               documentView.ClientRectantangleMin.Height, selectionBounds.Height + 2,
                                               ScaleFactor.MinValue);

            // Zoom out to fit the image
            documentView.ScaleFactor = zoom;

            cornerPosition = new Point(selectionCenter.X - (VisibleDocumentRectangle.Width / 2),
                selectionCenter.Y - (VisibleDocumentRectangle.Height / 2));

            documentView.DocumentScrollPosition = cornerPosition;
        }

        public void ZoomToSelection()
        {
            if (environment.Selection.IsEmpty) 
            {
                ZoomToWindow();
            } 
            else 
            {
                using (PdnRegion region = environment.Selection.CreateRegion())
                {
                    ZoomToRectangle(region.GetBoundsInt());
                }
            }
        }

        private uint ignore = 0; //to stop the feedback loop
        private void zoomConfigWidget_ZoomBasisChanged(object sender, EventArgs e)
        {
            if (ignore == 0) 
            {
                ++ignore;

                try
                {
                    switch (zoomConfigWidget.ZoomBasis) 
                    {
                        case ZoomBasis.Window:
                            ZoomToWindow();
                            // Enable PanelAutoScroll only long enough to recenter the view
                            documentView.PanelAutoScroll = true;
                            documentView.PanelAutoScroll = false;
                            // This would be unset by the scalefactor change.
                            zoomConfigWidget.ZoomBasis = ZoomBasis.Window;
                            break;

                        case ZoomBasis.Selection:
                            ZoomToSelection();
                            documentView.PanelAutoScroll = true;
                            zoomConfigWidget.ZoomBasis = ZoomBasis.Factor;
                            break;

                        case ZoomBasis.Factor:
                            documentView.PanelAutoScroll = true;
                            break;

                        default:
                            throw new InvalidEnumArgumentException("zoomConfigWidget.ZoomBasis was not a valid enumeration value");
                    }
                }

                finally
                {
                    --ignore;
                }
            }
        }

        private void zoomConfigWidget_ZoomScaleChanged(object sender, EventArgs e)
        {
            if (zoomConfigWidget.ZoomBasis == ZoomBasis.Factor) 
            {
                documentView.ScaleFactor = zoomConfigWidget.ScaleFactor;
            }
        }

        private void documentView_RulersEnabledChanged(object sender, EventArgs e)
        {
            workspaceOptionsConfigWidget.RulersEnabled = documentView.RulersEnabled;
            zoomConfigWidget_ZoomBasisChanged(this, EventArgs.Empty);
            UpdateRulerSelectionTinting();
        }

        private void documentView_DrawGridChanged(object sender, EventArgs e)
        {
            workspaceOptionsConfigWidget.DrawGrid = documentView.DrawGrid;
        }

        private void Environment_AntiAliasingChanged(object sender, EventArgs e)
        {
            drawModesConfigWidget.AntiAliasing = Environment.AntiAliasing;
        }

        private void zoomConfigWidget_ZoomIn(object sender, EventArgs e)
        {
            this.ZoomIn();
        }

        private void zoomConfigWidget_ZoomOut(object sender, EventArgs e)
        {
            this.ZoomOut();
        }

        private void workspaceOptionsConfigWidget_UnitsChanged(object sender, EventArgs e)
        {
            this.environment.Units = this.workspaceOptionsConfigWidget.Units;

            if (this.workspaceOptionsConfigWidget.Units != MeasurementUnit.Pixel)
            {
                Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, this.workspaceOptionsConfigWidget.Units.ToString());
            }
        }

        private void Environment_UnitsChanged(object sender, EventArgs e)
        {
            this.workspaceOptionsConfigWidget.Units = this.environment.Units;
            this.documentView.Units = this.environment.Units;
        }

        private void DocumentView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    this.documentView.ScaleFactor = this.documentView.ScaleFactor.GetNextLarger();
                }
                else if (e.Delta < 0)
                {
                    this.documentView.ScaleFactor = this.documentView.ScaleFactor.GetNextSmaller();
                }
            }
        }

        private void drawModesConfigWidget_AlphaBlendingChanged(object sender, EventArgs e)
        {
            if (Environment.AlphaBlending != widgets.DrawModesConfigWidget.AlphaBlending)
            {
                Environment.AlphaBlending = widgets.DrawModesConfigWidget.AlphaBlending;
            }
        }

        public event EventHandler ToolStatusChanged;
        private void OnToolStatusChanged()
        {
            if (ToolStatusChanged != null)
            {
                ToolStatusChanged(this, EventArgs.Empty);
            }
        }

        private void environment_ToolStatusChanged(object sender, EventArgs e)
        {
            OnToolStatusChanged();
        }
    }
}
