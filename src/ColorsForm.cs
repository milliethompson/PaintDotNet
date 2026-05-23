/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ColorsForm.
    /// </summary>
    public class ColorsForm 
        : FloatingToolForm
    {
        private System.Windows.Forms.NumericUpDown redUpDown;
        private System.Windows.Forms.NumericUpDown greenUpDown;
        private System.Windows.Forms.NumericUpDown blueUpDown;
        private System.Windows.Forms.Label redLabel;
        private System.Windows.Forms.Label blueLabel;
        private System.Windows.Forms.Label greenLabel;
        private System.Windows.Forms.Label hueLabel;
        private System.Windows.Forms.NumericUpDown hueUpDown;
        private System.Windows.Forms.NumericUpDown valueUpDown;
        private System.Windows.Forms.NumericUpDown saturationUpDown;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label saturationLabel;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.ComboBox whichUserColorBox;
        private ColorGradientControl colorGradientControl;
        private System.Windows.Forms.NumericUpDown alphaUpDown;
        private System.Windows.Forms.TrackBar alphaTrackBar;
        private PaintDotNet.ColorWheel colorWheel;

        private Stack ignoreChangedEvents = new Stack();
        private ColorBgra lastForeColor;
        private System.Windows.Forms.Button moreLessButton;
        private ColorBgra lastBackColor;

        private int suspendSetWhichUserColor;
        private string lessText;
        private string moreText;
        private Size moreSize;
        private Size lessSize;
        private System.Windows.Forms.Control lessModeButtonSentinel;
        private System.Windows.Forms.Control moreModeButtonSentinel;
        private System.Windows.Forms.Control lessModeHeaderSentinel;
        private System.Windows.Forms.Control moreModeHeaderSentinel;
        private bool inMoreState = true;
        private System.Windows.Forms.Label hexLabel;
        private System.Windows.Forms.TextBox hexBox;
        private uint ignore = 0;
        private PaintDotNet.HeaderLabel baseColorHeader;
        private PaintDotNet.HeaderLabel rgbHeader;
        private PaintDotNet.HeaderLabel hsvHeader;
        private PaintDotNet.HeaderLabel alphaHeader;

        private bool haveDoneInitStyles = false;

        private class WhichUserColorWrapper
        {
            private WhichUserColor whichUserColor;

            public WhichUserColor WhichUserColor
            {
                get
                {
                    return this.whichUserColor;
                }
            }

            public override int GetHashCode()
            {
                return this.whichUserColor.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                WhichUserColorWrapper rhs = obj as WhichUserColorWrapper;

                if (rhs == null)
                {
                    return false;
                }

                if (rhs.whichUserColor == this.whichUserColor)
                {
                    return true;
                }

                return false;
            }

            public override string ToString()
            {
                return PdnResources.GetString("WhichUserColor." + this.whichUserColor.ToString());
            }

            public WhichUserColorWrapper(WhichUserColor whichUserColor)
            {
                this.whichUserColor = whichUserColor;
            }
        }

        public void SuspendSetWhichUserColor()
        {
            ++this.suspendSetWhichUserColor;
        }

        public void ResumeSetWhichUserColor()
        {
            --this.suspendSetWhichUserColor;
        }

        public WhichUserColor WhichUserColor
        {
            get
            {
                return ((WhichUserColorWrapper)whichUserColorBox.SelectedItem).WhichUserColor;
            }

            set
            {
                if (this.suspendSetWhichUserColor <= 0)
                {
                    whichUserColorBox.SelectedItem = new WhichUserColorWrapper(value);
                }
            }
        }

        public void SetColorControlsRedraw(bool enabled)
        {
            SystemLayer.UI.SetControlRedraw(this.whichUserColorBox, enabled);
            SystemLayer.UI.SetControlRedraw(this.colorGradientControl, enabled);

            if (enabled)
            {
                this.whichUserColorBox.Invalidate(true);
                this.colorGradientControl.Invalidate(true);
            }
        }

        public event ColorEventHandler UserForeColorChanged;
        protected virtual void OnUserForeColorChanged(ColorBgra newColor)
        {
            if (UserForeColorChanged != null && ignore == 0)
            {
                UserForeColorChanged(this, new ColorEventArgs(newColor));
                lastForeColor = newColor;
            }
        }

        private ColorBgra userForeColor;
        public ColorBgra UserForeColor
        {
            get
            {
                return userForeColor;
            }

            set
            {
                if (IgnoreChangedEvents)
                {
                    return;
                }

                if (userForeColor != value)
                {
                    userForeColor = value;
                    OnUserForeColorChanged(value);

                    if (WhichUserColor != WhichUserColor.Foreground)
                    {
                        this.WhichUserColor = WhichUserColor.Foreground;
                    }

                    ignore++;

                    // only do the update on the last one, so partial RGB info isn't parsed.
                    Utility.SetNumericUpDownValue(alphaUpDown, value.A);
                    Utility.SetNumericUpDownValue(redUpDown, value.R);
                    Utility.SetNumericUpDownValue(greenUpDown, value.G);

                    ignore--;
                    Utility.SetNumericUpDownValue(blueUpDown, value.B);
                    Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;

                    SyncHsvFromRgb(value);
                }
            }
        }

        private string GetHexNumericUpDownValue(int red, int green, int blue)
        {
            int newHexNumber = (red << 16) | (green << 8) | blue;
            string newHexText = System.Convert.ToString(newHexNumber, 16);
            
            while (newHexText.Length < 6)
            {
                newHexText = "0" + newHexText;
            }

            return newHexText.ToUpper();
        }

        public event ColorEventHandler UserBackColorChanged;
        protected virtual void OnUserBackColorChanged(ColorBgra newColor)
        {
            if (UserBackColorChanged != null && ignore == 0)
            {
                lastBackColor = newColor;
                UserBackColorChanged(this, new ColorEventArgs(newColor));
            }
        }

        private ColorBgra userBackColor;
        public ColorBgra UserBackColor
        {
            get
            {
                return userBackColor;
            }

            set
            {
                if (IgnoreChangedEvents)
                {
                    return;
                }

                if (userBackColor != value)
                {
                    userBackColor = value;
                    OnUserBackColorChanged(value);

                    if (WhichUserColor != WhichUserColor.Background)
                    {
                        this.WhichUserColor = WhichUserColor.Background;
                    }

                    ignore++;

                    //only do the update on the last one, so partial RGB info isn't parsed.
                    Utility.SetNumericUpDownValue(alphaUpDown, value.A);
                    Utility.SetNumericUpDownValue(redUpDown, value.R);
                    Utility.SetNumericUpDownValue(greenUpDown, value.G);

                    ignore--;
                    Utility.SetNumericUpDownValue(blueUpDown, value.B);
                    Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;

                    SyncHsvFromRgb(value);
                }
            }
        }

        /// <summary>
        /// Convenience function for ColorsForm internals. Checks the value of the
        /// WhichUserColor property and raises either the UserForeColorChanged or
        /// the UserBackColorChanged events.
        /// </summary>
        /// <param name="newColor">The new color to notify clients about.</param>
        private void OnUserColorChanged(ColorBgra newColor)
        {
            switch (WhichUserColor)
            {
                case WhichUserColor.Foreground:
                    OnUserForeColorChanged(newColor);
                    break;

                case WhichUserColor.Background:
                    OnUserBackColorChanged(newColor);
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }
        }

        /// <summary>
        /// Whenever a color is changed via RGB methods, call this and the HSV
        /// counterparts will be sync'd up.
        /// </summary>
        /// <param name="newColor">The RGB color that should be converted to HSV.</param>
        private void SyncHsvFromRgb(ColorBgra newColor)
        {
            if (ignore == 0) 
            {
                ignore++;
                HsvColor hsvColor = HsvColor.FromColor(newColor.ToColor());

                Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
                Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
                Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

                if (((colorGradientControl.Value * 100) / 255) != hsvColor.Value)
                {
                    colorGradientControl.Value = (255 * hsvColor.Value) / 100;
                }

                colorGradientControl.TopColor = new HsvColor(hsvColor.Hue, hsvColor.Saturation, 100).ToColor();
                colorGradientControl.BottomColor = new HsvColor(hsvColor.Hue, hsvColor.Saturation, 0).ToColor();

                colorWheel.HsvColor = hsvColor;
                ignore--;
            }
        }

        /// <summary>
        /// Whenever a color is changed via HSV methods, call this and the RGB
        /// counterparts will be sync'd up.
        /// </summary>
        /// <param name="newColor">The HSV color that should be converted to RGB.</param>
        private void SyncRgbFromHsv(HsvColor newColor)
        {
            if (ignore == 0) 
            {
                ignore++;
                RgbColor rgbColor = newColor.ToRgb();

                Utility.SetNumericUpDownValue(redUpDown, rgbColor.Red);
                Utility.SetNumericUpDownValue(greenUpDown, rgbColor.Green);
                Utility.SetNumericUpDownValue(blueUpDown, rgbColor.Blue);

                string hexText = GetHexNumericUpDownValue(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
                hexBox.Text = hexText;

                ignore--;
            } 
        }

        public ColorsForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            whichUserColorBox.Items.Add(new WhichUserColorWrapper(WhichUserColor.Foreground));
            whichUserColorBox.Items.Add(new WhichUserColorWrapper(WhichUserColor.Background));
            whichUserColorBox.SelectedIndex = 0;

            moreSize = this.Size;
            lessSize = new Size(4 + rgbHeader.PointToScreen(new Point(0, 0)).X - Left, moreSize.Height);

            this.Text = PdnResources.GetString("ColorsForm.Text");
            this.baseColorHeader.Text = PdnResources.GetString("ColorsForm.BaseColorHeader.Text");
            this.redLabel.Text = PdnResources.GetString("ColorsForm.RedLabel.Text");
            this.blueLabel.Text = PdnResources.GetString("ColorsForm.BlueLabel.Text");
            this.greenLabel.Text = PdnResources.GetString("ColorsForm.GreenLabel.Text");
            this.saturationLabel.Text = PdnResources.GetString("ColorsForm.SaturationLabel.Text");
            this.valueLabel.Text = PdnResources.GetString("ColorsForm.ValueLabel.Text");
            this.hueLabel.Text = PdnResources.GetString("ColorsForm.HueLabel.Text");
            this.rgbHeader.Text = PdnResources.GetString("ColorsForm.RgbHeader.Text");
            this.hexLabel.Text = PdnResources.GetString("ColorsForm.HexLabel.Text");
            this.hsvHeader.Text = PdnResources.GetString("ColorsForm.HsvHeader.Text");
            this.alphaHeader.Text = PdnResources.GetString("ColorsForm.AlphaHeader.Text");

            this.lessText = "<< " + PdnResources.GetString("ColorsForm.MoreLessButton.Text.Less");
            this.moreText = PdnResources.GetString("ColorsForm.MoreLessButton.Text.More") + " >>";
            this.moreLessButton.Text = lessText;
        }

        protected override void OnLoad(EventArgs e)
        {
            this.inMoreState = true;
            haveDoneInitStyles = true;
            moreLessButton.PerformClick();
            haveDoneInitStyles = false;
            base.OnLoad(e);
        }

        protected override void OnEnableStyles()
        {
            // do nothing else (yet)
            EnableStyles(this.moreLessButton);
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
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.colorGradientControl = new PaintDotNet.ColorGradientControl();
            this.colorWheel = new PaintDotNet.ColorWheel();
            this.redUpDown = new System.Windows.Forms.NumericUpDown();
            this.greenUpDown = new System.Windows.Forms.NumericUpDown();
            this.blueUpDown = new System.Windows.Forms.NumericUpDown();
            this.redLabel = new System.Windows.Forms.Label();
            this.blueLabel = new System.Windows.Forms.Label();
            this.greenLabel = new System.Windows.Forms.Label();
            this.saturationLabel = new System.Windows.Forms.Label();
            this.valueLabel = new System.Windows.Forms.Label();
            this.hueLabel = new System.Windows.Forms.Label();
            this.valueUpDown = new System.Windows.Forms.NumericUpDown();
            this.saturationUpDown = new System.Windows.Forms.NumericUpDown();
            this.hueUpDown = new System.Windows.Forms.NumericUpDown();
            this.hexBox = new System.Windows.Forms.TextBox();
            this.hexLabel = new System.Windows.Forms.Label();
            this.whichUserColorBox = new System.Windows.Forms.ComboBox();
            this.alphaUpDown = new System.Windows.Forms.NumericUpDown();
            this.alphaTrackBar = new System.Windows.Forms.TrackBar();
            this.moreLessButton = new System.Windows.Forms.Button();
            this.lessModeButtonSentinel = new System.Windows.Forms.Control();
            this.moreModeButtonSentinel = new System.Windows.Forms.Control();
            this.lessModeHeaderSentinel = new System.Windows.Forms.Control();
            this.moreModeHeaderSentinel = new System.Windows.Forms.Control();
            this.baseColorHeader = new PaintDotNet.HeaderLabel();
            this.rgbHeader = new PaintDotNet.HeaderLabel();
            this.hsvHeader = new PaintDotNet.HeaderLabel();
            this.alphaHeader = new PaintDotNet.HeaderLabel();
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // colorGradientControl
            // 
            this.colorGradientControl.BottomColor = System.Drawing.Color.Black;
            this.colorGradientControl.Count = 1;
            this.colorGradientControl.Location = new System.Drawing.Point(208, 56);
            this.colorGradientControl.Name = "colorGradientControl";
            this.colorGradientControl.Size = new System.Drawing.Size(29, 200);
            this.colorGradientControl.TabIndex = 2;
            this.colorGradientControl.TabStop = false;
            this.colorGradientControl.TopColor = System.Drawing.Color.White;
            this.colorGradientControl.Value = 0;
            this.colorGradientControl.ValueChanged += new PaintDotNet.IndexEventHandler(this.colorGradientControl_ValueChanged);
            // 
            // colorWheel
            // 
            this.colorWheel.Location = new System.Drawing.Point(16, 64);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new System.Drawing.Size(184, 184);
            this.colorWheel.TabIndex = 3;
            this.colorWheel.TabStop = false;
            this.colorWheel.ColorChanged += new System.EventHandler(this.colorWheel_ColorChanged);
            // 
            // redUpDown
            // 
            this.redUpDown.Location = new System.Drawing.Point(320, 24);
            this.redUpDown.Maximum = new System.Decimal(new int[] {
                                                                      255,
                                                                      0,
                                                                      0,
                                                                      0});
            this.redUpDown.Name = "redUpDown";
            this.redUpDown.Size = new System.Drawing.Size(56, 20);
            this.redUpDown.TabIndex = 2;
            this.redUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.redUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.redUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.redUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.redUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // greenUpDown
            // 
            this.greenUpDown.Location = new System.Drawing.Point(320, 48);
            this.greenUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.greenUpDown.Name = "greenUpDown";
            this.greenUpDown.Size = new System.Drawing.Size(56, 20);
            this.greenUpDown.TabIndex = 3;
            this.greenUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.greenUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.greenUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.greenUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.greenUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // blueUpDown
            // 
            this.blueUpDown.Location = new System.Drawing.Point(320, 72);
            this.blueUpDown.Maximum = new System.Decimal(new int[] {
                                                                       255,
                                                                       0,
                                                                       0,
                                                                       0});
            this.blueUpDown.Name = "blueUpDown";
            this.blueUpDown.Size = new System.Drawing.Size(56, 20);
            this.blueUpDown.TabIndex = 4;
            this.blueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.blueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.blueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.blueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.blueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // redLabel
            // 
            this.redLabel.Location = new System.Drawing.Point(256, 24);
            this.redLabel.Name = "redLabel";
            this.redLabel.Size = new System.Drawing.Size(56, 24);
            this.redLabel.TabIndex = 7;
            this.redLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // blueLabel
            // 
            this.blueLabel.Location = new System.Drawing.Point(256, 72);
            this.blueLabel.Name = "blueLabel";
            this.blueLabel.Size = new System.Drawing.Size(56, 24);
            this.blueLabel.TabIndex = 8;
            this.blueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // greenLabel
            // 
            this.greenLabel.Location = new System.Drawing.Point(256, 48);
            this.greenLabel.Name = "greenLabel";
            this.greenLabel.Size = new System.Drawing.Size(56, 24);
            this.greenLabel.TabIndex = 9;
            this.greenLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // saturationLabel
            // 
            this.saturationLabel.Location = new System.Drawing.Point(248, 160);
            this.saturationLabel.Name = "saturationLabel";
            this.saturationLabel.Size = new System.Drawing.Size(62, 24);
            this.saturationLabel.TabIndex = 16;
            this.saturationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueLabel
            // 
            this.valueLabel.Location = new System.Drawing.Point(248, 184);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(64, 24);
            this.valueLabel.TabIndex = 15;
            this.valueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // hueLabel
            // 
            this.hueLabel.Location = new System.Drawing.Point(248, 136);
            this.hueLabel.Name = "hueLabel";
            this.hueLabel.Size = new System.Drawing.Size(64, 24);
            this.hueLabel.TabIndex = 14;
            this.hueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueUpDown
            // 
            this.valueUpDown.Location = new System.Drawing.Point(320, 184);
            this.valueUpDown.Name = "valueUpDown";
            this.valueUpDown.Size = new System.Drawing.Size(56, 20);
            this.valueUpDown.TabIndex = 8;
            this.valueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.valueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.valueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.valueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // saturationUpDown
            // 
            this.saturationUpDown.Location = new System.Drawing.Point(320, 160);
            this.saturationUpDown.Name = "saturationUpDown";
            this.saturationUpDown.Size = new System.Drawing.Size(56, 20);
            this.saturationUpDown.TabIndex = 7;
            this.saturationUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.saturationUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.saturationUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.saturationUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.saturationUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // hueUpDown
            // 
            this.hueUpDown.Location = new System.Drawing.Point(320, 136);
            this.hueUpDown.Maximum = new System.Decimal(new int[] {
                                                                      360,
                                                                      0,
                                                                      0,
                                                                      0});
            this.hueUpDown.Name = "hueUpDown";
            this.hueUpDown.Size = new System.Drawing.Size(56, 20);
            this.hueUpDown.TabIndex = 6;
            this.hueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.hueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.hueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.hueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // hexBox
            // 
            this.hexBox.Location = new System.Drawing.Point(320, 96);
            this.hexBox.Name = "hexBox";
            this.hexBox.Size = new System.Drawing.Size(56, 20);
            this.hexBox.TabIndex = 5;
            this.hexBox.Text = "000000";
            this.hexBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hexBox.TextChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.hexBox.Leave += new System.EventHandler(this.hexUpDown_Leave);
            this.hexBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.hexUpDown_KeyUp);
            this.hexBox.Enter += new System.EventHandler(this.hexUpDown_Enter);
            // 
            // hexLabel
            // 
            this.hexLabel.Location = new System.Drawing.Point(256, 96);
            this.hexLabel.Name = "hexLabel";
            this.hexLabel.Size = new System.Drawing.Size(56, 24);
            this.hexLabel.TabIndex = 13;
            this.hexLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // whichUserColorBox
            // 
            this.whichUserColorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.whichUserColorBox.Location = new System.Drawing.Point(8, 8);
            this.whichUserColorBox.Name = "whichUserColorBox";
            this.whichUserColorBox.Size = new System.Drawing.Size(112, 21);
            this.whichUserColorBox.TabIndex = 0;
            this.whichUserColorBox.SelectedIndexChanged += new System.EventHandler(this.whichUserColorBox_SelectedIndexChanged);
            // 
            // alphaUpDown
            // 
            this.alphaUpDown.Location = new System.Drawing.Point(320, 228);
            this.alphaUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.alphaUpDown.Name = "alphaUpDown";
            this.alphaUpDown.Size = new System.Drawing.Size(56, 20);
            this.alphaUpDown.TabIndex = 10;
            this.alphaUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.alphaUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.alphaUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.alphaUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.alphaUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // alphaTrackBar
            // 
            this.alphaTrackBar.AutoSize = false;
            this.alphaTrackBar.LargeChange = 64;
            this.alphaTrackBar.Location = new System.Drawing.Point(248, 228);
            this.alphaTrackBar.Maximum = 255;
            this.alphaTrackBar.Name = "alphaTrackBar";
            this.alphaTrackBar.Size = new System.Drawing.Size(64, 25);
            this.alphaTrackBar.TabIndex = 9;
            this.alphaTrackBar.TickFrequency = 64;
            this.alphaTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.alphaTrackBar.ValueChanged += new System.EventHandler(this.alphaTrackBar_ValueChanged);
            // 
            // moreLessButton
            // 
            this.moreLessButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.moreLessButton.Location = new System.Drawing.Point(165, 7);
            this.moreLessButton.Name = "moreLessButton";
            this.moreLessButton.TabIndex = 1;
            this.moreLessButton.Click += new System.EventHandler(this.moreLessButton_Click);
            // 
            // lessModeButtonSentinel
            // 
            this.lessModeButtonSentinel.Location = new System.Drawing.Point(128, 7);
            this.lessModeButtonSentinel.Name = "lessModeButtonSentinel";
            this.lessModeButtonSentinel.TabIndex = 22;
            this.lessModeButtonSentinel.Text = "we put the lessMore control here when in \"Less\" mode";
            this.lessModeButtonSentinel.Visible = false;
            // 
            // moreModeButtonSentinel
            // 
            this.moreModeButtonSentinel.Location = new System.Drawing.Point(165, 7);
            this.moreModeButtonSentinel.Name = "moreModeButtonSentinel";
            this.moreModeButtonSentinel.TabIndex = 23;
            this.moreModeButtonSentinel.Visible = false;
            // 
            // lessModeHeaderSentinel
            // 
            this.lessModeHeaderSentinel.Location = new System.Drawing.Point(8, 40);
            this.lessModeHeaderSentinel.Name = "lessModeHeaderSentinel";
            this.lessModeHeaderSentinel.Size = new System.Drawing.Size(192, 188);
            this.lessModeHeaderSentinel.TabIndex = 24;
            this.lessModeHeaderSentinel.Visible = false;
            // 
            // moreModeHeaderSentinel
            // 
            this.moreModeHeaderSentinel.Location = new System.Drawing.Point(8, 40);
            this.moreModeHeaderSentinel.Name = "moreModeHeaderSentinel";
            this.moreModeHeaderSentinel.Size = new System.Drawing.Size(232, 216);
            this.moreModeHeaderSentinel.TabIndex = 25;
            this.moreModeHeaderSentinel.TabStop = false;
            this.moreModeHeaderSentinel.Visible = false;
            // 
            // baseColorHeader
            // 
            this.baseColorHeader.Location = new System.Drawing.Point(8, 40);
            this.baseColorHeader.Name = "baseColorHeader";
            this.baseColorHeader.RightMargin = 0;
            this.baseColorHeader.Size = new System.Drawing.Size(232, 14);
            this.baseColorHeader.TabIndex = 26;
            this.baseColorHeader.TabStop = false;
            // 
            // rgbHeader
            // 
            this.rgbHeader.Location = new System.Drawing.Point(248, 8);
            this.rgbHeader.Name = "rgbHeader";
            this.rgbHeader.RightMargin = 0;
            this.rgbHeader.Size = new System.Drawing.Size(128, 14);
            this.rgbHeader.TabIndex = 27;
            this.rgbHeader.TabStop = false;
            // 
            // hsvHeader
            // 
            this.hsvHeader.Location = new System.Drawing.Point(248, 120);
            this.hsvHeader.Name = "hsvHeader";
            this.hsvHeader.RightMargin = 0;
            this.hsvHeader.Size = new System.Drawing.Size(128, 14);
            this.hsvHeader.TabIndex = 28;
            this.hsvHeader.TabStop = false;
            // 
            // alphaHeader
            // 
            this.alphaHeader.Location = new System.Drawing.Point(248, 212);
            this.alphaHeader.Name = "alphaHeader";
            this.alphaHeader.RightMargin = 0;
            this.alphaHeader.Size = new System.Drawing.Size(128, 14);
            this.alphaHeader.TabIndex = 29;
            this.alphaHeader.TabStop = false;
            // 
            // ColorsForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(386, 264);
            this.Controls.Add(this.alphaHeader);
            this.Controls.Add(this.hsvHeader);
            this.Controls.Add(this.rgbHeader);
            this.Controls.Add(this.colorWheel);
            this.Controls.Add(this.colorGradientControl);
            this.Controls.Add(this.baseColorHeader);
            this.Controls.Add(this.moreModeButtonSentinel);
            this.Controls.Add(this.lessModeButtonSentinel);
            this.Controls.Add(this.moreLessButton);
            this.Controls.Add(this.whichUserColorBox);
            this.Controls.Add(this.lessModeHeaderSentinel);
            this.Controls.Add(this.moreModeHeaderSentinel);
            this.Controls.Add(this.greenLabel);
            this.Controls.Add(this.blueLabel);
            this.Controls.Add(this.redLabel);
            this.Controls.Add(this.blueUpDown);
            this.Controls.Add(this.greenUpDown);
            this.Controls.Add(this.redUpDown);
            this.Controls.Add(this.hexLabel);
            this.Controls.Add(this.hexBox);
            this.Controls.Add(this.hueUpDown);
            this.Controls.Add(this.saturationUpDown);
            this.Controls.Add(this.valueUpDown);
            this.Controls.Add(this.hueLabel);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.saturationLabel);
            this.Controls.Add(this.alphaTrackBar);
            this.Controls.Add(this.alphaUpDown);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ColorsForm";
            this.Controls.SetChildIndex(this.alphaUpDown, 0);
            this.Controls.SetChildIndex(this.alphaTrackBar, 0);
            this.Controls.SetChildIndex(this.saturationLabel, 0);
            this.Controls.SetChildIndex(this.valueLabel, 0);
            this.Controls.SetChildIndex(this.hueLabel, 0);
            this.Controls.SetChildIndex(this.valueUpDown, 0);
            this.Controls.SetChildIndex(this.saturationUpDown, 0);
            this.Controls.SetChildIndex(this.hueUpDown, 0);
            this.Controls.SetChildIndex(this.hexBox, 0);
            this.Controls.SetChildIndex(this.hexLabel, 0);
            this.Controls.SetChildIndex(this.redUpDown, 0);
            this.Controls.SetChildIndex(this.greenUpDown, 0);
            this.Controls.SetChildIndex(this.blueUpDown, 0);
            this.Controls.SetChildIndex(this.redLabel, 0);
            this.Controls.SetChildIndex(this.blueLabel, 0);
            this.Controls.SetChildIndex(this.greenLabel, 0);
            this.Controls.SetChildIndex(this.moreModeHeaderSentinel, 0);
            this.Controls.SetChildIndex(this.lessModeHeaderSentinel, 0);
            this.Controls.SetChildIndex(this.whichUserColorBox, 0);
            this.Controls.SetChildIndex(this.moreLessButton, 0);
            this.Controls.SetChildIndex(this.lessModeButtonSentinel, 0);
            this.Controls.SetChildIndex(this.moreModeButtonSentinel, 0);
            this.Controls.SetChildIndex(this.baseColorHeader, 0);
            this.Controls.SetChildIndex(this.colorGradientControl, 0);
            this.Controls.SetChildIndex(this.colorWheel, 0);
            this.Controls.SetChildIndex(this.rgbHeader, 0);
            this.Controls.SetChildIndex(this.hsvHeader, 0);
            this.Controls.SetChildIndex(this.alphaHeader, 0);
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private void whichUserColorBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ColorBgra color;

            switch (WhichUserColor)
            {
                case WhichUserColor.Foreground:
                    color = userForeColor;
                    break;

                case WhichUserColor.Background:
                    color = userBackColor;
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            PushIgnoreChangedEvents();
            Utility.SetNumericUpDownValue(redUpDown, color.R);
            Utility.SetNumericUpDownValue(greenUpDown, color.G);
            Utility.SetNumericUpDownValue(blueUpDown, color.B);

            string hexText = GetHexNumericUpDownValue(color.R, color.G, color.B);
            hexBox.Text = hexText;

            Utility.SetNumericUpDownValue(alphaUpDown, color.A);
            PopIgnoreChangedEvents();

            SyncHsvFromRgb(color);

            OnRelinquishFocus();
        }

        private void colorWheel_ColorChanged(object sender, EventArgs e)
        {
            if (IgnoreChangedEvents)
            {
                return;
            }

            PushIgnoreChangedEvents();

            HsvColor hsvColor = colorWheel.HsvColor;
            RgbColor rgbColor = hsvColor.ToRgb();
            ColorBgra color = ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value);

            Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
            Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
            Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

            Utility.SetNumericUpDownValue(redUpDown, color.R);
            Utility.SetNumericUpDownValue(greenUpDown, color.G);
            Utility.SetNumericUpDownValue(blueUpDown, color.B);

            string hexText = GetHexNumericUpDownValue(color.R, color.G, color.B);
            hexBox.Text = hexText;
            
            Utility.SetNumericUpDownValue(alphaUpDown, color.A);

            colorGradientControl.TopColor = new HsvColor(hsvColor.Hue, hsvColor.Saturation, 100).ToColor();
            colorGradientControl.BottomColor = new HsvColor(hsvColor.Hue, hsvColor.Saturation, 0).ToColor();
            colorGradientControl.Value = (255 * hsvColor.Value) / 100;
            
            switch (WhichUserColor)
            {
                case WhichUserColor.Foreground:
                    userForeColor = color;
                    OnUserForeColorChanged(color);
                    OnRelinquishFocus();
                    break;

                case WhichUserColor.Background:
                    userBackColor = color;
                    OnUserBackColorChanged(color);
                    OnRelinquishFocus();
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            PopIgnoreChangedEvents();

            Update();
        }

        private void colorGradientControl_ValueChanged(object sender, IndexEventArgs e)
        {
            if (IgnoreChangedEvents)
            {
                return;
            }

            int hue = (int)hueUpDown.Value;
            int saturation = (int)saturationUpDown.Value;
            int value = (colorGradientControl.Value * 100) / 255;

            HsvColor hsvColor = new HsvColor(hue, saturation, value);
            colorWheel.HsvColor = hsvColor;
            RgbColor rgbColor = hsvColor.ToRgb();
            ColorBgra color = ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value);

            Utility.SetNumericUpDownValue(hueUpDown, hsvColor.Hue);
            Utility.SetNumericUpDownValue(saturationUpDown, hsvColor.Saturation);
            Utility.SetNumericUpDownValue(valueUpDown, hsvColor.Value);

            Utility.SetNumericUpDownValue(redUpDown, rgbColor.Red);
            Utility.SetNumericUpDownValue(greenUpDown, rgbColor.Green);
            Utility.SetNumericUpDownValue(blueUpDown, rgbColor.Blue);
            
            string hexText = GetHexNumericUpDownValue(rgbColor.Red, rgbColor.Green, rgbColor.Blue);
            hexBox.Text = hexText;
            
            switch (WhichUserColor)
            {
                case WhichUserColor.Foreground:
                    userForeColor = color;
                    OnUserForeColorChanged(color);
                    OnRelinquishFocus();
                    break;

                case WhichUserColor.Background:
                    userBackColor = color;
                    OnUserBackColorChanged(color);
                    OnRelinquishFocus();
                    break;

                default:
                    throw new InvalidEnumArgumentException("WhichUserColor property");
            }

            Update();
        }

        private void upDown_Enter(object sender, System.EventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;
            nud.Select(0, nud.Text.Length);
        }

        private void upDown_Leave(object sender, System.EventArgs e)
        {
            upDown_ValueChanged(sender, e);
        }

        private void hexUpDown_Enter(object sender, System.EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Select(0, tb.Text.Length);
        }

        private void hexUpDown_Leave(object sender, System.EventArgs e)
        {
            hexBox.Text = hexBox.Text.ToUpper();
            upDown_ValueChanged(sender, e);
        }

        private void hexUpDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (CheckHexBox(tb.Text))
            {
                upDown_ValueChanged(sender, e);
            }
        }

        private bool CheckHexBox(String checkHex)
        {
            int num;
        
            try
            {
                num = int.Parse(checkHex, System.Globalization.NumberStyles.HexNumber);
            }

            catch (FormatException)
            {
                return false;
            }

            catch (OverflowException)
            {
                return false;
            }
        
            if ((num <= 16777215) && (num >= 0))
            {
                return true;
            }   
            else
            {
                return false;
            }
        }

        private void upDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            NumericUpDown nud = (NumericUpDown)sender;

            if (Utility.CheckNumericUpDown(nud))
            {
                upDown_ValueChanged(sender, e);
            }
        }

        private void upDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (sender == alphaUpDown)
            {
                if (alphaTrackBar.Value != (int)alphaUpDown.Value)
                {
                    alphaTrackBar.Value = (int)alphaUpDown.Value;
                }

                PushIgnoreChangedEvents();

                switch (WhichUserColor)
                {
                    case WhichUserColor.Foreground:
                        OnUserForeColorChanged(ColorBgra.FromBgra(lastForeColor.B, lastForeColor.G, lastForeColor.R, (byte)alphaTrackBar.Value));
                        break;

                    case WhichUserColor.Background:
                        OnUserBackColorChanged(ColorBgra.FromBgra(lastBackColor.B, lastBackColor.G, lastBackColor.R, (byte)alphaTrackBar.Value));
                        break;

                    default:
                        throw new InvalidEnumArgumentException("WhichUserColor property");
                }

                PopIgnoreChangedEvents();
            }
            else if (IgnoreChangedEvents)
            {
                return;
            }
            else
            {
                PushIgnoreChangedEvents();

                if (sender == redUpDown || sender == greenUpDown || sender == blueUpDown)
                {
                    string hexText = GetHexNumericUpDownValue((int)redUpDown.Value, (int)greenUpDown.Value, (int)blueUpDown.Value);
                    hexBox.Text = hexText;

                    ColorBgra rgbColor = ColorBgra.FromBgra((byte)blueUpDown.Value, (byte)greenUpDown.Value, (byte)redUpDown.Value, (byte)alphaUpDown.Value);
                
                    SyncHsvFromRgb(rgbColor);
                    OnUserColorChanged(rgbColor);
                }
                else if (sender == hexBox)
                {
                    int hexInt = 0;

                    if (hexBox.Text.Length > 0)
                    {
                        try
                        {
                            hexInt = int.Parse(hexBox.Text,System.Globalization.NumberStyles.HexNumber);
                        }

                        // Needs to be changed so it reads what the RGB values were last
                        catch (FormatException)
                        {
                            hexInt = 0;
                            hexBox.Text = "";
                        }

                        catch (OverflowException)
                        {
                            hexInt = 16777215;
                            hexBox.Text = "FFFFFF";
                        }
        
                        if (!((hexInt <= 16777215) && (hexInt >= 0)))
                        {
                            hexInt = 16777215;
                            hexBox.Text = "FFFFFF";
                        }   
                    }

                    int newRed = ((hexInt & 0xff0000) >> 16);
                    int newGreen = ((hexInt & 0x00ff00) >> 8);
                    int newBlue = (hexInt & 0x0000ff);
                
                    Utility.SetNumericUpDownValue(redUpDown, newRed);
                    Utility.SetNumericUpDownValue(greenUpDown, newGreen);
                    Utility.SetNumericUpDownValue(blueUpDown, newBlue);             

                    ColorBgra rgbColor = ColorBgra.FromBgra((byte)newBlue, (byte)newGreen, (byte)newRed, (byte)alphaUpDown.Value);
                    SyncHsvFromRgb(rgbColor);
                    OnUserColorChanged(rgbColor);
                }
                else if (sender == hueUpDown || sender == saturationUpDown || sender == valueUpDown)
                {
                    HsvColor oldHsvColor = colorWheel.HsvColor;
                    HsvColor newHsvColor = new HsvColor((int)hueUpDown.Value, (int)saturationUpDown.Value, (int)valueUpDown.Value);

                    if (oldHsvColor != newHsvColor)
                    {
                        colorWheel.HsvColor = newHsvColor;

                        if (((colorGradientControl.Value * 100) / 255) != newHsvColor.Value)
                        {
                            colorGradientControl.Value = (newHsvColor.Value * 255) / 100;
                        }

                        colorGradientControl.TopColor = new HsvColor(newHsvColor.Hue, newHsvColor.Saturation, 100).ToColor();
                        colorGradientControl.BottomColor = new HsvColor(newHsvColor.Hue, newHsvColor.Saturation, 0).ToColor();

                        SyncRgbFromHsv(newHsvColor);
                        RgbColor rgbColor = newHsvColor.ToRgb();
                        OnUserColorChanged(ColorBgra.FromBgra((byte)rgbColor.Blue, (byte)rgbColor.Green, (byte)rgbColor.Red, (byte)alphaUpDown.Value));
                    }
                }

                PopIgnoreChangedEvents();
            }
        }

        private void PushIgnoreChangedEvents()
        {
            ignoreChangedEvents.Push(new object());
        }

        private void PopIgnoreChangedEvents()
        {
            ignoreChangedEvents.Pop();
        }

        private void alphaTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (alphaUpDown.Value != (decimal)alphaTrackBar.Value)
            {
                ColorBgra rgbColor = ColorBgra.FromBgra((byte)blueUpDown.Value, (byte)greenUpDown.Value, (byte)redUpDown.Value, (byte)alphaTrackBar.Value);
                OnUserColorChanged(rgbColor);
                OnRelinquishFocus();
                alphaUpDown.Value = (decimal)alphaTrackBar.Value;
            }
        }

        private void moreLessButton_Click(object sender, System.EventArgs e)
        {
            OnRelinquishFocus();

            if (!haveDoneInitStyles)
            {
                EnableStyles(this);
                haveDoneInitStyles = true;
            }

            this.SuspendLayout();

            if (this.inMoreState)
            {
                this.inMoreState = false;
                Size newSize = lessSize;
                this.moreLessButton.Text = this.moreText;
                this.moreLessButton.Location = this.lessModeButtonSentinel.Location;
                this.baseColorHeader.Size = lessModeHeaderSentinel.Size;
                this.baseColorHeader.Top -= 4;

                int widthDelta = (moreModeHeaderSentinel.Width - lessModeHeaderSentinel.Width);
                newSize.Width -= widthDelta;
                this.colorWheel.Width -= widthDelta;
                this.colorWheel.Height -= widthDelta;
                this.colorGradientControl.Left -= widthDelta;

                int heightDelta = (moreModeHeaderSentinel.Height - lessModeHeaderSentinel.Height);
                this.colorGradientControl.Height -= widthDelta;
                this.colorWheel.Top -= 5;
                this.colorGradientControl.Height -= 10;
                newSize.Height -= heightDelta;

                newSize.Height -= 18;

                this.Size = newSize;
            }
            else
            {
                this.inMoreState = true;
                this.moreLessButton.Text = this.lessText;
                this.moreLessButton.Location = this.moreModeButtonSentinel.Location;
                this.baseColorHeader.Size = moreModeHeaderSentinel.Size;
                this.baseColorHeader.Top += 4;

                int widthDelta = (moreModeHeaderSentinel.Width - lessModeHeaderSentinel.Width);
                this.colorWheel.Width += widthDelta;
                this.colorWheel.Height += widthDelta;
                this.colorGradientControl.Left += widthDelta;

                int heightDelta = (moreModeHeaderSentinel.Height - lessModeHeaderSentinel.Height);
                this.colorGradientControl.Height += widthDelta;
                this.colorWheel.Top += 5;
                this.colorGradientControl.Height += 10;

                this.Size = moreSize;
            }

            this.ResumeLayout(false);
        }

        private bool IgnoreChangedEvents
        {
            get
            {
                return ignoreChangedEvents.Count != 0;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            if (this.AttachControl != null)
            {
                this.RepositionForm(this, e);
            }
        }
    }
}
