using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for TextConfigWidget.
	/// </summary>
	public class TextConfigWidget : System.Windows.Forms.UserControl
	{
		private FontFamily[] families;
		private FontFamily arial;
		private FontStyle style;
		private TextAlignment alignment;
		private ImageList imageList;
		private float oldSizeValue;
		private System.Windows.Forms.ErrorProvider sizeErrorProvider;
		private DotNetWidgets.DotNetToolbar fontToolbar;
		private DotNetWidgets.DotNetToolbarButtonItem boldButton;
		private DotNetWidgets.DotNetToolbarButtonItem italicButton;
		private DotNetWidgets.DotNetToolbarButtonItem underlineButton;
		private DotNetWidgets.DotNetToolbarButtonItem alignCenterButton;
		private DotNetWidgets.DotNetToolbarButtonItem alignLeftButton;
		private DotNetWidgets.DotNetToolbarButtonItem alignRightButton;
        private DotNetWidgets.DotNetToolbarComboBoxItem sizeComboBoxTB;
        private DotNetWidgets.FlatComboBox sizeComboBox; // this aliases to sizeComboBoxTB.ContainedControl
        private DotNetWidgets.FlatComboBox fontComboBox; // this aliases to fontComboBoxTB.ContainedControl
        private DotNetWidgets.DotNetToolbarComboBoxItem fontComboBoxTB;
        private DotNetWidgets.DotNetToolbarLabelItem dotNetToolbarLabelItem1;
        private DotNetWidgets.DotNetToolbarIconButtonItem placeHolderIconButton;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TextConfigWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

			#region initializaion of fontComboBox
            this.fontComboBox = (DotNetWidgets.FlatComboBox)fontComboBoxTB.ContainedControl;
            this.fontComboBox.AllowDrop = true;
            this.fontComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.fontComboBox.DropDownWidth = 240;
            this.fontComboBox.InitialText = "";
            this.fontComboBox.Location = new System.Drawing.Point(32, 5);
            this.fontComboBox.MaxDropDownItems = 12;
            this.fontComboBox.Name = "fontComboBox";
            this.fontComboBox.Size = new System.Drawing.Size(112, 21);
            this.fontComboBox.TabIndex = 0;
            this.fontComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.fontComboBox_MeasureItem);
            this.fontComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.fontComboBox_DrawItem);
			
			// Get an array of the available font families.
			FontStyle fontStyle = new FontStyle();
			families = FontFamily.GetFamilies(this.CreateGraphics());
			int i = 0;
			foreach (FontFamily family in families)
			{
				try
				{
					this.fontComboBox.Items.Add(family.Name);

					if(family.Name == "Arial")
					{
						this.fontComboBox.SelectedItem = families[i].Name;
						arial = families[i];
					}
					i++;
				}
				catch(NullReferenceException)
				{
					//Debug.WriteLine("Adding a font family name to the font text box caused a null reference exception");
				}
			}

			#endregion

			#region sizeComboBoxTB
            this.sizeComboBox = (DotNetWidgets.FlatComboBox)sizeComboBoxTB.ContainedControl;
            this.sizeComboBox.TextChanged += new EventHandler(sizeComboBox_TextChanged);
            this.sizeComboBox.Validating += new CancelEventHandler(sizeComboBox_Validating);
			// defaults
			//sizeComboBoxTB.SelectedText = "10";
			sizeComboBoxTB.Text = "10";
			#endregion

			#region sizeErrorProvider
			// Create and set the ErrorProvider for textBoxSize data entry control.
			sizeErrorProvider = new  System.Windows.Forms.ErrorProvider();
			sizeErrorProvider.SetIconAlignment (this.sizeComboBox, ErrorIconAlignment.MiddleRight);
			sizeErrorProvider.SetIconPadding (this.sizeComboBox, 2);
			sizeErrorProvider.BlinkRate = 1000;
			sizeErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
			#endregion
	
			#region Create ImageList
			// Create ImageList
			imageList = new ImageList();
			imageList.ImageSize = new Size(16, 16);
			imageList.TransparentColor = Color.FromArgb(192, 192, 192);

			this.fontToolbar.ImageList = imageList;

			int boldIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontBoldIcon.bmp"), imageList.TransparentColor);
			int italicIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontItalicIcon.bmp"), imageList.TransparentColor);
			int underlineIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontUnderlineIcon.bmp"), imageList.TransparentColor);
			int alignLeftIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignLeftIcon.bmp"), imageList.TransparentColor);
			int alignCenterIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignCenterIcon.bmp"), imageList.TransparentColor);
			int alignRightIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignRightIcon.bmp"), imageList.TransparentColor);

			boldButton.ImageIndex = boldIndex;
			italicButton.ImageIndex = italicIndex;
			underlineButton.ImageIndex = underlineIndex;
			alignLeftButton.ImageIndex = alignLeftIndex;
			alignCenterButton.ImageIndex = alignCenterIndex;
			alignRightButton.ImageIndex = alignRightIndex;
			#endregion

			alignment = TextAlignment.Left;
			alignLeftButton.Pushed = true;
			oldSizeValue = 2;
            
		}

		public event EventHandler FontTextChanged;
		protected virtual void OnFontTextChanged()
		{
			if (FontTextChanged != null)
			{
				FontTextChanged(this, EventArgs.Empty);
			}
		}

		public event EventHandler TextAlignmentChanged;
		protected virtual void OnTextAlignmentChanged()
		{
			if (TextAlignmentChanged != null)
			{
				TextAlignmentChanged(this, EventArgs.Empty);
			}
		}

		public FontInfo FontInfo
		{
			get
			{
				return new FontInfo(this.FontFamily, this.FontSize, this.FontStyle);
			}	
			set
			{
				if(value != this.FontInfo)
				{
					this.FontFamily = value.FontFamily;
					this.FontSize = value.Size;
					this.FontStyle = value.FontStyle;
				}
			}
		}

		/// <summary>
		/// Gets or sets the font style i.e. bold, italic, and underline
		/// </summary>
		public FontStyle FontStyle
		{
			get 
			{
				return style;
			}
			set
			{
				if (style != value)
				{
					style = value;
					this.OnFontTextChanged();

				}
			}
		}

		/// <summary>
		///  Gets or sets the size of the font.
		/// </summary>
		public float FontSize
		{
			get
			{
				
				bool invalid = false;
				float number = oldSizeValue;

				try
				{
					number = float.Parse(sizeComboBoxTB.Text);
				}

				catch (FormatException)
				{
					invalid = true;
				}

				catch (OverflowException)
				{
					invalid = true;
				}

				// if the size is valid update the new size else return the last known good size.
				if (!invalid)
				{
					oldSizeValue = number;
				}
					
				return oldSizeValue;
					
			}
			set
			{
				/**/
				bool invalid = false;
				float number = oldSizeValue;

				try
				{
					number = float.Parse(sizeComboBoxTB.Text);
				}

				catch (FormatException)
				{
					invalid = true;
				}

				catch (OverflowException)
				{
					invalid = true;
				}

				// if the size is valid update the new size else return the last known good size.
				if (!invalid)
				{
					if(float.Parse(sizeComboBoxTB.Text) != value)
					{
						sizeComboBoxTB.Text = value.ToString();
						this.OnFontTextChanged();
					}
				}
				// else the value is invalid and don't set
				/**/
				/*	commented out.  caused bug.  did not validate size. * /
				if(float.Parse(sizeComboBoxTB.Text) != value)
				{
					sizeComboBoxTB.Text = value.ToString();
					this.OnFontTextChanged();
				}
				/**/
			}
		}
	
		/// <summary>
		/// Gets or sets the font family.
		/// </summary>
		public FontFamily FontFamily
		{
			get
			{
				return families[this.fontComboBox.SelectedIndex];
			}
			set
			{
				FontFamily ff = families[this.fontComboBox.SelectedIndex];
				if (ff.Name != value.Name)
				{
					int index = 0;
					int savedIndex = 0;
					bool found = false;

					foreach (FontFamily family in families)
					{
						if(family.Name == value.Name)	
						{
							found = true;
							savedIndex = index;
						}
						index++;
					}

					if(found)
					{
						this.fontComboBox.SelectedIndex = savedIndex;
						this.OnFontTextChanged();
					}
					else
					{
						throw new InvalidOperationException("Font Family is invalid");
					}
				}
				
			}
		}

		public TextAlignment TextAlignment
		{
			get 
			{
				return alignment;
			}
			set
			{
				if (alignment != value)
				{
					alignment = value;
				
					// if the user sets the text alignment the buttons must be updated
					if(alignment == TextAlignment.Left)
					{
						this.alignLeftButton.Pushed = true;
						this.alignCenterButton.Pushed = false;
						this.alignRightButton.Pushed = false;
					}
					else if(alignment == TextAlignment.Center)
					{
						this.alignLeftButton.Pushed = false;
						this.alignCenterButton.Pushed = true;
						this.alignRightButton.Pushed = false;
					}
					else if(alignment == TextAlignment.Right)
					{
						this.alignLeftButton.Pushed = false;
						this.alignCenterButton.Pushed = false;
						this.alignRightButton.Pushed = true;
					}
					else
					{
						throw new InvalidOperationException("Text alignment type is invalid");
					}

					this.OnTextAlignmentChanged();
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
            this.fontToolbar = new DotNetWidgets.DotNetToolbar();
            this.dotNetToolbarLabelItem1 = new DotNetWidgets.DotNetToolbarLabelItem();
            this.fontComboBoxTB = new DotNetWidgets.DotNetToolbarComboBoxItem();
            this.sizeComboBoxTB = new DotNetWidgets.DotNetToolbarComboBoxItem();
            this.placeHolderIconButton = new DotNetWidgets.DotNetToolbarIconButtonItem();
            this.boldButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.italicButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.underlineButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignLeftButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignCenterButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignRightButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.SuspendLayout();
            // 
            // fontToolbar
            // 
            this.fontToolbar.Buttons.Add(this.dotNetToolbarLabelItem1);
            this.fontToolbar.Buttons.Add(this.fontComboBoxTB);
            this.fontToolbar.Buttons.Add(this.sizeComboBoxTB);
            this.fontToolbar.Buttons.Add(this.placeHolderIconButton);
            this.fontToolbar.Buttons.Add(this.boldButton);
            this.fontToolbar.Buttons.Add(this.italicButton);
            this.fontToolbar.Buttons.Add(this.underlineButton);
            this.fontToolbar.Buttons.Add(this.alignLeftButton);
            this.fontToolbar.Buttons.Add(this.alignCenterButton);
            this.fontToolbar.Buttons.Add(this.alignRightButton);
            this.fontToolbar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fontToolbar.DrawGrabHandle = false;
            this.fontToolbar.ImageList = null;
            this.fontToolbar.Location = new System.Drawing.Point(0, 0);
            this.fontToolbar.MenuProvider = null;
            this.fontToolbar.Name = "fontToolbar";
            this.fontToolbar.Size = new System.Drawing.Size(456, 27);
            this.fontToolbar.TabIndex = 3;
            this.fontToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.fontToolbar_ButtonClick);
            // 
            // dotNetToolbarLabelItem1
            // 
            this.dotNetToolbarLabelItem1.BeginGroup = true;
            this.dotNetToolbarLabelItem1.Text = "Font:";
            // 
            // fontComboBoxTB
            // 
            this.fontComboBoxTB.ControlWidth = 112;
            this.fontComboBoxTB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontComboBoxTB.Text = "";
            this.fontComboBoxTB.SelectedIndexChanged += new DotNetWidgets.DotNetToolbarComboBoxItem.SelectedIndexChangedEventHandler(this.fontComboBoxTB_SelectedIndexChanged);
            // 
            // sizeComboBoxTB
            // 
            this.sizeComboBoxTB.ControlWidth = 48;
            this.sizeComboBoxTB.Items.AddRange(new object[] {
                                                                "8",
                                                                "9",
                                                                "10",
                                                                "11",
                                                                "12",
                                                                "14",
                                                                "16",
                                                                "18",
                                                                "20",
                                                                "22",
                                                                "24",
                                                                "26",
                                                                "28",
                                                                "36",
                                                                "48",
                                                                "72"});
            this.sizeComboBoxTB.Text = "";
            // 
            // placeHolderIconButton
            // 
            this.placeHolderIconButton.Enabled = false;
            this.placeHolderIconButton.Icon = null;
            this.placeHolderIconButton.IdealSize = new System.Drawing.Size(16, 16);
            // 
            // boldButton
            // 
            this.boldButton.BeginGroup = true;
            this.boldButton.ToolTipText = "Bold Text";
            // 
            // italicButton
            // 
            this.italicButton.ToolTipText = "Italics Text";
            // 
            // underlineButton
            // 
            this.underlineButton.ToolTipText = "Underline Text";
            // 
            // alignLeftButton
            // 
            this.alignLeftButton.BeginGroup = true;
            this.alignLeftButton.ToolTipText = "Align Text Left";
            // 
            // alignCenterButton
            // 
            this.alignCenterButton.ToolTipText = "Center Text";
            // 
            // alignRightButton
            // 
            this.alignRightButton.ToolTipText = "Align Text Right";
            // 
            // TextConfigWidget
            // 
            this.Controls.Add(this.fontToolbar);
            this.Name = "TextConfigWidget";
            this.Size = new System.Drawing.Size(456, 32);
            this.ResumeLayout(false);

        }
		#endregion

		private void fontComboBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
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
			if(e.Index != -1)
			{
				string displayText = families[e.Index].Name;

				SizeF stringSize = e.Graphics.MeasureString(displayText, new FontInfo(arial, 10, FontStyle.Regular).CreateFont());
				int size = (int)stringSize.Width;
				
				// set up two areas to draw
				Rectangle r = e.Bounds;

				Rectangle rd = r; 
				rd.Width = rd.Left + size;
        
				Rectangle rt = r;
				r.X = rd.Right;

				Font myFont = new FontInfo(families[e.Index], 10, FontStyle.Regular).CreateFont();
				NativeMethods.LOGFONT logFont = new NativeMethods.LOGFONT();
				myFont.ToLogFont(logFont);	// logFont is populated.

				// DEBUG print
				//Debug.WriteLine("DEBUG: Name: " + displayText + " Charset: " + logFont.lfCharSet.ToString() + " Pitch&family: " + logFont.lfPitchAndFamily.ToString());
				// end debug
				if((e.State & DrawItemState.Selected)==0)
				{
					if (logFont.lfCharSet == NativeMethods.GdiCharSets.SYMBOL_CHARSET)
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
						e.Graphics.DrawString(displayText, new FontInfo(arial, 10, FontStyle.Regular).CreateFont(),new SolidBrush(SystemColors.WindowText), e.Bounds);
						e.Graphics.DrawString(displayText, myFont, new SolidBrush(SystemColors.WindowText), r);
					}
					else
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Window), e.Bounds);
						e.Graphics.DrawString(displayText, myFont, new SolidBrush(SystemColors.WindowText), e.Bounds);
					}
				}
				else
				{
					if (logFont.lfCharSet == NativeMethods.GdiCharSets.SYMBOL_CHARSET)
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
						e.Graphics.DrawString(displayText, new FontInfo(arial, 10, FontStyle.Regular).CreateFont(),new SolidBrush(SystemColors.HighlightText), e.Bounds);
						e.Graphics.DrawString(displayText, myFont, new SolidBrush(SystemColors.HighlightText), r);
					}
					else
					{
						e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), e.Bounds);
						e.Graphics.DrawString(displayText, myFont, new SolidBrush(SystemColors.HighlightText), e.Bounds);
					}
				}

			}
				e.DrawFocusRectangle();	
		}

		private void fontComboBox_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
		{
			//Work out what the text will be
			string displayText = families[e.Index].Name;

			//Get width & height of string
			SizeF stringSize=e.Graphics.MeasureString(displayText, new FontInfo(families[e.Index], 10, FontStyle.Regular).CreateFont());

			//Account for top margin
			stringSize.Height += 6;

			// set hight to text height
			e.ItemHeight = (int)stringSize.Height;	

			if(e.ItemHeight > 20)
				e.ItemHeight = 20;

			// set width to text width
			e.ItemWidth = (int)stringSize.Width;
	
		}
		
		private void fontToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
		{
			if (e.Button == boldButton)
			{
				style ^= FontStyle.Bold;
				
				if(this.boldButton.Pushed)
					this.boldButton.Pushed = false;
				else
					this.boldButton.Pushed = true;
				this.OnFontTextChanged();
			}
			else if (e.Button == italicButton)
			{
				style ^= FontStyle.Italic;

				if(this.italicButton.Pushed)
					this.italicButton.Pushed = false;
				else
					this.italicButton.Pushed = true;
				this.OnFontTextChanged();
			}
			else if (e.Button == underlineButton)
			{
				style ^= FontStyle.Underline;

				if(this.underlineButton.Pushed)
					this.underlineButton.Pushed = false;
				else
					this.underlineButton.Pushed = true;
				this.OnFontTextChanged();
			}
			else if (e.Button == alignLeftButton)
			{
				this.TextAlignment = TextAlignment.Left;
			}
			else if (e.Button == alignCenterButton)
			{
				this.TextAlignment = TextAlignment.Center;
			}
			else if (e.Button == alignRightButton)
			{
				this.TextAlignment = TextAlignment.Right;
			}
		}

		private void sizeComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				bool invalid = false;

				try
				{
					float number = float.Parse(sizeComboBoxTB.Text);
				}

				catch (FormatException)
				{
					invalid = true;
				}

				catch (OverflowException)
				{
					invalid = true;
				}

				if (invalid)
				{
					sizeErrorProvider.SetError(this.sizeComboBox, "Invalid number");
				}
				else
				{
					if ((float.Parse(sizeComboBoxTB.Text) < 1))
					{
						// Set the error if the size is too small.
						sizeErrorProvider.SetError(this.sizeComboBox, "Size is smaller than 1");
					}
					else if ((float.Parse(sizeComboBoxTB.Text) > 100 ))
					{
						// Set the error if the size is too large.
						sizeErrorProvider.SetError(this.sizeComboBox, "Size is larger than 100");
					}
					else 
					{
						// Clear the error, if any, in the error provider.
						sizeErrorProvider.SetError(this.sizeComboBox, "");
						OnFontTextChanged();
					}
				}
			}
			catch (FormatException)
			{
				e.Cancel = true;
			}
		}

		private void sizeComboBox_TextChanged(object sender, System.EventArgs e)
		{
			this.Validate();
		}

		private void fontComboBoxTB_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			OnFontTextChanged();
		}
	}
}
