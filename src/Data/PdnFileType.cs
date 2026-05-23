/////////////////////////////////////////////////////////////////////////////////
// Paint.NET
// Copyright (C) Rick Brewster, Chris Crosetto, Dennis Dietrich, Tom Jackson, 
//               Michael Kelsey, Brandon Ortiz, Craig Taylor, Chris Trevino, 
//               and Luke Walker
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
        : FileType
    {
        public PdnFileType()
            : base(PdnResources.GetString("Application.ProductName"), true, true, true, true, true, new string[] { ".pdn" })
        {
        }

        protected override Document OnLoad(Stream input)
        {
            return Document.FromStream(input);
        }

        protected override void OnSave(Document input, Stream output, SaveConfigToken token, ProgressEventHandler callback)
        {
            if (callback == null)
            {
                input.SaveToStream(output);
            }
            else
            {
                UpdateProgressTranslator upt = new UpdateProgressTranslator(ApproximateMaxOutputOffset(input), callback);
                input.SaveToStream(output, new IOEventHandler(upt.IOEventHandler));
            }
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

        private long ApproximateMaxOutputOffset(Document measureMe)
        {
            return (long)measureMe.Layers.Count * (long)measureMe.Width * (long)measureMe.Height * (long)ColorBgra.SizeOf;
        }
    }
}
