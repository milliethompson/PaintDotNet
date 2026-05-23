/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FillTool.
    /// </summary>
    abstract public class FloodTool
        : Tool
    {
        private bool limitToSelection = true;
        protected bool LimitToSelection
        {
            get 
            {
                return limitToSelection;
            }
            set
            {
                limitToSelection = value;
            }
        }

        public FloodTool(DocumentWorkspace workspace,
            Image toolBarImage,
            string name,
            string description,
            string helpText,
            char hotKey)
            : base(workspace, toolBarImage, name, description, helpText, hotKey)
        {
        }


        private static bool CheckColor(ColorBgra a, ColorBgra b, int tolerance)
        {
            int sum = 0, diff;

            diff = a.R - b.R;
            sum += (1 + diff * diff) * a.A / 256;

            diff = a.G - b.G;
            sum += (1 + diff * diff) * a.A / 256;

            diff = a.B - b.B;
            sum += (1 + diff * diff) * a.A / 256;

            diff = a.A - b.A;
            sum += diff * diff;

            return (sum <= tolerance * tolerance * 4);
        }
        
        public unsafe static void FillStencilFromPoint(Surface surface, IBitVector2D stencil, Point start, int tolerance, out Rectangle boundingBox, PdnRegion limitRegion, bool limitToSelection)
        {
            ColorBgra cmp = surface[start];
            int added;
            int length = 0;
			int top = int.MaxValue;
			int bottom = int.MinValue;
			int left = int.MaxValue;
			int right = int.MinValue;
			Rectangle[] scans;
            
			stencil.Clear(false);
            if (limitToSelection)
            {
                using (PdnRegion excluded = new PdnRegion(new Rectangle(0, 0, stencil.Width, stencil.Height)))
                {
                    excluded.Xor(limitRegion);
                    scans = excluded.GetRegionScansReadOnlyInt();
                }
            }
            else
            {
                scans = new Rectangle[0];
            }

			foreach (Rectangle rect in scans)
			{
				stencil.Set(rect, true);
			}


			Queue queue = new Queue(1024, 4.0f);
			queue.Enqueue(start);

            do
            {
                length = queue.Count;
                added = 0;

                while (length-- > 0)
                {
                    Point pt = (Point)queue.Dequeue();

                    if (!stencil.GetUnchecked(pt.X, pt.Y) && 
                        CheckColor(cmp, *surface.GetPointAddressUnchecked(pt), tolerance))
                    {
                        stencil.SetUnchecked(pt.X, pt.Y, true);

                        if (pt.X > 0)
                        {
					        if (pt.X - 1 < left)
					        {
						        left = pt.X - 1;
					        }
                            
					        added++;
                            queue.Enqueue(new Point(pt.X - 1, pt.Y));
                        }

                        if (pt.Y > 0)
                        {
					        if (pt.Y - 1 < top)
					        {
						        top = pt.Y - 1;
					        }

                            added++;
                            queue.Enqueue(new Point(pt.X, pt.Y - 1));
                        }

                        if (pt.X < surface.Width - 1)
                        {
					        if (pt.X + 1 > right)
					        {
						        right = pt.X + 1;
					        }

                            added++;
                            queue.Enqueue(new Point(pt.X + 1, pt.Y));
                        }

                        if (pt.Y < surface.Height - 1)
                        {
					        if (pt.Y + 1 > bottom)
					        {
						        bottom = pt.Y + 1;
					        }

                            added++;
                            queue.Enqueue(new Point(pt.X, pt.Y + 1));
                        }

                        if (pt.X < left)
                        {
                            left = pt.X;
                        }
        				
				        if (pt.X > right)
                        {
                            right = pt.X;
                        }

                        if (pt.Y < top)
                        {
					        top = pt.Y;
                        }
        				
				        if (pt.Y > bottom)
                        {
					        bottom = pt.Y;
                        }
                    }
                }
            } while (added > 0);

            foreach (Rectangle rect in scans)
            {
				stencil.Set(rect, false);
            }

			boundingBox = Rectangle.FromLTRB(left, top, right + 1, bottom + 1);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point pos = new Point(e.X, e.Y);

            if (Utility.IsPointInRectangle(pos, Workspace.Document.Bounds))
            {
                base.OnMouseDown (e);
                PdnRegion currentRegion;

                // Create the Current Region
                if (!Workspace.Environment.IsSelectionEmpty)
                {
                    currentRegion = Workspace.Environment.CreateSelectedRegion();
                }
                else
                {
                    currentRegion = new PdnRegion();
                    currentRegion.MakeInfinite();
                }

                currentRegion.Intersect(Workspace.Document.Bounds);

                // See if the mouse click is valid
                if (!currentRegion.IsVisible(pos) && limitToSelection)
                {
                    currentRegion.Dispose();
                    currentRegion = null;
                    return;
                }           
            
                // Set the current surface, color picked and color to draw
                Surface surface = ((BitmapLayer)Workspace.ActiveLayer).Surface;

                IBitVector2D stencilBuffer = new BitVector2DSurfaceAdapter(this.ScratchSurface);

                Rectangle boundingBox;
                int tolerance = (int)(Workspace.Environment.Tolerance * Workspace.Environment.Tolerance * 256);
                FillStencilFromPoint(surface, stencilBuffer, pos, tolerance, out boundingBox, currentRegion, limitToSelection);
                
                using (PdnGraphicsPath fillPerimeter = PdnGraphicsPath.PathFromStencil(stencilBuffer, boundingBox))
                {
                    PerimeterFound(fillPerimeter);

                    using (PdnRegion fillRegion = new PdnRegion(fillPerimeter))
                    {
                        RegionSelected(fillRegion, boundingBox);
                    }
                }
            }

            base.OnMouseDown(e);
        }

        protected virtual void RegionSelected(PdnRegion fillRegion, Rectangle boundingBox)
        {
        }

        protected virtual void PerimeterFound(PdnGraphicsPath path)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();
            }
        }
    }
}
