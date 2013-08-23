using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Plankton
{
    /// <summary>
    /// Provides access to the halfedges and Halfedge related functionality of a Mesh.
    /// </summary>
    public class PlanktonHalfEdgeList : IEnumerable<PlanktonHalfedge>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonHalfedge> _list;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonHalfedgeList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="ownerMesh">The mesh to which this list of halfedges belongs.</param>
        internal PlanktonHalfEdgeList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonHalfedge>();
            this._mesh = owner;
        }
        
        /// <summary>
        /// Gets the number of halfedges.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }
        
        #region methods
        #region halfedge access
        /// <summary>
        /// Adds a new halfedge to the end of the Halfedge list.
        /// </summary>
        /// <param name="halfEdge">Halfedge to add.</param>
        /// <returns>The index of the newly added halfedge.</returns>
        public int Add(PlanktonHalfedge halfedge)
        {
            if (halfedge == null) return -1;
            this._list.Add(halfedge);
            return this.Count - 1;
        }
        
        /// <summary>
        /// Add a pair of halfedges to the mesh.
        /// </summary>
        /// <param name="start">A vertex index (from which the first halfedge originates).</param>
        /// <param name="end">A vertex index (from which the second halfedge originates).</param>
        /// <param name="face">A face index (adjacent to the first halfedge).</param>
        /// <returns>The index of the first halfedge in the pair.</returns>
        public int AddPair(int start, int end, int face)
        {
            // he->next = he->pair
            int i = this.Count;
            this.Add(new PlanktonHalfedge(start, face, i + 1));
            this.Add(new PlanktonHalfedge(end, -1, i));
            return i;
        }
        
        /// <summary>
        /// Returns the halfedge at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of halfedge to get.
        /// Must be larger than or equal to zero and smaller than the Halfedge Count of the mesh.
        /// </param>
        /// <returns>The halfedge at the given index.</returns>
        public PlanktonHalfedge this[int index]
        {
            get
            {
                return this._list[index];
            }
            private set
            {
                this._list[index] = value;
            }
        }
        
        /// <summary>
        /// Gets the opposing halfedge in a pair.
        /// </summary>
        /// <param name="index">A halfedge index.</param>
        /// <returns>The halfedge index with which the specified halfedge is paired.</returns>
        public int PairHalfedge(int index)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new IndexOutOfRangeException();
            }
            
            return index % 2 == 0 ? index + 1 : index - 1;
        }
        
        /// <summary>
        /// Gets the two vertices for a halfedge.
        /// </summary>
        /// <param name="index">A halfedge index.</param>
        /// <returns>The pair of vertex indices connected by the specified halfedge.
        /// The order follows the direction of the halfedge.</returns>
        public int[] GetVertices(int index)
        {
            int I, J;
            I = this[index].StartVertex;
            J = this[this.PairHalfedge(index)].StartVertex;
            
            return new int[]{ I, J };
        }
        #endregion
        
        /// <summary>
        /// Gets the halfedge index between two vertices.
        /// </summary>
        /// <param name="start">A vertex index.</param>
        /// <param name="end">A vertex index.</param>
        /// <returns>If it exists, the index of the halfedge which originates
        /// from <paramref name="start"/> and terminates at <paramref name="end"/>.
        /// Otherwise -1 is returned.</returns>
        public int FindHalfedge(int start, int end)
        {
            foreach (int h in _mesh.Vertices.GetHalfedgesCirculator(start))
            {
                if (end == this[this.PairHalfedge(h)].StartVertex)
                    return h;
            }
            return -1;
        }

        /// <summary>
        /// Gets the halfedge a given number of 'next's around a face from a starting halfedge
        /// </summary>        
        /// <param name="startHalfEdge">The halfedge to start from</param>
        /// <param name="around">How many steps around the face. 0 returns the start_he</param>
        /// <returns>The resulting halfedge</returns>
        /// 
        public int GetNextHalfEdge(int startHalfEdge,  int around)
        {
            int he_around = startHalfEdge;
            for (int i = 0; i < around; i++)
            {
                he_around = this[he_around].NextHalfedge;                
            }
            return he_around;
        }
        
        internal int EndVertex(int index)
        {
            return this[PairHalfedge(index)].StartVertex;
        }
        
        internal void MakeConsecutive(int prev, int next)
        {
            this[prev].NextHalfedge = next;
            this[next].PrevHalfedge = prev;
        }
        
        /// <summary>
        /// Performs an edge flip. This works by shifting the start/end vertices of the edge
        /// anticlockwise around their faces (by one vertex) and as such can be applied to any
        /// n-gon mesh, not just triangulations.
        /// </summary>
        /// <param name="index">The index of a halfedge in the edge to be flipped.</param>
        /// <returns>True on success, otherwise false.</returns>
        public bool FlipEdge(int index)
        {
            // Don't allow if halfedge is on a boundary
            if (this[index].AdjacentFace < 0 || this[PairHalfedge(index)].AdjacentFace < 0)
                return false;
            
            // Make a note of some useful halfedges, along with 'index' itself
            int pair = this.PairHalfedge(index);
            int next = this[index].NextHalfedge;
            int pair_next = this[pair].NextHalfedge;

            // Also don't allow if the edge that would be created by flipping already exists in the mesh
            if (FindHalfedge(EndVertex(pair_next), EndVertex(next)) != -1)
                return false;
            
            // to flip an edge
            // 6 nexts
            // 6 prevs
            this.MakeConsecutive(this[pair].PrevHalfedge, next);
            this.MakeConsecutive(index, this[next].NextHalfedge);
            this.MakeConsecutive(next, pair);
            this.MakeConsecutive(this[index].PrevHalfedge, pair_next);
            this.MakeConsecutive(pair, this[pair_next].NextHalfedge);
            this.MakeConsecutive(pair_next, index);
            // for each vert, check if need to update outgoing
            int v = this[index].StartVertex;
            if (_mesh.Vertices[v].OutgoingHalfedge == index)
                _mesh.Vertices[v].OutgoingHalfedge = pair_next;
            v = this[pair].StartVertex;
            if (_mesh.Vertices[v].OutgoingHalfedge == pair)
                _mesh.Vertices[v].OutgoingHalfedge = next;
            // for each face, check if need to update start he
            int f = this[index].AdjacentFace;
            if (_mesh.Faces[f].FirstHalfedge == next)
                _mesh.Faces[f].FirstHalfedge = index;
            f = this[pair].AdjacentFace;
            if (_mesh.Faces[f].FirstHalfedge == pair_next)
                _mesh.Faces[f].FirstHalfedge = pair;
            // update 2 start verts
            this[index].StartVertex = EndVertex(pair_next);
            this[pair].StartVertex = EndVertex(next);
            // 2 adjacentfaces
            this[next].AdjacentFace = this[pair].AdjacentFace;
            this[pair_next].AdjacentFace = this[index].AdjacentFace;
            
            return true;
        }

        /// <summary>
        /// Creates a new vertex, and inserts it along an existing edge, splitting it in 2.
        /// </summary>
        /// <param name="index">The index of a halfedge in the edge to be split.</param>
        /// <returns>The index of the newly created halfedge in the same direction as the input halfedge.</returns>
        public int SplitEdge(int index)
        {
            // add a new vertex   
            PlanktonVertex new_vertex = new PlanktonVertex();
            int new_vertex_index = _mesh.Vertices.Add(new_vertex);
            // add a new halfedge pair
            int new_halfedge1 = this.AddPair(this[index].StartVertex, this.EndVertex(index), this[index].AdjacentFace);
            int new_halfedge2 = this.PairHalfedge(new_halfedge1);
            // update :
            // input he's next
            this[index].NextHalfedge = new_halfedge1;
            // input's pair's prev
            this[this.PairHalfedge(index)].PrevHalfedge = new_halfedge2;
            // new he's prev & next
            this[new_halfedge1].PrevHalfedge = index;
            this[new_halfedge1].NextHalfedge = this[index].NextHalfedge;
            // new he's pair's prev, next, adjface
            this[new_halfedge2].PrevHalfedge = this[this.PairHalfedge(index)].PrevHalfedge;
            this[new_halfedge2].NextHalfedge = this.PairHalfedge(index);
            this[new_halfedge2].AdjacentFace = this[this.PairHalfedge(index)].AdjacentFace;
            // new vert's outgoing he
            _mesh.Vertices[new_vertex_index].OutgoingHalfedge = new_halfedge1;
            // end vert's outgoing 
            if (_mesh.Vertices[this.EndVertex(index)].OutgoingHalfedge == this.PairHalfedge(index))
            { _mesh.Vertices[this.EndVertex(index)].OutgoingHalfedge = new_halfedge2; }

            return new_halfedge1;
        }        

        /// <summary>
        /// Remove an edge and combine its 2 adjacent faces into 1
        /// </summary>
        /// <param name="index">The index of a halfedge in the edge to remove</param>        
        /// <returns>The index of the removed face, or -1 on failure</returns>
        public int RemoveEdge(int index)
        {
            //TODO : add special treatment for boundary halfedges
            //TODO : deal with case where the 2 faces share more than one edge
            this[index].Dead = true;
            this[this.PairHalfedge(index)].Dead = true;
            //Keep the adjacent face, but remove the pair's adjacent face
            int Pair_HE = this.PairHalfedge(index);
            int PairFace = this[Pair_HE].AdjacentFace;
            int KeptFace = this[index].AdjacentFace;
            _mesh.Faces[PairFace].Dead = true;

            //go around the dead face, reassigning adjacency
            int next_he_around = this[Pair_HE].NextHalfedge;
            while (next_he_around != Pair_HE)
            {
                this[next_he_around].AdjacentFace = KeptFace;
                next_he_around = this[next_he_around].NextHalfedge;
            }

            //make combined face halfedges consecutive
            this.MakeConsecutive(this[Pair_HE].PrevHalfedge, this[index].NextHalfedge);
            this.MakeConsecutive(this[index].PrevHalfedge, this[Pair_HE].NextHalfedge);

            //reassign the start and end vert's outgoing halfedges and the face's first halfedge,
            //in case they were one of the ones we've just removed
            _mesh.Vertices[this.EndVertex(index)].OutgoingHalfedge = this[index].NextHalfedge;
            _mesh.Vertices[this[index].StartVertex].OutgoingHalfedge = this[this.PairHalfedge(index)].NextHalfedge;
            _mesh.Faces[KeptFace].FirstHalfedge = this[index].NextHalfedge;

            return PairFace;
        }

        /// <summary>
        /// Collapse an edge by combining 2 vertices
        /// </summary>
        /// <param name="index">The index of a halfedge in the edge to collapse. The end vertex will be removed</param>        
        /// <returns>True on success, otherwise False.</returns>
        public bool CollapseEdge(int index)
        {
            // TODO : Add some more checks in here to prevent creating non-manifold meshes, such as if the edge is not naked but its ends are
            // TODO : Add treatment for boundary edges

            //Find the halfedges starting at the vertex we are about to remove
            //and reconnect them to the one we are keeping
            int[] Outgoing = _mesh.Vertices.GetHalfedges(this.EndVertex(index));            

            for (int i = 0; i < Outgoing.Length ; i++)
            {
                this[Outgoing[i]].StartVertex = this[index].StartVertex;                
            }

            //Kill the edge pair and its end
            this[index].Dead = true;
            this[this.PairHalfedge(index)].Dead = true;
            _mesh.Vertices[this.EndVertex(index)].Dead = true;           

            //Make sure the OutgoingHalfEdge is one that still exists
            _mesh.Vertices[this[index].StartVertex].OutgoingHalfedge = this.PairHalfedge(this[index].PrevHalfedge);

            int pair_he = this.PairHalfedge(index);
            int next_he = this[index].NextHalfedge;
            int next_pair = this.PairHalfedge(next_he);

            if (this.GetNextHalfEdge(index, 3) == index)  // if adjacent face is a triangle, we need to get rid of another edge
            {                                
                this.RemoveEdge(next_pair);
                //remake the prevs and nexts:
                int prev_he = this[index].PrevHalfedge;
                this.MakeConsecutive(prev_he, this[next_pair].NextHalfedge);
                _mesh.Faces[this[next_pair].AdjacentFace].FirstHalfedge = this[index].PrevHalfedge;                
            }
            else
            {
                this.MakeConsecutive(this[index].PrevHalfedge, this[index].NextHalfedge);
            }
            //same for the other side:            
            if (this.GetNextHalfEdge(pair_he, 3) == pair_he)
            {
                this.RemoveEdge(this.PairHalfedge(this[pair_he].PrevHalfedge));
                //remake the prevs and nexts:
                int pair_next_he = this[pair_he].NextHalfedge;
                int pair_prev_he = this[pair_he].PrevHalfedge;
                this.MakeConsecutive(this[this.PairHalfedge(pair_prev_he)].PrevHalfedge, pair_next_he);
                _mesh.Faces[this[this.PairHalfedge(pair_prev_he)].AdjacentFace].FirstHalfedge = pair_next_he;
            }
            else
            {
                this.MakeConsecutive(this[pair_he].PrevHalfedge, this[pair_he].NextHalfedge);
            }

            return true;
        }
        #endregion
        
        #region IEnumerable implementation
        /// <summary>
        /// Gets an enumerator that yields all halfedges in this collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<PlanktonHalfedge> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}