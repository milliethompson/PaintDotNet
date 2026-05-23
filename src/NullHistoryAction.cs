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
