/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.Effects
{
    public static class SubmenuNames
    {
        public static string Blurs
        {
            get
            {
                return PdnResources.GetString("Effects.Blurring.Submenu.Name");
            }
        }

        public static string Distort
        {
            get
            {
                return PdnResources.GetString("DistortSubmenu.Name");
            }
        }
    }
}
