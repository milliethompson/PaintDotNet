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
    /// Summary description for LayerEventArgs.
    /// </summary>
    ///
    public class LayerEventArgs 
        : EventArgs
    {
        Layer layer;

        public Layer Layer
        {
            get
            {
                return layer;
            }
        }

        public LayerEventArgs(Layer layer)
        {
            this.layer = layer;
        }
    }
}
