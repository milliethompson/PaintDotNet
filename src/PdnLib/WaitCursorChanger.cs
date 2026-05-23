using System;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Simply sets a control's Cursor to the WaitCursor (hourglass) on creation,
    /// and sets it back to its original setting upon disposal.
    /// </summary>
    public class WaitCursorChanger
        : CursorChanger
    {
        public WaitCursorChanger(Control control)
            : base(control, Cursors.WaitCursor)
        {
        }
    }
}
