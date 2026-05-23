/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for LayerElement.
    /// </summary>
    public class LayerElement 
        : System.Windows.Forms.UserControl
    {
        private Layer layer;
        private bool isSelected;

        private PropertyEventHandler layerPropertyChangedDelegate;
        private System.Windows.Forms.Label layerDescription;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.CheckBox layerVisible;
        private PaintDotNet.Threading.ThreadPool threadPool = new PaintDotNet.Threading.ThreadPool(2);
		
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
                if (icon.Image != null)
                {
                    icon.Image.Dispose();
                    icon.Image = null;
                }

                icon.Image = value;
                Invalidate(true);
                Update();
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
                if (object.ReferenceEquals(this.layer, value))
                {
                    return;
                }

                if (layer != null)
                {
                    layer.PropertyChanged -= layerPropertyChangedDelegate;
                    layer.Invalidated -= new InvalidateEventHandler(layer_Invalidated);
                }
                
                if (this.threadPool != null)
                {
                    this.threadPool.Drain();
                }

                layer = value;

                if (layer != null)
                {
                    layer.PropertyChanged += layerPropertyChangedDelegate;
                    layer.Invalidated += new InvalidateEventHandler(layer_Invalidated);
                    this.layerPropertyChangedDelegate(layer, new PropertyEventArgs("")); // sync up

                    // Add italics if it's the background layer
                    if (layer.IsBackground)
                    {
                        this.layerDescription.Font = new Font(layerDescription.Font.FontFamily, layerDescription.Font.Size, layerDescription.Font.Style | FontStyle.Italic);
                    }
                }

                Update();
            }
        }
        
        public LayerElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            this.IsSelected = false;

            layerPropertyChangedDelegate = new PropertyEventHandler(LayerPropertyChangedHandler);

            this.TabStop = false;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Layer = null;

                if (threadPool != null)
                {
                    threadPool.Drain();
                    threadPool = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        private void LayerPropertyChangedHandler(object sender, PropertyEventArgs e)
        {
            this.layerDescription.Text = layer.Name;
            this.layerVisible.Checked = layer.Visible;
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
            this.icon.BackColor = System.Drawing.SystemColors.Control;
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
            this.layerVisible.BackColor = System.Drawing.SystemColors.Window;
            this.layerVisible.Checked = true;
            this.layerVisible.CheckState = System.Windows.Forms.CheckState.Checked;
            this.layerVisible.Dock = System.Windows.Forms.DockStyle.Right;
            this.layerVisible.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.layerVisible.Location = new System.Drawing.Point(184, 0);
            this.layerVisible.Name = "layerVisible";
            this.layerVisible.Size = new System.Drawing.Size(16, 34);
            this.layerVisible.TabIndex = 7;
            this.layerVisible.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.layerVisible_KeyPress);
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
            Update();
        }

        private void layerVisible_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            this.OnKeyPress(e);
        }

        private void layerVisible_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }

        private int suspendPreviewUpdates = 0;
        private int needToUpdatePreview = 0;
        private object previewLock = new object();

        private void layer_Invalidated(object sender, InvalidateEventArgs e)
        {
            RefreshPreview();
        }

        public void SuspendPreviewUpdates()
        {
            ++suspendPreviewUpdates;
        }

        public void ResumePreviewUpdates()
        {
            --suspendPreviewUpdates;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            RefreshPreview();
            base.OnHandleCreated(e);
        }


        public void RefreshPreview()
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            if (suspendPreviewUpdates == 0)
            {
                if (1 == Interlocked.Increment(ref this.needToUpdatePreview))
                {
                    this.threadPool.QueueUserWorkItem(new WaitCallback(UpdatePreviewHandler), 25);
                }
            }
        }

        private void UpdatePreviewHandler(object context)
        {
            if (!this.IsHandleCreated)
            {
                return;
            }

            int delay = (int)context;

            lock (previewLock) // make sure to serialize preview updates ... sometimes we get more than one in there
            {
                ThreadPriority oldPriority = Thread.CurrentThread.Priority;
                bool oldIsBackground = Thread.CurrentThread.IsBackground;
                Thread.CurrentThread.IsBackground = false;
                Thread.CurrentThread.Priority = ThreadPriority.Lowest;

                try
                {
                    Thread.Sleep(delay);
                    UpdatePreview();
                }

                catch
                {
                }                

                finally
                {
                    if (this.needToUpdatePreview > 1)
                    {
                        if (this.IsHandleCreated)
                        {
                            this.BeginInvoke(new VoidVoidDelegate(RefreshPreview));
                        }
                    }

                    this.needToUpdatePreview = 0;

                    Thread.CurrentThread.Priority = oldPriority;
                    Thread.CurrentThread.IsBackground = oldIsBackground;
                }
            }
        }    

        private void UpdatePreview()
        {
            int previewSide = 32;
            Size previewSize;

            // decide size ... are we 'tall' or 'wide' ?
            if (layer.Width > layer.Height)
            {   // wide
                previewSize = new Size(previewSide, Math.Max(1, (layer.Height * previewSide) / layer.Width));
            }
            else
            {   //tall
                previewSize = new Size(Math.Max(1, (layer.Width * previewSide) / layer.Height), previewSide);
            }

            Surface surface = new Surface(previewSide, previewSide);
            surface.Clear(ColorBgra.White);
            Surface previewWindow = surface.CreateWindow(new Rectangle(new Point((previewSide - previewSize.Width) / 2, (previewSide - previewSize.Height) / 2), previewSize));

            if (layer is BitmapLayer)
            {
                previewWindow.SuperSamplingFitSurface(((BitmapLayer)layer).Surface);
            }
            else
            {
                // TODO: once we get double-dispatch in place (v2.2) for pairing actions and layer types, use that for this
                // see bug #996
                // (IResize x IBitmapLayerAction).Resize(dstRA, srcLayer)
            }

            previewWindow.Dispose();

            //new PaintDotNet.Effects.SharpenEffect().RenderInPlace(new RenderArgs(surface), surface.Bounds);

            Bitmap bitmap = new Bitmap(surface.Width, surface.Height);

            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    bitmap.SetPixel(x, y, surface[x,y].ToColor());
                }
            }

            // We wrap this Dispose() in a try/empty-catch because it's possible we're executing after the rest
            // of the app has shutdown. Which means that Memory's heap is destroyed and this will throw an
            // exception. And if that throws an exception we don't want to do any of the rest of the code.
            surface.Dispose();

            DateTime start = DateTime.Now;
            while (!this.IsHandleCreated)
            {
                Thread.Sleep(20);

                if (DateTime.Now - start > new TimeSpan(0, 0, 3))
                {
                    return;
                }
            }

            this.BeginInvoke(new VoidObjectDelegate(SetImageProperty), new object[] { bitmap });
        }

        private void SetImageProperty(object newValue)
        {
            this.Image = (Image)newValue;
        }
    }
}
