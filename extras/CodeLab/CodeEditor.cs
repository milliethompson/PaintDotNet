/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
	public class CodeEditor : TextBox
	{
        private Timer compileTimer;

		public CodeEditor()
		{
			this.AcceptsReturn = true;
			this.AcceptsTab = true;		
			this.Multiline = true;
			this.ScrollBars = ScrollBars.Vertical;
			this.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));

            this.compileTimer = new Timer();
            this.compileTimer.Interval = 500;
            this.compileTimer.Enabled = false;
            this.compileTimer.Tick += new EventHandler(compileTimer_Tick);
		}

        public event EventHandler CompileTimeHint;
        protected virtual void OnCompileTimeHint()
        {
            if (CompileTimeHint != null)
            {
                CompileTimeHint(this, EventArgs.Empty);
            }
        }

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
			{
				int count = 0;
				int end = this.SelectionStart;
				string text = this.Text, indent = "\r\n";
				for (int i = 0; i < end; i++) 
				{
					if (text[i] == '(' || text[i] == '[' || text[i] == '{') 
					{
						count++;
					}
					if (text[i] == ')' || text[i] == ']' || text[i] == '}') 
					{
						count--;
					}
				}

				while (count-- > 0)
				{
					indent += "    ";
				}

				this.SelectedText = indent;
				e.Handled = true;
			}

            this.compileTimer.Enabled = false;
            this.compileTimer.Enabled = true;

			base.OnKeyPress (e);
		}

		public void HighlightLine(int line) 
		{
        }

        private void compileTimer_Tick(object sender, EventArgs e)
        {
            OnCompileTimeHint();
            this.compileTimer.Enabled = false;
        }
    }
}
