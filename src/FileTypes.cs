using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PaintDotNet
{
	/// <summary>
	/// Summary description for FileTypes.
	/// </summary>
	public sealed class FileTypes
	{
		private FileTypes()
		{
		}

        // This class handles the layered bitmap format
        [Serializable]
        private class LayeredBitmapFileType
            : FileType,
              ISaveWithProgress
        {
            public LayeredBitmapFileType()
                : base("Layered Bitmap", null, true, new string[] { ".lbmp" })
            {
            }

            protected override Document CustomLoad(Stream input)
            {
                return Document.FromStream(input);
            }

            protected override void CustomSave(Document input, Stream output)
            {
                input.SaveToStream(output);
            }

            private class UpdateProgressTranslator
            {
                private long maxBytes;
                private long totalBytes;
                private ProgressEventHandler callback;

                public void IOEventHandler(object sender, IOEventArgs e)
                {
                    totalBytes += (long)e.Count;
					double percent = Math.Max(0.0, Math.Min(100.0, ((double)totalBytes * 100.0) / (double)maxBytes));
                    callback(sender, new ProgressEventArgs(percent));
                }

                public UpdateProgressTranslator(long maxBytes, ProgressEventHandler callback)
                {
                    this.maxBytes = maxBytes;
                    this.callback = callback;
                    this.totalBytes = 0;
                }
            }

            public void SaveWithProgress(Document input, Stream output, ProgressEventHandler callback)
            {
                UpdateProgressTranslator upt = new UpdateProgressTranslator(ApproximateMaxOutputOffset(input), callback);
                input.SaveToStream(output, new IOEventHandler(upt.IOEventHandler));
            }

            private long ApproximateMaxOutputOffset(Document measureMe)
            {
                return measureMe.Layers.Count * measureMe.Width * measureMe.Height * Marshal.SizeOf(typeof(ColorBgra));
            }
        }

        // Here are the static types
        public static readonly FileType Bmp = new FileType("Bitmap", ImageFormat.Bmp, false, new string[] { ".bmp" });
        public static readonly FileType Jpeg = new FileType("JPEG", ImageFormat.Jpeg, false, new string[] { ".jpg", ".jpeg", ".jpe", ".jfif" });
        public static readonly FileType Gif = new FileType("GIF", ImageFormat.Gif, false, new string[] { ".gif" });
        public static readonly FileType Tiff = new FileType("TIFF", ImageFormat.Tiff, false, new string[] { ".tif", ".tiff" });
        public static readonly FileType Png = new FileType("PNG", ImageFormat.Png, false, new string[] { ".png" });
        public static readonly FileType Lbmp = new LayeredBitmapFileType();
	}
}
