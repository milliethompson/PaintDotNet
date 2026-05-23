using System;

namespace PaintDotNet
{
	/// <summary>
	/// This exception is thrown when there is a discrepancy in the size of at
	/// least 2 objects that must be worked on together. e.g. all the bitmaps
	/// must be the same size (width + height).
	/// </summary>
	public class SizeMismatchException
		: ArgumentException
	{
        public SizeMismatchException()
            : base()
        {
        }

        public SizeMismatchException(string message)
            : base(message)
        {
        }

        public SizeMismatchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SizeMismatchException(string message, string paramName)
            : base(message, paramName)
        {
        }

        public SizeMismatchException(string message, string paramName, Exception innerException)
            : base(message, paramName, innerException)
        {
        }
	}
}
