/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.IO;
using Microsoft.CSharp;

namespace PaintDotNet.Effects
{
    public class CodeLab : Effect
    {
        public static Image StaticImage
        {
            get
            {
                Assembly ourAssembly = Assembly.GetExecutingAssembly();
                Stream imageStream = ourAssembly.GetManifestResourceStream("PaintDotNet.Effects.Icons.CodeLab.png");
                Image image = Image.FromStream(imageStream);
                return image;
            }
        }

        public CodeLab()
            : base("Code Lab", StaticImage, true)
        {
        }

        public override EffectConfigDialog CreateConfigDialog()
        {
            CodeLabConfigDialog secd = new CodeLabConfigDialog();
            return secd;
        }

        public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
        {
            CodeLabConfigToken sect = (CodeLabConfigToken)parameters;
            Effect userEffect = sect.UserScriptObject;

            if (userEffect != null)
            {
                userEffect.EnvironmentParameters = this.EnvironmentParameters;

                try
                {
                    userEffect.Render(null, dstArgs, srcArgs, rois, startIndex, length);
                }

                catch (Exception exc)
                {
                    sect.LastExceptions.Add(exc);
                    dstArgs.Surface.CopySurface(srcArgs.Surface);
                    sect.UserScriptObject = null;
                }
            }
        }
    }
}
