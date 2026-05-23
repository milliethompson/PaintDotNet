/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Base;
using PaintDotNet.PropertySystem;
using System;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public abstract class PropertyControl
        : Control
    {
        private Property property;

        public Property Property
        {
            get
            {
                return this.property;
            }
        }

        public abstract ValueType Value
        {
            get;
            set;
        }

        public event EventHandler ValueChanged;

        protected virtual void OnValueChanged()
        {
            if (ValueChanged != null)
            {
                ValueChanged(this, EventArgs.Empty);
            }
        }

        internal PropertyControl(Property property)
        {
            this.property = property;
        }
    }
}
