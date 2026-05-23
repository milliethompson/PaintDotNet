using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    public class MosaicEffectConfigDialog 
        : EffectConfigDialog
    {
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TrackBar cellSizeTrackBar;
        private System.Windows.Forms.NumericUpDown cellSizeUpDown;
        private System.ComponentModel.IContainer components = null;

        public MosaicEffectConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cellSizeTrackBar = new System.Windows.Forms.TrackBar();
            this.cellSizeUpDown = new System.Windows.Forms.NumericUpDown();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cellSizeTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cellSizeUpDown)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // cellSizeTrackBar
            // 
            this.cellSizeTrackBar.AutoSize = false;
            this.cellSizeTrackBar.Location = new System.Drawing.Point(12, 50);
            this.cellSizeTrackBar.Maximum = 100;
            this.cellSizeTrackBar.Minimum = 1;
            this.cellSizeTrackBar.Name = "cellSizeTrackBar";
            this.cellSizeTrackBar.Size = new System.Drawing.Size(154, 24);
            this.cellSizeTrackBar.TabIndex = 1;
            this.cellSizeTrackBar.TickFrequency = 10;
            this.cellSizeTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.cellSizeTrackBar.Value = 1;
            this.cellSizeTrackBar.ValueChanged += new System.EventHandler(this.cellSizeTrackBar_ValueChanged);
            // 
            // cellSizeUpDown
            // 
            this.cellSizeUpDown.Location = new System.Drawing.Point(14, 22);
            this.cellSizeUpDown.Minimum = new System.Decimal(new int[] {
                                                                           1,
                                                                           0,
                                                                           0,
                                                                           0});
            this.cellSizeUpDown.Name = "cellSizeUpDown";
            this.cellSizeUpDown.Size = new System.Drawing.Size(64, 20);
            this.cellSizeUpDown.TabIndex = 0;
            this.cellSizeUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cellSizeUpDown.Value = new System.Decimal(new int[] {
                                                                         2,
                                                                         0,
                                                                         0,
                                                                         0});
            this.cellSizeUpDown.Enter += new System.EventHandler(this.cellSizeUpDown_Enter);
            this.cellSizeUpDown.ValueChanged += new System.EventHandler(this.cellSizeUpDown_ValueChanged);
            this.cellSizeUpDown.Leave += new System.EventHandler(this.cellSizeUpDown_Leave);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(93, 90);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(13, 90);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.cellSizeUpDown);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(161, 76);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cell Size";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(80, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "pixels";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // MosaicEffectConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(177, 121);
            this.ControlBox = false;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cellSizeTrackBar);
            this.Controls.Add(this.groupBox1);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "MosaicEffectConfigDialog";
            this.Text = "Mosaic";
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.cellSizeTrackBar, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            ((System.ComponentModel.ISupportInitialize)(this.cellSizeTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cellSizeUpDown)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void InitialInitToken()
        {
            theEffectToken = new MosaicEffectConfigToken(2);
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            this.cellSizeTrackBar.Value = ((MosaicEffectConfigToken)effectToken).CellSize;
        }

        protected override void InitTokenFromDialog()
        {
            ((MosaicEffectConfigToken)theEffectToken).CellSize = cellSizeTrackBar.Value;
        }

        private void cellSizeTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (cellSizeTrackBar.Value != (int)cellSizeUpDown.Value)
            {
                cellSizeUpDown.Value = cellSizeTrackBar.Value;
                UpdateToken();
            }
        }

        private void cellSizeUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (cellSizeTrackBar.Value != (int)cellSizeUpDown.Value)
            {
                cellSizeTrackBar.Value = (int)cellSizeUpDown.Value;
                UpdateToken();
            }
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

        private void cellSizeUpDown_Enter(object sender, System.EventArgs e)
        {
            cellSizeUpDown.Select(0,cellSizeUpDown.Text.Length);
        }

        private void cellSizeUpDown_Leave(object sender, System.EventArgs e)
        {
            if (Utility.CheckNumericUpDown(cellSizeUpDown))
            {
                cellSizeUpDown.Value = decimal.Parse(cellSizeUpDown.Text);
            }
        }
    }
}

