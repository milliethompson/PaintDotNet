using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MosaicEffect.
	/// </summary>
	public class MosaicEffect 
		: Effect, IConfigurableEffect
	{
		public MosaicEffect() 
			: base("Mosaic", "Tiles a Picture", null)
		{
			//
			// TODO: Add constructor logic here
			//
		}

		public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, Rectangle roi)
		{
			throw new InvalidOperationException("MosaicEffect must be used via the other Render overload");
			// Render the Effect all over dat bish
		}

		private ColorBgra RenderPixel(int x, int y, RenderArgs src, int cellSize)
		{
			Rectangle cell = GetCellBox(x,y,src, cellSize);
			
			int left = cell.Left;
			int right = cell.Right - 1;
			int bottom = cell.Bottom - 1;
			int top = cell.Top;

			Point topLeft = new Point(left, top);
			Point topRight = new Point(right, top);
			Point bottomLeft = new Point(left, bottom);
			Point bottomRight = new Point(right, bottom); 

			// Check for Overlapping Points
			if(!Utility.IsPointInRectangle(topLeft, src.Bounds))
			{
				topLeft = new Point(src.Bounds.Left, src.Bounds.Top);
			}
			if(!Utility.IsPointInRectangle(topRight, src.Bounds))
			{
				topRight = new Point(src.Bounds.Right - 1, src.Bounds.Top);
			}
			if(!Utility.IsPointInRectangle(bottomLeft, src.Bounds))
			{
				bottomLeft = new Point(src.Bounds.Left, src.Bounds.Bottom - 1);
			}
			if(!Utility.IsPointInRectangle(bottomRight, src.Bounds))
			{
				bottomRight = new Point(src.Bounds.Right - 1, src.Bounds.Bottom - 1);
			}

			ColorBgra colorTopLeft     = src.Surface[topLeft.X, topLeft.Y];
			ColorBgra colorTopRight    = src.Surface[topRight.X, topRight.Y];
			ColorBgra colorBottomLeft  = src.Surface[bottomLeft.X, bottomRight.Y];
			ColorBgra colorBottomRight = src.Surface[bottomRight.X, bottomRight.Y];

            byte a = (byte)((colorTopLeft.a + colorTopRight.a + colorBottomLeft.a + colorBottomRight.a) / 4);
			byte r = (byte)((colorTopLeft.r + colorTopRight.r + colorBottomLeft.r + colorBottomRight.r) / 4);
			byte g = (byte)((colorTopLeft.g + colorTopRight.g + colorBottomLeft.g + colorBottomRight.g) / 4);
			byte b = (byte)((colorTopLeft.b + colorTopRight.b + colorBottomLeft.b + colorBottomRight.b) / 4);	
		
			return ColorBgra.FromBgra((byte)b,(byte)g,(byte)r,(byte)a);
		}

		private Rectangle GetCellBox(int x, int y, RenderArgs src, int cellSize)
		{
			int widthBoxNum  = x % cellSize;
			int heightBoxNum = y % cellSize;
			Point leftUpper = new Point(x - widthBoxNum, y - heightBoxNum);
			Rectangle returnMe = new Rectangle(leftUpper, new Size(cellSize, cellSize));
			return returnMe;
		}
		#region IConfigurableEffect Members

		public EffectConfigDialog CreateConfigDialog()
		{
			return new MosaicEffectConfigDialog();
		}

		void PaintDotNet.IConfigurableEffect.Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, Region roi)
		{
			MosaicEffectConfigToken mect = (MosaicEffectConfigToken)properties;

			foreach (RectangleF rectF in roi.GetRegionScans(Utility.IdentityMatrix))
			{
				Rectangle rect = Rectangle.Truncate(rectF);

				for(int x = rect.Left; x < rect.Right; x++)
				{
					for(int y = rect.Top; y < rect.Bottom; y++)
					{
						dstArgs.Surface[x,y] = RenderPixel(x,y,srcArgs, mect.CellSize);
					}
				}
			}
		}

		#endregion
	}

}
