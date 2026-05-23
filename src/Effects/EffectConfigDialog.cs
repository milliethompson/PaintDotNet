using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet.Effects
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
            InitialInitToken();
            this.Opacity = 0.9;
        }

        private void InitializeComponent()
        {
            // 
            // EffectConfigDialog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(282, 253);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EffectConfigDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "EffectConfigDialog";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad (e);
            InitDialogFromToken();
            UpdateToken();
        }

        [Browsable(false)]
        public event EventHandler EffectTokenChanged;
        protected void OnEffectTokenChanged()
        {
            if (EffectTokenChanged != null)
            {
                EffectTokenChanged(this, EventArgs.Empty);
            }
        }

        public void UpdateToken()
        {
            InitTokenFromDialog();
            OnEffectTokenChanged();
        }

        /// <summary>
        /// This method must be overriden in the derived classes.
        /// In this you initialize the default values for the token, and
        /// thus the default values for the dialog box.
        /// </summary>
        protected virtual void InitialInitToken()
        {
            //throw new InvalidOperationException("InitialInitToken was not implemented, or the derived method called the base method");
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the given EffectToken
        /// and use them to properly initialize the dialog's user interface elements.
        /// </summary>
        protected virtual void InitDialogFromToken(EffectConfigToken effectToken)
        {
            //throw new InvalidOperationException("InitDialogFromToken was not implemented, or the derived method called the base method");
        }

        protected void InitDialogFromToken()
        {   // If we don't check for null, we get awful errors in the designer.
            // Good idea to check for that anyway, yeah?
            if (theEffectToken != null)
            {
                InitDialogFromToken((EffectConfigToken)theEffectToken.Clone());
            }
        }

        /// <summary>
        /// This method must be overridden in derived classes.
        /// In this method you must take the values from the dialog box
        /// and use them to properly initialize theEffectToken.
        /// </summary>
        protected virtual void InitTokenFromDialog()
        {
            //throw new InvalidOperationException("InitTokenFromDialog was not implemented, or the derived method called the base method");
        }

        private Effect effect;

        [Browsable(false)]
        public Effect Effect
        {
            get
            {
                return effect;
            }

            set
            {
                effect = value;

                if (effect.Image != null)
                {
                    this.Icon = Utility.ImageToIcon(effect.Image, Color.FromArgb(192, 192, 192));
                }
                else
                {
                    this.Icon = null;
                }
            }
        }

        protected EffectConfigToken theEffectToken;

        [Browsable(false)]
        public EffectConfigToken EffectToken
        {
            get
            {
                return theEffectToken;
            }

            set
            {
                theEffectToken = value;
                OnEffectTokenChanged();
                InitDialogFromToken();
            }
        }
    }
}
