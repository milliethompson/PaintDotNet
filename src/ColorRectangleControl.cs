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
    /// <summary>
    /// Summary description for ColorRectangleControl.
    /// </summary>
    public class ColorRectangleControl : System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Bitmap renderSurface = null;

        private Color rectangleColor;
        public Color RectangleColor
        {
            get
            {
                return rectangleColor;
            }

            set
            {
                rectangleColor = value;
                InvalidateRenderSurface();
                Invalidate();
            }
        }

        public ColorRectangleControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ResizeRedraw = true;
        }

        private void DrawColorRectangle(Graphics g, Rectangle rect, Color color)
        {
            Rectangle colorRectangle = Rectangle.Inflate(rect, -2, -2);
            Brush colorBrush = new LinearGradientBrush(colorRectangle, Color.FromArgb(255, color), color, 90.0f, false);
            HatchBrush backgroundBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.FromArgb(128, 128, 128), Color.FromArgb(192, 192, 192));

            g.DrawRectangle(Pens.Black, 0, 0, rect.Width - 1, rect.Height - 1);
            g.DrawRectangle(Pens.White, 1, 1, rect.Width - 3, rect.Height - 3);

            g.FillRectangle(backgroundBrush, colorRectangle);
            g.FillRectangle(colorBrush, colorRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);
            InvalidateRenderSurface();
        }


        private void RedoRenderSurface()
        {
            InvalidateRenderSurface();

            renderSurface = new Bitmap(this.Width, this.Height);
            using (Graphics g = Graphics.FromImage(renderSurface))
            {
                DrawColorRectangle(g, this.ClientRectangle, rectangleColor);
            }
        }

        private void InvalidateRenderSurface()
        {
            if (renderSurface != null)
            {
                renderSurface.Dispose();
                renderSurface = null;
            }
        }

        private void HandleRenderSurface()
        {
            if (renderSurface == null ||
                renderSurface.Size != this.Size ||
                renderSurface.GetPixel(renderSurface.Width / 2, renderSurface.Height / 2) != rectangleColor)
            {
                RedoRenderSurface();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            HandleRenderSurface();
            e.Graphics.DrawImage(renderSurface, 0, 0, renderSurface.Width, renderSurface.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground(pevent); // do not call to avoid flickering
            HandleRenderSurface();
            pevent.Graphics.DrawImage(renderSurface, 0, 0, renderSurface.Width, renderSurface.Height);
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
