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

namespace PaintDotNet.Effects
{
    public class RedEyeRemoveEffectDialog 
        : PaintDotNet.Effects.TwoAmountsConfigDialog
    {
        private System.Windows.Forms.Label usageHintLabel;
        private System.ComponentModel.IContainer components = null;

        public RedEyeRemoveEffectDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.usageHintLabel.Text = PdnResources.GetString("RedEyeRemoveEffectDialog.UsageHintLabel.Text");
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.usageHintLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(96, 179);
            this.okButton.Name = "okButton";
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(177, 179);
            this.cancelButton.Name = "cancelButton";
            // 
            // usageHintLabel
            // 
            this.usageHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.usageHintLabel.Location = new System.Drawing.Point(8, 147);
            this.usageHintLabel.Name = "usageHintLabel";
            this.usageHintLabel.Size = new System.Drawing.Size(240, 32);
            this.usageHintLabel.TabIndex = 9;
            // 
            // RedEyeRemoveEffectDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(258, 208);
            this.Controls.Add(this.usageHintLabel);
            this.Location = new System.Drawing.Point(0, 0);
            this.Name = "RedEyeRemoveEffectDialog";
            this.Controls.SetChildIndex(this.okButton, 0);
            this.Controls.SetChildIndex(this.cancelButton, 0);
            this.Controls.SetChildIndex(this.usageHintLabel, 0);
            this.ResumeLayout(false);

        }
        #endregion
    }
}

