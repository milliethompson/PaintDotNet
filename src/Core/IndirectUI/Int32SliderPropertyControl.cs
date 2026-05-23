/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet;
using PaintDotNet.PropertySystem;
using System;

namespace PaintDotNet.IndirectUI
{
    [PropertyControlInfo(typeof(Int32Property), PropertyControlType.Slider, IsDefault = true)]
    internal sealed class Int32SliderPropertyControl
         : SliderPropertyControl<int>
    {
        private const int maxMax = 100000000;
        private const int minMin = -100000000;

        public Int32SliderPropertyControl(PropertyControlInfo propInfo)
            : base(propInfo)
        {
        }

        protected override int ToSliderValue(int propertyValue)
        {
            return Utility.Clamp(propertyValue, minMin, maxMax);
        }

        protected override int FromSliderValue(int sliderValue)
        {
            return Utility.Clamp(sliderValue, minMin, maxMax);
        }

        protected override decimal ToNudValue(int propertyValue)
        {
            return (decimal)Utility.Clamp(propertyValue, minMin, maxMax);
        }

        protected override int FromNudValue(decimal nudValue)
        {
            return Utility.Clamp((int)nudValue, minMin, maxMax);
        }
    }
}
