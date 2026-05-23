// ReliefEffectConfigDialog.cs
// ui for configuring ReliefEffect parameters. creates 
// a ReliefEffectConfigToken for ReliefEffect
//
// Chris Crosetto

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
    /// Summary description for ReliefEffectConfigDialog.
    /// </summary>
    public class ReliefEffectConfigDialog 
        : AngleChooserConfigDialog
    {
        public ReliefEffectConfigDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();
        }

        // create default config token with angle 45 degress
        protected override void InitialInitToken()
        {
            theEffectToken = new ReliefEffectConfigToken(45);
        }

        /*
        protected override void InitTokenFromDialog()
        {
            ((ReliefEffectConfigToken)EffectToken).Angle = angleChooserControl.Value;
        }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            ReliefEffectConfigToken token = (ReliefEffectConfigToken)effectToken;
            angleChooserControl.Value = token.Angle;
        }
        */

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // ReliefEffectConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(186, 128);
            this.Location = new System.Drawing.Point(0, 0);
			this.Name = "ReliefEffectConfigDialog";
			//this.Icon = Utility.GetIconResource("Icons.ReliefEffect.bmp");
            this.Text = "Relief";

        }
        #endregion
    }
}
