/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
	/// <summary>
	/// Summary description for EffectEnvironmentParameters.
	/// </summary>
	public class EffectEnvironmentParameters
        : IDisposable
	{
		public static EffectEnvironmentParameters DefaultParameters
		{
			get
			{
				return new EffectEnvironmentParameters(ColorBgra.FromBgra(255, 255, 255, 255),
					                                   ColorBgra.FromBgra(0, 0, 0, 255),
					                                   2.0f,
                                                       new PdnRegion());
			}
		}

		private ColorBgra foreColor = ColorBgra.FromBgra(0, 0, 0, 0);
		public ColorBgra ForeColor 
		{
			get
			{
				return foreColor;
			}
		}

		private ColorBgra backColor = ColorBgra.FromBgra(0, 0, 0, 0);
		public ColorBgra BackColor
		{
			get 
			{
				return backColor;
			}
		}

		private float brushWidth = 0.0f;
		public float BrushWidth 
		{
			get 
			{
				return brushWidth;
			}
		}

        private PdnRegion selection;
        private bool haveIntersectedSelection = false;

        /// <summary>
        /// Gets the user's currently selected area.
        /// </summary>
        /// <param name="boundingRect">
        /// The bounding rectangle of the surface you will be rendering to. 
        /// The region returned will be clipped to this bounding rectangle.
        /// </param>
        /// <remarks>
        /// Note that calls to Render() will already be clipped to this selection area. 
        /// This data is only useful when an effect wants to change its rendering based
        /// on what the user has selected. This is used by Auto-Levels to only calculate
        /// new levels based on what the user has selected, and also by Rotate/Zoom to
        /// set the center point of rotation.
        /// </remarks>
        public PdnRegion GetSelection(Rectangle boundingRect)
        {
            if (!haveIntersectedSelection)
            {
                selection.Intersect(boundingRect);
                haveIntersectedSelection = true;
            }

            return selection;
        }

        public EffectEnvironmentParameters(ColorBgra fore, ColorBgra back, float brushWidth, PdnRegion selection)
        {
            this.foreColor = fore;
            this.backColor = back;
            this.brushWidth = brushWidth;
            this.selection = (PdnRegion)selection.Clone();
        }

        ~EffectEnvironmentParameters()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.selection != null)
                {
                    this.selection.Dispose();
                    this.selection = null;
                }
            }
        }
    }
}
