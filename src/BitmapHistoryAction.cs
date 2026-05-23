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
        private string tempFileName;
        private DeleteFileOnFree tempFileHandle;

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
                    File.Delete(fileName);
                    this.bstrFileName = IntPtr.Zero;
                }
            }
        }

        [Serializable]
        private sealed class BitmapHistoryActionData
            : HistoryActionData
        {
            private int layerIndex;

            // only one of the following may be non-null
            private IrregularSurface undoImage;
            private PdnRegion savedRegion;

            public int LayerIndex
            {
                get
                {
                    return layerIndex;
                }
            }

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

            public BitmapHistoryActionData(int layerIndex, IrregularSurface undoImage, PdnRegion savedRegion)
            {
                if (undoImage != null && savedRegion != null)
                {
                    throw new ArgumentException("Only one of undoImage or savedRegion may be non-null");
                }

                this.layerIndex = layerIndex;
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

        private static void SaveSurfaceRegion(object outputHandle, Surface surface, PdnRegion region)
        {
            Rectangle[] scans = region.GetRegionScansReadOnlyInt();
            Rectangle bounds = surface.Bounds;
            int scanCount = 0;

            for (int i = 0; i < scans.Length; ++i)
            {
                Rectangle rect = scans[i];
                rect.Intersect(bounds);

                if (rect.Width != 0 && rect.Height != 0)
                {
                    scanCount += rect.Height;
                }
            }

            unsafe
            {
                int scanIndex = 0;
                void *[] ppvBuffers = new void *[scanCount];
                uint[] lengths = new uint[scanCount];

                for (int i = 0; i < scans.Length; ++i)
                {
                    Rectangle rect = scans[i];
                    rect.Intersect(bounds);

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

                FileSystem.WriteToStreamingFileGather(outputHandle, ppvBuffers, lengths);
            }
        }

        private static void LoadSurfaceRegion(FileStream input, Surface surface, PdnRegion region)
        {
            Rectangle[] scans = region.GetRegionScansReadOnlyInt();
            Rectangle bounds = surface.Bounds;
            int scanCount = 0;

            for (int i = 0; i < scans.Length; ++i)
            {
                Rectangle rect = scans[i];
                rect.Intersect(bounds);

                if (rect.Width != 0 && rect.Height != 0)
                {
                    scanCount += rect.Height;
                }
            }

            unsafe
            {
                int scanIndex = 0;
                void *[] ppvBuffers = new void *[scanCount];
                uint[] lengths = new uint[scanCount];

                for (int i = 0; i < scans.Length; ++i)
                {
                    Rectangle rect = scans[i];
                    rect.Intersect(bounds);

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

                FileSystem.ReadFromStreamScatter(input, ppvBuffers, lengths);
            }
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

            PdnRegion region = changedRegion.Clone();
            this.tempFileName = FileSystem.GetTempFileName();

            object outputHandle = null;
            
            try
            {
                outputHandle = FileSystem.CreateStreamingFileHandleWrite(this.tempFileName);
                SaveSurfaceRegion(outputHandle, copyFromThisSurface, region);
            }

            finally
            {
                if (outputHandle != null)
                {
                    FileSystem.CloseStreamingFileHandle(outputHandle);
                }
            }

            this.tempFileHandle = new DeleteFileOnFree(this.tempFileName);
            BitmapHistoryActionData data = new BitmapHistoryActionData(layerIndex, null, region);
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
            IrregularSurface iss;

            if (takeOwnershipOfSaved)
            {
                iss = saved;
            }
            else
            {
                iss = (IrregularSurface)saved.Clone();
            }

            BitmapHistoryActionData data = new BitmapHistoryActionData(layerIndex, iss, null);
            this.Data = data;
        }

        protected override HistoryAction OnUndo()
        {
            BitmapHistoryActionData data = (BitmapHistoryActionData)this.Data;
            BitmapLayer layer = (BitmapLayer)workspace.Document.Layers[data.LayerIndex];
            
            PdnRegion region;

            if (data.UndoImage == null)
            {
                region = data.SavedRegion;
            }
            else
            {
                region = data.UndoImage.Region;
            }

            BitmapHistoryAction redo = new BitmapHistoryAction(Name, Image, workspace, data.LayerIndex, region);

            if (data.UndoImage == null)
            {
                using (FileStream input = FileSystem.OpenStreamingFileRead(this.tempFileName))
                {
                    LoadSurfaceRegion(input, layer.Surface, data.SavedRegion);
                }

                using (PdnRegion simple = Utility.SimplifyAndInflateRegion(data.SavedRegion))
                {
                    layer.Invalidate(simple);
                }

                data.SavedRegion.Dispose();
                this.tempFileHandle.Dispose();
                this.tempFileHandle = null;
            }
            else
            {
                data.UndoImage.Draw(layer.Surface);

                using (PdnRegion simple = Utility.SimplifyAndInflateRegion(data.UndoImage.Region))
                {
                    layer.Invalidate(simple);
                }

                data.UndoImage.Dispose();
            }

            return redo;
        }
    }
}
