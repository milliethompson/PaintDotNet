/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for EmbossEffectConfigDialog.
    /// </summary>
    public class EmbossEffectConfigDialog 
        : AngleChooserConfigDialog
    {
        public EmbossEffectConfigDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();
        }

        // create default config token with angle 45 degress
        protected override void InitialInitToken()
        {
            theEffectToken = new EmbossEffectConfigToken(45);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // EmbossEffectConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(186, 128);
            this.Location = new System.Drawing.Point(0, 0);
			this.Name = "EmbossEffectConfigDialog";
			//this.Icon = Utility.GetIconResource("Icons.EmbossEffect.bmp");
            this.Text = "Emboss";

        }
        #endregion
    }
}
