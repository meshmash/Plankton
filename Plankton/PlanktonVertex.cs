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
            : this()
        {
            Position = (Point3d)V;
        }
        public PlanktonVertex(Point3d V)
            : this()
        {
            Position = V;
        }
        public PlanktonVertex(double x, double y, double z)
            : this()
        {
            Position = new Point3d(x, y, z);
        }
        public PlanktonVertex(float x, float y, float z)
            : this()
        {
            Position = new Point3d(x, y, z);
        }
    }
}
