/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using Microsoft.CSharp;

namespace PaintDotNet.Effects
{
	public class CodeLab : Effect, IConfigurableEffect
	{
		public CodeLab() : base("Code Lab", null)
		{
		}

		public EffectConfigDialog CreateConfigDialog()
		{
			CodeLabConfigDialog secd = new CodeLabConfigDialog();
			return secd;
		}

		public void Render(EffectConfigToken properties, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
		{
			CodeLabConfigToken sect = (CodeLabConfigToken)properties;
            Effect userEffect = sect.UserScriptObject;
			if (userEffect != null) 
			{
                userEffect.EnvironmentParameters = this.EnvironmentParameters;

				try 
				{
					foreach (Rectangle rect in roi.GetRegionScansReadOnlyInt()) 
					{
						userEffect.Render(dstArgs, srcArgs, rect);
					}
				}

				catch (Exception exc)
				{
					sect.LastException = exc;
					dstArgs.Surface.CopySurface(srcArgs.Surface);
					sect.UserScriptObject = null;
				}
			}
		}
	}
}
