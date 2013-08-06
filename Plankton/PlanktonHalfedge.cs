using Rhino.Geometry;
using System;

namespace Plankton
{
    /// <summary>
    /// Description of PlanktonHalfedge.
    /// </summary>
    public class PlanktonHalfedge
    {
        //primary properties - the minimum ones needed for the halfedge structure
        public int StartVertex;
        public int AdjacentFace;
        public int NextHalfedge;
        //secondary properties - these should still be kept updated if you change the topology
        public int PrevHalfedge;
        public int Index;
        //tertiary properties - less vital, calculate or refresh only as needed
        public Vector3d Normal;

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
        public int Pair()
        {
            if (Index % 2 == 0)
            { return Index + 1; }
            else
            { return Index - 1; }
        }
    }
}
