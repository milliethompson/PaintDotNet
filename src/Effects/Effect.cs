/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
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
        private string description;
        private Image image;
        private Shortcut shortcut;
        private bool singleThreaded;
        private EffectEnvironmentParameters envParams;

        /// <summary>
        /// Returns the category of the effect. If there is no EffectCategoryAttribute
        /// applied to the runtime type, then the default category, EffectCategory.Effect,
        /// will be returned.
        /// </summary>
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

        /// <summary>
        /// Gets whether an Effect wants to be rendered using only 1 thread.
        /// It is up to the caller of Render() to enforce this.
        /// </summary>
        public bool SingleThreaded
        {
            get
            {
                return singleThreaded;
            }
        }

        public string SubMenuName
        {
            get
            {
                object[] attributes = this.GetType().GetCustomAttributes(true);

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is EffectSubMenuAttribute)
                    {
                        return ((EffectSubMenuAttribute)attribute).SubMenuName;
                    }
                }

                return null;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public string Description
        {
            get
            {
                return description;
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

        [Obsolete("user EnvironmentParameters.GetSelection() instead")]
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

        public Effect(string name, string description, Image image)
            : this(name, description, image, Shortcut.None)
        {
        }

        public Effect(string name, string description, Image image, Shortcut shortcut)
        {
            this.name = name;
            this.description = description;
            this.image = image;
            this.shortcut = shortcut;
            this.singleThreaded = false;
            this.envParams = EffectEnvironmentParameters.DefaultParameters;

            object[] attributes = this.GetType().GetCustomAttributes(true);

            foreach (Attribute attribute in attributes)
            {
                if (attribute is SingleThreadedEffectAttribute)
                {
                    this.singleThreaded = true;
                }
            }
        }
    }
}
