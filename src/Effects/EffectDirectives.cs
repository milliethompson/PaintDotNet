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
    /// Flags that specify important information that an effect rendering host
    /// must be aware of and take into consideration when executing a particular
    /// effect.
    /// </summary>
    [Flags]
    public enum EffectDirectives
    {
        /// <summary>
        /// No special directive.
        /// </summary>
        None = 0,

        /// <summary>
        /// Specifies that the effect must only execute in one thread at once.
        /// Normally multiple threads are used in order to increase performance
        /// (esp. on dual processor / dual core systems).
        /// </summary>
        SingleThreaded = 1
    }
}
