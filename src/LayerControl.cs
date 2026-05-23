using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for LayerControl.
	/// </summary>
	public class LayerControl : System.Windows.Forms.UserControl
	{
		private PanelEx layerControlPanel;
	
		private EventHandler elementClickDelegate;
        private EventHandler elementDoubleClickDelegate;
		private EventHandler documentChangedDelegate;
		private EventHandler documentChangingDelegate;
        private EventHandler layerChangedDelegate;
        private KeyEventHandler keyUpDelegate;
        private IndexEventHandler layerInsertedDelegate;
        private IndexEventHandler layerRemovedDelegate;
		
		private const int elementHeight = 32;
		
		private DocumentWorkspace workspace; 
		private ArrayList layerControls;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public LayerControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//currentLEC = null;
			elementClickDelegate = new EventHandler(ElementClickHandler);
            elementDoubleClickDelegate = new EventHandler(ElementDoubleClickHandler);
			documentChangedDelegate = new EventHandler(DocumentChangedHandler);
			documentChangingDelegate = new EventHandler(DocumentChangingHandler);
            layerInsertedDelegate = new IndexEventHandler(LayerInsertedHandler);
            layerRemovedDelegate = new IndexEventHandler(LayerRemovedHandler);
            layerChangedDelegate = new EventHandler(LayerChangedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            layerControls = new ArrayList();

		}

		private void DocumentChangedHandler(object sender, EventArgs e)
		{
			// Subscribe the Events
			workspace.Document.Layers.Inserted += layerInsertedDelegate;
			workspace.Document.Layers.RemovedAt += layerRemovedDelegate;
			
            layerControlPanel.SuspendLayout();

            for (int i = 0; i < workspace.Document.Layers.Count; ++i)
            {
                this.LayerInsertedHandler(this, new IndexEventArgs(i));
            }

            foreach (LayerElementControl lec in layerControls)
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

            layerControlPanel.ResumeLayout(true);
            PerformLayout();
        }

		private void DocumentChangingHandler(object sender, EventArgs e)
		{
            foreach (LayerElementControl lec in layerControls)
            {
                lec.Click -= elementClickDelegate;
                lec.DoubleClick -= elementDoubleClickDelegate;
                lec.KeyUp -= keyUpDelegate;
                lec.Layer = null;
                layerControlPanel.Controls.Remove(lec);
                lec.Dispose();
            }

            layerControls.Clear();
            layerControls.TrimToSize();

            // Unsubscribe to the Events
			if (workspace.Document != null)
			{
				workspace.Document.Layers.Inserted -= layerInsertedDelegate;
                workspace.Document.Layers.RemovedAt -= layerRemovedDelegate;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout (levent);

			if (layerControlPanel != null)
			{
                for (int i = 0; i < layerControls.Count; ++i)
                {
                    LayerElementControl lec = (LayerElementControl)layerControls[i];
                    lec.Width = layerControlPanel.ClientRectangle.Width;
                    lec.Location = new Point(layerControlPanel.AutoScrollPosition.X, layerControlPanel.AutoScrollPosition.Y + (elementHeight * i));
                }
			}
		}

		private void LayerRemovedHandler(object sender, IndexEventArgs e)
		{
            LayerElementControl lec = (LayerElementControl)layerControls[e.Index];
            lec.Click -= this.elementClickDelegate;
            lec.DoubleClick -= this.elementDoubleClickDelegate;
            lec.KeyUp -= keyUpDelegate;
            lec.Layer = null;
            layerControls.Remove(lec);
			layerControlPanel.Controls.Remove(lec);
            lec.Dispose();
            PerformLayout();
		}

		private void InitializeLayerElementControl(LayerElementControl lec, Layer l)
		{
			lec.Height = elementHeight;
			lec.Width = layerControlPanel.ClientRectangle.Width;
			lec.Layer = l;
			lec.Click += elementClickDelegate;
            lec.DoubleClick += elementDoubleClickDelegate;
            lec.KeyUp += keyUpDelegate;
            lec.IsSelected = false;
		}

		private void Select(LayerElementControl lec)
		{
            Select(lec.Layer);
		}

        private void Select(Layer layer)
        {
            foreach(LayerElementControl lec in layerControls)
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
			Layer layer = (Layer)workspace.Document.Layers[e.Index];
			LayerElementControl lec = new LayerElementControl();
			InitializeLayerElementControl(lec, layer);
            layerControls.Insert(e.Index, lec);
            PerformLayout();
            layerControlPanel.Controls.Add(lec);
            PerformLayout();
            layerControlPanel.ScrollControlIntoView(lec);
            lec.Select();
        }

        /// <summary>
        /// This event is raised whenever the user clicks on a layer within the
        /// LayerControl.
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
			LayerElementControl lec = (LayerElementControl) sender;
			Select(lec);
			OnClickedOnLayer(lec.Layer);	
		}

        private void ElementDoubleClickHandler(object sender, EventArgs e)
        {
            OnDoubleClickedOnLayer(((LayerElementControl)sender).Layer);
        }
    
        private void LayerChangedHandler(object sender, EventArgs e)
        {
            Select(workspace.ActiveLayer);
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            this.OnKeyUp(e);
        }
	
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
            this.layerControlPanel = new PanelEx();
            this.SuspendLayout();
            // 
            // layerControlPanel
            // 
            this.layerControlPanel.AutoScroll = true;
            this.layerControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layerControlPanel.Location = new System.Drawing.Point(0, 0);
            this.layerControlPanel.Name = "layerControlPanel";
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
