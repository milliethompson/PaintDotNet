/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for HeaderLabel.
    /// </summary>
    public class HeaderLabel : 
        System.Windows.Forms.Control
    {
        private Control leftMask;
        private Control rightMask;
        private int leftMargin = 1;
        private int rightMargin = 8;
        private System.Windows.Forms.GroupBox groupBox;

        public override string Text
        {
            get
            {
                if (this.groupBox == null)
                {
                    return string.Empty;
                }
                else
                {           
                    return this.groupBox.Text;
                }
            }

            set
            {
                if (this.groupBox != null)
                {
                    this.groupBox.Text = value + "  ";
                }
            }
        }

        [DefaultValue(1)]
        public int LeftMargin
        {
            get
            {
                return this.leftMargin;
            }

            set
            {
                this.leftMargin = value;
                PerformLayout();
            }
        }

        [DefaultValue(8)]
        public int RightMargin
        {
            get
            {
                return this.rightMargin;
            }

            set
            {
                this.rightMargin = value;
                PerformLayout();
            }
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public HeaderLabel()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            PerformLayout();
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
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.leftMask = new System.Windows.Forms.Control();
            this.rightMask = new System.Windows.Forms.Control();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Location = new System.Drawing.Point(128, 144);
            this.groupBox.Name = "groupBox";
            this.groupBox.TabStop = false;
            this.groupBox.FlatStyle = FlatStyle.System;
            // 
            // leftMask
            // 
            this.leftMask.Location = new System.Drawing.Point(0, 0);
            this.leftMask.Name = "leftMask";
            this.leftMask.TabStop = false;
            // 
            // rightMask
            // 
            this.rightMask.Location = new System.Drawing.Point(0, 0);
            this.rightMask.Name = "rightMask";
            this.rightMask.TabStop = false;
            // 
            // HeaderLabel
            // 
            this.Controls.Add(this.leftMask);
            this.Controls.Add(this.rightMask);
            this.Controls.Add(this.groupBox);
            this.TabStop = false;
            this.Name = "HeaderLabel";
            this.Size = new System.Drawing.Size(144, 14);
            this.ResumeLayout(false);

        }
        #endregion

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.groupBox.Location = new Point(-8 + leftMargin, 0);
            this.groupBox.Size = new Size(this.ClientRectangle.Width + 16, this.ClientRectangle.Height + 16);
            this.leftMask.Location = new Point(-1, 0);
            this.leftMask.Size = new Size(1 + leftMargin, this.ClientRectangle.Height);
            this.rightMask.Location = new Point(this.ClientRectangle.Width - rightMargin, 0);
            this.rightMask.Size = new Size(1 + rightMargin, this.ClientRectangle.Height);

            base.OnLayout(levent);
        }

        private void textLabel_TextChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
        }

        private void textLabel_FontChanged(object sender, System.EventArgs e)
        {
            PerformLayout();
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }
    }
}
