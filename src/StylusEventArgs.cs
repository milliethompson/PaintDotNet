using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet
{
	/// <summary>
	/// This class contains information about the pointer's position,
	/// buttons, wheel rotation, and pressure, if applicable.
	/// </summary>

	public class StylusEventArgs : MouseEventArgs
	{
		private PointF position;
		public float fX 
		{
			get 
			{
				return position.X;
			}
		}
		public float fY 
		{
			get 
			{
				return position.Y;
			}
		}
		private float pressure;
		public float Pressure 
		{
			get
			{
				return pressure;
			}
		}
		/// <summary>
		/// Constructs a new StylusEventArgs object
		/// </summary>
		/// <param name="button">Which button was pressed</param>
		/// <param name="clicks">The number of times the button was pressed</param>
		/// <param name="x">The horizontal position of the pointer</param>
		/// <param name="y">The vertical position of the pointer</param>
		/// <param name="delta">The number of detents the wheel has rotated, signed</param>
		public StylusEventArgs(MouseButtons myButton, int myClicks, float myX, float myY, int myDelta)
			: base(myButton, myClicks, (int)myX, (int)myY, myDelta)
		{
			this.position = new PointF(myX, myY);
			this.pressure = 1.0f;
		}
		/// <summary>
		/// Constructs a new StylusEventArgs object
		/// </summary>
		/// <param name="button">Which button was pressed</param>
		/// <param name="clicks">The number of times the button was pressed</param>
		/// <param name="x">The horizontal position of the pointer</param>
		/// <param name="y">The vertical position of the pointer</param>
		/// <param name="delta">The number of detents the wheel has rotated, signed</param>
		public StylusEventArgs(MouseEventArgs e)
			: base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
		{
			this.position = new PointF(e.X, e.Y);
			this.pressure = 1.0f;
		}
		/// <summary>
		/// Constructs a new StylusEventArgs object
		/// </summary>
		/// <param name="button">Which button was pressed</param>
		/// <param name="clicks">The number of times the button was pressed</param>
		/// <param name="x">The horizontal position of the pointer</param>
		/// <param name="y">The vertical position of the pointer</param>
		/// <param name="delta">The number of detents the wheel has rotated, signed</param>
		/// <param name="pressure">The force applied with the pointer, as a fraction of the maximum</param>
		public StylusEventArgs(MouseButtons myButton, int myClicks, float myX, float myY, int myDelta, float myPressure)
			: base(myButton, myClicks, (int)myX, (int)myY, myDelta)
		{
			this.position = new PointF(myX, myY);
			this.pressure = myPressure;
		}
	}
}
