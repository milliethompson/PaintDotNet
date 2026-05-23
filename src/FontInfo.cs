using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Data;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// Carries information about the Font details that we support.
	/// Does not carry text alignment information.
	/// </summary>
	public class FontInfo
	{
		private FontFamily family;
		private float size;
		private FontStyle style;

		public override bool Equals(object obj)
		{
			FontInfo fi = (FontInfo)obj;
			//cast object to FontInfo and if all the same return true
			if((fi.family == family) && (fi.size == size) && (fi.style == style))
				return true;
			else
				return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		
		public FontInfo(FontFamily fontFamily, float size, FontStyle fontStyle)
		{
			this.FontFamily = fontFamily;
			this.Size = size;
			this.FontStyle = fontStyle;
		}

		/// <summary>
		/// The FontFamily property gets and sets the font family.
		/// </summary>
		public FontFamily FontFamily
		{
			get
			{
				return family;
			}
			set
			{
				if (family != value)
				{
					family = value;
				}
			}
		}

		/// <summary>
		/// The Size property gets and sets the size of the text.
		/// </summary>
		public float Size
		{
			get
			{
				return size;
			}
			set
			{
				if (size != value)
				{
					size = value;
				}
			}
		}

		/// <summary>
		/// The FontStyle property gets and sets the font style to bold and or italic and or underline.
		/// </summary>
		public FontStyle FontStyle
		{
			get 
			{
				return style;
			}
			set
			{
				if (style != value)
				{
					style = value;
				}
			}
		}

		public Font CreateFont()
		{	
			// it may be possible that the font style may not be available in the font returned.  But I am not sure.
			// I am checking for this possibility in the textconfig widget getFontInfo fucntion

			if(this.FontFamily.IsStyleAvailable(this.FontStyle))	// check to see if style is availble in family
			{
				return new Font(this.FontFamily, this.Size, this.FontStyle);
			}
			else	// find the style it is available in (coersion)
			{
				FontStyle fs = new FontStyle();
				if(this.FontFamily.IsStyleAvailable(FontStyle.Regular))
					fs = FontStyle.Regular;
				else if(this.FontFamily.IsStyleAvailable(FontStyle.Italic))
					fs = FontStyle.Italic;
				else if(this.FontFamily.IsStyleAvailable(FontStyle.Bold))
					fs = FontStyle.Bold;	
				else if(this.FontFamily.IsStyleAvailable(FontStyle.Strikeout))
					fs = FontStyle.Strikeout;
				else if(this.FontFamily.IsStyleAvailable(FontStyle.Underline))
					fs = FontStyle.Underline;

				return new Font(this.FontFamily, this.Size, fs);
			}
		}
	}
}
