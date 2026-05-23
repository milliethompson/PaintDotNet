using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	public class RedEyeRemoveEffectDialog : PaintDotNet.Effects.TwoAmountsConfigDialog
	{
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.IContainer components = null;

		public RedEyeRemoveEffectDialog()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();
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
					components. Dispose();
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
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point(86, 201);
			this.okButton.Name = "okButton";
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point(174, 201);
			this.cancelButton.Name = "cancelButton";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 168);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(240, 24);
			this.label1.TabIndex = 9;
			this.label1.Text = "Hint: Use select tool and select each eye for best results";
			// 
			// RedEyeRemoveEffectDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(258, 232);
			this.Controls.Add(this.label1);
			this.Location = new System.Drawing.Point(0, 0);
			this.Name = "RedEyeRemoveEffectDialog";
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.ResumeLayout(false);

		}
		#endregion
	}
}

