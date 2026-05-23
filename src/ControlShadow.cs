using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ControlShadow.
    /// </summary>
    public class ControlShadow 
        : System.Windows.Forms.Control
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Control occludingControl; 

        [Browsable(false)]
        public Control OccludingControl
        {
            get 
            { 
                return occludingControl; 
            }

            set 
            { 
                occludingControl = value;
                Invalidate();
            }
        }

        public ControlShadow()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitComponent call
            this.Dock = DockStyle.Fill;
            this.ResizeRedraw = true;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if ( components != null )
                    components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);
            DrawShadow(pe.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground (pevent);
            DrawShadow(pevent.Graphics);
        }

        private void DrawShadow(Graphics g)
        {
            if (occludingControl != null)
            {
                // figure out where the outline rectangle should go
                Rectangle outlineRect = new Rectangle(new Point(0, 0), occludingControl.Size);
                outlineRect = occludingControl.RectangleToScreen(outlineRect);
                outlineRect = RectangleToClient(outlineRect);
                outlineRect.X -= 1;
                outlineRect.Y -= 1;
                outlineRect.Width += 1;
                outlineRect.Height += 1;

                // draw a shadow beyond that rectangle
                // Actually I don't really like how it looks, so *don't* draw it.
                Color shadowColor = Color.FromArgb(this.BackColor.R / 2, this.BackColor.G / 2, this.BackColor.B / 2);

                using (Brush shadowBrush = new SolidBrush(shadowColor))
                {
                    Rectangle shadowRect = outlineRect;
                    shadowRect.Width -= 1;
                    shadowRect.Height -= 1;
                    shadowRect.X += 1;
                    shadowRect.Y += 1;
                    g.FillRectangle(shadowBrush, shadowRect.Right, shadowRect.Top + 3, 4, shadowRect.Height + 1);
                    g.FillRectangle(shadowBrush, shadowRect.Left + 2, shadowRect.Bottom, shadowRect.Width + 1, 4);
                }

                // draw the outline
                g.DrawRectangle(Pens.Black, outlineRect);
            }
        }

        private sealed class NativeMethods
        {
            internal sealed class WmConstants
            {
                public static int WM_SETFOCUS = 7;

                private WmConstants()
                {
                }
            }

            private NativeMethods()
            {
            }
        }

        protected override void WndProc(ref Message m)
        {
            IntPtr preR = m.Result;

            // Ignore focus
            if (m.Msg == NativeMethods.WmConstants.WM_SETFOCUS)
            {
                return;
            }

            base.WndProc (ref m);
        }
    }
}
