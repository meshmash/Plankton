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

        // by dyliu, not used yet
        //public int EndVertex;
        //public int PairHalfEdge;  // either +1 or -1 
        public int Index;

        public double angleToX;
        public double angleToY;


        internal PlanktonHalfedge()
        {
            StartVertex = -1;
            //EndVertex = -1;
            AdjacentFace = -1;
            NextHalfedge = -1;
            PrevHalfedge = -1;
            //PairHalfEdge = 
        }
        


        internal PlanktonHalfedge(int StartV, int AdjFace, int NextE)
        {
            StartVertex = StartV;
            AdjacentFace = AdjFace;
            NextHalfedge = NextE;
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
                    AdjacentFace = -1, // if true, this is a naked edge
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

        #region by dyliu
        //public Line ToLine()
        //{

        //    return new Line();

        //}

        #endregion

    }
}
