using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for Ruler.
	/// </summary>
	public class Ruler : System.Windows.Forms.UserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private Orientation orientation;

        [DefaultValue(Orientation.Horizontal)]
        public Orientation Orientation
        {
            get
            {
                return orientation;
            }

            set
            {
                if (orientation != value)
                {
                    orientation = value;
                    Invalidate();
                }
            }
        }

        private ScaleFactor scaleFactor;

        public ScaleFactor ScaleFactor
        {
            get
            {
                return scaleFactor;
            }

            set
            {
                if (scaleFactor != value)
                {
                    scaleFactor = value;
                    Invalidate();
                    Update();
                }
            }
        }

        private int offset;

        [DefaultValue(0)]
        public int Offset
        {
            get
            {
                return offset;
            }

            set
            {
                if (offset != value)
                {
                    offset = value;
                    Invalidate();
                }
            }
        }

        private int value;

        [DefaultValue(0)]
        public int Value
        {
            get
            {
                return value;
            }

            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    Invalidate();
                }
            }
        }

        private int majorDivisionLength;

        [DefaultValue(100)]
        public int MajorDivisionLength
        {
            get
            {
                return majorDivisionLength;
            }

            set
            {
                if (majorDivisionLength != value)
                {
                    majorDivisionLength = value;
                    Invalidate();
                }
            }
        }

        private int mediumDivisionCount;

        [DefaultValue(2)]
        public int MediumDivisionCount
        {
            get
            {
                return mediumDivisionCount;
            }

            set
            {
                if (mediumDivisionCount != value)
                {
                    mediumDivisionCount = value;
                    Invalidate();
                }
            }
        }


        private int minorDivisionCount;

        [DefaultValue(10)]
        public int MinorDivisionCount
        {
            get
            {
                return minorDivisionCount;
            }

            set
            {
                if (minorDivisionCount != value)
                {
                    minorDivisionCount = value;
                    Invalidate();
                }
            }
        }

        private Bitmap renderSurface = null;

		public Ruler()
		{
            scaleFactor = new ScaleFactor(1, 1);
            majorDivisionLength = 100;
            mediumDivisionCount = 2;
            minorDivisionCount = 10;
            offset = 0;
            value = 0;
            orientation = Orientation.Horizontal;

            // This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

            renderSurface = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            this.ResizeRedraw = true;
		}

        protected override void OnResize(EventArgs e)
        {
            base.OnResize (e);

            if (renderSurface != null)
            {
                renderSurface.Dispose();
            }

            renderSurface = new Bitmap(Math.Max(1, Width), Math.Max(1, Height), PixelFormat.Format32bppArgb);
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // do nothing so as to avoid flickering effect
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // double buffered rendering prevents any type of flicker
            using (Graphics g = Graphics.FromImage(renderSurface))
            {
                using (PaintEventArgs e2 = new PaintEventArgs(g, e.ClipRectangle))
                {
                    DrawRuler (e2);
                    e.Graphics.DrawImage(renderSurface, 0, 0, renderSurface.Width, renderSurface.Height);
                }
            }
        }

        protected void DrawRuler(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);

            Pen pen = new Pen(ForeColor);
            Brush textBrush = new SolidBrush (ForeColor);
            StringFormat textFormat = new StringFormat();
            float mediumMarkSize = (float)majorDivisionLength / (float)mediumDivisionCount;
            int maxPixel;

            if (orientation == Orientation.Horizontal)
            {
                maxPixel = ScaleFactor.UnscaleScalar(ClientRectangle.Width);
                textFormat.Alignment = StringAlignment.Near;
                textFormat.LineAlignment = StringAlignment.Far;
            }
            else
            {   // orientation == Orientation.Vertical
                maxPixel = ScaleFactor.UnscaleScalar(ClientRectangle.Height);
                textFormat.Alignment = StringAlignment.Near;
                textFormat.LineAlignment = StringAlignment.Near;
                textFormat.FormatFlags |= StringFormatFlags.DirectionVertical;
            }

            int majorMarks = (int)(((float)maxPixel + (float)majorDivisionLength)) / majorDivisionLength;
            int startMajor = (offset / majorDivisionLength) - 1;
            int endMajor = ((offset + maxPixel) / majorDivisionLength) + 1;

            for (int major = startMajor; major < endMajor; ++major)
            {
                int majorMarkPos = (major * majorDivisionLength) - offset;
                string majorText = (major * majorDivisionLength).ToString();

                if (orientation == Orientation.Horizontal)
                {
                    Point a = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos, ClientRectangle.Top));
                    Point b = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos, ClientRectangle.Bottom));
                    e.Graphics.DrawLine(pen, a, b);
                    e.Graphics.DrawString(majorText, Font, textBrush, new PointF(a.X, b.Y), textFormat);
                }
                else
                if (orientation == Orientation.Vertical)
                {
                    Point a = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left, ClientRectangle.Top + majorMarkPos));
                    Point b = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Right, ClientRectangle.Top + majorMarkPos));
                    e.Graphics.DrawLine(pen, a, b);
                    e.Graphics.DrawString(majorText, Font, textBrush, new PointF(a.X, b.Y), textFormat);
                }

                for (int medium = 0; medium < mediumDivisionCount; ++medium)
                {
                    int mediumMarkPos = (majorDivisionLength * medium) / mediumDivisionCount;

                    if (orientation == Orientation.Horizontal)
                    {
                        Point a = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos + mediumMarkPos, ClientRectangle.Top));
                        Point b = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos + mediumMarkPos, ClientRectangle.Top + (ClientRectangle.Height / 2)));
                        e.Graphics.DrawLine (pen, a, b);
                    }
                    else
                    if (orientation == Orientation.Vertical)
                    {
                        Point a = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left, ClientRectangle.Top + majorMarkPos + mediumMarkPos));
                        Point b = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left + (ClientRectangle.Width / 2), ClientRectangle.Top + majorMarkPos + mediumMarkPos));
                        e.Graphics.DrawLine (pen, a, b);
                    }
                }

                for (int minor = 0; minor < minorDivisionCount; ++minor)
                {
                    int minorMarkPos = (majorDivisionLength * minor) / minorDivisionCount;

                    if (orientation == Orientation.Horizontal)
                    {
                        Point a = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos + minorMarkPos, ClientRectangle.Top));
                        Point b = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + majorMarkPos + minorMarkPos, ClientRectangle.Top + (ClientRectangle.Height / 4)));
                        e.Graphics.DrawLine (pen, a, b);
                    }
                    else
                    if (orientation == Orientation.Vertical)
                    {
                        Point a = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left, ClientRectangle.Top + majorMarkPos + minorMarkPos));
                        Point b = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left + (ClientRectangle.Width / 4), ClientRectangle.Top + majorMarkPos + minorMarkPos));
                        e.Graphics.DrawLine (pen, a, b);
                    }
                }
            }

            if (orientation == Orientation.Horizontal)
            {
                // draw Value
                Point a = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + Value - Offset, ClientRectangle.Top));
                Point b = ScaleFactor.ScalePointJustX(new Point(ClientRectangle.Left + Value - Offset, ClientRectangle.Bottom));
                e.Graphics.DrawLine (pen, a, b);

                // draw border
                e.Graphics.DrawLine (pen, new Point(ClientRectangle.Left, ClientRectangle.Bottom - 1),
                                          new Point(ClientRectangle.Right - 1, ClientRectangle.Bottom - 1));
            }
            else
            if (orientation == Orientation.Vertical)
            {
                // draw Value
                Point a = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Left, ClientRectangle.Top + Value - Offset));
                Point b = ScaleFactor.ScalePointJustY(new Point(ClientRectangle.Right, ClientRectangle.Top + Value - Offset));
                e.Graphics.DrawLine (pen, a, b);

                // draw border
                e.Graphics.DrawLine (pen, new Point(ClientRectangle.Right - 1, ClientRectangle.Top),
                                          new Point(ClientRectangle.Right - 1, ClientRectangle.Bottom - 1));
            }

            textBrush.Dispose();
            textFormat.Dispose();
            pen.Dispose();
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
