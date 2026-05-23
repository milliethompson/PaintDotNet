/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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

            this.Dock = DockStyle.Fill;
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

        protected override void OnPaint(PaintEventArgs pe)
        {
            // Calling the base class OnPaint
            base.OnPaint(pe);
            DrawShadow(pe.Graphics, pe.ClipRectangle, false);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //base.OnPaintBackground (pevent);
            DrawShadow(pevent.Graphics, pevent.ClipRectangle, true);
        }

        private void DrawShadow(Graphics g, Rectangle clipRect, bool drawBackground)
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

                Rectangle shadowRect = outlineRect;
                shadowRect.Width -= 1;
                shadowRect.Height -= 1;
                shadowRect.X += 1;
                shadowRect.Y += 1;

                Rectangle[] rects = new Rectangle[] { new Rectangle(shadowRect.Right, shadowRect.Top + 3, 4, shadowRect.Height + 1),
                                                      new Rectangle(shadowRect.Left + 2, shadowRect.Bottom, shadowRect.Width + 1, 4) };

                Color shadowColor = Color.FromArgb(this.BackColor.R / 2, this.BackColor.G / 2, this.BackColor.B / 2);
                PdnGraphics.FillRectangles(g, shadowColor, rects);

                Point[] polygon = new Point[] { 
                                                  new Point(outlineRect.Left, outlineRect.Top),
                                                  new Point(outlineRect.Right, outlineRect.Top),
                                                  new Point(outlineRect.Right, outlineRect.Bottom),
                                                  new Point(outlineRect.Left, outlineRect.Bottom),
                                                  new Point(outlineRect.Left, outlineRect.Top)
                                              };
                
                PdnGraphics.DrawPolyLine(g, Color.Black, polygon);

                if (drawBackground)
                {
                    using (PdnRegion backRegion = new PdnRegion(clipRect))
                    {
                        foreach (Rectangle rect in rects)
                        {
                            backRegion.Exclude(rect);
                        }

                        for (int i = 0; i < polygon.Length - 1; ++i)
                        {
                            Rectangle rect = Rectangle.FromLTRB(polygon[i].X, polygon[i].Y,
                                                                polygon[i + 1].X + 1, polygon[i + 1].Y + 1);

                            backRegion.Exclude(rect);
                        }

                        Rectangle[] backRects = backRegion.GetRegionScansReadOnlyInt();
                        PdnGraphics.FillRectangles(g, this.BackColor, backRects);
                    }
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            // Ignore focus
            if (m.Msg == 7 /* WM_SETFOCUS */)
            {
                return;
            }

            base.WndProc (ref m);
        }
    }
}
