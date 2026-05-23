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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for BrushConfigWidget.
    /// </summary>
    public class BrushConfigWidget : System.Windows.Forms.UserControl
    {
        private EnumWrapper hatchStyleNames = EnumWrapper.Create(typeof(HatchStyle));
        private string solidBrushText;
        private System.Windows.Forms.Label fillStyleLabel;
        private System.Windows.Forms.ComboBox styleComboBox;
        private DotNetWidgets.DotNetToolbar dotNetToolbar1;
        private DotNetWidgets.DotNetToolbarButtonItem dotNetToolbarButtonItem1; // aliases to styleComboBoxTB.ContainedControl

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

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

        public BrushConfigWidget()
        {
            // This call is required by the Windows.Forms Form Designer.
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
            
            this.styleComboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.DropDownWidth = 190;
            this.styleComboBox.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.comboBoxStyle_MeasureItem);
            this.styleComboBox.SelectedValueChanged += new System.EventHandler(this.comboBoxStyle_SelectedValueChanged);
            this.styleComboBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.comboBoxStyle_DrawItem);

            this.fillStyleLabel.Text = PdnResources.GetString("BrushConfigWidget.FillStyleLabel.Text");
        }

        public event EventHandler BrushChanged;
        protected virtual void OnBrushChanged()
        {
            if (BrushChanged != null)
            {
                BrushChanged(this, EventArgs.Empty);
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
            this.fillStyleLabel = new System.Windows.Forms.Label();
            this.styleComboBox = new System.Windows.Forms.ComboBox();
            this.dotNetToolbar1 = new DotNetWidgets.DotNetToolbar();
            this.dotNetToolbarButtonItem1 = ((DotNetWidgets.DotNetToolbarButtonItem)(new DotNetWidgets.DotNetToolbarButtonItem()));
            this.SuspendLayout();
            // 
            // fillStyleLabel
            // 
            this.fillStyleLabel.Location = new System.Drawing.Point(12, 1);
            this.fillStyleLabel.Name = "fillStyleLabel";
            this.fillStyleLabel.Size = new System.Drawing.Size(82, 23);
            this.fillStyleLabel.TabIndex = 5;
            this.fillStyleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // styleComboBox
            // 
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.Location = new System.Drawing.Point(78, 3);
            this.styleComboBox.Name = "styleComboBox";
            this.styleComboBox.Size = new System.Drawing.Size(128, 21);
            this.styleComboBox.TabIndex = 6;
            // 
            // dotNetToolbar1
            // 
            this.dotNetToolbar1.Buttons.Add(this.dotNetToolbarButtonItem1);
            this.dotNetToolbar1.Dock = System.Windows.Forms.DockStyle.None;
            this.dotNetToolbar1.DrawGrabHandle = false;
            this.dotNetToolbar1.ImageList = null;
            this.dotNetToolbar1.Location = new System.Drawing.Point(0, 0);
            this.dotNetToolbar1.MenuProvider = null;
            this.dotNetToolbar1.Name = "dotNetToolbar1";
            this.dotNetToolbar1.Size = new System.Drawing.Size(32, 26);
            this.dotNetToolbar1.TabIndex = 7;
            // 
            // dotNetToolbarButtonItem1
            // 
            this.dotNetToolbarButtonItem1.BeginGroup = true;
            this.dotNetToolbarButtonItem1.Enabled = false;
            // 
            // BrushConfigWidget
            // 
            this.Controls.Add(this.styleComboBox);
            this.Controls.Add(this.fillStyleLabel);
            this.Controls.Add(this.dotNetToolbar1);
            this.Name = "BrushConfigWidget";
            this.Size = new System.Drawing.Size(226, 32);
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
    }
}
