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
    /// Summary description for HistoryElement.
    /// </summary>
    public class HistoryElement : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label historyDescription;
        private IconBox historyIcon;
        private bool isUndo;

        protected override void WndProc(ref Message m)
        {
            IntPtr preR = m.Result;

            // Ignore focus
            if (m.Msg == NativeMethods.WmConstants.WM_SETFOCUS)
            {
                return;
            }

            base.WndProc (ref m);
        }   

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public HistoryElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

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
                this.ForeColor = SystemColors.WindowText;
            }
            else
            {
                this.BackColor = Color.SlateGray;
                this.ForeColor = SystemColors.InactiveCaptionText;
            }
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
                    components = null;
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
            this.historyDescription = new System.Windows.Forms.Label();
            this.historyIcon = new PaintDotNet.IconBox();
            this.SuspendLayout();
            // 
            // historyDescription
            // 
            this.historyDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.historyDescription.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.historyDescription.Location = new System.Drawing.Point(16, 0);
            this.historyDescription.Name = "historyDescription";
            this.historyDescription.Size = new System.Drawing.Size(134, 24);
            this.historyDescription.TabIndex = 0;
            this.historyDescription.UseMnemonic = false;
            this.historyDescription.Click += new System.EventHandler(this.Control_Click);
            this.historyDescription.DoubleClick += new System.EventHandler(this.Control_Click);
            this.historyDescription.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.historyDescription.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
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
            this.historyIcon.TransparentColor = System.Drawing.Color.Empty;
            this.historyIcon.Click += new System.EventHandler(this.Control_Click);
            this.historyIcon.DoubleClick += new System.EventHandler(this.Control_Click);
            this.historyIcon.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Control_MouseMove);
            this.historyIcon.MouseLeave += new System.EventHandler(this.Control_MouseLeave);
            // 
            // HistoryElement
            // 
            this.Controls.Add(this.historyDescription);
            this.Controls.Add(this.historyIcon);
            this.Name = "HistoryElement";
            this.Size = new System.Drawing.Size(150, 24);
            this.ResumeLayout(false);

        }
        #endregion

        private void Control_Click(object sender, System.EventArgs e)
        {
            OnClick(e);
        }

        private void Control_DoubleClick(object sender, System.EventArgs e)
        {
            OnClick(e);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // Do not call base so as to avoid flickering
            //base.OnPaintBackground (pevent);
        }

        private void historyIcon_KeyUp(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        protected override void Select(bool directed, bool forward)
        {
            base.Select (directed, forward);
        }

        private void Control_MouseLeave(object sender, System.EventArgs e)
        {
            SetColor();
        }

        private void Control_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.BackColor = SystemColors.Highlight;
            this.ForeColor = SystemColors.HighlightText;
        }
    }
}
