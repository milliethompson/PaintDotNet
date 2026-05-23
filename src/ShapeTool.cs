/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Allows the user to draw a shape that can be defined using two points on the canvas.
    /// The user clicks and drags between two points to define the area that bounds the shape.
    /// </summary>
    public abstract class ShapeTool
        : Tool
    {
        private bool mouseDown;
        private MouseButtons mouseButton;
        private BitmapLayer bitmapLayer;
        private RenderArgs renderArgs;
        private IrregularSurface interiorSaveSurface;
        private IrregularSurface outlineSaveSurface;
        private ArrayList points;
		private PdnRegion lastDrawnRegion = null;
		private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;

        protected override bool SupportsInk
        {
            get
            {
                return true;
            }
        }

        // This is for shapes that should only be draw in one ShapeDrawType
        // The line shape, for instance, should only ever be drawn in ShapeDrawType.Outline
        private bool forceShapeType = false;
        public bool ForceShapeDrawType
        {
            get
            {
                return forceShapeType;
            }

            set
            {
                forceShapeType = value;
            }
        }

        private ShapeDrawType forcedShapeDrawType = ShapeDrawType.Both;
        public ShapeDrawType ForcedShapeDrawType
        {
            get
            {
                return forcedShapeDrawType;
            }

            set
            {
                forcedShapeDrawType = value;
            }
        }


        /// <summary>
        /// Different shapes may not require all the points given to them, and as such
        /// if the user is drawing for a long time there may be lots of memory that's
        /// allocated that doesn't need to be. So before CreateShapePath is called,
        /// this method is called first.
        /// For example, the LineTool would return a new array containing only the
        /// first and last points.
        /// It is ok to return the same array that was passed in, even if it is modified.
        /// </summary>
        /// <param name="points">An ArrayList containing PointF instances.</param>
        /// <returns></returns>
        protected virtual ArrayList TrimShapePath(ArrayList points)
        {
            return points;
        }

        /// <summary>
        /// Override this function to return an "optimized" region that encompasses 
        /// the shape's outline. For example, a circle would return a list of rectangles
        /// that traces the outline. This is necessary because normally simplification
        /// will produce a region that, for a circle's outline, encompasses its
        /// interior as well. If you return null, then the default simplification
        /// algorithm will be used.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual RectangleF[] GetOptimizedShapeOutlineRegion(PointF[] points, PdnGraphicsPath path)
        {
            return null;
        }

        // Implement this!
        protected abstract PdnGraphicsPath CreateShapePath(PointF[] points);

        protected override void OnActivate()
        {
            base.OnActivate();

            outlineSaveSurface = null;
            interiorSaveSurface = null;

            // creates a bitmap layer from the active layer
            bitmapLayer = (BitmapLayer)Workspace.ActiveLayer;

            // create Graphics object
            renderArgs = new RenderArgs(bitmapLayer.Surface);

            lastDrawnRegion = new PdnRegion();
            lastDrawnRegion.MakeEmpty();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();

            if (mouseDown)
            {
                PointF lastPoint = (PointF)points[points.Count - 1];
                OnStylusUp(new StylusEventArgs(mouseButton, 0, lastPoint.X, lastPoint.Y, 0));
            }

            bitmapLayer = null;

            if (renderArgs != null)
            {
                renderArgs.Dispose();
            }

            renderArgs = null;

            if (outlineSaveSurface != null)
            {
                outlineSaveSurface.Dispose();
            }

            outlineSaveSurface = null;

            if (interiorSaveSurface != null)
            {
                interiorSaveSurface.Dispose();
            }

            interiorSaveSurface = null;

            points = null;
        }

        protected override void OnStylusDown(StylusEventArgs  e)
        {
            base.OnStylusDown(e);

			cursorMouseUp = Cursor;
			Cursor = cursorMouseDown;

            if (mouseDown)
            {
                return;
            }

            if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                mouseDown = true;
                mouseButton = e.Button;

                PdnRegion clipRegion = null;
                
                if (!Workspace.Environment.IsSelectionEmpty)
                {
                    clipRegion = Workspace.Environment.CreateSelectedRegion();
                }
                else
                {
                    clipRegion = new PdnRegion(Workspace.Document.Bounds);
                }

                renderArgs.Graphics.SetClip(clipRegion, CombineMode.Replace);
                clipRegion.Dispose();

                // reset the points we're drawing!
                points = new ArrayList();

                OnStylusMove(e);
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove (e);

            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                PointF mouseXY = new PointF(e.Fx, e.Fy);
                points.Add(mouseXY);
            }
        }

        public virtual PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.Half;
        }

        private void Render()
        {
            // create the Pen we will use to draw with
            Pen outlinePen = null;
            Brush interiorBrush = null;
            PenInfo pi = Workspace.Environment.PenInfo;
            BrushInfo bi = Workspace.Environment.BrushInfo;

            // Initialize pens and brushes to the correct colors
            if ((mouseButton & MouseButtons.Left) == MouseButtons.Left)
            {
                outlinePen = pi.CreatePen(Workspace.Environment.BrushInfo,
                    Workspace.Environment.ForeColor.ToColor(), Workspace.Environment.BackColor.ToColor());
                
                interiorBrush = bi.CreateBrush(Workspace.Environment.BackColor.ToColor(), Workspace.Environment.ForeColor.ToColor());
            }
            else if ((mouseButton & MouseButtons.Right) == MouseButtons.Right)
            {
                outlinePen = pi.CreatePen(Workspace.Environment.BrushInfo,
                    Workspace.Environment.BackColor.ToColor(), Workspace.Environment.ForeColor.ToColor());

                interiorBrush = bi.CreateBrush(Workspace.Environment.ForeColor.ToColor(), Workspace.Environment.BackColor.ToColor());
            }

            outlinePen.LineJoin = LineJoin.MiterClipped;
            outlinePen.MiterLimit = 2;

            // redraw the old saveSurface
            if (interiorSaveSurface != null)
            {
                interiorSaveSurface.Draw(bitmapLayer.Surface);
                bitmapLayer.Invalidate(interiorSaveSurface.Region);
                interiorSaveSurface.Dispose();
                interiorSaveSurface = null;
            }

            if (outlineSaveSurface != null)
            {
                outlineSaveSurface.Draw(bitmapLayer.Surface);
                bitmapLayer.Invalidate(outlineSaveSurface.Region);
                outlineSaveSurface.Dispose();
                outlineSaveSurface = null;
            }

            // anti-aliasing? Don't mind if I do
            if (Workspace.Environment.AntiAliasing)
            {
                renderArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            }
            else
            {
                renderArgs.Graphics.SmoothingMode = SmoothingMode.None;
            }

            // also set the pixel offset mode
            renderArgs.Graphics.PixelOffsetMode = GetPixelOffsetMode();

            // figure out how we're going to draw
            ShapeDrawType drawType;

            if (ForceShapeDrawType)
            {
                drawType = ForcedShapeDrawType;
            }
            else
            {
                drawType = Workspace.Environment.ShapeDrawType;
            }

            // get the region we want to save
            points = this.TrimShapePath(points);
            PointF[] pointsArray = (PointF[])points.ToArray(typeof(PointF));
            PdnGraphicsPath shapePath = CreateShapePath(pointsArray);

            if (shapePath != null)
            {
                // create non-optimized interior region
                PdnRegion interiorRegion = new PdnRegion(shapePath);

                // create non-optimized outline region
                PdnRegion outlineRegion;

                using (PdnGraphicsPath outlinePath = (PdnGraphicsPath)shapePath.Clone())
                {
                    outlinePath.Widen(outlinePen);
                    outlineRegion = new PdnRegion(outlinePath);
                }

                // create optimized outlineRegion for purposes of rendering, if it is possible to do so
                // shapes will often provide an "optimized" region that circumvents the fact that
                // we'd otherwise get a region that encompasses the outline *and* the interior, thus
                // slowing rendering significantly in many cases.
                RectangleF[] optimizedOutlineRegion = GetOptimizedShapeOutlineRegion(pointsArray, shapePath);
                PdnRegion invalidOutlineRegion;

                if (optimizedOutlineRegion != null)
                {
                    Utility.InflateRectanglesInPlace(optimizedOutlineRegion, (int)(outlinePen.Width + 2));
                    invalidOutlineRegion = Utility.RectanglesToRegion(optimizedOutlineRegion);
                }
                else
                {
                    invalidOutlineRegion = Utility.SimplifyAndInflateRegion(outlineRegion, Utility.DefaultSimplificationFactor, 2);
                }

                // create optimized interior region
                PdnRegion invalidInteriorRegion = Utility.SimplifyAndInflateRegion(interiorRegion, Utility.DefaultSimplificationFactor, 3);

                PdnRegion invalidRegion = new PdnRegion();
                invalidRegion.MakeEmpty();

                // set up alpha blending
                renderArgs.Graphics.CompositingMode = CompositingMode.SourceOver;

                outlineSaveSurface = new IrregularSurface(bitmapLayer.Surface, invalidOutlineRegion);
                if ((drawType & ShapeDrawType.Outline) != 0)
                {
                    renderArgs.Graphics.DrawPath(outlinePen, shapePath);
                }

                invalidRegion.Union(invalidOutlineRegion);

                // draw shape
                if ((drawType & ShapeDrawType.Interior) != 0)
                {
                    interiorSaveSurface = new IrregularSurface(bitmapLayer.Surface, invalidInteriorRegion);
                    renderArgs.Graphics.FillPath(interiorBrush, shapePath);
                    invalidRegion.Union(invalidInteriorRegion);
                }

                bitmapLayer.Invalidate(invalidRegion);
                invalidRegion.Dispose();

                invalidInteriorRegion.Dispose();
                invalidOutlineRegion.Dispose();
                outlineRegion.Dispose();
                interiorRegion.Dispose();
            }

            Workspace.Update();
            Utility.Dispose(shapePath);
            outlinePen.Dispose();
            interiorBrush.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button not down then leave function
            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                Render();
            }
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);

			Cursor = cursorMouseUp;

            if (mouseDown)
            {
                mouseDown = false;

                ArrayList has = new ArrayList();
                PdnRegion activeRegion;

                if (Workspace.Environment.IsSelectionEmpty)
                {
                    activeRegion = new PdnRegion();
                }
                else
                {
                    activeRegion = Workspace.Environment.CreateSelectedRegion();
                }

                if (outlineSaveSurface != null)
                {
                    using (PdnRegion clipTest = activeRegion.Clone())
                    {
                        clipTest.Intersect(outlineSaveSurface.Region);
                    
                        if (!clipTest.IsEmpty())
                        {
                            //has.Add(bitmapLayer.CreateHistoryAction(Name, Image, outlineSaveSurface));
                            has.Add(new BitmapHistoryAction(Name, Image, this.Workspace, this.Workspace.ActiveLayerIndex, outlineSaveSurface));
                            outlineSaveSurface.Dispose();
                            outlineSaveSurface = null;
                        }
                    }
                }

                if (interiorSaveSurface != null)
                {
                    using (PdnRegion clipTest = activeRegion.Clone())
                    {
                        clipTest.Intersect(interiorSaveSurface.Region);
                        
                        if (!clipTest.IsEmpty())
                        {
                            //has.Add(bitmapLayer.CreateHistoryAction(Name, Image, interiorSaveSurface));
                            has.Add(new BitmapHistoryAction(Name, Image, this.Workspace, this.Workspace.ActiveLayerIndex, interiorSaveSurface));
                            interiorSaveSurface.Dispose();
                            interiorSaveSurface = null;
                        }
                    }
                }

                if (has.Count > 0)
                {
                    CompoundHistoryAction cha = new CompoundHistoryAction(Name, Image, (HistoryAction[])has.ToArray(typeof(HistoryAction)));
                    Workspace.History.PushNewAction(cha);
                }

                activeRegion.Dispose();
                points = null;
                Workspace.Update();
            }
        }

        public ShapeTool(DocumentWorkspace parent,
                         Image toolBarImage,
                         string name,
                         string description,
                         string helpText)
            : this(parent,
                   toolBarImage,
                   name,
                   description,
                   helpText,
                   'o')
        {
        }

        public ShapeTool(DocumentWorkspace parent,
                         Image toolBarImage,
                         string name,
                         string description,
                         string helpText,
                         char hotKey)
            : base(parent,
                   toolBarImage,
                   name,
                   description,
                   helpText,
                   hotKey)
        {
            mouseDown = false;
            points = null;

			cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.ShapeToolCursor.cur")); 
			cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.ShapeToolCursorMouseDown.cur"));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                if (cursorMouseUp != null)
                {
                    cursorMouseUp.Dispose();
                    cursorMouseUp = null;
                }

                if (cursorMouseDown != null)
                {
                    cursorMouseDown.Dispose();
                    cursorMouseDown = null;
                }
            }
        }

    }
}
