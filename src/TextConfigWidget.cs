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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for TextConfigWidget.
    /// </summary>
    public class TextConfigWidget : System.Windows.Forms.UserControl
    {
        private const int maxFontSize = 1000;
        private const int minFontSize = 1;

        private FontFamily arialFontFamily;
        private FontStyle style;
        private TextAlignment alignment;
        private ImageList imageList;
        private float oldSizeValue;
        private DotNetWidgets.DotNetToolbar fontToolbar;
        private DotNetWidgets.DotNetToolbarButtonItem boldButton;
        private DotNetWidgets.DotNetToolbarButtonItem italicButton;
        private DotNetWidgets.DotNetToolbarButtonItem underlineButton;
        private DotNetWidgets.DotNetToolbarButtonItem alignCenterButton;
        private DotNetWidgets.DotNetToolbarButtonItem alignLeftButton;
        private DotNetWidgets.DotNetToolbarButtonItem alignRightButton;
        private System.Windows.Forms.ComboBox fontComboBox;
        private System.Windows.Forms.ComboBox sizeComboBox;
        private DotNetWidgets.DotNetToolbar dotNetToolbar;
        private DotNetWidgets.DotNetToolbarLabelItem lblFont;
        private System.Windows.Forms.ToolTip tooltipProvider;
        private System.ComponentModel.IContainer components;
        private Brush highlightBrush;
        private Brush highlightTextBrush;
        private Brush windowBrush;
        private Brush windowTextBrush;
        private Font arialFont10;
        private const string arialName = "Arial";

        public TextConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.

            InitializeComponent();

            this.SuspendLayout();
            this.arialFontFamily = new FontFamily(arialName);
            this.arialFont10 = new Font(arialFontFamily, 10, FontStyle.Regular);

            this.fontComboBox.AllowDrop = true;
            this.fontComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.fontComboBox.DropDownWidth = 240;
            this.fontComboBox.MaxDropDownItems = 12;
            this.fontComboBox.Name = "fontComboBox";
            this.fontComboBox.TabIndex = 0;
            this.fontComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.fontComboBox_MeasureItem);
            this.fontComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.fontComboBox_DrawItem);
            this.fontComboBox.Sorted = true;
            this.fontComboBox.DropDown += new EventHandler(fontComboBox_DropDown);
            this.fontComboBox.Items.Add(arialName);
            this.fontComboBox.SelectedItem = arialName;

            this.sizeComboBox.TextChanged += new EventHandler(sizeComboBox_TextChanged);
            this.sizeComboBox.Validating += new CancelEventHandler(sizeComboBox_Validating);
            this.sizeComboBox.Text = "10";

            alignment = TextAlignment.Left;
            alignLeftButton.Pushed = true;
            oldSizeValue = 2;

            this.ResumeLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);

            this.highlightBrush = new SolidBrush(SystemColors.Highlight);
            this.highlightTextBrush = new SolidBrush(SystemColors.HighlightText);
            this.windowBrush = new SolidBrush(SystemColors.Window);
            this.windowTextBrush = new SolidBrush(SystemColors.WindowText);

            this.imageList = new ImageList();
            this.imageList.ImageSize = new Size(16, 16);
            this.imageList.TransparentColor = Color.FromArgb(192, 192, 192);

            this.fontToolbar.ImageList = imageList;

            int boldIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontBoldIcon.bmp"), imageList.TransparentColor);
            int italicIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontItalicIcon.bmp"), imageList.TransparentColor);
            int underlineIndex = imageList.Images.Add(Utility.GetImageResource("Icons.FontUnderlineIcon.bmp"), imageList.TransparentColor);
            int alignLeftIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignLeftIcon.bmp"), imageList.TransparentColor);
            int alignCenterIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignCenterIcon.bmp"), imageList.TransparentColor);
            int alignRightIndex = imageList.Images.Add(Utility.GetImageResource("Icons.TextAlignRightIcon.bmp"), imageList.TransparentColor);

            this.boldButton.ImageIndex = boldIndex;
            this.italicButton.ImageIndex = italicIndex;
            this.underlineButton.ImageIndex = underlineIndex;
            this.alignLeftButton.ImageIndex = alignLeftIndex;
            this.alignCenterButton.ImageIndex = alignCenterIndex;
            this.alignRightButton.ImageIndex = alignRightIndex;        
        }

        private void PopulateFonts()
        {
            using (Graphics g = this.CreateGraphics())
            {
                using (new WaitCursorChanger(this))
                {
                    FontFamily[] families = FontFamily.GetFamilies(g);

                    foreach (FontFamily family in families)
                    {
                        if (fontComboBox.Items.Contains(family.Name))
                        {
                            continue;
                        }

                        using (FontInfo fi = new FontInfo(family, 10, FontStyle.Regular))
                        {
                            if (fi.CanCreateFont())
                            {
                                fontComboBox.Items.Add(family.Name);
                            }
                        }
                    }
                }

                // HACK: If you move the mouse while we have that waitcursor in place,
                //       then the cursor STAYS as a wait cursor
                if (this.ParentForm != null)
                {
                    this.ParentForm.Cursor = Cursors.Default;
                }
            }
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
                if (value != this.FontInfo)
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
                    number = float.Parse(sizeComboBox.Text);
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
                bool invalid = false;
                float number = oldSizeValue;

                try
                {
                    number = float.Parse(sizeComboBox.Text);
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
                    if (float.Parse(sizeComboBox.Text) != value)
                    {
                        sizeComboBox.Text = value.ToString();
                        this.OnFontTextChanged();
                    }
                }
            }
        }

        private FontFamily IndexToFontFamily(int i)
        {
            return new FontFamily((string)this.fontComboBox.Items[i]);
        }

        private Font IndexToFont(int i, float size, FontStyle style)
        {
            using (FontFamily ff = IndexToFontFamily(i))
            {
                return new FontInfo(ff, size, style).CreateFont();
            }
        }
    
        /// <summary>
        /// Gets or sets the font family.
        /// </summary>
        public FontFamily FontFamily
        {
            get
            {
                return new FontFamily((string)this.fontComboBox.SelectedItem);
            }

            set
            {
                string current = (string)this.fontComboBox.SelectedItem;

                if (current != value.Name)
                {
                    int index = fontComboBox.Items.IndexOf(value.Name);

                    if (index != -1)
                    {
                        fontComboBox.SelectedIndex = index;
                        this.OnFontTextChanged();
                    }
                    else
                    {
                        throw new InvalidOperationException("FontFamily is not valid");
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
                    if (alignment == TextAlignment.Left)
                    {
                        this.alignLeftButton.Pushed = true;
                        this.alignCenterButton.Pushed = false;
                        this.alignRightButton.Pushed = false;
                    }
                    else if (alignment == TextAlignment.Center)
                    {
                        this.alignLeftButton.Pushed = false;
                        this.alignCenterButton.Pushed = true;
                        this.alignRightButton.Pushed = false;
                    }
                    else if (alignment == TextAlignment.Right)
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
            this.components = new System.ComponentModel.Container();
            this.fontToolbar = new DotNetWidgets.DotNetToolbar();
            this.boldButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.italicButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.underlineButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignLeftButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignCenterButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.alignRightButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.fontComboBox = new System.Windows.Forms.ComboBox();
            this.sizeComboBox = new System.Windows.Forms.ComboBox();
            this.dotNetToolbar = new DotNetWidgets.DotNetToolbar();
            this.lblFont = new DotNetWidgets.DotNetToolbarLabelItem();
            this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // fontToolbar
            // 
            this.fontToolbar.Buttons.Add(this.boldButton);
            this.fontToolbar.Buttons.Add(this.italicButton);
            this.fontToolbar.Buttons.Add(this.underlineButton);
            this.fontToolbar.Buttons.Add(this.alignLeftButton);
            this.fontToolbar.Buttons.Add(this.alignCenterButton);
            this.fontToolbar.Buttons.Add(this.alignRightButton);
            this.fontToolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.fontToolbar.DrawGrabHandle = false;
            this.fontToolbar.ImageList = null;
            this.fontToolbar.Location = new System.Drawing.Point(216, 0);
            this.fontToolbar.MenuProvider = null;
            this.fontToolbar.Name = "fontToolbar";
            this.fontToolbar.Size = new System.Drawing.Size(456, 26);
            this.fontToolbar.TabIndex = 3;
            this.fontToolbar.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.fontToolbar_ButtonClick);
            // 
            // boldButton
            // 
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
            // fontComboBox
            // 
            this.fontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fontComboBox.Location = new System.Drawing.Point(45, 3);
            this.fontComboBox.Name = "fontComboBox";
            this.fontComboBox.Size = new System.Drawing.Size(120, 21);
            this.fontComboBox.TabIndex = 4;
            this.fontComboBox.SelectedIndexChanged += new System.EventHandler(this.fontComboBox_SelectedIndexChanged);
            // 
            // sizeComboBox
            // 
            this.sizeComboBox.Items.AddRange(new object[] {
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
            this.sizeComboBox.Location = new System.Drawing.Point(168, 3);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(48, 21);
            this.sizeComboBox.TabIndex = 9;
            this.sizeComboBox.Text = "comboBox1";
            // 
            // dotNetToolbar
            // 
            this.dotNetToolbar.Buttons.Add(this.lblFont);
            this.dotNetToolbar.Dock = System.Windows.Forms.DockStyle.None;
            this.dotNetToolbar.DrawGrabHandle = false;
            this.dotNetToolbar.ImageList = null;
            this.dotNetToolbar.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar.MenuProvider = null;
            this.dotNetToolbar.Name = "dotNetToolbar";
            this.dotNetToolbar.Size = new System.Drawing.Size(72, 26);
            this.dotNetToolbar.TabIndex = 8;
            this.dotNetToolbar.TabStop = false;
            // 
            // lblFont
            // 
            this.lblFont.BeginGroup = true;
            this.lblFont.Text = "Font:";
            // 
            // TextConfigWidget
            // 
            this.Controls.Add(this.sizeComboBox);
            this.Controls.Add(this.fontComboBox);
            this.Controls.Add(this.fontToolbar);
            this.Controls.Add(this.dotNetToolbar);
            this.Name = "TextConfigWidget";
            this.Size = new System.Drawing.Size(456, 26);
            this.ResumeLayout(false);

        }
        #endregion

        private void fontComboBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                string displayText = (string)fontComboBox.Items[e.Index];

                SizeF stringSize = e.Graphics.MeasureString(displayText, arialFont10);
                int size = (int)stringSize.Width;
                
                // set up two areas to draw
                Rectangle r = e.Bounds;

                Rectangle rd = r; 
                rd.Width = rd.Left + size;
        
                Rectangle rt = r;
                r.X = rd.Right;

                using (Font myFont = IndexToFont(e.Index, 10, FontStyle.Regular))
                {
                    bool isSymbol = PaintDotNet.SystemLayer.Fonts.IsSymbolFont(myFont);
                    bool isSelected = ((e.State & DrawItemState.Selected) != 0);
                    Brush fillBrush;
                    Brush textBrush;

                    if (isSelected)
                    {
                        fillBrush = highlightBrush;
                        textBrush = highlightTextBrush;
                    }
                    else
                    {
                        fillBrush = windowBrush;
                        textBrush = windowTextBrush;
                    }

                    e.Graphics.FillRectangle(fillBrush, e.Bounds);

                    if (isSymbol)
                    {
                        e.Graphics.DrawString(displayText, arialFont10, textBrush, e.Bounds);
                        e.Graphics.DrawString(displayText, myFont, textBrush, r);
                    }
                    else
                    {
                        e.Graphics.DrawString(displayText, myFont, textBrush, e.Bounds);
                    }
                }
            }

            e.DrawFocusRectangle(); 
        }

        private void fontComboBox_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            // Work out what the text will be
            string displayText = (string)fontComboBox.Items[e.Index];

            // Get width & height of string
            SizeF stringSize;
            using (Font font = IndexToFont(e.Index, 10, FontStyle.Regular))
            {
                stringSize = e.Graphics.MeasureString(displayText, font);
            }

            // Account for top margin
            stringSize.Height += 6;

            // set hight to text height
            e.ItemHeight = (int)stringSize.Height;  

            if (e.ItemHeight > 20)
            {
                e.ItemHeight = 20;
            }

            // set width to text width
            e.ItemWidth = (int)stringSize.Width;    
        }
        
        private void fontToolbar_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            if (e.Button == boldButton)
            {
                style ^= FontStyle.Bold;
                
                if (this.boldButton.Pushed)
                {
                    this.boldButton.Pushed = false;
                }
                else
                {
                    this.boldButton.Pushed = true;
                }

                this.OnFontTextChanged();
            }
            else if (e.Button == italicButton)
            {
                style ^= FontStyle.Italic;

                if (this.italicButton.Pushed)
                {
                    this.italicButton.Pushed = false;
                }
                else
                {
                    this.italicButton.Pushed = true;
                }

                this.OnFontTextChanged();
            }
            else if (e.Button == underlineButton)
            {
                style ^= FontStyle.Underline;

                if (this.underlineButton.Pushed)
                {
                    this.underlineButton.Pushed = false;
                }
                else
                {
                    this.underlineButton.Pushed = true;
                }

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
                    float number = float.Parse(sizeComboBox.Text);
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
                    this.sizeComboBox.BackColor = Color.Red;
                    this.tooltipProvider.SetToolTip(this.sizeComboBox, "Invalid number");
                }
                else
                {
                    if ((float.Parse(sizeComboBox.Text) < minFontSize))
                    {   // Set the error if the size is too small.
                        this.sizeComboBox.BackColor = Color.Red;
                        this.tooltipProvider.SetToolTip(this.sizeComboBox, "Size is smaller than " + minFontSize);
                    } 
                    else if ((float.Parse(sizeComboBox.Text) > maxFontSize))
                    {   // Set the error if the size is too large.
                        this.sizeComboBox.BackColor = Color.Red;
                        this.tooltipProvider.SetToolTip(this.sizeComboBox, "Size is larger than " + maxFontSize);
                    }
                    else 
                    {   // Clear the error, if any, in the error provider.
                        this.tooltipProvider.RemoveAll();
                        this.sizeComboBox.BackColor = SystemColors.Window;
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

        private void fontComboBox_DropDown(object sender, EventArgs e)
        {
            PopulateFonts();
            fontComboBox.DropDown -= new EventHandler(fontComboBox_DropDown);
        }

        private void fontComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            OnFontTextChanged();
        }
    }
}
