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
        public CursorChanger(Control control, Cursor newCursor)
        {
            this.control = control;
            this.oldCursor = this.control.Cursor;
            control.Cursor = newCursor;
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
                    control.Cursor = oldCursor;
                }

                disposed = true;
            }
        }
    }
}
