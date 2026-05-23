using System;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// The SentinelHistoryAction class is meant to be used by Tools. 
	/// Normally when Undo is clicked while a tool is active, the tool is first
	/// deactivated and the Environment's Tool selection is set to "no tool".
	/// Then the action is undone, and then the tool is reactivated.
	/// A Tool may place a SentinelHistoryAction on the history stack while it
	/// is active and if the user undoes it, the tool will NOT be deactivated.
	/// Instead, an event will be fired that the Tool can then respond to.
	/// </summary>
	public class SentinelHistoryAction
        : HistoryAction
	{
        public event EventHandler Undo;

        protected override HistoryAction OnUndo()
        {
            if (Undo != null)
            {
                Undo(this, EventArgs.Empty);
            }

            return new SentinelHistoryAction(this.Name, this.Image);
        }

		public SentinelHistoryAction(string name, Image image)
            : base(name, image)
		{
		}
	}
}
