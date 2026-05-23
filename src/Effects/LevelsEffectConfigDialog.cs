using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for LevelsEffectConfigDialog.
	/// </summary>
	public class LevelsEffectConfigDialog : EffectConfigDialog
	{
		private PaintDotNet.ColorGradientControl gradientOutput;
		private System.Windows.Forms.NumericUpDown numericUpDown5;
		private System.Windows.Forms.GroupBox grpOutput;
		private System.Windows.Forms.NumericUpDown txtOutputHi;
		private System.Windows.Forms.NumericUpDown txtOutputLo;
		private System.Windows.Forms.NumericUpDown txtOutputGamma;
		private PaintDotNet.HistogramControl histogramOutput;
		private System.Windows.Forms.Panel panelOutput;
		private uint ignore = 0;
		private ColorBgra mask;
		private System.Windows.Forms.Panel swatchOutHigh;
		private System.Windows.Forms.Panel swatchOutLow;
		private System.Windows.Forms.Panel panelMask;
		private System.Windows.Forms.CheckBox chkRedMask;
		private System.Windows.Forms.CheckBox chkGreenMask;
		private System.Windows.Forms.CheckBox chkBlueMask;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button btnAuto;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Panel panelInput;
		private PaintDotNet.HistogramControl histogramInput;
		private System.Windows.Forms.GroupBox grpInput;
		private System.Windows.Forms.Panel swatchInHigh;
		private System.Windows.Forms.NumericUpDown txtInputLo;
		private System.Windows.Forms.NumericUpDown txtInputHi;
		private PaintDotNet.ColorGradientControl gradientInput;
		private System.Windows.Forms.Panel swatchInLow;
		private System.Windows.Forms.Panel panelAdjustments;
		private System.Windows.Forms.ToolTip tooltipProvider;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.GroupBox grpOutputHistogram;
		private System.Windows.Forms.GroupBox grpInputHistogram;
		private System.Windows.Forms.Panel swatchOutMid;
		
	
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.gradientOutput = new PaintDotNet.ColorGradientControl();
			this.txtOutputHi = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
			this.txtOutputLo = new System.Windows.Forms.NumericUpDown();
			this.grpOutput = new System.Windows.Forms.GroupBox();
			this.txtOutputGamma = new System.Windows.Forms.NumericUpDown();
			this.swatchOutHigh = new System.Windows.Forms.Panel();
			this.swatchOutLow = new System.Windows.Forms.Panel();
			this.swatchOutMid = new System.Windows.Forms.Panel();
			this.grpOutputHistogram = new System.Windows.Forms.GroupBox();
			this.histogramOutput = new PaintDotNet.HistogramControl();
			this.panelOutput = new System.Windows.Forms.Panel();
			this.panelAdjustments = new System.Windows.Forms.Panel();
			this.panelInput = new System.Windows.Forms.Panel();
			this.grpInputHistogram = new System.Windows.Forms.GroupBox();
			this.histogramInput = new PaintDotNet.HistogramControl();
			this.grpInput = new System.Windows.Forms.GroupBox();
			this.swatchInHigh = new System.Windows.Forms.Panel();
			this.txtInputLo = new System.Windows.Forms.NumericUpDown();
			this.txtInputHi = new System.Windows.Forms.NumericUpDown();
			this.gradientInput = new PaintDotNet.ColorGradientControl();
			this.swatchInLow = new System.Windows.Forms.Panel();
			this.panelMask = new System.Windows.Forms.Panel();
			this.chkRedMask = new System.Windows.Forms.CheckBox();
			this.chkGreenMask = new System.Windows.Forms.CheckBox();
			this.chkBlueMask = new System.Windows.Forms.CheckBox();
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.btnAuto = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.txtOutputHi)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtOutputLo)).BeginInit();
			this.grpOutput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtOutputGamma)).BeginInit();
			this.grpOutputHistogram.SuspendLayout();
			this.panelOutput.SuspendLayout();
			this.panelAdjustments.SuspendLayout();
			this.panelInput.SuspendLayout();
			this.grpInputHistogram.SuspendLayout();
			this.grpInput.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.txtInputLo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.txtInputHi)).BeginInit();
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
			this.gradientOutput.ValueChanged += new System.EventHandler(this.gradientOutput_ValueChanged);
			// 
			// txtOutputHi
			// 
			this.txtOutputHi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputHi.Location = new System.Drawing.Point(46, 16);
			this.txtOutputHi.Maximum = new System.Decimal(new int[] {
																		255,
																		0,
																		0,
																		0});
			this.txtOutputHi.Name = "txtOutputHi";
			this.txtOutputHi.Size = new System.Drawing.Size(48, 20);
			this.txtOutputHi.TabIndex = 1;
			this.txtOutputHi.Value = new System.Decimal(new int[] {
																	  255,
																	  0,
																	  0,
																	  0});
			this.txtOutputHi.ValueChanged += new System.EventHandler(this.txtOutputHi_ValueChanged);
			// 
			// numericUpDown5
			// 
			this.numericUpDown5.Location = new System.Drawing.Point(-24, 24);
			this.numericUpDown5.Name = "numericUpDown5";
			this.numericUpDown5.Size = new System.Drawing.Size(24, 20);
			this.numericUpDown5.TabIndex = 1;
			// 
			// txtOutputLo
			// 
			this.txtOutputLo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputLo.Location = new System.Drawing.Point(46, 158);
			this.txtOutputLo.Maximum = new System.Decimal(new int[] {
																		255,
																		0,
																		0,
																		0});
			this.txtOutputLo.Name = "txtOutputLo";
			this.txtOutputLo.Size = new System.Drawing.Size(48, 20);
			this.txtOutputLo.TabIndex = 1;
			this.txtOutputLo.ValueChanged += new System.EventHandler(this.txtOutputLo_ValueChanged);
			// 
			// grpOutput
			// 
			this.grpOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.grpOutput.Controls.Add(this.txtOutputGamma);
			this.grpOutput.Controls.Add(this.numericUpDown5);
			this.grpOutput.Controls.Add(this.txtOutputLo);
			this.grpOutput.Controls.Add(this.txtOutputHi);
			this.grpOutput.Controls.Add(this.gradientOutput);
			this.grpOutput.Controls.Add(this.swatchOutHigh);
			this.grpOutput.Controls.Add(this.swatchOutLow);
			this.grpOutput.Controls.Add(this.swatchOutMid);
			this.grpOutput.Location = new System.Drawing.Point(2, 0);
			this.grpOutput.Name = "grpOutput";
			this.grpOutput.Size = new System.Drawing.Size(102, 190);
			this.grpOutput.TabIndex = 2;
			this.grpOutput.TabStop = false;
			this.grpOutput.Text = "Output";
			this.grpOutput.Resize += new System.EventHandler(this.grpOutput_Resize);
			// 
			// txtOutputGamma
			// 
			this.txtOutputGamma.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.txtOutputGamma.DecimalPlaces = 2;
			this.txtOutputGamma.Increment = new System.Decimal(new int[] {
																			 1,
																			 0,
																			 0,
																			 65536});
			this.txtOutputGamma.Location = new System.Drawing.Point(46, 73);
			this.txtOutputGamma.Maximum = new System.Decimal(new int[] {
																		   100,
																		   0,
																		   0,
																		   65536});
			this.txtOutputGamma.Minimum = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   65536});
			this.txtOutputGamma.Name = "txtOutputGamma";
			this.txtOutputGamma.Size = new System.Drawing.Size(48, 20);
			this.txtOutputGamma.TabIndex = 1;
			this.tooltipProvider.SetToolTip(this.txtOutputGamma, "Output Gamma");
			this.txtOutputGamma.Value = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 0});
			this.txtOutputGamma.ValueChanged += new System.EventHandler(this.txtOutputGamma_ValueChanged);
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
			this.tooltipProvider.SetToolTip(this.swatchOutHigh, "Output Whitepoint (Doubleclick to choose)");
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
			this.tooltipProvider.SetToolTip(this.swatchOutLow, "Output Blackpoint (Doubleclick to choose)");
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
			// grpOutputHistogram
			// 
			this.grpOutputHistogram.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpOutputHistogram.Controls.Add(this.histogramOutput);
			this.grpOutputHistogram.Location = new System.Drawing.Point(108, 0);
			this.grpOutputHistogram.Name = "grpOutputHistogram";
			this.grpOutputHistogram.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.grpOutputHistogram.Size = new System.Drawing.Size(130, 190);
			this.grpOutputHistogram.TabIndex = 3;
			this.grpOutputHistogram.TabStop = false;
			this.grpOutputHistogram.Text = "Output Histogram";
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
			this.tooltipProvider.SetToolTip(this.histogramOutput, "This histogram shows a preview of the distribution of color in the output image");
			// 
			// panelOutput
			// 
			this.panelOutput.Controls.Add(this.grpOutputHistogram);
			this.panelOutput.Controls.Add(this.grpOutput);
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
			this.panelAdjustments.Resize += new System.EventHandler(this.panelAdjustments_Resize);
			// 
			// panelInput
			// 
			this.panelInput.Controls.Add(this.grpInputHistogram);
			this.panelInput.Controls.Add(this.grpInput);
			this.panelInput.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelInput.Location = new System.Drawing.Point(0, 0);
			this.panelInput.Name = "panelInput";
			this.panelInput.Size = new System.Drawing.Size(240, 192);
			this.panelInput.TabIndex = 6;
			// 
			// grpInputHistogram
			// 
			this.grpInputHistogram.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpInputHistogram.Controls.Add(this.histogramInput);
			this.grpInputHistogram.Location = new System.Drawing.Point(2, 0);
			this.grpInputHistogram.Name = "grpInputHistogram";
			this.grpInputHistogram.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.grpInputHistogram.Size = new System.Drawing.Size(130, 190);
			this.grpInputHistogram.TabIndex = 3;
			this.grpInputHistogram.TabStop = false;
			this.grpInputHistogram.Text = "Input Histogram";
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
			this.tooltipProvider.SetToolTip(this.histogramInput, "This histogram shows the distribution of color in the image.");
			// 
			// grpInput
			// 
			this.grpInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpInput.Controls.Add(this.swatchInHigh);
			this.grpInput.Controls.Add(this.txtInputLo);
			this.grpInput.Controls.Add(this.txtInputHi);
			this.grpInput.Controls.Add(this.gradientInput);
			this.grpInput.Controls.Add(this.swatchInLow);
			this.grpInput.Location = new System.Drawing.Point(136, 0);
			this.grpInput.Name = "grpInput";
			this.grpInput.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.grpInput.Size = new System.Drawing.Size(102, 190);
			this.grpInput.TabIndex = 2;
			this.grpInput.TabStop = false;
			this.grpInput.Text = "Input";
			// 
			// swatchInHigh
			// 
			this.swatchInHigh.BackColor = System.Drawing.Color.White;
			this.swatchInHigh.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.swatchInHigh.Location = new System.Drawing.Point(8, 37);
			this.swatchInHigh.Name = "swatchInHigh";
			this.swatchInHigh.Size = new System.Drawing.Size(48, 24);
			this.swatchInHigh.TabIndex = 2;
			this.tooltipProvider.SetToolTip(this.swatchInHigh, "Input Whitepoint (Doubleclick to choose)");
			this.swatchInHigh.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
			// 
			// txtInputLo
			// 
			this.txtInputLo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.txtInputLo.Location = new System.Drawing.Point(8, 158);
			this.txtInputLo.Maximum = new System.Decimal(new int[] {
																	   255,
																	   0,
																	   0,
																	   0});
			this.txtInputLo.Name = "txtInputLo";
			this.txtInputLo.Size = new System.Drawing.Size(48, 20);
			this.txtInputLo.TabIndex = 1;
			this.txtInputLo.ValueChanged += new System.EventHandler(this.txtInputLo_ValueChanged);
			// 
			// txtInputHi
			// 
			this.txtInputHi.Location = new System.Drawing.Point(8, 16);
			this.txtInputHi.Maximum = new System.Decimal(new int[] {
																	   255,
																	   0,
																	   0,
																	   0});
			this.txtInputHi.Name = "txtInputHi";
			this.txtInputHi.Size = new System.Drawing.Size(48, 20);
			this.txtInputHi.TabIndex = 1;
			this.txtInputHi.Value = new System.Decimal(new int[] {
																	 255,
																	 0,
																	 0,
																	 0});
			this.txtInputHi.ValueChanged += new System.EventHandler(this.txtInputHi_ValueChanged);
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
			this.gradientInput.ValueChanged += new System.EventHandler(this.gradientInput_ValueChanged);
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
			this.tooltipProvider.SetToolTip(this.swatchInLow, "Input Blackpoint (Doubleclick to choose)");
			this.swatchInLow.DoubleClick += new System.EventHandler(this.swatch_DoubleClick);
			// 
			// panelMask
			// 
			this.panelMask.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.panelMask.Controls.Add(this.chkRedMask);
			this.panelMask.Controls.Add(this.chkGreenMask);
			this.panelMask.Controls.Add(this.chkBlueMask);
			this.panelMask.Location = new System.Drawing.Point(192, 200);
			this.panelMask.Name = "panelMask";
			this.panelMask.Size = new System.Drawing.Size(96, 24);
			this.panelMask.TabIndex = 14;
			// 
			// chkRedMask
			// 
			this.chkRedMask.Checked = true;
			this.chkRedMask.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRedMask.Location = new System.Drawing.Point(0, 0);
			this.chkRedMask.Name = "chkRedMask";
			this.chkRedMask.Size = new System.Drawing.Size(32, 24);
			this.chkRedMask.TabIndex = 1;
			this.chkRedMask.Text = "R";
			this.tooltipProvider.SetToolTip(this.chkRedMask, "Toggle manipulation of the Red Channel");
			this.chkRedMask.Click += new System.EventHandler(this.chkRedMask_CheckedChanged);
			// 
			// chkGreenMask
			// 
			this.chkGreenMask.Checked = true;
			this.chkGreenMask.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkGreenMask.Location = new System.Drawing.Point(32, 0);
			this.chkGreenMask.Name = "chkGreenMask";
			this.chkGreenMask.Size = new System.Drawing.Size(32, 24);
			this.chkGreenMask.TabIndex = 1;
			this.chkGreenMask.Text = "G";
			this.tooltipProvider.SetToolTip(this.chkGreenMask, "Toggle manipulation of the Green Channel");
			this.chkGreenMask.Click += new System.EventHandler(this.chkGreenMask_CheckedChanged);
			// 
			// chkBlueMask
			// 
			this.chkBlueMask.Checked = true;
			this.chkBlueMask.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkBlueMask.Location = new System.Drawing.Point(64, 0);
			this.chkBlueMask.Name = "chkBlueMask";
			this.chkBlueMask.Size = new System.Drawing.Size(32, 24);
			this.chkBlueMask.TabIndex = 1;
			this.chkBlueMask.Text = "B";
			this.tooltipProvider.SetToolTip(this.chkBlueMask, "Toggle manipulation of the Blue Channel");
			this.chkBlueMask.Click += new System.EventHandler(this.chkBlueMask_CheckedChanged);
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.okButton.Location = new System.Drawing.Point(312, 200);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 11;
			this.okButton.Text = "&OK";
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
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// btnAuto
			// 
			this.btnAuto.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAuto.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnAuto.Location = new System.Drawing.Point(8, 200);
			this.btnAuto.Name = "btnAuto";
			this.btnAuto.TabIndex = 10;
			this.btnAuto.Text = "Auto";
			this.tooltipProvider.SetToolTip(this.btnAuto, "Automatically Adjusts the input white point, black point, and output gamma to equ" +
				"alize the image.");
			this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnReset.Location = new System.Drawing.Point(88, 200);
			this.btnReset.Name = "btnReset";
			this.btnReset.TabIndex = 12;
			this.btnReset.Text = "Reset";
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
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
			this.Controls.Add(this.btnAuto);
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.panelAdjustments);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
			this.Location = new System.Drawing.Point(0, 0);
			this.MaximizeBox = true;
			this.MinimumSize = new System.Drawing.Size(439, 231);
			this.Name = "LevelsEffectConfigDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Levels Adjustment";
			this.Resize += new System.EventHandler(this.LevelsEffectConfigDialog_Resize);
			this.Load += new System.EventHandler(this.LevelsEffectConfigDialog_Load);
			this.Controls.SetChildIndex(this.panelAdjustments, 0);
			this.Controls.SetChildIndex(this.btnReset, 0);
			this.Controls.SetChildIndex(this.btnAuto, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.panelMask, 0);
			((System.ComponentModel.ISupportInitialize)(this.txtOutputHi)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtOutputLo)).EndInit();
			this.grpOutput.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.txtOutputGamma)).EndInit();
			this.grpOutputHistogram.ResumeLayout(false);
			this.panelOutput.ResumeLayout(false);
			this.panelAdjustments.ResumeLayout(false);
			this.panelInput.ResumeLayout(false);
			this.grpInputHistogram.ResumeLayout(false);
			this.grpInput.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.txtInputLo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.txtInputHi)).EndInit();
			this.panelMask.ResumeLayout(false);
			this.ResumeLayout(false);

		}
	
		public LevelsEffectConfigDialog()
		{
			mask = ColorBgra.FromColor(Color.White);
			InitializeComponent();
		}

		private void MaskChanged() 
		{
			if ((mask.Bgra & 0x00ffffff) == 0) 
			{
				grpInput.Enabled = false;
				grpOutput.Enabled = false;
			}			
			else
			{
				grpInput.Enabled = true;
				grpOutput.Enabled = true;
			}
			Color top = mask.ToColor();
			gradientInput.TopColor = top;
			gradientOutput.TopColor = top;
			histogramInput.Mask = mask;
			histogramOutput.Mask = mask;
			ignore++;
			InitDialogFromToken();
			ignore--;
		}

		private int MaskAvg(ColorBgra before) 
		{
			int count = 0, total = 0;	
			for (int c = 0; c < 3; c++) 
			{
				if (mask[c] != 0) 
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
			if ((mask.Bgra & 0x00ffffff) == 0)
			{
				return before;
			}
			do
			{
				float factor;
				oldaverage = average;
				average = MaskAvg(after);
				if (average == 0)
					break;
				factor = (float)val / average;
				for (int c = 0; c < 3; c++) 
				{
					if (mask[c] != 0) 
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
					if (mask[c] != 0) 
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
			mask[0] = mask[1] = mask[2] = 255;
			MaskChanged();
			UpdateOutputHistogram();
		}

		private void UpdateOutputHistogram() 
		{
			this.histogramOutput.Histogram.SetFromLeveledHistogram(this.histogramInput.Histogram, ((LevelsEffectConfigToken)this.theEffectToken).Levels);
			this.histogramOutput.Update();
		}

		protected override void InitialInitToken()
		{
			theEffectToken = new LevelsEffectConfigToken();
		}

		private void UpdateGammaByMask(UnaryPixelOps.Level levels, float val) 
		{
			float average = -1;
			if ((mask.Bgra & 0x00ffffff) == 0)
			{
				return;
			}
			do
			{
				average = MaskGamma(levels);

				float factor = val / average;
				for (int c = 0; c < 3; c++) 
				{
					if (mask[c] != 0) 
					{
						levels.SetGamma(c, factor * levels.GetGamma(c));
					}
				}
			} while (Math.Abs(val - average) > 0.001);
		}

		protected override void InitTokenFromDialog()
		{
			UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;
			levels.ColorOutHigh = UpdateByMask(levels.ColorOutHigh, (byte)txtOutputHi.Value);
			levels.ColorOutLow = UpdateByMask(levels.ColorOutLow, (byte)txtOutputLo.Value);
			levels.ColorInHigh = UpdateByMask(levels.ColorInHigh, (byte)txtInputHi.Value);
			levels.ColorInLow = UpdateByMask(levels.ColorInLow, (byte)txtInputLo.Value);
			UpdateGammaByMask(levels, (float)txtOutputGamma.Value);

			swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
			swatchInHigh.Invalidate();
			swatchInLow.BackColor = levels.ColorInLow.ToColor();
			swatchInLow.Invalidate();
			swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
			swatchOutHigh.Invalidate();
			swatchOutMid.BackColor = levels.Apply(histogramInput.Histogram.GetMeanColor()).ToColor();
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
				if (mask[c] != 0)
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

			txtOutputHi.Value = MaskAvg(levels.ColorOutHigh);
			txtOutputLo.Value = MaskAvg(levels.ColorOutLow);
			txtInputHi.Value = MaskAvg(levels.ColorInHigh);
			txtInputLo.Value = MaskAvg(levels.ColorInLow);
			
			gradientOutput.SetValue(0, (int)txtOutputLo.Value);
			gradientOutput.SetValue(2, (int)txtOutputHi.Value);
			txtOutputGamma.Value = txtOutputGamma.Value;

			swatchInHigh.BackColor = levels.ColorInHigh.ToColor();
			swatchInLow.BackColor = levels.ColorInLow.ToColor();
			swatchOutMid.BackColor = levels.Apply(histogramInput.Histogram.GetMeanColor()).ToColor();
			swatchOutMid.Invalidate();
			swatchOutHigh.BackColor = levels.ColorOutHigh.ToColor();
			swatchOutLow.BackColor = levels.ColorOutLow.ToColor();

			txtOutputGamma.Value = (decimal)MaskGamma(levels);
		}

		private void UpdateLevels() 
		{	
			UpdateToken();
			UpdateOutputHistogram();
		}

		private void grpOutput_Resize(object sender, System.EventArgs e)
		{
			txtOutputGamma.Top = grpOutput.Height / 2 - txtOutputGamma.Height ;
			swatchOutMid.Top = grpOutput.Height / 2 + 1;
		}

		private void gradientOutput_ValueChanged(object sender, System.EventArgs e)
		{
			if (ignore == 0) 
			{
				int lo = gradientOutput.GetValue(0), md, hi = gradientOutput.GetValue(2);
				md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)txtOutputGamma.Value));
				ignore++;
				switch (((ColorGradientEventArgs)e).Index) 
				{
					case 0:
						txtOutputLo.Text = lo.ToString();
						break;
					case 1:
						md = gradientOutput.GetValue(1);
						txtOutputGamma.Value = (decimal)Utility.Clamp(1 / Math.Log(0.5, (float)(md - lo) / (float)(hi - lo)), 0.1, 10.0);
						break;
					case 2:
						txtOutputHi.Text = hi.ToString();
						break;
				}
				gradientOutput.SetValue(1, md);
				UpdateLevels();
				ignore--;
			}
		}

		private void txtOutputHi_ValueChanged(object sender, System.EventArgs e)
		{
			if (ignore == 0) 
			{
				ignore++;
				gradientOutput.SetValue(2, (int)txtOutputHi.Value);
				UpdateLevels();
				ignore--;
			}
		}

		private void txtOutputGamma_ValueChanged(object sender, System.EventArgs e)
		{
			int lo = gradientOutput.GetValue(0), md, hi = gradientOutput.GetValue(2);
			md = (int)(lo + (hi - lo) * Math.Pow(0.5, (double)txtOutputGamma.Value));
			gradientOutput.SetValue(1, md);
			if (ignore == 0) 
			{
				ignore++;
				UpdateLevels();
				ignore--;
			}
		}

		private void txtOutputLo_ValueChanged(object sender, System.EventArgs e)
		{
			if (ignore == 0) 
			{
				ignore++;
				gradientOutput.SetValue(0, (int)txtOutputLo.Value);
				UpdateLevels();
				ignore--;
			}
		}

		private void gradientInput_ValueChanged(object sender, System.EventArgs e)
		{
			if (ignore == 0) 
			{
				int lo = gradientInput.GetValue(0), hi = gradientInput.GetValue(1);
				ignore++;
				switch (((ColorGradientEventArgs)e).Index) 
				{
					case 0:
						txtInputLo.Text = lo.ToString();
						break;
					case 1:
						txtInputHi.Text = hi.ToString();
						break;
				}
				UpdateLevels();
				ignore--;
			}
		}

		private void txtInputHi_ValueChanged(object sender, System.EventArgs e)
		{
			gradientInput.SetValue(1, (int)txtInputHi.Value);
			if (ignore == 0) 
			{
				ignore++;
				UpdateLevels();
				ignore--;
			}
		}

		private void txtInputLo_ValueChanged(object sender, System.EventArgs e)
		{
			gradientInput.SetValue(0, (int)txtInputLo.Value);
			if (ignore == 0) 
			{
				ignore++;
				UpdateLevels();
				ignore--;
			}
		}

		private void LevelsEffectConfigDialog_Resize(object sender, System.EventArgs e)
		{
			panelMask.Left = (this.ClientSize.Width - panelMask.Width) / 2;
		}

		private void panelAdjustments_Resize(object sender, System.EventArgs e)
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

		private void btnReset_Click(object sender, System.EventArgs e)
		{
			((LevelsEffectConfigToken)this.EffectToken).Levels = new UnaryPixelOps.Level();
			ignore++;
			InitDialogFromToken();
			ignore--;
			UpdateLevels();		
		}

		private void btnAuto_Click(object sender, System.EventArgs e)
		{
			((LevelsEffectConfigToken)this.EffectToken).Levels = histogramInput.Histogram.MakeLevelsAuto();

			ignore++;
			InitDialogFromToken();
			ignore--;
			UpdateLevels();
		}

		private void swatch_DoubleClick(object sender, System.EventArgs e)
		{
			UnaryPixelOps.Level levels = ((LevelsEffectConfigToken)theEffectToken).Levels;
			ColorDialog cd = new ColorDialog();
			if ((sender is Panel)) 
			{
				cd.Color = ((Panel)sender).BackColor;
				cd.AnyColor = true;
				if (cd.ShowDialog() == DialogResult.OK) 
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
						ColorBgra lo = levels.ColorInLow, md = histogramInput.Histogram.GetMeanColor(), hi = levels.ColorInHigh;
						ColorBgra out_lo = levels.ColorOutLow, out_hi = levels.ColorOutHigh;
						for (int i = 0; i < 3; i++) 
						{
							levels.SetGamma
								(
									i, 
									(float)Utility.Clamp
									(
										Math.Log
										(
											(float)(col[i] - out_lo[i]) / (out_hi[i] - out_lo[i]),
											(float)(md[i] - lo[i]) / (float)(hi[i] - lo[i])
										),
										0.1,
										10.0
									)
								);
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

		private void chkBlueMask_CheckedChanged(object sender, System.EventArgs e)
		{
			mask.B = (byte)(chkBlueMask.Checked ? 255 : 0);
			MaskChanged();
		}

		private void chkGreenMask_CheckedChanged(object sender, System.EventArgs e)
		{
			mask.G = (byte)(chkGreenMask.Checked ? 255 : 0);
			MaskChanged();
		}

		private void chkRedMask_CheckedChanged(object sender, System.EventArgs e)
		{
			mask.R = (byte)(chkRedMask.Checked ? 255 : 0);
			MaskChanged();
		}
	}
}