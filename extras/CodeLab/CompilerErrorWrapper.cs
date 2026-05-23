/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.CodeDom.Compiler;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Container for a CompilerError object, overrides ToString to a more readable form.
	/// </summary>
	public class CompilerErrorWrapper
	{
		public CompilerError CompilerError = null;

		public override string ToString()
		{
			if (this.CompilerError == null) 
			{
				throw new ArgumentNullException("inner", "inner may not be null");
			}

			return (this.CompilerError.IsWarning  ? "Warning" : "Error")
				+ " at line "
				+ this.CompilerError.Line
				+ ": "
				+ this.CompilerError.ErrorText
				+ " (" + this.CompilerError.ErrorNumber + ")";
		}

	}
}
