using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Provided for compatibility with v1.1
    /// </summary>
    public class MosaicEffectConfigDialog 
        : AmountEffectConfigDialog
    {
        public MosaicEffectConfigDialog()
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            this.SliderMinimum = 1;
            this.SliderMaximum = 100;
            this.SliderLabel = "Cell Size";
            this.SliderUnitsName = "pixels";
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MosaicEffectConfigDialog
            // 
			this.Name = "MosaicEffectConfigDialog";
			//this.Icon = Utility.GetIconResource("Icons.MosaicEffect.bmp");
            this.Text = "Mosaic";
            this.ResumeLayout(false);

        }
        #endregion

        protected override void InitialInitToken()
        {
            base.InitialInitToken();
            theEffectToken = new MosaicEffectConfigToken(2);
        }
    }
}

