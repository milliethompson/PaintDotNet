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
    /// <summary>
    /// Summary description for UnitsComboBox.
    /// </summary>
    public class UnitsComboBox 
        : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.ComboBox comboBox;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private bool lowercase = true;

        private Hashtable unitsToString;
        private Hashtable stringToUnits;

        // maps from MeasurementUnit->bool for whether that item should be in the list or not
        private Hashtable measurementItems;

        private UnitsDisplayType unitsDisplayType = UnitsDisplayType.Plural;

        [DefaultValue(UnitsDisplayType.Plural)]
        public UnitsDisplayType UnitsDisplayType
        {
            get
            {
                return this.unitsDisplayType;
            }

            set
            {
                if (this.unitsDisplayType != value)
                {
                    this.unitsDisplayType = value;
                    ReloadItems();
                }
            }
        }

        [DefaultValue(true)]
        public bool LowercaseStrings
        {
            get
            {
                return this.lowercase;
            }

            set
            {
                if (this.lowercase != value)
                {
                    this.lowercase = value;
                    ReloadItems();
                }
            }
        }

        [DefaultValue(MeasurementUnit.Pixel)]
        public MeasurementUnit Units
        {
            get
            {
                object selected = this.stringToUnits[this.comboBox.SelectedItem];
                return (MeasurementUnit)selected;
            }
            
            set
            {
                object selectMe = this.unitsToString[value];
                this.comboBox.SelectedItem = selectMe;
            }
        }

        [Browsable(false)]
        public string UnitsText
        {
            get
            {
                if (this.comboBox.SelectedItem == null)
                {
                    return string.Empty;
                }
                else
                {
                    return (string)this.comboBox.SelectedItem;
                }
            }
        }

        [DefaultValue(true)]
        public bool PixelsAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Pixel];
            }

            set
            {
                if (value != this.PixelsAvailable)
                {
                    if (value)
                    {
                        AddUnit(MeasurementUnit.Pixel);
                    }
                    else
                    {
                        if (this.Units == MeasurementUnit.Pixel)
                        {
                            if (this.InchesAvailable)
                            {
                                this.Units = MeasurementUnit.Inch;
                            }
                            else if (this.CentimetersAvailable)
                            {
                                this.Units = MeasurementUnit.Centimeter;
                            }
                        }

                        RemoveUnit(MeasurementUnit.Pixel);
                    }
                }
            }
        }

        [DefaultValue(true)]
        public bool InchesAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Inch];
            }
        }

        [DefaultValue(true)]
        public bool CentimetersAvailable
        {
            get
            {
                return (bool)this.measurementItems[MeasurementUnit.Centimeter];
            }
        }

        public void RemoveUnit(MeasurementUnit removeMe)
        {
            InitMeasurementItems();
            this.measurementItems[removeMe] = false;
            ReloadItems();
        }

        public void AddUnit(MeasurementUnit addMe)
        {
            InitMeasurementItems();
            this.measurementItems[addMe] = true;
            ReloadItems();
        }

        private void InitMeasurementItems()
        {
            if (this.measurementItems == null)
            {
                this.measurementItems = new Hashtable();
                this.measurementItems.Add(MeasurementUnit.Pixel, true);
                this.measurementItems.Add(MeasurementUnit.Centimeter, true);
                this.measurementItems.Add(MeasurementUnit.Inch, true);
            }
        }

        private void ReloadItems()
        {
            string suffix;
            switch (this.unitsDisplayType)
            {
                case UnitsDisplayType.Plural:
                    suffix = ".Plural";
                    break;

                case UnitsDisplayType.Singular:
                    suffix = string.Empty;
                    break;

                case UnitsDisplayType.Ratio:
                    suffix = ".Ratio";
                    break;

                default:
                    throw new InvalidEnumArgumentException("UnitsDisplayType");
            }

            InitMeasurementItems();

            MeasurementUnit oldUnits;
            
            if (this.unitsToString == null)
            {
                oldUnits = MeasurementUnit.Pixel;
            }
            else
            {
                oldUnits = this.Units;
            }

            this.comboBox.Items.Clear();

            string pixelsString = PdnResources.GetString("MeasurementUnit.Pixel" + suffix);
            string inchesString = PdnResources.GetString("MeasurementUnit.Inch" + suffix);
            string centimetersString = PdnResources.GetString("MeasurementUnit.Centimeter" + suffix);

            if (lowercase)
            {
                pixelsString = pixelsString.ToLower();
                inchesString = inchesString.ToLower();
                centimetersString = centimetersString.ToLower();
            }

            this.unitsToString = new Hashtable();
            this.unitsToString.Add(MeasurementUnit.Pixel, pixelsString);
            this.unitsToString.Add(MeasurementUnit.Inch, inchesString);
            this.unitsToString.Add(MeasurementUnit.Centimeter, centimetersString);
            
            this.stringToUnits = new Hashtable();

            if ((bool)this.measurementItems[MeasurementUnit.Pixel])
            {
                this.stringToUnits.Add(pixelsString, MeasurementUnit.Pixel);
                this.comboBox.Items.Add(pixelsString);
            }

            if ((bool)this.measurementItems[MeasurementUnit.Inch])
            {
                this.stringToUnits.Add(inchesString, MeasurementUnit.Inch);
                this.comboBox.Items.Add(inchesString);
            }

            if ((bool)this.measurementItems[MeasurementUnit.Centimeter])
            {
                this.stringToUnits.Add(centimetersString, MeasurementUnit.Centimeter);
                this.comboBox.Items.Add(centimetersString);
            }

            if (!(bool)this.measurementItems[oldUnits])
            {
                if (this.comboBox.Items.Count == 0)
                {
                    this.comboBox.SelectedItem = null;
                }
                else
                {
                    this.comboBox.SelectedIndex = 0;
                }
            }
            else
            {
                this.Units = oldUnits;
            }
        }

        public event EventHandler UnitsChanged;
        protected virtual void OnUnitsChanged()
        {
            if (UnitsChanged != null)
            {
                UnitsChanged(this, EventArgs.Empty);
            }
        }

        public UnitsComboBox()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            ReloadItems();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox.Dock = DockStyle.Fill;
            this.comboBox.SelectedIndexChanged += new EventHandler(comboBox_SelectedIndexChanged);
            this.Controls.Add(this.comboBox);

        }
        #endregion

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnUnitsChanged();
        }
    }
}
