using System;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for LayerEventArgs.
	/// </summary>
	///
	public class LayerEventArgs : EventArgs
	{
		Layer l;

		public Layer Layer
		{
			get
			{
				return l;
			}
		}

		public LayerEventArgs(Layer l)
		{
			this.l = l;
		}
	}
}
