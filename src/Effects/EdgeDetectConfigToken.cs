// EdgeDetectConfigToken.cs 
// effect parameter token passed from EdgeDetectConfigDialog to 
// the EdgeDetectEffect. builds the convolution filter weights according
// to specified angle
//
// Chris Crosetto

using PaintDotNet;
using PaintDotNet.Effects;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PaintDotNet.Effects
{
    /// <summary>
    /// Summary description for EdgeDetectConfigToken.
    /// </summary>
    public class EdgeDetectConfigToken
        : AngleChooserConfigToken
    {
        // the convolution filter weights
        private double[,] weights = null;
        public double[,] Weights
        {
            get
            {
                double[,] localWeights = this.weights;

                if (localWeights == null)
                {
                    // adjust and convert angle to radians
                    double r = (double)Angle * 2.0 * Math.PI / 360.0;

                    // angle delta for each weight
                    double dr = Math.PI / 4.0;

                    // for r = 0 this builds an edge detect filter pointing straight left
                    localWeights = new double[3, 3];

                    localWeights[0,0] = Math.Cos(r + dr);
                    localWeights[0,1] = Math.Cos(r + 2.0*dr);
                    localWeights[0,2] = Math.Cos(r + 3.0*dr);
                    
                    localWeights[1,0] = Math.Cos(r);
                    localWeights[1,1] = 0;                     
                    localWeights[1,2] = Math.Cos(r + 4.0*dr);
                    
                    localWeights[2,0] = Math.Cos(r - dr);
                    localWeights[2,1] = Math.Cos(r - 2.0*dr);
                    localWeights[2,2] = Math.Cos(r - 3.0*dr);

                    this.weights = localWeights;
                }

                return localWeights;
            }
        }

        protected override void OnAngleChanged()
        {
            weights = null;
            base.OnAngleChanged();
        }

        public override object Clone()
        {
            return new EdgeDetectConfigToken(this);
        }

        public EdgeDetectConfigToken(double angle)
            : base(angle)
        {
        }

        protected EdgeDetectConfigToken(EdgeDetectConfigToken copyMe)
            : base(copyMe)
        {
        }
    }
}
