using System;
using System.Drawing;

namespace PaintDotNet
{
	public interface IInvalidate
	{
		void Invalidate(Region invalidRegion);
		void Invalidate(Rectangle invalidRect);
	}
}
