using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Security;

namespace PaintDotNet
{
	/// <summary>
	/// Used for opening multilayer pictures, namely PDN files. Will show a dialog that allows
	/// user to select/deselect layers of an images.
	/// </summary>
	public class ImportAsNewLayersDialog : PdnBaseForm
	{
		private PaintDotNet.LayerList layerList;
		private Surface renderSurface = null;	
		private string fileName;
		private FileType ft;
		private Document document = null;
		private const int elementHeight = 34;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button allButton;
		private System.Windows.Forms.Button noneButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button cancelButton;
		private PropertyEventHandler layerPropertyChangingDelegate;
		private System.Windows.Forms.Panel layersPanel;
		private System.Windows.Forms.Panel previewPanel;
		private PaintDotNet.LayerControl layerControl;
		private PaintDotNet.SurfaceBox surfaceBox;

		/// <summary>
		/// The list of layers that are being manipulated
		/// </summary>
		public PaintDotNet.LayerList LayerList
		{
			get
			{
				return this.layerList;
			}
		}

		/// <summary>
		/// Get or Set the File Name that this dialog will open
		/// </summary>
		public string FileName
		{
			set
			{
				this.fileName = value;
			}
			get
			{
				return this.fileName;
			}
		}

		/// <summary>
		/// Get or Set the FileType of the fileName
		/// </summary>
		public FileType Ft
		{
			set
			{
				this.ft = value;
			}
			get
			{
				return this.ft;
			}
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Used for opening multilayer pictures, namely PDN files
		/// </summary>
		public ImportAsNewLayersDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			
			layerPropertyChangingDelegate = new PropertyEventHandler(l_PropertyChanged);
			this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuLayersImportFromFileIcon.bmp"));
		}

		private void InitializeLayerElement(LayerElement lec, Layer l)
		{
			lec.Height = elementHeight;
			lec.Width = this.layersPanel.Width;
			lec.Layer = l;			
			lec.Layer.Name = Path.GetFileName(FileName).ToString() + ": " + l.Name;
			lec.IsSelected = false;
			l.PropertyChanged += new PropertyEventHandler(l_PropertyChanged);
			lec.Click += new EventHandler(lec_Click);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);

			if (fileName == null)
			{
				 throw new ArgumentNullException("fileName");
			}

			Stream stream = null;

			try
			{
				stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);

				if (stream != null)
				{
					LoadProgressDialog ld = new LoadProgressDialog(this, stream, ft);

					document = ld.Load();
					if (document == null)
					{
						stream.Close();
						this.DialogResult = DialogResult.Cancel;
						this.Close();
					}
				}
				else
				{
					this.DialogResult = DialogResult.Cancel;
					this.Close();
				}
			}

			catch (ArgumentException)
			{
				if (fileName.Length == 0)
				{
					Utility.ErrorBox(this, "The requested filename is blank.");
				}
				else
				{
					Utility.ErrorBox(this, "There was an error opening the file.");
				}
			}

			catch (UnauthorizedAccessException)
			{
				Utility.ErrorBox(this, "Access was denied to the requested file.");
			}
    
			catch (SecurityException)
			{
				Utility.ErrorBox(this, "Access was denied to the requested file.");
			}

			catch (FileNotFoundException)
			{
				Utility.ErrorBox(this, "The file could not be found.");
			}

			catch (DirectoryNotFoundException)
			{
				Utility.ErrorBox(this, "The directory could not be found.");
			}

			catch (PathTooLongException)
			{
				Utility.ErrorBox(this, "The filename is too long.");
			}

			catch (IOException)
			{
				Utility.ErrorBox(this, "There was an error reading the file.");
			}

			catch(Exception)
			{
				Utility.ErrorBox(this, "There was an unknown error while opening the file.");
			}

			finally
			{
				if (stream != null)
				{
					stream.Close();
					stream = null;
				}
			}

			if (document == null)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			}
			else
			{	
				this.Text = "Import From File: " + Path.GetFileName(FileName);
				Surface newRenderSurface = new Surface(document.Width, document.Height);

				this.renderSurface = newRenderSurface;
				surfaceBox.Surface = newRenderSurface;
				PaintDotNet.RenderArgs renderMe = new RenderArgs(surfaceBox.Surface);
				document.Render(renderMe, renderMe.Bounds);
				renderMe.Dispose();

				// Populate the layer preview stuff
				this.layerList = document.Layers;
				int i = 0;

				foreach(Layer layer in layerList)
				{
					LayerElement lec = new LayerElement();
					InitializeLayerElement(lec, layer);

					this.layerControl.LayerControls.Insert(i++,lec);
					this.layerControl.PerformLayout();

					this.layerControl.Controls.Add(lec);
					this.layersPanel.Controls.Add(lec);

					layer.Invalidate();
					lec.Invalidate();
				}

				this.layersPanel.Controls.Add(this.layerControl);				
				
				// Fit the document to the window
				this.surfaceBox.FitToSize(this.previewPanel.Size);

				// Get the buttons figured out
				this.okButton.Enabled = AnyChecked();

				foreach (Control c in layerControl.LayerControls)
				{
					c.Width = layersPanel.ClientRectangle.Width;
				}

				//Invalidate(true);
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.previewPanel = new System.Windows.Forms.Panel();
			this.surfaceBox = new PaintDotNet.SurfaceBox();
			this.layersPanel = new System.Windows.Forms.Panel();
			this.layerControl = new PaintDotNet.LayerControl();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.allButton = new System.Windows.Forms.Button();
			this.noneButton = new System.Windows.Forms.Button();
			this.previewPanel.SuspendLayout();
			this.layersPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(410, 282);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(490, 282);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// previewPanel
			// 
			this.previewPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.previewPanel.Controls.Add(this.surfaceBox);
			this.previewPanel.Location = new System.Drawing.Point(216, 24);
			this.previewPanel.Name = "previewPanel";
			this.previewPanel.Size = new System.Drawing.Size(347, 248);
			this.previewPanel.TabIndex = 8;
			// 
			// surfaceBox
			// 
			this.surfaceBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.surfaceBox.DrawGrid = false;
			this.surfaceBox.Location = new System.Drawing.Point(0, 0);
			this.surfaceBox.Name = "surfaceBox";
			this.surfaceBox.Size = new System.Drawing.Size(343, 244);
			this.surfaceBox.Surface = null;
			this.surfaceBox.TabIndex = 9;
			this.surfaceBox.Text = "surfaceBox";
			// 
			// layersPanel
			// 
			this.layersPanel.AutoScroll = true;
			this.layersPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.layersPanel.Controls.Add(this.layerControl);
			this.layersPanel.Location = new System.Drawing.Point(8, 24);
			this.layersPanel.Name = "layersPanel";
			this.layersPanel.Size = new System.Drawing.Size(200, 248);
			this.layersPanel.TabIndex = 7;
			// 
			// layerControl
			// 
			this.layerControl.AutoScroll = true;
			this.layerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.layerControl.Location = new System.Drawing.Point(0, 0);
			this.layerControl.Name = "layerControl";
			this.layerControl.Size = new System.Drawing.Size(196, 244);
			this.layerControl.TabIndex = 0;
			this.layerControl.Workspace = null;
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(216, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(168, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "Preview of Imported Image";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 8);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(184, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Layers to Insert";
			// 
			// allButton
			// 
			this.allButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.allButton.Location = new System.Drawing.Point(8, 282);
			this.allButton.Name = "allButton";
			this.allButton.TabIndex = 3;
			this.allButton.Text = "&All";
			this.allButton.Click += new System.EventHandler(this.allButton_Click);
			// 
			// noneButton
			// 
			this.noneButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.noneButton.Location = new System.Drawing.Point(88, 282);
			this.noneButton.Name = "noneButton";
			this.noneButton.TabIndex = 4;
			this.noneButton.Text = "&None";
			this.noneButton.Click += new System.EventHandler(this.noneButton_Click);
			// 
			// ImportAsNewLayersDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(570, 311);
			this.Controls.Add(this.noneButton);
			this.Controls.Add(this.allButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.layersPanel);
			this.Controls.Add(this.previewPanel);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "ImportAsNewLayersDialog";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Import";
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.previewPanel, 0);
			this.Controls.SetChildIndex(this.layersPanel, 0);
			this.Controls.SetChildIndex(this.label1, 0);
			this.Controls.SetChildIndex(this.label2, 0);
			this.Controls.SetChildIndex(this.allButton, 0);
			this.Controls.SetChildIndex(this.noneButton, 0);
			this.previewPanel.ResumeLayout(false);
			this.layersPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// Sets the result to OK and closes the dialog
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void okButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			Utility.Dispose(this.surfaceBox);
			this.Close();      
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			Utility.Dispose(this.surfaceBox);
		}

		/// <summary>
		/// Enables/disables the all and none buttons
		/// </summary>
		/// <returns>True if any layers are checked</returns>
		private bool AnyChecked()
		{
			int n = 0;
			foreach(LayerElement layerElement in this.layerControl.LayerControls)
			{
				if(layerElement.LayerVisible.Checked == true)
					++n;
			}	

			if(n == this.layerControl.LayerControls.Count) // All of them are checked, disable all button, return true
			{
				this.allButton.Enabled = false;
				this.noneButton.Enabled = true;
				return true;
			}
			else if(n > 0) // If any are checked, all buttons enabled. Yah!
			{
				this.allButton.Enabled = true;
				this.noneButton.Enabled = true;
				return true;
			}
			else // None are selected
			{
				this.noneButton.Enabled = false;
				return false;
			}
		}

		/// <summary>
		/// Whenever a layer element is changed, re-render the preview and update the buttons
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void l_PropertyChanged(object sender, PropertyEventArgs e)
		{
			PaintDotNet.RenderArgs renderMe = new RenderArgs(surfaceBox.Surface);
			document.Render(renderMe,new Rectangle(0,0,document.Width,document.Height));
			renderMe.Dispose();

			if(AnyChecked() == true)
				this.okButton.Enabled = true;
			else
				this.okButton.Enabled = false;

			Invalidate(true);
		}

		/// <summary>
		/// On a click to a layer element, will give focus to the panel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void lec_Click(object sender, EventArgs e)
		{
			this.layersPanel.Focus();
		}

		/// <summary>
		/// Selects all the layers, updates the buttons and the preview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void allButton_Click(object sender, System.EventArgs e)
		{
			foreach(LayerElement layerElement in this.layerControl.LayerControls)
			{
				layerElement.Layer.PropertyChanged -= layerPropertyChangingDelegate;
				layerElement.LayerVisible.Checked = true;
				layerElement.Layer.PropertyChanged += layerPropertyChangingDelegate;
			}
			PaintDotNet.RenderArgs renderMe = new RenderArgs(surfaceBox.Surface);
			document.Render(renderMe,new Rectangle(0,0,document.Width,document.Height));
			renderMe.Dispose();

			this.allButton.Enabled = false;
			this.okButton.Enabled = true;
			this.noneButton.Enabled = true;
			Invalidate(true);
		}

		/// <summary>
		/// Deselects all the layers, updates the buttons and the preview
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void noneButton_Click(object sender, System.EventArgs e)
		{
			foreach(LayerElement layerElement in this.layerControl.LayerControls)
			{
				layerElement.Layer.PropertyChanged -= layerPropertyChangingDelegate;
				layerElement.LayerVisible.Checked = false;
				layerElement.Layer.PropertyChanged += layerPropertyChangingDelegate;
			}
			PaintDotNet.RenderArgs renderMe = new RenderArgs(surfaceBox.Surface);
			document.Render(renderMe,new Rectangle(0,0,document.Width,document.Height));
			renderMe.Dispose();

			this.noneButton.Enabled = false;
			this.okButton.Enabled = false;
			this.allButton.Enabled = true;
			Invalidate(true);
		}
	}
}
