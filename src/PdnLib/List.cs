using System;

namespace PaintDotNet
{
    /// <summary>
    /// A very simple linked-list class, done functional style. Use null for
    /// the tail to indicate the end of a list.
    /// </summary>
    public class List
    {
        private object head;
        public object Head
        {
            get
            {
                return head;
            }
        }

        private List tail;
        public List Tail
        {
            get
            {
                return tail;
            }
        }

        public List(object head, List tail)
        {
            this.head = head;
            this.tail = tail;
        }
    }
}
