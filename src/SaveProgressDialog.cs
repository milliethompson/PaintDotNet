using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PaintDotNet
{
    public class SaveProgressDialog
        : CallbackWithProgressDialog
    {
        private FileType fileType;
        private Document document;
        private Stream stream;

        private void SaveCallback()
        {
            ((ISaveWithProgress)fileType).SaveWithProgress(document, stream, new ProgressEventHandler(ProgressHandler));
        }

        public SaveProgressDialog(IWin32Window owner)
            : base(owner, "Saving", "Saving:")
        {
            this.Icon = Utility.ImageToIcon(Utility.GetImageResource("Icons.MenuFileSaveIcon.bmp"), Color.FromArgb(192, 192, 192));
        }

        public void Save(Stream stream, Document document, FileType fileType)
        {
            if (!(fileType is ISaveWithProgress))
            {
                throw new ArgumentException("fileType does not implement ISaveWithProgress");
            }

            this.document = document;
            this.fileType = fileType;
            this.stream = stream;
            DialogResult dr = this.ShowDialog(false, new ThreadStart(SaveCallback));
        }

        private void ProgressHandler(object sender, ProgressEventArgs e)
        {
            Progress = (int)e.Percent;
        }        
    }
}
