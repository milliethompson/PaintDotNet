/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Tom Jackson, Michael Kelsey, Brandon Ortiz,
//               Craig Taylor, Chris Trevino, and Luke Walker
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.
// See src/setup/License.rtf for complete licensing and attribution information.
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace PaintDotNet
{
    // This class handles the layered bitmap format
    [Serializable]
    public class PdnFileType
        : FileType,
          ISaveWithProgress
    {
        public PdnFileType()
            : base(System.Windows.Forms.Application.ProductName, true, true, new string[] { ".pdn" })
        {
        }

        public override Document Load(Stream input)
        {
            return Document.FromStream(input);
        }

        public override void Save(Document input, Stream output, SaveConfigToken token)
        {
            input.SaveToStream(output);
        }

        public override bool IsReflexive(SaveConfigToken token)
        {
            return true;
        }

        private sealed class UpdateProgressTranslator
        {
            private long maxBytes;
            private long totalBytes;
            private ProgressEventHandler callback;

            public void IOEventHandler(object sender, IOEventArgs e)
            {
                double percent;
                lock (this)
                {
                    totalBytes += (long)e.Count;
                    percent = Math.Max(0.0, Math.Min(100.0, ((double)totalBytes * 100.0) / (double)maxBytes));
                }

                callback(sender, new ProgressEventArgs(percent));
            }

            public UpdateProgressTranslator(long maxBytes, ProgressEventHandler callback)
            {
                this.maxBytes = maxBytes;
                this.callback = callback;
                this.totalBytes = 0;
            }
        }

        /// <summary>
        /// Saves a document and raises events that detail the progress of the operation.
        /// </summary>
        /// <param name="input">The document to save.</param>
        /// <param name="output">The stream to save to.</param>
        /// <param name="parameters">The parameters for the FileType.</param>
        /// <param name="callback">A callback to handle progress events. This event may be raised from any thread.</param>
        public void SaveWithProgress(Document input, Stream output, SaveConfigToken parameters, ProgressEventHandler callback)
        {
            UpdateProgressTranslator upt = new UpdateProgressTranslator(ApproximateMaxOutputOffset(input), callback);
            input.SaveToStream(output, new IOEventHandler(upt.IOEventHandler));
        }

        private long ApproximateMaxOutputOffset(Document measureMe)
        {
            return (long)measureMe.Layers.Count * (long)measureMe.Width * (long)measureMe.Height * (long)ColorBgra.SizeOf;
        }
    }
}
