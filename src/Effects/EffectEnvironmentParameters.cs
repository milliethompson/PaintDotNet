using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for EffectEnvironmentParameters.
	/// </summary>
	public class EffectEnvironmentParameters
	{
		private ColorBgra foreColor = ColorBgra.FromBgra(0, 0, 0, 0);
		public ColorBgra ForeColor 
		{
			get
			{
				return foreColor;
			}
		}

		private ColorBgra backColor = ColorBgra.FromBgra(0, 0, 0, 0);
		public ColorBgra BackColor
		{
			get 
			{
				return backColor;
			}
		}

		private float brushWidth = 0.0f;
		public float BrushWidth 
		{
			get 
			{
				return brushWidth;
			}
		}

		public EffectEnvironmentParameters() 
		{
		}

		public EffectEnvironmentParameters(ColorBgra fore, ColorBgra back, float brushWidth) 
		{
			this.foreColor = fore;
			this.backColor = back;
			this.brushWidth = brushWidth;
		}
	}
}
