/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class ColorGradientControl 
        : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private int tracking = -1;
        private int highlight = -1;

        private const int triangleSize = 7;
        private const int triangleSides = (triangleSize - 1) / 2;

        private int[] vals;

        // value from [0,255] that specifies the hsv "value" component
        // where we should draw little triangles that show the value
        public int Value 
        {
            get 
            {
                return GetValue(0);
            }

            set
            {
                SetValue(0, value);
            }
        }

        public int Count
        {
            get 
            {
                return vals.Length;
            }

            set 
            {
                if (value < 0 || value > 16) 
                {
                    throw new ArgumentOutOfRangeException("value", value, "Count must be between 0 and 16");
                }

                vals = new int[value];

                if (value > 1) 
                {
                    for (int i = 0; i < value; i++) 
                    {
                        vals[i] = i * 255 / (value - 1);
                    }
                } 
                else if (value == 1) 
                {
                    vals[0] = 128;
                }

                OnValueChanged(0);
                Invalidate();
            }
        }

        public int GetValue(int index) 
        {
            if (index < 0 || index >= vals.Length) 
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }

            return vals[index];
        }

        public void SetValue(int index, int val)
        {
            int min = -1;
            int max = 256;

            if (index < 0 || index >= vals.Length) 
            {
                throw new ArgumentOutOfRangeException("index", index, "Index must be within the bounds of the array");
            }

            if (index - 1 >= 0) 
            {
                min = vals[index - 1];
            }

            if (index + 1 < vals.Length) 
            {
                max = vals[index + 1];
            }

            if (vals[index] != val) 
            {
                vals[index] = Utility.Clamp(val, min+1, max-1);
                OnValueChanged(index);
                Invalidate();
            }

            Update();
        }

        public event IndexEventHandler ValueChanged;
        protected virtual void OnValueChanged(int index)
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, new IndexEventArgs(index));
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
                if (topColor != value)
                {
                    topColor = value;
                    Invalidate();
                }
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
                if (bottomColor != value)
                {
                    bottomColor = value;
                    Invalidate();
                }
            }
        }

        public ColorGradientControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ResizeRedraw = true;
            this.Count = 1;
        }

        private void DrawGradient(Graphics g)
        {
            Rectangle gradientRect;

            // draw gradient
            using (LinearGradientBrush lgb = new LinearGradientBrush(this.ClientRectangle, 
                       topColor, bottomColor, 90, false))
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
                    g.FillRegion(sb, nonGradientRegion.GetRegionReadOnly());
                }
            }

            // draw value triangles
            for (int i = 0; i < vals.Length; i++)
            {
                int valueY = ValueToPosition(vals[i]);
                Brush brush;

                if (i == highlight) 
                {
                    brush = Brushes.Blue;
                } 
                else 
                {
                    brush = Brushes.Black;
                }

                g.SmoothingMode = SmoothingMode.AntiAlias;

                Point a1 = new Point(0, valueY - triangleSides);
                Point b1 = new Point(triangleSize - 1, valueY);
                Point c1 = new Point(0, valueY + triangleSides);
                g.FillPolygon(brush, new Point[] { a1, b1, c1, a1 });

                Point a2 = new Point(Width - 1 - a1.X, a1.Y);
                Point b2 = new Point(Width - 1 - b1.X, b1.Y);
                Point c2 = new Point(Width - 1 - c1.X, c1.Y);
                g.FillPolygon(brush, new Point[] { a2, b2, c2, a2 });
            }
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

        int PositionToValue(int position)
        {
            return (((Height - triangleSize) - (position - triangleSides)) * 255) / (Height - triangleSize);
        }

        int ValueToPosition(int val)
        {
            return triangleSides + ((Height - triangleSize) - (((val * (Height - triangleSize)) / 255)));
        }

        private int WhichTriangle(int yval) 
        {
            int bestIndex = -1, bestDistance = int.MaxValue;
            int y = PositionToValue(yval);

            for (int i = 0; i < vals.Length; i++) 
            {
                int distance = Math.Abs(vals[i] - y);
                if (distance < bestDistance) 
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if (e.Button == MouseButtons.Left)
            {
                tracking = WhichTriangle(e.Y);
                Invalidate();
                OnMouseMove(e);
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
            {
                OnMouseMove(e);
                tracking = -1;
                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);

            if (tracking >= 0)
            {
                this.SetValue(tracking, PositionToValue(e.Y));
            }
            else 
            {
                int oldHighlight = highlight;
                highlight = WhichTriangle(e.Y);

                if (highlight != oldHighlight) 
                {
                    this.InvalidateTriangle(oldHighlight);
                    this.InvalidateTriangle(highlight);
                }
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            int oldhighlight = highlight;
            highlight = -1;
            this.InvalidateTriangle(oldhighlight);
        }

        private void InvalidateTriangle(int index) 
        {
            if (index < 0 || index >= vals.Length) 
            {
                return;
            }

            int valueY = ValueToPosition(vals[index]);
            Rectangle rect = new Rectangle(0, valueY - triangleSides, this.Width, triangleSize);

            this.Invalidate(rect, true);
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
