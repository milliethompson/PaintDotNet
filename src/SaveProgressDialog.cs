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
        private SaveConfigToken saveConfigToken;

        private void SaveCallback()
        {
            fileType.Save(document, stream, saveConfigToken, new ProgressEventHandler(ProgressHandler), true);
        }

        public SaveProgressDialog(Control owner)
            : base(owner, 
                   PdnResources.GetString("SaveProgressDialog.Title"), 
                   PdnResources.GetString("SaveProgressDialog.Description"))
        {
            this.Icon = Utility.ImageToIcon(PdnResources.GetImage("Icons.MenuFileSaveIcon.bmp"), Color.FromArgb(192, 192, 192));
        }

        public void Save(Stream stream, Document document, FileType fileType, SaveConfigToken parameters)
        {
            this.document = document;
            this.fileType = fileType;
            this.stream = stream;
            this.saveConfigToken = parameters;
            DialogResult dr = this.ShowDialog(false, !fileType.SavesWithProgress, new ThreadStart(SaveCallback));
        }

        private void ProgressHandler(object sender, ProgressEventArgs e)
        {
            Progress = (int)e.Percent;
        }
    }
}
