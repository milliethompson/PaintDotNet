/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

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
