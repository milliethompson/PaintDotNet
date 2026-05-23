/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for IconBox.
    /// </summary>
    public class IconBox : 
        System.Windows.Forms.UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private Bitmap renderSurface = null;
        private Bitmap icon = null;
        private Color transparentColor;

        public Color TransparentColor
        {
            get
            {
                return transparentColor;
            }

            set
            {
                transparentColor = value;

                if (renderSurface != null)
                {
                    renderSurface.Dispose();
                }

                renderSurface = null;
                Invalidate();
            }
        }

        public Bitmap Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;

                if (renderSurface != null)
                {
                    renderSurface.Dispose();
                }

                renderSurface = null;
                Invalidate();
            }
        }

        private void DoRenderSurface()
        {
            if (renderSurface != null)
            {
                renderSurface.Dispose();
            }

            renderSurface = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);

            using (Graphics g = Graphics.FromImage(renderSurface))
            {
                g.Clear(this.BackColor);
            }

            if (icon != null)
            {
                for (int y = 0; y < icon.Height; ++y)
                {
                    for (int x = 0; x < icon.Width; ++x)
                    {
                        Color c = icon.GetPixel(x, y);

                        if (c != transparentColor)
                        {
                            renderSurface.SetPixel(x, y, c);
                        }
                    }
                }
            }
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged (e);

            if (renderSurface != null)
            {
                renderSurface.Dispose();
            }

            renderSurface = null;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            if (renderSurface != null)
            {
                renderSurface.Dispose();
            }

            renderSurface = null;
        }

        private void DoDraw(Graphics g)
        {
            if (renderSurface == null)
            {
                DoRenderSurface();
            }

            g.DrawImage(renderSurface, ClientRectangle, ClientRectangle, GraphicsUnit.Pixel);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint (e);
            DoDraw(e.Graphics);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            DoDraw(pevent.Graphics);
        }


        public IconBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ResizeRedraw = true;
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
