using System;
using System.Runtime.Serialization;

namespace PaintDotNet
{
    /// <summary>
    /// This is the base exception for all Paint.NET exceptions.
    /// </summary>
    public class PdnException
        : ApplicationException
    {
        public PdnException()
            : base()
        {
        }

        public PdnException(string message)
            : base(message)
        {
        }

        public PdnException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PdnException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
