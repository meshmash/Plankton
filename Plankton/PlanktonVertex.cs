using System;

namespace Plankton
{
    /// <summary>
    /// Represents a vertex in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonVertex
    {
        public int OutgoingHalfedge;
        public bool Dead;
        
        public PlanktonVertex()
        {
            OutgoingHalfedge = -1;
        }
        public PlanktonVertex(double x, double y, double z)
            : this((float) x, (float) y, (float) z)
        {}
        public PlanktonVertex(float x, float y, float z)
            : this()
        {
            X = x; Y = y; Z = z;
        }

        public float X { get; set; }
        
        public float Y { get; set; }
        
        public float Z { get; set; }

        public PlanktonXYZ ToXYZ()
        {
            return new PlanktonXYZ(X, Y, Z);
        }

    }
}
