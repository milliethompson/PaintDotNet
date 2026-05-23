/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

namespace PaintDotNet
{
    public interface IUnitsComboBox
    {
        UnitsDisplayType UnitsDisplayType
        {
            get;
            set;
        }

        bool LowercaseStrings
        {
            get;
            set;
        }

        MeasurementUnit Units
        {
            get;
            set;
        }

        string UnitsText
        {
            get;
        }

        bool PixelsAvailable
        {
            get;
            set;
        }

        bool InchesAvailable
        {
            get;
        }

        bool CentimetersAvailable
        {
            get;
        }

        void RemoveUnit(MeasurementUnit removeMe);
        void AddUnit(MeasurementUnit addMe);

        event EventHandler UnitsChanged;
    }
}
