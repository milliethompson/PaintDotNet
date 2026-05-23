using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MoveTool.
	/// </summary>
	public class MoveTool
        : Tool
	{
        public static string StaticName
        {
            get
            {
                return "Move";
            }
        }

        private IrregularSurface liftedPixels;
        private IrregularSurface saveSurface;
        private BitmapLayer activeLayer;
        private RenderArgs renderArgs;
        private HistoryAction undoAction;
        private Point startMouseXY;
        private Point offset;
        private IPixelOp pixelOp;
		private bool didPaste = false;
        private bool tracking;
		private bool dontDrop = false; // so that OnSelectionChanging() can tell who is raising the event ... don't drop the pixels if WE caused the event

        protected override void OnActivate()
        {
            base.OnActivate ();
            liftedPixels = null;
            offset = new Point(0, 0);
            activeLayer = (BitmapLayer)Workspace.ActiveLayer;
            renderArgs = new RenderArgs(activeLayer.Surface);
            tracking = false;
        }

		private void DropPixels()
		{
			if (saveSurface != null)
			{
				saveSurface.Draw(renderArgs.Surface);
				saveSurface.Dispose();
				saveSurface = null;
			}

            // 1. simplify the region into a few rectangles
			RectangleF[] simplifiedRectsF = Utility.SimplifyRegion(liftedPixels.Region, 10);

            // 2. inflate it -- this is necessary to make sure there are no gaps between the 
            // rectangles, and to account for the extra rendering surface area from the selection 
            // outline
            RectangleF[] inflatedRectsF = Utility.InflateRectangles(simplifiedRectsF, 2);

            // 3. translate it
            RectangleF[] translatedRectsF = Utility.TranslateRectangles(inflatedRectsF, offset);

            // 4. convert it back to a Region
            Region simplifiedRegion = Utility.RectanglesToRegion(translatedRectsF);

			HistoryAction bitmapAction2 = activeLayer.CreateHistoryAction(Name, Image, simplifiedRegion);

			liftedPixels.Draw(activeLayer.Surface, offset.X, offset.Y, pixelOp);
			liftedPixels.Dispose();
			liftedPixels = null;
			activeLayer.Invalidate(simplifiedRegion);

			if (didPaste || !(offset.X == 0 && offset.Y == 0))
			{
				string name;
				Image image;

				if (didPaste)
				{
					name = "Paste";
					image = Utility.GetImageResource("Icons.MenuEditPasteIcon.bmp");
				}
				else
				{
					name = this.Name;
					image = this.Image;
				}

				CompoundHistoryAction cha = new CompoundHistoryAction(name, Image, new HistoryAction[] { undoAction, bitmapAction2 });
				Workspace.History.PushNewAction(cha);
				didPaste = false;
			}
		}

        protected override void OnDeactivate()
        {
            base.OnDeactivate ();

            if (liftedPixels != null)
            {   
				DropPixels();
            }

            activeLayer = null;

            renderArgs.Dispose();
            renderArgs = null;

            tracking = false;
        }

        protected override void OnClick()
        {
            base.OnClick ();
        }

		protected override void OnSelectionChanging()
		{
			base.OnSelectionChanging();

			if (!dontDrop)
			{
				if (liftedPixels != null)
				{
					DropPixels();
				}

                if (tracking)
                {
					tracking = false;
                }
			}
		}

		protected override void OnSelectionChanged()
		{
			base.OnSelectionChanged();
		}

        /// <summary>
        /// Provided as a special entry point so that Paste can work well.
        /// </summary>
        /// <param name="surface">What you want to paste.</param>
        /// <param name="offset">Where you want to paste it.</param>
        public void PasteMouseDown(SurfaceForClipboard sfc, Point offset)
        {
            if (liftedPixels != null)
            {
                DropPixels();
            }

            liftedPixels = sfc.Surface;
            startMouseXY = new Point(0, 0);
            //offset = new Point(0, 0);

            undoAction = (HistoryAction)new SelectionHistoryAction(Name, Image, Workspace);

            Matrix translationMatrix = new Matrix();
            translationMatrix.Reset();
            translationMatrix.Translate((float)offset.X, (float)offset.Y);
            GraphicsPath translatedPath = (GraphicsPath)sfc.Outline.CreateGraphicsPath();
            //translatedPath.Transform(translationMatrix);

			dontDrop = true;
            Workspace.Environment.SelectedPath = translatedPath;
			dontDrop = false;

            tracking = true;
            pixelOp = new UnaryPixelOps.Identity();

			this.didPaste = true;
            MouseEventArgs mea = new MouseEventArgs(MouseButtons.None, 0, offset.X, offset.Y, 0);
            OnMouseDown(mea);
            OnMouseUp(mea);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown (e);

            if (tracking)
            {
                return;
            }

            if (Workspace.Environment.IsSelectionEmpty)
            {
                return;
            }

            if (liftedPixels == null)
            {   // lift!
                Region liftRegion = null; 

                SelectionHistoryAction selectionAction = new SelectionHistoryAction(Name, Image, Workspace);

                liftRegion = Workspace.Environment.CreateSelectedRegion();
                liftRegion.Intersect(activeLayer.Bounds);

                liftedPixels = new IrregularSurface(activeLayer.Surface, liftRegion);
				//Region simplifiedRegion = Utility.SimplifyAndInflateRegion(liftedPixels.Region, Utility.DefaultSimplificationFactor, 2);
				Region simplifiedRegion = new Region(liftedPixels.Region.GetBounds(this.renderArgs.Graphics));

                HistoryAction bitmapAction = activeLayer.CreateHistoryAction(Name, Image, simplifiedRegion);
                undoAction = new CompoundHistoryAction(Name, Image, new HistoryAction[] { bitmapAction, selectionAction });

                startMouseXY = new Point(e.X, e.Y);
                offset = new Point(0, 0);

				// If the user is holding down the control key, we want to *copy* the pixels
				// and not "lift and erase"
				if ((ModifierKeys & Keys.Control) == Keys.None)
				{
					ColorBgra fill = Workspace.Environment.BackColor;
                    fill.a = 0;
                    UnaryPixelOp op = new UnaryPixelOps.Constant(fill);

                    op.Apply(renderArgs.Surface, liftRegion);

                    activeLayer.Invalidate(simplifiedRegion);
                }

                pixelOp = new UnaryPixelOps.Identity();
				simplifiedRegion.Dispose();
            }

            Point mouseXY = new Point(e.X, e.Y);
            Point offsetXY = new Point(startMouseXY.X + offset.X, startMouseXY.Y + offset.Y);
            Point delta = new Point(mouseXY.X - offsetXY.X, mouseXY.Y - offsetXY.Y);
            startMouseXY.X += delta.X;
            startMouseXY.Y += delta.Y;

            tracking = true;

            OnMouseMove(e);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove (e);
            
            if (tracking)
            {
                Point newMouseXY = new Point(e.X, e.Y);
                Point newOffset = new Point(newMouseXY.X - startMouseXY.X, newMouseXY.Y - startMouseXY.Y);
                Size delta = new Size(newOffset.X - offset.X, newOffset.Y - offset.Y);

                if (saveSurface != null)
                {
                    saveSurface.Draw(renderArgs.Surface);
                    activeLayer.Invalidate(saveSurface.Region);
                    saveSurface.Dispose();
                    saveSurface = null;
                }

                Matrix translateMatrix = new Matrix();
                translateMatrix.Reset();
                translateMatrix.Translate((float)delta.Width, (float)delta.Height);

				dontDrop = true;
                Workspace.Environment.PerformSelectedPathChanging();
                Workspace.Environment.SelectedPath.Transform(translateMatrix);

                Region selectedRegion = null;
                
                if (Workspace.Environment.IsSelectionEmpty)
                {
                    selectedRegion = new Region(renderArgs.Surface.Bounds);
                }
                else
                {
                    selectedRegion = Workspace.Environment.CreateSelectedRegion();
                }

                Region simplifiedRegion = Utility.SimplifyAndInflateRegion(selectedRegion);

                saveSurface = new IrregularSurface(renderArgs.Surface, simplifiedRegion);
                liftedPixels.Draw(renderArgs.Surface, newOffset.X, newOffset.Y, pixelOp);
                activeLayer.Invalidate(simplifiedRegion);
                Workspace.Environment.PerformSelectedPathChanged();
				dontDrop = false;
                Workspace.Update();

                simplifiedRegion.Dispose();
                selectedRegion.Dispose();
                this.offset = newOffset;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp (e);
            OnMouseMove(e);
            tracking = false;
        }

        protected override void OnKeyPress(Keys key)
        {
            base.OnKeyPress(key);

            if (!tracking)
            {
                int dx = 0;
                int dy = 0;

                if ((key & Keys.KeyCode) == Keys.Left)
                {
                    dx = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Right)
                {
                    dx = +1;
                } 
                else if ((key & Keys.KeyCode) == Keys.Up)
                {
                    dy = -1;
                }
                else if ((key & Keys.KeyCode) == Keys.Down) 
                {
                    dy = +1;
                }

                if ((key & Keys.Control) != Keys.None)
                {
                    dx *= 10;
                    dy *= 10;
                }

                OnMouseDown(new MouseEventArgs(MouseButtons.Left, 0, 0, 0, 0));
                OnMouseMove(new MouseEventArgs(MouseButtons.Left, 0, dx, dy, 0));
                OnMouseUp(new MouseEventArgs(MouseButtons.Left, 0, dx, dy, 0));
            }
        }

		public MoveTool(DocumentWorkspace workspace)
            : base(workspace)
		{
            this.name = MoveTool.StaticName;
            this.description = "Allows you to move around pixels that have been selected.";
            this.toolBarImage = Utility.GetImageResource("Icons.MoveToolIcon.bmp");
            this.cursor = new Cursor(Utility.GetResourceStream("Cursors.MoveToolCursor.cur"));
		}
	}
}
