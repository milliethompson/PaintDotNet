using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Lets you combine multiple HistoryActions that can be undon/redone
    /// in a single operation, and be referred to by one name.
    /// The actions will be undone in the reverse order they are given to
    /// the constructor via the actions array.
    /// </summary>
	public class CompoundHistoryAction
        : HistoryAction
	{
        private HistoryAction[] actions;

        protected override HistoryAction OnUndo()
        {
            HistoryAction[] redoActions = new HistoryAction[actions.Length];

            for (int i = 0; i < actions.Length; ++i)
            {
                redoActions[i] = actions[actions.Length - i - 1].PerformUndo();
            }

            CompoundHistoryAction cha = new CompoundHistoryAction(Name, Image, redoActions);
            cha.id = this.id;

            return cha;
        }

		public CompoundHistoryAction(string name, Image image, HistoryAction[] actions)
            : base(name, image)
		{
            this.actions = (HistoryAction[])actions.Clone();
		}
	}
}
