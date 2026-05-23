/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
        private int leftXOffset;
        private int upperYOffset;

        //Snap bounds variables
        private WhichEdge snapToEdge;
        private DocumentView attachControl;
        private const int snapThreshold = 6;

        /// <summary>
        /// Occurs when it is appropriate for the parent to steal focus.
        /// </summary>
        public event EventHandler RelinquishFocus;
        protected virtual void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
            }
        }

        // The control that it needs to snap to
        public DocumentView AttachControl
        {
            get
            {
                return attachControl;
            }

            set
            {
                if (this.attachControl != null)
                {
                    this.attachControl.Resize -= new EventHandler(RepositionForm);
                    this.attachControl.Layout -= new LayoutEventHandler(LayoutRepositionForm);
                }

                this.attachControl = value;
                
                if (this.attachControl != null)
                {
                    this.attachControl.Resize += new EventHandler(RepositionForm);
                    this.attachControl.Layout += new LayoutEventHandler(LayoutRepositionForm);
                }
            }
        }

        // Used to force the floater to reposition
        // This should only be used when you click windows -> Reset window locations
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
        // Should only be set in DocumentWorkspace.cs, and should only be set once on initialization
        // After that, the rest of the logic is taken care of in here
        public WhichEdge SnapToEdge
        {
            get
            {
                return snapToEdge;
            }

            set
            { 
                // Only allow the floaters to snap if they have focus.. Unless it is
                // a forced command by windows -> reset window locations
                if (snapToEdge != value && !this.Focused)
                {
                    if (!repositionMe)
                    {
                        return;
                    }
                }

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

            this.ControlAdded += controlAddedDelegate; // we don't override OnControlAdded so we can re-use the method (see code below for ControlAdded)
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }
            base.Dispose(disposing);
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
            Point myLocation = new Point(x, y);

            // All the points where the form can snap to
            Rectangle attachedRect = AttachControl.ClientRectangle2;

            Point topLeft = AttachControl.PointToScreen(new Point(attachedRect.Left, attachedRect.Top));            
            if (Math.Abs(myLocation.X - topLeft.X) <= snapThreshold &&
                Math.Abs(myLocation.Y - topLeft.Y) <= snapThreshold)
            {
                return WhichEdge.TopLeft;
            }

            Point bottomLeft = AttachControl.PointToScreen(new Point(attachedRect.Left, attachedRect.Bottom - this.Height));
            if (Math.Abs(myLocation.X - bottomLeft.X) <= snapThreshold &&
                Math.Abs(myLocation.Y - bottomLeft.Y) <= snapThreshold)
            {
                return WhichEdge.BottomLeft;
            }

            Point topRight = AttachControl.PointToScreen(new Point(attachedRect.Right - this.Width, attachedRect.Top));
            if (Math.Abs(myLocation.X - topRight.X) <= snapThreshold &&
                Math.Abs(myLocation.Y - topRight.Y) <= snapThreshold)
            {
                return WhichEdge.TopRight;
            }

            Point bottomRight = AttachControl.PointToScreen(new Point(attachedRect.Right - this.Width, attachedRect.Bottom - this.Height ));
            if (Math.Abs(myLocation.X - bottomRight.X) <= snapThreshold &&
                Math.Abs(myLocation.Y - bottomRight.Y) <= snapThreshold)
            {
                return WhichEdge.BottomRight;
            }

            Point left = AttachControl.PointToScreen(new Point(attachedRect.Left, attachedRect.Top + (attachedRect.Height / 2) - (this.Height / 2)));
            Point bottom = AttachControl.PointToScreen(new Point(attachedRect.Left + (attachedRect.Width / 2) - (this.Width / 2), attachedRect.Bottom  - this.Height));
            Point top = AttachControl.PointToScreen(new Point(attachedRect.Left + (attachedRect.Width / 2) - (this.Width / 2), attachedRect.Top));
            Point right = AttachControl.PointToScreen(new Point(attachedRect.Right - this.Width, attachedRect.Top + (attachedRect.Height / 2) - (this.Height / 2)));

            int testTop = attachedRect.Top;
            int testBottom = attachedRect.Bottom;
            int testLeft = attachedRect.Left;
            int testRight = attachedRect.Right;

            if (Math.Abs(myLocation.Y - top.Y) <= snapThreshold &&
                myLocation.X > testLeft && 
                myLocation.X < testRight)
            {
                return WhichEdge.Top;
            }

            if (Math.Abs(myLocation.X - left.X) <= snapThreshold &&
                myLocation.Y > testTop && 
                myLocation.Y < testBottom)
            {
                return WhichEdge.Left;
            }

            if (Math.Abs(myLocation.X - right.X) <= snapThreshold &&
                myLocation.Y > testTop && 
                myLocation.Y < testBottom)
            {
                return WhichEdge.Right;
            }

            if (Math.Abs(myLocation.Y - bottom.Y) <= snapThreshold &&
                myLocation.X > testLeft && 
                myLocation.X < testRight)
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
            Point topLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3, 
                                                                  AttachControl.ClientRectangle2.Top + 3));

            return topLeft;
        }

        private Point GetTop()
        {
            int testLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left,
                                                                 AttachControl.ClientRectangle2.Top )).X;

            int testRight = new Point(AttachControl.ClientRectangle2.Right, 
                                      AttachControl.ClientRectangle2.Bottom).X;

            Point top = AttachControl.PointToScreen(new Point(0, AttachControl.ClientRectangle2.Top + 3));
                    
            if (this.Left < testLeft)
            {
                top.X = testLeft;
            }
            else if (this.Left > testRight)
            {
                top.X = testRight;
            }
            else
            {
                top.X = this.Location.X;
            }

            // Do some offset checking, make sure it moves with the form properly
            if (this.leftXOffset > 0)
            {
                top = new Point(AttachControl.PointToScreen(new Point(this.leftXOffset, 0)).X, top.Y);
            }

            // Keep it bounded
            Point topRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 
                                                                   AttachControl.ClientRectangle2.Top + 3));

            if (top.X >= topRight.X)
            {
                top = topRight;
            }

            return top;
        }

        private Point GetBottom()
        {
            int testLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left,
                                                                 AttachControl.ClientRectangle2.Top )).X;

            int testRight = new Point(AttachControl.ClientRectangle2.Right,
                                      AttachControl.ClientRectangle2.Bottom).X;

            Point bottom = AttachControl.PointToScreen(new Point(0, AttachControl.ClientRectangle2.Bottom - 3 - this.Height));
                    
            if (this.Left < testLeft)
            {
                bottom.X = testLeft;
            }
            else if (this.Left > testRight)
            {
                bottom.X = testRight;
            }
            else
            {
                bottom.X = this.Location.X;
            }

            // Do some offset checking, make sure it moves with the form properly
            if (this.leftXOffset > 0)
            {
                bottom = new Point(AttachControl.PointToScreen(new Point(this.leftXOffset,0)).X, bottom.Y);
            }

            // Keep it bounded
            Point bottomRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 
                                                                      AttachControl.ClientRectangle2.Bottom - this.Height - 3));

            if (bottom.X >= bottomRight.X)
            {
                bottom = bottomRight;
            }

            return bottom;
        }

        private Point GetTopRight()
        {
            Point topRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, AttachControl.ClientRectangle2.Top + 3));
            return topRight;
        }

        private Point GetRight()
        {
            int testTop = AttachControl.ClientRectangle2.Location.Y;
            int testRight= new Point(AttachControl.ClientRectangle2.Left,AttachControl.ClientRectangle2.Bottom).Y;
            Point right = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 0));
                    
            if (this.Location.Y < testTop)
            {
                right.Y = testTop;
            }
            else if (this.Location.Y > testRight)
            {
                right.Y = testRight;
            }
            else
            {
                right.Y = this.Location.Y;
            }

            // Do some offset checking, make sure it moves with the form properly
            if (this.upperYOffset > 0)
            {
                right = new Point(right.X, AttachControl.PointToScreen(new Point(0, this.upperYOffset)).Y);
            }

            // Keep it bounded
            Point bottomRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 
                                                                      AttachControl.ClientRectangle2.Bottom - this.Height - 3));

            if (right.Y > bottomRight.Y)
            {
                right = bottomRight;
            }

            return right;
        }

        private Point GetBottomRight()
        {
            Point bottomRight = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Right - this.Width - 3, 
                                                                      AttachControl.ClientRectangle2.Bottom - this.Height - 3));
            return bottomRight;
        }

        private Point GetBottomLeft()
        {
            Point bottomLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3, 
                                                                     AttachControl.ClientRectangle2.Bottom - this.Height - 3));
            return bottomLeft;
        }

        private Point GetLeft()
        {
            int testTop = AttachControl.ClientRectangle2.Location.Y;
            int testBottom = new Point(AttachControl.ClientRectangle2.Left, AttachControl.ClientRectangle2.Bottom).Y;
            Point left = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3, 0));
                    
            if (this.Location.Y < testTop)
            {
                left.Y = testTop;
            }
            else if (this.Location.Y > testBottom)
            {
                left.Y = testBottom;
            }
            else
            {
                left.Y = this.Location.Y;
            }

            // Do some offset checking, make sure it moves with the form properly
            if (this.upperYOffset > 0)
            {
                left = new Point(left.X, AttachControl.PointToScreen(new Point(0, this.upperYOffset)).Y);
            }

            // Keep it bounded
            Point bottomLeft = AttachControl.PointToScreen(new Point(AttachControl.ClientRectangle2.Left + 3, 
                                                                     AttachControl.ClientRectangle2.Bottom - this.Height - 3));

            if (left.Y > bottomLeft.Y)
            {
                left = bottomLeft;
            }

            return left;
        }

        protected override void OnMoving(MovingEventArgs mea)
        {
            base.OnMoving(mea);

            // If it is in area to where the rectangle wants to move
            WhichEdge where = InArea(mea.Rectangle.X,mea.Rectangle.Y);

            Point placeMe = new Point(mea.Rectangle.X,mea.Rectangle.Y);

            // Set the offsets to zero
            this.upperYOffset = this.leftXOffset = 0;

            switch(where)
            {
                case WhichEdge.TopLeft:
                    placeMe = GetTopLeft();
                    break;

                case WhichEdge.Top:
                    placeMe = GetTop();
                    this.leftXOffset = placeMe.X = mea.Rectangle.X;
                    break;

                case WhichEdge.TopRight:
                    placeMe = GetTopRight();
                    break;

                case WhichEdge.Right:
                    placeMe = GetRight();
                    this.upperYOffset = placeMe.Y = mea.Rectangle.Y;
                    break;

                case WhichEdge.BottomRight:
                    placeMe = GetBottomRight();
                    break;

                case WhichEdge.Bottom:
                    placeMe = GetBottom();
                    this.leftXOffset = placeMe.X = mea.Rectangle.X;
                    break;

                case WhichEdge.BottomLeft:
                    placeMe = GetBottomLeft();
                    break;

                case WhichEdge.Left:
                    placeMe = GetLeft();
                    this.upperYOffset = placeMe.Y = mea.Rectangle.Y;
                    break;

                case WhichEdge.None:
                    break;          
            }

            // Convert both of them to client coords, might as well do it at the same time, yea?
            Point temp = AttachControl.PointToClient(new Point(leftXOffset,upperYOffset));
            leftXOffset = temp.X;
            upperYOffset = temp.Y;

            Rectangle rect = new Rectangle(placeMe.X,placeMe.Y,this.Width,this.Height);
            this.snapToEdge = where;
            mea.Rectangle = rect;
        }

        public void LayoutRepositionForm(object sender, LayoutEventArgs e)
        {
            //RepositionForm(sender, e);
        }

        public void RepositionForm(object sender, EventArgs e)
        {
            if (this.snapToEdge != WhichEdge.None)
            {
                this.SnapToEdge = this.snapToEdge;
            }
        }
    }
}
