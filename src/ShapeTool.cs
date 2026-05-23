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
        private bool moveOriginMode;
        private PointF lastXY;
        private bool mouseDown;
        private MouseButtons mouseButton;
        private BitmapLayer bitmapLayer;
        private RenderArgs renderArgs;
        private PdnRegion interiorSaveRegion;
        private PdnRegion outlineSaveRegion;
        private ArrayList points;
        private PdnRegion lastDrawnRegion = null;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;
        private bool shapeWasCommited = true;
        private CompoundHistoryAction chaAlreadyOnStack = null;

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

            outlineSaveRegion = null;
            interiorSaveRegion = null;

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

            if (!this.shapeWasCommited)
            {
                CommitShape();
            }

            bitmapLayer = null;

            if (renderArgs != null)
            {
                renderArgs.Dispose();
                renderArgs = null;
            }

            if (outlineSaveRegion != null)
            {
                outlineSaveRegion.Dispose();
                outlineSaveRegion = null;
            }

            if (interiorSaveRegion != null)
            {
                interiorSaveRegion.Dispose();
                interiorSaveRegion = null;
            }

            points = null;
        }

        protected virtual void OnShapeBegin()
        {
        }

        /// <summary>
        /// Called when the shape is finished being traced by the default input handlers.
        /// </summary>
        /// <remarks>Do not call the base implementation of this method if you are overriding it.</remarks>
        /// <returns>true to commit the shape immediately</returns>
        protected virtual bool OnShapeEnd()
        {
            return true;
        }

        protected override void OnStylusDown(StylusEventArgs  e)
        {
            base.OnStylusDown(e);

            if (!this.shapeWasCommited)
            {
                CommitShape();
            }
            
            this.ClearSavedMemory();
            this.ClearSavedRegion();

            cursorMouseUp = Cursor;
            Cursor = cursorMouseDown;

            if (mouseDown && e.Button == mouseButton)
            {
                return;
            }

            if (mouseDown)
            {
                moveOriginMode = true;
                lastXY = new PointF(e.Fx, e.Fy);
                OnStylusMove(e);
            }
            else if (((e.Button & MouseButtons.Left) == MouseButtons.Left) ||
                     ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                // begin new shape
                this.shapeWasCommited = false;

                OnShapeBegin();

                mouseDown = true;
                mouseButton = e.Button;

                using (PdnRegion clipRegion = Workspace.Environment.Selection.CreateRegion())
                {
                    renderArgs.Graphics.SetClip(clipRegion.GetRegionReadOnly(), CombineMode.Replace);
                }

                // reset the points we're drawing!
                points = new ArrayList();

                OnStylusMove(e);
            }
        }

        protected override void OnStylusMove(StylusEventArgs e)
        {
            base.OnStylusMove (e);

            if (moveOriginMode)
            {
                SizeF delta = new SizeF(e.Fx - lastXY.X, e.Fy - lastXY.Y);

                for (int i = 0; i < points.Count; ++i)
                {
                    PointF ptF = (PointF)points[i];
                    ptF.X += delta.Width;
                    ptF.Y += delta.Height;
                    points[i] = ptF;
                }

                lastXY = new PointF(e.Fx, e.Fy);
            }
            else if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                PointF mouseXY = new PointF(e.Fx, e.Fy);
                points.Add(mouseXY);
            }
        }

        public virtual PixelOffsetMode GetPixelOffsetMode()
        {
            return PixelOffsetMode.Half;
        }

        protected ArrayList GetTrimmedShapePath()
        {
            ArrayList pointsCopy = (ArrayList)this.points.Clone();
            pointsCopy = TrimShapePath(pointsCopy);
            return pointsCopy;
        }

        protected void SetShapePath(ArrayList newPoints)
        {
            this.points = newPoints;
        }

        protected void RenderShape()
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
            if (interiorSaveRegion != null)
            {
                RestoreRegion(interiorSaveRegion);
                interiorSaveRegion.Dispose();
                interiorSaveRegion = null;
            }

            if (outlineSaveRegion != null)
            {
                RestoreRegion(outlineSaveRegion);
                outlineSaveRegion.Dispose();
                outlineSaveRegion = null;
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
                    try
                    {
                        outlinePath.Widen(outlinePen);
                        outlineRegion = new PdnRegion(outlinePath);
                    }

                    // Sometimes GDI+ gets cranky if we have a very small shape (e.g. all points
                    // are coincident). 
                    catch (OutOfMemoryException)
                    {
                        outlineRegion = new PdnRegion(shapePath);
                    }
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
                    invalidOutlineRegion = Utility.SimplifyAndInflateRegion(outlineRegion, Utility.DefaultSimplificationFactor, (int)(outlinePen.Width + 2));
                }

                // create optimized interior region
                PdnRegion invalidInteriorRegion = Utility.SimplifyAndInflateRegion(interiorRegion, Utility.DefaultSimplificationFactor, 3);

                PdnRegion invalidRegion = new PdnRegion();
                invalidRegion.MakeEmpty();

                // set up alpha blending
                renderArgs.Graphics.CompositingMode = Workspace.Environment.GetCompositingMode();

                SaveRegion(invalidOutlineRegion, invalidOutlineRegion.GetBoundsInt());
                this.outlineSaveRegion = invalidOutlineRegion;
                if ((drawType & ShapeDrawType.Outline) != 0)
                {
                    shapePath.Draw(renderArgs.Graphics, outlinePen);
                }

                invalidRegion.Union(invalidOutlineRegion);

                // draw shape
                if ((drawType & ShapeDrawType.Interior) != 0)
                {
                    SaveRegion(invalidInteriorRegion, invalidInteriorRegion.GetBoundsInt());
                    this.interiorSaveRegion = invalidInteriorRegion;
                    renderArgs.Graphics.FillPath(interiorBrush, shapePath);
                    invalidRegion.Union(invalidInteriorRegion);
                }
                else
                {
                    invalidInteriorRegion.Dispose();
                    invalidInteriorRegion = null;
                }

                bitmapLayer.Invalidate(invalidRegion);
                invalidRegion.Dispose();

                outlineRegion.Dispose();
                interiorRegion.Dispose();
            }

            Update();

            if (shapePath != null)
            {
                shapePath.Dispose();
                shapePath = null;
            }

            outlinePen.Dispose();
            interiorBrush.Dispose();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button not down then leave function
            if (mouseDown && ((e.Button & mouseButton) != MouseButtons.None))
            {
                RenderShape();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (mouseDown)
            {
                RenderShape();
            }

            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (mouseDown)
            {
                RenderShape();
            }

            base.OnKeyUp(e);
        }

        protected virtual void OnShapeCommitting()
        {
        }

        protected void CommitShape()
        {
            OnShapeCommitting();

            mouseDown = false;

            ArrayList has = new ArrayList();
            PdnRegion activeRegion = Workspace.Environment.Selection.CreateRegion();

            if (outlineSaveRegion != null)
            {
                using (PdnRegion clipTest = activeRegion.Clone())
                {
                    clipTest.Intersect(outlineSaveRegion);
                    
                    if (!clipTest.IsEmpty())
                    {
                        BitmapHistoryAction bha = new BitmapHistoryAction(Name, Image, this.Workspace, 
                            this.Workspace.ActiveLayerIndex, outlineSaveRegion, this.ScratchSurface);

                        has.Add(bha);
                        outlineSaveRegion.Dispose();
                        outlineSaveRegion = null;
                    }
                }
            }

            if (interiorSaveRegion != null)
            {
                using (PdnRegion clipTest = activeRegion.Clone())
                {
                    clipTest.Intersect(interiorSaveRegion);
                        
                    if (!clipTest.IsEmpty())
                    {
                        BitmapHistoryAction bha = new BitmapHistoryAction(Name, Image, this.Workspace, 
                            this.Workspace.ActiveLayerIndex, interiorSaveRegion, this.ScratchSurface);

                        has.Add(bha);
                        interiorSaveRegion.Dispose();
                        interiorSaveRegion = null;
                    }
                }
            }

            if (has.Count > 0)
            {
                CompoundHistoryAction cha = new CompoundHistoryAction(Name, Image, (HistoryAction[])has.ToArray(typeof(HistoryAction)));

                if (this.chaAlreadyOnStack == null)
                {
                    Workspace.History.PushNewAction(cha);
                }
                else
                {
                    this.chaAlreadyOnStack.PushNewAction(cha);
                    this.chaAlreadyOnStack = null;
                }
            }

            activeRegion.Dispose();
            points = null;
            Update();
            this.shapeWasCommited = true;
        }

        protected override void OnStylusUp(StylusEventArgs e)
        {
            base.OnStylusUp(e);

            Cursor = cursorMouseUp;

            if (moveOriginMode)
            {
                moveOriginMode = false;
            }
            else if (mouseDown)
            {
                bool doCommit = OnShapeEnd();

                if (doCommit)
                {
                    CommitShape();
                }
                else
                {
                    // place a 'sentinel' history action on the stack that will be filled in later
                    CompoundHistoryAction cha = new CompoundHistoryAction(Name, Image, new ArrayList());
                    Workspace.History.PushNewAction(cha);
                    this.chaAlreadyOnStack = cha;
                }
            }
        }

        public ShapeTool(DocumentWorkspace parent,
                         Image toolBarImage,
                         string name,
                         string helpText)
            : this(parent,
                   toolBarImage,
                   name,
                   helpText,
                   'o')
        {
        }

        public ShapeTool(DocumentWorkspace parent,
                         Image toolBarImage,
                         string name,
                         string helpText,
                         char hotKey)
            : base(parent,
                   toolBarImage,
                   name,
                   helpText,
                   hotKey)
        {
            mouseDown = false;
            points = null;

            cursorMouseUp = new Cursor(PdnResources.GetResourceStream("Cursors.ShapeToolCursor.cur")); 
            cursorMouseDown = new Cursor(PdnResources.GetResourceStream("Cursors.ShapeToolCursorMouseDown.cur"));
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
