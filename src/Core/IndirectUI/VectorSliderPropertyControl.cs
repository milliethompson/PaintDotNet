/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.PropertySystem;
using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.IndirectUI
{
    internal abstract class VectorSliderPropertyControl<TValue>
         : PropertyControl<Pair<TValue, TValue>, VectorProperty<TValue>>
           where TValue : struct, IComparable<TValue>
    {
        private int decimalPlaces = 2;
        private HeaderLabel header;

        private TrackBar sliderX;
        private PdnNumericUpDown numericUpDownX;
        private Button resetButtonX;

        private TrackBar sliderY;
        private PdnNumericUpDown numericUpDownY;
        private Button resetButtonY;

        private Label descriptionText;

        protected virtual void OnDecimalPlacesChanged()
        {
            ResetUIRanges();
        }

        protected int DecimalPlaces
        {
            get
            {
                return this.decimalPlaces;
            }

            set
            {
                this.decimalPlaces = value;
                this.numericUpDownX.DecimalPlaces = value;
                this.numericUpDownY.DecimalPlaces = value;
                OnDecimalPlacesChanged();
            }
        }

        protected bool SliderShowTickMarksX
        {
            get
            {
                return this.sliderX.TickStyle != TickStyle.None;
            }

            set
            {
                this.sliderX.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        protected bool SliderShowTickMarksY
        {
            get
            {
                return this.sliderY.TickStyle != TickStyle.None;
            }

            set
            {
                this.sliderY.TickStyle = value ? TickStyle.BottomRight : TickStyle.None;
            }
        }

        protected TValue SliderSmallChangeX
        {
            get
            {
                return FromSliderValueX(this.sliderX.SmallChange);
            }

            set
            {
                this.sliderX.SmallChange = ToSliderValueX(value);
            }
        }

        protected TValue SliderSmallChangeY
        {
            get
            {
                return FromSliderValueY(this.sliderY.SmallChange);
            }

            set
            {
                this.sliderY.SmallChange = ToSliderValueY(value);
            }
        }

        protected TValue SliderLargeChangeX
        {
            get
            {
                return FromSliderValueX(this.sliderX.LargeChange);
            }

            set
            {
                this.sliderX.LargeChange = ToSliderValueX(value);
            }
        }

        protected TValue SliderLargeChangeY
        {
            get
            {
                return FromSliderValueY(this.sliderY.LargeChange);
            }

            set
            {
                this.sliderY.LargeChange = ToSliderValueY(value);
            }
        }

        protected TValue UpDownIncrementX
        {
            get
            {
                return FromNudValueX(this.numericUpDownX.Increment);
            }

            set
            {
                this.numericUpDownX.Increment = ToNudValueX(value);
            }
        }

        protected TValue UpDownIncrementY
        {
            get
            {
                return FromNudValueY(this.numericUpDownY.Increment);
            }

            set
            {
                this.numericUpDownY.Increment = ToNudValueY(value);
            }
        }

        protected abstract int ToSliderValueX(TValue propertyValue);
        protected abstract TValue FromSliderValueX(int sliderValue);
        protected abstract decimal ToNudValueX(TValue propertyValue);
        protected abstract TValue FromNudValueX(decimal nudValue);

        protected abstract int ToSliderValueY(TValue propertyValue);
        protected abstract TValue FromSliderValueY(int sliderValue);
        protected abstract decimal ToNudValueY(TValue propertyValue);
        protected abstract TValue FromNudValueY(decimal nudValue);

        protected abstract TValue RoundPropertyValue(TValue value);

        protected override void OnLayout(LayoutEventArgs levent)
        {
            int vMargin = UI.ScaleHeight(4);
            int hMargin = UI.ScaleWidth(4);

            this.header.Location = new Point(0, 0);
            this.header.Width = ClientSize.Width;
            this.header.Height = this.header.GetPreferredSize(new Size(this.header.Width, 0)).Height;

            this.resetButtonX.PerformLayout();
            this.resetButtonY.PerformLayout();

            int baseNudWidth = UI.ScaleWidth(80);
            int nudWidth = Math.Max(baseNudWidth, this.resetButtonX.Width);

            this.numericUpDownX.PerformLayout();
            this.numericUpDownX.Width = nudWidth;
            this.numericUpDownX.Location = new Point(ClientSize.Width - this.numericUpDownX.Width, this.header.Bottom + vMargin);

            this.sliderX.Location = new Point(0, this.header.Bottom + vMargin);
            this.sliderX.Width = this.numericUpDownX.Left - hMargin;
            this.sliderX.PerformLayout();

            this.resetButtonX.Location = new Point(ClientSize.Width - this.resetButtonX.Width, this.numericUpDownX.Bottom + vMargin);
            this.resetButtonX.Width = Math.Max(this.resetButtonX.Width, this.numericUpDownX.Width);

            this.numericUpDownY.PerformLayout();
            this.numericUpDownY.Width = nudWidth;
            this.numericUpDownY.Location = new Point(ClientSize.Width - this.numericUpDownY.Width, this.resetButtonX.Bottom + vMargin);

            this.sliderY.Location = new Point(0, this.resetButtonX.Bottom + vMargin);
            this.sliderY.Width = this.numericUpDownY.Left - hMargin;
            this.sliderY.PerformLayout();

            this.resetButtonY.Location = new Point(ClientSize.Width - this.resetButtonY.Width, this.numericUpDownY.Bottom + vMargin);
            this.resetButtonY.Width = Math.Max(this.resetButtonY.Width, this.numericUpDownY.Width);

            this.descriptionText.Location = new Point(0, Math.Max(this.resetButtonY.Bottom, this.sliderY.Bottom));
            this.descriptionText.Width = ClientSize.Width;
            this.descriptionText.Height = string.IsNullOrEmpty(this.descriptionText.Text) ? 0 :
                this.descriptionText.GetPreferredSize(new Size(this.descriptionText.Width, 1)).Height;

            ClientSize = new Size(ClientSize.Width, this.descriptionText.Bottom);

            base.OnLayout(levent);
        }

        protected override void OnDisplayNameChanged()
        {
            this.header.Text = this.DisplayName;
            base.OnDisplayNameChanged();
        }

        protected override void OnDescriptionChanged()
        {
            this.descriptionText.Text = this.Description;
            base.OnDescriptionChanged();
        }

        protected void ResetUIRanges()
        {
            this.sliderX.Minimum = ToSliderValueX(Property.MinValueX);
            this.sliderX.Maximum = ToSliderValueX(Property.MaxValueX);
            this.sliderX.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.sliderX);

            this.numericUpDownX.Minimum = ToNudValueX(Property.MinValueX);
            this.numericUpDownX.Maximum = ToNudValueX(Property.MaxValueX);

            this.sliderY.Minimum = ToSliderValueY(Property.MinValueY);
            this.sliderY.Maximum = ToSliderValueY(Property.MaxValueY);
            this.sliderY.TickFrequency = PropertyControlUtil.GetGoodSliderTickFrequency(this.sliderY);

            this.numericUpDownY.Minimum = ToNudValueY(Property.MinValueY);
            this.numericUpDownY.Maximum = ToNudValueY(Property.MaxValueY);
        }

        private void ValidateUIRanges()
        {
            try
            {
                int value1 = ToSliderValueX(Property.MinValueX);
                int value2 = ToSliderValueX(Property.MaxValueX);
                int value3 = ToSliderValueY(Property.MinValueY);
                int value4 = ToSliderValueY(Property.MaxValueY);

                decimal value5 = ToNudValueX(Property.MinValueX);
                decimal value6 = ToNudValueX(Property.MaxValueX);
                decimal value7 = ToNudValueY(Property.MinValueY);
                decimal value8 = ToNudValueY(Property.MaxValueY);

                TValue value9 = FromSliderValueX(ToSliderValueX(Property.MinValueX));
                TValue value10 = FromSliderValueX(ToSliderValueX(Property.MaxValueX));
                TValue value11 = FromSliderValueY(ToSliderValueY(Property.MinValueY));
                TValue value12 = FromSliderValueY(ToSliderValueY(Property.MaxValueY));
            }

            catch (Exception ex)
            {
                string message = string.Format(
                    "The property's range, [({0},{1}), ({2},{3})], cannot be accomodated. Try a smaller range, or a smaller value for DecimalPlaces.",
                    Property.MinValueX,
                    Property.MinValueY,
                    Property.MaxValueX,
                    Property.MaxValueY);

                throw new PdnException(message, ex);
            }
        }

        public VectorSliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
            SuspendLayout();

            this.header = new HeaderLabel();
            this.sliderX = new TrackBar();
            this.numericUpDownX = new PdnNumericUpDown();
            this.resetButtonX = new Button();
            this.sliderY = new TrackBar();
            this.numericUpDownY = new PdnNumericUpDown();
            this.resetButtonY = new Button();
            this.descriptionText = new Label();

            this.header.Name = "header";
            this.header.RightMargin = 0;
            this.header.Text = this.DisplayName;

            this.sliderX.Name = "sliderX";
            this.sliderX.ValueChanged += new EventHandler(SliderX_ValueChanged);
            this.sliderX.Orientation = Orientation.Horizontal;

            this.numericUpDownX.Name = "numericUpDownX";
            this.numericUpDownX.ValueChanged += new EventHandler(NumericUpDownX_ValueChanged);
            this.numericUpDownX.TextAlign = HorizontalAlignment.Right;

            this.resetButtonX.Name = "resetButtonX";
            this.resetButtonX.AutoSize = true;
            this.resetButtonX.FlatStyle = FlatStyle.System;
            this.resetButtonX.Click += new EventHandler(ResetButtonX_Click);
            this.resetButtonX.Text = PdnResources.GetString("Form.ResetButton.Text");

            this.sliderY.Name = "sliderY";
            this.sliderY.ValueChanged += new EventHandler(SliderY_ValueChanged);
            this.sliderY.Orientation = Orientation.Horizontal;

            this.numericUpDownY.Name = "numericUpDownY";
            this.numericUpDownY.ValueChanged += new EventHandler(NumericUpDownY_ValueChanged);
            this.numericUpDownY.TextAlign = HorizontalAlignment.Right;

            this.resetButtonY.Name = "resetButtonY";
            this.resetButtonY.AutoSize = true;
            this.resetButtonY.FlatStyle = FlatStyle.System;
            this.resetButtonY.Click += new EventHandler(ResetButtonY_Click);
            this.resetButtonY.Text = PdnResources.GetString("Form.ResetButton.Text");

            this.descriptionText.Name = "descriptionText";
            this.descriptionText.AutoSize = false;
            this.descriptionText.Text = this.Description;

            ValidateUIRanges();

            ResetUIRanges();

            Controls.AddRange(
                new Control[]
                {
                    this.header,
                    this.sliderX,
                    this.numericUpDownX,
                    this.resetButtonX,
                    this.sliderY,
                    this.numericUpDownY,
                    this.resetButtonY,
                    this.descriptionText
                });

            ResumeLayout(false);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            this.header.Text = this.Text;
            base.OnTextChanged(e);
        }

        private void ResetButtonX_Click(object sender, EventArgs e)
        {
            Property.ValueX = Property.DefaultValueX;
        }

        private void ResetButtonY_Click(object sender, EventArgs e)
        {
            Property.ValueY = Property.DefaultValueY;
        }

        private bool IsEqualTo(TValue lhs, TValue rhs)
        {
            return ScalarProperty<TValue>.IsEqualTo(lhs, rhs);
        }

        protected override void OnPropertyValueChanged()
        {
            if (!IsEqualTo(RoundPropertyValue(FromNudValueX(this.numericUpDownX.Value)), RoundPropertyValue(Property.ValueX)))
            {
                this.numericUpDownX.Value = ToNudValueX(RoundPropertyValue(Property.ValueX));
            }

            if (this.sliderX.Value != ToSliderValueX(RoundPropertyValue(Property.ValueX)))
            {
                this.sliderX.Value = ToSliderValueX(RoundPropertyValue(Property.ValueX));
            }

            if (!IsEqualTo(RoundPropertyValue(FromNudValueY(this.numericUpDownY.Value)), RoundPropertyValue(Property.ValueY)))
            {
                this.numericUpDownY.Value = ToNudValueY(RoundPropertyValue(Property.ValueY));
            }

            if (this.sliderY.Value != ToSliderValueY(RoundPropertyValue(Property.ValueY)))
            {
                this.sliderY.Value = ToSliderValueY(RoundPropertyValue(Property.ValueY));
            }
        }

        protected override void OnPropertyReadOnlyChanged()
        {
            this.numericUpDownX.Enabled = !Property.ReadOnly;
            this.sliderX.Enabled = !Property.ReadOnly;
            this.resetButtonX.Enabled = !Property.ReadOnly;
            this.numericUpDownY.Enabled = !Property.ReadOnly;
            this.sliderY.Enabled = !Property.ReadOnly;
            this.resetButtonY.Enabled = !Property.ReadOnly;
        }

        private void SliderX_ValueChanged(object sender, EventArgs e)
        {
            if (ToSliderValueX(Property.ValueX) != ToSliderValueX(FromSliderValueX(this.sliderX.Value)))
            {
                Property.ValueX = FromSliderValueX(this.sliderX.Value);
            }
        }

        private void NumericUpDownX_ValueChanged(object sender, EventArgs e)
        {
            if (!IsEqualTo(RoundPropertyValue(Property.ValueX), RoundPropertyValue(FromNudValueX(this.numericUpDownX.Value))))
            {
                Property.ValueX = RoundPropertyValue(FromNudValueX(this.numericUpDownX.Value));
            }
        }

        private void SliderY_ValueChanged(object sender, EventArgs e)
        {
            if (ToSliderValueY(Property.ValueY) != ToSliderValueY(FromSliderValueY(this.sliderY.Value)))
            {
                Property.ValueY = FromSliderValueY(this.sliderY.Value);
            }
        }

        private void NumericUpDownY_ValueChanged(object sender, EventArgs e)
        {
            if (!IsEqualTo(RoundPropertyValue(Property.ValueY), RoundPropertyValue(FromNudValueY(this.numericUpDownY.Value))))
            {
                Property.ValueY = RoundPropertyValue(FromNudValueY(this.numericUpDownY.Value));
            }
        }

        protected override bool OnFirstSelect()
        {
            this.numericUpDownX.Select();
            return true;
        }
    }
}
