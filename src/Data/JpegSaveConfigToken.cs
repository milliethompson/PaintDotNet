/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for JpegSaveConfigToken.
    /// </summary>
    [Serializable]
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
            this.quality = quality;
            Validate();
        }

        protected JpegSaveConfigToken(JpegSaveConfigToken cloneMe)
        {
            this.quality = cloneMe.quality;
        }

        public override void Validate()
        {
            if (this.quality < 0 || this.quality > 100)
            {
                throw new ArgumentOutOfRangeException("quality must be 0 to 100, inclusive");
            }

            base.Validate();
        }

        public override object Clone()
        {
            return new JpegSaveConfigToken(this);
        }
    }
}
