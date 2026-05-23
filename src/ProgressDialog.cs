using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ProgressDialog.
    /// </summary>
    public class ProgressDialog 
        : PdnBaseForm
    {
        private System.Windows.Forms.ProgressBar percentBar;
        private System.Windows.Forms.Label percentText;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label descriptionLabel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private int normalHeight;
        private int noButtonHeight;
        private bool cancellable = true;
        private bool done = false;

        public ProgressDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Value = 0.0;
            
            Point bottomPoint = this.PointToScreen(new Point(0, Bottom));
            Point topPoint = this.cancelButton.PointToScreen(new Point(0, 0));
            normalHeight = Height;
            noButtonHeight = Height - 32; // (bottomPoint.Y - topPoint.Y);
        }

        public string Description
        {
            get
            {
                return descriptionLabel.Text;
            }

            set
            {
                descriptionLabel.Text = value;
            }
        }

        public bool Cancellable
        {
            get
            {
                return cancelButton.Visible;
            }

            set
            {
                if (value == true)
                {
                    this.Height = normalHeight;
                }
                else
                {
                    this.Height = noButtonHeight;
                }

                this.cancelButton.Visible = value;
                cancellable = value;
            }
        }

        public double Value
        {
            set
            {
                int intValue = (int)value;
                string text = intValue.ToString() + "%";

                if (text != percentText.Text)
                {
                    percentText.Text = intValue.ToString() + "%";
                    percentBar.Value = intValue;
                    Update();
                }
            }

            get
            {
                return (double)percentBar.Value;
            }
        }

        private void SetValue(object value)
        {
            this.Value = (double)value;
        }

        public void ExternalFinish()
        {
            done = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        public void RenderedTileHandler(object sender, RenderedTileEventArgs e)
        {
            lock (this)
            {
                double newValue = 100.0 * ((double)(e.TileNumber + 1) / (double)e.TileCount);

                if (newValue > Value)
                {
                    if (this.IsHandleCreated)
                    {
                        BeginInvoke(new WaitCallback(SetValue), new object[] { newValue });
                    }
                }
            }
        }

        public void FinishedRenderingHandler(object sender, EventArgs e)
        {
            BeginInvoke(new VoidVoidDelegate(ExternalFinish), null);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing (e);

            if (cancellable == false && done == false)
            {
                e.Cancel = true;
            }
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.percentBar = new System.Windows.Forms.ProgressBar();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.percentText = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // percentBar
            // 
            this.percentBar.Location = new System.Drawing.Point(17, 32);
            this.percentBar.Name = "percentBar";
            this.percentBar.Size = new System.Drawing.Size(184, 16);
            this.percentBar.Step = 1;
            this.percentBar.TabIndex = 0;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Location = new System.Drawing.Point(16, 8);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(184, 16);
            this.descriptionLabel.TabIndex = 1;
            this.descriptionLabel.Text = "Description goes here";
            // 
            // percentText
            // 
            this.percentText.Location = new System.Drawing.Point(59, 56);
            this.percentText.Name = "percentText";
            this.percentText.Size = new System.Drawing.Size(100, 16);
            this.percentText.TabIndex = 2;
            this.percentText.Text = "label2";
            this.percentText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(72, 80);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // ProgressDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(218, 109);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.percentText);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.percentBar);
            this.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Dialog";
            this.Controls.SetChildIndex(this.percentBar, 0);
            this.Controls.SetChildIndex(this.descriptionLabel, 0);
            this.Controls.SetChildIndex(this.percentText, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.ResumeLayout(false);

        }
        #endregion

        public event EventHandler CancelClick;
        protected virtual void OnCancelClick()
        {
            if (CancelClick != null)
            {
                CancelClick(this, EventArgs.Empty);
            }
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            OnCancelClick();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            Owner.Cursor = this.Cursor;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed (e);
            Owner.Cursor = Cursors.Default;
        }
    }
}
