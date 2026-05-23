using System;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Use this to mark that a particular effect should only use 1 thread
	/// for execution. This is especially important if you want to use GDI+
	/// (that is, System.Drawing) facilities for drawing.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class SingleThreadedEffectAttribute
		: Attribute
	{
		public SingleThreadedEffectAttribute()
		{
		}
	}
}
