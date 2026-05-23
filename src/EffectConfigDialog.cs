using System;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for EffectConfigDialog.
	/// </summary>
	public class EffectConfigDialog
        : PdnBaseForm
	{
        public EffectConfigDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // 
            // EffectConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EffectConfigDialog";
            this.Opacity = 0.8;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;

        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            if (AcceptButton is Control)
            {
                ((Control)AcceptButton).Select();
            }
        }

    
        public event EventHandler EffectTokenChanged;
        protected void OnEffectTokenChanged()
        {
            if (EffectTokenChanged != null)
            {
                EffectTokenChanged(this, EventArgs.Empty);
            }
        }

        public void PerformTokenChanged()
        {
            OnEffectTokenChanged();
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the EffectToken property
        /// and use them to properly initialize the dialog's user interface elements.
        /// </summary>
        protected virtual void InitDialogFromToken()
        {
            throw new InvalidOperationException("InitDialogFromToken was not initialized, or the derived method called the base method");
        }

        protected Effect effect;
        public Effect Effect
        {
            get
            {
                return effect;
            }

            set
            {
                effect = value;
            }
        }

        protected EffectConfigToken effectToken;
        public EffectConfigToken EffectToken
        {
            get
            {
                return effectToken;
            }

            set
            {
                effectToken = value;
                InitDialogFromToken();
                OnEffectTokenChanged();
            }
        }
    }
}
