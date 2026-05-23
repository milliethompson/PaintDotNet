using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Lets you combine multiple HistoryActions that can be undon/redone
    /// in a single operation, and be referred to by one name.
    /// The actions will be undone in the reverse order they are given to
    /// the constructor via the actions array.
    /// You can use 'null' for a HistoryAction and it will be ignored.
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
                HistoryAction ha = actions[actions.Length - i - 1];
                HistoryAction rha = null;

                if (ha != null)
                {
                    rha = ha.PerformUndo();
                }

                redoActions[i] = rha;
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
