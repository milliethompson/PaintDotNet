using System;
using System.ComponentModel;

namespace PaintDotNet
{
    [Flags]
    public enum ShapeDrawType
    {
        Interior = 1,
        Outline = 2,
        Both = 3
    }
}
