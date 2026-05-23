/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
    public class ColorsForm :
        FloatingToolForm
    {
        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.NumericUpDown redUpDown;
        private System.Windows.Forms.NumericUpDown greenUpDown;
        private System.Windows.Forms.NumericUpDown blueUpDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown hueUpDown;
        private System.Windows.Forms.NumericUpDown valueUpDown;
        private System.Windows.Forms.NumericUpDown saturationUpDown;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox rgbGroupBox;
        private System.Windows.Forms.GroupBox hsvGroupBox;
        private System.Windows.Forms.ComboBox whichUserColorBox;
        private ColorGradientControl colorGradientControl;
        private System.Windows.Forms.NumericUpDown alphaUpDown;
        private System.Windows.Forms.TrackBar alphaTrackBar;
        private PaintDotNet.ColorWheel colorWheel;
        private System.Windows.Forms.GroupBox alphaGroupBox;

        private Stack ignoreChangedEvents = new Stack();
        private ColorBgra lastForeColor;
        private System.Windows.Forms.Button moreLessButton;
        private ColorBgra lastBackColor;

        private Size moreSize;
        private Size lessSize;
        private System.Windows.Forms.Control lessModeButtonSentinel;
        private System.Windows.Forms.Control moreModeButtonSentinel;
        private System.Windows.Forms.Control lessModeGroupBoxSentinel;
        private System.Windows.Forms.Control moreModeGroupBoxSentinel;
        private bool inMoreState = true;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox hexBox;
        private uint ignore = 0;

        private bool haveDoneInitStyles = false;

        public WhichUserColor WhichUserColor
        {
            get
            {
                return (WhichUserColor)whichUserColorBox.SelectedItem;
            }

            set
            {
                whichUserColorBox.SelectedItem = value;
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
                    rgbGroupBox.Update();
                    hsvGroupBox.Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;
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
                    rgbGroupBox.Update();
                    hsvGroupBox.Update();

                    string hexText = GetHexNumericUpDownValue(value.R, value.G, value.B);
                    hexBox.Text = hexText;
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

                if (hsvColor.Value != ((colorGradientControl.Value * 100) / 255))
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

            whichUserColorBox.Items.Add(WhichUserColor.Foreground);
            whichUserColorBox.Items.Add(WhichUserColor.Background);
            whichUserColorBox.SelectedIndex = 0;

            moreSize = this.Size;
            lessSize = new Size(4 + rgbGroupBox.PointToScreen(new Point(0, 0)).X - Left, moreSize.Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            this.inMoreState = true;
            haveDoneInitStyles = true;
            moreLessButton.PerformClick();
            haveDoneInitStyles = false;
            base.OnLoad (e);
        }

        protected override void OnEnableStyles()
        {
            // do nothing
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.colorWheel = new PaintDotNet.ColorWheel();
            this.redUpDown = new System.Windows.Forms.NumericUpDown();
            this.greenUpDown = new System.Windows.Forms.NumericUpDown();
            this.blueUpDown = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.valueUpDown = new System.Windows.Forms.NumericUpDown();
            this.saturationUpDown = new System.Windows.Forms.NumericUpDown();
            this.hueUpDown = new System.Windows.Forms.NumericUpDown();
            this.rgbGroupBox = new System.Windows.Forms.GroupBox();
            this.hexBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.hsvGroupBox = new System.Windows.Forms.GroupBox();
            this.whichUserColorBox = new System.Windows.Forms.ComboBox();
            this.alphaUpDown = new System.Windows.Forms.NumericUpDown();
            this.alphaTrackBar = new System.Windows.Forms.TrackBar();
            this.alphaGroupBox = new System.Windows.Forms.GroupBox();
            this.moreLessButton = new System.Windows.Forms.Button();
            this.lessModeButtonSentinel = new System.Windows.Forms.Control();
            this.moreModeButtonSentinel = new System.Windows.Forms.Control();
            this.lessModeGroupBoxSentinel = new System.Windows.Forms.Control();
            this.moreModeGroupBoxSentinel = new System.Windows.Forms.Control();
            this.groupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).BeginInit();
            this.rgbGroupBox.SuspendLayout();
            this.hsvGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).BeginInit();
            this.alphaGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // colorGradientControl
            // 
            this.colorGradientControl.BottomColor = System.Drawing.Color.Black;
            this.colorGradientControl.Count = 1;
            this.colorGradientControl.Location = new System.Drawing.Point(192, 16);
            this.colorGradientControl.Name = "colorGradientControl";
            this.colorGradientControl.Size = new System.Drawing.Size(29, 216);
            this.colorGradientControl.TabIndex = 2;
            this.colorGradientControl.TabStop = false;
            this.colorGradientControl.TopColor = System.Drawing.Color.White;
            this.colorGradientControl.Value = 0;
            this.colorGradientControl.ValueChanged += new IndexEventHandler(this.colorGradientControl_ValueChanged);
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.colorWheel);
            this.groupBox.Controls.Add(this.colorGradientControl);
            this.groupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox.Location = new System.Drawing.Point(8, 40);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(232, 248);
            this.groupBox.TabIndex = 3;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Choose Base Color";
            // 
            // colorWheel
            // 
            this.colorWheel.Location = new System.Drawing.Point(8, 32);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new System.Drawing.Size(184, 184);
            this.colorWheel.TabIndex = 3;
            this.colorWheel.TabStop = false;
            this.colorWheel.ColorChanged += new System.EventHandler(this.colorWheel_ColorChanged);
            // 
            // redUpDown
            // 
            this.redUpDown.Location = new System.Drawing.Point(80, 16);
            this.redUpDown.Maximum = new System.Decimal(new int[] {
                                                                      255,
                                                                      0,
                                                                      0,
                                                                      0});
            this.redUpDown.Name = "redUpDown";
            this.redUpDown.Size = new System.Drawing.Size(56, 20);
            this.redUpDown.TabIndex = 1;
            this.redUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.redUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.redUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.redUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.redUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // greenUpDown
            // 
            this.greenUpDown.Location = new System.Drawing.Point(80, 40);
            this.greenUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.greenUpDown.Name = "greenUpDown";
            this.greenUpDown.Size = new System.Drawing.Size(56, 20);
            this.greenUpDown.TabIndex = 2;
            this.greenUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.greenUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.greenUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.greenUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.greenUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // blueUpDown
            // 
            this.blueUpDown.Location = new System.Drawing.Point(80, 64);
            this.blueUpDown.Maximum = new System.Decimal(new int[] {
                                                                       255,
                                                                       0,
                                                                       0,
                                                                       0});
            this.blueUpDown.Name = "blueUpDown";
            this.blueUpDown.Size = new System.Drawing.Size(56, 20);
            this.blueUpDown.TabIndex = 3;
            this.blueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.blueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.blueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.blueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.blueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 24);
            this.label1.TabIndex = 7;
            this.label1.Text = "Red:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label2.Location = new System.Drawing.Point(32, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 24);
            this.label2.TabIndex = 8;
            this.label2.Text = "Blue:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label3.Location = new System.Drawing.Point(32, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 24);
            this.label3.TabIndex = 9;
            this.label3.Text = "Green:";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(8, 40);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(62, 24);
            this.label7.TabIndex = 16;
            this.label7.Text = "Saturation:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(32, 64);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(40, 24);
            this.label8.TabIndex = 15;
            this.label8.Text = "Value:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(32, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 24);
            this.label6.TabIndex = 14;
            this.label6.Text = "Hue:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // valueUpDown
            // 
            this.valueUpDown.Location = new System.Drawing.Point(80, 64);
            this.valueUpDown.Name = "valueUpDown";
            this.valueUpDown.Size = new System.Drawing.Size(56, 20);
            this.valueUpDown.TabIndex = 6;
            this.valueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.valueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.valueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.valueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.valueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // saturationUpDown
            // 
            this.saturationUpDown.Location = new System.Drawing.Point(80, 40);
            this.saturationUpDown.Name = "saturationUpDown";
            this.saturationUpDown.Size = new System.Drawing.Size(56, 20);
            this.saturationUpDown.TabIndex = 5;
            this.saturationUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.saturationUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.saturationUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.saturationUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.saturationUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // hueUpDown
            // 
            this.hueUpDown.Location = new System.Drawing.Point(80, 16);
            this.hueUpDown.Maximum = new System.Decimal(new int[] {
                                                                      360,
                                                                      0,
                                                                      0,
                                                                      0});
            this.hueUpDown.Name = "hueUpDown";
            this.hueUpDown.Size = new System.Drawing.Size(56, 20);
            this.hueUpDown.TabIndex = 4;
            this.hueUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hueUpDown.ValueChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.hueUpDown.Enter += new System.EventHandler(this.upDown_Enter);
            this.hueUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.upDown_KeyUp);
            this.hueUpDown.Leave += new System.EventHandler(this.upDown_Leave);
            // 
            // rgbGroupBox
            // 
            this.rgbGroupBox.Controls.Add(this.hexBox);
            this.rgbGroupBox.Controls.Add(this.label4);
            this.rgbGroupBox.Controls.Add(this.redUpDown);
            this.rgbGroupBox.Controls.Add(this.greenUpDown);
            this.rgbGroupBox.Controls.Add(this.blueUpDown);
            this.rgbGroupBox.Controls.Add(this.label1);
            this.rgbGroupBox.Controls.Add(this.label2);
            this.rgbGroupBox.Controls.Add(this.label3);
            this.rgbGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rgbGroupBox.Location = new System.Drawing.Point(248, 8);
            this.rgbGroupBox.Name = "rgbGroupBox";
            this.rgbGroupBox.Size = new System.Drawing.Size(144, 120);
            this.rgbGroupBox.TabIndex = 17;
            this.rgbGroupBox.TabStop = false;
            this.rgbGroupBox.Text = "RGB";
            // 
            // hexBox
            // 
            this.hexBox.Location = new System.Drawing.Point(80, 88);
            this.hexBox.Name = "hexBox";
            this.hexBox.Size = new System.Drawing.Size(56, 20);
            this.hexBox.TabIndex = 14;
            this.hexBox.Text = "000000";
            this.hexBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.hexBox.TextChanged += new System.EventHandler(this.upDown_ValueChanged);
            this.hexBox.Leave += new System.EventHandler(this.hexUpDown_Leave);
            this.hexBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.hexUpDown_KeyUp);
            this.hexBox.Enter += new System.EventHandler(this.hexUpDown_Enter);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.label4.Location = new System.Drawing.Point(40, 88);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 24);
            this.label4.TabIndex = 13;
            this.label4.Text = "Hex:";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // hsvGroupBox
            // 
            this.hsvGroupBox.Controls.Add(this.label8);
            this.hsvGroupBox.Controls.Add(this.saturationUpDown);
            this.hsvGroupBox.Controls.Add(this.valueUpDown);
            this.hsvGroupBox.Controls.Add(this.label6);
            this.hsvGroupBox.Controls.Add(this.hueUpDown);
            this.hsvGroupBox.Controls.Add(this.label7);
            this.hsvGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.hsvGroupBox.Location = new System.Drawing.Point(248, 136);
            this.hsvGroupBox.Name = "hsvGroupBox";
            this.hsvGroupBox.Size = new System.Drawing.Size(144, 96);
            this.hsvGroupBox.TabIndex = 18;
            this.hsvGroupBox.TabStop = false;
            this.hsvGroupBox.Text = "HSV";
            // 
            // whichUserColorBox
            // 
            this.whichUserColorBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.whichUserColorBox.Location = new System.Drawing.Point(8, 8);
            this.whichUserColorBox.Name = "whichUserColorBox";
            this.whichUserColorBox.Size = new System.Drawing.Size(121, 21);
            this.whichUserColorBox.TabIndex = 0;
            this.whichUserColorBox.SelectedIndexChanged += new System.EventHandler(this.whichUserColorBox_SelectedIndexChanged);
            // 
            // alphaUpDown
            // 
            this.alphaUpDown.Location = new System.Drawing.Point(80, 16);
            this.alphaUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.alphaUpDown.Name = "alphaUpDown";
            this.alphaUpDown.Size = new System.Drawing.Size(56, 20);
            this.alphaUpDown.TabIndex = 7;
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
            this.alphaTrackBar.Location = new System.Drawing.Point(8, 16);
            this.alphaTrackBar.Maximum = 255;
            this.alphaTrackBar.Name = "alphaTrackBar";
            this.alphaTrackBar.Size = new System.Drawing.Size(64, 25);
            this.alphaTrackBar.TabIndex = 8;
            this.alphaTrackBar.TickFrequency = 64;
            this.alphaTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.alphaTrackBar.ValueChanged += new System.EventHandler(this.alphaTrackBar_ValueChanged);
            // 
            // alphaGroupBox
            // 
            this.alphaGroupBox.Controls.Add(this.alphaUpDown);
            this.alphaGroupBox.Controls.Add(this.alphaTrackBar);
            this.alphaGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.alphaGroupBox.Location = new System.Drawing.Point(248, 240);
            this.alphaGroupBox.Name = "alphaGroupBox";
            this.alphaGroupBox.Size = new System.Drawing.Size(144, 48);
            this.alphaGroupBox.TabIndex = 20;
            this.alphaGroupBox.TabStop = false;
            this.alphaGroupBox.Text = "Transparency - Alpha";
            // 
            // moreLessButton
            // 
            this.moreLessButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.moreLessButton.Location = new System.Drawing.Point(165, 7);
            this.moreLessButton.Name = "moreLessButton";
            this.moreLessButton.TabIndex = 21;
            this.moreLessButton.Text = "<< Less";
            this.moreLessButton.Click += new System.EventHandler(this.moreLessButton_Click);
            // 
            // lessModeButtonSentinel
            // 
            this.lessModeButtonSentinel.Location = new System.Drawing.Point(136, 7);
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
            this.moreModeButtonSentinel.Text = "control1";
            this.moreModeButtonSentinel.Visible = false;
            // 
            // lessModeGroupBoxSentinel
            // 
            this.lessModeGroupBoxSentinel.Location = new System.Drawing.Point(8, 40);
            this.lessModeGroupBoxSentinel.Name = "lessModeGroupBoxSentinel";
            this.lessModeGroupBoxSentinel.Size = new System.Drawing.Size(200, 192);
            this.lessModeGroupBoxSentinel.TabIndex = 24;
            this.lessModeGroupBoxSentinel.Text = "control1";
            this.lessModeGroupBoxSentinel.Visible = false;
            // 
            // moreModeGroupBoxSentinel
            // 
            this.moreModeGroupBoxSentinel.Location = new System.Drawing.Point(8, 40);
            this.moreModeGroupBoxSentinel.Name = "moreModeGroupBoxSentinel";
            this.moreModeGroupBoxSentinel.Size = new System.Drawing.Size(232, 248);
            this.moreModeGroupBoxSentinel.TabIndex = 25;
            this.moreModeGroupBoxSentinel.Text = "control1";
            this.moreModeGroupBoxSentinel.Visible = false;
            // 
            // ColorsForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(402, 296);
            this.Controls.Add(this.moreModeButtonSentinel);
            this.Controls.Add(this.lessModeButtonSentinel);
            this.Controls.Add(this.moreLessButton);
            this.Controls.Add(this.hsvGroupBox);
            this.Controls.Add(this.whichUserColorBox);
            this.Controls.Add(this.rgbGroupBox);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.alphaGroupBox);
            this.Controls.Add(this.lessModeGroupBoxSentinel);
            this.Controls.Add(this.moreModeGroupBoxSentinel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ColorsForm";
            this.Text = "Colors";
            this.Controls.SetChildIndex(this.moreModeGroupBoxSentinel, 0);
            this.Controls.SetChildIndex(this.lessModeGroupBoxSentinel, 0);
            this.Controls.SetChildIndex(this.alphaGroupBox, 0);
            this.Controls.SetChildIndex(this.groupBox, 0);
            this.Controls.SetChildIndex(this.rgbGroupBox, 0);
            this.Controls.SetChildIndex(this.whichUserColorBox, 0);
            this.Controls.SetChildIndex(this.hsvGroupBox, 0);
            this.Controls.SetChildIndex(this.moreLessButton, 0);
            this.Controls.SetChildIndex(this.lessModeButtonSentinel, 0);
            this.Controls.SetChildIndex(this.moreModeButtonSentinel, 0);
            this.groupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.redUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.greenUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.saturationUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hueUpDown)).EndInit();
            this.rgbGroupBox.ResumeLayout(false);
            this.hsvGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.alphaUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.alphaTrackBar)).EndInit();
            this.alphaGroupBox.ResumeLayout(false);
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
                this.moreLessButton.Text = "More >>";
                this.moreLessButton.Location = this.lessModeButtonSentinel.Location;
                this.groupBox.Size = lessModeGroupBoxSentinel.Size;
                this.groupBox.Top -= 4;

                int widthDelta = (moreModeGroupBoxSentinel.Width - lessModeGroupBoxSentinel.Width);
                newSize.Width -= widthDelta;
                this.colorWheel.Width -= widthDelta;
                this.colorWheel.Height -= widthDelta;
                this.colorGradientControl.Left -= widthDelta;

                int heightDelta = (moreModeGroupBoxSentinel.Height - lessModeGroupBoxSentinel.Height);
                this.colorGradientControl.Height -= widthDelta;
                this.colorWheel.Top -= 8;
                this.colorGradientControl.Height -= 20;
                newSize.Height -= heightDelta;

                this.Size = newSize;
            }
            else
            {
                this.inMoreState = true;
                this.moreLessButton.Text = "<< Less";
                this.moreLessButton.Location = this.moreModeButtonSentinel.Location;
                this.groupBox.Size = moreModeGroupBoxSentinel.Size;
                this.groupBox.Top += 4;

                int widthDelta = (moreModeGroupBoxSentinel.Width - lessModeGroupBoxSentinel.Width);
                this.colorWheel.Width += widthDelta;
                this.colorWheel.Height += widthDelta;
                this.colorGradientControl.Left += widthDelta;

                int heightDelta = (moreModeGroupBoxSentinel.Height - lessModeGroupBoxSentinel.Height);
                this.colorGradientControl.Height += widthDelta;
                this.colorWheel.Top += 8;
                this.colorGradientControl.Height += 20;

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
