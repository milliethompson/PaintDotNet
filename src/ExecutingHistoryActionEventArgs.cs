/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    public class ExecutingHistoryActionEventArgs
        : EventArgs
    {
        private HistoryAction historyAction;
        private bool mayAlterSuspendToolProperty;
        private bool suspendTool;

        public HistoryAction HistoryAction
        {
            get
            {
                return this.historyAction;
            }
        }

        public bool MayAlterSuspendTool
        {
            get
            {
                return this.mayAlterSuspendToolProperty;
            }
        }

        public bool SuspendTool
        {
            get
            {
                return this.suspendTool;
            }

            set
            {
                if (!this.mayAlterSuspendToolProperty)
                {
                    throw new InvalidOperationException("May not alter the SuspendTool property when MayAlterSuspendToolProperty is false");
                }

                this.suspendTool = value;
            }
        }

        public ExecutingHistoryActionEventArgs(HistoryAction historyAction, bool mayAlterSuspendToolProperty, bool suspendTool)
        {
            this.historyAction = historyAction;
            this.mayAlterSuspendToolProperty = mayAlterSuspendToolProperty;
            this.suspendTool = suspendTool;
        }
    }
}
