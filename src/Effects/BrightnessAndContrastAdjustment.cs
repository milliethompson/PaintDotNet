using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for BrightnessAndContrastAdjustment.
    /// </summary>
    [EffectCategory(EffectCategory.Adjustment)]
    public class BrightnessAndContrastAdjustment
        : Effect,
          IConfigurableEffect
    {
        public EffectConfigDialog CreateConfigDialog()
        {
            return new BrightnessAndContrastAdjustmentConfigDialog();
        }

        private float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }
            else
            {
                return value;
            }
        }

        private float ApplyContrast(float value, float contrastFactor)
        {
            //value -= 127.50f;
            value *= contrastFactor;
            //value += 127.50f;
            return value;
        }

        public void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            BrightnessAndContrastAdjustmentConfigToken token = (BrightnessAndContrastAdjustmentConfigToken)properties;
            float contrast = (100.0f + token.Contrast) / 100.0f;
            contrast *= contrast;
            int brightness = token.Brightness;

            unsafe
            {
                foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt())
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra *srcRowPtr = srcArgs.Surface.GetPointAddress(rect.Left, y);
                        ColorBgra *dstRowPtr = dstArgs.Surface.GetPointAddress(rect.Left, y);

                        for (int x = 0; x < rect.Width; ++x)
                        {
                            // read
                            ColorBgra c = *srcRowPtr;
                            ++srcRowPtr;

                            // apply Brightness
                            float r = (float)(c.R + brightness);
                            float g = (float)(c.G + brightness);
                            float b = (float)(c.B + brightness);

                            // apply Contrast
                            r = ApplyContrast(r, contrast);
                            g = ApplyContrast(g, contrast);
                            b = ApplyContrast(b, contrast);

                            // clamp
                            r = Clamp(r, 0, 255.0f);
                            g = Clamp(g, 0, 255.0f);
                            b = Clamp(b, 0, 255.0f);

                            // store
                            *dstRowPtr = ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, c.A);
                            ++dstRowPtr;
                        }
                    }
                }
            }

            return;
        }

        public override void Render(RenderArgs dstArgs, RenderArgs srcArgs, System.Drawing.Rectangle roi)
        {
            throw new InvalidOperationException("BrightnessAndContrastEffect must be used via the other Render overload");
        }

        public BrightnessAndContrastAdjustment()
            : base("Brightness/Contrast", 
                   "Adjusts the brightness and contrast levels of an image", 
                   null)
        {
        }
    }
}
