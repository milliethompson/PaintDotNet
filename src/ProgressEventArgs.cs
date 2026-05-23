using System;

namespace PaintDotNet
{
    /// <summary>
    /// Summary description for ProgressEventArgs.
    /// </summary>
    public class ProgressEventArgs
        : System.EventArgs
    {
        private double percent;
        public double Percent
        {
            get
            {
                return percent;
            }
        }

        public ProgressEventArgs(double percent)
        {
            this.percent = percent;
        }
    }
}
