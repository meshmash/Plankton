using Rhino.Geometry;
using System;

namespace Plankton
{
    /// <summary>
    /// Description of PlanktonFace.
    /// </summary>
    public class PlanktonFace
    {
        public int FirstHalfedge;
        //
        public int EdgeCount;
        public Vector3d Normal;
        public double Area;
        
        public PlanktonFace()
        {
            FirstHalfedge = -1;
        }
    }
}
