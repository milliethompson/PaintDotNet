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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
    public class LayerControl 
        : System.Windows.Forms.UserControl
    {
        private class PanelWithLayout
            : PanelEx
        {
            private LayerControl parentLayerControl;
            public LayerControl ParentLayerControl
            {
                get
                {
                    return this.parentLayerControl;
                }

                set
                {
                    this.parentLayerControl = value;
                }
            }

            public PanelWithLayout()
            {
                this.HideHScroll = true;
            }

            public void PositionLayers()
            {
                if (this.parentLayerControl != null &&
                    this.parentLayerControl.layerControls != null)
                {
                    int cursor = this.AutoScrollPosition.Y;
                    int newWidth = this.ClientRectangle.Width;

                    for (int i = this.parentLayerControl.layerControls.Count - 1; i >= 0; --i)
                    {
                        LayerElement lec = this.parentLayerControl.layerControls[i];
                        lec.Width = newWidth;
                        lec.Top = cursor;
                        cursor += lec.Height;
                    }
                }
            }

            protected override void OnResize(EventArgs eventargs)
            {
                SystemLayer.UI.SetControlRedraw(this, false);
                PositionLayers();
                this.AutoScrollPosition = new Point(0, -this.AutoScrollOffset.Y);
                base.OnResize(eventargs);
                SystemLayer.UI.SetControlRedraw(this, true);
                Invalidate(true);
            }

            protected override void OnLayout(LayoutEventArgs levent)
            {
                PositionLayers();
                base.OnLayout(levent);
            }
        }

        public void PositionLayers()
        {
            this.layerControlPanel.PositionLayers();
        }

        private EventHandler elementClickDelegate;
        private EventHandler elementDoubleClickDelegate;
        private EventHandler documentChangedDelegate;
        private EventHandler documentChangingDelegate;
        private EventHandler layerChangedDelegate;
        private KeyEventHandler keyUpDelegate;
        private IndexEventHandler layerInsertedDelegate;
        private IndexEventHandler layerRemovedDelegate;
        
        private int elementHeight = LayerElement.ThumbSize + 2;
        
        private DocumentWorkspace workspace;
        private Document document;

        private List<LayerElement> layerControls;
        private PanelWithLayout layerControlPanel;

        [Browsable(false)]
        public LayerElement[] Layers
        {
            get
            {
                if (layerControls == null)
                {
                    return new LayerElement[0];
                }
                else
                {
                    return this.layerControls.ToArray();
                }
            }
        }

        public int SelectedLayer
        {
            get
            {
                LayerElement[] layers = this.Layers;
                for (int i = 0; i < layers.Length; ++i)
                {
                    if (layers[i].IsSelected)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }

        public new BorderStyle BorderStyle
        {
            get
            {
                return layerControlPanel.BorderStyle;
            }

            set
            {
                layerControlPanel.BorderStyle = value;
            }
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public LayerControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            elementClickDelegate = new EventHandler(ElementClickHandler);
            elementDoubleClickDelegate = new EventHandler(ElementDoubleClickHandler);
            documentChangedDelegate = new EventHandler(DocumentChangedHandler);
            documentChangingDelegate = new EventHandler(DocumentChangingHandler);
            layerInsertedDelegate = new IndexEventHandler(LayerInsertedHandler);
            layerRemovedDelegate = new IndexEventHandler(LayerRemovedHandler);
            layerChangedDelegate = new EventHandler(LayerChangedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            layerControls = new List<LayerElement>();
        }

        private void SetupNewDocument(Document document)
        {
            // Subscribe to the eevents
            this.document = document;
            this.document.Layers.Inserted += layerInsertedDelegate;
            this.document.Layers.RemovedAt += layerRemovedDelegate;

            SystemLayer.UI.SetControlRedraw(this.layerControlPanel, false);

            for (int i = 0; i < this.document.Layers.Count; ++i)
            {
                this.LayerInsertedHandler(this, new IndexEventArgs(i));
            }

            if (workspace != null)
            {
                foreach (LayerElement lec in layerControls)
                {
                    if (lec.Layer == workspace.ActiveLayer)
                    {
                        lec.IsSelected = true;
                    }
                    else
                    {
                        lec.IsSelected = false;
                    }
                }
            }

            SystemLayer.UI.SetControlRedraw(this.layerControlPanel, true);
            this.layerControlPanel.Invalidate(true);
        }

        private void TearDownOldDocument()
        {
            foreach (LayerElement lec in layerControls)
            {
                lec.Click -= elementClickDelegate;
                lec.DoubleClick -= elementDoubleClickDelegate;
                lec.KeyUp -= keyUpDelegate;
                lec.Layer = null;
                layerControlPanel.Controls.Remove(lec);
                lec.Dispose();
            }

            layerControls.Clear();
            layerControls.TrimExcess();

            // Unsubscribe to the Events
            if (this.Document != null)
            {
                this.document.Layers.Inserted -= layerInsertedDelegate;
                this.document.Layers.RemovedAt -= layerRemovedDelegate;
            }
        }

        private void DocumentChangedHandler(object sender, EventArgs e)
        {
            SetupNewDocument(workspace.Document);
        }

        private void DocumentChangingHandler(object sender, EventArgs e)
        {
            TearDownOldDocument();
        }

        private void LayerRemovedHandler(object sender, IndexEventArgs e)
        {
            LayerElement lec = layerControls[e.Index];
            lec.Click -= this.elementClickDelegate;
            lec.DoubleClick -= this.elementDoubleClickDelegate;
            lec.KeyUp -= keyUpDelegate;
            lec.Layer = null;
            layerControls.Remove(lec);
            layerControlPanel.Controls.Remove(lec);
            lec.Dispose();
            PerformLayout();
        }

        private void InitializeLayerElement(LayerElement lec, Layer l)
        {
            lec.Height = elementHeight;
            lec.Layer = l;
            lec.Click += elementClickDelegate;
            lec.DoubleClick += elementDoubleClickDelegate;
            lec.KeyUp += keyUpDelegate;
            lec.IsSelected = false;
        }

        private void Select(LayerElement lec)
        {
            Select(lec.Layer);
        }

        private void Select(Layer layer)
        {
            foreach (LayerElement lec in layerControls)
            {
                bool select = (lec.Layer == layer);
                lec.IsSelected = select;

                if (select)
                {
                    OnSelectedLayerChanged(lec.Layer);
                    layerControlPanel.ScrollControlIntoView(lec);
                    lec.Select();
                    Update();
                }
            }
        }

        private void LayerInsertedHandler(object sender, IndexEventArgs e)
        {
            this.SuspendLayout();
            this.layerControlPanel.SuspendLayout();
            Layer layer = (Layer)this.document.Layers[e.Index];
            LayerElement lec = new LayerElement();
            InitializeLayerElement(lec, layer);
            layerControls.Insert(e.Index, lec);
            layerControlPanel.Controls.Add(lec);
            layerControlPanel.ScrollControlIntoView(lec);
            lec.Select();
            lec.RefreshPreview();
            this.layerControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.layerControlPanel.PerformLayout();
            PerformLayout();
        }

        public void RefreshPreviews()
        {
            for (int i = 0; i < this.layerControls.Count; ++i)
            {
                this.layerControls[i].RefreshPreview();
            }
        }

        /// <summary>
        /// This event is raised whenever the user clicks on a layer within the
        /// LayerControl to select it.
        /// </summary>
        public event LayerEventHandler ClickedOnLayer;
        private void OnClickedOnLayer(Layer layer)
        {
            if (ClickedOnLayer != null)
            {
                ClickedOnLayer(this, new LayerEventArgs(layer));
            }
        }

        /// <summary>
        /// This event is raised whenever the selected layer is changed. Note that
        /// this can occur without user intervention, which distinguishes this event
        /// from ClickedOnLayer.
        /// </summary>
        public event LayerEventHandler SelectedLayerChanged;
        private void OnSelectedLayerChanged(Layer layer)
        {
            if (SelectedLayerChanged != null)
            {
                SelectedLayerChanged(this, new LayerEventArgs(layer));
            }
        }

        public event LayerEventHandler DoubleClickedOnLayer;
        private void OnDoubleClickedOnLayer(Layer layer)
        {
            if (DoubleClickedOnLayer != null)
            {
                DoubleClickedOnLayer(this, new LayerEventArgs(layer));
            }
        }

        private void ElementClickHandler(object sender, EventArgs e)
        {
            LayerElement lec = (LayerElement) sender;
            Select(lec);
            OnClickedOnLayer(lec.Layer);    
        }

        private void ElementDoubleClickHandler(object sender, EventArgs e)
        {
            OnDoubleClickedOnLayer(((LayerElement)sender).Layer);
        }
    
        private void LayerChangedHandler(object sender, EventArgs e)
        {
            Select(workspace.ActiveLayer);
        }

        public void SuspendLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.SuspendPreviewUpdates();
            }
        }

        public void ResumeLayerPreviewUpdates()
        {
            foreach (LayerElement element in this.layerControls)
            {
                element.ResumePreviewUpdates();
            }
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
    
        [Browsable(false)]
        public DocumentWorkspace Workspace
        {
            get
            {
                return workspace;
            }
            set
            {
                if (workspace != null)
                {
                    workspace.DocumentChanged -= documentChangedDelegate;
                    workspace.DocumentChanging -= documentChangingDelegate;
                    workspace.ActiveLayerChanged -= layerChangedDelegate;
                }

                workspace = value;

                if (workspace != null)
                {
                    workspace.DocumentChanged += documentChangedDelegate;
                    workspace.DocumentChanging += documentChangingDelegate;
                    workspace.ActiveLayerChanged += layerChangedDelegate;
                }
            }
        }

        [Browsable(false)]
        public Document Document
        {
            get
            {
                return this.document;
            }

            set
            {
                if (this.workspace != null)
                {
                    throw new InvalidOperationException("Workspace property is already set");
                }

                if (this.document != null)
                {
                    TearDownOldDocument();
                }

                if (value != null)
                {
                    SetupNewDocument(value);
                }
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
            this.layerControlPanel = new PanelWithLayout();
            this.SuspendLayout();
            // 
            // layerControlPanel
            // 
            this.layerControlPanel.AutoScroll = true;
            this.layerControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControlPanel.Location = new System.Drawing.Point(0, 0);
            this.layerControlPanel.Name = "layerControlPanel";
            this.layerControlPanel.ParentLayerControl = this;
            this.layerControlPanel.Size = new System.Drawing.Size(150, 150);
            this.layerControlPanel.TabIndex = 2;
            // 
            // LayerControl
            // 
            this.Controls.Add(this.layerControlPanel);
            this.Name = "LayerControl";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
