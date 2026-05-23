/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
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
        private static bool allowRefreshPreview = true; // when non-zero, RefreshPreview() will not do anything (and will then decrement this) -- used as startup perf optimization

        public static void SetAllowRefreshPreview(bool flag)
        {
            allowRefreshPreview = flag;
        }

        private Layer layer;
        private bool isSelected;
        public const int ThumbSize = 40;

        private PropertyEventHandler layerPropertyChangedDelegate;
        private System.Windows.Forms.Label layerDescription;
        private System.Windows.Forms.PictureBox icon;
        private System.Windows.Forms.CheckBox layerVisible;

        private int suspendPreviewUpdates = 0;

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
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;

                if (this.isSelected)
                {
                    this.layerDescription.BackColor = SystemColors.Highlight;
                    this.layerDescription.ForeColor = SystemColors.HighlightText;
                    this.layerVisible.BackColor = this.layerDescription.BackColor;
                }
                else // !selected
                {               
                    this.layerDescription.ForeColor = SystemColors.WindowText;
                    this.layerDescription.BackColor = SystemColors.Window;
                    this.layerVisible.BackColor = this.layerDescription.BackColor;
                    this.icon.BackColor = Color.White;
                }

                Update();
            }
        }

        public Image Image
        {
            get
            {
                return this.icon.Image;
            }

            set
            {
                if (this.icon.Image != null)
                {
                    this.icon.Image.Dispose();
                    this.icon.Image = null;
                }

                this.icon.Image = value;
                Invalidate(true);
                Update();
            }
        }

        public Layer Layer 
        {
            get
            {
                return this.layer;
            }

            set
            {
                if (object.ReferenceEquals(this.layer, value))
                {
                    return;
                }

                if (this.layer != null)
                {
                    this.layer.PropertyChanged -= this.layerPropertyChangedDelegate;
                    this.layer.Invalidated -= new InvalidateEventHandler(layer_Invalidated);
                }
                
                this.layer = value;

                if (this.layer != null)
                {
                    this.layer.PropertyChanged += this.layerPropertyChangedDelegate;
                    this.layer.Invalidated += new InvalidateEventHandler(layer_Invalidated);
                    this.layerPropertyChangedDelegate(layer, new PropertyEventArgs("")); // sync up

                    // Add italics if it's the background layer
                    if (this.layer.IsBackground)
                    {
                        this.layerDescription.Font = new Font(this.layerDescription.Font.FontFamily, this.layerDescription.Font.Size, 
                            this.layerDescription.Font.Style | FontStyle.Italic);
                    }
                }

                Update();
            }
        }
        
        public LayerElement()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.SuspendLayout();
            InitializeComponent();
            InitializeComponent2();
            this.ResumeLayout(false);
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

        void InitializeComponent2()
        {
            this.Size = new System.Drawing.Size(200, 2 + LayerElement.ThumbSize);
            this.layerDescription.Location = new System.Drawing.Point(this.Height, 0);
            this.icon.Size = new System.Drawing.Size(this.Height, this.Height);
            this.layerVisible.Size = new System.Drawing.Size(16, this.Height);
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
            this.layerDescription.Name = "layerDescription";
            this.layerDescription.Size = new System.Drawing.Size(150, 50);
            this.layerDescription.TabIndex = 9;
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
            this.layer.Visible = this.layerVisible.Checked;
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
            if (!allowRefreshPreview)
            {
                return;
            }

            if (this.suspendPreviewUpdates > 0)
            {
                return;
            }

            if (!this.IsHandleCreated)
            {
                return;
            }

            lock (refreshLock)
            {
                if (layersToRefresh.Count == 0 ||
                    !object.ReferenceEquals(layersToRefresh.Peek(), this))
                {
                    layersToRefresh.Push(this);
                }

                Monitor.Pulse(refreshLock);
            }
        }

        private static object refreshLock = new object();
        private static Stack layersToRefresh = new Stack();
        private static bool quitPreviewThread = false;
        private static Thread previewThread;

        private static void PreviewThread()
        {
            while (true)
            {
                LayerElement layerElement;

                lock (refreshLock)
                {
                    while (layersToRefresh.Count == 0)
                    {
                        Monitor.Wait(refreshLock);

                        if (quitPreviewThread)
                        {
                            return;
                        }
                    }

                    layerElement = (LayerElement)layersToRefresh.Pop();
                }
            
                Thread.Sleep(100);

                if (layerElement.IsHandleCreated)
                {
                    try
                    {
                        layerElement.UpdatePreview();
                    }

                    catch (Exception ex)
                    {
                        try
                        {
                            Tracing.Ping("Exception in PreviewThread after calling UpdatePreview: " + ex);
                        }

                        catch
                        {
                        }
                    }
                }
            }
        }

        static LayerElement()
        {
            previewThread = new Thread(new ThreadStart(PreviewThread));
            previewThread.Priority = ThreadPriority.Lowest;
            previewThread.IsBackground = true;
            previewThread.Start();
        }

        private void UpdatePreview()
        {
            int previewSide = LayerElement.ThumbSize;
            Size previewSize;

            // decide size ... are we 'tall' or 'wide' ?
            if (layer.Width > layer.Height)
            {   
                // wide
                previewSize = new Size(previewSide, Math.Max(1, (layer.Height * previewSide) / layer.Width));
            }
            else
            {   
                // tall
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
                // TODO: once we get double-dispatch in place (v3.0) for pairing actions and layer types, use that for this
                // see bug #996
                // (IResize x IBitmapLayerAction).Resize(dstRA, srcLayer)
            }

            previewWindow.Dispose();

            Bitmap bitmap = new Bitmap(surface.Width, surface.Height);

            for (int y = 0; y < bitmap.Height; ++y)
            {
                for (int x = 0; x < bitmap.Width; ++x)
                {
                    bitmap.SetPixel(x, y, surface[x,y].ToColor());
                }
            }

            surface.Dispose();

            DateTime start = DateTime.Now;
            while (!this.IsHandleCreated)
            {
                Thread.Sleep(20);

                if ((DateTime.Now - start) > new TimeSpan(0, 0, 0, 0, 250))
                {
                    return;
                }
            }

            // Make sure the next queued layer refresh isn't ourself. If it is, throw away our work
            // to avoid ugly thumbnail flickering
            lock (refreshLock)
            {
                if (layersToRefresh.Count > 0 && object.ReferenceEquals(this, layersToRefresh.Peek()))
                {
                    bitmap.Dispose();
                }
                else
                {
                    this.BeginInvoke(new VoidObjectDelegate(SetImageProperty), new object[] { bitmap });
                }
            }
        }

        private void SetImageProperty(object newValue)
        {
            this.Image = (Image)newValue;
        }
    }
}
