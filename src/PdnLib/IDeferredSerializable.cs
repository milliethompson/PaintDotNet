/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.Serialization;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for IDeferredSerializable.
	/// </summary>
	public interface IDeferredSerializable
        : ISerializable
	{
        void FinishSerialization(Stream output, DeferredFormatter context);
        void FinishDeserialization(Stream input, DeferredFormatter context);
	}
}
