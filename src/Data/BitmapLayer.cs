/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Threading;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for BitmapLayer.
    /// </summary>
    [Serializable]
    public class BitmapLayer
        : Layer,
          IDeserializationCallback
    {
        private bool disposed = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;

                try
                {
                    if (disposing)
                    {
                        if (surface != null)
                        {
                            surface.Dispose();
                            surface = null;
                        }
                    }
                }
                    
                finally
                {
                    base.Dispose(disposing);
                }
            }
        }

        private IPixelOp compiledBlendOp = null;

        /// <summary>
        /// This handles the case when blendOp is null, but opacity is not equal to 255
        /// </summary>
        [Serializable]
        private sealed class BlendWithOpacityOp
            : BinaryPixelOp
        {
            private int opacity;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                rhs.A = (byte)(((1 + rhs.A) * opacity) / 256);
                return BinaryPixelOps.AlphaBlend.ApplyStatic(lhs, rhs);
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * lhs, ColorBgra * rhs, int length)
            {
                while (length > 0)
                {
                    int rhsA = ((1 + rhs->A) * opacity) / 256;
                    int invRhsA = 256 - rhsA;
                    int lhsA = lhs->A + 1;
                    int invLhsA = 256 - lhsA;

                    int r = (((invRhsA * (lhsA * lhs->R)) / 256) + (rhsA * rhs->R)) / 256;
                    int g = (((invRhsA * (lhsA * lhs->G)) / 256) + (rhsA * rhs->G)) / 256;
                    int b = (((invRhsA * (lhsA * lhs->B)) / 256) + (rhsA * rhs->B)) / 256;
                    int a = ComputeAlpha(lhs->A, rhs->A);
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)a << 24));

                    ++dst;
                    ++lhs;
                    ++rhs;
                    --length;
                }
            }

            protected override unsafe void Apply(ColorBgra * dst, ColorBgra * src, int length)
            {
                while (length > 0)
                {
                    int srcA = ((1 + src->A) * opacity) / 256;
                    int invSrcA = 256 - srcA;
                    int dstA = dst->A + 1;

                    int r = (((invSrcA * (dstA * dst->R)) / 256) + (srcA * src->R)) / 256;
                    int g = (((invSrcA * (dstA * dst->G)) / 256) + (srcA * src->G)) / 256;
                    int b = (((invSrcA * (dstA * dst->B)) / 256) + (srcA * src->B)) / 256;
                    int a = ComputeAlpha(dst->A, src->A);
                
                    dst->Bgra = (uint)(b + (g << 8) + (r << 16) + ((uint)a << 24));

                    ++dst;
                    ++src;
                    --length;
                }
            }

            public BlendWithOpacityOp(int opacity)
            {
                this.opacity = opacity;
            }
        }

        /// <summary>
        /// This handles the case when blendOp is not null, and opacity is not 255
        /// </summary>
        [Serializable]
        private sealed class BlendWithBlendOpAndOpacityOp
            : BinaryPixelOp
        {
            private int opacity;
            private BinaryPixelOp op;

            public override ColorBgra Apply(ColorBgra lhs, ColorBgra rhs)
            {
                ColorBgra mid = op.Apply(lhs, rhs);
                mid.A = (byte)(((1 + mid.A) * opacity) / 256);
                return BinaryPixelOps.AlphaBlend.ApplyStatic(lhs, mid);
            }

            public BlendWithBlendOpAndOpacityOp(int opacity, BinaryPixelOp op)
            {
                this.opacity = opacity;
                this.op = op;
            }
        }

        private void CompileBlendOp()
        {
            bool isDefaultOp = (properties.blendOp.GetType() == UserBlendOps.GetDefaultBlendOp());

            if (isDefaultOp && this.Opacity == 255)
            {
                compiledBlendOp = new BinaryPixelOps.AlphaBlend();
            }
            else if (isDefaultOp && this.Opacity != 255)
            {
                compiledBlendOp = new BitmapLayer.BlendWithOpacityOp(this.Opacity);
            }
            else if (!isDefaultOp && this.Opacity == 255)
            {
                compiledBlendOp = properties.blendOp;
            }
            else if (!isDefaultOp && this.Opacity != 255)
            {
                compiledBlendOp = new BitmapLayer.BlendWithBlendOpAndOpacityOp(this.Opacity, properties.blendOp);
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            compiledBlendOp = null;
            base.OnPropertyChanged (propertyName);
        }

        [Serializable]
        internal sealed class BitmapLayerProperties
            : ICloneable,
              ISerializable
        {
            public UserBlendOp blendOp;
            internal int opacity; // this is ONLY used when loading older version PDN files! should normally equal -1

            public const string BlendOpName = "Blend Mode";

            public BitmapLayerProperties(UserBlendOp blendOp)
            {
                this.blendOp = blendOp;
                this.opacity = -1;
            }

            public BitmapLayerProperties(BitmapLayerProperties cloneMe)
            {
                this.blendOp = cloneMe.blendOp;
                this.opacity = -1;
            }

            #region ICloneable Members

            public object Clone()
            {
                return new BitmapLayerProperties(this);
            }

            #endregion

            #region ISerializable Members

            public BitmapLayerProperties(SerializationInfo info, StreamingContext context)
            {
                this.blendOp = (UserBlendOp)info.GetValue("blendOp", typeof(UserBlendOp));

                // search for 'opacity' and load it if it exists
                this.opacity = -1;

                foreach (SerializationEntry entry in info)
                {
                    if (entry.Name == "opacity")
                    {
                        this.opacity = (int)((byte)entry.Value);
                        break;
                    }
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("blendOp", this.blendOp);
            }

            #endregion
        }

        private BitmapLayerProperties properties;
        private Surface surface;

        public override object SaveProperties()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            object baseProperties = base.SaveProperties();
            return new List(properties.Clone(), new List(baseProperties, null));
        }

        public override void LoadProperties(object oldState, bool suppressEvents)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            List list = (List)oldState;

            // Get the base class' state, and our state
            LayerProperties baseState = (LayerProperties)list.Tail.Head;
            BitmapLayerProperties blp = (BitmapLayerProperties)(((List)oldState).Head);

            // Opacity is only couriered for compatibility with PDN v2.0 and v1.1
            // files. It should not be present in v2.1+ files (well, it'll be
            // part of the base class' serialization)
            if (blp.opacity != -1)
            {
                baseState.opacity = (byte)blp.opacity;
                blp.opacity = -1;
            }            

            // Have the base class load its properties
            base.LoadProperties(baseState, suppressEvents);

            // Now load our properties, and announce them to the world
            bool raiseBlendOp = false;

            if (blp.blendOp.GetType() != properties.blendOp.GetType())
            {
                if (!suppressEvents)
                {
                    raiseBlendOp = true;
                    OnPropertyChanging(BitmapLayerProperties.BlendOpName);
                }
            }

            this.properties = (BitmapLayerProperties)blp.Clone();
            this.compiledBlendOp = null;

            Invalidate();

            if (raiseBlendOp)
            {
                OnPropertyChanged(BitmapLayerProperties.BlendOpName);
            }
        }

        public void SetBlendOp(UserBlendOp blendOp)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (blendOp.GetType() != properties.blendOp.GetType())
            {
                OnPropertyChanging(BitmapLayerProperties.BlendOpName);
                properties.blendOp = blendOp;
                compiledBlendOp = null;
                Invalidate();
                OnPropertyChanged(BitmapLayerProperties.BlendOpName);
            }
        }

        public override object Clone()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            return (object)new BitmapLayer(this);
        }

        public Surface Surface
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return surface;
            }
        }

        public UserBlendOp BlendOp
        {
            get
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("BitmapLayer");
                }

                return properties.blendOp;
            }
        }

        public BitmapLayer(int width, int height)
            : this(width, height, ColorBgra.FromBgra(255, 255, 255, 0))
        {
        }

        public BitmapLayer(int width, int height, ColorBgra fillColor)
            : base(width, height)
        {
            this.surface = new Surface(width, height);
            // clear to see-through white, 0x00ffffff
            this.Surface.Clear(fillColor);
            this.properties = new BitmapLayerProperties(UserBlendOps.CreateDefaultBlendOp());
        }

        /// <summary>
        /// Creates a new BitmapLayer of the same size as the given Surface, and copies the 
        /// pixels from the given Surface.
        /// </summary>
        /// <param name="surface">The Surface to copy pixels from.</param>
        public BitmapLayer(Surface surface)
            : this(surface, false)
        {
        }

        /// <summary>
        /// Creates a new BitmapLayer of the same size as the given Surface, and either
        /// copies the pixels of the given Surface or takes ownership of it.
        /// </summary>
        /// <param name="surface">The Surface.</param>
        /// <param name="takeOwnership">
        /// true to take ownership of the surface (make sure to Dispose() it yourself), or
        /// false to copy its pixels
        /// </param>
        public BitmapLayer(Surface surface, bool takeOwnership)
            : base(surface.Width, surface.Height)
        {
            if (takeOwnership)
            {
                this.surface = surface;
            }
            else
            {
                this.surface = surface.Clone();
            }

            this.properties = new BitmapLayerProperties(UserBlendOps.CreateDefaultBlendOp());
        }

        protected BitmapLayer(BitmapLayer copyMe)
            : base(copyMe)
        {
            this.surface = copyMe.Surface.Clone();
            this.properties = (BitmapLayerProperties)copyMe.properties.Clone();
        }

        public BitmapLayer(Image image)
            : base(image.Width, image.Height)
        {
            using (Bitmap bitmap = Surface.CreateAliasedBitmap())
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
        }

        protected override void RenderImpl(RenderArgs args, Rectangle roi)
        {
            if (disposed)
            {
                throw new ObjectDisposedException("BitmapLayer");
            }

            if (Opacity == 0)
            {
                return;
            }

            if (compiledBlendOp == null)
            {
                CompileBlendOp();
            }

            compiledBlendOp.Apply(args.Surface, roi.Location, this.Surface, roi.Location, roi.Size);
        }

        public override PdnBaseForm CreateConfigDialog()
        {
            BitmapLayerPropertiesDialog blpd = new BitmapLayerPropertiesDialog();
            blpd.Layer = this;
            return blpd;
        }

        #region IDeserializationCallback Members

        public void OnDeserialization(object sender)
        {
            if (this.properties.opacity != -1)
            {
                this.PushSuppressPropertyChanged();
                base.Opacity = (byte)this.properties.opacity;
                this.properties.opacity = -1;
                this.PopSuppressPropertyChanged();
            }
        }

        #endregion
    }
}
