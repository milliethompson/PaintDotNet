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
    /// Summary description for LevelsEffectConfigToken.
    /// </summary>
    public class LevelsEffectConfigToken 
        : EffectConfigToken
    {
        private UnaryPixelOps.Level levels = null;

        public UnaryPixelOps.Level Levels
        {
            get 
            {
                return levels;
            }

            set 
            {
                levels = value;
            }
        }

        public LevelsEffectConfigToken()
        {
            levels = new UnaryPixelOps.Level();
        }

        public override object Clone()
        {
            LevelsEffectConfigToken cpy = new LevelsEffectConfigToken();
            cpy.levels = (UnaryPixelOps.Level)this.levels.Clone();
            return cpy;
        }
    }
}
