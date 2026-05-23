/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

/*****************************************************************************

    This program/project is just a dummy program whose real purpose lies in
    the pre-build events which are used to config the PaintDotNet.msi file
    in ways that can't otherwise be automated.

    Currently we:

    1. Change the .msi from "Install for Just Me" to "Install for Everyone."
       This is accomplished using a VB-Script that executes an SQL query.

    2. Packages together the .NET 1.1 installer and the PaintDotNet.msi along
       with the ".NET Bootstrapper" (downloadable @ MSDN) that installs .NET
       (if required) and then installs PDN. These are packaged into a self-
       extracting EXE using Nullsoft's Scriptable Install System. See the file
       MakeFullSetup.nsi for more info.

*****************************************************************************/

int main(char argc, char **argv)
{
    return 0;
}
