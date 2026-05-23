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
using System.Windows.Forms;

namespace PaintDotNet
{
    public class ZoomConfigWidget : System.Windows.Forms.UserControl
    {
        private string windowText;
        private string selectionText;
        private string percentageFormat;

        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ComboBox udZoom;
        private ScaleFactor scaleFactor;
        private DotNetWidgets.DotNetToolbar tbZoomConfig;
        private DotNetWidgets.DotNetToolbarButtonItem zoomInButton;
        private DotNetWidgets.DotNetToolbarButtonItem zoomOutButton;
        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ToolTip tooltipProvider;
    
        private ZoomBasis zoomBasis;
        public ZoomBasis ZoomBasis 
        {
            get 
            {
                return zoomBasis;
            }
            set 
            {
                zoomBasis = value;
                /* Call OnZoomBasisChanged regardless of whether or not this is actually
                 * a new value. If this is not done, the document will not be re-fitted
                 * when this is assigned, as expected (Such as in MainForm's DoOpenFile)
                 */
                OnZoomBasisChanged();
            }
        }

        public ScaleFactor ScaleFactor 
        {
            get
            {
                return scaleFactor;
            }

            set 
            {
                if (scaleFactor.Ratio != value.Ratio) 
                {
                    scaleFactor = value;
                    OnZoomScaleChanged();
                }
            }
        }

        public ZoomConfigWidget()
        {
            InitializeComponent();

            this.windowText = EnumWrapper.EnumValueToLocalizedName(typeof(ZoomBasis), ZoomBasis.Window);
            this.selectionText = EnumWrapper.EnumValueToLocalizedName(typeof(ZoomBasis), ZoomBasis.Selection);
            this.percentageFormat = PdnResources.GetString("ZoomConfigWidget.Percentage.Format");

            this.udZoom.Items.AddRange(new object[] {
                                                        string.Format(this.percentageFormat, 3200),
                                                        string.Format(this.percentageFormat, 1600),
                                                        string.Format(this.percentageFormat, 800),
                                                        string.Format(this.percentageFormat, 400),
                                                        string.Format(this.percentageFormat, 200),
                                                        string.Format(this.percentageFormat, 100),
                                                        string.Format(this.percentageFormat, 50),
                                                        string.Format(this.percentageFormat, 25),
                                                        string.Format(this.percentageFormat, 10),
                                                        string.Format(this.percentageFormat, 5),
                                                        string.Format(this.percentageFormat, 2),
                                                        this.windowText
                                                    });

            this.udZoom.Text = string.Format(this.percentageFormat, 100);

            this.ScaleFactor = ScaleFactor.OneToOne;
            udZoom.SelectedIndex = 4;
            udZoom.DropDownWidth = 1;

            Graphics g = Graphics.FromHwnd(udZoom.Handle);

            foreach (string str in udZoom.Items) 
            {
                udZoom.DropDownWidth = (int)Math.Max(udZoom.DropDownWidth, 2 + g.MeasureString(str, udZoom.Font).Width);
            }

            tbZoomConfig.ImageList = imageList;
            imageList.TransparentColor = Color.FromArgb(192, 192, 192);
            zoomInButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuViewZoomInIcon.bmp"), imageList.TransparentColor);            
            zoomOutButton.ImageIndex = imageList.Images.Add(PdnResources.GetImage("Icons.MenuViewZoomOutIcon.bmp"), imageList.TransparentColor);
            zoomInButton.ToolTipText = PdnResources.GetString("ZoomConfigWidget.ZoomInButton.ToolTipText");
            zoomOutButton.ToolTipText = PdnResources.GetString("ZoomConfigWidget.ZoomOutButton.ToolTipText");
            zoomBasis = ZoomBasis.Factor;
            ScaleFactor = ScaleFactor.OneToOne;
        }

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
            this.udZoom = new System.Windows.Forms.ComboBox();
            this.tooltipProvider = new System.Windows.Forms.ToolTip(this.components);
            this.tbZoomConfig = new DotNetWidgets.DotNetToolbar();
            this.zoomInButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.zoomOutButton = new DotNetWidgets.DotNetToolbarButtonItem();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // udZoom
            // 
            this.udZoom.DropDownWidth = 128;
            this.udZoom.ItemHeight = 13;


            this.udZoom.Location = new System.Drawing.Point(57, 3);
            this.udZoom.MaxDropDownItems = 99;
            this.udZoom.Name = "udZoom";
            this.udZoom.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.udZoom.Size = new System.Drawing.Size(70, 21);
            this.udZoom.TabIndex = 1;
            this.udZoom.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.udZoom_KeyPress);
            this.udZoom.Validating += new System.ComponentModel.CancelEventHandler(this.udZoom_Validating);
            this.udZoom.SelectedIndexChanged += new System.EventHandler(this.udZoom_SelectedIndexChanged);
            // 
            // tbZoomConfig
            // 
            this.tbZoomConfig.Buttons.Add(this.zoomOutButton);
            this.tbZoomConfig.Buttons.Add(this.zoomInButton);
            this.tbZoomConfig.DrawGrabHandle = false;
            this.tbZoomConfig.ImageList = null;
            this.tbZoomConfig.Location = new System.Drawing.Point(0, 0);
            this.tbZoomConfig.MenuProvider = null;
            this.tbZoomConfig.Name = "tbZoomConfig";
            this.tbZoomConfig.Size = new System.Drawing.Size(120, 26);
            this.tbZoomConfig.TabIndex = 2;
            this.tbZoomConfig.ButtonClick += new DotNetWidgets.DotNetToolbar.ButtonClickEventHandler(this.tbZoomConfig_ButtonClick);
            // 
            // zoomInButton
            // 
            this.zoomOutButton.BeginGroup = true;
            // 
            // imageList
            // 
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ZoomConfigWidget
            // 
            this.Controls.Add(this.udZoom);
            this.Controls.Add(this.tbZoomConfig);
            this.Name = "ZoomConfigWidget";
            this.Size = new System.Drawing.Size(120, 26);
            this.ResumeLayout(false);
        }
        #endregion
        
        private void SetZoomText() 
        {
            this.udZoom.BackColor = SystemColors.Window;
            string newText = udZoom.Text;

            switch (zoomBasis) 
            {
                case ZoomBasis.Window: 
                    newText = this.windowText;
                    break;

                case ZoomBasis.Selection:
                    newText = this.selectionText;
                    break;

                case ZoomBasis.Factor:
                    newText = scaleFactor.ToString();
                    break;
            }

            if (udZoom.Text != newText)
            {
                udZoom.Text = newText;
            }
        }

        public event EventHandler ZoomScaleChanged;
        protected void OnZoomScaleChanged() 
        {
            if (zoomBasis == ZoomBasis.Factor) 
            {
                SetZoomText();

                if (ZoomScaleChanged != null)
                {
                    ZoomScaleChanged(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler ZoomIn;
        protected void OnZoomIn() 
        {
            if (ZoomIn != null)
            {
                ZoomIn(this, EventArgs.Empty);
            }
        }

        public event EventHandler ZoomOut;
        protected void OnZoomOut() 
        {
            if (ZoomOut != null)
            {
                ZoomOut(this, EventArgs.Empty);
            }
        }

        public void PerformZoomBasisChanged() 
        {
            OnZoomBasisChanged();
        }

        public event EventHandler ZoomBasisChanged;
        protected void OnZoomBasisChanged() 
        {
            SetZoomText();

            if (ZoomBasisChanged != null)
            {
                ZoomBasisChanged(this, EventArgs.Empty);
            }
        }   

        public void PerformZoomScaleChanged()
        {
            OnZoomScaleChanged();
        }

        private void udZoom_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                int val = 1;
                e.Cancel = false;

                if (udZoom.Text == this.windowText) 
                {
                    ZoomBasis = ZoomBasis.Window;
                } 
                else if (udZoom.Text == this.selectionText)
                {
                    ZoomBasis = ZoomBasis.Selection;
                }
                else 
                {
                    try
                    {
                        string text = udZoom.Text;

                        if (text.Length == 0)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            if (text[text.Length - 1] == '%')
                            {
                                text = text.Substring(0, text.Length - 1);
                            }
                            else if (text[0] == '%')
                            {
                                text = text.Substring(1);
                            }

                            val = (int)Math.Round(double.Parse(text));
                            ZoomBasis = ZoomBasis.Factor;
                        }
                    }

                    catch (FormatException)
                    {
                        e.Cancel = true;
                    }

                    catch (OverflowException)
                    {
                        e.Cancel = true;
                    }

                    if (e.Cancel)
                    {
                        this.udZoom.BackColor = Color.Red;
                        this.tooltipProvider.SetToolTip(this.udZoom, PdnResources.GetString("ZoomConfigWidget.Error.InvalidNumber"));
                    }
                    else
                    {
                        if (val < 1)
                        {
                            e.Cancel = true;
                            this.udZoom.BackColor = Color.Red;
                            this.tooltipProvider.SetToolTip(this.udZoom, PdnResources.GetString("ZoomConfigWidget.Error.TooSmall"));
                        }
                        else if (val > 3200)
                        {
                            e.Cancel = true;
                            this.udZoom.BackColor = Color.Red;
                            this.tooltipProvider.SetToolTip(this.udZoom, PdnResources.GetString("ZoomConfigWidget.Error.TooLarge"));
                        }
                        else 
                        {
                            // Clear the error, if any, in the error provider.
                            e.Cancel = false;
                            this.tooltipProvider.RemoveAll();
                            this.udZoom.BackColor = SystemColors.Window;
                            ScaleFactor = new ScaleFactor(val, 100);
                        }
                    }
                }
            }

            catch (FormatException)
            {
            }
        }

        private void tbZoomConfig_ButtonClick(object sender, DotNetWidgets.DotNetToolbarItemClickEventArgs e)
        {
            ScaleFactor oldSF = this.ScaleFactor;

            // We often end up in a feedback loop of some sort where the scale factor will be read
            // as, say, 6.125% and then get increased to 6.25% and then converted back to 6.125%
            // So we first force an increase of one percentage point, then jump up (or down) to the 
            // next power of 2
            if (e.Button == zoomInButton)
            {
                OnZoomIn();
            } 
            else if (e.Button == zoomOutButton) 
            {
                OnZoomOut();
            }
        }

        private void udZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Validate();
        }

        private void udZoom_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.KeyChar == '\n' || e.KeyChar == '\r')
            {
                udZoom_Validating(sender, new CancelEventArgs(false));
                udZoom.Select(0, udZoom.Text.Length);
            }
        }
    }
}
