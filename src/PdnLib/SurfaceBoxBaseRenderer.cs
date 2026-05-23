/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Drawing;

namespace PaintDotNet
{
    public class SurfaceBoxBaseRenderer
        : SurfaceBoxRenderer
    {
        private Surface source;
        private RenderDelegate renderDelegate;

        public Surface Source
        {
            get
            {
                return this.source;
            }

            set
            {
                this.source = value;
                Flush();
            }
        }

        private void Flush()
        {
            this.renderDelegate = null;
        }

        protected override void OnVisibleChanged()
        {
            Invalidate();
        }

        private void ChooseRenderDelegate()
        {
            if (SourceSize.Width > DestinationSize.Width)
            {
                // zoom out
                this.renderDelegate = new RenderDelegate(RenderZoomOutRotatedGridMultisampling);
            }
            else if (SourceSize == DestinationSize)
            {
                // zoom 100%
                this.renderDelegate = new RenderDelegate(RenderOneToOne);
            }
            else if (SourceSize.Width < DestinationSize.Width)
            {
                // zoom in
                this.renderDelegate = new RenderDelegate(RenderZoomInNearestNeighbor);
            }
        }

        public override void OnDestinationSizeChanged()
        {
            ChooseRenderDelegate();
            this.OwnerList.InvalidateLookups();
            base.OnDestinationSizeChanged();
        }

        public override void OnSourceSizeChanged()
        {
            ChooseRenderDelegate();
            this.OwnerList.InvalidateLookups();
            base.OnSourceSizeChanged();
        }

        public void RenderOneToOne(Surface dst, Point offset)
        {
            Rectangle srcRect = new Rectangle(offset, dst.Size);
            srcRect.Intersect(source.Bounds);
            dst.CopySurface(this.source, srcRect);
        }

        private void RenderZoomInNearestNeighbor(Surface dst, Point offset)
        {
            unsafe
            {
                int[] d2SLookupY = OwnerList.Dst2SrcLookupY;
                int[] d2SLookupX = OwnerList.Dst2SrcLookupX;

                for (int dstRow = 0; dstRow < dst.Height; ++dstRow)
                {
                    int nnY = dstRow + offset.Y;
                    int srcY = d2SLookupY[nnY];
                    ColorBgra *dstPtr = dst.GetRowAddressUnchecked(dstRow);
                    ColorBgra *srcRow = this.source.GetRowAddressUnchecked(srcY);

                    for (int dstCol = 0; dstCol < dst.Width; ++dstCol)
                    {
                        int nnX = dstCol + offset.X;
                        int srcX = d2SLookupX[nnX];

                        *dstPtr = *(srcRow + srcX);
                        ++dstPtr;
                    }
                }
            }
        }

        private void RenderZoomOutRotatedGridMultisampling(Surface dst, Point offset)
        {
            unsafe
            {
                long fDstLeftLong = ((long)offset.X * 4096 * (long)SourceSize.Width) / (long)DestinationSize.Width;
                long fDstTopLong = ((long)offset.Y * 4096 * (long)SourceSize.Height) / (long)DestinationSize.Height;
                long fDstRightLong = ((long)(offset.X + dst.Width) * 4096 * (long)SourceSize.Width) / (long)DestinationSize.Width;
                long fDstBottomLong = ((long)(offset.Y + dst.Height) * 4096 * (long)SourceSize.Height) / (long)DestinationSize.Height;
                int fDstLeft = (int)fDstLeftLong;
                int fDstTop = (int)fDstTopLong;
                int fDstRight = (int)fDstRightLong;
                int fDstBottom = (int)fDstBottomLong;
                int dx = (fDstRight - fDstLeft) / dst.Width;
                int dy = (fDstBottom - fDstTop) / dst.Height;

                for (int dstRow = 0, fDstY = fDstTop; 
                    dstRow < dst.Height && fDstY < fDstBottom; 
                    ++dstRow, fDstY += dy)
                {
                    int srcY1 = fDstY >> 12;                            // y
                    int srcY2 = (fDstY + (dy >> 2)) >> 12;              // y + 0.25
                    int srcY3 = (fDstY + (dy >> 1)) >> 12;              // y + 0.50
                    int srcY4 = (fDstY + (dy >> 1) + (dy >> 2)) >> 12;  // y + 0.75

#if DEBUG
                    Debug.Assert(this.source.IsRowVisible(srcY1));
                    Debug.Assert(this.source.IsRowVisible(srcY2));
                    Debug.Assert(this.source.IsRowVisible(srcY3));
                    Debug.Assert(this.source.IsRowVisible(srcY4));
                    Debug.Assert(dst.IsRowVisible(dstRow));
#endif

                    ColorBgra *src1 = this.source.GetRowAddressUnchecked(srcY1);
                    ColorBgra *src2 = this.source.GetRowAddressUnchecked(srcY2);
                    ColorBgra *src3 = this.source.GetRowAddressUnchecked(srcY3);
                    ColorBgra *src4 = this.source.GetRowAddressUnchecked(srcY4);
                    ColorBgra *dstPtr = dst.GetRowAddressUnchecked(dstRow);

                    for (int dstCol = 0, fDstX = fDstLeft;
                         dstCol < dst.Width && fDstX < fDstRight;
                         ++dstCol, fDstX += dx)
                    {
                        int srcX1 = (fDstX + (dx >> 2)) >> 12;             // x + 0.25
                        int srcX2 = (fDstX + (dx >> 1) + (dx >> 2)) >> 12; // x + 0.75
                        int srcX3 = fDstX >> 12;                           // x
                        int srcX4 = (fDstX + (dx >> 1)) >> 12;             // x + 0.50

#if DEBUG
                        Debug.Assert(this.source.IsColumnVisible(srcX1));
                        Debug.Assert(this.source.IsColumnVisible(srcX2));
                        Debug.Assert(this.source.IsColumnVisible(srcX3));
                        Debug.Assert(this.source.IsColumnVisible(srcX4));
                        Debug.Assert(dst.IsColumnVisible(dstCol));
#endif

                        ColorBgra *p1 = src1 + srcX1;
                        ColorBgra *p2 = src2 + srcX2;
                        ColorBgra *p3 = src3 + srcX3;
                        ColorBgra *p4 = src4 + srcX4;

                        int r = (2 + p1->R + p2->R + p3->R + p4->R) >> 2;
                        int g = (2 + p1->G + p2->G + p3->G + p4->G) >> 2;
                        int b = (2 + p1->B + p2->B + p3->B + p4->B) >> 2;
                        int a = (2 + p1->A + p2->A + p3->A + p4->A) >> 2;

                        dstPtr->Bgra = (uint)b + ((uint)g << 8) + ((uint)r << 16) + ((uint)a << 24);
                        ++dstPtr;
                    }
                }
            }
        }

        public override void Render(Surface dst, Point offset)
        {
            if (this.renderDelegate == null)
            {
                ChooseRenderDelegate();
            }

            this.renderDelegate(dst, offset);
        }

        public SurfaceBoxBaseRenderer(SurfaceBoxRendererList ownerList, Surface source)
            : base(ownerList)
        {
            this.source = source;
            ChooseRenderDelegate();
        }
    }
}
