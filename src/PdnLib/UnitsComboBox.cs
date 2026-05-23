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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class UnitsComboBox
        : UserControl,
          IUnitsComboBox
    {
        private ComboBox comboBox;
        private UnitsComboBoxHandler comboBoxHandler;

        public UnitsComboBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.comboBoxHandler = new UnitsComboBoxHandler(this.comboBox);
        }

        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.comboBox.Dock = DockStyle.Fill;
            this.Controls.Add(this.comboBox);
        }

        public UnitsDisplayType UnitsDisplayType
        {
            get
            {
                return this.comboBoxHandler.UnitsDisplayType;                
            }

            set
            {
                this.comboBoxHandler.UnitsDisplayType = value;                
            }
        }

        public bool LowercaseStrings
        {
            get
            {
                return this.comboBoxHandler.LowercaseStrings;                
            }

            set
            {
                this.comboBoxHandler.LowercaseStrings = value;                
            }
        }

        public MeasurementUnit Units
        {
            get
            {
                return this.comboBoxHandler.Units;                
            }

            set
            {
                this.comboBoxHandler.Units = value;                
            }
        }

        public string UnitsText
        {
            get 
            {
                return this.comboBoxHandler.UnitsText;
            }
        }

        public bool PixelsAvailable
        {
            get
            {
                return this.comboBoxHandler.PixelsAvailable;
            }

            set
            {
                this.comboBoxHandler.PixelsAvailable = value;
            }
        }

        public bool InchesAvailable
        {
            get 
            {
                return this.comboBoxHandler.InchesAvailable;
            }
        }

        public bool CentimetersAvailable
        {
            get 
            {
                return this.comboBoxHandler.CentimetersAvailable;
            }
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            this.comboBoxHandler.AddUnit(removeMe);            
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            this.comboBoxHandler.AddUnit(addMe);
        }

        public event EventHandler UnitsChanged
        {
            add
            {
                this.comboBoxHandler.UnitsChanged += value;
            }

            remove
            {
                this.comboBoxHandler.UnitsChanged -= value;
            }
        }
    }
}
