/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates rendering the document by itself, including rulers and
    /// scrollbar decorators. It also raises events for mouse movement that
    /// are properly translated to (x,y) pixel coordinates within the document
    /// (DocumentMouse* events).
    /// </summary>
    public class DocumentView
        : UserControl2,
          IInkHooks
    {
        //rulers really are on by default, so 'true' was set to show this.
        private bool rulersEnabled = true;

        private bool inkAvailable = true;
        private int refreshSuspended = 0;

        private Document document;
        private Surface renderSurface;
        private PaintDotNet.Ruler leftRuler;
        private PaintDotNet.PanelEx panel;
        private PaintDotNet.Ruler topRuler;
        private InvalidateEventHandler documentInvalidatedDelegate;
        private EventHandler documentMetaDataChangedDelegate;
        private PaintDotNet.SurfaceBox surfaceBox;
        private System.ComponentModel.IContainer components = null;
        private ControlShadow controlShadow;
        private bool freeRenderSurface = true;

        public event EventHandler Scroll;

        Graphics IInkHooks.CreateGraphics()
        {
            return this.CreateGraphics();
        }

        public SurfaceBoxRendererList Renderers
        {
            get
            {
                return this.surfaceBox.Renderers;
            }
        }

        public void IncrementJustPaintWhite()
        {
            this.surfaceBox.IncrementJustPaintWhite();
        }

        /// <summary>
        /// You may use this to optimize memory usage in some cases where you already have
        /// a Surface allocated that is the same size as the Document. You may only set
        /// this before the control is shown, and before the Document property is set.
        /// </summary>
        /// <param name="newRenderSurface"></param>
        public void SetRenderSurface(Surface newRenderSurface)
        {
            if (document != null)
            {
                if (document.Size != newRenderSurface.Size)
                {
                    throw new ArgumentException("Document != null, and newRenderSurface.Size != Document.Size");
                }
            }

            if (this.renderSurface != null)
            {
                this.renderSurface.Dispose();
                this.renderSurface = null;
            }

            this.renderSurface = newRenderSurface;
            this.freeRenderSurface = false;
        }

        public MeasurementUnit Units
        {
            get
            {
                return leftRuler.MeasurementUnit;
            }

            set
            {
                leftRuler.MeasurementUnit = value;
                topRuler.MeasurementUnit = value;
                DocumentMetaDataChangedHandler(this, EventArgs.Empty);
            }
        } 

        private void InitRenderSurface()
        {
            if (this.renderSurface == null && Document != null)
            {
                this.renderSurface = new Surface(Document.Size);
                this.freeRenderSurface = true;
            }
        }

        public bool DrawGrid 
        {
            get 
            {
                return surfaceBox.DrawGrid;
            }

            set 
            {
                if (surfaceBox.DrawGrid != value) 
                {
                    surfaceBox.DrawGrid = value;
                    OnDrawGridChanged();
                }
            }
        }
    
        [Browsable(false)]
        public override bool Focused
        {
            get
            {
                return base.Focused || panel.Focused || surfaceBox.Focused || controlShadow.Focused || leftRuler.Focused || topRuler.Focused;
            }
        }

        public BorderStyle BorderStyle
        {
            get
            {
                return this.panel.BorderStyle;
            }

            set
            {
                this.panel.BorderStyle = value;
            }
        }

        /// <summary>
        /// Initializes an instance of the DocumentView class.
        /// </summary>
        public DocumentView()
        {
            InitializeComponent();
            document = null;
            renderSurface = null;
            documentInvalidatedDelegate = new InvalidateEventHandler(DocumentInvalidatedHandler);
            documentMetaDataChangedDelegate = new EventHandler(DocumentMetaDataChangedHandler);

            controlShadow = new ControlShadow();
            controlShadow.OccludingControl = surfaceBox;
            panel.Controls.Add(controlShadow);
            panel.Controls.SetChildIndex(controlShadow, panel.Controls.Count - 1);

            //this.surfaceBox.Renderers.Invalidated += new InvalidateEventHandler(Renderers_Invalidated);
        }

        private bool hookedMouseEvents = false;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);

            InitRenderSurface();
            inkAvailable = Ink.IsAvailable();

            // Sometimes OnLoad() gets called *twice* for some reason.
            // See bug #1415 for the symptoms.
            if (!this.hookedMouseEvents)
            {
                this.hookedMouseEvents = true;
                foreach (Control c in Controls)
                {
                    HookMouseEvents(c);
                }
            }

            this.panel.Select();
        }

        protected virtual void OnScroll()
        {
            if (Scroll != null)
            {
                Scroll(this, EventArgs.Empty);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // scroll by e.Delta pixels, in screen coordinates
            double docDelta = (double)e.Delta / this.ScaleFactor.Ratio;
            double oldX = this.DocumentScrollPosition.X;
            double oldY = this.DocumentScrollPosition.Y;
            double newX;
            double newY;

            if (Control.ModifierKeys == Keys.Shift)
            {
                newX = this.DocumentScrollPosition.X - docDelta;
                newY = this.DocumentScrollPosition.Y;
            }
            else if (Control.ModifierKeys == Keys.None)
            {
                newX = this.DocumentScrollPosition.X;
                newY = this.DocumentScrollPosition.Y - docDelta;
            }
            else
            {
                newX = this.DocumentScrollPosition.X;
                newY = this.DocumentScrollPosition.Y;
            }

            if (newX != oldX || newY != oldY)
            {
                this.DocumentScrollPosition = new Point((int)newX, (int)newY);
                UpdateRulerOffsets();
            }

            base.OnMouseWheel(e);
        }

        public override bool IsMouseCaptured()
        {
            return this.Capture || panel.Capture || surfaceBox.Capture || controlShadow.Capture || leftRuler.Capture || topRuler.Capture;
        }

        /// <summary>
        /// Get or set upper left of scroll location in document coordinates.
        /// </summary>
        [Browsable(false)]
        public Point DocumentScrollPosition
        {
            get
            {
                if (panel == null || surfaceBox == null)
                {
                    return Point.Empty;
                }
                else
                {
                    return VisibleDocumentRectangle.Location;
                }
            }

            set
            {
                if (panel == null)
                {
                    return;
                }

                Point sbClient = surfaceBox.SurfaceToClient(value);

                if (panel.AutoScrollPosition != new Point(-sbClient.X, -sbClient.Y))
                {
                    panel.AutoScrollPosition = sbClient;
                    UpdateRulerOffsets();
                    topRuler.Invalidate();
                    leftRuler.Invalidate();
                }
            }
        }

        [Browsable(false)]
        public Point DocumentCenterPoint
        {
            get
            {
                Rectangle vsb = VisibleDocumentRectangle;
                Point centerPt = new Point((vsb.Left + vsb.Right) / 2, (vsb.Top + vsb.Bottom) / 2);
                return centerPt;
            }

            set
            {
                Rectangle vsb = VisibleDocumentRectangle;
                Point newCornerPt = new Point(value.X - (vsb.Width / 2), value.Y - (vsb.Height / 2));
                this.DocumentScrollPosition = newCornerPt;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null) 
                {
                    this.components.Dispose();
                    this.components = null;
                }

                if (this.renderSurface != null && this.freeRenderSurface)
                {
                    this.renderSurface.Dispose();
                    this.renderSurface = null;
                }
            }

            base.Dispose(disposing);
        }

        public event EventHandler ScaleFactorChanged;
        protected virtual void OnScaleFactorChanged()
        {
            if (ScaleFactorChanged != null)
            {
                ScaleFactorChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler DrawGridChanged;
        protected virtual void OnDrawGridChanged() 
        {
            if (DrawGridChanged != null)
            {
                DrawGridChanged(this, EventArgs.Empty);
            }
        }

        protected virtual bool QueryNewZoomCenterPoint(ref Point newCenterPoint)
        {
            return false;
        }

        public void ZoomToWindow()
        {
            if (this.document != null)
            {
                ScaleFactor zoom = ScaleFactor.Min(ClientRectantangleMax.Width - 10, 
                                                   document.Width,
                                                   ClientRectantangleMax.Height - 10, 
                                                   document.Height,
                                                   ScaleFactor.MinValue);
               
                ScaleFactor min = ScaleFactor.Min(zoom, ScaleFactor.OneToOne);
                this.ScaleFactor = min;
            }
        }

        public void ZoomIn()
        {
            Point centerPt = this.DocumentCenterPoint;

            ScaleFactor oldSF = this.ScaleFactor;
            ScaleFactor newSF = this.ScaleFactor;
            int countdown = 2;

            do
            {
                newSF = newSF.GetNextLarger();
                this.ScaleFactor = newSF;
                --countdown;
            } while (this.ScaleFactor == oldSF && countdown > 0);

            this.DocumentCenterPoint = centerPt;
        }

        public void ZoomOut()
        {
            Point centerPt = this.DocumentCenterPoint;

            ScaleFactor oldSF = this.ScaleFactor;
            ScaleFactor newSF = this.ScaleFactor;
            int countdown = 2;

            do
            {
                newSF = newSF.GetNextSmaller();
                this.ScaleFactor = newSF;
                --countdown;
            } while (this.ScaleFactor == oldSF && countdown > 0);

            this.DocumentCenterPoint = centerPt;
        }

        private ScaleFactor scaleFactor = new ScaleFactor(1, 1);

        [Browsable(false)]
        public ScaleFactor ScaleFactor
        {
            get
            {
                return scaleFactor;
            }

            set
            {
                UI.SetControlRedraw(this, false);

                if (value == this.scaleFactor && this.scaleFactor == ScaleFactor.OneToOne)
                {
                    // this space intentionally left blank
                }
                else
                {       
                    Rectangle visibleRect = this.VisibleDocumentRectangle;
                    ScaleFactor oldSF = scaleFactor;
                    scaleFactor = value;

                    // This value is used later below to re-center the document on screen
                    Point centerPt = new Point(visibleRect.X + visibleRect.Width / 2, 
                        visibleRect.Y + visibleRect.Height / 2);

                    if (surfaceBox != null && renderSurface != null)
                    {
                        surfaceBox.Size = Size.Truncate((SizeF)scaleFactor.ScaleSize(renderSurface.Bounds.Size));
                        scaleFactor = surfaceBox.ScaleFactor;

                        if (leftRuler != null)
                        {
                            this.leftRuler.ScaleFactor = scaleFactor;
                        }

                        if (topRuler != null)
                        {
                            this.topRuler.ScaleFactor = scaleFactor;
                        }
                    }

                    // re center ourself
                    Rectangle visibleRect2 = this.VisibleDocumentRectangle;

                    // zoom towards the selection -- DocumentWorkspace implements QueryNewZoomCenterPoint
                    Point newCenterPt = Point.Empty;
                    bool useNewCenterPt = QueryNewZoomCenterPoint(ref newCenterPt);

                    if (useNewCenterPt)
                    {
                        Point centerDifference = new Point(centerPt.X - newCenterPt.X,
                            centerPt.Y - newCenterPt.Y);

                        centerDifference = oldSF.ScalePoint(centerDifference);
                        centerDifference = scaleFactor.UnscalePoint(centerDifference);
                        centerDifference = scaleFactor.UnscalePoint(centerDifference);

                        centerPt = new Point(newCenterPt.X + centerDifference.X,
                            newCenterPt.Y + centerDifference.Y);
                    }

                    RecenterView(centerPt);
                }

                this.OnResize(EventArgs.Empty);
                this.OnScaleFactorChanged();

                UI.SetControlRedraw(this, true);
                Invalidate(true);
            }
        }

        /// <summary>
        /// Returns a rectangle for the bounding rectangle of what is currently visible on screen,
        /// in document coordinates.
        /// </summary>
        [Browsable(false)]
        public Rectangle VisibleDocumentRectangle
        {
            get
            {
                Rectangle panelRect = panel.RectangleToScreen(panel.ClientRectangle); // screen coords
                Rectangle surfaceBoxRect = surfaceBox.RectangleToScreen(surfaceBox.ClientRectangle); // screen coords
                Rectangle docScreenRect = Rectangle.Intersect(panelRect, surfaceBoxRect); // screen coords
                Rectangle docClientRect = RectangleToClient(docScreenRect);
                Rectangle docDocRect = Utility.RoundRectangle(ClientToDocument(docClientRect));
                return docDocRect;
            }
        }

        /// <summary>
        /// Returns a rectangle in <b>screen</b> coordinates that represents the space taken up
        /// by the document that is visible on screen.
        /// </summary>
        [Browsable(false)]
        public Rectangle VisibleDocumentBounds
        {
            get
            {
                // convert coordinates: document -> client -> screen
                return RectangleToScreen(Utility.RoundRectangle(DocumentToClient(VisibleDocumentRectangle)));
            }
        }

        public bool ScrollBarsVisible
        {
            get
            {
                return this.HScroll || this.VScroll;
            }
        }
        
        /// <summary>
        /// Returns a rectangle in client coordinates that represents the space that
        /// this control has left over to display the document in. That is, the size of
        /// this control minus rulers and scrollbars, if present
        /// </summary>
        [Browsable(false)]
        public Rectangle ClientRectangle2
        {
            get
            {
                return RectangleToClient(panel.RectangleToScreen(panel.ClientRectangle));
            }
        }

        public Rectangle ClientRectantangleMax 
        {
            get 
            {
                return RectangleToClient(panel.Bounds);
            }
        }

        public Rectangle ClientRectantangleMin 
        {
            get 
            {
                Rectangle bounds = RectangleToClient(panel.Bounds);
                bounds.Width -= SystemInformation.VerticalScrollBarWidth;
                bounds.Height -= SystemInformation.HorizontalScrollBarHeight;
                return bounds;
            }
        }

        public void SetHighlightRectangle(RectangleF rectF)
        {
            if (rectF.Width == 0 || rectF.Height == 0)
            {
                this.leftRuler.HighlightEnabled = false;
                this.topRuler.HighlightEnabled = false;
            }
            else
            {
                if (this.topRuler != null)
                {
                    this.topRuler.HighlightEnabled = true;
                    this.topRuler.HighlightStart = rectF.Left;
                    this.topRuler.HighlightLength = rectF.Width;
                }

                if (this.leftRuler != null)
                {
                    this.leftRuler.HighlightEnabled = true;
                    this.leftRuler.HighlightStart = rectF.Top;
                    this.leftRuler.HighlightLength = rectF.Height;
                }
            }
        }

        [Browsable(false)]
        public Document Document
        {
            get
            {
                return document;
            }

            set
            {
                SuspendRefresh();

                try
                {
                    if (document != null)
                    {
                        document.Invalidated -= documentInvalidatedDelegate;
                        document.MetaData.Changed -= this.documentMetaDataChangedDelegate;
                    }

                    document = value;

                    if (document != null)
                    {
                        if (this.renderSurface != null && 
                            this.renderSurface.Size != document.Size)
                        {
                            if (this.freeRenderSurface)
                            {
                                this.renderSurface.Dispose();
                            }

                            this.renderSurface = null;
                        }

                        if (this.renderSurface == null)
                        {
                            this.renderSurface = new Surface(Document.Size);
                            this.freeRenderSurface = true;
                        }

                        this.renderSurface.Clear(ColorBgra.White);

                        if (this.surfaceBox.Surface != this.renderSurface)
                        {
                            this.surfaceBox.Surface = this.renderSurface;
                        }

                        if (this.ScaleFactor != this.surfaceBox.ScaleFactor)
                        {
                            this.ScaleFactor = this.surfaceBox.ScaleFactor;
                        }

                        this.document.Invalidated += this.documentInvalidatedDelegate;
                        this.document.MetaData.Changed += this.documentMetaDataChangedDelegate;
                    }

                    Invalidate(true);
                    DocumentMetaDataChangedHandler(this, EventArgs.Empty);
                    this.OnResize(EventArgs.Empty);
                }

                finally
                {
                    ResumeRefresh();
                }
            }
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.topRuler = new PaintDotNet.Ruler();
            this.leftRuler = new PaintDotNet.Ruler();
            this.panel = new PaintDotNet.PanelEx();
            this.surfaceBox = new PaintDotNet.SurfaceBox();
            //this.selectionTimer = new System.Windows.Forms.Timer(this.components);
            this.panel.SuspendLayout();
            this.SuspendLayout();
            // 
            // topRuler
            // 
            this.topRuler.BackColor = System.Drawing.Color.White;
            this.topRuler.Dock = System.Windows.Forms.DockStyle.Top;
            this.topRuler.Location = new System.Drawing.Point(0, 0);
            this.topRuler.Name = "topRuler";
            this.topRuler.Offset = -16;
            this.topRuler.Size = new System.Drawing.Size(384, 16);
            this.topRuler.TabIndex = 3;
            // 
            // leftRuler
            // 
            this.leftRuler.BackColor = System.Drawing.Color.White;
            this.leftRuler.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftRuler.Location = new System.Drawing.Point(0, 16);
            this.leftRuler.Name = "leftRuler";
            this.leftRuler.Orientation = System.Windows.Forms.Orientation.Vertical;
            this.leftRuler.Size = new System.Drawing.Size(16, 304);
            this.leftRuler.TabIndex = 4;
            // 
            // panel
            // 
            this.panel.AutoScroll = true;
            this.panel.Controls.Add(this.surfaceBox);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(16, 16);
            this.panel.Name = "panel";
            this.panel.ScrollPosition = new System.Drawing.Point(0, 0);
            this.panel.Size = new System.Drawing.Size(368, 304);
            this.panel.TabIndex = 5;
            this.panel.Scroll += new System.EventHandler(this.panel_Scroll);
            this.panel.KeyDown += new KeyEventHandler(panel_KeyDown);
            this.panel.KeyUp += new KeyEventHandler(panel_KeyUp);
            this.panel.KeyPress += new KeyPressEventHandler(panel_KeyPress);
            // 
            // surfaceBox
            // 
            this.surfaceBox.Location = new System.Drawing.Point(0, 0);
            this.surfaceBox.Name = "surfaceBox";
            this.surfaceBox.Surface = null;
            this.surfaceBox.TabIndex = 0;
            this.surfaceBox.PrePaint += new PaintDotNet.PaintEventHandler2(this.surfaceBox_PrePaint);
            // 
            // DocumentView
            // 
            this.Controls.Add(this.panel);
            this.Controls.Add(this.leftRuler);
            this.Controls.Add(this.topRuler);
            this.Name = "DocumentView";
            this.Size = new System.Drawing.Size(384, 320);
            this.panel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Used to enable or disable the rulers.
        /// </summary>
        public bool RulersEnabled
        {
            get
            {
                return rulersEnabled;
            }

            set
            {
                if (rulersEnabled != value) 
                {
                    rulersEnabled = value;

                    if (topRuler != null)
                    {
                        topRuler.Enabled = value;
                        topRuler.Visible = value;
                    }

                    if (leftRuler != null)
                    {
                        leftRuler.Enabled = value;
                        leftRuler.Visible = value;
                    }

                    this.OnResize(EventArgs.Empty);
                    OnRulersEnabledChanged();
                }
            }
        }

        public event EventHandler RulersEnabledChanged;
        protected void OnRulersEnabledChanged() 
        {
            if (RulersEnabledChanged != null) 
            {
                RulersEnabledChanged(this, EventArgs.Empty);
            }
        }

        public bool PanelAutoScroll
        {
            get 
            {
                return panel.AutoScroll;
            }

            set
            {
                if (panel.AutoScroll != value) 
                {
                    panel.AutoScroll = value;
                }
            }
        }

        /// <summary>
        /// Converts a point from the Windows Forms "client" coordinate space (wrt the DocumentView)
        /// into the Document coordinate space.
        /// </summary>
        /// <param name="clientPt">A Point that is in our client coordinates.</param>
        /// <returns>A Point that is in Document coordinates.</returns>
        public PointF ClientToDocument(Point clientPt)
        {
            Point screen = PointToScreen(clientPt);
            Point sbClient = surfaceBox.PointToClient(screen);
            return surfaceBox.ClientToSurface(sbClient);
        }

        /// <summary>
        /// Converts a point from screen coordinates to document coordinates
        /// </summary>
        /// <param name="screen">The point in screen coordinates to convert to document coordinates</param>
        public PointF ScreenToDocument(PointF screen)
        {
            Point offset = surfaceBox.PointToClient(new Point(0, 0));
            return surfaceBox.ClientToSurface(new PointF(screen.X + (float)offset.X, screen.Y + (float)offset.Y));
        }

        /// <summary>
        /// Converts a point from screen coordinates to document coordinates
        /// </summary>
        /// <param name="screen">The point in screen coordinates to convert to document coordinates</param>
        public Point ScreenToDocument(Point screen)
        {
            Point offset = surfaceBox.PointToClient(new Point(0, 0));
            return surfaceBox.ClientToSurface(new Point(screen.X + offset.X, screen.Y + offset.Y));
        }

        /// <summary>
        /// Converts a PointF from the RealTimeStylus coordinate space
        /// into the Document coordinate space.
        /// </summary>
        /// <param name="clientPt">A Point that is in RealTimeStylus coordinate space.</param>
        /// <returns>A Point that is in Document coordinates.</returns>
        public PointF ClientToSurface(PointF clientPt)
        {
            return surfaceBox.ClientToSurface(clientPt);
        }

        /// <summary>
        /// Converts a point from Document coordinate space into the Windows Forms "client"
        /// coordinate space.
        /// </summary>
        /// <param name="clientPt">A Point that is in Document coordinates.</param>
        /// <returns>A Point that is in client coordinates.</returns>
        public PointF DocumentToClient(PointF documentPt)
        {
            PointF sbClient = surfaceBox.SurfaceToClient(documentPt);
            Point screen = surfaceBox.PointToScreen(Point.Round(sbClient));
            return PointToClient(screen);
        }

        /// <summary>
        /// Converts a rectangle from the Windows Forms "client" coordinate space into the Document
        /// coordinate space.
        /// </summary>
        /// <param name="clientPt">A Rectangle that is in client coordinates.</param>
        /// <returns>A Rectangle that is in Document coordinates.</returns>
        public RectangleF ClientToDocument(Rectangle clientRect)
        {
            Rectangle screen = RectangleToScreen(clientRect);
            Rectangle sbClient = surfaceBox.RectangleToClient(screen);
            return surfaceBox.ClientToSurface(sbClient);
        }

        /// <summary>
        /// Converts a rectangle from Document coordinate space into the Windows Forms "client"
        /// coordinate space.
        /// </summary>
        /// <param name="clientPt">A Rectangle that is in Document coordinates.</param>
        /// <returns>A Rectangle that is in client coordinates.</returns>
        public RectangleF DocumentToClient(RectangleF documentRect)
        {
            RectangleF sbClient = surfaceBox.SurfaceToClient(documentRect);
            Rectangle screen = surfaceBox.RectangleToScreen(Utility.RoundRectangle(sbClient));
            return RectangleToClient(screen);
        }

        private void HookMouseEvents(Control c)
        {
            if (inkAvailable)
            {
                // This must be in a separate function, otherwise we will throw an exception when JITting
                // because MS.Ink.dll won't be available
                // This is to support systems that don't have ink installed
                Ink.HookInk(this, c);
            }

            c.MouseEnter += new EventHandler(this.MouseEnterHandler);
            c.MouseLeave += new EventHandler(this.MouseLeaveHandler);
            c.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            c.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            c.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
            c.Click += new EventHandler(this.ClickHandler);

            foreach (Control c2 in c.Controls)
            {
                HookMouseEvents(c2);
            }
        }

        // these events will report mouse coordinates in document space
        // i.e. if the image is zoomed at 200% then the mouse coordinates will be divided in half

        /// <summary>
        /// Occurs when the mouse enters an element of the UI that is considered to be part of
        /// the document space.
        /// </summary>
        public event EventHandler DocumentMouseEnter;
        protected virtual void OnDocumentMouseEnter(EventArgs e)
        {
            if (DocumentMouseEnter != null)
            {
                DocumentMouseEnter(this, e);
            }
        }            

        /// <summary>
        /// Occurs when the mouse leaves an element of the UI that is considered to be part of
        /// the document space.
        /// </summary>
        /// <remarks>
        /// This event being raised does not necessarily correpond to the mouse leaving
        /// document space, only that it has left the screen space of an element of the UI
        /// that is part of document space. For example, if the mouse leaves the canvas and
        /// then enters the rulers, you will see a DocumentMouseLeave event raised which is
        /// then immediately followed by a DocumentMouseEnter event.
        /// </remarks>
        public event EventHandler DocumentMouseLeave;
        protected virtual void OnDocumentMouseLeave(EventArgs e)
        {
            if (DocumentMouseLeave != null)
            {
                DocumentMouseLeave(this, e);
            }
        }            

        /// <summary>
        /// Occurs when the mouse or stylus point is moved over the document.
        /// </summary>
        /// <remarks>
        /// Note: This event will always be raised twice in succession. One will provide a 
        /// MouseEventArgs, and the other will provide a StylusEventArgs. It is up to consumers
        /// of this event to decide which one is pertinent and to then filter out the other
        /// type of event.
        /// </remarks>
        public event MouseEventHandler DocumentMouseMove;
        protected virtual void OnDocumentMouseMove(MouseEventArgs e)
        {
            if (!inkAvailable)
            {
                if (DocumentMouseMove != null)
                {
                    DocumentMouseMove(this, new StylusEventArgs(e));
                }
            }

            if (DocumentMouseMove != null)
            {
                DocumentMouseMove(this, e);
            }
        }

        public void PerformDocumentMouseMove(MouseEventArgs e) 
        {
            OnDocumentMouseMove(e);
        }

        void IInkHooks.PerformDocumentMouseMove(MouseButtons button, int clicks, float x, float y, int delta, float pressure)
        {
            PerformDocumentMouseMove(new StylusEventArgs(button, clicks, x, y, delta, pressure));
        }

        /// <summary>
        /// Occurs when the mouse or stylus point is over the document and a mouse button is released
        /// or the stylus is lifted.
        /// </summary>
        /// <remarks>
        /// Note: This event will always be raised twice in succession. One will provide a 
        /// MouseEventArgs, and the other will provide a StylusEventArgs. It is up to consumers
        /// of this event to decide which one is pertinent and to then filter out the other
        /// type of event.
        /// </remarks>
        public event MouseEventHandler DocumentMouseUp;

        protected virtual void OnDocumentMouseUp(MouseEventArgs e)
        {
            if (!inkAvailable)
            {
                if (DocumentMouseUp != null)
                {
                    DocumentMouseUp(this, new StylusEventArgs(e));
                }
            }

            if (DocumentMouseUp != null)
            {
                DocumentMouseUp(this, e);
            }
        }

        public void PerformDocumentMouseUp(MouseEventArgs e) 
        {
            OnDocumentMouseUp(e);
        }

        void IInkHooks.PerformDocumentMouseUp(MouseButtons button, int clicks, float x, float y, int delta, float pressure)
        {
            PerformDocumentMouseUp(new StylusEventArgs(button, clicks, x, y, delta, pressure));
        }

        /// <summary>
        /// Occurs when the mouse or stylus point is over the document and a mouse button or
        /// stylus is pressed.
        /// </summary>
        /// <remarks>
        /// Note: This event will always be raised twice in succession. One will provide a 
        /// MouseEventArgs, and the other will provide a StylusEventArgs. It is up to consumers
        /// of this event to decide which one is pertinent and to then filter out the other
        /// type of event.
        /// </remarks>
        public event MouseEventHandler DocumentMouseDown;

        protected virtual void OnDocumentMouseDown(MouseEventArgs e)
        {
            if (!inkAvailable)
            {
                if (DocumentMouseDown != null)
                {
                    DocumentMouseDown(this, new StylusEventArgs(e));
                }
            }

            if (DocumentMouseDown != null)
            {
                DocumentMouseDown(this, e);
            }
        }

        public void PerformDocumentMouseDown(MouseEventArgs e) 
        {
            OnDocumentMouseDown(e);
        }

        void IInkHooks.PerformDocumentMouseDown(MouseButtons button, int clicks, float x, float y, int delta, float pressure)
        {
            PerformDocumentMouseDown(new StylusEventArgs(button, clicks, x, y, delta, pressure));
        }

        public event EventHandler DocumentClick;
        protected void OnDocumentClick()
        {
            if (DocumentClick != null)
            {
                DocumentClick(this, EventArgs.Empty);
            }
        }

        public event KeyPressEventHandler DocumentKeyPress;
        protected void OnDocumentKeyPress(KeyPressEventArgs e)
        {
            if (DocumentKeyPress != null)
            {
                DocumentKeyPress(this, e);
            }
        }

        private void panel_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnDocumentKeyPress(e);
        }

        public event KeyEventHandler DocumentKeyDown;
        protected void OnDocumentKeyDown(KeyEventArgs e)
        {
            if (DocumentKeyDown != null)
            {
                DocumentKeyDown(this, e);
            }
        }

        private void panel_KeyDown(object sender, KeyEventArgs e)
        {
            OnDocumentKeyDown(e);

            if (!e.Handled)
            {
                Point oldPt = this.DocumentScrollPosition;
                Point newPt = oldPt;
                Rectangle vdr = VisibleDocumentRectangle;

                switch (e.KeyData)
                {
                    case Keys.Next:
                        newPt.Y += vdr.Height;
                        break;

                    case (Keys.Next | Keys.Shift):
                        newPt.X += vdr.Width;
                        break;

                    case Keys.Prior:
                        newPt.Y -= vdr.Height;
                        break;

                    case (Keys.Prior | Keys.Shift):
                        newPt.X -= vdr.Width;
                        break;

                    case Keys.Home:
                        if (oldPt.X == 0)
                        {
                            newPt.Y = 0;
                        }
                        else
                        {
                            newPt.X = 0;
                        }
                        break;

                    case Keys.End:
                        if (vdr.Right < this.document.Width - 1)
                        {
                            newPt.X = this.document.Width;
                        }
                        else
                        {
                            newPt.Y = this.document.Height;
                        }
                        break;

                    default:
                        break;
                }

                if (newPt != oldPt)
                {
                    DocumentScrollPosition = newPt;
                    e.Handled = true;
                }
            }
        }

        public event KeyEventHandler DocumentKeyUp;
        protected void OnDocumentKeyUp(KeyEventArgs e)
        {
            if (DocumentKeyUp != null)
            {
                DocumentKeyUp(this, e);
            }
        }

        private void panel_KeyUp(object sender, KeyEventArgs e)
        {
            OnDocumentKeyUp(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;

            if (Utility.IsArrowKey(keyData) || 
                keyCode == Keys.Delete ||
                keyCode == Keys.Tab)
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                // We only intercept WM_KEYDOWN because WM_KEYUP is not sent!
                switch (msg.Msg)
                {
                    case 0x100: //NativeMethods.WmConstants.WM_KEYDOWN:
                        if (this.ContainsFocus)
                        {
                            OnDocumentKeyDown(kea);
                            //OnDocumentKeyUp(kea);

                            if (Utility.IsArrowKey(keyData))
                            {
                                kea.Handled = true;
                            }
                        }

                        if (kea.Handled)
                        {
                            return true;
                        }

                        break;

                        /*
                    case 0x101: //NativeMethods.WmConstants.WM_KEYUP:
                        if (this.ContainsFocus)
                        {
                            OnDocumentKeyUp(kea);
                        }

                        return kea.Handled;
                        */
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateRulerOffsets()
        {
            topRuler.Offset = ScaleFactor.UnscaleScalar(-16.0f - surfaceBox.Location.X);
            leftRuler.Offset = ScaleFactor.UnscaleScalar(0.0f - surfaceBox.Location.Y);
        }

        public void InvalidateSurface(PdnRegion region)
        {
            surfaceBox.Invalidate(region.GetRegionReadOnly());
        }

        public void InvalidateSurface(Rectangle rect)
        {
            this.surfaceBox.Invalidate(rect);
        }

        public void InvalidateSurface()
        {
            surfaceBox.Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            DoLayout();
            base.OnLayout(e);
        }

        private void DoLayout()
        {
            if (panel.ClientRectangle != new Rectangle(0, 0, 0, 0))
            {
                // If the client area is bigger than the area used to display the image, center it
                int newX = panel.AutoScrollPosition.X;
                int newY = panel.AutoScrollPosition.Y;

                if (panel.ClientRectangle.Width > surfaceBox.Width)
                {
                    newX = panel.AutoScrollPosition.X + ((panel.ClientRectangle.Width - surfaceBox.Width) / 2);
                }

                if (panel.ClientRectangle.Height > surfaceBox.Height)
                {
                    newY = panel.AutoScrollPosition.Y + ((panel.ClientRectangle.Height - surfaceBox.Height) / 2);
                }

                Point newPoint = new Point(newX, newY); 
                
                if (surfaceBox.Location != newPoint)
                {
                    surfaceBox.Location = newPoint;
                }
            }

            this.UpdateRulerOffsets();
        }

        private FormWindowState oldWindowState = FormWindowState.Minimized;
        protected override void OnResize(EventArgs e)
        {
            // enable or disable timer: no sense drawing selection if we're minimized
            if (ParentForm == null)
            {   
                // but we can't make that decision if we have no parent yet ...
                // ... which can and does happen during first time init/setup
            }
            else 
            {
                if (ParentForm.WindowState != oldWindowState)
                {
                    PerformLayout();
                }

                oldWindowState = ParentForm.WindowState;
            }

            base.OnResize(e);
            DoLayout();
        }       

        private Point MouseToDocument(object sender, Point mouse) 
        {
            if (!(sender is Control))
            {
                throw new ArgumentException("sender must reference a valid control", "sender");
            }

            Control control = (Control)sender;
            Point screenPoint = control.PointToScreen(mouse);
            Point sbClient = surfaceBox.PointToClient(screenPoint);

            // Note: We're intentionally making this truncate instead of rounding so that
            // when the image is zoomed in, the proper pixel is affected
            Point docPoint = Point.Truncate(surfaceBox.ClientToSurface(sbClient));
            
            return docPoint;
        }

        private void MouseEnterHandler(object sender, EventArgs e)
        {
            OnDocumentMouseEnter(EventArgs.Empty);
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            OnDocumentMouseLeave(EventArgs.Empty);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            Point docPoint = MouseToDocument(sender, new Point(e.X, e.Y));

            if (RulersEnabled)
            {
                topRuler.Value = docPoint.X;
                leftRuler.Value = docPoint.Y;

                UpdateRulerOffsets();
            }

            OnDocumentMouseMove(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (sender is Ruler)
            {
                return;
            }

            Point docPoint = MouseToDocument(sender, new Point(e.X, e.Y));
            Point pt = panel.AutoScrollPosition;
            panel.Focus();

            OnDocumentMouseUp(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            if (sender is Ruler)
            {
                return;
            }

            Point docPoint = MouseToDocument(sender, new Point(e.X, e.Y));
            Point pt = panel.AutoScrollPosition;
            panel.Focus();

            OnDocumentMouseDown(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            Point pt = panel.AutoScrollPosition;
            panel.Focus();
            OnDocumentClick();
        }

        private void DocumentInvalidatedHandler(object sender, InvalidateEventArgs e)
        {
            if (this.ScaleFactor == ScaleFactor.OneToOne)
            {
                surfaceBox.Invalidate(e.InvalidRect);
            }
            else
            {
                Rectangle inflatedInvalidRect = Rectangle.Inflate(e.InvalidRect, 1, 1);
                Rectangle clientRect = surfaceBox.SurfaceToClient(inflatedInvalidRect);
                Rectangle inflatedClientRect = Rectangle.Inflate(clientRect, 1, 1);
                surfaceBox.Invalidate(inflatedClientRect);
            }
        }

        private void panel_Scroll(object sender, System.EventArgs e)
        {
            OnScroll();
            UpdateRulerOffsets();
        }

        /// <summary>
        /// Before the SurfaceBox paints itself, we need to make sure that the document's composition is up to date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void surfaceBox_PrePaint(object sender, PaintEventArgs2 e)
        {
            // e.ClipRectangle is in SurfaceBox's client coordinates
            // Rectangle docClipRect = Utility.RoundRectangle(surfaceBox.ClientToSurface(e.ClipRectangle));

            // render the document
            using (RenderArgs ra = new RenderArgs(renderSurface))
            {
                document.Update(ra);
            }
        }

        // Note: You use the Suspend/Resume pattern to suspend and resume refreshing (it hides the controls for a brief moment)
        //       This is used by set_Document to avoid twitching/flickering in certain cases.
        //       However, you should use Resume followed by Suspend to bypass the SetDocument()'s use of that.
        //       Interestingly, SaveConfigDialog does this to avoid 'blinking' when the save parameters are changed.
        public void SuspendRefresh()
        {
            ++refreshSuspended;

            surfaceBox.Visible 
                = controlShadow.Visible = (refreshSuspended <= 0);
        }

        public void ResumeRefresh()
        {
            --refreshSuspended;

            surfaceBox.Visible 
                = controlShadow.Visible = (refreshSuspended <= 0);
        }

        public void RecenterView(Point newCenter) 
        {
            Rectangle visibleRect = VisibleDocumentRectangle;

            Point cornerPt = new Point(
                newCenter.X - (visibleRect.Width / 2), 
                newCenter.Y - (visibleRect.Height / 2));

            this.DocumentScrollPosition = cornerPt;
        }

        public new void Focus()
        {
            panel.Focus();
        }

        private void DocumentMetaDataChangedHandler(object sender, EventArgs e)
        {
            if (document != null)
            {
                this.leftRuler.Dpu = 1 / document.PixelToPhysicalY(1, this.leftRuler.MeasurementUnit);
                this.topRuler.Dpu = 1 / document.PixelToPhysicalY(1, this.topRuler.MeasurementUnit);
            }
        }
    }
}
