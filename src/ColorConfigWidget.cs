using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ColorConfigWidget.
	/// </summary>
	public class ColorConfigWidget 
		: System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		private Color userForeColor;
		public Color UserForeColor
		{
			get
			{
				return userForeColor;
			}

			set
			{
				userForeColor = value;
				Invalidate();
			}
		}

		private Color userBackColor;
		public Color UserBackColor
		{
			get
			{
				return userBackColor;
			}

			set
			{
				userBackColor = value;
				Invalidate();
			}
		}

        private static Color InvertColor(Color c)
        {
            return Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B);
        }

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint (e);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Far;
            e.Graphics.FillRectangle(new SolidBrush(userForeColor), 0, 0, Width / 2, Height);
            e.Graphics.FillRectangle(new SolidBrush(userBackColor), Width / 2, 0, Width / 2, Height);
            e.Graphics.DrawString("Foreground", this.Font, new SolidBrush(InvertColor(userForeColor)), new PointF((float)Width / 4, (float)Height), sf);
            e.Graphics.DrawString("Background", this.Font, new SolidBrush(InvertColor(userBackColor)), new PointF(3 * ((float)Width / 4), (float)Height), sf);
        } 

		public ColorConfigWidget()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			userForeColor = Color.Black;
			userBackColor = Color.White;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
