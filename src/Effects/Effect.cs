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
        private Keys shortcutKeys;
        private EffectDirectives effectDirectives;
        private string subMenuName;
        private EffectEnvironmentParameters envParams;
        private bool isConfigurable;

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
                return this.envParams;
            }

            set 
            {
                this.envParams = value;
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
                return this.name;
            }
        }

        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        public Keys ShortcutKeys
        {
            get 
            {
                return this.shortcutKeys;
            }
        }

        public bool IsConfigurable
        {
            get
            {
                return this.isConfigurable;
            }
        }

        /// <summary>
        /// Performs the effect's rendering. The source is to be treated as read-only,
        /// and only the destination pixels within the given rectangle-of-interest are
        /// to be written to. However, in order to compute the destination pixels,
        /// any pixels from the source may be utilized.
        /// </summary>
        /// <param name="parameters">The parameters to the effect. If IsConfigurable is true, then this must not be null.</param>
        /// <param name="dstArgs">Describes the destination surface.</param>
        /// <param name="srcArgs">Describes the source surface.</param>
        /// <param name="rois">The list of rectangles that describes the region of interest.</param>
        /// <param name="startIndex">The index within roi to start enumerating from.</param>
        /// <param name="length">The number of rectangles to enumerate from roi.</param>
        public abstract void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length);

        public void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois)
        {
            Render(parameters, dstArgs, srcArgs, rois, 0, rois.Length);
        }

        public void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, PdnRegion roi)
        {
            Rectangle[] scans = roi.GetRegionScansReadOnlyInt();
            Render(parameters, dstArgs, srcArgs, scans, 0, scans.Length);
        }

        public virtual EffectConfigDialog CreateConfigDialog()
        {
            if (this.IsConfigurable)
            {
                throw new NotImplementedException("If IsConfigurable is true, then CreateConfigDialog() must be implemented");
            }
            else
            {
                return null;
            }
        }

        /*
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
         * */

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
                    Rectangle[] scans = roi.GetRegionScansReadOnlyInt();
                    Render(null, renderArgs, srcAndDstArgs, scans);
                    srcAndDstArgs.Surface.CopySurface(renderSurface, roi);
                }
            }
        }

        public void RenderInPlace(RenderArgs srcAndDstArgs, Rectangle roi)
        {
            using (PdnRegion region = new PdnRegion(roi))
            {
                RenderInPlace(srcAndDstArgs, region);
            }
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
            : this(name, image, Keys.None)
        {
        }

        public Effect(string name, Image image, bool isConfigurable)
            : this(name, image, Keys.None, isConfigurable)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcutKeys">A shortcut key for accessing the effect.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// </remarks>
        public Effect(string name, Image image, Keys shortcutKeys)
            : this(name, image, shortcutKeys, null)
        {
        }

        public Effect(string name, Image image, Keys shortcutKeys, bool isConfigurable)
            : this(name, image, shortcutKeys, null, isConfigurable)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcutKeys">A shortcut key for accessing the effect.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// </remarks>
        public Effect(string name, Image image, Keys shortcutKeys, string subMenuName)
            : this(name, image, shortcutKeys, subMenuName, EffectDirectives.None)
        {
        }

        public Effect(string name, Image image, Keys shortcutKeys, string subMenuName, bool isConfigurable)
            : this(name, image, shortcutKeys, subMenuName, EffectDirectives.None, isConfigurable)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcutKeys">A shortcut key for accessing the effect.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <param name="effectDirectives">A set of flags indicating important information about the effect.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// </remarks>
        public Effect(string name, Image image, Keys shortcutKeys, string subMenuName,
            EffectDirectives effectDirectives)
            : this(name, image, shortcutKeys, subMenuName, effectDirectives, false)
        {
        }

        /// <summary>
        /// Base constructor for the Effect class.
        /// </summary>
        /// <param name="name">A unique name for the effect.</param>
        /// <param name="image">A 16x16 icon for the effect that will show up in the menu.</param>
        /// <param name="shortcutKeys">A shortcut key for accessing the effect.</param>
        /// <param name="subMenuName">The name of a sub-menu to place the effect into. Pass null for no sub-menu.</param>
        /// <param name="effectDirectives">A set of flags indicating important information about the effect.</param>
        /// <param name="isConfigurable">A flag indicating whether the effect is configurable. If this is true, then CreateConfigDialog must be implemented.</param>
        /// <remarks>
        /// Do not include the word 'effect' in the name parameter.
        /// The shortcut key is only honored for effects with the [EffectCategory(EffectCategory.Adjustment)] attribute.
        /// The sub-menu parameter can be used to group effects. The name parameter must still be unique.
        /// </remarks>
        public Effect(string name, Image image, Keys shortcutKeys, string subMenuName,
            EffectDirectives effectDirectives, bool isConfigurable)
        {
            this.name = name;
            this.image = image;
            this.subMenuName = subMenuName;
            this.shortcutKeys = shortcutKeys;
            this.effectDirectives = effectDirectives;
            this.envParams = EffectEnvironmentParameters.DefaultParameters;
            this.isConfigurable = isConfigurable;
        }
    }
}
