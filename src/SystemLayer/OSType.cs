/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;

namespace PaintDotNet.SystemLayer
{
    [Flags]
    public enum OSType
    {
        Unknown = 0,
        Workstation = (int)NativeConstants.VER_NT_WORKSTATION,
        DomainController = (int)NativeConstants.VER_NT_DOMAIN_CONTROLLER,
        Server = (int)NativeConstants.VER_NT_SERVER,
    }
}
