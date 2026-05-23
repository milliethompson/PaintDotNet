using System;
using System.Drawing;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// A HistoryAction is generally used to save part of the state of the DocumentWorkspace
    /// so that an action that is yet to be performed can be undone at a later time.
    /// For example, if you are going to paint in a certain region, you first create a
    /// HistoryAction that saves the contents of the area you are painting to. Then you
    /// paint.
    /// </summary>
    public abstract class HistoryAction
    {
        private string name;
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        private Image image;
        public Image Image
        {
            get
            {
                return image;
            }

            set
            {
                image = value;
            }
        }

        protected int id;
        private static int nextId = 0;
        public int ID
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        /// <summary>
        /// This will perform the necessary work required to undo an action.
        /// Note that the returned HistoryAction should have the same ID.
        /// </summary>
        /// <returns>
        /// Returns a HistoryAction that can be used to redo the action.
        /// Note that this property should hold: undoAction = undoAction.PerformUndo().PerformUndo()
        /// </returns>
        protected abstract HistoryAction OnUndo();

        /// <summary>
        /// This method ensures that the returned HistoryAction has the appropriate ID tag.
        /// </summary>
        /// <returns>Returns a HistoryAction that can be used to redo the action. 
        /// The ID of this HistoryAction will be the same as the object that this 
        /// method was called on.</returns>
        public HistoryAction PerformUndo()
        {
            HistoryAction ha = OnUndo();
            ha.ID = this.ID;
            return ha;
        }

        public HistoryAction(string name, Image image)
        {
            this.name = name;
            this.image = image;
            this.id = Interlocked.Increment(ref nextId);
        }
    }
}
