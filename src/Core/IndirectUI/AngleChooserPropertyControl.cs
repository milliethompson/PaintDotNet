/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Core;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(DoubleProperty), PropertyControlType.AngleChooser)]
    internal sealed class AngleChooserPropertyControl
        : PropertyControl<double, DoubleProperty>
    {
        private HeaderLabel displayNameHeader;
        private AngleChooserControl angleChooser;
        private NumericUpDown numericUpDown;
        private Label degreeLabel;
        private Button resetButton;
        private Label descriptionText;

        protected override void OnPropertyReadOnlyChanged()
        {
            this.angleChooser.Enabled = !Property.ReadOnly;
            this.numericUpDown.Enabled = !Property.ReadOnly;
            this.degreeLabel.Enabled = !Property.ReadOnly;
            this.resetButton.Enabled = !Property.ReadOnly;
        }

        private double FromAngleChooserValue(double angleChooserValue)
        {
            if (this.Property.MinValue == -180)
            {
                // property value's range is [-180, +180]
                return angleChooserValue;
            }
            else
            {
                // property value's range is [0, 360]
                if (angleChooserValue > 0)
                {
                    return angleChooserValue;
                }
                else
                {
                    return angleChooserValue + 360;
                }
            }
        }

        private double ToAngleChooserValue(double nudValue)
        {
            if (this.Property.MinValue == -180)
            {
                // property value's range is [-180, +180]
                return nudValue;
            }
            else
            {
                // property value's range is [0, 360]
                if (nudValue <= 180.0)
                {
                    return nudValue;
                }
                else
                {
                    return nudValue - 360;
                }
            }
        }

        protected override void OnPropertyValueChanged()
        {
            if (this.angleChooser.ValueDouble != ToAngleChooserValue(Property.Value))
            {
                this.angleChooser.ValueDouble = ToAngleChooserValue(Property.Value);
            }

            if (this.numericUpDown.Value != (decimal)Property.Value)
            {
                this.numericUpDown.Value = (decimal)Property.Value;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.displayNameHeader.Location = new Point(0, 0);
            this.displayNameHeader.Size = this.displayNameHeader.GetPreferredSize(new Size(ClientSize.Width, 1));

            this.degreeLabel.Size = this.degreeLabel.GetPreferredSize(new Size(0, 0));
            this.degreeLabel.Location = new Point(ClientSize.Width - this.degreeLabel.Width, this.displayNameHeader.Bottom + vMargin);

            int baseNudWidth = UI.ScaleWidth(80) - this.degreeLabel.Width - hMargin;
            this.numericUpDown.PerformLayout();
            this.numericUpDown.Width = baseNudWidth;
            this.numericUpDown.Location = new Point(this.degreeLabel.Left - hMargin - this.numericUpDown.Width, this.displayNameHeader.Bottom + vMargin);

            this.resetButton.PerformLayout();
            this.resetButton.Width = Math.Max(this.resetButton.Width, ClientSize.Width - this.numericUpDown.Left);
            this.resetButton.Location = new Point(
                ClientSize.Width - this.resetButton.Width,
                vMargin + Math.Max(this.numericUpDown.Bottom, this.degreeLabel.Bottom));

            this.angleChooser.Size = UI.ScaleSize(new Size(60, 60));
            int angleChooserMinLeft = hMargin;
            int angleChooserMaxRight = Math.Min(this.resetButton.Left, this.numericUpDown.Left) - hMargin;
            double angleChooserCenter = (double)(angleChooserMinLeft + angleChooserMaxRight) / 2.0;
            int angleChooserLeft = (int)(angleChooserCenter - ((double)this.angleChooser.Width / 2.0));
            this.angleChooser.Location = new Point(angleChooserLeft, this.displayNameHeader.Bottom + vMargin);

            this.descriptionText.Location = new Point(0, Math.Max(this.resetButton.Bottom, this.angleChooser.Bottom));
            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 :
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, descriptionText.Bottom);

            base.OnLayout(levent);
        }

        protected override void OnDisplayNameChanged()
        {
            this.displayNameHeader.Text = this.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.descriptionText.Text = this.Description;
            base.OnDescriptionChanged();
        }

        public AngleChooserPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            DoubleProperty doubleProp = (DoubleProperty)propInfo.Property;
            if (!((doubleProp.MinValue == -180 && doubleProp.MaxValue == +180) ||
                (doubleProp.MinValue == 0 && doubleProp.MaxValue == 360)))
            {
                throw new ArgumentException("Only two min/max ranges are allowed for the AngleChooser control type: [-180, +180] and [0, 360]");
            }

            this.displayNameHeader = new HeaderLabel();
            this.displayNameHeader.Name = "header";
            this.displayNameHeader.RightMargin = 0;
            this.displayNameHeader.Text = this.DisplayName;

            this.angleChooser = new AngleChooserControl();
            this.angleChooser.Name = "angleChooser";
            this.angleChooser.ValueChanged += new EventHandler(AngleChooser_ValueChanged);

            this.numericUpDown = new NumericUpDown();
            this.numericUpDown.Name = "numericUpDown";
            this.numericUpDown.Minimum = (decimal)Property.MinValue;
            this.numericUpDown.Maximum = (decimal)Property.MaxValue;
            this.numericUpDown.DecimalPlaces = 2;
            this.numericUpDown.ValueChanged += new EventHandler(NumericUpDown_ValueChanged);
            this.numericUpDown.TextAlign = HorizontalAlignment.Right;

            this.degreeLabel = new Label();
            this.degreeLabel.Name = "degreeLabel";
            this.degreeLabel.AutoSize = false;
            this.degreeLabel.Text = PdnResources.GetString("AngleChooserConfigDialog.DegreeLabel.Text"); // TODO: put into own string, then deprecate AngleChooserControl

            this.resetButton = new Button();
            this.resetButton.Name = "resetButton";
            this.resetButton.AutoSize = true;
            this.resetButton.FlatStyle = FlatStyle.System;
            this.resetButton.Click += new EventHandler(ResetButton_Click);
            this.resetButton.Text = PdnResources.GetString("Form.ResetButton.Text");

            this.descriptionText = new Label();
            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            SuspendLayout();

            this.Controls.AddRange(
                new Control[]
                {
                    this.displayNameHeader,
                    this.angleChooser,
                    this.numericUpDown,
                    this.degreeLabel,
                    this.resetButton,
                    this.descriptionText
                });

            ResumeLayout(false);
            PerformLayout();
        }

        private void AngleChooser_ValueChanged(object sender, EventArgs e)
        {
            if (Property.Value != FromAngleChooserValue(this.angleChooser.ValueDouble))
            {
                Property.Value = FromAngleChooserValue(this.angleChooser.ValueDouble);
            }
        }

        private void NumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            if (Property.Value != (double)this.numericUpDown.Value)
            {
                Property.Value = (double)this.numericUpDown.Value;
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            Property.Value = (double)Property.DefaultValue;
        }

        protected override bool OnFirstSelect()
        {
            this.numericUpDown.Select();
            return true;
        }
    }
}
