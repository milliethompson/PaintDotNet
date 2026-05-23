/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using PaintDotNet.SystemLayer;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Threading;

namespace PaintDotNet
{
    public class BitmapHistoryAction
        : HistoryAction
    {
        private DocumentWorkspace workspace;
        private int layerIndex;
        private string tempFileName;
        private DeleteFileOnFree tempFileHandle;
        private Guid poMaskedSurfaceRef; // if this is non-Guid.Empty, then tempFileName, tempFileHandle, and Data must be null
        private Guid poUndoMaskedSurfaceRef;
        
        private class DeleteFileOnFree
            : IDisposable
        {
            private IntPtr bstrFileName;

            public DeleteFileOnFree(string fileName)
            {
                this.bstrFileName = Marshal.StringToBSTR(fileName);
            }

            ~DeleteFileOnFree()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (this.bstrFileName != IntPtr.Zero)
                {
                    string fileName = Marshal.PtrToStringBSTR(this.bstrFileName);
                    Marshal.FreeBSTR(this.bstrFileName);
                    File.Delete(fileName);
                    this.bstrFileName = IntPtr.Zero;
                }
            }
        }

        [Serializable]
        private sealed class BitmapHistoryActionData
            : HistoryActionData
        {
            // only one of the following may be non-null
            private IrregularSurface undoImage;
            private PdnRegion savedRegion;

            public IrregularSurface UndoImage
            {
                get
                {
                    return undoImage;
                }
            }

            public PdnRegion SavedRegion
            {
                get
                {
                    return savedRegion;
                }
            }

            public BitmapHistoryActionData(IrregularSurface undoImage, PdnRegion savedRegion)
            {
                if (undoImage != null && savedRegion != null)
                {
                    throw new ArgumentException("Only one of undoImage or savedRegion may be non-null");
                }

                this.undoImage = undoImage;
                this.savedRegion = savedRegion;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (undoImage != null)
                    {
                        undoImage.Dispose();
                        undoImage = null;
                    }

                    if (savedRegion != null)
                    {
                        savedRegion.Dispose();
                        savedRegion = null;
                    }
                }

                base.Dispose(disposing);
            }
        }

        private static unsafe void LoadOrSaveSurfaceRegion(FileStream fileHandle, Surface surface, PdnRegion region, bool trueForSave)
        {
            Rectangle[] scans = region.GetRegionScansReadOnlyInt();
            Rectangle regionBounds = region.GetBoundsInt();
            Rectangle surfaceBounds = surface.Bounds;
            int scanCount = 0;

            void*[] ppvBuffers;
            uint[] lengths;

            regionBounds.Intersect(surfaceBounds);
            long length = (long)regionBounds.Width * (long)regionBounds.Height * (long)ColorBgra.SizeOf;

            if (scans.Length == 1 &&
                length <= uint.MaxValue &&
                surface.IsContiguousMemoryRegion(regionBounds))
            {
                ppvBuffers = new void*[1];
                lengths = new uint[1];

                ppvBuffers[0] = surface.GetPointAddressUnchecked(regionBounds.Location);
                lengths[0] = (uint)length;
            }
            else
            {
                for (int i = 0; i < scans.Length; ++i)
                {
                    Rectangle rect = scans[i];
                    rect.Intersect(surfaceBounds);

                    if (rect.Width != 0 && rect.Height != 0)
                    {
                        scanCount += rect.Height;
                    }
                }

                int scanIndex = 0;
                ppvBuffers = new void*[scanCount];
                lengths = new uint[scanCount];

                for (int i = 0; i < scans.Length; ++i)
                {
                    Rectangle rect = scans[i];
                    rect.Intersect(surfaceBounds);

                    if (rect.Width != 0 && rect.Height != 0)
                    {
                        for (int y = rect.Top; y < rect.Bottom; ++y)
                        {
                            ppvBuffers[scanIndex] = surface.GetPointAddressUnchecked(rect.Left, y);
                            lengths[scanIndex] = (uint)(rect.Width * ColorBgra.SizeOf);
                            ++scanIndex;
                        }
                    }
                }
            }

            if (trueForSave)
            {
                FileSystem.WriteToStreamingFileGather(fileHandle, ppvBuffers, lengths);
            }
            else
            {
                FileSystem.ReadFromStreamScatter(fileHandle, ppvBuffers, lengths);
            }
        }

        private static unsafe void SaveSurfaceRegion(FileStream outputHandle, Surface surface, PdnRegion region)
        {
            LoadOrSaveSurfaceRegion(outputHandle, surface, region, true);
        }

        private static unsafe void LoadSurfaceRegion(FileStream inputHandle, Surface surface, PdnRegion region)
        {
            LoadOrSaveSurfaceRegion(inputHandle, surface, region, false);
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace,
            int layerIndex, Guid poMaskedSurfaceRef)
            : base(name, image)
        {
            this.layerIndex = layerIndex;
            this.workspace = workspace;
            this.poMaskedSurfaceRef = poMaskedSurfaceRef;
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, 
            int layerIndex, PdnRegion changedRegion)
            : this(name, image, workspace, layerIndex, changedRegion, 
                   ((BitmapLayer)workspace.Document.Layers[layerIndex]).Surface)
        {
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, 
            int layerIndex, PdnRegion changedRegion, Surface copyFromThisSurface)
            : base(name, image)
        {
            this.workspace = workspace;
            this.layerIndex = layerIndex;

            PdnRegion region = changedRegion.Clone();
            this.tempFileName = FileSystem.GetTempFileName();

            FileStream outputStream = null;
            
            try
            {
                outputStream = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Write);
                SaveSurfaceRegion(outputStream, copyFromThisSurface, region);
            }

            finally
            {
                if (outputStream != null)
                {
                    outputStream.Dispose();
                    outputStream = null;
                }
            }

            this.tempFileHandle = new DeleteFileOnFree(this.tempFileName);
            BitmapHistoryActionData data = new BitmapHistoryActionData(null, region);

            this.Data = data;
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, 
            int layerIndex, IrregularSurface saved)
            : this(name, image, workspace, layerIndex, saved, false)
        {
        }

        public BitmapHistoryAction(string name, Image image, DocumentWorkspace workspace, int layerIndex, 
            IrregularSurface saved, bool takeOwnershipOfSaved)
            : base(name, image)
        {
            this.workspace = workspace;
            this.layerIndex = layerIndex;

            IrregularSurface iss;

            if (takeOwnershipOfSaved)
            {
                iss = saved;
            }
            else
            {
                iss = (IrregularSurface)saved.Clone();
            }

            BitmapHistoryActionData data = new BitmapHistoryActionData(iss, null);
            this.Data = data;
        }

        protected override HistoryAction OnUndo()
        {
            BitmapHistoryActionData data = this.Data as BitmapHistoryActionData;
            BitmapLayer layer = (BitmapLayer)workspace.Document.Layers[this.layerIndex];
            
            PdnRegion region;
            MaskedSurface maskedSurface = null;

            if (this.poMaskedSurfaceRef != Guid.Empty)
            {
                PersistedObject<MaskedSurface> poMS = PersistedObjectLocker.Get<MaskedSurface>(this.poMaskedSurfaceRef);
                maskedSurface = poMS.Object;
                region = maskedSurface.CreateRegion();
            }
            else if (data.UndoImage == null)
            {
                region = data.SavedRegion;
            }
            else
            {
                region = data.UndoImage.Region;
            }

            BitmapHistoryAction redo;

            if (this.poUndoMaskedSurfaceRef == Guid.Empty)
            {
                redo = new BitmapHistoryAction(Name, Image, workspace, this.layerIndex, region);
                redo.poUndoMaskedSurfaceRef = this.poMaskedSurfaceRef;
            }
            else
            {
                redo = new BitmapHistoryAction(Name, Image, workspace, this.layerIndex, this.poUndoMaskedSurfaceRef);
            }

            PdnRegion simplified = Utility.SimplifyAndInflateRegion(region);

            if (maskedSurface != null)
            {
                maskedSurface.Draw(layer.Surface);
            }
            else if (data.UndoImage == null)
            {
                using (FileStream input = FileSystem.OpenStreamingFile(this.tempFileName, FileAccess.Read))
                {
                    LoadSurfaceRegion(input, layer.Surface, data.SavedRegion);
                }

                data.SavedRegion.Dispose();
                this.tempFileHandle.Dispose();
                this.tempFileHandle = null;
            }
            else
            {
                data.UndoImage.Draw(layer.Surface);
                data.UndoImage.Dispose();
            }

            layer.Invalidate(simplified);
            simplified.Dispose();

            return redo;
        }
    }
}
