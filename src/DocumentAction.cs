using System;

namespace PaintDotNet
{
    /// <summary>
    /// Provides a way to do a tool-less action that operates on the DocumentWorkspace.
    /// DocumentActions should NOT mess with the History.
    /// DocumentActions should ONLY mutate the DocumentWorkspace and any contained
    /// entities. For example, "Copy" should not be a DocumentAction because it affects
    /// the global Windows clipboard.
    /// </summary>
    public abstract class DocumentAction
    {
        protected string name;
        public string Name
        {
            get
            {
                return name;
            }
        }

        private DocumentWorkspace workspace;
        public DocumentWorkspace Workspace
        {
            get
            {
                return workspace;
            }
        }

        /// <summary>
        /// Implement this to provide an action. You must return a HistoryAction so that you
        /// can be undone. However, you should return null if you didn't do anything.
        /// </summary>
        /// <returns>A HistoryAction object that will be placed onto the HistoryStack.</returns>
        public abstract HistoryAction PerformAction();

        public DocumentAction(DocumentWorkspace workspace, string name)
        {
            this.workspace = workspace;
            this.name = name;
        }
    }
}
