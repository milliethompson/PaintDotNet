/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) Rick Brewster, Tom Jackson, and past contributors.            //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.Base;
using System;
using System.ComponentModel;

namespace PaintDotNet.PropertySystem
{
    // TODO: get rid of 

    public sealed class AngleProperty
        : ScalarProperty<double>
    {
        public static double MinAngleValue
        {
            get
            {
                return -180.0;
            }
        }

        public static double MaxAngleValue
        {
            get
            {
                return +180.0;
            }
        }

        public AngleProperty(object name)
            : this(name, 0)
        {
        }

        public AngleProperty(object name, double defaultValue)
            : this(name, defaultValue, false)
        {
        }

        public AngleProperty(object name, double defaultValue, bool readOnly)
            : base(name, defaultValue, MinAngleValue, MaxAngleValue, readOnly)
        {
        }

        private AngleProperty(AngleProperty copyMe, AngleProperty sentinelNotUsed)
            : base(copyMe, sentinelNotUsed)
        {
        }

        public override Property Clone()
        {
            return new AngleProperty(this, this);
        }
    }
}
