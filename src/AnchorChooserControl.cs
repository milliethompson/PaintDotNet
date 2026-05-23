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
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for AnchorChooserControl.
    /// </summary>
    public class AnchorChooserControl : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.RadioButton middleButton;
        private System.Windows.Forms.RadioButton leftButton;
        private System.Windows.Forms.RadioButton topRightButton;
        private System.Windows.Forms.RadioButton topButton;
        private System.Windows.Forms.RadioButton topLeftButton;
        private System.Windows.Forms.RadioButton[,] buttons;
        private System.Windows.Forms.RadioButton bottomRightButton;
        private System.Windows.Forms.RadioButton bottomButton;
        private System.Windows.Forms.RadioButton bottomLeftButton;
        private System.Windows.Forms.RadioButton rightButton;
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private Point ButtonToXy(RadioButton button)
        {
            for (int y = 0; y < 3; ++y)
            {
                for (int x = 0; x < 3; ++x)
                {
                    if (buttons[y,x] == button)
                    {
                        return new Point(x, y);
                    }
                }
            }

            return new Point(-1, -1);
        }

        public event EventHandler AnchorEdgeChanged;
        protected virtual void OnAnchorEdgeChanged()
        {
            if (AnchorEdgeChanged != null)
            {
                AnchorEdgeChanged(this, EventArgs.Empty);
            }
        }

        private AnchorEdge anchorEdge;
        public AnchorEdge AnchorEdge
        {
            get
            {
                return anchorEdge;
            }

            set
            {
                if (anchorEdge != value)
                {
                    anchorEdge = value;
                    AnchorEdgeToButton(value).PerformClick();
                    OnAnchorEdgeChanged();
                }
            }
        }

        private RadioButton AnchorEdgeToButton(AnchorEdge edge)
        {
            foreach (RadioButton button in buttons)
            {
                if ((AnchorEdge)button.Tag == edge)
                {
                    return button;
                }
            }

            return null;
        }

        protected override void OnLoad(EventArgs e)
        {
            AnchorEdgeToButton(anchorEdge).PerformClick();
        }


        public AnchorChooserControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            buttons = new System.Windows.Forms.RadioButton[3,3];
            topLeftButton.Tag = AnchorEdge.TopLeft;
            topButton.Tag = AnchorEdge.Top;
            topRightButton.Tag = AnchorEdge.TopRight;
            leftButton.Tag = AnchorEdge.Left;
            middleButton.Tag = AnchorEdge.Middle;
            rightButton.Tag = AnchorEdge.Right;
            bottomLeftButton.Tag = AnchorEdge.BottomLeft;
            bottomButton.Tag = AnchorEdge.Bottom;
            bottomRightButton.Tag = AnchorEdge.BottomRight;

            buttons[0,0] = topLeftButton;
            buttons[0,1] = topButton;
            buttons[0,2] = topRightButton;
            buttons[1,0] = leftButton;
            buttons[1,1] = middleButton;
            buttons[1,2] = rightButton;
            buttons[2,0] = bottomLeftButton;
            buttons[2,1] = bottomButton;
            buttons[2,2] = bottomRightButton;

            AnchorEdge = AnchorEdge.Middle;

            OnResize(EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            for (int y = 0; y < 3; ++y)
            {
                for (int x = 0; x < 3; ++x)
                {
                    int cx = (x * this.Width) / 3;
                    int cy = (y * this.Height) / 3;
                    int width = (((x + 1) * this.Width) / 3) - cx + 1;
                    int height = (((y + 1) * this.Height) / 3) - cy + 1;

                    buttons[y,x].Location = new Point(cx, cy);
                    buttons[y,x].Size = new Size(width, height);
                }
            }
        }

        private void button_Click(object sender, System.EventArgs e)
        {
            this.AnchorEdge = (AnchorEdge)((RadioButton)sender).Tag;
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
            this.bottomRightButton = new System.Windows.Forms.RadioButton();
            this.bottomButton = new System.Windows.Forms.RadioButton();
            this.bottomLeftButton = new System.Windows.Forms.RadioButton();
            this.rightButton = new System.Windows.Forms.RadioButton();
            this.middleButton = new System.Windows.Forms.RadioButton();
            this.leftButton = new System.Windows.Forms.RadioButton();
            this.topRightButton = new System.Windows.Forms.RadioButton();
            this.topButton = new System.Windows.Forms.RadioButton();
            this.topLeftButton = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // bottomRightButton
            // 
            this.bottomRightButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.bottomRightButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.bottomRightButton.Location = new System.Drawing.Point(103, 99);
            this.bottomRightButton.Name = "bottomRightButton";
            this.bottomRightButton.Size = new System.Drawing.Size(56, 48);
            this.bottomRightButton.TabIndex = 17;
            this.bottomRightButton.Click += new System.EventHandler(this.button_Click);
            // 
            // bottomButton
            // 
            this.bottomButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.bottomButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.bottomButton.Location = new System.Drawing.Point(47, 99);
            this.bottomButton.Name = "bottomButton";
            this.bottomButton.Size = new System.Drawing.Size(56, 48);
            this.bottomButton.TabIndex = 16;
            this.bottomButton.Click += new System.EventHandler(this.button_Click);
            // 
            // bottomLeftButton
            // 
            this.bottomLeftButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.bottomLeftButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.bottomLeftButton.Location = new System.Drawing.Point(-9, 99);
            this.bottomLeftButton.Name = "bottomLeftButton";
            this.bottomLeftButton.Size = new System.Drawing.Size(56, 48);
            this.bottomLeftButton.TabIndex = 15;
            this.bottomLeftButton.Click += new System.EventHandler(this.button_Click);
            // 
            // rightButton
            // 
            this.rightButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.rightButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.rightButton.Location = new System.Drawing.Point(103, 51);
            this.rightButton.Name = "rightButton";
            this.rightButton.Size = new System.Drawing.Size(56, 48);
            this.rightButton.TabIndex = 14;
            this.rightButton.Click += new System.EventHandler(this.button_Click);
            // 
            // middleButton
            // 
            this.middleButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.middleButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.middleButton.Location = new System.Drawing.Point(47, 51);
            this.middleButton.Name = "middleButton";
            this.middleButton.Size = new System.Drawing.Size(56, 48);
            this.middleButton.TabIndex = 13;
            this.middleButton.Click += new System.EventHandler(this.button_Click);
            // 
            // leftButton
            // 
            this.leftButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.leftButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.leftButton.Location = new System.Drawing.Point(-9, 51);
            this.leftButton.Name = "leftButton";
            this.leftButton.Size = new System.Drawing.Size(56, 48);
            this.leftButton.TabIndex = 12;
            this.leftButton.Click += new System.EventHandler(this.button_Click);
            // 
            // topRightButton
            // 
            this.topRightButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.topRightButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.topRightButton.Location = new System.Drawing.Point(103, 3);
            this.topRightButton.Name = "topRightButton";
            this.topRightButton.Size = new System.Drawing.Size(56, 48);
            this.topRightButton.TabIndex = 11;
            this.topRightButton.Click += new System.EventHandler(this.button_Click);
            // 
            // topButton
            // 
            this.topButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.topButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.topButton.Location = new System.Drawing.Point(47, 3);
            this.topButton.Name = "topButton";
            this.topButton.Size = new System.Drawing.Size(56, 48);
            this.topButton.TabIndex = 10;
            this.topButton.Click += new System.EventHandler(this.button_Click);
            // 
            // topLeftButton
            // 
            this.topLeftButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.topLeftButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.topLeftButton.Location = new System.Drawing.Point(-9, 3);
            this.topLeftButton.Name = "topLeftButton";
            this.topLeftButton.Size = new System.Drawing.Size(56, 48);
            this.topLeftButton.TabIndex = 9;
            this.topLeftButton.Click += new System.EventHandler(this.button_Click);
            // 
            // AnchorChooserControl
            // 
            this.Controls.Add(this.bottomRightButton);
            this.Controls.Add(this.bottomButton);
            this.Controls.Add(this.bottomLeftButton);
            this.Controls.Add(this.rightButton);
            this.Controls.Add(this.middleButton);
            this.Controls.Add(this.leftButton);
            this.Controls.Add(this.topRightButton);
            this.Controls.Add(this.topButton);
            this.Controls.Add(this.topLeftButton);
            this.Name = "AnchorChooserControl";
            this.ResumeLayout(false);

        }
        #endregion
    }
}
