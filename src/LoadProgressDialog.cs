using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class LoadProgressDialog
                : CallbackWithProgressDialog
    {
        private SiphonStream siphonStream;
        private FileType fileType;
        private Document document;
        private long totalBytes;

        private void LoadCallback()
        {
            document = fileType.Load(siphonStream);
        }

        public LoadProgressDialog(IWin32Window owner, Stream stream, FileType fileType)
            : base(owner, Application.ProductName, "Loading:")
        {
            this.fileType = fileType;
            this.siphonStream = new SiphonStream(stream);
            this.siphonStream.IOFinished += new IOEventHandler(siphonStream_IOFinished);
        }

        public Document Load()
        {
            totalBytes = 0;
            DialogResult dr = this.ShowDialog(false, new ThreadStart(LoadCallback));
            return document;
        }

        public Document Load(Point screenPos)
        {
            this.StartPos = screenPos;
            return Load();
        }

        private void siphonStream_IOFinished(object sender, IOEventArgs e)
        {
            totalBytes += (long)e.Count;
            Progress = Math.Max(0, Math.Min(100, (int)((totalBytes * 100) / siphonStream.Length))); 
        }
    }
}
