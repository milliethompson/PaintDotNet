using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for BrushConfigWidget.
	/// </summary>
	public class BrushConfigWidget : System.Windows.Forms.UserControl
	{
		private DotNetWidgets.DotNetToolbar brushToolbar;
		private DotNetWidgets.DotNetToolbarLabelItem brushStyleToolbarLabel;
		private DotNetWidgets.DotNetToolbarComboBoxItem styleComboBoxTB;
		private DotNetWidgets.FlatComboBox styleComboBox; // alises to styleComboBoxTB.ContgainedControl
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public BrushInfo BrushInfo
		{
			get
			{
				if (this.styleComboBox.SelectedIndex == 0)
				{
					return new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal);
				}
				if (this.styleComboBox.SelectedIndex == -1)
				{
					return new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal);
				}
				else
				{
					return new BrushInfo(BrushType.Hatch, getHatchStyle(this.styleComboBox.SelectedItem.ToString()));
				}
			}				
		}

		public BrushConfigWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			
			this.styleComboBox = (DotNetWidgets.FlatComboBox)styleComboBoxTB.ContainedControl;
			
			this.styleComboBox.Items.Add("Solid Brush");
			foreach (string styleName in Enum.GetNames(typeof(HatchStyle))) 
			{ 
				String name = Utility.InsertSpaces(styleName);
				styleComboBox.Items.Add(name); 
			}

			styleComboBox.SelectedIndex = 0;	
			
			this.styleComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.styleComboBox.DropDownWidth = 190;
			this.styleComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.comboBoxStyle_MeasureItem);
			this.styleComboBox.SelectedValueChanged += new System.EventHandler(this.comboBoxStyle_SelectedValueChanged);
			this.styleComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxStyle_DrawItem);

		}

		public event EventHandler BrushChanged;
		protected virtual void OnBrushChanged()
		{
			if (BrushChanged != null)
			{
				BrushChanged(this, EventArgs.Empty);
			}
		}

		private HatchStyle getHatchStyle(String s)
		{
			String str = RemoveSpaces(s);
			return (HatchStyle)Enum.Parse(typeof(HatchStyle), str, true);
		}

		private String RemoveSpaces(String str1)
		{
			int start;
			int at;
			int end;

			String str2 = String.Copy(str1);
			
			at = 0;
			end = str2.Length - 1;
			start = 0;
			
			while((start <= end) && (at > -1))
			{
				// start+count must be a position within str2.
				at = str2.IndexOf(" ", start);
				if (at == -1) break;
				str2 = str2.Remove(at,1);
				start = at+1;
			}

			return str2;
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
            this.brushToolbar = new DotNetWidgets.DotNetToolbar();
            this.brushStyleToolbarLabel = new DotNetWidgets.DotNetToolbarLabelItem();
            this.styleComboBoxTB = new DotNetWidgets.DotNetToolbarComboBoxItem();
            this.SuspendLayout();
            // 
            // brushToolbar
            // 
            this.brushToolbar.Buttons.Add(this.brushStyleToolbarLabel);
            this.brushToolbar.Buttons.Add(this.styleComboBoxTB);
            this.brushToolbar.DrawGrabHandle = false;
            this.brushToolbar.DrawSeparator = false;
            this.brushToolbar.ImageList = null;
            this.brushToolbar.Location = new System.Drawing.Point(0, 0);
            this.brushToolbar.MenuProvider = null;
            this.brushToolbar.Name = "brushToolbar";
            this.brushToolbar.Size = new System.Drawing.Size(248, 27);
            this.brushToolbar.TabIndex = 4;
            // 
            // brushStyleToolbarLabel
            // 
            this.brushStyleToolbarLabel.BeginGroup = true;
            this.brushStyleToolbarLabel.Text = "Fill Style: ";
            // 
            // styleComboBoxTB
            // 
            this.styleComboBoxTB.ControlWidth = 121;
            this.styleComboBoxTB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBoxTB.Text = "";
            // 
            // BrushConfigWidget
            // 
            this.Controls.Add(this.brushToolbar);
            this.Name = "BrushConfigWidget";
            this.Size = new System.Drawing.Size(248, 40);
            this.ResumeLayout(false);

        }
		#endregion	

		private void comboBoxStyle_SelectedValueChanged(object sender, System.EventArgs e)
		{
			OnBrushChanged();
		}

        public void PerformBrushChanged()
        {
            OnBrushChanged();
        }

		private void comboBoxStyle_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			// The following method should generally be called before drawing.
			// It is actually superfluous here, since the subsequent drawing
			// will completely cover the area of interest.
			e.DrawBackground();

			//The system provides the context
			//into which the owner custom-draws the required graphics.
			//The context into which to draw is e.graphics.
			//The index of the item to be painted is e.Index.
			//The painting should be done into the area described by e.Bounds.
			Graphics g = e.Graphics;
			Rectangle r = e.Bounds;

			if(e.Index != -1)
			{

				if(e.Index > 0)
				{
					Rectangle rd = r; 
					rd.Width = rd.Left + 25; 
				
					Rectangle rt = r;
					r.X = rd.Right; 

					string displayText = this.styleComboBox.Items[e.Index].ToString();
					HatchStyle hs = this.getHatchStyle(displayText);
					// TODO add user selected foreground and background colors here
					HatchBrush b = new HatchBrush(hs, e.ForeColor, e.BackColor);
					g.FillRectangle(b  , rd);
					StringFormat sf = new StringFormat();
					sf.Alignment = StringAlignment.Near;

					if((e.State & DrawItemState.Focus)==0)
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), r);
						e.Graphics.DrawString(displayText, this.Font, new SolidBrush(SystemColors.WindowText), r, sf);
					}
					else
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), r);
						e.Graphics.DrawString(displayText, this.Font, new SolidBrush(SystemColors.HighlightText), r, sf);
					}
				}
				else
				{

					if((e.State & DrawItemState.Focus)==0)
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
						
						string displayText = this.styleComboBox.Items[e.Index].ToString();
						e.Graphics.DrawString(displayText, this.Font, new SolidBrush(SystemColors.WindowText), e.Bounds);
					}
					else
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
						string displayText = this.styleComboBox.Items[e.Index].ToString();
						e.Graphics.DrawString(displayText, this.Font, new SolidBrush(SystemColors.HighlightText), e.Bounds);
					}

				}

				e.DrawFocusRectangle();

			}

		}

		private void comboBoxStyle_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
		{
			//Work out what the text will be
			string displayText = this.styleComboBox.Items[e.Index].ToString();

			//Get width & height of string
			SizeF stringSize=e.Graphics.MeasureString(displayText, this.Font);

			//Account for top margin
			//stringSize.Height += 6;

			// set hight to text height
			e.ItemHeight = (int)stringSize.Height;	

			// set width to text width
			e.ItemWidth = (int)stringSize.Width;
		}

	}
}
