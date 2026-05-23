using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for FloatingToolForm.
    /// </summary>
    public class FloatingToolForm 
        : PdnBaseForm
    {
        private System.ComponentModel.IContainer components = null;

        private ControlEventHandler controlAddedDelegate;
        private ControlEventHandler controlRemovedDelegate;
        private KeyEventHandler keyUpDelegate;

        public FloatingToolForm()
        {
            this.KeyPreview = true;
            controlAddedDelegate = new ControlEventHandler(ControlAddedHandler);
            controlRemovedDelegate = new ControlEventHandler(ControlRemovedHandler);
            keyUpDelegate = new KeyEventHandler(KeyUpHandler);

            this.ControlAdded += controlAddedDelegate;
            this.ControlRemoved += controlRemovedDelegate;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated (e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (Utility.IsArrowKey(keyData))
            {
                KeyEventArgs kea = new KeyEventArgs(keyData);

                switch (msg.Msg)
                {
                    case NativeMethods.WmConstants.WM_KEYDOWN:
                        this.OnKeyDown(kea);
                        return kea.Handled;

                        /*
                    case NativeMethods.WmConstants.WM_KEYUP:
                        this.OnKeyUp(kea);
                        return kea.Handled;
                        */
                }
            }

            return base.ProcessCmdKey (ref msg, keyData);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            if ( disposing )
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // FloatingToolForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(292, 271);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FloatingToolForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "FloatingToolForm";

        }
        #endregion

        private void ControlAddedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded += controlAddedDelegate;
            e.Control.ControlRemoved += controlRemovedDelegate;
            e.Control.KeyUp += keyUpDelegate;
        }

        private void ControlRemovedHandler(object sender, ControlEventArgs e)
        {
            e.Control.ControlAdded -= controlAddedDelegate;
            e.Control.ControlRemoved -= controlRemovedDelegate;
            e.Control.KeyUp -= keyUpDelegate;
        }

        private void KeyUpHandler(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                this.OnKeyUp(e);
            }
        }
    }
}
