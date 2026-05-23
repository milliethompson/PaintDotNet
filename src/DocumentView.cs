using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.StylusInput;
using Microsoft.StylusInput.PluginData;
using Microsoft.Ink;

namespace PaintDotNet
{
    /// <summary>
    /// Encapsulates rendering the document by itself, including rulers and
    /// scrollbar decorators. It also raises events for mouse movement that
    /// are properly translated to (x,y) pixel coordinates within the document
    /// (DocumentMouse* events).
    /// </summary>
	public class DocumentView
		: UserControl
	{
		//rulers really are on by default, so 'true' was set to show this.
		private bool rulersEnabled = true;

		private uint refreshSuspended = 0;

		private Document document;
		private Surface renderSurface;
		private PaintDotNet.Ruler leftRuler;
		private PaintDotNet.PanelEx panel;
		private PaintDotNet.Ruler topRuler;
		private InvalidateEventHandler documentInvalidatedDelegate;
		private InvalidateEventHandler surfaceBoxInvalidatedDelegate;
		private PaintDotNet.SurfaceBox surfaceBox;
		private System.Windows.Forms.Timer selectionTimer;
		private System.ComponentModel.IContainer components = null;
		private ControlShadow controlShadow;
		private const int dancingAntsInterval = 50;

		public event EventHandler Scroll;

		private bool enableOutlineAnimation = true;

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
					if (scaleFactor.Ratio >= SurfaceBox.DrawGridMinimumZoom) 
					{
						this.Invalidate(true);
					}
					OnDrawGridChanged();
				}
			}
		}

        public bool EnableOutlineAnimation
        {
            get
            {
                return enableOutlineAnimation;
            }

            set
            {
                enableOutlineAnimation = value;
            }
        }
    
		public override bool Focused
		{
			get
			{
				return base.Focused || panel.Focused || surfaceBox.Focused || controlShadow.Focused;
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
            surfaceBoxInvalidatedDelegate = new InvalidateEventHandler(SurfaceBoxInvalidatedHandler);
            surfaceBox.Invalidated += surfaceBoxInvalidatedDelegate;
            panel.Focus();
            this.selectionTimer.Interval = DocumentView.dancingAntsInterval / 4;

            controlShadow = new ControlShadow();
            controlShadow.OccludingControl = surfaceBox;
            panel.Controls.Add(controlShadow);
            panel.Controls.SetChildIndex(controlShadow, panel.Controls.Count - 1);

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);

            foreach (Control c in Controls)
            {
                HookMouseEvents(c);
            }
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
            base.OnMouseWheel (e);

            if (0 != (Control.ModifierKeys & Keys.Control))
            {
                if (e.Delta > 0)
				{
					this.ScaleFactor = this.ScaleFactor.GetNextLarger();
				}
				else if (e.Delta < 0)
				{
					this.ScaleFactor = this.ScaleFactor.GetNextSmaller();
                }
            }
        }


        [Browsable(false)]
        public bool IsMouseCaptured()
        {
            //return Utility.DoesControlHaveMouseCaptured(this);
            return this.Capture || panel.Capture || surfaceBox.Capture || controlShadow.Capture;;
        }

        /// <summary>
        /// Get or set upper left of scroll location in document coordinates.
        /// </summary>
        [Browsable(false)]
        public PointF DocumentScrollPosition
        {
            get
            {
                if (panel == null)
                {
                    return Point.Empty;
                }

                return VisibleDocumentRectangle.Location;
            }

            set
            {
                if (panel == null)
                {
                    return;
                }

                PointF sbClientF = surfaceBox.SurfaceToClient(value);
                Point sbClient = Point.Round(sbClientF);

                if (panel.AutoScrollPosition != new Point(-sbClient.X, -sbClient.Y))
                {
                    panel.AutoScrollPosition = sbClient;
                }
				topRuler.Invalidate();
				leftRuler.Invalidate();
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
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

        private ScaleFactor scaleFactor = new ScaleFactor(1, 1);

        public ScaleFactor ScaleFactor
        {
            get
            {
                return scaleFactor;
            }

            set
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

				// zoom towards the selection
				if (this.Parent is DocumentWorkspace) 
				{
					DocumentEnvironment env = (this.Parent as DocumentWorkspace).Environment;
					if (!env.IsSelectionEmpty) 
					{
						Rectangle selectionBounds = Rectangle.Truncate(env.CreateSelectedRegion().GetBounds());
						Point selectionCenter = new Point((selectionBounds.Left + selectionBounds.Right) / 2,
							(selectionBounds.Top + selectionBounds.Bottom) / 2);
						Point centerDifference = new Point(centerPt.X - selectionCenter.X,
							centerPt.Y - selectionCenter.Y);
						centerDifference = oldSF.ScalePoint(centerDifference);
						centerDifference = scaleFactor.UnscalePoint(centerDifference);
						centerDifference = scaleFactor.UnscalePoint(centerDifference);
						centerPt = new Point(selectionCenter.X + centerDifference.X,
							selectionCenter.Y + centerDifference.Y);
					}
				}
				RecenterView(centerPt);
                Invalidate(true);
                this.OnResize(EventArgs.Empty);
                this.OnScaleFactorChanged();
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
        /// <summary>
        /// We hold a reference to a PdnGraphicsPath that we use to draw the "selected region"
        /// Basically this is a way to get around the fact we do not have access to the 
        /// Document's Environment ...
        /// </summary>
        private PdnGraphicsPath selectedPath;

        [Browsable(false)]
        public PdnGraphicsPath SelectedPath
        {
            set
            {
                selectedPath = value;
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

                if (document != null)
                {
                    document.Invalidated -= documentInvalidatedDelegate;
                }

                document = value;

                if (document != null)
                {
                    Surface newRenderSurface = new Surface(document.Width, document.Height);
                    new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 255)).Apply(newRenderSurface, newRenderSurface.Bounds);

                    if (renderSurface != null)
                    {
                        renderSurface.Dispose();
                        renderSurface = null;
                    }

                    this.renderSurface = newRenderSurface;
                    surfaceBox.Surface = newRenderSurface;
					ScaleFactor = surfaceBox.ScaleFactor;

                    document.Invalidated += documentInvalidatedDelegate;
                }

                Invalidate(true);
                this.OnResize(EventArgs.Empty);
				
				ResumeRefresh();
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
            this.selectionTimer = new System.Windows.Forms.Timer(this.components);
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
            this.panel.Resize += new System.EventHandler(this.panel_Resize);
            this.panel.Scroll += new System.EventHandler(this.panel_Scroll);
            // 
            // surfaceBox
            // 
            this.surfaceBox.Location = new System.Drawing.Point(0, 0);
            this.surfaceBox.Name = "surfaceBox";
            this.surfaceBox.Surface = null;
            this.surfaceBox.TabIndex = 0;
            this.surfaceBox.PrePaint += new System.Windows.Forms.PaintEventHandler(this.surfaceBox_PrePaint);
            // 
            // selectionTimer
            // 
            this.selectionTimer.Enabled = true;
            this.selectionTimer.Interval = 50;
            this.selectionTimer.Tick += new System.EventHandler(this.selectionTimer_Tick);
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

					Invalidate();
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
        /// This is a memory usage optimization ONLY, provided for the MainForm to generate the
        /// thumbnails for the list of recently opened files. This allows us to avoid allocating
        /// another chunk of memory to generate the thumbs for this list.
        /// </summary>
        /// <returns></returns>
        public Surface BorrowRenderSurface()
        {
            return this.renderSurface;
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
			return surfaceBox.ClientToSurface(new PointF(screen.X + offset.X, screen.Y + offset.Y));
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

        
		//If we don't keep the styluses, they get garbagecollected.
		private ArrayList stylusList = new ArrayList();
        private void HookMouseEvents(Control c)
		{
			StylusReader stylusReader = new StylusReader(this, c);
			RealTimeStylus stylus = new RealTimeStylus(c, true);
			
			stylus.AsyncPluginCollection.Add(stylusReader);
			stylus.SetDesiredPacketDescription(new Guid[] {PacketProperty.X, PacketProperty.Y, PacketProperty.NormalPressure, PacketProperty.PacketStatus});
			stylus.Enabled = true;

			stylusList.Add(stylus);

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
        public event MouseEventHandler DocumentMouseMove;
        protected virtual void OnDocumentMouseMove(MouseEventArgs e)
        {
			if (DocumentMouseMove != null)
			{
                DocumentMouseMove(this, e);
            }
        }

		public void PerformDocumentMouseMove(MouseEventArgs e) 
		{
			OnDocumentMouseMove(e);
		}

        public event MouseEventHandler DocumentMouseUp;
        protected virtual void OnDocumentMouseUp(MouseEventArgs e)
        {
            if (DocumentMouseUp != null)
            {
                DocumentMouseUp(this, e);
            }
		}

		public void PerformDocumentMouseUp(MouseEventArgs e) 
		{
			OnDocumentMouseUp(e);
		}

        public event MouseEventHandler DocumentMouseDown;
        protected virtual void OnDocumentMouseDown(MouseEventArgs e)
        {
            if (DocumentMouseDown != null)
            {
                DocumentMouseDown(this, e);
            }
		}

		public void PerformDocumentMouseDown(MouseEventArgs e) 
		{
			OnDocumentMouseDown(e);
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

        private void UpdateRulerOffsets()
        {
            topRuler.Offset = ScaleFactor.UnscaleScalar(-16 - surfaceBox.Location.X);
            leftRuler.Offset = ScaleFactor.UnscaleScalar(0 - surfaceBox.Location.Y);
        }

        protected override void OnResize(EventArgs e)
        {
			base.OnResize (e);

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

            surfaceBox.Location = new Point(newX, newY);
            UpdateRulerOffsets();
            panel.PerformLayout();

            // enable or disable timer: no sense drawing selection if we're minimized
            if (ParentForm == null)
            {   // but we can't make that decision if we have no parent yet ...
                // ... which can and does happen during first time init/setup
                return;
            }

            if (ParentForm.WindowState == FormWindowState.Minimized)
            {
                selectionTimer.Enabled = false;
            }
            else
            {
                selectionTimer.Enabled = true;
            }
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
			//Note:We're intentionally making this truncate instead of rounding so that
			//when the image is zoomed in, the proper pixel is effected
			Point docPoint = Point.Truncate(surfaceBox.ClientToSurface((PointF)sbClient));
			
			return docPoint;
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
            Point docPoint = MouseToDocument(sender, new Point(e.X, e.Y));
            Point pt = panel.AutoScrollPosition;
            panel.Focus();

            OnDocumentMouseUp(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
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
            surfaceBox.Invalidated -= surfaceBoxInvalidatedDelegate;
            surfaceBox.Invalidate(Rectangle.Inflate(Utility.RoundRectangle(surfaceBox.SurfaceToClient(Rectangle.Inflate(e.InvalidRect, 1, 1))), 1, 1), true);
            surfaceBox.Invalidated += surfaceBoxInvalidatedDelegate;
        }

        private void SurfaceBoxInvalidatedHandler(object sender, InvalidateEventArgs e)
        {
            if (document != null)
            {
                document.Invalidated -= documentInvalidatedDelegate;
                document.Invalidate(Utility.RoundRectangle(surfaceBox.ClientToSurface(RectangleF.Inflate((RectangleF)e.InvalidRect, 1, 1))));
                document.Invalidated += documentInvalidatedDelegate;
            }
        }

        private int dancingAntsT = 0;

        private static Pen outlinePen1 = null;
        private static Pen outlinePen2 = null;

        private void DrawSelectionOutline(RenderArgs ra, Graphics gdiG, PdnGraphicsPath outline)
        {
            if (outline == null)
            {
                return;
            }

            ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (outlinePen1 == null)
            {
                outlinePen1 = new Pen(Color.FromArgb(160, Color.Black), 1.0f);
                outlinePen1.Alignment = PenAlignment.Outset;
                outlinePen1.LineJoin = LineJoin.Round;
            }

            if (outlinePen2 == null)
            {
                outlinePen2 = new Pen(Color.White, 1.0f);
                outlinePen2.Alignment = PenAlignment.Outset;
                outlinePen2.LineJoin = LineJoin.Round;
                outlinePen2.MiterLimit = 2;
                outlinePen2.Width = 1.0f;
                outlinePen2.DashStyle = DashStyle.Dash;
                outlinePen2.DashPattern = new float[] { 4, 4 };
                outlinePen2.Color = Color.White;
                outlinePen2.DashOffset = 4.0f;
            }

            ra.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            ra.Graphics.DrawPath(outlinePen1, outline);
            outlinePen2.DashOffset += dancingAntsT;
            ra.Graphics.DrawPath(outlinePen2, outline);
            outlinePen2.DashOffset -= dancingAntsT;
        }

        [ThreadStatic]
        private Brush interiorBrush;

        private Brush InteriorBrush
        {
            get
            {
                if (interiorBrush == null)
                {
                    interiorBrush =  new SolidBrush(Color.FromArgb(32, 96, 96, 255));
                }

                return interiorBrush;
            }
        }

		// MK 26OCT2004 added property to draw selection without the Marching Ants
		// when selection is in motion with a MoveTool class object
		private bool enableSelectionOutline = true;
		public bool EnableSelectionOutline
		{
			get
			{
				return(enableSelectionOutline);
			}

			set
			{
				enableSelectionOutline = value;
			}
		}

        private void DrawSelectionInterior(RenderArgs ra, PdnRegion clipInterior, PdnGraphicsPath outline)
        {
            if (clipInterior == null || outline == null)
            {
                return;
            }

            ra.Graphics.SetClip(clipInterior, CombineMode.Replace);
            ra.Graphics.CompositingMode = CompositingMode.SourceOver;
            ra.Graphics.FillPath(InteriorBrush, outline);
            ra.Graphics.ResetClip();
        }

        private void DrawSelection(RenderArgs surfaceRa, Graphics gdiG, PdnRegion interior, PdnGraphicsPath outline)
        {
            if (interior == null || outline == null)
            {
                return;
            }

            DrawSelectionInterior(surfaceRa, interior, outline);

			// MK new if block to see if we should draw the selection
			// outline.  When we mouse drag our selection we don't
			// want the outline present because we can't fine tune
			// pixels
			if (EnableSelectionOutline == true)
			{
				DrawSelectionOutline(surfaceRa, gdiG, outline);
			}
        }

        private void panel_Scroll(object sender, System.EventArgs e)
        {
            UpdateRulerOffsets();
            Update();
            OnScroll();
        }

        private void panel_Resize(object sender, System.EventArgs e)
        {
            this.OnResize(e);
        }

        /// <summary>
        /// Before the SurfaceBox paints itself, we need to make sure that the document's composition is up to date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void surfaceBox_PrePaint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // e.ClipRectangle is in SurfaceBox's client coordinates
            Rectangle docClipRect = Utility.RoundRectangle(surfaceBox.ClientToSurface(e.ClipRectangle));

            // render the document
            using (RenderArgs ra = new RenderArgs(renderSurface))
            {
                using (PdnRegion sRegion = new PdnRegion(selectedPath))
                {
                    using (PdnRegion sRegion2 = Utility.SimplifyAndInflateRegion(sRegion, Utility.DefaultSimplificationFactor, 2))
                    {
                        sRegion2.Intersect(ra.Surface.Bounds);
                        sRegion2.Intersect(docClipRect);
                        sRegion.Intersect(document.UpdateRegion);

						document.Update(ra);
						document.Render(ra, sRegion2);

                        // handle region interior
                        if (selectedPath.PointCount != 0)
                        {
                            DrawSelection(ra, e.Graphics, sRegion2, selectedPath);
                        }
                    }
                }
            }
        }

        private int lastTickMod = 0;

        private void selectionTimer_Tick(object sender, System.EventArgs e)
        {
            if (!enableOutlineAnimation)
            {
                return;
            }

            if (selectedPath == null || renderSurface == null)
            {
                return;
            }

            if (this.IsMouseCaptured())
            {
                return;
            }

            if (selectedPath.PointCount == 0)
            {
                return;
            }

            int presentTickMod = (int)((Utility.GetTimeMs() / dancingAntsInterval) % 2);

            if (presentTickMod != lastTickMod)
            {
                lastTickMod = presentTickMod;
                dancingAntsT = unchecked(dancingAntsT + 1);

				using (PdnRegion simplified = Utility.RectanglesToRegion(Utility.SimplifyTrace(selectedPath)))
				{
					this.Document.Invalidate(simplified);
				}
            }
		}
		protected override void OnInvalidated(InvalidateEventArgs e)
		{
			base.OnInvalidated (e);
		}

		public void SuspendRefresh()
		{
			refreshSuspended++;
			surfaceBox.Visible = controlShadow.Visible = (refreshSuspended == 0);
		}

		public void ResumeRefresh()
		{
			refreshSuspended--;
			surfaceBox.Visible = controlShadow.Visible = (refreshSuspended == 0);
		}

		public void RecenterView(PointF newCenter) 
		{
			Rectangle visibleRect = VisibleDocumentRectangle;
			PointF cornerPt = new PointF(
				newCenter.X - (visibleRect.Width / 2), 
				newCenter.Y - (visibleRect.Height / 2));

			this.DocumentScrollPosition = cornerPt;
		}
	}
}
