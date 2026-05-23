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
    /// Abstract base class that all "user" blend ops derive from.
    /// These ops are available in the UI for a user to choose from
    /// in order to configure the blending properties of a Layer.
    /// 
    /// See UserBlendOps.cs for guidelines on implementation.
    /// </summary>
    [Serializable]
    public abstract class UserBlendOp
        : BinaryPixelOp
    {
        public override string ToString()
        {
            return Utility.GetStaticName(this.GetType());
        }
    }
}
