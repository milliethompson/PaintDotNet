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
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for LevelsEffectConfigDialog.
    /// </summary>
    public class LevelsEffectConfigDialog 
        : EffectConfigDialog
    {
        private bool[] mask = new bool[3];
        private PaintDotNet.ColorGradientControl gradientOutput;
        private System.Windows.Forms.NumericUpDown numericUpDown5;
        private System.Windows.Forms.GroupBox outputGroupBox;
        private System.Windows.Forms.NumericUpDown outputHiUpDown;
        private System.Windows.Forms.NumericUpDown outputLowUpDown;
        private System.Windows.Forms.NumericUpDown outputGammaUpDown;
        private PaintDotNet.HistogramControl histogramOutput;
        private System.Windows.Forms.Panel panelOutput;
        private uint ignore = 0;
        private System.Windows.Forms.Panel swatchOutHigh;
        private System.Windows.Forms.Panel swatchOutLow;
        private System.Windows.Forms.Panel panelMask;
        private System.Windows.Forms.CheckBox redMaskCheckBox;
        private System.Windows.Forms.CheckBox greenMaskCheckBox;
        private System.Windows.Forms.CheckBox blueMaskCheckBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button autoButton;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Panel panelInput;
        private PaintDotNet.HistogramControl histogramInput;
        private System.Windows.Forms.GroupBox inputGroupBox;
        private System.Windows.Forms.Panel swatchInHigh;
        private System.Windows.Forms.NumericUpDown inputLoUpDown;
        private System.Windows.Forms.NumericUpDown inputHiUpDown;
        private PaintDotNet.ColorGradientControl gradientInput;
        private System.Windows.Forms.Panel swatchInLow;
        private System.Windows.Forms.Panel panelAdjustments;
        private System.Windows.Forms.ToolTip tooltipProvider;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.GroupBox outputHistogramGroupBox;
        private System.Windows.Forms.GroupBox inputHistogramGroupBox;
        private System.Windows.Forms.Panel swatchOutMid;
    
        public LevelsEffectConfigDialog()
        {
            InitializeComponent();

            this.Text = PdnResources.GetString("LevelsEffectConfigDialog.Text");
            this.outputGroupBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.OutputGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.outputGammaUpDown, PdnResources.GetString("LevelsEffectConfigDialog.OutputGammaUpDown.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchOutHigh, PdnResources.GetString("LevelsEffectConfigDialog.SwatchOutHigh.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchOutLow, PdnResources.GetString("LevelsEffectConfigDialog.SwatchOutLow.ToolTipText"));
            this.outputHistogramGroupBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.OutputHistogramGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.histogramOutput, PdnResources.GetString("LevelsEffectConfigDialog.HistogramOutput.ToolTipText"));
            this.inputHistogramGroupBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.InputHistogramGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.histogramInput, PdnResources.GetString("LevelsEffectConfigDialog.HistogramInput.ToolTipText"));
            this.inputGroupBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.InputGroupBox.Text");
            this.tooltipProvider.SetToolTip(this.swatchInHigh, PdnResources.GetString("LevelsEffectConfigDialog.SwatchInHigh.ToolTipText"));
            this.tooltipProvider.SetToolTip(this.swatchInLow, PdnResources.GetString("LevelsEffectConfigDialog.SwatchInLow.ToolTipText"));
            this.redMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.RedMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.redMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.RedMaskCheckBox.ToolTipText"));
            this.greenMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.GreenMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.greenMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.GreenMaskCheckBox.ToolTipText"));
            this.blueMaskCheckBox.Text = PdnResources.GetString("LevelsEffectConfigDialog.BlueMaskCheckBox.Text");
            this.tooltipProvider.SetToolTip(this.blueMaskCheckBox, PdnResources.GetString("LevelsEffectConfigDialog.BlueMaskCheckBox.ToolTipText"));
            this.okButton.Text = PdnResources.GetString("Form.OkButton.Text");
            this.cancelButton.Text = PdnResources.GetString("Form.CancelButton.Text");
            this.autoButton.Text = PdnResources.GetString("LevelsEffectConfigDialog.AutoButton.Text");
            this.tooltipProvider.SetToolTip(this.autoButton, PdnResources.GetString("LevelsEffectConfigDialog.AutoButton.ToolTipText"));
            this.resetButton.Text = PdnResources.GetString("LevelsEffectConfigDialog.ResetButton.Text");

        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.gradientOutput = new PaintDotNet.ColorGradientControl();
            this.outputHiUpDown = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
            this.outputLowUpDown = new System.Windows.Forms.NumericUpDown();
            this.outputGroupBox = new System.Windows.Forms.GroupBox();
            this.outputGammaUpDown = new System.Windows.Forms.NumericUpDown();
            this.swatchOutHigh = new System.Windows.Forms.Panel();
            this.swatchOutLow = new System.Windows.Forms.Panel();
            this.swatchOutMid = new System.Windows.Forms.Panel();
            this.outputHistogramGroupBox = new System.Windows.Forms.GroupBox();
            this.histogramOutput = new PaintDotNet.HistogramControl();
            this.panelOutput = new System.Windows.Forms.Panel();
            this.panelAdjustments = new System.Windows.Forms.Panel();
            this.panelInput = new System.Windows.Forms.Panel();
            this.inputHistogramGroupBox = new System.Windows.Forms.GroupBox();
            this.histogramInput = new PaintDotNet.HistogramControl();
            this.inputGroupBox = new System.Windows.Forms.GroupBox();
            this.swatchInHigh = new System.Windows.Forms.Panel();
            this.inputLoUpDown = new System.Windows.Forms.NumericUpDown();
            this.inputHiUpDown = new System.Windows.Forms.NumericUpDown();
            this.gradientInput = new PaintDotNet.ColorGradientControl();
            this.swatchInLow = new System.Windows.Forms.Panel();
            this.panelMask = new System.Windows.Forms.Panel();
            this.redMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.greenMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.blueMaskCheckBox = new System.Windows.Forms.CheckBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.autoButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.outputHiUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputLowUpDown)).BeginInit();
            this.outputGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.outputGammaUpDown)).BeginInit();
            this.outputHistogramGroupBox.SuspendLayout();
            this.panelOutput.SuspendLayout();
            this.panelAdjustments.SuspendLayout();
            this.panelInput.SuspendLayout();
            this.inputHistogramGroupBox.SuspendLayout();
            this.inputGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.inputLoUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHiUpDown)).BeginInit();
            this.panelMask.SuspendLayout();
            this.SuspendLayout();
            // 
            // gradientOutput
            // 
            this.gradientOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left)));
            this.gradientOutput.BottomColor = System.Drawing.Color.Black;
            this.gradientOutput.Count = 3;
            this.gradientOutput.Location = new System.Drawing.Point(8, 16);
            this.gradientOutput.Name = "gradientOutput";
            this.gradientOutput.Size = new System.Drawing.Size(32, 166);
            this.gradientOutput.TabIndex = 0;
            this.gradientOutput.TopColor = System.Drawing.Color.White;
            this.gradientOutput.Value = 0;
            this.gradientOutput.ValueChanged += new IndexEventHandler(this.gradientOutput_ValueChanged);
            // 
            // outputHiUpDown
            // 
            this.outputHiUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputHiUpDown.Location = new System.Drawing.Point(46, 16);
            this.outputHiUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.outputHiUpDown.Name = "outputHiUpDown";
            this.outputHiUpDown.Size = new System.Drawing.Size(48, 20);
            this.outputHiUpDown.TabIndex = 1;
            this.outputHiUpDown.Value = new System.Decimal(new int[] {
                                                                      255,
                                                                      0,
                                                                      0,
                                                                      0});
            this.outputHiUpDown.Validated += new System.EventHandler(this.outputHiUpDown_ValueChanged);
            this.outputHiUpDown.ValueChanged += new System.EventHandler(this.outputHiUpDown_ValueChanged);
            // 
            // numericUpDown5
            // 
            this.numericUpDown5.Location = new System.Drawing.Point(-24, 24);
            this.numericUpDown5.Name = "numericUpDown5";
            this.numericUpDown5.Size = new System.Drawing.Size(24, 20);
            this.numericUpDown5.TabIndex = 1;
            // 
            // outputLowUpDown
            // 
            this.outputLowUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.outputLowUpDown.Location = new System.Drawing.Point(46, 158);
            this.outputLowUpDown.Maximum = new System.Decimal(new int[] {
                                                                        255,
                                                                        0,
                                                                        0,
                                                                        0});
            this.outputLowUpDown.Name = "outputLowUpDown";
            this.outputLowUpDown.Size = new System.Drawing.Size(48, 20);
            this.outputLowUpDown.TabIndex = 1;
            this.outputLowUpDown.Validated += new System.EventHandler(this.outputLowUpDown_ValueChanged);
            this.outputLowUpDown.ValueChanged += new System.EventHandler(this.outputLowUpDown_ValueChanged);
            // 
            // outputGroupBox
            // 
            this.outputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left)));
            this.outputGroupBox.Controls.Add(this.outputGammaUpDown);
            this.outputGroupBox.Controls.Add(this.numericUpDown5);
            this.outputGroupBox.Controls.Add(this.outputLowUpDown);
            this.outputGroupBox.Controls.Add(this.outputHiUpDown);
            this.outputGroupBox.Controls.Add(this.gradientOutput);
            this.outputGroupBox.Controls.Add(this.swatchOutHigh);
            this.outputGroupBox.Controls.Add(this.swatchOutLow);
            this.outputGroupBox.Controls.Add(this.swatchOutMid);
            this.outputGroupBox.Location = new System.Drawing.Point(2, 0);
            this.outputGroupBox.Name = "outputGroupBox";
            this.outputGroupBox.Size = new System.Drawing.Size(102, 190);
            this.outputGroupBox.TabIndex = 2;
            this.outputGroupBox.TabStop = false;
            this.outputGroupBox.Layout += new System.Windows.Forms.LayoutEventHandler(this.grpOutput_Layout);
            // 
            // outputGammaUpDown
            // 
            this.outputGammaUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputGammaUpDown.DecimalPlaces = 2;
            this.outputGammaUpDown.Increment = new System.Decimal(new int[] {
                                                                             1,
                                                                             0,
                                                                             0,
                                                                             65536});
            this.outputGammaUpDown.Location = new System.Drawing.Point(46, 73);
            this.outputGammaUpDown.Maximum = new System.Decimal(new int[] {
                                                                           100,
                                                                           0,
                                                                           0,
                                                                           65536});
            this.outputGammaUpDown.Minimum = new System.Decimal(new int[] {
                                                                           1,
                                                                           0,
                                                                           0,
                                                                           65536});
            this.outputGammaUpDown.Name = "outputGammaUpDown";
            this.outputGammaUpDown.Size = new System.Drawing.Size(48, 20);
            this.outputGammaUpDown.TabIndex = 1;
            this.outputGammaUpDown.Value = new System.Decimal(new int[] {
                                                                         1,
                                                                         0,
                                                                         0,
                                                                         0});
            this.outputGammaUpDown.Validated += new System.EventHandler(this.outputGammaUpDown_ValueChanged);
            this.outputGammaUpDown.ValueChanged += new System.EventHandler(this.outputGammaUpDown_ValueChanged);
            // 
            // swatchOutHigh
            // 
            this.swatchOutHigh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.swatchOutHigh.BackColor = System.Drawing.Color.White;
            this.swatchOutHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutHigh.Location = new System.Drawing.Point(46, 37);
            this.swatchOutHigh.Name = "swatchOutHigh";
            this.swatchOutHigh.Size = new System.Drawing.Size(48, 24);
            this.swatchOutHigh.TabIndex = 2;
            this.swatchOutHigh.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
            // 
            // swatchOutLow
            // 
            this.swatchOutLow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.swatchOutLow.BackColor = System.Drawing.Color.Black;
            this.swatchOutLow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutLow.Location = new System.Drawing.Point(46, 133);
            this.swatchOutLow.Name = "swatchOutLow";
            this.swatchOutLow.Size = new System.Drawing.Size(48, 24);
            this.swatchOutLow.TabIndex = 2;
            this.swatchOutLow.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
            // 
            // swatchOutMid
            // 
            this.swatchOutMid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.swatchOutMid.BackColor = System.Drawing.Color.White;
            this.swatchOutMid.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchOutMid.Location = new System.Drawing.Point(46, 97);
            this.swatchOutMid.Name = "swatchOutMid";
            this.swatchOutMid.Size = new System.Drawing.Size(48, 24);
            this.swatchOutMid.TabIndex = 2;
            this.swatchOutMid.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
            // 
            // outputHistogramGroupBox
            // 
            this.outputHistogramGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.outputHistogramGroupBox.Controls.Add(this.histogramOutput);
            this.outputHistogramGroupBox.Location = new System.Drawing.Point(108, 0);
            this.outputHistogramGroupBox.Name = "outputHistogramGroupBox";
            this.outputHistogramGroupBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.outputHistogramGroupBox.Size = new System.Drawing.Size(130, 190);
            this.outputHistogramGroupBox.TabIndex = 3;
            this.outputHistogramGroupBox.TabStop = false;
            // 
            // histogramOutput
            // 
            this.histogramOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.histogramOutput.FlipHorizontal = false;
            this.histogramOutput.FlipVertical = false;
            this.histogramOutput.Location = new System.Drawing.Point(8, 16);
            this.histogramOutput.Name = "histogramOutput";
            this.histogramOutput.Size = new System.Drawing.Size(114, 166);
            this.histogramOutput.TabIndex = 0;
            this.histogramOutput.Histogram = new HistogramRgb();
            // 
            // panelOutput
            // 
            this.panelOutput.Controls.Add(this.outputHistogramGroupBox);
            this.panelOutput.Controls.Add(this.outputGroupBox);
            this.panelOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelOutput.Location = new System.Drawing.Point(240, 0);
            this.panelOutput.Name = "panelOutput";
            this.panelOutput.Size = new System.Drawing.Size(240, 192);
            this.panelOutput.TabIndex = 5;
            // 
            // panelAdjustments
            // 
            this.panelAdjustments.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdjustments.Controls.Add(this.panelOutput);
            this.panelAdjustments.Controls.Add(this.panelInput);
            this.panelAdjustments.Location = new System.Drawing.Point(0, 0);
            this.panelAdjustments.Name = "panelAdjustments";
            this.panelAdjustments.Size = new System.Drawing.Size(480, 192);
            this.panelAdjustments.TabIndex = 7;
            this.panelAdjustments.Layout += new System.Windows.Forms.LayoutEventHandler(this.panelAdjustments_Layout);
            // 
            // panelInput
            // 
            this.panelInput.Controls.Add(this.inputHistogramGroupBox);
            this.panelInput.Controls.Add(this.inputGroupBox);
            this.panelInput.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelInput.Location = new System.Drawing.Point(0, 0);
            this.panelInput.Name = "panelInput";
            this.panelInput.Size = new System.Drawing.Size(240, 192);
            this.panelInput.TabIndex = 6;
            // 
            // inputHistogramGroupBox
            // 
            this.inputHistogramGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.inputHistogramGroupBox.Controls.Add(this.histogramInput);
            this.inputHistogramGroupBox.Location = new System.Drawing.Point(2, 0);
            this.inputHistogramGroupBox.Name = "inputHistogramGroupBox";
            this.inputHistogramGroupBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.inputHistogramGroupBox.Size = new System.Drawing.Size(130, 190);
            this.inputHistogramGroupBox.TabIndex = 3;
            this.inputHistogramGroupBox.TabStop = false;
            // 
            // histogramInput
            // 
            this.histogramInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.histogramInput.FlipHorizontal = true;
            this.histogramInput.FlipVertical = false;
            this.histogramInput.Location = new System.Drawing.Point(8, 16);
            this.histogramInput.Name = "histogramInput";
            this.histogramInput.Size = new System.Drawing.Size(114, 166);
            this.histogramInput.TabIndex = 0;
            this.histogramInput.Histogram = new HistogramRgb();
            // 
            // inputGroupBox
            // 
            this.inputGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.inputGroupBox.Controls.Add(this.swatchInHigh);
            this.inputGroupBox.Controls.Add(this.inputLoUpDown);
            this.inputGroupBox.Controls.Add(this.inputHiUpDown);
            this.inputGroupBox.Controls.Add(this.gradientInput);
            this.inputGroupBox.Controls.Add(this.swatchInLow);
            this.inputGroupBox.Location = new System.Drawing.Point(136, 0);
            this.inputGroupBox.Name = "inputGroupBox";
            this.inputGroupBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.inputGroupBox.Size = new System.Drawing.Size(102, 190);
            this.inputGroupBox.TabIndex = 2;
            this.inputGroupBox.TabStop = false;
            // 
            // swatchInHigh
            // 
            this.swatchInHigh.BackColor = System.Drawing.Color.White;
            this.swatchInHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchInHigh.Location = new System.Drawing.Point(8, 37);
            this.swatchInHigh.Name = "swatchInHigh";
            this.swatchInHigh.Size = new System.Drawing.Size(48, 24);
            this.swatchInHigh.TabIndex = 2;
            this.swatchInHigh.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
            // 
            // inputLoUpDown
            // 
            this.inputLoUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.inputLoUpDown.Location = new System.Drawing.Point(8, 158);
            this.inputLoUpDown.Maximum = new System.Decimal(new int[] {
                                                                       255,
                                                                       0,
                                                                       0,
                                                                       0});
            this.inputLoUpDown.Name = "inputLoUpDown";
            this.inputLoUpDown.Size = new System.Drawing.Size(48, 20);
            this.inputLoUpDown.TabIndex = 1;
            this.inputLoUpDown.Validated += new System.EventHandler(this.txtInputLo_ValueChanged);
            this.inputLoUpDown.ValueChanged += new System.EventHandler(this.txtInputLo_ValueChanged);
            // 
            // inputHiUpDown
            // 
            this.inputHiUpDown.Location = new System.Drawing.Point(8, 16);
            this.inputHiUpDown.Maximum = new System.Decimal(new int[] {
                                                                       255,
                                                                       0,
                                                                       0,
                                                                       0});
            this.inputHiUpDown.Name = "inputHiUpDown";
            this.inputHiUpDown.Size = new System.Drawing.Size(48, 20);
            this.inputHiUpDown.TabIndex = 1;
            this.inputHiUpDown.Value = new System.Decimal(new int[] {
                                                                     255,
                                                                     0,
                                                                     0,
                                                                     0});
            this.inputHiUpDown.Validated += new System.EventHandler(this.txtInputHi_ValueChanged);
            this.inputHiUpDown.ValueChanged += new System.EventHandler(this.txtInputHi_ValueChanged);
            // 
            // gradientInput
            // 
            this.gradientInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.gradientInput.BottomColor = System.Drawing.Color.Black;
            this.gradientInput.Count = 2;
            this.gradientInput.Location = new System.Drawing.Point(62, 16);
            this.gradientInput.Name = "gradientInput";
            this.gradientInput.Size = new System.Drawing.Size(32, 166);
            this.gradientInput.TabIndex = 0;
            this.gradientInput.TopColor = System.Drawing.Color.White;
            this.gradientInput.Value = 0;
            this.gradientInput.ValueChanged += new IndexEventHandler(this.gradientInput_ValueChanged);
            // 
            // swatchInLow
            // 
            this.swatchInLow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.swatchInLow.BackColor = System.Drawing.Color.Black;
            this.swatchInLow.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.swatchInLow.Location = new System.Drawing.Point(8, 133);
            this.swatchInLow.Name = "swatchInLow";
            this.swatchInLow.Size = new System.Drawing.Size(48, 24);
            this.swatchInLow.TabIndex = 2;
            this.swatchInLow.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
            // 
            // panelMask
            // 
            this.panelMask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.panelMask.Controls.Add(this.redMaskCheckBox);
            this.panelMask.Controls.Add(this.greenMaskCheckBox);
            this.panelMask.Controls.Add(this.blueMaskCheckBox);
            this.panelMask.Location = new System.Drawing.Point(192, 200);
            this.panelMask.Name = "panelMask";
            this.panelMask.Size = new System.Drawing.Size(96, 24);
            this.panelMask.TabIndex = 14;
            // 
            // redMaskCheckBox
            // 
            this.redMaskCheckBox.Checked = true;
            this.redMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.redMaskCheckBox.Location = new System.Drawing.Point(0, 0);
            this.redMaskCheckBox.Name = "redMaskCheckBox";
            this.redMaskCheckBox.Size = new System.Drawing.Size(32, 24);
            this.redMaskCheckBox.TabIndex = 1;
            this.redMaskCheckBox.Click += new System.EventHandler(this.redMaskCheckBox_CheckedChanged);
            // 
            // greenMaskCheckBox
            // 
            this.greenMaskCheckBox.Checked = true;
            this.greenMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.greenMaskCheckBox.Location = new System.Drawing.Point(32, 0);
            this.greenMaskCheckBox.Name = "greenMaskCheckBox";
            this.greenMaskCheckBox.Size = new System.Drawing.Size(32, 24);
            this.greenMaskCheckBox.TabIndex = 1;
            this.greenMaskCheckBox.Click += new System.EventHandler(this.greenMaskCheckBox_CheckedChanged);
            // 
            // blueMaskCheckBox
            // 
            this.blueMaskCheckBox.Checked = true;
            this.blueMaskCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.blueMaskCheckBox.Location = new System.Drawing.Point(64, 0);
            this.blueMaskCheckBox.Name = "blueMaskCheckBox";
            this.blueMaskCheckBox.Size = new System.Drawing.Size(32, 24);
            this.blueMaskCheckBox.TabIndex = 1;
            this.blueMaskCheckBox.Click += new System.EventHandler(this.blueMaskCheckBox_CheckedChanged);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(312, 200);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 11;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(400, 200);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // autoButton
            // 
            this.autoButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.autoButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.autoButton.Location = new System.Drawing.Point(8, 200);
            this.autoButton.Name = "autoButton";
            this.autoButton.TabIndex = 10;
            this.autoButton.Click += new System.EventHandler(this.autoButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.resetButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.resetButton.Location = new System.Drawing.Point(88, 200);
            this.resetButton.Name = "resetButton";
            this.resetButton.TabIndex = 12;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // LevelsEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(480, 229);
            this.Controls.Add(this.panelMask);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.autoButton);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.panelAdjustments);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = true;
            this.MinimumSize = new System.Drawing.Size(439, 231);
            this.Name = "LevelsEffectConfigDialog";
            this.Load += new System.EventHandler(this.LevelsEffectConfigDialog_Load);
            this.Controls.SetChildIndex(this.panelAdjustments, 0);
            this.Controls.SetChildIndex(this.resetButton, 0);
            this.Controls.SetChildIndex(this.autoButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.panelMask, 0);
            ((System.ComponentModel.ISupportInitialize)(this.outputHiUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputLowUpDown)).EndInit();
            this.outputGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.outputGammaUpDown)).EndInit();
            this.outputHistogramGroupBox.ResumeLayout(false);
            this.panelOutput.ResumeLayout(false);
            this.panelAdjustments.ResumeLayout(false);
            this.panelInput.ResumeLayout(false);
            this.inputHistogramGroupBox.ResumeLayout(false);
            this.inputGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.inputLoUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inputHiUpDown)).EndInit();
            this.panelMask.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    
        private void MaskChanged() 
        {
            bool anyOn = mask[0] || mask[1] || mask[2];

            inputGroupBox.Enabled = anyOn;
            outputGroupBox.Enabled = anyOn;

            ColorBgra top = ColorBgra.Black;

            top.Bgra |= mask[0] ? (uint)0xFF : 0;
            top.Bgra |= mask[1] ? (uint)0xFF00 : 0;
            top.Bgra |= mask[2] ? (uint)0xFF0000 : 0;

            gradientInput.TopColor = top.ToColor();
            gradientOutput.TopColor = top.ToColor();

            for (int i = 0; i < 3; ++i)
            {
                histogramInput.SetSelected(i, mask[i]);
                histogramOutput.SetSelected(i, mask[i]);
            }

            ignore++;
            InitDialogFromToken();
            ignore--;
        }

        private int MaskAvg(ColorBgra before) 
        {
            int count = 0, total = 0;   

            for (int c = 0; c < 3; c++) 
            {
                if (mask[c])
                {
                    total += before[c];
                    count++;
                }
            }

            if (count > 0) 
            {
                return total / count;
            } 
            else
            {
                return 0;
            }
        }

        private ColorBgra UpdateByMask(ColorBgra before, byte val) 
        {
            ColorBgra after = before;
            int average = -1, oldaverage = -1;

            if (!(mask[0] || mask[1] || mask[2]))
            {
                return before;
            }

            do
            {
                float factor;

                oldaverage = average;
                average = MaskAvg(after);

                if (average == 0)
                {
                    break;
                }
                factor = (float)val / average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        after[c] = (byte)Utility.ClampToByte(after[c] * factor);
                    }
                }
            } while (average != val && oldaverage != average);

            while (average != val) 
            {
                average = MaskAvg(after);
                int diff = val - average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        after[c] = (byte)Utility.ClampToByte(after[c] + diff);
                    }
                }
            }

            after.A = 255;
            return after;           
        }

        private void LevelsEffectConfigDialog_Load(object sender, System.EventArgs e)
        {
            histogramInput.Histogram.UpdateHistogram(this.EffectSourceSurface, this.Selection);
            mask[0] = true;
            mask[1] = true;
            mask[2] = true;
            MaskChanged();
            UpdateOutputHistogram();
        }

        private void UpdateOutputHistogram() 
        {
            ((HistogramRgb)this.histogramOutput.Histogram).SetFromLeveledHistogram((HistogramRgb)this.histogramInput.Histogram, ((LevelsEffectConfigToken)this.theEffectToken).Levels);
            this.histogramOutput.Update();
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new LevelsEffectConfigToken();
        }

        private void UpdateGammaByMask(UnaryPixelOps.Level levels, float val) 
        {
            float average = -1;

            if (!(mask[0] || mask[1] || mask[2]))
            {
                return;
            }

            do
            {
                average = MaskGamma(levels);
                float factor = val / average;

                for (int c = 0; c < 3; c++) 
                {
                    if (mask[c]) 
                    {
                        levels.SetGamma(c, factor * levels.GetGamma(c));
                    }
                }
            } while (Math.Abs(val - average) > 0.001);
        }

        protected override void InitTokenFromDialog()
        {
            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;

            levels.ColorOutHigh = UpdateByMask(levels.ColorOutHigh, (byte)outputHiUpDown.Value);
            levels.ColorOutLow = UpdateByMask(levels.ColorOutLow, (byte)outputLowUpDown.Value);

            levels.ColorInHigh = UpdateByMask(levels.ColorInHigh, (byte)inputHiUpDown.Value);
            levels.ColorInLow = UpdateByMask(levels.ColorInLow, (byte)inputLoUpDown.Value);

            UpdateGammaByMask(levels, (float)outputGammaUpDown.Value);

            swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
            swatchInHigh.Invalidate();

            swatchInLow.BackColor = levels.ColorInLow.ToColor();
            swatchInLow.Invalidate();

            swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
            swatchOutHigh.Invalidate();

            swatchOutMid.BackColor = levels.Apply(((HistogramRgb)histogramInput.Histogram).GetMeanColor()).ToColor();
            swatchOutMid.Invalidate();

            swatchOutLow.BackColor = levels.ColorOutLow.ToColor();
            swatchOutLow.Invalidate();
        }

        private float MaskGamma(UnaryPixelOps.Level levels) 
        {
            int count = 0;
            float total = 0;

            for (int c = 0; c < 3; c++) 
            {
                if (mask[c])
                {
                    total += levels.GetGamma(c);
                    count++;
                }
            }

            if (count > 0) 
            {
                return total / count;
            } 
            else
            {
                return 1;
            }
    
        }
        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)effectToken).Levels;

            outputHiUpDown.Value = MaskAvg(levels.ColorOutHigh);
            outputLowUpDown.Value = MaskAvg(levels.ColorOutLow);
            inputHiUpDown.Value = MaskAvg(levels.ColorInHigh);
            inputLoUpDown.Value = MaskAvg(levels.ColorInLow);
            
            gradientOutput.SetValue(0, (int)outputLowUpDown.Value);
            gradientOutput.SetValue(2, (int)outputHiUpDown.Value);
            outputGammaUpDown.Value = outputGammaUpDown.Value;

            swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
            swatchInLow.BackColor = levels.ColorInLow.ToColor();
            swatchOutMid.BackColor = levels.Apply(((HistogramRgb)histogramInput.Histogram).GetMeanColor()).ToColor();
            swatchOutMid.Invalidate();
            swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
            swatchOutLow.BackColor = levels.ColorOutLow.ToColor();

            outputGammaUpDown.Value = (decimal)MaskGamma(levels);
        }

        private void UpdateLevels() 
        {   
            UpdateToken();
            UpdateOutputHistogram();
        }

        private void grpOutput_Layout(object sender, LayoutEventArgs e)
        {
            outputGammaUpDown.Top = (outputGroupBox.Height / 2) - outputGammaUpDown.Height;
            swatchOutMid.Top = 1 + (outputGroupBox.Height / 2);
        }

        private void gradientOutput_ValueChanged(object sender, IndexEventArgs e)
        {
            if (ignore == 0) 
            {
                int lo = gradientOutput.GetValue(0), md, hi = gradientOutput.GetValue(2);
                md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)outputGammaUpDown.Value));
                ignore++;

                switch (e.Index) 
                {
                    case 0:
                        outputLowUpDown.Text = lo.ToString();
                        break;

                    case 1:
                        md = gradientOutput.GetValue(1);
                        outputGammaUpDown.Value = (decimal)Utility.Clamp(1 / Math.Log(0.5, (float)(md - lo) / (float)(hi - lo)), 0.1, 10.0);
                        break;

                    case 2:
                        outputHiUpDown.Text = hi.ToString();
                        break;
                }

                gradientOutput.SetValue(1, md);
                UpdateLevels();
                ignore--;
            }
        }

        private void outputHiUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ignore == 0) 
            {
                ignore++;
                gradientOutput.SetValue(2, (int)outputHiUpDown.Value);
                UpdateLevels();
                ignore--;
            }
        }

        private void outputGammaUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            int lo = gradientOutput.GetValue(0);
            int hi = gradientOutput.GetValue(2);
            int md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)outputGammaUpDown.Value));

            gradientOutput.SetValue(1, md);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        private void outputLowUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (ignore == 0) 
            {
                ignore++;
                gradientOutput.SetValue(0, (int)outputLowUpDown.Value);
                UpdateLevels();
                ignore--;
            }
        }

        private void gradientInput_ValueChanged(object sender, IndexEventArgs e)
        {
            if (ignore == 0) 
            {
                int lo = gradientInput.GetValue(0), hi = gradientInput.GetValue(1);
                ignore++;

                switch (e.Index) 
                {
                    case 0:
                        inputLoUpDown.Text = lo.ToString();
                        break;

                    case 1:
                        inputHiUpDown.Text = hi.ToString();
                        break;
                }

                UpdateLevels();
                ignore--;
            }
        }

        private void txtInputHi_ValueChanged(object sender, System.EventArgs e)
        {
            gradientInput.SetValue(1, (int)inputHiUpDown.Value);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        private void txtInputLo_ValueChanged(object sender, System.EventArgs e)
        {
            gradientInput.SetValue(0, (int)inputLoUpDown.Value);

            if (ignore == 0) 
            {
                ignore++;
                UpdateLevels();
                ignore--;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout (levent);
            if (levent.AffectedControl == this && panelMask != null)
            {
                panelMask.Left = (this.ClientSize.Width - panelMask.Width) / 2;
            }
        }

        private void panelAdjustments_Layout(object sender, LayoutEventArgs e)
        {
            panelInput.Width = this.ClientSize.Width / 2;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void resetButton_Click(object sender, System.EventArgs e)
        {
            ((LevelsEffectConfigToken)this.EffectToken).Levels = new UnaryPixelOps.Level();
            ignore++;
            InitDialogFromToken();
            ignore--;
            UpdateLevels();     
        }

        private void autoButton_Click(object sender, System.EventArgs e)
        {
            ((LevelsEffectConfigToken)this.EffectToken).Levels = ((HistogramRgb)histogramInput.Histogram).MakeLevelsAuto();

            ignore++;
            InitDialogFromToken();
            ignore--;
            UpdateLevels();
        }

        private void swatch_DoubleClick(object sender, System.EventArgs e)
        {
            UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;

            using (ColorDialog cd = new ColorDialog())
            {
                if ((sender is Panel)) 
                {
                    cd.Color = ((Panel)sender).BackColor;
                    cd.AnyColor = true;

                    if (cd.ShowDialog(this) == DialogResult.OK) 
                    {
                        ColorBgra col = ColorBgra.FromColor(cd.Color);

                        if (sender == swatchInLow) 
                        {
                            levels.ColorInLow = col;
                        }
                        else if (sender == swatchInHigh) 
                        {
                            levels.ColorInHigh = col;
                        }
                        else if (sender == swatchOutLow) 
                        {
                            levels.ColorOutLow = col;
                        }
                        else if (sender == swatchOutMid)
                        {
                            ColorBgra lo = levels.ColorInLow, md = ((HistogramRgb)histogramInput.Histogram).GetMeanColor(), hi = levels.ColorInHigh;
                            ColorBgra out_lo = levels.ColorOutLow, out_hi = levels.ColorOutHigh;

                            for (int i = 0; i < 3; i++) 
                            {
                                levels.SetGamma(i, 
                                    (float)Utility.Clamp(Math.Log((float)(col[i] - out_lo[i]) / (out_hi[i] - out_lo[i]),
                                    (float)(md[i] - lo[i]) / (float)(hi[i] - lo[i])),
                                    0.1,
                                    10.0));
                            }
                        }
                        else if (sender == swatchOutHigh) 
                        {
                            levels.ColorOutHigh = col;
                        }
                        else if (sender == swatchInHigh) 
                        {
                            levels.ColorInHigh = col;
                        }

                        InitDialogFromToken();
                    }
                }
            }
        }

        private void blueMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[0] = blueMaskCheckBox.Checked;
            MaskChanged();
        }

        private void greenMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[1] = greenMaskCheckBox.Checked;
            MaskChanged();
        }

        private void redMaskCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            mask[2] = redMaskCheckBox.Checked;
            MaskChanged();
        }

        private void txtInputHi_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputHi_ValueChanged(sender, EventArgs.Empty);
        }

        private void outputHiUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputHiUpDown_ValueChanged(sender, EventArgs.Empty);      
        }

        private void txtInputLo_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputLo_ValueChanged(sender, EventArgs.Empty);       
        }

        private void outputLowUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            outputLowUpDown_ValueChanged(sender, EventArgs.Empty);      
        }

        private void outputGammaUpDown_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            txtInputHi_ValueChanged(sender, EventArgs.Empty);       
        }
    }
}