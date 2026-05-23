using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// This Form class is used to fix a few bugs in Windows Forms. We derive from
	/// this class instead of from Windows.Forms.Form directly.
	/// </summary>
	public class PdnBaseForm 
        : System.Windows.Forms.Form
	{
        private System.Windows.Forms.ToolTip toolTipSentinel;
        private System.Windows.Forms.Control fixNoToolTipsAndFocusBugSentinel;
        private System.ComponentModel.IContainer components;

		public PdnBaseForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
            toolTipSentinel.SetToolTip(fixNoToolTipsAndFocusBugSentinel, "fixed");
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            this.toolTipSentinel = new System.Windows.Forms.ToolTip(this.components);
            this.fixNoToolTipsAndFocusBugSentinel = new System.Windows.Forms.Control();
            this.SuspendLayout();
            // 
            // fixNoToolTipsAndFocusBugSentinel
            // 
            this.fixNoToolTipsAndFocusBugSentinel.Location = new System.Drawing.Point(10000, 10000);
            this.fixNoToolTipsAndFocusBugSentinel.Name = "fixNoToolTipsAndFocusBugSentinel";
            this.fixNoToolTipsAndFocusBugSentinel.Size = new System.Drawing.Size(75, 23);
            this.fixNoToolTipsAndFocusBugSentinel.TabIndex = 0;
            this.fixNoToolTipsAndFocusBugSentinel.Text = "control1";
            this.fixNoToolTipsAndFocusBugSentinel.Location = new Point(10000, 10000);
            // 
            // PdnBaseForm
            // 
            this.Controls.Add(this.fixNoToolTipsAndFocusBugSentinel);
            this.Name = "PdnBaseForm";
            this.Text = "PdnBaseForm";
            this.ResumeLayout(false);

        }
		#endregion
	}
}
