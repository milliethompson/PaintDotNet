using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for ColorGradientControl.
	/// </summary>
	public class ColorGradientEventArgs : EventArgs
	{
		private int index;
		public int Index 
		{
			get 
			{
				return index;
			} 
			set 
			{
				index = value;
			}
		}
		public ColorGradientEventArgs(int index) 
		{
			this.index = index;
		}
	}

}
