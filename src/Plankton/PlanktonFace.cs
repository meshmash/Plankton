using System;

namespace Plankton
{
    /// <summary>
    /// Represents a face in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonFace
    {
        public int FirstHalfedge;
        
        public PlanktonFace()
        {
            this.FirstHalfedge = -1;
        }
        
        internal PlanktonFace(int halfedgeIndex)
        {
            this.FirstHalfedge = halfedgeIndex;
        }
        
        /// <summary>
        /// Gets an unset PlanktonFace. Unset faces have -1 for their first halfedge index.
        /// </summary>
        public static PlanktonFace Unset
        {
            get { return new PlanktonFace() { FirstHalfedge = -1 }; }
        }
        
        /// <summary>
        /// Whether or not the face is currently being referenced in the mesh.
        /// </summary>
        public bool IsUnused { get { return (this.FirstHalfedge < 0); } }
        
        [Obsolete()]
        public bool Dead { get { return this.IsUnused; } }
    }
}
