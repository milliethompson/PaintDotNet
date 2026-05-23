/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Describes an interface for some sort of source->destination rendering effect.
    /// The pixels of the destination are the result of apply some function to the
    /// pixel values of the source that may be affect by some other state.
    /// </summary>
    public abstract class Effect
    {
        private string name;
        private Image image;
        private Shortcut shortcut;
        private EffectDirectives effectDirectives;
        private string subMenuName;
        private EffectEnvironmentParameters envParams;

        /// <summary>
        /// Returns the category of the effect. If there is no EffectCategoryAttribute
        /// applied to the runtime type, then the default category, EffectCategory.Effect,
        /// will be returned.
        /// </summary>
        /// <remarks>
        /// This controls which menu in the user interface the effect is placed in to.
        /// </remarks>
        public EffectCategory Category
        {
            get
            {
                object[] attributes = this.GetType().GetCustomAttributes(true);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is EffectCategoryAttribute)
                    {
                        return ((EffectCategoryAttribute)attribute).Category;
                    }
                }

                return EffectCategory.Effect;
            }
        }

        public EffectEnvironmentParameters EnvironmentParameters 
        {
            get 
            {
                return envParams;
            }

            set 
            {
                envParams = value;
            }
        }

        public EffectDirectives EffectDirectives
        {
            get
            {
                return this.effectDirectives;
            }
        }

        public string SubMenuName
        {
            get
            {
                return this.subMenuName;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        [Obsolete("This attribute was never used, and has been removed.", true)]
        public string Description
        {
            get
            {
                return string.Empty;
            }
        }

        public Image Image
        {
            get
            {
                return image;
            }
        }

        public Shortcut Shortcut 
        {
            get 
            {
                return shortcut;
            }
        }

        [Obsolete("Use EnvironmentParameters.GetSelection() instead")]
        public PdnRegion Selection
        {
            get
            {
                return new PdnRegion();
            }
        }

        /// <summary>
        /// Performs the effect's rendering. The source is to be treated as read-only,
        /// and only the destination pixels within the given rectangle-of-interest are
        /// to be written to. However, in order to compute the destination pixels,
        /// any pixels from the source may be utilized.
        /// </summary>
        /// <param name="dstArgs">Describes the destination surface.</param>
        /// <param name="srcArgs">Describes the source surface.</param>
        /// <param name="roi">The rectangle we want rendered in dstArgs.</param>
        public virtual void Render(RenderArgs dstArgs, RenderArgs srcArgs, Rectangle roi)
        {
            if (dstArgs.Surface.Size != srcArgs.Surface.Size)
            {
                throw new ArgumentException("Destination surface and Source surface sizes do not match", "dstArgs, srcArgs");
            }

            Rectangle checkRect = Rectangle.Intersect(dstArgs.Surface.Bounds, roi);

            if (checkRect != roi)
            {
                throw new ArgumentOutOfRangeException("roi", "Region of interest was out of bounds");
            }
        }

        /// <summary>
        /// This is a helper function. For every rectangle that makes up the requested
        /// region, the other form of Render will be called.
        /// </summary>
        /// <param name="dstArgs">Describes the destination surface.</param>
        /// <param name="srcArgs">Describes the source surface.</param>
        /// <param name="roi">The region we want rendered in dstArgs.</param>
        public void Render(RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            Rectangle[] rects = roi.GetRegionScansReadOnlyInt();

            foreach (Rectangle rect in rects)
            {
                Render(dstArgs, srcArgs, rect);
            }
        }

        /// <summary>
        /// This is a helper function. It allows you to render an effect "in place."
        /// That is, you don't need both a destination and a source Surface.
        /// </summary>
        public void RenderInPlace(RenderArgs srcAndDstArgs, PdnRegion roi)
        {
            using (Surface renderSurface = new Surface(srcAndDstArgs.Surface.Size))
            {
                using (RenderArgs renderArgs = new RenderArgs(renderSurface))
                {
                    Render(renderArgs, srcAndDstArgs, roi);
                    srcAndDstArgs.Surface.CopySurface(renderSurface, roi);
                }
            }
        }

        public void RenderInPlace(RenderArgs srcAndDstArgs, Rectangle roi)
        {
            PdnRegion region = new PdnRegion(roi);            
            RenderInPlace(srcAndDstArgs, region);
            region.Dispose();
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// </remarks>
        public Effect(string name, Image image)
            : this(name, image, Shortcut.None)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcut">A shortcut key for accessing the effect.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// </remarks>
        public Effect(string name, Image image, Shortcut shortcut)
            : this(name, image, shortcut, null)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcut">A shortcut key for accessing the effect.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// </remarks>
        public Effect(string name, Image image, Shortcut shortcut, string subMenuName)
            : this(name, image, shortcut, null, EffectDirectives.None)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcut">A shortcut key for accessing the effect.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <param name="effectDirectives">A set of flags indicating important information about the effect.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// For performance reasons, it is recommended that you not set forceSingleThreaded to false.
        /// </remarks>
        public Effect(string name, Image image, Shortcut shortcut, string subMenuName, 
            EffectDirectives effectDirectives)
        {
            this.name = name;
            this.image = image;
            this.subMenuName = subMenuName;
            this.shortcut = shortcut;
            this.effectDirectives = effectDirectives;
            this.envParams = EffectEnvironmentParameters.DefaultParameters;
        }

        /// <summary>
        /// This constructor is obsolete.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="image"></param>
        [Obsolete("The description property has been removed.", true)]
        public Effect(string name, string description, Image image)
            : this(name, description, image, Shortcut.None)
        {
        }

        /// <summary>
        /// This constructor is obsolete.
        /// </summary>
        [Obsolete("The description property has been removed.", true)]
        public Effect(string name, string description, Image image, Shortcut shortcut)
            : this(name, image, shortcut, null)
        {
        }
    }
}
