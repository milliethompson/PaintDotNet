// EdgeDetectEffectConfigDialog.cs
// ui for configuring EdgeDetectEffect parameters. creates 
// a EdgeDetectEffectConfigToken for EdgeDetectEffect
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
    /// Summary description for EdgeDetectEffectConfigDialog.
    /// </summary>
    public class EdgeDetectConfigDialog 
        : AngleChooserConfigDialog
    {
        public EdgeDetectConfigDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();
        }

        // create default config token with angle 45 degress
        protected override void InitialInitToken()
        {
            theEffectToken = new EdgeDetectConfigToken(45);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // EdgeDetectConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(186, 128);
            this.Location = new System.Drawing.Point(0, 0);
			this.Name = "EdgeDetectConfigDialog";
			//this.Icon = Utility.GetIconResource("Icons.EdgeDetectEffect.bmp");
            this.Text = "Edge Detect";

        }
        #endregion
    }
}
