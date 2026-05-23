////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class TextConfigStrip
        : ToolStripEx
    {
        private const int maxFontSize = 1000;
        private const int minFontSize = 1;
        private const int initialFontSize = 12;

        private FontFamily arialFontFamily;
        private FontStyle style;
        private TextAlignment alignment;
        private float oldSizeValue;
        private Brush highlightBrush;
        private Brush highlightTextBrush;
        private Brush windowBrush;
        private Brush windowTextBrush;
        private Font arialFontBase;
        private const string arialName = "Arial";

        private bool populatedFonts = false;

        private ToolStripLabel fontLabel;
        private ToolStripComboBox fontComboBox;
        private ToolStripComboBox sizeComboBox;
        private ToolStripSeparator separator1;
        private ToolStripButton boldButton;
        private ToolStripButton italicsButton;
        private ToolStripButton underlineButton;
        private ToolStripSeparator separator2;
        private ToolStripButton alignLeftButton;
        private ToolStripButton alignCenterButton;
        private ToolStripButton alignRightButton;

        private int[] fontSizes = new int[] { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        public TextConfigStrip()
        {
            InitializeComponent();

            this.sizeComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < this.fontSizes.Length; ++i)
            {
                this.sizeComboBox.Items.Add(this.fontSizes[i].ToString());
            }
            this.sizeComboBox.ComboBox.ResumeLayout(false);

            this.fontLabel.Text = PdnResources.GetString("TextConfigWidget.FontLabel.Text");

            this.arialFontFamily = new FontFamily(arialName);
            this.arialFontBase = new Font(arialFontFamily, initialFontSize, FontStyle.Regular);

            this.alignment = TextAlignment.Left;
            this.alignLeftButton.Checked = true;
            this.oldSizeValue = initialFontSize;

            this.highlightBrush = new SolidBrush(SystemColors.Highlight);
            this.highlightTextBrush = new SolidBrush(SystemColors.HighlightText);
            this.windowBrush = new SolidBrush(SystemColors.Window);
            this.windowTextBrush = new SolidBrush(SystemColors.WindowText);

            // These buttons need a color key to maintain consistency with v2.5 language packs
            this.boldButton.ImageTransparentColor = Utility.TransparentKey;
            this.italicsButton.ImageTransparentColor = Utility.TransparentKey;
            this.underlineButton.ImageTransparentColor = Utility.TransparentKey;

            this.boldButton.Image = PdnResources.GetImageBmpOrPng("Icons.FontBoldIcon");
            this.italicsButton.Image = PdnResources.GetImageBmpOrPng("Icons.FontItalicIcon");
            this.underlineButton.Image = PdnResources.GetImageBmpOrPng("Icons.FontUnderlineIcon");

            this.alignLeftButton.Image = PdnResources.GetImage("Icons.TextAlignLeftIcon.png");
            this.alignCenterButton.Image = PdnResources.GetImage("Icons.TextAlignCenterIcon.png");
            this.alignRightButton.Image = PdnResources.GetImage("Icons.TextAlignRightIcon.png");

            this.boldButton.ToolTipText = PdnResources.GetString("TextConfigWidget.BoldButton.ToolTipText");
            this.italicsButton.ToolTipText = PdnResources.GetString("TextConfigWidget.ItalicButton.ToolTipText");
            this.underlineButton.ToolTipText = PdnResources.GetString("TextConfigWidget.UnderlineButton.ToolTipText");
            this.alignLeftButton.ToolTipText = PdnResources.GetString("TextConfigWidget.AlignLeftButton.ToolTipText");
            this.alignCenterButton.ToolTipText = PdnResources.GetString("TextConfigWidget.AlignCenterButton.ToolTipText");
            this.alignRightButton.ToolTipText = PdnResources.GetString("TextConfigWidget.AlignRightButton.ToolTipText");

            this.fontComboBox.Size = new Size(UI.ScaleWidth(this.fontComboBox.Width), fontComboBox.Height);
            this.fontComboBox.DropDownWidth = UI.ScaleWidth(this.fontComboBox.DropDownWidth);
            this.sizeComboBox.Size = new Size(UI.ScaleWidth(this.sizeComboBox.Width), sizeComboBox.Height);
        }

        private void InitializeComponent()
        {
            this.fontLabel = new ToolStripLabel();
            this.fontComboBox = new ToolStripComboBox();
            this.sizeComboBox = new ToolStripComboBox();
            this.separator1 = new ToolStripSeparator();
            this.boldButton = new ToolStripButton();
            this.italicsButton = new ToolStripButton();
            this.underlineButton = new ToolStripButton();
            this.separator2 = new ToolStripSeparator();
            this.alignLeftButton = new ToolStripButton();
            this.alignCenterButton = new ToolStripButton();
            this.alignRightButton = new ToolStripButton();
            this.SuspendLayout();
            //
            // fontLabel
            //
            this.fontLabel.Name = "fontLabel";
            //
            // fontComboBox
            //
            this.fontComboBox.Name = "fontComboBox";
            this.fontComboBox.AllowDrop = true;
            this.fontComboBox.DropDownWidth = 240;
            this.fontComboBox.MaxDropDownItems = 12;
            this.fontComboBox.Name = "fontComboBox";
            this.fontComboBox.Sorted = true;
            //this.fontComboBox.DropDown += new EventHandler(fontComboBox_DropDown);
            this.fontComboBox.GotFocus += new EventHandler(fontComboBox_GotFocus);
            this.fontComboBox.Items.Add(arialName);
            this.fontComboBox.SelectedItem = arialName;
            this.fontComboBox.SelectedIndexChanged += fontComboBox_SelectedIndexChanged;
            this.fontComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            //
            // fontComboBox.ComboBox
            //
            this.fontComboBox.ComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.fontComboBox.ComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.fontComboBox_MeasureItem);
            this.fontComboBox.ComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.fontComboBox_DrawItem);
            //
            // sizeComboBox
            //
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.AutoSize = false;
            this.sizeComboBox.TextChanged += new EventHandler(sizeComboBox_TextChanged);
            this.sizeComboBox.Validating += new CancelEventHandler(sizeComboBox_Validating);
            this.sizeComboBox.Text = initialFontSize.ToString();
            this.sizeComboBox.Width = 44;
            //
            // boldButton
            //
            this.boldButton.Name = "boldButton";
            //
            // italicsButton
            //
            this.italicsButton.Name = "italicsButton";
            //
            // underlineButton
            //
            this.underlineButton.Name = "underlineButton";
            //
            // alignLeftButton
            //
            this.alignLeftButton.Name = "alignLeftButton";
            //
            // alignCenterButton
            //
            this.alignCenterButton.Name = "alignCenterButton";
            //
            // alignRightButton
            //
            this.alignRightButton.Name = "alignRightButton";
            //
            // TextConfigStrip
            //
            this.Items.Add(this.fontLabel);
            this.Items.Add(this.fontComboBox);
            this.Items.Add(this.sizeComboBox);
            this.Items.Add(this.separator1);
            this.Items.Add(this.boldButton);
            this.Items.Add(this.italicsButton);
            this.Items.Add(this.underlineButton);
            this.Items.Add(this.separator2);
            this.Items.Add(this.alignLeftButton);
            this.Items.Add(this.alignCenterButton);
            this.Items.Add(this.alignRightButton);
            this.ResumeLayout(false);
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
            }
        }

        public event EventHandler RelinquishFocus;
        protected virtual void OnRelinquishFocus()
        {
            if (RelinquishFocus != null)
            {
                RelinquishFocus(this, EventArgs.Empty);
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
                        this.alignLeftButton.Checked = true;
                        this.alignCenterButton.Checked = false;
                        this.alignRightButton.Checked = false;
                    }
                    else if (alignment == TextAlignment.Center)
                    {
                        this.alignLeftButton.Checked = false;
                        this.alignCenterButton.Checked = true;
                        this.alignRightButton.Checked = false;
                    }
                    else if (alignment == TextAlignment.Right)
                    {
                        this.alignLeftButton.Checked = false;
                        this.alignCenterButton.Checked = false;
                        this.alignRightButton.Checked = true;
                    }
                    else
                    {
                        throw new InvalidOperationException("Text alignment type is invalid");
                    }

                    this.OnTextAlignmentChanged();
                }
            }
        }


        private void fontComboBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index != -1)
            {
                string displayText = (string)fontComboBox.Items[e.Index];

                SizeF stringSize = e.Graphics.MeasureString(displayText, arialFontBase);
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
                        e.Graphics.DrawString(displayText, arialFontBase, textBrush, e.Bounds);
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
            stringSize.Height += UI.ScaleHeight(6);

            // set hight to text height
            e.ItemHeight = (int)stringSize.Height;
            int maxHeight = UI.ScaleHeight(20);

            if (e.ItemHeight > maxHeight)
            {
                e.ItemHeight = maxHeight;
            }

            // set width to text width
            e.ItemWidth = (int)stringSize.Width;
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == boldButton)
            {
                style ^= FontStyle.Bold;

                if (this.boldButton.Checked)
                {
                    this.boldButton.Checked = false;
                }
                else
                {
                    this.boldButton.Checked = true;
                }

                this.OnFontTextChanged();
            }
            else if (e.ClickedItem == italicsButton)
            {
                style ^= FontStyle.Italic;

                if (this.italicsButton.Checked)
                {
                    this.italicsButton.Checked = false;
                }
                else
                {
                    this.italicsButton.Checked = true;
                }

                this.OnFontTextChanged();
            }
            else if (e.ClickedItem == underlineButton)
            {
                style ^= FontStyle.Underline;

                if (this.underlineButton.Checked)
                {
                    this.underlineButton.Checked = false;
                }
                else
                {
                    this.underlineButton.Checked = true;
                }

                this.OnFontTextChanged();
            }
            else if (e.ClickedItem == alignLeftButton)
            {
                this.TextAlignment = TextAlignment.Left;
            }
            else if (e.ClickedItem == alignCenterButton)
            {
                this.TextAlignment = TextAlignment.Center;
            }
            else if (e.ClickedItem == alignRightButton)
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
                    this.sizeComboBox.ToolTipText = PdnResources.GetString("TextConfigWidget.Error.InvalidNumber");
                }
                else
                {
                    if ((float.Parse(sizeComboBox.Text) < minFontSize))
                    {
                        // Set the error if the size is too small.
                        this.sizeComboBox.BackColor = Color.Red;
                        string format = PdnResources.GetString("TextConfigWidget.Error.TooSmall.Format");
                        string text = string.Format(format, minFontSize);
                        this.sizeComboBox.ToolTipText = text;
                    }
                    else if ((float.Parse(sizeComboBox.Text) > maxFontSize))
                    {
                        // Set the error if the size is too large.
                        this.sizeComboBox.BackColor = Color.Red;
                        string format = PdnResources.GetString("TextConfigWidget.Error.TooLarge.Format");
                        string text = string.Format(format, maxFontSize);
                        this.sizeComboBox.ToolTipText = text;
                    }
                    else
                    {
                        // Clear the error, if any
                        this.sizeComboBox.ToolTipText = string.Empty;
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
            sizeComboBox_Validating(sender, new CancelEventArgs());
            //this.Validate();
        }

        /*
        private void fontComboBox_DropDown(object sender, EventArgs e)
        {
            this.droppedDown = true;
            this.changeCommited = false;
        }
        */

        private void fontComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            OnFontTextChanged();
        }

        private void fontComboBox_GotFocus(object sender, EventArgs e)
        {
            if (!populatedFonts)
            {
                PopulateFonts();
                populatedFonts = true;
            }
        }
    }
}
