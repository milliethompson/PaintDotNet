using System;
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
        : UserControl
    {
        private bool rulersEnabled;
        private Document document;
        private Surface renderSurface;
        private PaintDotNet.Ruler leftRuler;
        private PaintDotNet.PanelEx panel;
        private PaintDotNet.Ruler topRuler;
        private InvalidateEventHandler documentInvalidatedDelegate;
        private InvalidateEventHandler surfaceBoxInvalidatedDelegate;
        private PaintDotNet.SurfaceBox surfaceBox;
        private System.ComponentModel.IContainer components = null;

        [Browsable(false)]
        public bool IsMouseCaptured
        {
            get
            {
                //return Utility.DoesControlHaveMouseCaptured(this);
                return this.Capture || panel.Capture || surfaceBox.Capture;
            }
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

				Point pt = new Point(-panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y);
				PointF pt2 = ClientToDocument(PointToClient(panel.PointToScreen(pt)));
				return pt2;
			}

			set
			{
                if (panel == null)
                {
                    return;
                }

				Point pt = panel.PointToClient(PointToScreen(Point.Round(DocumentToClient(value))));
				pt.X = (pt.X / this.ScaleFactor.Numerator) * this.ScaleFactor.Numerator;
				pt.Y = (pt.Y / this.ScaleFactor.Numerator) * this.ScaleFactor.Numerator;

				if (pt.X != -panel.AutoScrollPosition.X ||
					pt.Y != -panel.AutoScrollPosition.Y)
				{
					panel.AutoScrollPosition = pt;
					panel.Update();
					Debug.WriteLine("set value=" + value.ToString() + " pt=" + pt.ToString());
				}
				else
				{
					Debug.WriteLine("set value=" + value.ToString() + " (no set)");
				}
			}
		}

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
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
				scaleFactor = value;

				if (surfaceBox != null && renderSurface != null)
				{
                    int factor = Math.Max(value.Numerator, value.Denominator);
                    Size newSize = scaleFactor.ScaleSize(new Size(renderSurface.Width, renderSurface.Height));
					surfaceBox.Size = new Size(Math.Max(1, newSize.Width), Math.Max(1, newSize.Height));
				}

                if (surfaceBox != null)
                {
                    if (leftRuler != null)
                    {
                        this.leftRuler.ScaleFactor = scaleFactor;
                    }

                    if (topRuler != null)
                    {
                        this.topRuler.ScaleFactor = scaleFactor;
                    }
                }

				Invalidate(true);
				this.OnResize(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Returns a rectangle for the bounding rectangle of what is currently visible on screen,
		/// in document coordinates.
		/// </summary>
		public Rectangle VisibleDocumentRectangle
		{
			get
			{
				//return Utility.RoundRectangle(this.ClientToDocument(new Rectangle(-panel.AutoScrollPosition.X, -panel.AutoScrollPosition.Y, panel.ClientRectangle.Width, panel.ClientRectangle.Height)));
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
        public Rectangle ClientRectangle2
        {
            get
            {

                return RectangleToClient(panel.RectangleToScreen(panel.ClientRectangle));
            }
        }

        /// <summary>
        /// We hold a reference to a GraphicsPath that we use to draw the "selected region"
        /// Basically this is a way to get around the fact we do not have access to the 
        /// Document's Environment ...
        /// </summary>
        private GraphicsPath selectedPath;
        public GraphicsPath SelectedPath
        {
            set
            {
                selectedPath = value;
            }
        }

        private bool highlightSelection = true;
        public bool HighlightSelection
        {
            get
            {
                return highlightSelection;
            }

            set
            {
                highlightSelection = value;
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

                    document.Invalidated += documentInvalidatedDelegate;
                    //document.Invalidate();
                }

				this.ScaleFactor = this.ScaleFactor;
                Invalidate(true);
                this.OnResize(EventArgs.Empty);
            }
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.topRuler = new PaintDotNet.Ruler();
            this.leftRuler = new PaintDotNet.Ruler();
            this.panel = new PaintDotNet.PanelEx();
            this.surfaceBox = new PaintDotNet.SurfaceBox();
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
            this.topRuler.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.topRuler.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            this.topRuler.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
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
            this.leftRuler.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.leftRuler.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            this.leftRuler.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
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
            this.panel.Click += new System.EventHandler(this.ClickHandler);
            this.panel.Resize += new System.EventHandler(this.panel_Resize);
            this.panel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.panel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            this.panel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
            this.panel.Scroll += new System.EventHandler(this.panel_Scroll);
            // 
            // surfaceBox
            // 
            this.surfaceBox.Location = new System.Drawing.Point(0, 0);
            this.surfaceBox.Name = "surfaceBox";
            this.surfaceBox.Surface = null;
            this.surfaceBox.TabIndex = 0;
            this.surfaceBox.Click += new System.EventHandler(this.ClickHandler);
            this.surfaceBox.PrePaint += new System.Windows.Forms.PaintEventHandler(this.surfaceBox_PrePaint);
            this.surfaceBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MouseUpHandler);
            this.surfaceBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MouseMoveHandler);
            this.surfaceBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MouseDownHandler);
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
        }

        // these events will report mouse coordinates in document space
        // i.e. if the image is zoomed at 200% then the mouse coordinates will be divided in half
        public event MouseEventHandler DocumentMouseMove;
        protected void OnDocumentMouseMove(MouseEventArgs e)
        {
            if (DocumentMouseMove != null)
            {
                DocumentMouseMove(this, e);
            }
        }

        public event MouseEventHandler DocumentMouseUp;
        protected void OnDocumentMouseUp(MouseEventArgs e)
        {
            if (DocumentMouseUp != null)
            {
                DocumentMouseUp(this, e);
            }
        }

        public event MouseEventHandler DocumentMouseDown;
        protected void OnDocumentMouseDown(MouseEventArgs e)
        {
            if (DocumentMouseDown != null)
            {
                DocumentMouseDown(this, e);
            }
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
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Point screenPoint = control.PointToScreen(new Point(e.X, e.Y));
            Point sbClient = surfaceBox.PointToClient(screenPoint);
            Point docPoint = Point.Truncate(surfaceBox.ClientToSurface(sbClient));

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
            Control control = (Control)sender;
            Point screenPoint = control.PointToScreen(new Point(e.X, e.Y));
            Point sbClient = surfaceBox.PointToClient(screenPoint);
            Point docPoint = Point.Truncate(surfaceBox.ClientToSurface(sbClient));

            Point pt = panel.AutoScrollPosition;
			panel.Focus();
            panel.AutoScrollPosition = new Point(-pt.X, -pt.Y);

            OnDocumentMouseUp(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            Control control = (Control)sender;
            Point screenPoint = control.PointToScreen(new Point(e.X, e.Y));
            Point sbClient = surfaceBox.PointToClient(screenPoint);
            Point docPoint = Point.Truncate(surfaceBox.ClientToSurface(sbClient));

            Point pt = panel.AutoScrollPosition;
            panel.Focus();
            panel.AutoScrollPosition = new Point(-pt.X, -pt.Y);

            OnDocumentMouseDown(new MouseEventArgs(e.Button, e.Clicks, docPoint.X, docPoint.Y, e.Delta));
        }

        private void ClickHandler(object sender, EventArgs e)
        {
            Point pt = panel.AutoScrollPosition;
            panel.Focus();
            panel.AutoScrollPosition = new Point(-pt.X, -pt.Y);
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

        private static void DrawSelection(RenderArgs ra, Region interior, GraphicsPath outline)
        {
            if (interior == null || outline == null)
            {
                return;
            }

            ra.Graphics.SetClip(interior, CombineMode.Replace);

            using (Brush brush = new SolidBrush(Color.FromArgb(32, Color.Blue)))
            {
                ra.Graphics.CompositingMode = CompositingMode.SourceOver;
                ra.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                ra.Graphics.FillPath(brush, outline);
            }
        }

        private void panel_Scroll(object sender, System.EventArgs e)
        {
            UpdateRulerOffsets();
            Update();
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
                using (GraphicsPath closed = (GraphicsPath)selectedPath.Clone())
                {
                    closed.CloseAllFigures();

                    using (Region sRegion = new Region(closed))
                    {
                        sRegion.Intersect(ra.Surface.Bounds);
                        sRegion.Intersect(docClipRect);
                        //sRegion.Intersect(document.UpdateRegion);
                        document.Update(ra);
                        document.Render(ra, sRegion);

                        // handle region interior
                        if (selectedPath.PointCount != 0)
                        {
                            DrawSelection(ra, sRegion, closed);
                        }
                    }
                }
            }
        }
    }
}
