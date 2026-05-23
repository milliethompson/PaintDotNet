/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
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
        private ArrayList actions;

        protected override void OnFlush()
        {
            for (int i = 0; i < actions.Count; ++i)
            {
                if (actions[i] != null)
                {
                    ((HistoryAction)actions[i]).Flush();
                }
            }
        }

        protected override HistoryAction OnUndo()
        {
            ArrayList redoActions = new ArrayList(actions.Count);

            for (int i = 0; i < actions.Count; ++i)
            {
                HistoryAction ha = (HistoryAction)actions[actions.Count - i - 1];
                HistoryAction rha = null;

                if (ha != null)
                {
                    rha = ha.PerformUndo();
                }

                redoActions.Add(rha);
            }

            CompoundHistoryAction cha = new CompoundHistoryAction(Name, Image, redoActions);
            return cha;
        }

        public void PushNewAction(HistoryAction newHA)
        {
            actions.Add(newHA);
        }

        public CompoundHistoryAction(string name, Image image, ArrayList actions)
            : base(name, image)
        {
            this.actions = (ArrayList)actions.Clone();
        }

        public CompoundHistoryAction(string name, Image image, HistoryAction[] actions)
            : base(name, image)
        {
            this.actions = new ArrayList(actions);
        }
    }
}
