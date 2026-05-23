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
        //private DotNetWidgets.FlatComboBox styleComboBox = null;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox styleComboBox;
        private DotNetWidgets.DotNetToolbar dotNetToolbar1;
        private DotNetWidgets.DotNetToolbarButtonItem dotNetToolbarButtonItem1; // alises to styleComboBoxTB.ContgainedControl
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
            
            while ((start <= end) && (at > -1))
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
            this.label1 = new System.Windows.Forms.Label();
            this.styleComboBox = new System.Windows.Forms.ComboBox();
            this.dotNetToolbar1 = new DotNetWidgets.DotNetToolbar();
            this.dotNetToolbarButtonItem1 = new DotNetWidgets.DotNetToolbarButtonItem();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "Fill Style:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // styleComboBox
            // 
            this.styleComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.styleComboBox.Location = new System.Drawing.Point(68, 3);
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
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dotNetToolbar1);
            this.Name = "BrushConfigWidget";
            this.Size = new System.Drawing.Size(304, 224);
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
                if (e.Index > 0)
                {
                    Rectangle rd = r; 
                    rd.Width = rd.Left + 25; 
                
                    Rectangle rt = r;
                    r.X = rd.Right; 

                    string displayText = this.styleComboBox.Items[e.Index].ToString();
                    HatchStyle hs = this.getHatchStyle(displayText);

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
            //Work out what the text will be
            string displayText = this.styleComboBox.Items[e.Index].ToString();

            //Get width & height of string
            SizeF stringSize = e.Graphics.MeasureString(displayText, this.Font);

            // set hight to text height
            e.ItemHeight = (int)stringSize.Height;  

            // set width to text width
            e.ItemWidth = (int)stringSize.Width;
        }

    }
}
