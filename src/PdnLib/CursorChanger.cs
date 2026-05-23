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
    /// This class will set the cursor of a control to the requested one,
    /// and then when this class is Disposed it will reset the cursor
    /// to the original cursor.
    /// </summary>
    public class CursorChanger
        : IDisposable
    {
        private Control control;
        private Cursor oldCursor;

        private Control FindTopParent(Control control)
        {
            Control parent = control.Parent;

            if (parent == null)
            {
                return control;
            }
            else
            {
                return FindTopParent(parent);
            }
        }

        public CursorChanger(Control control, Cursor newCursor)
        {
            this.control = control;
            this.oldCursor = this.control.Cursor;
            FindTopParent(control).Cursor = newCursor;
        }

        ~CursorChanger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    FindTopParent(control).Cursor = oldCursor;
                }

                disposed = true;
            }
        }
    }
}
