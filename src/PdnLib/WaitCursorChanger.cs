/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
