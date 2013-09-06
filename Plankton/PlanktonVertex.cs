using System;

namespace Plankton
{
    /// <summary>
    /// Represents a vertex in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonVertex
    {
        public int OutgoingHalfedge;
        
        internal PlanktonVertex()
        {
            this.OutgoingHalfedge = -1;
        }
        
        internal PlanktonVertex(float x, float y, float z)
        {
            OutgoingHalfedge = -1;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        
        internal PlanktonVertex(double x, double y, double z)
            : this((float) x, (float) y, (float) z)
        {
            // empty
        }

        public float X { get; set; }
        
        public float Y { get; set; }
        
        public float Z { get; set; }

        public PlanktonXYZ ToXYZ()
        {
            return new PlanktonXYZ(this.X, this.Y, this.Z);
        }

        /// <summary>
        /// Gets an unset PlanktonVertex. Unset vertices have an outgoing halfedge index of -1.
        /// </summary>
        public static PlanktonVertex Unset
        {
            get { return new PlanktonVertex() { OutgoingHalfedge = -1 }; }
        }
        
        /// <summary>
        /// Whether or not the vertex is currently being referenced in the mesh.
        /// </summary>
        public bool IsUnused { get { return (this.OutgoingHalfedge < 0); } }
        
        [Obsolete()]
        public bool Dead { get { return this.IsUnused; } }
    }
}
