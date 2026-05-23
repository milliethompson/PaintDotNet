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
    /// Summary description for RectangleSelectTool.
    /// </summary>
    public class MagicWandTool
        : FloodTool
    {
        private Cursor cursorMouseUp;
        private Cursor cursorMouseDown;
        private Cursor cursorMagicWandUnion;
        private Cursor cursorMagicWandXor;
        private Cursor cursorMagicWandSubtract;
        private PdnRegion regionBefore = null;
        private Keys modifiersOnClick;
        private const Keys UnionModifiers = Keys.Shift;
        private const Keys XorModifiers = Keys.Control;
        private const Keys SubtractModifiers = Keys.Shift | Keys.Control;
        private const Keys RegularModifiers = 0;
        private const Keys ModifierMask = Keys.Shift | Keys.Control;

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            UpdateCursor();
            base.OnMouseUp (e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Cursor = cursorMouseDown;
            modifiersOnClick = ModifierKeys;
            regionBefore = null;

            if ((modifiersOnClick & ModifierMask) == UnionModifiers ||
                (modifiersOnClick & ModifierMask) == XorModifiers)
            {
                regionBefore = Workspace.Environment.CreateSelectedRegion();
                base.OnMouseDown(e);
            }
            else if ((modifiersOnClick & ModifierMask) == SubtractModifiers)
            {
                if (!Workspace.Environment.IsSelectionEmpty)
                {
                    regionBefore = Workspace.Environment.CreateSelectedRegion();
                    base.OnMouseDown(e);
                }
            }
            else
            {
                base.OnMouseDown(e);
            }
        }

        protected override void RegionSelected(PdnRegion fillRegion, Rectangle boundingBox)
        {
            base.RegionSelected(fillRegion, boundingBox);

            if (regionBefore != null &&
                (modifiersOnClick & ModifierMask) == UnionModifiers ||
                (modifiersOnClick & ModifierMask) == SubtractModifiers ||
                (modifiersOnClick & ModifierMask) == XorModifiers)
            {
                Rectangle bounds = Rectangle.Union(fillRegion.GetBoundsInt(), regionBefore.GetBoundsInt());
                BitVector2D stencil = new BitVector2D(bounds.Width, bounds.Height);
                Rectangle[] scansBefore = regionBefore.GetRegionScansReadOnlyInt();
                Rectangle[] scansNew = fillRegion.GetRegionScansReadOnlyInt();

                for (int i = 0; i < scansBefore.Length; ++i)
                {
                    Rectangle rect = scansBefore[i];

                    rect.X -= bounds.X;
                    rect.Y -= bounds.Y;
                    stencil.Set(rect, true);
                }

                if ((modifiersOnClick & ModifierMask) == UnionModifiers)
                {
                    for (int i = 0; i < scansNew.Length; ++i)
                    {
                        Rectangle rect = scansNew[i];

                        rect.X -= bounds.X;
                        rect.Y -= bounds.Y;
                        stencil.Set(rect, true);
                    }
                }
                else if ((modifiersOnClick & ModifierMask) == SubtractModifiers)
                {
                    for (int i = 0; i < scansNew.Length; ++i)
                    {
                        Rectangle rect = scansNew[i];

                        rect.X -= bounds.X;
                        rect.Y -= bounds.Y;
                        stencil.Set(rect, false);
                    }
                }
                else if ((modifiersOnClick & ModifierMask) == XorModifiers)
                {
                    for (int i = 0; i < scansNew.Length; ++i)
                    {
                        Rectangle rect = scansNew[i];

                        rect.X -= bounds.X;
                        rect.Y -= bounds.Y;
                        stencil.Invert(rect);
                    }
                }

                PdnGraphicsPath path = PdnGraphicsPath.PathFromStencil(stencil, new Rectangle(0, 0, stencil.Width, stencil.Height));

                using (Matrix matrix = new Matrix())
                {
                    matrix.Reset();
                    matrix.Translate(bounds.X, bounds.Y);
                    path.Transform(matrix);
                }

                SelectionHistoryAction undoAction = new SelectionHistoryAction(this.Name, this.Image, Workspace);

                Workspace.Environment.SelectedPath = path;
                Workspace.History.PushNewAction(undoAction);

                regionBefore.Dispose();
                regionBefore = null;
            }
        }

        protected override void PerimeterFound(PdnGraphicsPath path)
        {
            base.PerimeterFound(path);

            if ((modifiersOnClick & ModifierMask) == RegularModifiers)
            {
                SelectionHistoryAction undoAction = new SelectionHistoryAction(this.Name, this.Image, Workspace);

                Workspace.Environment.SelectedPath = (PdnGraphicsPath)path.Clone();
                Workspace.History.PushNewAction(undoAction);
            }
        }

        public MagicWandTool(DocumentWorkspace workspace)
            : base(workspace,
            Utility.GetImageResource("Icons.MagicWandToolIcon.bmp"),
            "Magic Wand",
            "Selects a Homogenous Color Region",
            "Click to select an area of similar color. Hold Shift to add (union), Ctrl to xor, or Ctrl+Shift to remove.",
            's')
        {
            cursorMouseUp = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursor.cur"));
            cursorMouseDown = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursorMouseDown.cur"));
            cursorMagicWandUnion = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursorUnion.cur"));
            cursorMagicWandXor = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursorXor.cur"));
            cursorMagicWandSubtract = new Cursor(Utility.GetResourceStream("Cursors.MagicWandToolCursorSubtract.cur"));
            Cursor = cursorMouseUp;
            LimitToSelection = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose (disposing);

            if (disposing)
            {
                DisposeImage();

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

                if (cursorMagicWandSubtract != null)
                {
                    cursorMagicWandSubtract.Dispose();
                    cursorMagicWandSubtract = null;
                }

                if (cursorMagicWandUnion != null)
                {
                    cursorMagicWandUnion.Dispose();
                    cursorMagicWandUnion = null;
                }

                if (cursorMagicWandXor != null)
                {
                    cursorMagicWandXor.Dispose();
                    cursorMagicWandXor = null;
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            UpdateCursor();

            base.OnKeyDown (e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            UpdateCursor();

            base.OnKeyUp (e);
        }

        private void UpdateCursor()
        {
            if ((ModifierKeys & ModifierMask) == SubtractModifiers)
            {
                Cursor = cursorMagicWandSubtract;
            }
            else if ((ModifierKeys & ModifierMask) == XorModifiers)
            {
                Cursor = cursorMagicWandXor;
            }
            else if ((ModifierKeys & ModifierMask) == UnionModifiers)
            {
                Cursor = cursorMagicWandUnion;
            }
            else
            {
                Cursor = cursorMouseUp;
            }
        }
    }
}