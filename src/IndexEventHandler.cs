using System;

namespace PaintDotNet
{
    /// <summary>
    /// Declares a delegate type for an event that needs a single integer, interpreted
    /// as an index, as event information.
    /// </summary>
	public delegate void IndexEventHandler(object sender, IndexEventArgs ce);
}