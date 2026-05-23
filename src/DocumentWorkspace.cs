/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Effects;
using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.Collections.Generic;
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
            private DocumentWorkspace parent;
            public DocumentWorkspace WorkspaceParent
            {
                get
                {
                    return this.parent;
                }

                set
                {
                    this.parent = value;
                }
            }

            protected override bool QueryNewZoomCenterPoint(ref PointF newCenterPt)
            {
                DocumentEnvironment env = parent.Environment;

                if (!env.Selection.IsEmpty) 
                {
                    using (PdnRegion selectedRegion = env.Selection.CreateRegion())
                    {
                        Rectangle selectionBounds = selectedRegion.GetBoundsInt();

                        PointF selectionCenter = new PointF((selectionBounds.Left + selectionBounds.Right) / 2,
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

        private DocumentWidgets widgets;

        private MainToolBarForm mainToolBarForm;
        private LayerForm layerForm;
        private HistoryForm historyForm;
        private System.Windows.Forms.Timer toolPulseTimer;
        private ColorsForm colorsForm;

        private const ToolStripGripStyle toolStripsGripStyle = ToolStripGripStyle.Hidden;
        private CommonActionsStrip commonActionsStrip;
        private ViewConfigStrip viewConfigStrip;
        private DrawConfigStrip drawConfigStrip;
        private TextConfigStrip textConfigStrip;

        private ToolStripContainer toolStripContainer;

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
        public RectangleF VisibleDocumentRectangleF
        {
            get
            {
                return documentView.VisibleDocumentRectangleF;
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
                return viewConfigStrip.ZoomBasis;
            }
            set 
            {
                viewConfigStrip.ZoomBasis = value;
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
            PropertyItem pi = Exif.CreateAscii(ExifTagID.Software, PdnInfo.GetProductName(false)); 
            document.Metadata.ReplaceExifValues(ExifTagID.Software, new PropertyItem[1] { pi });
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
            UI.SetControlRedraw(this, false);
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

            UI.SetControlRedraw(this, true);
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
            // enable/disable buttons on the CommonActionsStrip
            if (history.UndoStack.Count > 1)
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, true);
            }
            else
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Undo, false);
            }

            if (history.RedoStack.Count > 0)
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, true);
            }
            else
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Redo, false);
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
            this.documentView.WorkspaceParent = this;
            InitializeFloatingForms();
            InitializeToolBars();

            this.historyForm.HistoryControl.HistoryStack = this.history;
            
            history.Changed += new EventHandler(HistoryChangedHandler);
           
            // set the workspace toggle buttons correctly
            this.viewConfigStrip.Units = Environment.Units;
            this.viewConfigStrip.RulersEnabled = documentView.RulersEnabled;
            this.viewConfigStrip.DrawGrid = documentView.DrawGrid;
            this.drawConfigStrip.AntiAliasing = Environment.AntiAliasing;

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
            
            Environment.FontInfo = textConfigStrip.FontInfo;
            Environment.TextAlignment = textConfigStrip.TextAlignment;
            textConfigStrip.TextAlignmentChanged += new EventHandler(textConfigStrip_TextAlignmentChanged);
            textConfigStrip.FontTextChanged += new EventHandler(textConfigStrip_FontTextChanged);
            textConfigStrip.RelinquishFocus += new EventHandler(RelinquishFocusHandler2);
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
            widgets.ViewConfigStrip = viewConfigStrip;
            widgets.DrawConfigStrip = drawConfigStrip;
            widgets.CommonActionsStrip = this.commonActionsStrip;
            widgets.TextConfigStrip = this.textConfigStrip;
            widgets.MainToolBarForm = mainToolBarForm;
            widgets.LayerForm = layerForm;
            widgets.HistoryForm = historyForm;
            widgets.ColorsForm = colorsForm;

            //
            drawConfigStrip.PerformPenChanged();
            drawConfigStrip.PerformBrushChanged();
            drawConfigStrip.PerformShapeDrawTypeChanged();
            
            // Synchronize
            Environment.PerformAllChanged();

            // PaintBrush tool = the default
            Widgets.MainToolBar.SelectTool(typeof(PaintBrushTool));
        }

        public void SaveSettings()
        {
            /*
            Settings.CurrentUser.SetPoint(PdnSettings.CommonActionsLocation, this.commonActionsStrip.Location);
            Settings.CurrentUser.SetPoint(PdnSettings.ViewConfigLocation, this.viewConfigStrip.Location);
            Settings.CurrentUser.SetPoint(PdnSettings.DrawConfigLocation, this.drawConfigStrip.Location);
            Settings.CurrentUser.SetPoint(PdnSettings.TextConfigLocation, this.textConfigStrip.Location);
             * */
        }

        public void LoadSettings()
        {
            try
            {
                this.documentView.RulersEnabled = Settings.CurrentUser.GetBoolean(PdnSettings.Rulers, false);
                this.documentView.DrawGrid = Settings.CurrentUser.GetBoolean(PdnSettings.DrawGrid, false);
                this.environment.Units = (MeasurementUnit)Enum.Parse(typeof(MeasurementUnit), Settings.CurrentUser.GetString(PdnSettings.Units, MeasurementUnit.Pixel.ToString()), true);
                this.environment.AlphaBlending = Settings.CurrentUser.GetBoolean(PdnSettings.AlphaBlending, true);
                this.environment.AntiAliasing = Settings.CurrentUser.GetBoolean(PdnSettings.Antialiasing, true);

                /*
                this.commonActionsStrip.Location = Settings.CurrentUser.GetPoint(PdnSettings.CommonActionsLocation, this.commonActionsStrip.Location);
                this.viewConfigStrip.Location = Settings.CurrentUser.GetPoint(PdnSettings.ViewConfigLocation, this.viewConfigStrip.Location);
                this.drawConfigStrip.Location = Settings.CurrentUser.GetPoint(PdnSettings.DrawConfigLocation, this.drawConfigStrip.Location);
                this.textConfigStrip.Location = Settings.CurrentUser.GetPoint(PdnSettings.TextConfigLocation, this.textConfigStrip.Location);
                 * */
            }

            catch
            {
                try
                {
                    Settings.CurrentUser.Delete(
                        new string[] 
                        {    
                             PdnSettings.Rulers, 
                             PdnSettings.DrawGrid, 
                             PdnSettings.Units,
                             PdnSettings.Antialiasing,
                             PdnSettings.AlphaBlending

                            /*
                            PdnSettings.CommonActionsLocation,
                            PdnSettings.ViewConfigLocation,
                            PdnSettings.DrawConfigLocation,
                            PdnSettings.TextConfigLocation
                             * */
                        });
                }

                catch
                {
                }
            }
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
            if (widgets.DrawConfigStrip.ShapeDrawType != Environment.ShapeDrawType)
            {
                widgets.DrawConfigStrip.ShapeDrawType = Environment.ShapeDrawType;
            }
        }

        /// <summary>
        /// Keeps the Environment's alpha blending value and the corresponding widget synchronized
        /// </summary>
        private void AlphaBlendingChangedHandler(object sender, EventArgs e)
        {
            if (widgets.DrawConfigStrip.AlphaBlending != Environment.AlphaBlending)
            {
                widgets.DrawConfigStrip.AlphaBlending = Environment.AlphaBlending;
            }

            Settings.CurrentUser.SetBoolean(PdnSettings.AlphaBlending, Environment.AlphaBlending);
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
                PdnResources.GetImage("Icons.MenuLayersLayerPropertiesIcon.png"), 
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

            // set buttons on CommonActionsStrip
            if (Environment.Selection.IsEmpty)
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, false);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, false);
            }
            else
            {
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Cut, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Copy, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.Deselect, true);
                widgets.CommonActionsStrip.SetButtonEnabled(CommonAction.CropToSelection, true);
            }
        }
        
        private void ZoomChangedHandler(object sender, EventArgs e)
        {
            ScaleFactor sf = documentView.ScaleFactor;
            viewConfigStrip.SuspendEvents();
            viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
            viewConfigStrip.ScaleFactor = sf;
            viewConfigStrip.ResumeEvents();
        }

        private void InitializeComponent()
        {
            this.documentView = new PaintDotNet.DocumentWorkspace.OurDocumentView();
            this.toolStripContainer = new ToolStripContainer();
            this.commonActionsStrip = new CommonActionsStrip();
            this.viewConfigStrip = new ViewConfigStrip();
            this.drawConfigStrip = new DrawConfigStrip();
            this.textConfigStrip = new TextConfigStrip();
            this.toolPulseTimer = new System.Windows.Forms.Timer();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
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
            this.documentView.Scroll += new System.Windows.Forms.ScrollEventHandler(this.documentView_Scroll);
            this.documentView.DrawGridChanged += new System.EventHandler(this.documentView_DrawGridChanged);
            this.documentView.DocumentClick += new System.EventHandler(this.DocumentClick);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseUpHandler);
            this.documentView.DocumentKeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DocumentKeyPress);
            this.documentView.DocumentKeyUp += new KeyEventHandler(DocumenKeyUp);
            this.documentView.DocumentKeyDown += new KeyEventHandler(DocumentKeyDown);
            this.documentView.MouseWheel += new MouseEventHandler(DocumentView_MouseWheel);
            //
            // toolStripContainer
            //
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.TabIndex = 0;
            this.toolStripContainer.TabStop = false;
            this.toolStripContainer.BottomToolStripPanelVisible = false;
            this.toolStripContainer.LeftToolStripPanelVisible = false;
            this.toolStripContainer.RightToolStripPanelVisible = false;
            //
            // toolStripContainer.ContentPanel
            //
            this.toolStripContainer.ContentPanel.Controls.Add(this.documentView);
            //
            // toolStripContainer.ToolStripPanel
            //
            LoadSettings();
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.viewConfigStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.commonActionsStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.textConfigStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.drawConfigStrip);
            //
            // commonActionsStrip
            //
            this.commonActionsStrip.Name = "commonActionsStrip";
            this.commonActionsStrip.TabIndex = 0;
            this.commonActionsStrip.Dock = DockStyle.None;
            this.commonActionsStrip.GripStyle = toolStripsGripStyle;
            this.commonActionsStrip.RelinquishFocusRequest += new EventHandler(OnToolStripRelinquishFocusRequest);
            this.commonActionsStrip.MouseWheel += new MouseEventHandler(OnToolStripMouseWheel);
            //
            // viewConfigStrip
            //
            this.viewConfigStrip.Name = "viewConfigStrip";
            this.viewConfigStrip.ZoomBasis = PaintDotNet.ZoomBasis.Window;
            this.viewConfigStrip.TabStop = false;
            this.viewConfigStrip.DrawGrid = false;
            this.viewConfigStrip.DrawGridChanged += new System.EventHandler(this.viewConfigStrip_DrawGridChanged);
            this.viewConfigStrip.RulersEnabledChanged += new System.EventHandler(this.viewConfigStrip_RulersEnabledChanged);
            this.viewConfigStrip.ZoomBasisChanged += new System.EventHandler(this.viewConfigStrip_ZoomBasisChanged);
            this.viewConfigStrip.ZoomScaleChanged += new System.EventHandler(this.viewConfigStrip_ZoomScaleChanged);
            this.viewConfigStrip.ZoomIn += new EventHandler(viewConfigStrip_ZoomIn);
            this.viewConfigStrip.ZoomOut += new EventHandler(viewConfigStrip_ZoomOut);
            this.viewConfigStrip.UnitsChanged += new EventHandler(viewConfigStrip_UnitsChanged);
            this.viewConfigStrip.TabIndex = 1;
            this.viewConfigStrip.Dock = DockStyle.None;
            this.viewConfigStrip.GripStyle = toolStripsGripStyle;
            this.viewConfigStrip.RelinquishFocusRequest += new EventHandler(OnToolStripRelinquishFocusRequest);
            this.viewConfigStrip.MouseWheel += new MouseEventHandler(OnToolStripMouseWheel);
            //
            // drawConfigStrip
            //
            this.drawConfigStrip.Name = "drawConfigStrip";
            this.drawConfigStrip.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.drawConfigStrip.BrushChanged += new System.EventHandler(this.drawConfigStrip_BrushChanged);
            this.drawConfigStrip.ShapeDrawTypeChanged += new System.EventHandler(this.drawConfigStrip_ShapeDrawTypeChanged);
            this.drawConfigStrip.PenChanged += new System.EventHandler(this.drawConfigStrip_PenChanged);
            this.drawConfigStrip.AlphaBlendingChanged += new EventHandler(drawConfigStrip_AlphaBlendingChanged);
            this.drawConfigStrip.AntiAliasingChanged += new System.EventHandler(this.drawConfigStrip_AntiAliasingChanged);
            this.drawConfigStrip.TabIndex = 2;
            this.drawConfigStrip.Dock = DockStyle.None;
            this.drawConfigStrip.GripStyle = toolStripsGripStyle;
            this.drawConfigStrip.RelinquishFocusRequest += new EventHandler(OnToolStripRelinquishFocusRequest);
            this.drawConfigStrip.MouseWheel += new MouseEventHandler(OnToolStripMouseWheel);
            // 
            // textConfigStrip
            // 
            this.textConfigStrip.FontSize = 12F;
            this.textConfigStrip.FontStyle = System.Drawing.FontStyle.Regular;
            this.textConfigStrip.Name = "textConfigStrip";
            this.textConfigStrip.TabIndex = 3;
            this.textConfigStrip.TextAlignment = PaintDotNet.TextAlignment.Left;
            this.textConfigStrip.Dock = DockStyle.None;
            this.textConfigStrip.GripStyle = toolStripsGripStyle;
            this.textConfigStrip.RelinquishFocusRequest += new EventHandler(OnToolStripRelinquishFocusRequest);
            this.textConfigStrip.MouseWheel += new MouseEventHandler(OnToolStripMouseWheel);
            // 
            // toolPulseTimer
            // 
            this.toolPulseTimer.Interval = 16;
            this.toolPulseTimer.Tick += new EventHandler(this.toolPulseTimer_Tick);
            // 
            // DocumentWorkspace
            // 
            this.Controls.Add(this.toolStripContainer);
            this.Name = "DocumentWorkspace";
            this.Size = new System.Drawing.Size(872, 640);
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.ContentPanel.PerformLayout();
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.ResumeLayout(false);
        }

        void OnToolStripMouseWheel(object sender, MouseEventArgs e)
        {
            this.documentView.PerformMouseWheel(e);
            DocumentView_MouseWheel(sender, e);
        }

        void OnToolStripRelinquishFocusRequest(object sender, EventArgs e)
        {
            this.documentView.Focus();
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
            mainToolBarForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(OnToolFormProcessCmdKeyEvent);

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
            layerForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(OnToolFormProcessCmdKeyEvent);
            
            // HistoryForm
            historyForm = new HistoryForm();
            historyForm.ClearHistoryButtonClicked += new EventHandler(historyForm_ClearHistoryButtonClicked);
            historyForm.RewindButtonClicked += new EventHandler(historyForm_RewindButtonClicked);
            historyForm.UndoButtonClicked += new EventHandler(historyForm_UndoButtonClicked);
            historyForm.RedoButtonClicked += new EventHandler(historyForm_RedoButtonClicked);
            historyForm.FastForwardButtonClicked += new EventHandler(historyForm_FastForwardButtonClicked);
            historyForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            historyForm.AttachControl = this.DocumentView;
            historyForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(OnToolFormProcessCmdKeyEvent);

            // ColorsForm
            colorsForm = new ColorsForm();
            colorsForm.UserForeColor = Environment.ForeColor;
            colorsForm.UserBackColor = Environment.BackColor;
            colorsForm.WhichUserColor = WhichUserColor.Foreground;
            colorsForm.UserForeColorChanged += new ColorEventHandler(colorsForm_UserForeColorChanged);
            colorsForm.UserBackColorChanged += new ColorEventHandler(colorsForm_UserBackColorChanged);
            colorsForm.RelinquishFocus += new EventHandler(RelinquishFocusHandler);
            colorsForm.AttachControl = this.DocumentView;
            colorsForm.ProcessCmdKeyEvent += new CmdKeysEventHandler(OnToolFormProcessCmdKeyEvent);
        }

        public event CmdKeysEventHandler ProcessCmdKeyEvent;

        bool OnToolFormProcessCmdKeyEvent(object sender, ref Message msg, Keys keyData)
        {
            if (ProcessCmdKeyEvent != null)
            {
                return ProcessCmdKeyEvent(sender, ref msg, keyData);
            }
            else
            {
                return false;
            }
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
            List<Type> effects = new List<Type>();

            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(Effect)) && !type.IsAbstract)
                {
                    effects.Add(type);
                }
            }

            effects.Sort(delegate(Type x, Type y) { return string.Compare(x.Name, y.Name); });

            return effects.ToArray();
        }

        private void InitializeEffects()
        {
            List<Type[]> effectsArrays = new List<Type[]>();
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

                    catch (Exception ex)
                    {
                        //string errorFormat = PdnResources.GetString("DocumentWorkspace.InitializeEffects.DllLoadFailed.Format");
                        //string errorText = string.Format(errorFormat, fileName);
                        //Utility.ErrorBox(this, errorText);
                        Tracing.Ping("Exception while loading " + fileName + ": " + ex.ToString());
                    }

                    if (success)
                    {
                        effectsArrays.Add(pluginEffects);
                    }
                }
            }

            effectsArrays.Add(GetEffectsFromAssembly(Assembly.GetExecutingAssembly()));

            // collate List<T> of arrays into one big array
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

        private void viewConfigStrip_DrawGridChanged(object sender, EventArgs e)
        {
            documentView.DrawGrid = ((ViewConfigStrip)sender).DrawGrid;
        }

        private void drawConfigStrip_AntiAliasingChanged(object sender, System.EventArgs e)
        {
            Environment.AntiAliasing = ((DrawConfigStrip)sender).AntiAliasing;
        }

        private void drawConfigStrip_PenChanged(object sender, System.EventArgs e)
        {
            Environment.PenInfo = drawConfigStrip.PenInfo;
        }

        private void drawConfigStrip_BrushChanged(object sender, System.EventArgs e)
        {
            Environment.BrushInfo = drawConfigStrip.BrushInfo;
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
            string newLayerNameFormat = PdnResources.GetString("DocumentWorkspace.NewLayerName.Format");
            newLayer.Name = string.Format(newLayerNameFormat, (1 + Document.Layers.Count).ToString());

            NewLayerHistoryAction ha = new NewLayerHistoryAction(
                PdnResources.GetString("DocumentWorkspace.AddNewLayerToDocument.NewLayerHistoryActionName"),
                PdnResources.GetImage("Icons.MenuLayersAddNewLayerIcon.png"), 
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
                        PdnResources.GetImage("Icons.MenuLayersDeleteLayerIcon.png"), 
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
                PdnResources.GetImage("Icons.MenuLayersDuplicateLayerIcon.png"), 
                this, 
                newIndex);

            Document.Layers.Insert(newIndex, newLayer);
            History.PushNewAction(ha);
            newLayer.Invalidate();
        }

        private void layerForm_MoveLayerUpButtonClicked(object sender, System.EventArgs e)
        {
            int index = Document.Layers.IndexOf(ActiveLayer);

            if (index == Document.Layers.Count - 1)
            {
                return;
            }

            SwapLayerHistoryAction slha = new SwapLayerHistoryAction(PdnResources.GetString("DocumentWorkspace.MoveLayerUpButtonClicked.MoveLayerUpHistoryActionName"),
                                                                     PdnResources.GetImage("Icons.MenuLayersMoveLayerUpIcon.png"),
                                                                     this,
                                                                     index,
                                                                     index + 1);

            HistoryAction ha = slha.PerformUndo();
            history.PushNewAction(ha);

            this.ActiveLayer = (Layer)Document.Layers[index + 1]; 
        }

        private void layerForm_MoveLayerDownButtonClicked(object sender, System.EventArgs e)
        {
            int index = Document.Layers.IndexOf(ActiveLayer);

            if (index == 0)
            {
                return;
            }

            SwapLayerHistoryAction slha = new SwapLayerHistoryAction(PdnResources.GetString("DocumentWorkspace.MoveLayerDownButtonClicked.MoveLayerDownHistoryActionName"),
                                                                     PdnResources.GetImage("Icons.MenuLayersMoveLayerDownIcon.png"),
                                                                     this,
                                                                     index,
                                                                     index - 1);

            HistoryAction ha = slha.PerformUndo();
            history.PushNewAction(ha);
        }

        private void drawConfigStrip_ShapeDrawTypeChanged(object sender, System.EventArgs e)
        {
            if (Environment.ShapeDrawType != widgets.DrawConfigStrip.ShapeDrawType)
            {
                Environment.ShapeDrawType = widgets.DrawConfigStrip.ShapeDrawType;
            }
        }

        private void historyForm_ClearHistoryButtonClicked(object sender, System.EventArgs e)
        {
            if (DialogResult.Yes == Utility.AskYesNo(this, PdnResources.GetString("DocumentWorkspace.ClearHistoryButtonClicked.Confirmation")))
            {
                history.ClearAll();

                history.PushNewAction(new NullHistoryAction(
                    PdnResources.GetString("DocumentWorkspace.ClearHistoryButtonClicked.ClearHistoryActionName"),
                    PdnResources.GetImage("Icons.MenuLayersDeleteLayerIcon.png")));
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

        private void viewConfigStrip_RulersEnabledChanged(object sender, System.EventArgs e)
        {
            documentView.RulersEnabled = viewConfigStrip.RulersEnabled;
        }

        private void historyForm_RewindButtonClicked(object sender, EventArgs e)
        {
            DateTime lastUpdate = DateTime.Now;

            History.BeginStepGroup();

            while (History.UndoStack.Count > 1)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepBackward();

                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 500)
                    {
                        History.EndStepGroup();
                        Update();
                        lastUpdate = DateTime.Now;
                        History.BeginStepGroup();
                    }
                }
            }

            History.EndStepGroup();

            Utility.GCFullCollect();
            document.Invalidate();
            Update();
        }

        private void historyForm_FastForwardButtonClicked(object sender, EventArgs e)
        {
            DateTime lastUpdate = DateTime.Now;

            History.BeginStepGroup();

            while (History.RedoStack.Count > 0)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepForward();

                    if ((DateTime.Now - lastUpdate).TotalMilliseconds >= 500)
                    {
                        History.EndStepGroup();
                        Update();
                        lastUpdate = DateTime.Now;
                        History.BeginStepGroup();
                    }                
                }
            }

            History.EndStepGroup();

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
            widgets.TextConfigStrip.FontInfo = Environment.FontInfo;
        }

        private void Environment_TextAlignmentChanged(object sender, EventArgs e)
        {
            widgets.TextConfigStrip.TextAlignment = Environment.TextAlignment;
        }

        private void textConfigStrip_TextAlignmentChanged(object sender, EventArgs e)
        {
            Environment.TextAlignment = widgets.TextConfigStrip.TextAlignment;
        }

        private void textConfigStrip_FontTextChanged(object sender, EventArgs e)
        {
            Environment.FontInfo = widgets.TextConfigStrip.FontInfo;
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
            viewConfigStrip_ZoomBasisChanged(this, EventArgs.Empty);
            base.OnLayout(levent);
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
                    viewConfigStrip_ZoomBasisChanged(this, EventArgs.Empty);
                }
            }
        }

        private void documentView_Scroll(object sender, System.Windows.Forms.ScrollEventArgs e)
        {
            OnScroll(e);
        }

        public void ZoomIn(double factor)
        {
            this.viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
            this.documentView.ZoomIn(factor);
        }

        public void ZoomIn()
        {
            this.viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
            this.documentView.ZoomIn();
        }

        public void ZoomOut(double factor)
        {
            this.viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
            this.documentView.ZoomOut(factor);
        }

        public void ZoomOut()
        {
            this.viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
            this.documentView.ZoomOut();
        }

        public void ZoomToWindow()
        {
            this.viewConfigStrip.ZoomBasis = ZoomBasis.Window;
            this.documentView.ZoomToWindow();
        }

        public void ZoomToRectangle(Rectangle selectionBounds)
        {
            PointF selectionCenter = new PointF((selectionBounds.Left + selectionBounds.Right + 1) / 2,
                (selectionBounds.Top + selectionBounds.Bottom + 1) / 2);

            PointF cornerPosition;

            ScaleFactor zoom = ScaleFactor.Min(documentView.ClientRectantangleMin.Width, selectionBounds.Width + 2,
                                               documentView.ClientRectantangleMin.Height, selectionBounds.Height + 2,
                                               ScaleFactor.MinValue);

            // Zoom out to fit the image
            documentView.ScaleFactor = zoom;

            cornerPosition = new PointF(selectionCenter.X - (VisibleDocumentRectangleF.Width / 2),
                selectionCenter.Y - (VisibleDocumentRectangleF.Height / 2));

            documentView.DocumentScrollPositionF = cornerPosition;
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
        private void viewConfigStrip_ZoomBasisChanged(object sender, EventArgs e)
        {
            if (ignore == 0) 
            {
                ++ignore;

                try
                {
                    switch (viewConfigStrip.ZoomBasis) 
                    {
                        case ZoomBasis.Window:
                            this.viewConfigStrip.BeginZoomChanges();
                            ZoomToWindow();
                            // Enable PanelAutoScroll only long enough to recenter the view
                            this.documentView.PanelAutoScroll = true;
                            this.documentView.PanelAutoScroll = false;
                            this.viewConfigStrip.EndZoomChanges();
                            // This would be unset by the scalefactor change.
                            this.viewConfigStrip.ZoomBasis = ZoomBasis.Window;
                            break;

                        case ZoomBasis.Selection:
                            ZoomToSelection();
                            this.documentView.PanelAutoScroll = true;
                            this.viewConfigStrip.ZoomBasis = ZoomBasis.Factor;
                            break;

                        case ZoomBasis.Factor:
                            documentView.PanelAutoScroll = true;
                            break;

                        default:
                            throw new InvalidEnumArgumentException("viewConfigStrip.ZoomBasis was not a valid enumeration value");
                    }
                }

                finally
                {
                    --ignore;
                }
            }
        }

        private void viewConfigStrip_ZoomScaleChanged(object sender, EventArgs e)
        {
            if (viewConfigStrip.ZoomBasis == ZoomBasis.Factor) 
            {
                documentView.ScaleFactor = viewConfigStrip.ScaleFactor;
            }
        }

        private void documentView_RulersEnabledChanged(object sender, EventArgs e)
        {
            viewConfigStrip.RulersEnabled = documentView.RulersEnabled;
            viewConfigStrip_ZoomBasisChanged(this, EventArgs.Empty);
            UpdateRulerSelectionTinting();

            Settings.CurrentUser.SetBoolean(PdnSettings.Rulers, this.documentView.RulersEnabled);
        }

        private void documentView_DrawGridChanged(object sender, EventArgs e)
        {
            viewConfigStrip.DrawGrid = documentView.DrawGrid;
            Settings.CurrentUser.SetBoolean(PdnSettings.DrawGrid, this.documentView.DrawGrid);
        }

        private void Environment_AntiAliasingChanged(object sender, EventArgs e)
        {
            drawConfigStrip.AntiAliasing = Environment.AntiAliasing;
            Settings.CurrentUser.SetBoolean(PdnSettings.Antialiasing, Environment.AntiAliasing);
        }

        private void viewConfigStrip_ZoomIn(object sender, EventArgs e)
        {
            this.ZoomIn();
        }

        private void viewConfigStrip_ZoomOut(object sender, EventArgs e)
        {
            this.ZoomOut();
        }

        private void viewConfigStrip_UnitsChanged(object sender, EventArgs e)
        {
            this.environment.Units = this.viewConfigStrip.Units;

            if (this.viewConfigStrip.Units != MeasurementUnit.Pixel)
            {
                Settings.CurrentUser.SetString(PdnSettings.LastNonPixelUnits, this.viewConfigStrip.Units.ToString());
            }
        }

        private void Environment_UnitsChanged(object sender, EventArgs e)
        {
            this.viewConfigStrip.Units = this.environment.Units;
            this.documentView.Units = this.environment.Units;
            Settings.CurrentUser.SetString(PdnSettings.Units, this.environment.Units.ToString());
        }

        private void DocumentView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Control.ModifierKeys == Keys.Control)
            {
                double mouseDelta = (double)e.Delta / 120.0f;
                Rectangle visibleDocBoundsStart = this.documentView.VisibleDocumentBounds;

                Point mouseDocPt = this.documentView.MouseToDocument(sender, new Point(e.X, e.Y));
                RectangleF visibleDocDocRect1 = this.documentView.VisibleDocumentRectangleF;
                PointF mouseNPt = new PointF(
                    (mouseDocPt.X - visibleDocDocRect1.X) / visibleDocDocRect1.Width,
                    (mouseDocPt.Y - visibleDocDocRect1.Y) / visibleDocDocRect1.Height);

                const double factor = 1.12;
                double mouseFactor = Math.Pow(factor, Math.Abs(mouseDelta));

                if (e.Delta > 0)
                {
                    this.ZoomIn(mouseFactor);
                }
                else if (e.Delta < 0)
                {
                    this.ZoomOut(mouseFactor);
                }

                RectangleF visibleDocDocRect2 = this.documentView.VisibleDocumentRectangleF;
                PointF scrollPt2 = new PointF(
                    mouseDocPt.X - visibleDocDocRect2.Width * mouseNPt.X,
                    mouseDocPt.Y - visibleDocDocRect2.Height * mouseNPt.Y);

                this.documentView.DocumentScrollPositionF = scrollPt2;

                Rectangle visibleDocBoundsEnd = this.documentView.VisibleDocumentBounds;

                if (e.Delta < 0 && visibleDocBoundsEnd != visibleDocBoundsStart)
                {
                    // Make sure the screen updates, otherwise it can get a little funky looking
                    this.documentView.Update();
                }
            }
        }

        private void drawConfigStrip_AlphaBlendingChanged(object sender, EventArgs e)
        {
            if (Environment.AlphaBlending != widgets.DrawConfigStrip.AlphaBlending)
            {
                Environment.AlphaBlending = widgets.DrawConfigStrip.AlphaBlending;
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
