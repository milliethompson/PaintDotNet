using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
	public class BitmapLayerPropertiesDialog : PaintDotNet.LayerPropertiesDialog
	{
        private System.Windows.Forms.GroupBox blendingGroupBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private DotNetWidgets.FlatComboBox blendOpComboBox;
        private System.Windows.Forms.NumericUpDown opacityUpDown;
        private System.Windows.Forms.TrackBar opacityTrackBar;
		private System.ComponentModel.IContainer components = null;

		public BitmapLayerPropertiesDialog()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

            // HACK: the designer keeps screwing up the location of the Apply button!
            this.applyButton.Location = new System.Drawing.Point(applyButton.Left, okButton.Top);

            // populate the blendOpComboBox with all the blend modes they're allowed to use
            foreach (Type type in UserBlendOps.GetBlendOps())
            {
                blendOpComboBox.Items.Add(UserBlendOps.CreateBlendOp(type));
            }
		}

        private void SelectOp(UserBlendOp setOp)
        {
            foreach (object op in blendOpComboBox.Items)
            {
                if (op.ToString() == setOp.ToString())
                {
                    blendOpComboBox.SelectedItem = op;
                    break;
                }
            }
        }

        protected override void InitDialogFromLayer()
        {
            opacityUpDown.Value = ((BitmapLayer)Layer).Opacity;
            SelectOp(((BitmapLayer)Layer).BlendOp);

            opacityUpDown.Enabled = !Layer.IsBackground;
            opacityTrackBar.Enabled = !Layer.IsBackground;
            blendOpComboBox.Enabled = !Layer.IsBackground;

            base.InitDialogFromLayer ();
        }

        protected override void InitLayerFromDialog()
        {
            ((BitmapLayer)Layer).Opacity = (byte)opacityUpDown.Value;
            ((BitmapLayer)Layer).SetBlendOp((UserBlendOp)blendOpComboBox.SelectedItem);
            base.InitLayerFromDialog();
        }

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
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
            this.blendingGroupBox = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.blendOpComboBox = new DotNetWidgets.FlatComboBox();
            this.opacityUpDown = new System.Windows.Forms.NumericUpDown();
            this.opacityTrackBar = new System.Windows.Forms.TrackBar();
            this.label2 = new System.Windows.Forms.Label();
            this.blendingGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.opacityUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // visibleCheckBox
            // 
            this.visibleCheckBox.Name = "visibleCheckBox";
            // 
            // label1
            // 
            this.label1.Name = "label1";
            // 
            // groupBox1
            // 
            this.groupBox1.Name = "groupBox1";
            // 
            // nameBox
            // 
            this.nameBox.Name = "nameBox";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(130, 176);
            this.cancelButton.Name = "cancelButton";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(50, 176);
            this.okButton.Name = "okButton";
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(208, 67);
            this.applyButton.Name = "applyButton";
            // 
            // blendingGroupBox
            // 
            this.blendingGroupBox.Controls.Add(this.label3);
            this.blendingGroupBox.Controls.Add(this.blendOpComboBox);
            this.blendingGroupBox.Controls.Add(this.opacityUpDown);
            this.blendingGroupBox.Controls.Add(this.opacityTrackBar);
            this.blendingGroupBox.Controls.Add(this.label2);
            this.blendingGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.blendingGroupBox.Location = new System.Drawing.Point(8, 80);
            this.blendingGroupBox.Name = "blendingGroupBox";
            this.blendingGroupBox.Size = new System.Drawing.Size(272, 80);
            this.blendingGroupBox.TabIndex = 7;
            this.blendingGroupBox.TabStop = false;
            this.blendingGroupBox.Text = "Blending";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(14, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 23);
            this.label3.TabIndex = 4;
            this.label3.Text = "Mode:";
            // 
            // blendOpComboBox
            // 
            this.blendOpComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.blendOpComboBox.InitialText = "";
            this.blendOpComboBox.Location = new System.Drawing.Point(56, 16);
            this.blendOpComboBox.Name = "blendOpComboBox";
            this.blendOpComboBox.Size = new System.Drawing.Size(121, 21);
            this.blendOpComboBox.TabIndex = 3;
            this.blendOpComboBox.SelectedIndexChanged += new System.EventHandler(this.blendOpComboBox_SelectedIndexChanged);
            // 
            // opacityUpDown
            // 
            this.opacityUpDown.Location = new System.Drawing.Point(71, 46);
            this.opacityUpDown.Maximum = new System.Decimal(new int[] {
                                                                          255,
                                                                          0,
                                                                          0,
                                                                          0});
            this.opacityUpDown.Name = "opacityUpDown";
            this.opacityUpDown.Size = new System.Drawing.Size(56, 20);
            this.opacityUpDown.TabIndex = 2;
            this.opacityUpDown.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.opacityUpDown.Enter += new System.EventHandler(this.opacityUpDown_Enter);
            this.opacityUpDown.KeyUp += new System.Windows.Forms.KeyEventHandler(this.opacityUpDown_KeyUp);
            this.opacityUpDown.ValueChanged += new System.EventHandler(this.opacityUpDown_ValueChanged);
            this.opacityUpDown.Leave += new System.EventHandler(this.opacityUpDown_Leave);
            // 
            // opacityTrackBar
            // 
            this.opacityTrackBar.AutoSize = false;
            this.opacityTrackBar.LargeChange = 32;
            this.opacityTrackBar.Location = new System.Drawing.Point(136, 45);
            this.opacityTrackBar.Maximum = 255;
            this.opacityTrackBar.Name = "opacityTrackBar";
            this.opacityTrackBar.Size = new System.Drawing.Size(128, 24);
            this.opacityTrackBar.TabIndex = 1;
            this.opacityTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.opacityTrackBar.ValueChanged += new System.EventHandler(this.opacityTrackBar_ValueChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 0;
            this.label2.Text = "Opacity:";
            // 
            // BitmapLayerPropertiesDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(290, 205);
            this.Controls.Add(this.blendingGroupBox);
            this.Name = "BitmapLayerPropertiesDialog";
            this.Controls.SetChildIndex(this.blendingGroupBox, 0);
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.applyButton, 0);
            this.blendingGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.opacityUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.opacityTrackBar)).EndInit();
            this.ResumeLayout(false);

        }
		#endregion

        private void opacityUpDown_ValueChanged(object sender, System.EventArgs e)
        {
            if (opacityTrackBar.Value != (int)opacityUpDown.Value)
            {
                opacityTrackBar.Value = (int)opacityUpDown.Value;
                this.ResetApplyButton();
            }
        }

        private void opacityUpDown_Enter(object sender, System.EventArgs e)
        {
            opacityUpDown.Select(0, opacityUpDown.Text.Length);
        }

        private void opacityUpDown_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            /*
            if (Utility.CheckNumericUpDown(opacityUpDown))
            {
                opacityUpDown_ValueChanged(sender, e);
            }
            */
        }

        private void opacityTrackBar_ValueChanged(object sender, System.EventArgs e)
        {
            if (opacityUpDown.Value != (decimal)opacityTrackBar.Value)
            {
                opacityUpDown.Value = (decimal)opacityTrackBar.Value;
                this.ResetApplyButton();
            }
        }

        private void opacityUpDown_Leave(object sender, System.EventArgs e)
        {
            opacityUpDown_ValueChanged(sender, e);
        }

        private void blendOpComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ResetApplyButton();
        }
	}
}

