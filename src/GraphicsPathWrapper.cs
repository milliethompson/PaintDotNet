using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace PaintDotNet
{
	/// <summary>
	/// Encapsulates the data necessary to create a GraphicsPath object
	/// so that we can serialize one to the clipboard (or anywhere else
	/// for that matter)
	/// </summary>
	[Serializable]
	public class GraphicsPathWrapper
	{
        private PointF[] points;
        private byte[] types;
        private FillMode fillMode;

        public GraphicsPath CreateGraphicsPath()
        {
            return new GraphicsPath(points, types, fillMode);
        }

		public GraphicsPathWrapper(GraphicsPath path)
        {
            points = (PointF[])path.PathPoints.Clone();
            types = (byte[])path.PathTypes.Clone();
            fillMode = path.FillMode;
		}
	}
}
