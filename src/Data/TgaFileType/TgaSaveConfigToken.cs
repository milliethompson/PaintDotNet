/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using System;

namespace PaintDotNet.Data.TgaFileType
{
    /// <summary>
    /// Summary description for TgaSaveConfigToken.
    /// </summary>
    [Serializable]
    public class TgaSaveConfigToken
        : SaveConfigToken
    {
        public override object Clone()
        {
            return new TgaSaveConfigToken(this);
        }

        private int bitDepth;
        public int BitDepth
        {
            get
            {
                return bitDepth;
            }

            set
            {
                if (value != 16 && value != 24 && value != 32)
                {
                    throw new NotSupportedException("bitDepth not one of { 16, 24, 32 }");
                }

                this.bitDepth = value;
            }
        }

        private bool rleCompress;
        public bool RleCompress
        {
            get
            {
                return this.rleCompress;
            }

            set
            {
                this.rleCompress = value;
            }
        }

        public TgaSaveConfigToken(int bitDepth, bool rleCompress)
        {
            this.BitDepth = bitDepth;
            this.RleCompress = rleCompress;
        }

        protected TgaSaveConfigToken(TgaSaveConfigToken copyMe)
        {
            this.bitDepth = copyMe.bitDepth;
            this.rleCompress = copyMe.rleCompress;
        }

        public override void Validate()
        {
            if (this.bitDepth != 16 && this.bitDepth != 24 && this.bitDepth != 32)
            {
                throw new NotSupportedException("bitDepth not one of { 16, 24, 32 }");
            }

            base.Validate();
        }

    }
}
