using System;

namespace PaintDotNet
{
    /// <summary>
    /// Declares an EventArgs type for an event that needs a single integer, interpreted
    /// as an index, as event information.
    /// </summary>
    public class IndexEventArgs : EventArgs
    {
        int index;

        public int Index
        {
            get
            {
                return index;
            }
        }

        public IndexEventArgs(int i)
        {
            this.index = i;
        }
    }
}
