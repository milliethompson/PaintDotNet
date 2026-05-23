/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FlattenAction.
    /// </summary>
    public class FlattenAction
        : DocumentAction
    {
        public static string StaticName
        {
            get
            {
                return "Flatten";
            }
        }

        public override HistoryAction PerformAction()
        {
            ReplaceDocumentHistoryAction rdha = new ReplaceDocumentHistoryAction(name, Utility.GetImageResource("Icons.MenuImageFlattenIcon.bmp"), Workspace);
            Document flat = Workspace.Document.Flatten();
            Workspace.SetDocument(flat);
            return rdha;
        }

        public FlattenAction(DocumentWorkspace workspace)
            : base(workspace, StaticName)
        {
        }
    }
}
