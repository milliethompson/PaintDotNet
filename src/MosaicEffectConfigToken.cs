using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MosaicEffectConfigToken.
	/// </summary>
	public class MosaicEffectConfigToken
		: EffectConfigToken
	{
		public int CellSize
		{
			get
			{
				return cellSize;
			}
			set
			{
				cellSize = value;
			}
		}
		private int cellSize;

		public override object Clone()
		{
			return new MosaicEffectConfigToken(this);
		}

		public MosaicEffectConfigToken(int newCellSize)
			: base()
		{
			this.CellSize = newCellSize;
		}

		protected MosaicEffectConfigToken(MosaicEffectConfigToken copyThis)
			: base(copyThis)
		{
			this.CellSize = copyThis.CellSize;
		}
	}
}
