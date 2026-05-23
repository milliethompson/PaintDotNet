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
    /// Summary description for JpegSaveConfigToken.
    /// </summary>
    public class JpegSaveConfigToken
        : SaveConfigToken
    {
        private int quality;
        public int Quality
        {
            get
            {
                return quality;
            }

            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException("quality must be 0 to 100, inclusive");
                }

                this.quality = value;
            }
        }

        public JpegSaveConfigToken(int quality)
        {
            if (quality < 0 || quality > 100)
            {
                throw new ArgumentOutOfRangeException("quality must be 0 to 100, inclusive");
            }
            
            this.quality = quality;
        }

        protected JpegSaveConfigToken(JpegSaveConfigToken cloneMe)
        {
            this.quality = cloneMe.quality;
        }

        public override object Clone()
        {
            return new JpegSaveConfigToken(this);
        }
    }
}
