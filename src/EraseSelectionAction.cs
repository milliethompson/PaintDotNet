/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for EraseSelectionAction.
    /// </summary>
    public class EraseSelectionAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return "Erase Selection";
            }
        }

        public override HistoryAction PerformAction()
        {
            if (Workspace.Environment.IsSelectionEmpty)
            {
                return null;
            }

            Type oldToolType = Workspace.Environment.GetToolType();
            Workspace.Environment.SetTool(null);
            SelectionHistoryAction sha = new SelectionHistoryAction(string.Empty, null, Workspace);

            PdnRegion region = Workspace.Environment.CreateSelectedRegion();

            region.Intersect(Workspace.Document.Bounds);
            BitmapLayer layer = ((BitmapLayer)Workspace.ActiveLayer);
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(region);

            HistoryAction ha = new BitmapHistoryAction(Name, null, Workspace, Workspace.ActiveLayerIndex, simplifiedRegion);
            new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 0)).Apply(layer.Surface, region);
            layer.Invalidate(simplifiedRegion);

            Workspace.Document.Invalidate(simplifiedRegion);

            simplifiedRegion.Dispose();
            region.Dispose();

            Workspace.Environment.PerformSelectedPathChanging();
            Workspace.Environment.SelectedPath.Reset();
            Workspace.Environment.PerformSelectedPathChanged();
            Workspace.Environment.SetTool(oldToolType, Workspace);

            return new CompoundHistoryAction(this.Name, Utility.GetImageResource("Icons.MenuEditEraseSelectionIcon.bmp"), new HistoryAction[] { ha, sha });
        }

        public EraseSelectionAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
