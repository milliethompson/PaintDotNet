using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FloatingToolForm.
    /// </summary>
	public class FloatingToolForm 
		: PdnBaseForm
	{
		private System.ComponentModel.IContainer components = null;

		private ControlEventHandler controlAddedDelegate;
		private ControlEventHandler controlRemovedDelegate;
		private KeyEventHandler keyUpDelegate;
		private bool repositionMe;

		//Snap bounds variables
		private WhichEdge snapToEdge;
		private DocumentView attachControl;
		public const int snapThreshold = 6;
		

		// The control that it needs to snap to
		public DocumentView AttachControl
		{
			get
			{
				return attachControl;
			}
			set
			{
				this.attachControl = value;
				
				if(value != null)
				{
					this.attachControl.Resize +=new EventHandler(attachControl_Resize);
					// hook up event handlers
				}
			}
		}

		public WhichEdge Reposition
		{
			get
			{
				return snapToEdge;
			}
			set
			{ 
				repositionMe = true;

				this.SnapToEdge = value;

				repositionMe = false;
			}
		}
		// Snaps the edge to the value given
		public WhichEdge SnapToEdge
		{
			get
			{
				return snapToEdge;
			}
			set
			{ 
				if((snapToEdge != value && this.Focused == false))
					if(repositionMe == false)
					return;

				this.snapToEdge = value;
				switch(this.snapToEdge)
				{
					case WhichEdge.TopLeft:
						this.Location = GetTopLeft();
						break;

					case WhichEdge.Top:
						this.Location = GetTop();
						break;

					case WhichEdge.TopRight:
						this.Location = GetTopRight();
						break;

					case WhichEdge.Right:
						this.Location = GetRight();
						break;

					case WhichEdge.BottomRight:
						this.Location = GetBottomRight();
						break;

					case WhichEdge.Bottom:
						this.Location = GetBottom();
						break;

					case WhichEdge.BottomLeft:
						this.Location = GetBottomLeft();
						break;

					case WhichEdge.Left:
						this.Location = GetLeft();
						break;

					case WhichEdge.None:
						break;			
				}
			}
		}

        public FloatingToolForm()
        {
            this.KeyPreview = true;
            controlAddedDelegate = new ControlEventHandler(ControlAddedHandler);
            controlRemovedDelegate = new ControlEventHandler(ControlRemovedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            this.ControlAdded += controlAddedDelegate;
            this.ControlRemoved += controlRemovedDelegate;

			attachControl = null;
			
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated (e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Utility.IsArrowKey(keyData))
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                switch (msg.Msg)
                {
                    case NativeMethods.WmConstants.WM_KEYDOWN:
                        this.OnKeyDown(kea);
                        return kea.Handled;

                        /*
                    case NativeMethods.WmConstants.WM_KEYUP:
                        this.OnKeyUp(kea);
                        return kea.Handled;
                        */
                }
            }

            return base.ProcessCmdKey (ref msg, keyData);
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			// 
			// FloatingToolForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 271);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FloatingToolForm";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "FloatingToolForm";

		}
        #endregion

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded += controlAddedDelegate;
            e.Control.ControlRemoved += controlRemovedDelegate;
            e.Control.KeyUp += keyUpDelegate;
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded -= controlAddedDelegate;
            e.Control.ControlRemoved -= controlRemovedDelegate;
            e.Control.KeyUp -= keyUpDelegate;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnKeyUp(e);
            }
        }

		//Checks if a form is in the area of a snap point
		private WhichEdge InArea(int x, int y)
		{
			Point myLocation = new Point(x,y);

			// All the points where the form can snap to
			Point topRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width, AttachControl.ClientRectangle2.Top ));
			Point bottomRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width ,AttachControl.ClientRectangle2.Bottom - this.Height ));
			Point topLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left ,AttachControl.ClientRectangle2.Top ));			
			Point bottomLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left , AttachControl.ClientRectangle2.Bottom - this.Height ));
			Point left = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left,AttachControl.ClientRectangle2.Top + (AttachControl.ClientRectangle2.Height/2) - (this.Height/2)));
			Point bottom = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + (AttachControl.ClientRectangle2.Width/2) - (this.Width/2),AttachControl.ClientRectangle2.Bottom  - this.Height));
			Point top = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + (AttachControl.ClientRectangle2.Width/2) - (this.Width/2),AttachControl.ClientRectangle2.Top ));
			Point right = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width , AttachControl.ClientRectangle2.Top + (AttachControl.ClientRectangle2.Height/2) - (this.Height/2)));

			int testTop = AttachControl.ClientRectangle2.Location.Y;
			int testBottom = new Point(AttachControl.ClientRectangle2.Left,AttachControl.ClientRectangle2.Bottom).Y;

			int testLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left ,AttachControl.ClientRectangle2.Top )).X;
			int testRight = new Point(AttachControl.ClientRectangle2.Right,AttachControl.ClientRectangle2.Bottom).X;

			if(Math.Abs(myLocation.X - topLeft.X) <= snapThreshold &&
			   Math.Abs(myLocation.Y - topLeft.Y) <= snapThreshold)
			{

				return WhichEdge.TopLeft;
			}
			else
				if(Math.Abs(myLocation.X - bottomLeft.X) <= snapThreshold &&
				Math.Abs(myLocation.Y - bottomLeft.Y) <= snapThreshold)
			{

				return WhichEdge.BottomLeft;
			}
			else
				if(Math.Abs(myLocation.X - topRight.X) <= snapThreshold &&
				Math.Abs(myLocation.Y - topRight.Y) <= snapThreshold)
			{
				return WhichEdge.TopRight;
			}
			else
				if(Math.Abs(myLocation.X - bottomRight.X) <= snapThreshold &&
				Math.Abs(myLocation.Y - bottomRight.Y) <= snapThreshold)
			{
				return WhichEdge.BottomRight;
			}
			else
				if(Math.Abs(myLocation.Y - top.Y) <= snapThreshold &&
				(myLocation.X > testLeft) && (myLocation.X < testRight))
			{
				return WhichEdge.Top;
			}
			else
				if(Math.Abs(myLocation.X - left.X) <= snapThreshold &&
				(myLocation.Y > testTop) && (myLocation.Y < testBottom))
			{
				return WhichEdge.Left;
			}
			else
				if(Math.Abs(myLocation.X - right.X) <= snapThreshold &&
				(myLocation.Y > testTop) && (myLocation.Y < testBottom))
			{
				return WhichEdge.Right;
			}
			else
				if(Math.Abs(myLocation.Y - bottom.Y) <= snapThreshold &&
				(myLocation.X > testLeft) && (myLocation.X < testRight))
			{
				return WhichEdge.Bottom;
			}
			else
			{
				return WhichEdge.None;
			}
		}

		// The following functions return the spot that the form is supposed to be snapping to
		// 8 snap points:
		// topleft, top, topright, right, bottomright, bottom, bottomleft, left
		private Point GetTopLeft()
		{
			Point topLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3,AttachControl.ClientRectangle2.Top + 3));

			return new Point(topLeft.X,topLeft.Y);
		}

		private Point GetTop()
		{
			int testLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left ,AttachControl.ClientRectangle2.Top )).X;			
			int testRight = new Point(AttachControl.ClientRectangle2.Right,AttachControl.ClientRectangle2.Bottom).X;
			Point top = AttachControl.PointToScreen(new Point(0,AttachControl.ClientRectangle2.Top + 3));
					
			if(this.Location.X < testLeft)
				top.X = testLeft;
			else
				if(this.Location.X > testRight)
				top.X = testRight;
			else
				top.X = this.Location.X;

			return top;
		}

		private Point GetBottom()
		{
			int testLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left ,AttachControl.ClientRectangle2.Top )).X;			
			int testRight = new Point(AttachControl.ClientRectangle2.Right,AttachControl.ClientRectangle2.Bottom).X;
			Point bottom = AttachControl.PointToScreen(new Point(0,AttachControl.ClientRectangle2.Bottom - 3 - this.Height));
					
			if(this.Location.X < testLeft)
				bottom.X = testLeft;
			else
				if(this.Location.X > testRight)
				bottom.X = testRight;
			else
				bottom.X = this.Location.X;

			return bottom;
		}

		private Point GetTopRight()
		{
			Point topRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, AttachControl.ClientRectangle2.Top + 3));

			return new Point(topRight.X, topRight.Y);
		}

		private Point GetRight()
		{
			int testTop = AttachControl.ClientRectangle2.Location.Y;
			int testRight= new Point(AttachControl.ClientRectangle2.Left,AttachControl.ClientRectangle2.Bottom).Y;
			Point right = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 0));
					
			if(this.Location.Y < testTop)
				right.Y = testTop;
			else
				if(this.Location.Y > testRight)
				right.Y = testRight;
			else
				right.Y = this.Location.Y;

			return new Point(right.X,right.Y);
		}

		private Point GetBottomRight()
		{
			Point bottomRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3,AttachControl.ClientRectangle2.Bottom - this.Height - 3));

			return new Point(bottomRight.X,bottomRight.Y);
		}

		private Point GetBottomLeft()
		{
			Point bottomLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3, AttachControl.ClientRectangle2.Bottom - this.Height - 3));

			return new Point(bottomLeft.X,bottomLeft.Y);
		}

		private Point GetLeft()
		{
			int testTop = AttachControl.ClientRectangle2.Location.Y;
			int testBottom = new Point(AttachControl.ClientRectangle2.Left,AttachControl.ClientRectangle2.Bottom).Y;
			Point left = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3,0));
					
			if(this.Location.Y < testTop)
				left.Y = testTop;
			else
				if(this.Location.Y > testBottom)
					left.Y = testBottom;
			else
				left.Y = this.Location.Y;

			return new Point(left.X,left.Y);
		}


		// Snaps the form if it needs it
		protected override void OnMove(System.EventArgs e)
		{
			base.OnMove(e);

			if(this.attachControl != null)
			{
				WhichEdge where = InArea(this.Location.X,this.Location.Y);
				this.SnapToEdge = where;
			}

		}

		private void attachControl_Resize(object sender, EventArgs e)
		{
			if(this.snapToEdge != WhichEdge.None)
			{
				this.SnapToEdge = this.snapToEdge;
			}
		}

		protected override void OnMoving(MovingEventArgs mea)
		{
			base.OnMoving(mea);

			// If it is in area to where the rectangle wants to move
			WhichEdge where = InArea(mea.Rectangle.X,mea.Rectangle.Y);

			Point placeMe = new Point(mea.Rectangle.X,mea.Rectangle.Y);

			switch(where)
			{
				case WhichEdge.TopLeft:
					placeMe = GetTopLeft();
					break;

				case WhichEdge.Top:
					placeMe = GetTop();
					placeMe.X = mea.Rectangle.X;
					break;

				case WhichEdge.TopRight:
					placeMe = GetTopRight();
					break;

				case WhichEdge.Right:
					placeMe = GetRight();
					placeMe.Y = mea.Rectangle.Y;
					break;

				case WhichEdge.BottomRight:
					placeMe = GetBottomRight();
					break;

				case WhichEdge.Bottom:
					placeMe = GetBottom();
					placeMe.X = mea.Rectangle.X;
					break;

				case WhichEdge.BottomLeft:
					placeMe = GetBottomLeft();
					break;

				case WhichEdge.Left:
					placeMe = GetLeft();
					placeMe.Y = mea.Rectangle.Y;
					break;

				case WhichEdge.None:
					break;			
			}

			Rectangle rect = new Rectangle(placeMe.X,placeMe.Y,this.Width,this.Height);
			this.snapToEdge = where;
			mea.Rectangle = rect;
		}
	}
}


