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
    /// This history action doesn't really do anything. It is useful for putting in a
    /// "New Image" placeholder, since the first item in the undo stack can't really
    /// be "undone".
    /// NullHistoryAction instances are also not undoable.
    /// </summary>
    public class NullHistoryAction
        : HistoryAction
    {
        protected override HistoryAction OnUndo()
        {
            throw new InvalidOperationException("NullHistoryActions are not undoable");
        }

        public NullHistoryAction(string name, Image image)
            : base(name, image)
        {
        }
    }
}
