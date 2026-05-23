using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerElement.
    /// </summary>
    public class LayerElement : System.Windows.Forms.UserControl
    {
        private Layer layer;
        private bool isSelected;

        private EventHandler layerPreviewChangedDelegate;
        private PropertyEventHandler layerPropertyChangedDelegate;
        private System.Windows.Forms.Label layerDescription;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.CheckBox layerVisible;
		
		public System.Windows.Forms.CheckBox LayerVisible
		{
			get
			{
				return this.layerVisible;
			}
		}

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;

                if (isSelected)
                {
                    this.layerDescription.BackColor = SystemColors.Highlight;
                    this.layerDescription.ForeColor = SystemColors.HighlightText;
                    this.layerVisible.BackColor = layerDescription.BackColor;
                }
                else // !selected
                {               
                    this.layerDescription.ForeColor = SystemColors.WindowText;
                    this.layerDescription.BackColor = SystemColors.Window;
                    this.layerVisible.BackColor = layerDescription.BackColor;
                    this.icon.BackColor = Color.White;
                }

                Update();
            }
        }

        public Image Image
        {
            get
            {
                return icon.Image;
            }
            set
            {
                icon.Image = value;
                Invalidate(true);
            }
        }

        public Layer Layer 
        {
            get
            {
                return layer;
            }

            set
            {
                if (layer != null)
                {
                    layer.PreviewChanged -= layerPreviewChangedDelegate;
                    layer.PropertyChanged -= layerPropertyChangedDelegate;
                }
                
                layer = value;

                if (layer != null)
                {
                    layer.PreviewChanged += layerPreviewChangedDelegate;
                    layer.PropertyChanged += layerPropertyChangedDelegate;
                    this.layerPropertyChangedDelegate(layer, new PropertyEventArgs("")); // sync up

                    if (layer.IsBackground)
                    {
                        this.layerDescription.Font = new Font(layerDescription.Font.FontFamily, layerDescription.Font.Size, layerDescription.Font.Style | FontStyle.Italic);
                    }
                }
            }
        }
        
        public LayerElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.IsSelected = false;

            layerPreviewChangedDelegate = new EventHandler(LayerPreviewChangedHandler);
            layerPropertyChangedDelegate = new PropertyEventHandler(LayerPropertyChangedHandler);
        }

        private void SetPreview()
        {
            if (layer != null && layer.Preview != null)
            {
                Image = layer.Preview;
            }
        }

        private void LayerPreviewChangedHandler(object sender, EventArgs e)
        {
            // hack: some weird bug pops up sometimes during initialization ...
            while (!this.IsHandleCreated)
            {
                Thread.Sleep(1);
            }

            if (this.layer != null)
            {
                this.BeginInvoke(new VoidVoidDelegate(this.SetPreview), null);
            }
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            this.layerDescription.Text = layer.Name;
            this.layerVisible.Checked = layer.Visible;
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layerDescription = new System.Windows.Forms.Label();
            this.icon = new System.Windows.Forms.PictureBox();
            this.layerVisible = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // layerDescription
            // 
            this.layerDescription.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.layerDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerDescription.Location = new System.Drawing.Point(34, 0);
            this.layerDescription.Name = "layerDescription";
            this.layerDescription.Size = new System.Drawing.Size(150, 34);
            this.layerDescription.TabIndex = 9;
            this.layerDescription.Text = "Layer Info";
            this.layerDescription.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.layerDescription.Click += new System.EventHandler(this.Control_Click);
            this.layerDescription.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // icon
            // 
            this.icon.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.icon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.icon.Dock = System.Windows.Forms.DockStyle.Left;
            this.icon.Location = new System.Drawing.Point(0, 0);
            this.icon.Name = "icon";
            this.icon.Size = new System.Drawing.Size(34, 34);
            this.icon.TabIndex = 8;
            this.icon.TabStop = false;
            this.icon.Click += new System.EventHandler(this.Control_Click);
            this.icon.DoubleClick += new System.EventHandler(this.Control_DoubleClick);
            // 
            // layerVisible
            // 
            this.layerVisible.BackColor = System.Drawing.SystemColors.Control;
            this.layerVisible.Checked = true;
            this.layerVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.layerVisible.Dock = System.Windows.Forms.DockStyle.Right;
            this.layerVisible.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.layerVisible.Location = new System.Drawing.Point(184, 0);
            this.layerVisible.Name = "layerVisible";
            this.layerVisible.Size = new System.Drawing.Size(16, 34);
            this.layerVisible.TabIndex = 7;
            this.layerVisible.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.layerVisible_KeyPress);
            this.layerVisible.Click += new System.EventHandler(this.Control_Click);
            this.layerVisible.CheckStateChanged += new System.EventHandler(this.layerVisible_CheckStateChanged);
            this.layerVisible.KeyUp += new System.Windows.Forms.KeyEventHandler(this.layerVisible_KeyUp);
            // 
            // LayerElement
            // 
            this.Controls.Add(this.layerDescription);
            this.Controls.Add(this.icon);
            this.Controls.Add(this.layerVisible);
            this.Name = "LayerElement";
            this.Size = new System.Drawing.Size(200, 34);
            this.ResumeLayout(false);

        }
        #endregion

        private void Control_Click(object sender, System.EventArgs e)
        {
            OnClick(e);
        }

        private void Control_DoubleClick(object sender, System.EventArgs e)
        {
            OnDoubleClick(e);
        }

        private void layerVisible_CheckStateChanged(object sender, System.EventArgs e)
        {
            layer.Visible = layerVisible.Checked;
        }

        private void layerVisible_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void layerVisible_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
    }
}
