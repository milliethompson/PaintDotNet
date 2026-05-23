/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
                return PdnResources.GetString("EraseSelectionAction.Name");
            }
        }

        public override HistoryAction PerformAction()
        {
            if (Workspace.Environment.Selection.IsEmpty)
            {
                return null;
            }

            Type oldToolType = Workspace.Environment.GetToolType();
            Workspace.Environment.SetTool(null);
            SelectionHistoryAction sha = new SelectionHistoryAction(string.Empty, null, Workspace);

            PdnRegion region = Workspace.Environment.Selection.CreateRegion();

            BitmapLayer layer = ((BitmapLayer)Workspace.ActiveLayer);
            PdnRegion simplifiedRegion = Utility.SimplifyAndInflateRegion(region);

            HistoryAction ha = new BitmapHistoryAction(Name, null, Workspace, Workspace.ActiveLayerIndex, simplifiedRegion);
            new UnaryPixelOps.Constant(ColorBgra.FromBgra(255, 255, 255, 0)).Apply(layer.Surface, region);
            layer.Invalidate(simplifiedRegion);

            Workspace.Document.Invalidate(simplifiedRegion);

            simplifiedRegion.Dispose();
            region.Dispose();

            Workspace.Environment.Selection.Reset();
            Workspace.Environment.SetTool(oldToolType, Workspace);

            return new CompoundHistoryAction(
                this.Name,
                PdnResources.GetImage("Icons.MenuEditEraseSelectionIcon.png"), 
                new HistoryAction[] { ha, sha });
        }

        public EraseSelectionAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
