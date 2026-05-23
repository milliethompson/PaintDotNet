/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for EffectConfigToken.
    /// </summary>
    [Serializable]
    public abstract class EffectConfigToken
        : ICloneable
    {
        #region ICloneable Members
        /// <summary>
        /// This should simply call "new myType(this)" ... do not call base class'
        /// implementation of Clone, as this is handled by the constructors.
        /// </summary>
        public abstract object Clone();
        #endregion
        
        public EffectConfigToken()
        {
        }

        protected EffectConfigToken(EffectConfigToken copyMe)
        {
        }
    }
}

