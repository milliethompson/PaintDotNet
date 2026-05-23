/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
	/// <summary>
	/// This interface is used to generate FileType instances.
	/// The FileTypes class, when requested for a list of FileType instances,
	/// will use reflection to search for classes that implement this interface 
	/// and then call their GetFileTypeInstances() methods.
	/// ... This interface may be useful if we want to have FileType plugins, no?
	/// </summary>
	public interface IFileTypeFactory
	{
        FileType[] GetFileTypeInstances();
	}
}
