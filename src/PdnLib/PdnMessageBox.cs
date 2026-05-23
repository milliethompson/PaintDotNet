using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
	public class PdnMessageBox 
		: PaintDotNet.PdnBaseForm
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Panel pnlButtons;
		private System.Windows.Forms.Label lblMessage;
		private System.Windows.Forms.Panel pnlOptions;
		private RadioButton [] radioOptions = null;

		public int Result 
		{
			get 
			{
				if (radioOptions == null) 
				{
					return -1;
				}
				for (int i = 0; i < radioOptions.Length; i++) 
				{
					RadioButton btn = radioOptions[i];
					if (btn != null && btn.Checked) 
					{
						return i;
					}
				}
				return -1;
			}
			set 
			{
				if (radioOptions == null || value < 0 || value > radioOptions.Length) 
				{
					throw new ArgumentException("value", "value must be a valid index into the options array, which must also be valid");
				}
				if (radioOptions[value] != null) 
				{
					radioOptions[value].Checked = true;
				}
				else
				{
					throw new Exception("radio buttons not initialized");
				}
			}
		}

		public void InitMessages(string message, string [] optionsText) 
		{
			Graphics g = CreateGraphics();
			
			if (optionsText == null) 
			{
				throw new ArgumentNullException("optionsText", "optionsText may not be null");
			}
			if (message == null) 
			{
				throw new ArgumentNullException("message", "message may not be null");
			}

			lblMessage.Text = message;
			lblMessage.Height = Size.Truncate(g.MeasureString(lblMessage.Text, lblMessage.Font, lblMessage.Width + 25)).Height;

			pnlOptions.Top = lblMessage.Bottom + 8;
			radioOptions = new RadioButton[optionsText.Length];
			pnlOptions.Width = 0;
			for (int i = 0; i < radioOptions.Length; i++) 
			{
				RadioButton rb = new RadioButton();
				rb.Dock = DockStyle.Top;
				rb.Text = optionsText[i];
				rb.Size = Size.Truncate(g.MeasureString(rb.Text, rb.Font, this.Width - 25));
				rb.Width += 25;
				rb.Height += 8;
				rb.TextAlign = ContentAlignment.MiddleLeft;
				radioOptions[i] = rb;
			}
			for (int i = radioOptions.Length - 1; i >= 0; i--) 
			{
				RadioButton rb = radioOptions[i];
				pnlOptions.Width = Math.Max(pnlOptions.Width, rb.Width);
				pnlOptions.Controls.Add(rb);
			}
			pnlOptions.Height = radioOptions[radioOptions.Length - 1].Bottom;
			pnlButtons.Top = pnlOptions.Bottom + 8;
			this.ClientSize = new Size(Math.Max(pnlOptions.Width, lblMessage.Width) + 16, pnlButtons.Bottom + 8);
		}

		public PdnMessageBox()
		{
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
			this.pnlButtons = new System.Windows.Forms.Panel();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOK = new System.Windows.Forms.Button();
			this.lblMessage = new System.Windows.Forms.Label();
			this.pnlOptions = new System.Windows.Forms.Panel();
			this.pnlButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlButtons
			// 
			this.pnlButtons.Controls.Add(this.btnCancel);
			this.pnlButtons.Controls.Add(this.btnOK);
			this.pnlButtons.Location = new System.Drawing.Point(58, 104);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Size = new System.Drawing.Size(163, 23);
			this.pnlButtons.TabIndex = 1;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(88, 0);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "&Cancel";
			// 
			// btnOK
			// 
			this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOK.Location = new System.Drawing.Point(0, 0);
			this.btnOK.Name = "btnOK";
			this.btnOK.TabIndex = 0;
			this.btnOK.Text = "&OK";
			// 
			// lblMessage
			// 
			this.lblMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblMessage.Location = new System.Drawing.Point(8, 8);
			this.lblMessage.Name = "lblMessage";
			this.lblMessage.Size = new System.Drawing.Size(272, 64);
			this.lblMessage.TabIndex = 2;
			// 
			// pnlOptions
			// 
			this.pnlOptions.Location = new System.Drawing.Point(40, 80);
			this.pnlOptions.Name = "pnlOptions";
			this.pnlOptions.Size = new System.Drawing.Size(216, 16);
			this.pnlOptions.TabIndex = 1;
			this.pnlOptions.Resize += new System.EventHandler(this.pnlOptions_Resize);
			// 
			// PdnMessageBox
			// 
			this.AcceptButton = this.btnOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(288, 133);
			this.Controls.Add(this.pnlOptions);
			this.Controls.Add(this.lblMessage);
			this.Controls.Add(this.pnlButtons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "PdnMessageBox";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Resize += new System.EventHandler(this.PdnMessageBox_Resize);
			this.Controls.SetChildIndex(this.pnlButtons, 0);
			this.Controls.SetChildIndex(this.lblMessage, 0);
			this.Controls.SetChildIndex(this.pnlOptions, 0);
			this.pnlButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		public static DialogResult Show(string question, string title, string [] options, ref int result) 
		{
			return Show(question, title, options, ref result, null);
		}

		public static DialogResult Show(string question, string title, string [] options, ref int result, IWin32Window owner) 
		{
			return Show(question, title, options, ref result, owner, null);
		}

		public static DialogResult Show(string question, string title, string[] options, ref int result, IWin32Window owner, Icon sysIcon)
		{
			if (options == null) 
			{
				options = new string[]{};
			}

			if (question == null) 
			{
				throw new ArgumentNullException("question", "question must be a valid string");
			}

			if (title == null) 
			{
				throw new ArgumentNullException("title", "title must be a valid string");
			}

			using (PdnMessageBox messageBox = new PdnMessageBox()) 
			{
				messageBox.Text = title;
				messageBox.Icon = sysIcon;
				messageBox.InitMessages(question, options);
				messageBox.Result = result;

                DialogResult dialogResult = messageBox.ShowDialog(owner);
				result = messageBox.Result;

				return dialogResult;
			}
		}

		private void PdnMessageBox_Resize(object sender, System.EventArgs e)
		{
			pnlButtons.Left = (this.ClientRectangle.Width - pnlButtons.Width) / 2;
			pnlOptions.Left = (this.ClientRectangle.Width - pnlOptions.Width) / 2;
		}

		private void pnlOptions_Resize(object sender, System.EventArgs e)
		{
			PdnMessageBox_Resize(null, EventArgs.Empty);
		}

		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated (e);

			// If PDN does not have focus, drag-and-dropping an image on to it makes
			// it so that the dialog box is not visible
			if (this.Owner != null)
			{
				this.Owner.Focus();
				Application.DoEvents();
				this.Focus();
				Application.DoEvents();
			}
		}
	}
}

