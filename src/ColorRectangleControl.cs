using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ColorRectangleControl.
	/// </summary>
	public class ColorRectangleControl : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private Bitmap renderSurface = null;

		private Color rectangleColor;
		public Color RectangleColor
		{
			get
			{
				return rectangleColor;
			}

			set
			{
				rectangleColor = value;
				Invalidate();
			}
		}

		public ColorRectangleControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
			this.ResizeRedraw = true;
		}

		private void DrawColorRectangle(Graphics g, Rectangle rect, Color color)
		{
			Brush colorBrush = new SolidBrush(Color.FromArgb(255, color.R, color.G, color.B));
		
			g.FillRectangle(Brushes.Black, rect);
			g.FillRectangle(Brushes.White, Rectangle.Inflate(rect, -1, -1));
			g.FillRectangle(colorBrush, Rectangle.Inflate(rect, -2, -2));
		}

        protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

            using (Graphics g = Graphics.FromImage(renderSurface))
            {
                DrawColorRectangle(g, this.ClientRectangle, rectangleColor);
            }

            e.Graphics.DrawImage(renderSurface, 0, 0, renderSurface.Width, renderSurface.Height);
        }

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
            if (renderSurface == null || renderSurface.Size != this.Size)
            {
                renderSurface = new Bitmap(Width, Height);

            }

            pevent.Graphics.DrawImage(renderSurface, 0, 0, renderSurface.Width, renderSurface.Height);
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
