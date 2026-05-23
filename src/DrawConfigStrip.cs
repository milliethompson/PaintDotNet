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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class DrawConfigStrip
        : ToolStripEx
    {
        private EnumWrapper hatchStyleNames = EnumWrapper.Create(typeof(HatchStyle));
        private string solidBrushText;
        private ToolStripLabel fillStyleLabel;
        private ToolStripComboBox styleComboBox;
        private ToolStripSeparator separator1;
        private ShapeDrawType shape;
        private ToolStripButton outlineButton;
        private ToolStripButton interiorButton;
        private ToolStripButton bothButton;
        private ToolStripSeparator separator2;
        private ToolStripLabel brushSizeLabel;
        private ToolStripComboBox brushSizeComboBox;
        private int[] brushSizes = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100 };
        private ToolStripSeparator separator3;
        private ToolStripButton alphaBlendingButton;
        private ToolStripButton aaButton;

        public BrushInfo BrushInfo
        {
            get
            {
                if (this.styleComboBox.SelectedItem.ToString() == this.solidBrushText)
                {
                    return new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal);
                }

                if (this.styleComboBox.SelectedIndex == -1)
                {
                    return new BrushInfo(BrushType.Solid, HatchStyle.BackwardDiagonal);
                }
                else
                {
                    return new BrushInfo(BrushType.Hatch, (HatchStyle)this.hatchStyleNames.LocalizedNameToEnumValue(this.styleComboBox.SelectedItem.ToString()));
                }
            }
        }

        public DrawConfigStrip()
        {
            InitializeComponent();

            this.solidBrushText = PdnResources.GetString("BrushConfigWidget.SolidBrush.Text"); // "Solid Brush"
            this.styleComboBox.Items.Add(this.solidBrushText);
            string[] styleNames = this.hatchStyleNames.GetLocalizedNames();
            Array.Sort(styleNames);

            foreach (string styleName in styleNames)
            {
                styleComboBox.Items.Add(styleName);
            }

            styleComboBox.SelectedIndex = 0;

            this.fillStyleLabel.Text = PdnResources.GetString("BrushConfigWidget.FillStyleLabel.Text");

            this.outlineButton.Image = PdnResources.GetImage("Icons.ShapeOutlineIcon.png");
            this.interiorButton.Image = PdnResources.GetImage("Icons.ShapeInteriorIcon.png");
            this.bothButton.Image = PdnResources.GetImage("Icons.ShapeBothIcon.png");

            this.outlineButton.ToolTipText = PdnResources.GetString("ShapeDrawTypeConfigWidget.OutlineButton.ToolTipText");
            this.interiorButton.ToolTipText = PdnResources.GetString("ShapeDrawTypeConfigWidget.InteriorButton.ToolTipText");
            this.bothButton.ToolTipText = PdnResources.GetString("ShapeDrawTypeConfigWidget.BothButton.ToolTipText");

            this.shape = ShapeDrawType.Outline;

            this.brushSizeLabel.Text = PdnResources.GetString("PenConfigWidget.BrushWidthLabel");

            this.brushSizeComboBox.ComboBox.SuspendLayout();
            for (int i = 0; i < this.brushSizes.Length; ++i)
            {
                this.brushSizeComboBox.Items.Add(this.brushSizes[i].ToString());
            }
            this.brushSizeComboBox.ComboBox.ResumeLayout(false);

            this.brushSizeComboBox.SelectedIndex = 1; // default to brush size of 2

            this.aaButton.Image = PdnResources.GetImage("Icons.MenuToolsAntialiasingIcon.png");
            this.alphaBlendingButton.Image = PdnResources.GetImage("Icons.MenuToolsAlphaBlendingIcon.png");

            this.aaButton.ToolTipText = PdnResources.GetString("DrawModesConfigWidget.AAButton.ToolTipText");
            this.alphaBlendingButton.ToolTipText = PdnResources.GetString("DrawModesConfigWidget.AlphaBlendingButton.ToolTipText");

            this.brushSizeComboBox.Size = new Size(UI.ScaleWidth(this.brushSizeComboBox.Width), brushSizeComboBox.Height);
            this.styleComboBox.Size = new Size(UI.ScaleWidth(this.styleComboBox.Width), styleComboBox.Height);
            this.styleComboBox.DropDownWidth = UI.ScaleWidth(this.styleComboBox.DropDownWidth);
            this.styleComboBox.DropDownHeight = UI.ScaleHeight(this.styleComboBox.DropDownHeight);
        }

        private void InitializeComponent()
        {
            this.fillStyleLabel = new ToolStripLabel();
            this.styleComboBox = new ToolStripComboBox();
            this.separator1 = new ToolStripSeparator();
            this.outlineButton = new ToolStripButton();
            this.interiorButton = new ToolStripButton();
            this.bothButton = new ToolStripButton();
            this.separator2 = new ToolStripSeparator();
            this.brushSizeLabel = new ToolStripLabel();
            this.brushSizeComboBox = new ToolStripComboBox();
            this.separator3 = new ToolStripSeparator();
            this.aaButton = new ToolStripButton();
            this.alphaBlendingButton = new ToolStripButton();
            this.SuspendLayout();
            //
            // fillStyleLabel
            //
            this.fillStyleLabel.Name = "fillStyleLabel";
            //
            // styleComboBox
            //
            this.styleComboBox.Name = "styleComboBox";
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.DropDownWidth = 234;
            //
            // styleComboBox.ComboBox
            //
            this.styleComboBox.ComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.styleComboBox.ComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.comboBoxStyle_MeasureItem);
            this.styleComboBox.ComboBox.SelectedValueChanged += new System.EventHandler(this.comboBoxStyle_SelectedValueChanged);
            this.styleComboBox.ComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxStyle_DrawItem);
            //
            // outlineButton
            //
            this.outlineButton.Name = "outlineButton";
            this.outlineButton.Checked = true;
            //
            // interiorButton
            //
            this.interiorButton.Name = "interiorButton";
            //
            // bothButton
            //
            this.bothButton.Name = "bothButton";
            //
            // separator2
            //
            this.separator2 = new ToolStripSeparator();
            //
            // brushSizeLabel
            //
            this.brushSizeLabel.Name = "brushSizeLabel";
            //
            // brushSizeComboBox
            //
            this.brushSizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.brushSizeComboBox_Validating);
            this.brushSizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
            this.brushSizeComboBox.AutoSize = false;
            this.brushSizeComboBox.Width = 44;
            //
            // aaButton
            //
            this.aaButton.Name = "aaButton";
            //
            // alphaBlendingButton
            //
            this.alphaBlendingButton.Name = "alphaBlendingButton";
            //
            // DrawConfigStrip
            //
            this.Items.Add(this.fillStyleLabel);
            this.Items.Add(this.styleComboBox);
            this.Items.Add(this.separator1);
            this.Items.Add(this.outlineButton);
            this.Items.Add(this.interiorButton);
            this.Items.Add(this.bothButton);
            this.Items.Add(this.separator2);
            this.Items.Add(this.aaButton);
            this.Items.Add(this.alphaBlendingButton);
            this.Items.Add(this.separator3);
            this.Items.Add(this.brushSizeLabel);
            this.Items.Add(this.brushSizeComboBox);
            this.ResumeLayout(false);            
        }

        public event EventHandler BrushChanged;
        protected virtual void OnBrushChanged()
        {
            if (BrushChanged != null)
            {
                BrushChanged(this, EventArgs.Empty);
            }
        }

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
            Rectangle r = e.Bounds;

            if (e.Index != -1)
            {
                string itemName = (string)this.styleComboBox.Items[e.Index];

                if (itemName != this.solidBrushText)
                {
                    Rectangle rd = r;
                    rd.Width = rd.Left + 25;

                    Rectangle rt = r;
                    r.X = rd.Right;

                    string displayText = this.styleComboBox.Items[e.Index].ToString();
                    HatchStyle hs = (HatchStyle)this.hatchStyleNames.LocalizedNameToEnumValue(displayText);

                    using (HatchBrush b = new HatchBrush(hs, e.ForeColor, e.BackColor))
                    {
                        e.Graphics.FillRectangle(b, rd);
                    }

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Near;

                    using (SolidBrush sb = new SolidBrush(Color.White))
                    {
                        if ((e.State & DrawItemState.Focus) == 0)
                        {
                            sb.Color = SystemColors.Window;
                            e.Graphics.FillRectangle(sb, r);
                            sb.Color = SystemColors.WindowText;
                            e.Graphics.DrawString(displayText, this.Font, sb, r, sf);
                        }
                        else
                        {
                            sb.Color = SystemColors.Highlight;
                            e.Graphics.FillRectangle(sb, r);
                            sb.Color = SystemColors.HighlightText;
                            e.Graphics.DrawString(displayText, this.Font, sb, r, sf);
                        }
                    }
                }
                else
                {
                    // Solid Brush
                    using (SolidBrush sb = new SolidBrush(Color.White))
                    {
                        if ((e.State & DrawItemState.Focus) == 0)
                        {
                            sb.Color = SystemColors.Window;
                            e.Graphics.FillRectangle(sb, e.Bounds);
                            string displayText = this.styleComboBox.Items[e.Index].ToString();
                            sb.Color = SystemColors.WindowText;
                            e.Graphics.DrawString(displayText, this.Font, sb, e.Bounds);
                        }
                        else
                        {
                            sb.Color = SystemColors.Highlight;
                            e.Graphics.FillRectangle(sb, e.Bounds);
                            string displayText = this.styleComboBox.Items[e.Index].ToString();
                            sb.Color = SystemColors.HighlightText;
                            e.Graphics.DrawString(displayText, this.Font, sb, e.Bounds);
                        }
                    }
                }

                e.DrawFocusRectangle();
            }
        }

        private void comboBoxStyle_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            // Work out what the text will be
            string displayText = this.styleComboBox.Items[e.Index].ToString();

            // Get width & height of string
            SizeF stringSize = e.Graphics.MeasureString(displayText, this.Font);

            // set height to text height
            e.ItemHeight = (int)stringSize.Height;

            // set width to text width
            e.ItemWidth = (int)stringSize.Width;
        }


        public event EventHandler ShapeDrawTypeChanged;
        protected virtual void OnShapeDrawTypeChanged()
        {
            if (ShapeDrawTypeChanged != null)
            {
                ShapeDrawTypeChanged(this, EventArgs.Empty);
            }
        }

        public void PerformShapeDrawTypeChanged()
        {
            OnShapeDrawTypeChanged();
        }

        public ShapeDrawType ShapeDrawType
        {
            get
            {
                return shape;
            }
            set
            {
                if (shape != value)
                {
                    shape = value;

                    // if the user sets the shape the buttons must be updated
                    if (shape == ShapeDrawType.Outline)
                    {
                        this.outlineButton.Checked = true;
                        this.bothButton.Checked = false;
                        this.interiorButton.Checked = false;
                    }
                    else if (shape == ShapeDrawType.Both)
                    {
                        this.outlineButton.Checked = false;
                        this.bothButton.Checked = true;
                        this.interiorButton.Checked = false;
                    }
                    else if (shape == ShapeDrawType.Interior)
                    {
                        this.outlineButton.Checked = false;
                        this.bothButton.Checked = false;
                        this.interiorButton.Checked = true;
                    }
                    else
                    {
                        // invalid shape
                        throw new InvalidOperationException("Shape draw type is invalid");
                    }

                    this.OnShapeDrawTypeChanged();
                }
            }
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == outlineButton)
            {
                this.ShapeDrawType = ShapeDrawType.Outline;
            }
            else if (e.ClickedItem == bothButton)
            {
                this.ShapeDrawType = ShapeDrawType.Both;
            }
            else if (e.ClickedItem == interiorButton)
            {
                this.ShapeDrawType = ShapeDrawType.Interior;
            }
            else if (e.ClickedItem == alphaBlendingButton)
            {
                alphaBlendingButton.Checked = !alphaBlendingButton.Checked;
                OnAlphaBlendingChanged();
            }
            else if (e.ClickedItem == aaButton)
            {
                aaButton.Checked = !aaButton.Checked;
                OnAntiAliasingChanged();
            }

            base.OnItemClicked(e);
        }

        public event EventHandler PenChanged;
        protected virtual void OnPenChanged()
        {
            if (PenChanged != null)
            {
                PenChanged(this, EventArgs.Empty);
            }
        }

        public void PerformPenChanged()
        {
            OnPenChanged();
        }

        public PenInfo PenInfo
        {
            get
            {
                return new PenInfo(DashStyle.Solid, float.Parse(this.brushSizeComboBox.Text));
            }
        }

        private void sizeComboBox_TextChanged(object sender, System.EventArgs e)
        {
            brushSizeComboBox_Validating(this, new CancelEventArgs());
        }

        private void brushSizeComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                bool invalid = false;

                try
                {
                    float number = float.Parse(this.brushSizeComboBox.Text);
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
                    this.brushSizeComboBox.BackColor = Color.Red;
                    this.brushSizeComboBox.ToolTipText = PdnResources.GetString("PenConfigWidget.Error.InvalidNumber");
                }
                else
                {
                    if (float.Parse(this.brushSizeComboBox.Text) < 1)
                    {
                        // Set the error if the size is too small.
                        this.brushSizeComboBox.BackColor = Color.Red;
                        this.brushSizeComboBox.ToolTipText = PdnResources.GetString("PenConfigWidget.Error.TooSmall");
                    }
                    else if ((float.Parse(this.brushSizeComboBox.Text) > 100))
                    {
                        // Set the error if the size is too large.
                        this.brushSizeComboBox.BackColor = Color.Red;
                        this.brushSizeComboBox.ToolTipText = PdnResources.GetString("PenConfigWidget.Error.TooLarge");
                    }
                    else
                    {
                        // Clear the error, if any
                        this.brushSizeComboBox.BackColor = SystemColors.Window;
                        this.brushSizeComboBox.ToolTipText = string.Empty;
                        OnPenChanged();
                    }
                }
            }

            catch (FormatException)
            {
            }
        }

        public event EventHandler AlphaBlendingChanged;
        protected virtual void OnAlphaBlendingChanged()
        {
            if (AlphaBlendingChanged != null)
            {
                AlphaBlendingChanged(this, EventArgs.Empty);
            }
        }

        public void PerformAlphaBlendingChanged()
        {
            OnAlphaBlendingChanged();
        }

        public bool AlphaBlending
        {
            get
            {
                return alphaBlendingButton.Checked;
            }

            set
            {
                alphaBlendingButton.Checked = value;
            }
        }

        public event EventHandler AntiAliasingChanged;
        protected virtual void OnAntiAliasingChanged()
        {
            if (AntiAliasingChanged != null)
            {
                AntiAliasingChanged(this, EventArgs.Empty);
            }
        }

        public void PerformAntiAliasingChanged()
        {
            OnAntiAliasingChanged();
        }

        public bool AntiAliasing
        {
            get
            {
                return aaButton.Checked;
            }

            set
            {
                aaButton.Checked = value;
            }
        }

    }
}
