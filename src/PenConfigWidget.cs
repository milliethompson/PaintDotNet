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
    /// Summary description for PenConfigWidget.
    /// </summary>
    public class PenConfigWidget : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.ComboBox sizeComboBox;
        private DotNetWidgets.DotNetToolbar tbPenConfig;
        private DotNetWidgets.DotNetToolbarLabelItem brushWidthLabel;
        private System.Windows.Forms.ToolTip tooltipProvider;
        private System.ComponentModel.IContainer components;

        public PenConfigWidget()
        {
            InitializeComponent();

            this.brushWidthLabel.Text = PdnResources.GetString("PenConfigWidget.BrushWidthLabel");

            this.sizeComboBox.Items.AddRange(new object[] {
                                                              1.ToString(),
                                                              2.ToString(),
                                                              3.ToString(),
                                                              4.ToString(),
                                                              5.ToString(),
                                                              6.ToString(),
                                                              7.ToString(),
                                                              8.ToString(),
                                                              9.ToString(),
                                                              10.ToString(),
                                                              11.ToString(),
                                                              12.ToString(),
                                                              13.ToString(),
                                                              14.ToString(),
                                                              15.ToString(),
                                                              20.ToString(),
                                                              25.ToString(),
                                                              30.ToString(),
                                                              35.ToString(),
                                                              40.ToString(),
                                                              45.ToString(),
                                                              50.ToString(),
                                                              55.ToString(),
                                                              60.ToString(),
                                                              65.ToString(),
                                                              70.ToString(),
                                                              75.ToString(),
                                                              80.ToString(),
                                                              85.ToString(),
                                                              90.ToString(),
                                                              95.ToString(),
                                                              100.ToString()});
            this.sizeComboBox.Text = 2.ToString();

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
                return new PenInfo(DashStyle.Solid, float.Parse(this.sizeComboBox.Text));   
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
            this.sizeComboBox = new System.Windows.Forms.ComboBox();
            this.tbPenConfig = new DotNetWidgets.DotNetToolbar();
            this.brushWidthLabel = new DotNetWidgets.DotNetToolbarLabelItem();
            this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // sizeComboBox
            // 
            this.sizeComboBox.ItemHeight = 13;
            this.sizeComboBox.Location = new System.Drawing.Point(87, 3);
            this.sizeComboBox.Name = "sizeComboBox";
            this.sizeComboBox.Size = new System.Drawing.Size(44, 21);
            this.sizeComboBox.TabIndex = 9;
            this.sizeComboBox.Validating += new System.ComponentModel.CancelEventHandler(this.sizeComboBox_Validating);
            this.sizeComboBox.TextChanged += new System.EventHandler(this.sizeComboBox_TextChanged);
            // 
            // tbPenConfig
            // 
            this.tbPenConfig.Buttons.Add(this.brushWidthLabel);
            this.tbPenConfig.DrawGrabHandle = false;
            this.tbPenConfig.ImageList = null;
            this.tbPenConfig.Location = new System.Drawing.Point(0, 0);
            this.tbPenConfig.MenuProvider = null;
            this.tbPenConfig.Name = "tbPenConfig";
            this.tbPenConfig.Size = new System.Drawing.Size(133, 26);
            this.tbPenConfig.TabIndex = 12;
            // 
            // brushWidthLabel
            // 
            this.brushWidthLabel.BeginGroup = true;
            // 
            // PenConfigWidget
            // 
            this.Controls.Add(this.sizeComboBox);
            this.Controls.Add(this.tbPenConfig);
            this.Name = "PenConfigWidget";
            this.Size = new System.Drawing.Size(133, 26);
            this.ResumeLayout(false);

        }
        #endregion


        private void sizeComboBox_TextChanged(object sender, System.EventArgs e)
        {
            this.Validate();
        }

        private void sizeComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                bool invalid = false;

                try
                {
                    float number = float.Parse(this.sizeComboBox.Text);
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
                    this.tooltipProvider.SetToolTip(this.sizeComboBox, PdnResources.GetString("PenConfigWidget.Error.InvalidNumber"));
                }
                else
                {
                    if (float.Parse(this.sizeComboBox.Text) < 1)
                    {
                        // Set the error if the size is too small.
                        this.sizeComboBox.BackColor = Color.Red;
                        this.tooltipProvider.SetToolTip(this.sizeComboBox, PdnResources.GetString("PenConfigWidget.Error.TooSmall"));
                    }
                    else if ((float.Parse(this.sizeComboBox.Text) > 100 ))
                    {
                        // Set the error if the size is too large.
                        this.sizeComboBox.BackColor = Color.Red;
                        this.tooltipProvider.SetToolTip(this.sizeComboBox, PdnResources.GetString("PenConfigWidget.Error.TooLarge"));
                    }
                    else 
                    {
                        // Clear the error, if any, in the error provider.
                        this.sizeComboBox.BackColor = SystemColors.Window;
                        this.tooltipProvider.RemoveAll();
                        OnPenChanged();
                    }
                }
            }

            catch (FormatException)
            {
            }
        }
    }
}
