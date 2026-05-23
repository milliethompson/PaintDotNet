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
    /// Summary description for ColorGradientControl.
    /// </summary>
    public class ColorGradientControl 
        : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private bool tracking = false;

        private const int triangleSize = 5;
        private const int triangleSides = (triangleSize - 1) / 2;

        // value from [0,255] that specifies the hsv "value" component
        // where we should draw little triangles that show the value
        private int value;
        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                int newValue = Math.Min(255, Math.Max(0, value));

                if (this.value != newValue)
                {
                    this.value = newValue;
                    OnValueChanged();
                    Invalidate();
                    Update();
                }
            }
        }

        public event EventHandler ValueChanged;
        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        private Color topColor;
        public Color TopColor
        {
            get
            {
                return topColor;
            }

            set
            {
                topColor = value;
                Invalidate();
            }
        }

        private Color bottomColor;
        public Color BottomColor
        {
            get
            {
                return bottomColor;
            }
            
            set
            {
                bottomColor = value;
                Invalidate();
            }
        }

        public ColorGradientControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
            this.ResizeRedraw = true;
        }

        private void DrawGradient(Graphics g)
        {
            Rectangle gradientRect;

            // draw gradient
            using (LinearGradientBrush lgb = new LinearGradientBrush(this.ClientRectangle, topColor, bottomColor, 90, false))
            {
                gradientRect = ClientRectangle;
                gradientRect.Inflate(-triangleSize, -triangleSides);
                g.FillRectangle(lgb, gradientRect);
            }

            // fill background
            using (PdnRegion nonGradientRegion = new PdnRegion())
            {
                nonGradientRegion.MakeInfinite();
                nonGradientRegion.Exclude(gradientRect);

                using (SolidBrush sb = new SolidBrush(this.BackColor))
                {
                    g.FillRegion(sb, nonGradientRegion);
                }
            }

            // draw value triangles
            int valueY = triangleSides + ((Height - triangleSize) - (((value * (Height - triangleSize)) / 255)));

            g.SmoothingMode = SmoothingMode.AntiAlias;

            Point a1 = new Point(0, valueY - triangleSides);
            Point b1 = new Point(triangleSize - 1, valueY);
            Point c1 = new Point(0, valueY + triangleSides);
            g.DrawLines(Pens.Black, new Point[] { a1, b1, c1, a1 });

            Point a2 = new Point(Width - 1 - a1.X, a1.Y);
            Point b2 = new Point(Width - 1 - b1.X, b1.Y);
            Point c2 = new Point(Width - 1 - c1.X, c1.Y);
            g.DrawLines(Pens.Black, new Point[] { a2, b2, c2, a2 });
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            DrawGradient(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DrawGradient(pevent.Graphics);
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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if (e.Button == MouseButtons.Left)
            {
                tracking = true;
                OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                OnMouseMove(e);
                tracking = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (tracking)
            {
                this.Value = (((Height - triangleSize) - (e.Y - triangleSides)) * 255) / (Height - triangleSize);
            }
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
    }
}
