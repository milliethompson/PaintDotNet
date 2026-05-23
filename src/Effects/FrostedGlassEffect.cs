// Frosted Glass effect
// Paint.NET implementation by Rick Brewster
// Original C++ implementation by Jason Waltman as part of "Filter Explorer," http://www.jasonwaltman.com/thesis/index.html

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;

namespace PaintDotNet.Effects
{
    public class FrostedGlassEffect
        : Effect,
          IConfigurableEffect
    {
        [ThreadStatic]
        private static Random random = null;

        private static Random Random
        {
            get
            {
                if (random == null)
                {
                    random = new Random();
                }

                return random;
            }
        }

        public FrostedGlassEffect() 
            : base("Frosted Glass", "Gives the illusion of peering through a pane of frosted glass", Utility.GetImageResource("Icons.FrostedGlassEffect.bmp")) 
        {
        }

        public EffectConfigDialog CreateConfigDialog()
        {
            AmountEffectConfigDialog aecd = new AmountEffectConfigDialog();
            aecd.SliderMinimum = 1;
            aecd.SliderMaximum = 10;
            aecd.Text = "Frosted Glass";
            aecd.SliderLabel = "Scatter Radius";
			aecd.SliderUnitsName = "pixels";

			aecd.Icon = Utility.GetIconResource("Icons.FrostedGlassEffect.bmp");
            return aecd;
        }

        public unsafe void Render(EffectConfigToken token, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            AmountEffectConfigToken realToken = (AmountEffectConfigToken)token;
            Surface src = srcArgs.Surface;
            Surface dst = dstArgs.Surface;

            int width = src.Width;
            int height = src.Height;
            int r = realToken.Amount;
            Random random = FrostedGlassEffect.Random;

            int[] intensityCount = new int[256];
            uint[] avgRed = new uint[256];
            uint[] avgGreen = new uint[256];
            uint[] avgBlue = new uint[256];
            uint[] avgAlpha = new uint[256];
            byte[] intensityChoices = new byte[(1 + (r * 2)) * (1 + (r * 2))];

            foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
            {
                int rectTop = rect.Top;
                int rectBottom = rect.Bottom;
                int rectLeft = rect.Left;
                int rectRight = rect.Right;

                for (int y = rectTop; y < rectBottom; ++y)
                {
                    ColorBgra *dstPtr = dst.GetPointAddress(rect.Left, y);
                    int top = y - r;
                    int bottom = y + r + 1;

                    if (top < 0)
                    {
                        top = 0;
                    }

                    if (bottom > height)
                    {
                        bottom = height;
                    }

                    for (int x = rectLeft; x < rectRight; ++x)
                    {
                        int intensityChoicesIndex = 0;

                        for (int i = 0; i < 256; ++i)
                        {
                            intensityCount[i] = 0;
                            avgRed[i] = 0;
                            avgGreen[i] = 0;
                            avgBlue[i] = 0;
                            avgAlpha[i] = 0;
                        }

                        int left = x - r;
                        int right = x + r + 1;

                        if (left < 0)
                        {
                            left = 0;
                        }

                        if (right > width)
                        {
                            right = width;
                        }

                        for (int j = top; j < bottom; ++j)
                        {
                            if (j < 0 || j >= height)
                            {
                                continue;
                            }

                            ColorBgra *srcPtr = src.GetPointAddress(left, j);

                            for (int i = left; i < right; ++i)
                            {
                                byte intensity = srcPtr->GetIntensityByte();

                                intensityChoices[intensityChoicesIndex] = intensity;
                                ++intensityChoicesIndex;

                                ++intensityCount[intensity];

                                avgRed[intensity] += srcPtr->R;
                                avgGreen[intensity] += srcPtr->G;
                                avgBlue[intensity] += srcPtr->B;
                                avgAlpha[intensity] += srcPtr->A;

                                ++srcPtr;
                            }
                        }

                        int randNum = random.Next(intensityChoicesIndex);
                        byte chosenIntensity = intensityChoices[randNum];

                        byte R = (byte)(avgRed[chosenIntensity] / intensityCount[chosenIntensity]);
                        byte G = (byte)(avgGreen[chosenIntensity] / intensityCount[chosenIntensity]);
                        byte B = (byte)(avgBlue[chosenIntensity] / intensityCount[chosenIntensity]);
                        byte A = (byte)(avgAlpha[chosenIntensity] / intensityCount[chosenIntensity]);

                        *dstPtr = ColorBgra.FromBgra(B, G, R, A);
                        ++dstPtr;

                        // prepare the array for the next loop iteration
                        for (int i = 0; i < intensityChoicesIndex; ++i)
                        {
                            intensityChoices[i] = 0;
                        }
                    }
                }
            }
        }
    }
}
