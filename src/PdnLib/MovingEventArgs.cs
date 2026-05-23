using System;
using System.Drawing;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for MovingEventArgs.
	/// </summary>
	public class MovingEventArgs
		: System.EventArgs
	{
		private Rectangle rectangle;
		public Rectangle Rectangle
		{
			get
			{
				return this.rectangle;
			}
			set
			{
				this.rectangle = value;
			}
		}

		public MovingEventArgs(Rectangle rect)
		{
			this.rectangle = rect;
		}
	}
}
