using System;

namespace Plankton
{
    /// <summary>
    /// Represents a halfedge in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonHalfedge
    {
        //primary properties - the minimum ones needed for the halfedge structure
        public int StartVertex;
        public int AdjacentFace;
        public int NextHalfedge;
        public bool Dead;
        //secondary properties - these should still be kept updated if you change the topology
        public int PrevHalfedge;

        public PlanktonHalfedge()
        {
            StartVertex = -1;
            AdjacentFace = -1;
            NextHalfedge = -1;
            PrevHalfedge = -1;
        }
        public PlanktonHalfedge(int Start, int AdjFace, int Next)
        {
            StartVertex = Start;
            AdjacentFace = AdjFace;
            NextHalfedge = Next;
        }
    }
}
