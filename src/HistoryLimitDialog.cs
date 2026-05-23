using System;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for HistoryLimitDialog.
	/// </summary>
	// Parameters: none
	// Properties: Limit (set before showing dialog, check after dialog)
	// Returns: DialogResult
	// Initial Conception: Michael Kelsey
	// ..Alterations: provide the dialog for adjustable history length
	// Changes: Rick Brewster
	// ..Alterations: revamp the dialog from a combobox to a trackbar
	// ...enumerate choices
	// ..."pretty-up" the widgets
	// ...add ESC and ENTER response
	// Most recent changes: Michael Kelsey
	// ..Alterations: added new icon that "appears" against a black menu bar
	// Purpose: presents the dialog for adjustable history length.

	public class HistoryLimitDialog : PdnBaseForm
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.TrackBar limitSlider;
		private System.Windows.Forms.Label limitLabel;
		private System.Windows.Forms.ToolTip tooltipProvider;
		private System.Windows.Forms.GroupBox groupBox;

		private string[] choices = new string[] { "10", "25", "50", "100", "500", "Unlimited" };

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
			this.limitSlider = new System.Windows.Forms.TrackBar();
			this.limitLabel = new System.Windows.Forms.Label();
			this.groupBox = new System.Windows.Forms.GroupBox();
			((System.ComponentModel.ISupportInitialize)(this.limitSlider)).BeginInit();
			this.groupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(50, 82);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "&OK";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(130, 82);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "&Cancel";
			// 
			// limitSlider
			// 
			this.limitSlider.Location = new System.Drawing.Point(8, 16);
			this.limitSlider.Name = "limitSlider";
			this.limitSlider.TabIndex = 4;
			this.limitSlider.ValueChanged += new System.EventHandler(this.limitSlider_ValueChanged);
			// 
			// limitLabel
			// 
			this.limitLabel.Location = new System.Drawing.Point(120, 24);
			this.limitLabel.Name = "limitLabel";
			this.limitLabel.Size = new System.Drawing.Size(64, 23);
			this.limitLabel.TabIndex = 5;
			this.limitLabel.Text = "Unlimited";
			// 
			// groupBox
			// 
			this.groupBox.Controls.Add(this.limitSlider);
			this.groupBox.Controls.Add(this.limitLabel);
			this.groupBox.Location = new System.Drawing.Point(8, 8);
			this.groupBox.Name = "groupBox";
			this.groupBox.Size = new System.Drawing.Size(194, 64);
			this.groupBox.TabIndex = 6;
			this.groupBox.TabStop = false;
			this.groupBox.Text = "Purge Limit";
			// 
			// HistoryLimitDialog
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(210, 111);
			this.Controls.Add(this.groupBox);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "HistoryLimitDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Limit History";
			this.Controls.SetChildIndex(this.buttonOK, 0);
			this.Controls.SetChildIndex(this.buttonCancel, 0);
			this.Controls.SetChildIndex(this.groupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.limitSlider)).EndInit();
			this.groupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		/// <summary>
		/// Represents the value of the history limit within the HistoryLimitDialog.
		/// </summary>
		private int limit = -1;
		public int Limit
		{
			set
			{
				int choicesIndex;

				if (value == -1)
				{
					choicesIndex = choices.Length - 1;
					this.limit = -1;
				}
				else
				{
					choicesIndex = Array.IndexOf(choices, value.ToString());
					
					if (choicesIndex == -1)
					{
						this.limit = -1;
					}
					else
					{
						this.limit = value;
					}
				}

				SetLimitChoicesIndex(choicesIndex);
			}

			get
			{
				int limitValue;

				try
				{
					limitValue = int.Parse(limitLabel.Text);
				}

				catch (FormatException)
				{
					limitValue = -1;
				}

				return limitValue;
			}
		}

		public HistoryLimitDialog()
		{
			InitializeComponent();
			limitSlider.Maximum = choices.Length - 1;
			this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.HistoryLimitDialogIcon.bmp"), System.Drawing.Color.FromArgb(192, 192, 192));
		}

		private void SetLimitChoicesIndex(int choice)
		{
			limitLabel.Text = choices[choice];

			if (limitSlider.Value != choice)
			{
				limitSlider.Value = choice;
			}
		}

		private void limitSlider_ValueChanged(object sender, System.EventArgs e)
		{
			try
			{
				this.Limit = int.Parse(choices[limitSlider.Value]);
			}

			catch
			{
				this.Limit = -1;
			}
		}
	}
}