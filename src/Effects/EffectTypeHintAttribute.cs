/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Tags an effect with a specific EffectTypeHint.
	/// </summary>
	public class EffectTypeHintAttribute
        : Attribute
	{
        private EffectTypeHint effectTypeHint;
        public EffectTypeHint EffectTypeHint
        {
            get
            {
                return effectTypeHint;
            }
        }

		public EffectTypeHintAttribute(EffectTypeHint effectTypeHint)
		{
            this.effectTypeHint = effectTypeHint;
		}
	}
}
