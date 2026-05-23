using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for HistoryElementControl.
	/// </summary>
	public class HistoryElementControl : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Label historyDescription;
		private IconBox historyIcon;
		private bool isUndo;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public HistoryElementControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			IsUndo = true;
            historyIcon.TransparentColor = Color.FromArgb(192, 192, 192);

            historyIcon.KeyUp += new KeyEventHandler(historyIcon_KeyUp);
		}

		public Image Image
		{
			get
			{
				return historyIcon.Icon;
			}

			set
			{
                historyIcon.Icon = null;

                if (value != null)
                {
                    historyIcon.Icon = new Bitmap(value);
                }

				Invalidate(true);
			}
		}

		public string Description
		{
			get
			{
				return historyDescription.Text;
			}

			set
			{
				historyDescription.Text = value;
				Invalidate(true);
			}
		}

		public bool IsUndo
		{
			get
			{
				return isUndo;
			}

			set
			{
                isUndo = value;

                FontStyle style = historyDescription.Font.Style;

                if (!isUndo)
                {
                    style |= FontStyle.Italic;
                }
                else
                {
                    style &= ~FontStyle.Italic;
                }

                historyDescription.Font = new Font(historyDescription.Font, style);

				SetColor();
			}
		}

		private void SetColor()
		{
			if (isUndo)
			{
				this.BackColor = Color.White;
			}
			else
			{
				this.BackColor = Color.SlateGray;
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.historyDescription = new System.Windows.Forms.Label();
            this.historyIcon = new PaintDotNet.IconBox();
            this.SuspendLayout();
            // 
            // historyDescription
            // 
            this.historyDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.historyDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyDescription.Location = new System.Drawing.Point(16, 0);
            this.historyDescription.Name = "historyDescription";
            this.historyDescription.Size = new System.Drawing.Size(134, 24);
            this.historyDescription.TabIndex = 0;
            this.historyDescription.Text = "I Love History";
            this.historyDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.historyDescription.Click += new System.EventHandler(this.historyIcon_Click);
            // 
            // historyIcon
            // 
            this.historyIcon.Dock = System.Windows.Forms.DockStyle.Left;
            this.historyIcon.Icon = null;
            this.historyIcon.Location = new System.Drawing.Point(0, 0);
            this.historyIcon.Name = "historyIcon";
            this.historyIcon.Size = new System.Drawing.Size(16, 24);
            this.historyIcon.TabIndex = 1;
            this.historyIcon.TabStop = false;
            this.historyIcon.Click += new System.EventHandler(this.historyIcon_Click);
            // 
            // HistoryElementControl
            // 
            this.Controls.Add(this.historyDescription);
            this.Controls.Add(this.historyIcon);
            this.Name = "HistoryElementControl";
            this.Size = new System.Drawing.Size(150, 24);
            this.ResumeLayout(false);

        }
		#endregion

		private void historyIcon_Click(object sender, System.EventArgs e)
		{
			OnClick(e);
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
//			base.OnPaintBackground (pevent);
        }

        private void historyIcon_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        protected override void Select(bool directed, bool forward)
        {
            base.Select (directed, forward);
        }
    }
}
