using System;

namespace Plankton
{
    /// <summary>
    /// Represents a halfedge in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonHalfedge
    {
        public int StartVertex;
        public int AdjacentFace;
        public int NextHalfedge;
        public int PrevHalfedge;

        internal PlanktonHalfedge()
        {
            StartVertex = -1;
            AdjacentFace = -1;
            NextHalfedge = -1;
            PrevHalfedge = -1;
        }
        
        internal PlanktonHalfedge(int Start, int AdjFace, int Next)
        {
            StartVertex = Start;
            AdjacentFace = AdjFace;
            NextHalfedge = Next;
        }
        
        /// <summary>
        /// Gets an Unset PlanktonHalfedge.
        /// </summary>
        public static PlanktonHalfedge Unset
        {
            get
            {
                return new PlanktonHalfedge()
                {
                    StartVertex = -1,
                    AdjacentFace = -1,
                    NextHalfedge = -1,
                    PrevHalfedge = -1
                };
            }
        }
        
        /// <summary>
        /// <para>Whether or not the vertex is currently being referenced in the mesh.</para>
        /// <para>Defined as a halfedge which has no starting vertex index.</para>
        /// </summary>
        public bool IsUnused { get { return (this.StartVertex < 0); } }
        
        [Obsolete()]
        public bool Dead { get { return this.IsUnused; } }
    }
}
