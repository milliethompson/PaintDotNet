using DotNetWidgets;
using PaintDotNet.Effects;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for DocumentWorkspace.
    /// </summary>
    public class DocumentWorkspace
        : UserControl
    {
        private Document document = null;
        private HistoryStack history;
        private Layer activeLayer;
        private DocumentEnvironment environment;
        private Type[] tools;
        private Type[] effects = null;
        private PenConfigWidget penConfigWidget;
        private BrushConfigWidget brushConfigWidget;
        private ShapeDrawTypeConfigWidget shapeDrawTypeConfigWidget;
        private DocumentView documentView;

        private EventHandler selectedPathChangingDelegate;
        private EventHandler selectedPathChangedDelegate;

        private EventHandler foreColorChangedDelegate;
        private EventHandler backColorChangedDelegate;
        private EventHandler shapeDrawTypeChangedDelegate;

        private IndexEventHandler layerRemovingDelegate;
        private IndexEventHandler layerRemovedDelegate;
        private IndexEventHandler layerInsertedDelegate;
        private PropertyEventHandler layerPropertyChangingDelegate;

        private FlowPanel topDock;

        private DocumentWidgets widgets;

        private MainToolBarForm mainToolBarForm;
        private LayerForm layerForm;
        private HistoryForm historyForm;
        private PaintDotNet.TextConfigWidget textConfigWidget;
        private PaintDotNet.WorkspaceOptionsConfigWidget workspaceOptionsConfigWidget;
        private System.Timers.Timer toolPulseTimer;
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
        public Document Document
        {
            get
            {
                return document;
            }
        }

        /// <summary>
        /// Sets the Document instance to be managed by DocumentWorkspace.
        /// Before the document is changed, a DocumentChanging event is raised.
        /// After the document is changed, a DocumentChanged event is raised.
        /// The DocumentView contained in our Workspace is also notified of the
        /// change to the Document instance.
        /// Since the DocumentWorkspace owns this object (it does not copy it),
        /// it was decided that a method instead of a property should be used
        /// for the 'set' behavior.
        /// </summary>
        /// <param name="document"></param>
        public void SetDocument(Document document)
        {
            Tool oldTool = Environment.Tool;

            Environment.SetTool(null);
            ActiveLayer = null;

            OnDocumentChanging();

            if (this.Document != null)
            {
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging -= this.layerPropertyChangingDelegate;
                }
            }

            Environment.PerformSelectedPathChanging();
            Environment.SelectedPath.Reset();       
            Environment.PerformSelectedPathChanged();

            if (this.document != null)
            {
                this.document.Layers.RemovingAt -= layerRemovingDelegate;
                this.document.Layers.RemovedAt -= layerRemovedDelegate;
                this.document.Layers.Inserted -= layerInsertedDelegate;
            }

            documentView.Document = document;
            this.document = document;

            OnDocumentChanged();

            if (this.Document != null)
            {
                foreach (Layer layer in this.Document.Layers)
                {
                    layer.PropertyChanging += this.layerPropertyChangingDelegate;
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
                this.document.Invalidate();
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
        public Layer ActiveLayer
        {
            get
            {
                return activeLayer;
            }

            set
            {
                OnLayerChanging();

                if (Environment.Tool != null)
                {
                    toolPulseTimer.Enabled = false;
                    Environment.Tool.PerformDeactivate();
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

                if (Environment.Tool != null)
                {
                    Environment.Tool.PerformActivate();
                    toolPulseTimer.Enabled = true;
                }

                // TODO: depending on type of layer, change which toolbar we're using, etc?
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

                return effects;
            }
        }

        [Browsable(false)]
        public Type[] Tools
        {
            get
            {
                return tools;
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
            Update();
            historyForm.Update();

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
            this.activeLayer = null;
            this.tools = null;
            this.history = new HistoryStack(this);

            // initialize!
            InitializeFloatingForms();
            InitializeTools();
            InitializeComponent();
            InitializeToolBars();
            InitializeEffects();

            this.historyForm.HistoryControl.HistoryStack = this.history;
            
            history.Changed += new EventHandler(HistoryChangedHandler);
           
            // set the workspace toggle buttons correctly
            this.workspaceOptionsConfigWidget.AntiAliasing = Environment.AntiAliasing;
            this.workspaceOptionsConfigWidget.RulersEnabled = documentView.RulersEnabled;

            // hook the DocumentView with its selectedPath ...
            this.documentView.SelectedPath = Environment.SelectedPath;

            // hook into Environment *Changed events
            foreColorChangedDelegate = new EventHandler(ForeColorChangedHandler);
            Environment.ForeColorChanged += foreColorChangedDelegate;
            mainToolBarForm.MainToolBar.ColorDisplay.UserForeColorChanged += foreColorChangedDelegate;

            backColorChangedDelegate = new EventHandler(BackColorChangedHandler);
            Environment.BackColorChanged += backColorChangedDelegate;
            shapeDrawTypeChangedDelegate = new EventHandler(ShapeDrawTypeChangedHandler);
            Environment.ShapeDrawTypeChanged += shapeDrawTypeChangedDelegate;
            mainToolBarForm.MainToolBar.ColorDisplay.UserBackColorChanged += backColorChangedDelegate;

            Environment.FontInfo = textConfigWidget.FontInfo;
            Environment.TextAlignment = textConfigWidget.TextAlignment;
            textConfigWidget.TextAlignmentChanged += new EventHandler(textConfigWidget_TextAlignmentChanged);
            textConfigWidget.FontTextChanged += new EventHandler(textConfigWidget_FontTextChanged);
            Environment.FontInfoChanged += new EventHandler(Environment_FontInfoChanged);
            Environment.TextAlignmentChanged += new EventHandler(Environment_TextAlignmentChanged);

            // hook into the SelectedPathChanged event ...
            selectedPathChangingDelegate = new EventHandler(SelectedPathChangingHandler);
            Environment.SelectedPathChanging += selectedPathChangingDelegate;
            selectedPathChangedDelegate = new EventHandler(SelectedPathChangedHandler);
            Environment.SelectedPathChanged += selectedPathChangedDelegate;

            // layer events
            layerRemovingDelegate = new IndexEventHandler(LayerRemovingHandler);
            layerRemovedDelegate = new IndexEventHandler(LayerRemovedHandler);
            layerInsertedDelegate = new IndexEventHandler(LayerInsertedHandler);
            layerPropertyChangingDelegate = new PropertyEventHandler(LayerPropertyChangingHandler);

            // init the Widgets container
            widgets = new DocumentWidgets(this);
            widgets.TopDock = this.topDock;
            widgets.PenConfigWidget = penConfigWidget;
            widgets.BrushConfigWidget = brushConfigWidget;
            widgets.ShapeDrawTypeConfigWidget = this.shapeDrawTypeConfigWidget;
            widgets.CommonActionsWidget = this.commonActionsWidget;
            widgets.TextConfigWidget = this.textConfigWidget;
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

        /// <summary>
        /// Whatever form owns this control should call this in its nLoad method
        /// </summary>
        public void OnLoad_ShowFloatingForms()
        {
            colorsForm.Show();
            mainToolBarForm.Show();
            layerForm.Show();
            historyForm.Show();
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
        /// Handles the ForeColorChanged event that is raised by the DocumentEnvironment.
        /// </summary>
        private void ForeColorChangedHandler(object sender, EventArgs e)
        {
            if (sender == environment)
            {
                widgets.ColorDisplayWidget.UserForeColor = Environment.ForeColor;
                widgets.ColorsForm.UserForeColor = Environment.ForeColor;
            }
            else
            if (sender == widgets.ColorDisplayWidget)
            {
                Environment.ForeColor = widgets.ColorDisplayWidget.UserForeColor;
            }
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
            else
            if (sender == widgets.ColorDisplayWidget)
            {
                Environment.BackColor = widgets.ColorDisplayWidget.UserBackColor;
            }
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
        }

        private void colorDisplay_BackColorClicked(object sender, System.EventArgs e)
        {
            widgets.ColorsForm.WhichUserColor = WhichUserColor.Background;
            widgets.ColorsForm.UserForeColor = Environment.ForeColor;
            widgets.ColorsForm.UserBackColor = Environment.BackColor;
            widgets.ColorsForm.Show();
        }

        private void LayerRemovingHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)Document.Layers[e.Index];
            layer.PropertyChanging -= layerPropertyChangingDelegate;
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {   // pick a new valid layer!
            ActiveLayer = (Layer)Document.Layers[Math.Min(e.Index, Document.Layers.Count - 1)];
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            Layer layer = (Layer)Document.Layers[e.Index];
            ActiveLayer = layer;
            layer.PropertyChanging += layerPropertyChangingDelegate;
        }

        private void LayerPropertyChangingHandler(object sender, PropertyEventArgs e)
        {
            LayerPropertyHistoryAction lpha = new LayerPropertyHistoryAction("Layer " + e.PropertyName, Utility.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.bmp"), (Layer)sender);
            History.PushNewAction(lpha);
        }

        /// <summary>
        /// This variable is used to accumulate an invalidation region. It is initialized
        /// upon responding to the SelectedPathChanging event that is raised by the
        /// DocumentEnvironment. Then, when the SelectedPathChanged event is raised, the
        /// full region that needs to be redrawn is accounted for.
        /// </summary>
        private PdnRegion selectionRedrawInterior;
        private PdnGraphicsPath selectionRedrawOutline;

        /// <summary>
        /// Handles the SelectedPathChanging event that is raised by the DocumentEnvironment.
        /// This method initializes the selectionRedrawInterior variable for later use in
        /// handling the SelectedPathChanged event inside the SelectedPathChangedHandler
        /// method.
        /// </summary>
        private void SelectedPathChangingHandler(object sender, EventArgs e)
        {   
            if (!Environment.IsSelectionEmpty)
            {
                selectionRedrawInterior = Environment.CreateSelectedRegion();
                selectionRedrawOutline = (PdnGraphicsPath)Environment.SelectedPath.Clone();
            }
            else
            {
                selectionRedrawInterior = new PdnRegion();
                selectionRedrawInterior.MakeEmpty();
                selectionRedrawOutline = new PdnGraphicsPath();
            }
        }

        /// <summary>
        /// Handles the SelectedPathChanged event that is raised by the DocumentEnvironment.
        /// This method notifiest the DocumentView of the new selection region, and
        /// finalizes the accumulated region stored in the selectionRedrawInterior variable.
        /// This accumulated redraw region is then used to Invalidate the Document and
        /// cause the correct regions to be erased and/or redrawn.
        /// We also go to extents to make sure that only the areas of the screen that
        /// have changed are redrawn, and that we don't just erase the old selection area
        /// and completely redraw the new one.
        /// </summary>
        private void SelectedPathChangedHandler(object sender, EventArgs e)
        {
            bool fullInvalidate = false;
            documentView.SelectedPath = Environment.SelectedPath;

            // if we're moving to a simpler selection region ...
            if (Environment.IsSelectionEmpty ||
                Environment.SelectedPath.PointCount < selectionRedrawOutline.PointCount)
            {   // then invalidate everything
                fullInvalidate = true;
            }
            else
            {   // otherwise, be intelligent about it
                PdnRegion unionMe = Environment.CreateSelectedRegion();
                PdnRegion excludeMe = (PdnRegion)unionMe.Clone();
                excludeMe.Intersect(selectionRedrawInterior);
                selectionRedrawInterior.Union(unionMe);
                selectionRedrawInterior.Exclude(excludeMe);
                unionMe.Dispose();
                excludeMe.Dispose();
            }

            if (Document != null)
            {
                using (PdnRegion simplified = Utility.SimplifyAndInflateRegion(selectionRedrawInterior, Utility.DefaultSimplificationFactor, 2))
                {
                    this.document.Invalidate(simplified);
                }

                if (fullInvalidate)
                {
                    this.document.Invalidate(this.VisibleDocumentRectangle);
                }

                Update();

                Rectangle[] rects = Utility.SimplifyTrace(selectionRedrawOutline);
                Utility.InflateRectanglesInPlace(rects, 2);

                using (PdnRegion simplified = Utility.RectanglesToRegion(rects))
                {
                    this.document.Invalidate(simplified);
                }

                Update();
            }

            selectionRedrawInterior.Dispose();
            selectionRedrawInterior = null;

            // set buttons on CommonActionsWidgets
            if (Environment.IsSelectionEmpty)
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

        private void InitializeComponent()
        {
            Utility.TraceMe("starting with news");
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DocumentWorkspace));
            this.documentView = new PaintDotNet.DocumentView();
            this.topDock = new PaintDotNet.FlowPanel();
            this.textConfigWidget = new PaintDotNet.TextConfigWidget();
            this.shapeDrawTypeConfigWidget = new PaintDotNet.ShapeDrawTypeConfigWidget();
            this.penConfigWidget = new PaintDotNet.PenConfigWidget();
            this.brushConfigWidget = new PaintDotNet.BrushConfigWidget();
            this.workspaceOptionsConfigWidget = new PaintDotNet.WorkspaceOptionsConfigWidget();
            this.commonActionsWidget = new PaintDotNet.CommonActionsWidget();
            this.toolPulseTimer = new System.Timers.Timer();
            this.topDock.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.toolPulseTimer)).BeginInit();
            this.SuspendLayout();
            Utility.TraceMe("done with news");
            // 
            // documentView
            // 
            this.documentView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.documentView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.documentView.Document = null;
            this.documentView.DocumentScrollPosition = ((System.Drawing.PointF)(resources.GetObject("documentView.DocumentScrollPosition")));
            this.documentView.EnableOutlineAnimation = true;
            this.documentView.Location = new System.Drawing.Point(0, 54);
            this.documentView.Name = "documentView";
            this.documentView.RulersEnabled = false;
            this.documentView.Size = new System.Drawing.Size(872, 586);
            this.documentView.TabIndex = 0;
            this.documentView.Scroll += new System.EventHandler(this.documentView_Scroll);
            this.documentView.DocumentMouseMove += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseMoveHandler);
            this.documentView.DocumentMouseDown += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseDownHandler);
            this.documentView.DocumentClick += new System.EventHandler(this.DocumentClick);
            this.documentView.DocumentMouseUp += new System.Windows.Forms.MouseEventHandler(this.DocumentMouseUpHandler);
            this.documentView.DocumentKeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DocumentKeyPress);
            // 
            // topDock
            // 
            this.topDock.Controls.Add(this.textConfigWidget);
            this.topDock.Controls.Add(this.shapeDrawTypeConfigWidget);
            this.topDock.Controls.Add(this.penConfigWidget);
            this.topDock.Controls.Add(this.brushConfigWidget);
            this.topDock.Controls.Add(this.workspaceOptionsConfigWidget);
            this.topDock.Controls.Add(this.commonActionsWidget);
            this.topDock.Dock = System.Windows.Forms.DockStyle.Top;
            this.topDock.Location = new System.Drawing.Point(0, 0);
            this.topDock.Name = "topDock";
            this.topDock.Size = new System.Drawing.Size(872, 54);
            this.topDock.TabIndex = 3;
            // 
            // textConfigWidget
            // 
            this.textConfigWidget.FontSize = 10F;
            this.textConfigWidget.FontStyle = System.Drawing.FontStyle.Regular;
            this.textConfigWidget.Location = new System.Drawing.Point(0, 27);
            this.textConfigWidget.Name = "textConfigWidget";
            this.textConfigWidget.Size = new System.Drawing.Size(397, 27);
            this.textConfigWidget.TabIndex = 2;
            this.textConfigWidget.TextAlignment = PaintDotNet.TextAlignment.Left;
            // 
            // shapeDrawTypeConfigWidget
            // 
            this.shapeDrawTypeConfigWidget.Location = new System.Drawing.Point(691, 0);
            this.shapeDrawTypeConfigWidget.Name = "shapeDrawTypeConfigWidget";
            this.shapeDrawTypeConfigWidget.ShapeDrawType = PaintDotNet.ShapeDrawType.Outline;
            this.shapeDrawTypeConfigWidget.Size = new System.Drawing.Size(78, 27);
            this.shapeDrawTypeConfigWidget.TabIndex = 1;
            this.shapeDrawTypeConfigWidget.ShapeDrawTypeChanged += new System.EventHandler(this.shapeDrawTypeConfigWidget_ShapeDrawTypeChanged);
            // 
            // penConfigWidget
            // 
            this.penConfigWidget.Location = new System.Drawing.Point(547, 0);
            this.penConfigWidget.Name = "penConfigWidget";
            this.penConfigWidget.Size = new System.Drawing.Size(144, 27);
            this.penConfigWidget.TabIndex = 0;
            this.penConfigWidget.PenChanged += new System.EventHandler(this.penConfigWidget_PenChanged);
            // 
            // brushConfigWidget
            // 
            this.brushConfigWidget.Location = new System.Drawing.Point(344, 0);
            this.brushConfigWidget.Name = "brushConfigWidget";
            this.brushConfigWidget.Size = new System.Drawing.Size(203, 27);
            this.brushConfigWidget.TabIndex = 0;
            this.brushConfigWidget.BrushChanged += new System.EventHandler(this.brushConfigWidget_BrushChanged);
            // 
            // workspaceOptionsConfigWidget
            // 
            this.workspaceOptionsConfigWidget.AntiAliasing = false;
            this.workspaceOptionsConfigWidget.Location = new System.Drawing.Point(286, 0);
            this.workspaceOptionsConfigWidget.Name = "workspaceOptionsConfigWidget";
            this.workspaceOptionsConfigWidget.RulersEnabled = false;
            this.workspaceOptionsConfigWidget.Size = new System.Drawing.Size(58, 27);
            this.workspaceOptionsConfigWidget.TabIndex = 3;
            this.workspaceOptionsConfigWidget.AntiAliasChanged += new System.EventHandler(this.workspaceOptionsConfigWidget_AntiAliasChanged);
            this.workspaceOptionsConfigWidget.RulersEnabledChanged += new System.EventHandler(this.workspaceOptionsConfigWidget_RulersEnabledChanged);
            // 
            // commonActionsWidget
            // 
            this.commonActionsWidget.Location = new System.Drawing.Point(0, 0);
            this.commonActionsWidget.Name = "commonActionsWidget";
            this.commonActionsWidget.Size = new System.Drawing.Size(286, 27);
            this.commonActionsWidget.TabIndex = 4;
            // 
            // toolPulseTimer
            // 
            this.toolPulseTimer.Interval = 25;
            this.toolPulseTimer.SynchronizingObject = this;
            this.toolPulseTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.toolPulseTimer_Elapsed);
            // 
            // DocumentWorkspace
            // 
            this.Controls.Add(this.documentView);
            this.Controls.Add(this.topDock);
            this.Name = "DocumentWorkspace";
            this.Size = new System.Drawing.Size(872, 640);
            this.topDock.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.toolPulseTimer)).EndInit();
            this.ResumeLayout(false);
        }

        // The Document* events are raised by the Document class, handled here,
        // and relayed as necessary. For instance, for the DocumentMouse* events, 
        // these are all relayed to the active tool.
        #region Document event handlers
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
        #endregion

        private void InitializeFloatingForms()
        {
            // MainToolBarForm
            mainToolBarForm = new MainToolBarForm();
            mainToolBarForm.MainToolBar.ColorDisplay.UserForeColorClick += new EventHandler(colorDisplay_ForeColorClicked);
            mainToolBarForm.MainToolBar.ColorDisplay.UserBackColorClick += new EventHandler(colorDisplay_BackColorClicked);

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

            // HistoryForm
            historyForm = new HistoryForm();
            historyForm.ClearHistoryButtonClicked += new EventHandler(historyForm_ClearHistoryButtonClicked);
            historyForm.RewindButtonClicked += new EventHandler(historyForm_RewindButtonClicked);
            historyForm.UndoButtonClicked += new EventHandler(historyForm_UndoButtonClicked);
            historyForm.RedoButtonClicked += new EventHandler(historyForm_RedoButtonClicked);
            historyForm.FastForwardButtonClicked += new EventHandler(historyForm_FastForwardButtonClicked);

            // ColorsForm
            colorsForm = new ColorsForm();
            colorsForm.UserForeColor = Environment.ForeColor;
            colorsForm.UserBackColor = Environment.BackColor;
            colorsForm.WhichUserColor = WhichUserColor.Foreground;
            colorsForm.UserForeColorChanged += new ColorEventHandler(colorsForm_UserForeColorChanged);
            colorsForm.UserBackColorChanged += new ColorEventHandler(colorsForm_UserBackColorChanged);
        }

        protected override void InitLayout()
        {
            base.InitLayout ();
            FindForm().AddOwnedForm(colorsForm);
            FindForm().AddOwnedForm(mainToolBarForm);
            FindForm().AddOwnedForm(layerForm);
            FindForm().AddOwnedForm(historyForm);
        }

        /// <summary>
        /// Creates an instance of every tool and adds each one to the tools array.
        /// </summary>
        protected void InitializeTools()
        {
            ArrayList tools = new ArrayList();

            // add all the tools
            tools.Add(typeof(RectangleSelectTool));
            tools.Add(typeof(MoveTool));
            tools.Add(typeof(LassoSelectTool));

            tools.Add(typeof(PanTool));

            tools.Add(typeof(PencilTool));
            tools.Add(typeof(PaintBrushTool));
            tools.Add(typeof(EraserTool));

            tools.Add(typeof(RectangleTool));
            tools.Add(typeof(EllipseTool));
            tools.Add(typeof(RoundedRectangleTool));
            tools.Add(typeof(LineTool));
            tools.Add(typeof(FreeformShapeTool));
            tools.Add(typeof(ColorPickerTool));
            tools.Add(typeof(PaintBucketTool));
            tools.Add(typeof(TextTool));

            // convert from ArrayList to a normal array
            this.tools = (Type[])tools.ToArray(typeof(Type));
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
            string homeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            try
            {
                string fileName = Path.Combine(homeDir, "PaintDotNet.Effects.dll");
                effectsArrays.Add(GetEffectsFromAssembly(Assembly.LoadFrom(fileName)));
            }

            catch (FileNotFoundException)
            {
                //Utility.ErrorBox(this, "PdnEffects.dll could not be found -- many Effects will not be available.");
            }

            try
            {
                string effectsDir = Path.Combine(homeDir, "Effects");
                DirectoryInfo dirInfo = new DirectoryInfo(effectsDir);

                if (dirInfo.Exists)
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

                        catch
                        {
                            Utility.ErrorBox(this, "There was an error loading " + fileName + ". It may be corrupt, or it may need to be recompiled.");
                        }

                        if (success)
                        {
                            effectsArrays.Add(pluginEffects);
                        }
                    }
                }
            }

            catch (System.IO.DirectoryNotFoundException)
            {
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
        /// Provides a way for you to perform an action by its class name. This 
        /// only works for actions that are registered in InitializeActions and 
        /// which don't take any extra context.
        /// </summary>
        /// <param name="actionName"></param>
        public void PerformAction(Type actionType)
        {
            Type oldToolType = Environment.GetToolType();
            Environment.SetTool(null);

            Update();

            using (new WaitCursorChanger(this))
            {
                ConstructorInfo ci = actionType.GetConstructor(new Type[] { typeof(DocumentWorkspace) });
                DocumentAction action = (DocumentAction)ci.Invoke(new object[] { this });
                PerformAction(action);
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
        
        private void mainToolBar_ToolClicked(object sender, ToolClickedEventArgs e)
        {
            Environment.SetTool(e.ToolType, this);
        }

        private void ToolChangedHandler(object sender, EventArgs e)
        {
            if (Environment.Tool != null)
            {
                documentView.Cursor = Environment.Tool.Cursor;
                Environment.Tool.CursorChanged += new EventHandler(ToolCursorChangedHandler);
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
            mainToolBarForm.MainToolBar.SetTools(tools, this);
            mainToolBarForm.MainToolBar.ToolClicked += new ToolClickedEventHandler(this.mainToolBar_ToolClicked);
            Environment.ToolChanging += new EventHandler(this.ToolChangingHandler);
            Environment.ToolChanged += new EventHandler(this.ToolChangedHandler);
        }

        private void workspaceOptionsConfigWidget_AntiAliasChanged(object sender, System.EventArgs e)
        {
            Environment.AntiAliasing = ((WorkspaceOptionsConfigWidget)sender).AntiAliasing;
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
            ActiveLayer = ce.Layer;
        }

        private void layerForm_NewLayerButtonClicked(object sender, System.EventArgs e)
        {
            try
            {
                AddNewLayerToDocument();
            }

            catch
            {
                Utility.ErrorBox(this, "Not enough memory to create the new layer.");
                return;
            }
        }

        public NewLayerHistoryAction AddNewLayerToDocument()
        {
            BitmapLayer newLayer = null;
            newLayer = new BitmapLayer(Document.Width, Document.Height);
            newLayer.Name = "Layer " + (Document.Layers.Count + 1).ToString();
            NewLayerHistoryAction ha = new NewLayerHistoryAction("New Layer", Utility.GetImageResource("Icons.MenuLayersAddNewLayerIcon.bmp"), this, newLayer);
            document.Layers.Add(newLayer);
            History.PushNewAction(ha);
            return ha;
        }

        private void layerForm_DeleteLayerButtonClicked(object sender, System.EventArgs e)
        {
            if (ActiveLayer.IsBackground)
            {
                Utility.ErrorBox(this, "The background layer may not be deleted.");
            }
            else if (Document.Layers.Count == 1)
            {
                Utility.ErrorBox(this, "There must be at least one layer in an image.");
            }
            else
            {
                if (DialogResult.Yes == Utility.AskYesNo(this, "Delete layer?"))
                {
                    HistoryAction ha = new DeleteLayerHistoryAction("Delete Layer", Utility.GetImageResource("Icons.MenuLayersDeleteLayerIcon.bmp"), this, ActiveLayer);
                    Document.Layers.Remove(ActiveLayer);
                    History.PushNewAction(ha);
                }
            }
        }

        private void layerForm_DuplicateLayerButtonClick(object sender, System.EventArgs e)
        {
            Layer newLayer = null;

            try
            {
                newLayer = (Layer)ActiveLayer.Clone();
            }

            catch (OutOfMemoryException)
            {
                Utility.ErrorBox(this, "Not enough memory to duplicate layer.");
                return;
            }

            newLayer.IsBackground = false;
            HistoryAction ha = new NewLayerHistoryAction("Duplicate Layer", Utility.GetImageResource("Icons.MenuLayersDuplicateLayerIcon.bmp"), this, newLayer);
            Document.Layers.Insert(1 + Document.Layers.IndexOf(ActiveLayer), newLayer);
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

            if (index == 1)
            {
                Utility.ErrorBox(this, "The Background layer may not be moved.");
                return;
            }

            Layer ourLayer = ActiveLayer;
            ActiveLayer = (Layer)Document.Layers[index - 1];
            HistoryAction delete = new DeleteLayerHistoryAction(null, null, this, ourLayer);
            Document.Layers.RemoveAt(index);
            HistoryAction add = new NewLayerHistoryAction(null, null, this, ourLayer);
            Document.Layers.Insert(index - 1, ourLayer);
            ourLayer.Invalidate();
            CompoundHistoryAction cha = new CompoundHistoryAction("Move Layer Up", Utility.GetImageResource("Icons.MenuLayersMoveLayerUpIcon.bmp"), new HistoryAction[] { delete, add });
            history.PushNewAction(cha);
        }

        private void layerForm_MoveLayerDownButtonClicked(object sender, System.EventArgs e)
        {
            int index = Document.Layers.IndexOf(ActiveLayer);
            
            if (index == Document.Layers.Count - 1)
            {
                return;
            }

            if (index == 0)
            {
                Utility.ErrorBox(this, "The Background layer may not be moved.");
                return;
            }

            Layer ourLayer = ActiveLayer;
            ActiveLayer = (Layer)Document.Layers[index + 1];
            HistoryAction delete = new DeleteLayerHistoryAction(null, null, this, ourLayer);
            Document.Layers.RemoveAt(index);
            HistoryAction add = new NewLayerHistoryAction(null, null, this, ourLayer);
            Document.Layers.Insert(index + 1, ourLayer);
            ourLayer.Invalidate();
            CompoundHistoryAction cha = new CompoundHistoryAction("Move Layer Down", Utility.GetImageResource("Icons.MenuLayersMoveLayerDownIcon.bmp"), new HistoryAction[] { delete, add });
            history.PushNewAction(cha);
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
            if (DialogResult.Yes == Utility.AskYesNo(this, "Clear history?"))
            {
                history.ClearAll();
                history.PushNewAction(new NullHistoryAction("Clear History", Utility.GetImageResource("Icons.MenuLayersDeleteLayerIcon.bmp")));
            }
        }

        private void historyForm_UndoButtonClicked(object sender, System.EventArgs e)
        {
            if (History.UndoStack.Count > 0)
            {
                if (!(History.UndoStack.Peek() is NullHistoryAction))
                {
                    using (new WaitCursorChanger(this))
                    {
                        History.StepBackward();
                        Update();
                    }
                }
            }
        }

        private void historyForm_RedoButtonClicked(object sender, System.EventArgs e)
        {
            if (History.RedoStack.Count > 0)
            {
                if (!(History.RedoStack.Peek() is NullHistoryAction))
                {
                    using (new WaitCursorChanger(this))
                    {
                        History.StepForward();
                        Update();
                    }
                }
            }
        }

        private void workspaceOptionsConfigWidget_RulersEnabledChanged(object sender, System.EventArgs e)
        {
            documentView.RulersEnabled = workspaceOptionsConfigWidget.RulersEnabled;
        }

        private void historyForm_RewindButtonClicked(object sender, EventArgs e)
        {
            while (History.UndoStack.Count > 1)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepBackward();
                    Update();
                }
            }
        }

        private void historyForm_FastForwardButtonClicked(object sender, EventArgs e)
        {
            while (History.RedoStack.Count > 0)
            {
                using (new WaitCursorChanger(this))
                {
                    History.StepForward();
                    Update();
                }            
            }
        }

        private void layerForm_PropertiesButtonClick(object sender, EventArgs e)
        {
            using (Form lpd = ActiveLayer.CreateConfigDialog())
            {
                DialogResult result = lpd.ShowDialog(FindForm());
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

        private void toolPulseTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
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

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            if (ParentForm == null)
            {
                return;
            }

            if (ParentForm.WindowState == FormWindowState.Minimized)
            {
                toolPulseTimer.Enabled = false;
            }
            else
            {
                toolPulseTimer.Enabled = true;
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
    }
}
