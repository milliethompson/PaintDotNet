/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// This effect was graciously provided by David Issel, aka BoltBait. His original
// copyright and license (MIT License) are reproduced below.

/*
PortraitEffect.cs 
Copyright (c) 2007 David Issel 
Contact Info: BoltBait@hotmail.com http://www.BoltBait.com 

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE. 
*/

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Collections;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public sealed class SoftenPortraitEffect
        : Effect
    {
        public static string StaticName
        {
            get
            {
                return PdnResources.GetString("SoftenPortraitEffect.Name");
            }
        }

        public static Image StaticIcon
        {
            get
            {
                return PdnResources.GetImage("Icons.SoftenPortraitEffectIcon.png");
            }
        }

        private BlurEffect blurEffect = new BlurEffect();
        private UnaryPixelOps.Desaturate desaturateOp = new UnaryPixelOps.Desaturate();
        private SepiaEffect sepiaEffect = new SepiaEffect();
        private BrightnessAndContrastAdjustment bacAdjustment = new BrightnessAndContrastAdjustment();
        private UserBlendOps.OverlayBlendOp overlayOp = new UserBlendOps.OverlayBlendOp();

        public override unsafe void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            ThreeAmountsConfigToken tacd = (ThreeAmountsConfigToken)parameters;
            float redAdjust = 1.0f + ((float)tacd.Amount3 / 100.0f);
            float blueAdjust = 1.0f - ((float)tacd.Amount3 / 100.0f);

            AmountEffectConfigToken blurToken = new AmountEffectConfigToken(tacd.Amount1 * 3);
            BrightnessAndContrastAdjustmentConfigToken bacToken = new BrightnessAndContrastAdjustmentConfigToken(tacd.Amount2, -tacd.Amount2 / 2);

            this.blurEffect.Render(blurToken, dstArgs, srcArgs, rois, startIndex, length);
            this.bacAdjustment.Render(bacToken, dstArgs, dstArgs, rois, startIndex, length);

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle roi = rois[i];

                for (int y = roi.Top; y < roi.Bottom; ++y)
                {
                    ColorBgra* srcPtr = srcArgs.Surface.GetPointAddress(roi.X, roi.Y);
                    ColorBgra* dstPtr = dstArgs.Surface.GetPointAddress(roi.X, roi.Y);

                    for (int x = roi.Left; x < roi.Right; ++x)
                    {
                        ColorBgra srcGrey = this.desaturateOp.Apply(*srcPtr);

                        srcGrey.R = Utility.ClampToByte((int)((float)srcGrey.R * redAdjust));
                        srcGrey.B = Utility.ClampToByte((int)((float)srcGrey.B * blueAdjust));

                        ColorBgra mypixel = this.overlayOp.Apply(srcGrey, *dstPtr);
                        *dstPtr = mypixel;

                        ++srcPtr;
                        ++dstPtr;
                    }
                }
            }
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            ThreeAmountsConfigDialog tacd = new ThreeAmountsConfigDialog();
            tacd.Text = StaticName;

            tacd.Amount1Label = PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.SoftnessLabel");
            tacd.Amount1Minimum = 0;
            tacd.Amount1Maximum = 10;
            tacd.Amount1Default = 5;

            tacd.Amount2Label = PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.LightingLabel");
            tacd.Amount2Minimum = -20;
            tacd.Amount2Maximum = 20;
            tacd.Amount2Default = 0;

            tacd.Amount3Label = PdnResources.GetString("SoftenPortraitEffect.ConfigDialog.WarmthLabel"); 
            tacd.Amount3Minimum = 0;
            tacd.Amount3Maximum = 20;
            tacd.Amount3Default = 10;

            return tacd;
        }

        public SoftenPortraitEffect()
            : base(StaticName, StaticIcon, true)
        {
        }
    }
}