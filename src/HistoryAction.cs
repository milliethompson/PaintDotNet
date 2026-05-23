/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
    /// paint. Then you push the history action on to the history stack.
    /// 
    /// Using the HistoryActionData class you can serialize your data to disk so that it
    /// doesn't fester in memory. There are important rules to follow here though:
    /// 1. Don't hold a reference to a Layer. Store a reference to the DocumentWorkspace and
    ///    the layer's index instead, and access it via Workspace.Document.Layers[index].
    /// 2. The exception to #1 is if you are deleting a layer. But you should use
    ///    DeleteLayerHistoryAction for that.
    /// 3. To generalize, avoid serializing something unless you're replacing or deleting it.
    ///    (and by 'serializing' I mean 'putting it in your HistoryActionData class')
    ///    It is better to hold a 'navigation reference' as opposed to a real reference.
    ///    An example of a 'navigation reference' is listed in #1, where we don't store a ref
    ///    to the layer itself but we store the information needed to navigate to it.
    ///    The reasoning for this is made clear if you consider the following case. Assume you
    ///    are holding on to a layer reference ("private Layer theLayer;"). Next, assume that
    ///    the layer is deleted. Then the deletion is undone. The new layer in memory is not
    ///    the layer you have a reference to even though they hold the same data. Changes made
    ///    to one do not show up in the other one.
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

        private PersistedObject historyActionData = null;

        /// <summary>
        /// Gets or sets the HistoryActionData associated with this HistoryAction.
        /// </summary>
        /// <remarks>
        /// Setting this property will immediately serialize the given object to disk.
        /// </remarks>
        protected HistoryActionData Data
        {
            get
            {
                return (HistoryActionData)historyActionData.Object;
            }

            set
            {
                this.historyActionData = new PersistedObject(value);
            }
        }

        /// <summary>
        /// Ensures that the memory held by the Data property is serialized to disk and
        /// freed from memory.
        /// </summary>
        public void Flush()
        {
            if (historyActionData != null)
            {
                historyActionData.Flush();
            }

            OnFlush();
        }

        protected virtual void OnFlush()
        {
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
