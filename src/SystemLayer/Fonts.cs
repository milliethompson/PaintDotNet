/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;

namespace PaintDotNet.SystemLayer
{
	/// <summary>
	/// Static methods related to font handling.
	/// </summary>
	public sealed class Fonts
	{
        /// <summary>
        /// Determines whether a font uses the 'symbol' character set.
        /// </summary>
        /// <remarks>
        /// Symbol fonts do not typically contain glyphs that represent letters of the alphabet.
        /// Instead they might contain pictures and symbols. As such, they are not useful for
        /// drawing text. Which means you can't use a symbol font to write out its own name for
        /// illustrative purposes.
        /// </remarks>
        public static bool IsSymbolFont(Font font)
        {
            NativeStructs.LOGFONT logFont = new NativeStructs.LOGFONT();
            font.ToLogFont(logFont);
            return logFont.lfCharSet == NativeConstants.SYMBOL_CHARSET;
        }

		private Fonts()
		{
		}
	}
}
