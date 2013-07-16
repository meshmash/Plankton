using Rhino.Geometry;
using System;

namespace Plankton
{
    /// <summary>
    /// Description of PlanktonVertex.
    /// </summary>
    public class PlanktonVertex
    {
        public Point3d Position;
        public int OutgoingHalfedge;
        //       
        public bool Dead;
        public Vector3d Normal;
        public PlanktonVertex()
        {
            OutgoingHalfedge = -1;
        }
        public PlanktonVertex(Point3f V)
        {
            Position = (Point3d)V;
            OutgoingHalfedge = -1;
        }
        public PlanktonVertex(Point3d V)
        {
            Position = V;
            OutgoingHalfedge = -1;
        }
    }
}
