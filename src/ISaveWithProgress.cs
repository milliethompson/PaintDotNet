using System;
using System.IO;

namespace PaintDotNet
{
    public interface ISaveWithProgress
    {
        void SaveWithProgress(Document input, Stream output, ProgressEventHandler callback);
    }
}
