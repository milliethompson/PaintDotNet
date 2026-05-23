using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerPropertiesDialog.
    /// </summary>
    public class LayerPropertiesDialog 
        : PdnBaseForm
    {
        protected System.Windows.Forms.CheckBox visibleCheckBox;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.GroupBox groupBox1;
        protected System.Windows.Forms.TextBox nameBox;
        protected System.Windows.Forms.Button cancelButton;
        protected System.Windows.Forms.Button okButton;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private object originalProperties = null;        

        private Layer layer;

        [Browsable(false)]
        public Layer Layer
        {
            get
            {
                return layer;
            }

            set
            {
                layer = value;
                originalProperties = layer.SaveProperties();
                InitDialogFromLayer();
            }
        }

        protected virtual void InitLayerFromDialog()
        {
            layer.Name = this.nameBox.Text;
            layer.Visible = this.visibleCheckBox.Checked;
            
            if (this.Owner != null)
            {
                this.Owner.Update();
            }
        }

        protected virtual void InitDialogFromLayer()
        {
            this.nameBox.Text = layer.Name;
            this.nameBox.ReadOnly = layer.IsBackground;
            this.visibleCheckBox.Checked = layer.Visible;
        }

        public LayerPropertiesDialog()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuLayersLayerPropertiesIcon.bmp"), Color.FromArgb(192, 192, 192));
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
            this.visibleCheckBox = new System.Windows.Forms.CheckBox();
            this.nameBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // visibleCheckBox
            // 
            this.visibleCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.visibleCheckBox.Location = new System.Drawing.Point(16, 40);
            this.visibleCheckBox.Name = "visibleCheckBox";
            this.visibleCheckBox.Size = new System.Drawing.Size(72, 16);
            this.visibleCheckBox.TabIndex = 3;
            this.visibleCheckBox.Text = "Visible";
            this.visibleCheckBox.CheckedChanged += new System.EventHandler(this.visibleCheckBox_CheckedChanged);
            // 
            // nameBox
            // 
            this.nameBox.Location = new System.Drawing.Point(56, 16);
            this.nameBox.Name = "nameBox";
            this.nameBox.Size = new System.Drawing.Size(200, 20);
            this.nameBox.TabIndex = 2;
            this.nameBox.Text = "Background";
            this.nameBox.Enter += new System.EventHandler(this.nameBox_Enter);
            // 
            // label1
            // 
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(7, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.visibleCheckBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.nameBox);
            this.groupBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBox1.Location = new System.Drawing.Point(8, 8);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 64);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "General";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(208, 88);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(128, 88);
            this.okButton.Name = "okButton";
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // LayerPropertiesDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(288, 115);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayerPropertiesDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Layer Properties";
            this.Controls.SetChildIndex(this.groupBox1, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.okButton, 0);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private void nameBox_Enter(object sender, System.EventArgs e)
        {
            nameBox.Select(0, nameBox.Text.Length);
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;

            using (new WaitCursorChanger(this))
            {
                layer.PushSuppressPropertyChanged();
                InitLayerFromDialog();
                object currentProperties = layer.SaveProperties();
                layer.LoadProperties(this.originalProperties);
                layer.PopSuppressPropertyChanged();

                layer.LoadProperties(currentProperties);
                originalProperties = layer.SaveProperties();
                layer.Invalidate();
            }
            
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;

            using (new WaitCursorChanger(this))
            {
                layer.PushSuppressPropertyChanged();
                layer.LoadProperties(originalProperties);
                layer.PopSuppressPropertyChanged();
                layer.Invalidate();
            }

            Close();
        }

        private void visibleCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            Layer.PushSuppressPropertyChanged();
            Layer.Visible = visibleCheckBox.Checked;
            Layer.PopSuppressPropertyChanged();
        }
    }
}
