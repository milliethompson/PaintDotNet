using System;
using System.Drawing;

namespace PaintDotNet
{
    /// <summary>
    /// Adapted from: 
    /// "A Primer on Building a Color Picker User Control with GDI+ in Visual Basic .NET or C#"
    /// http://www.msdnaa.net/Resources/display.aspx?ResID=2460
    /// 
    /// This class is only used by the ColorsForm and ColorWheel. Nothing else in this program
    /// should be using it!
    /// </summary>
    public struct HsvColor
    {
        // All values are between 0 and 255.
        public int Hue;
        public int Saturation;
        public int Value;

        public static bool operator== (HsvColor lhs, HsvColor rhs)
        {
            if ((lhs.Hue == rhs.Hue) &&
                (lhs.Saturation == rhs.Saturation) &&
                (lhs.Value == rhs.Value))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator!= (HsvColor lhs, HsvColor rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object obj)
        {
            return this == (HsvColor)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();//(Hue + (Saturation << 8) + (Value << 16)).GetHashCode();
        }

        public HsvColor(int H, int S, int V) 
        {
            Hue = H;
            Saturation = S;
            Value = V;
        }

		public static HsvColor FromColor(Color color)
		{
			RgbColor rgb = new RgbColor(color.R, color.G, color.B);
			return rgb.ToHsv();
		}

        public Color ToColor()
        {
            RgbColor rgb = ToRgb();
            return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        public RgbColor ToRgb() 
        {
            // HsvColor contains values scaled as in the color wheel:
            // that is, all from 0 to 255. 

            // for ( this code to work, HsvColor.Hue needs
            // to be scaled from 0 to 360 (it//s the angle of the selected
            // point within the circle). HsvColor.Saturation and HsvColor.value must be 
            // scaled to be between 0 and 1.

            double h;
            double s;
            double v;

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360. Saturation
            // and value scale to be between 0 and 1.
            h = ((double) Hue / 255 * 360) % 360;
            s = (double) Saturation / 255;
            v = (double) Value / 255;

            if ( s == 0 ) 
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            } 
            else 
            {
                double p;
                double q;
                double t;

                double fractionalSector;
                int sectorNumber;
                double sectorPos;

                // The color wheel consists of 6 sectors.
                // Figure out which sector you//re in.
                sectorPos = h / 60;
                sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color. 
                p = v * (1 - s);
                q = v * (1 - (s * fractionalSector));
                t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber) 
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }
            // return an RgbColor structure, with values scaled
            // to be between 0 and 255.
            return new RgbColor((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

        public override string ToString() 
        {
            return String.Format("({0}, {1}, {2})", Hue, Saturation, Value);
        }
    }
}
