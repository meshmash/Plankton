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
        /// <param name="owner">The mesh to which this list of halfedges belongs.</param>
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
        internal int AddPair(int start, int end, int face)
        {
            // he->next = he->pair
            int i = this.Count;
            this.Add(new PlanktonHalfedge(start, face, i + 1));
            this.Add(new PlanktonHalfedge(end, -1, i));
            return i;
        }

        /// <summary>
        /// Removes a pair of halfedges from the mesh.
        /// </summary>
        /// <param name="index">The index of a halfedge in the pair to remove.</param>
        /// <remarks>The halfedges are topologically disconnected from the mesh and marked for deletion.
        /// Note that this helper method doesn't update adjacent faces.</remarks>
        internal void RemovePairHelper(int index)
        {
            int pair = this.GetPairHalfedge(index);
            
            // Reconnect adjacent halfedges
            this.MakeConsecutive(this[pair].PrevHalfedge, this[index].NextHalfedge);
            this.MakeConsecutive(this[index].PrevHalfedge, this[pair].NextHalfedge);
            
            // Update vertices' outgoing halfedges, if necessary. If last halfedge then
            // make vertex unused (outgoing == -1), otherwise set to next around vertex.
            var v1 = _mesh.Vertices[this[index].StartVertex];
            var v2 = _mesh.Vertices[this[pair].StartVertex];
            if (v1.OutgoingHalfedge == index)
            {
                if (this[pair].NextHalfedge == index) { v1.OutgoingHalfedge = -1; }
                else { v1.OutgoingHalfedge = this[pair].NextHalfedge; }
            }
            if (v2.OutgoingHalfedge == pair)
            {
                if (this[index].NextHalfedge == pair) { v2.OutgoingHalfedge = -1; }
                else { v2.OutgoingHalfedge = this[index].NextHalfedge; }
            }
            
            // Mark halfedges for deletion
            this[index] = PlanktonHalfedge.Unset;
            this[pair] = PlanktonHalfedge.Unset;
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
            internal set
            {
                this._list[index] = value;
            }
        }
        #endregion

        /// <summary>
        /// Helper method to remove dead halfedges from the list, re-index and compact.
        /// </summary>
        internal void CompactHelper()
        {
            int marker = 0; // Location where the current halfedge should be moved to

            // Run through all the vertices
            for (int iter = 0; iter < _list.Count; iter++)
            {
                // If halfedge is alive, check if we need to shuffle it down the list
                if (!_list[iter].IsUnused)
                {
                    if (marker < iter)
                    {
                        // Room to shuffle. Copy current halfedge to marked slot.
                        _list[marker] = _list[iter];

                        // Update start vertex, if necessary
                        var vertex = _mesh.Vertices[_list[marker].StartVertex];
                        if (vertex.OutgoingHalfedge == iter) { vertex.OutgoingHalfedge = marker; }

                        // Update adjacent face, if necessary
                        if (_list[marker].AdjacentFace > -1)
                        {
                            var face = _mesh.Faces[_list[marker].AdjacentFace];
                            if (face.FirstHalfedge == iter) { face.FirstHalfedge = marker; }
                        }

                        // Update next/prev halfedges
                        _list[_list[marker].NextHalfedge].PrevHalfedge = marker;
                        _list[_list[marker].PrevHalfedge].NextHalfedge = marker;
                    }
                    marker++; // That spot's filled. Advance the marker.
                }
            }

            // Throw a fit if we've ended up with an odd number of halfedges
            // This could happen if only one of the halfedges in a pair was marked for deletion
            if (marker % 2 > 0) { throw new InvalidOperationException("Halfedge count was odd after compaction"); }

            // Trim list down to new size
            if (marker < _list.Count) { _list.RemoveRange(marker, _list.Count - marker); }
        }

        #region traversals
        /// <summary>
        /// Traverses clockwise around the starting vertex of a halfedge.
        /// </summary>
        /// <param name="halfedgeIndex">The index of a halfedge.</param>
        /// <returns>
        /// An enumerable of halfedge indices incident to the starting vertex of
        /// <paramref name="halfedgeIndex"/>. Ordered clockwise around the vertex.
        /// The returned enumerable will start with the specified halfedge.
        /// </returns>
        /// <remarks>Lazily evaluated so if you change the mesh topology whilst using
        /// this circulator, you'll know about it!</remarks>
        public IEnumerable<int> GetVertexCirculator(int halfedgeIndex)
        {
            if (halfedgeIndex < 0 || halfedgeIndex > this.Count) { yield break; }
            int h = halfedgeIndex;
            int count = 0;
            do
            {
                yield return h;
                h = this[this.GetPairHalfedge(h)].NextHalfedge;
                if (h < 0) { throw new InvalidOperationException("Unset index, cannot continue."); }
                if (count++ > 999) { throw new InvalidOperationException("Runaway vertex circulator"); }
            }
            while (h != halfedgeIndex);
        }

        /// <summary>
        /// Traverses anticlockwise around the adjacent face of a halfedge.
        /// </summary>
        /// <param name="halfedgeIndex">The index of a halfedge.</param>
        /// <returns>
        /// An enumerable of halfedge indices incident to the adjacent face of
        /// <paramref name="halfedgeIndex"/>. Ordered anticlockwise around the face.
        /// </returns>
        /// <remarks>Lazily evaluated so if you change the mesh topology whilst using
        /// this circulator, you'll know about it!</remarks>
        public IEnumerable<int> GetFaceCirculator(int halfedgeIndex)
        {
            if (halfedgeIndex < 0 || halfedgeIndex > this.Count) { yield break; }
            int h = halfedgeIndex;
            int count = 0;
            do
            {
                yield return h;
                h = this[h].NextHalfedge;
                if (h < 0) { throw new InvalidOperationException("Unset index, cannot continue."); }
                if (count++ > 999) { throw new InvalidOperationException("Runaway face circulator."); }
            }
            while (h != halfedgeIndex);
        }
        #endregion

        #region public helpers
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
            int halfedgeIndex = _mesh.Vertices[start].OutgoingHalfedge;
            foreach (int h in this.GetVertexCirculator(halfedgeIndex))
            {
                if (end == this[this.GetPairHalfedge(h)].StartVertex)
                    return h;
            }
            return -1;
        }

        /// <summary>
        /// Gets the opposing halfedge in a pair.
        /// </summary>
        /// <param name="index">A halfedge index.</param>
        /// <returns>The halfedge index with which the specified halfedge is paired.</returns>
        public int GetPairHalfedge(int halfedgeIndex)
        {
            if (halfedgeIndex < 0 || halfedgeIndex >= this.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return halfedgeIndex % 2 == 0 ? halfedgeIndex + 1 : halfedgeIndex - 1;
        }

        [Obsolete("PairHalfedge is deprecated, pease use GetPairHalfedge instead.")]
        public int PairHalfedge(int halfedgeIndex)
        {
            return this.GetPairHalfedge(halfedgeIndex);
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
            J = this[this.GetPairHalfedge(index)].StartVertex;

            return new int[]{ I, J };
        }

        /// <summary>
        /// Gets the halfedge a given number of 'next's around a face from a starting halfedge
        /// </summary>        
        /// <param name="startHalfEdge">The halfedge to start from</param>
        /// <param name="around">How many steps around the face. 0 returns the start_he</param>
        /// <returns>The resulting halfedge</returns>
        [Obsolete("GetNextHalfedge(int,int) is deprecated, please use" +
            "GetFaceCirculator(int).ElementAt(int) instead (LINQ).")]
        public int GetNextHalfEdge(int startHalfEdge,  int around)
        {
            int he_around = startHalfEdge;
            for (int i = 0; i < around; i++)
            {
                he_around = this[he_around].NextHalfedge;
            }
            return he_around;
        }

        /// <summary>
        /// A halfedge is on a boundary if it only has a face on one side.
        /// </summary>
        /// <param name="index">The index of a halfedge.</param>
        /// <returns><c>true</c> if the specified halfedge is on a boundary; otherwise, <c>false</c>.</returns>
        public bool IsBoundary(int index)
        {
            int pair = this.GetPairHalfedge(index);

            // Check for a face on both sides
            return (this[index].AdjacentFace == -1 || this[pair].AdjacentFace == -1);
        }

        /// <summary>
        /// Gets the index of the vertex at the <b>end</b> of a halfedge.
        /// </summary>
        /// <param name="index">The index of a halfedge.</param>
        /// <returns>The index of vertex at the end of the specified halfedge.</returns>
        /// <remarks>This helper actually returns the start vertex of the other halfedge in the pair.</remarks>
        public int EndVertex(int halfedgeIndex)
        {
            return this[GetPairHalfedge(halfedgeIndex)].StartVertex;
        }
        #endregion

        #region internal helpers
        internal void MakeConsecutive(int prev, int next)
        {
            this[prev].NextHalfedge = next;
            this[next].PrevHalfedge = prev;
        }
        #endregion

        #region Euler operators
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
            if (this[index].AdjacentFace < 0 || this[GetPairHalfedge(index)].AdjacentFace < 0)
                return false;
            
            // Make a note of some useful halfedges, along with 'index' itself
            int pair = this.GetPairHalfedge(index);
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
            int pair = this.GetPairHalfedge(index);

            // Create a copy of the existing vertex (user can move it afterwards if needs be)
            int end_vertex = this[pair].StartVertex;
            int new_vertex_index = _mesh.Vertices.Add(_mesh.Vertices[end_vertex].ToXYZ()); // use XYZ to copy            

            // Add a new halfedge pair
            int new_halfedge1 = this.AddPair(new_vertex_index, this.EndVertex(index), this[index].AdjacentFace);
            int new_halfedge2 = this.GetPairHalfedge(new_halfedge1);
            this[new_halfedge2].AdjacentFace = this[pair].AdjacentFace;

            // Link new pair into mesh
            this.MakeConsecutive(new_halfedge1, this[index].NextHalfedge);
            this.MakeConsecutive(index, new_halfedge1);
            this.MakeConsecutive(this[pair].PrevHalfedge, new_halfedge2);
            this.MakeConsecutive(new_halfedge2, pair);

            // Set new vertex's outgoing halfedge
            _mesh.Vertices[new_vertex_index].OutgoingHalfedge = new_halfedge1;

            // Change the start of the pair of the input halfedge to the new vertex
            this[pair].StartVertex = new_vertex_index;

            // Update end vertex's outgoing halfedge, if necessary 
            if (_mesh.Vertices[end_vertex].OutgoingHalfedge == pair)
            {
                _mesh.Vertices[end_vertex].OutgoingHalfedge = new_halfedge2;
            }

            return new_halfedge1;
        }

        /// <summary>
        /// Split 2 adjacent triangles into 4 by inserting a new vertex along the edge
        /// </summary>
        /// <param name="index">The index of the halfedge to split. Must be between 2 triangles.</param>        
        /// <returns>The index of the halfedge going from the new vertex to the end of the input halfedge, or -1 on failure</returns>
        public int TriangleSplitEdge(int index)
        {
            //split the edge
            // (I guess we could include a parameter for where along the edge to split)
            int new_halfedge = this.SplitEdge(index);
            int point_on_edge = this[new_halfedge].StartVertex;
             
            _mesh.Vertices[point_on_edge].X = 0.5F * (_mesh.Vertices[this[index].StartVertex].X + _mesh.Vertices[this.EndVertex(new_halfedge)].X);
            _mesh.Vertices[point_on_edge].Y = 0.5F * (_mesh.Vertices[this[index].StartVertex].Y + _mesh.Vertices[this.EndVertex(new_halfedge)].Y);
            _mesh.Vertices[point_on_edge].Z = 0.5F * (_mesh.Vertices[this[index].StartVertex].Z + _mesh.Vertices[this.EndVertex(new_halfedge)].Z);

            int new_face1 = _mesh.Faces.SplitFace(new_halfedge, this[this[new_halfedge].NextHalfedge].NextHalfedge);
            int new_face2 = _mesh.Faces.SplitFace(this.GetPairHalfedge(index), this[this[this.GetPairHalfedge(index)].NextHalfedge].NextHalfedge);

            return new_halfedge;
        }

        /// <summary>
        /// Collapse an edge by combining 2 vertices
        /// </summary>
        /// <param name="index">The index of a halfedge in the edge to collapse. The end vertex will be removed</param>        
        /// <returns>The successor to <paramref name="index"/> around its vertex, or -1 on failure.</returns>
        public int CollapseEdge(int index)
        {
            var fs = _mesh.Faces;
            int pair = this.GetPairHalfedge(index);
            int v_keep = this[index].StartVertex;
            int v_kill = this[pair].StartVertex;
            int f = this[index].AdjacentFace;
            int f_pair = this[pair].AdjacentFace;

            // Don't allow the creation of non-manifold vertices
            // This would happen if the edge is internal (face on both sides) and
            // both incident vertices lie on a boundary
            if (f > -1 && f_pair > -1)
            {
                if (this[_mesh.Vertices[v_keep].OutgoingHalfedge].AdjacentFace < 0 && 
                    this[_mesh.Vertices[v_kill].OutgoingHalfedge].AdjacentFace < 0)
                {
                    return -1;
                }
            }
            
            // Avoid creating a non-manifold edge...
            // If the edge is internal, then its ends must not have more than 2 neighbours in common.
            // If the edge is a boundary edge (or has one 3+ sided face), then its ends must not
            // have more than one neighbour in common.
            //int allowed = (f > -1 && f_pair > -1) ? 2 : 1;
            int allowed = 0;
            if (f >= 0 && fs.GetHalfedges(f).Length == 3) { allowed++; }
            if (f_pair >= 0 && fs.GetHalfedges(f_pair).Length == 3) { allowed++; }
            if (_mesh.Vertices.GetVertexNeighbours(v_keep)
                .Intersect(_mesh.Vertices.GetVertexNeighbours(v_kill)).Count() > allowed)
            {
                return -1;
            }
            
            // Save a couple of halfedges for later
            int next = this[index].NextHalfedge;
            int pair_prev = this[pair].PrevHalfedge;

            // Find the halfedges starting at the vertex we are about to remove
            // and reconnect them to the one we are keeping
            foreach (int h in this.GetVertexCirculator(next))
            {
                this[h].StartVertex = v_keep;
            }

            // Store return halfedge index (next around start vertex)
            int h_rtn = this[pair].NextHalfedge;

            // Set outgoing halfedge
            int v_kill_outgoing = _mesh.Vertices[v_kill].OutgoingHalfedge;
            if (this[v_kill_outgoing].AdjacentFace < 0 && v_kill_outgoing != pair)
                _mesh.Vertices[v_keep].OutgoingHalfedge = v_kill_outgoing;
            else if (_mesh.Vertices[v_keep].OutgoingHalfedge == index)
                _mesh.Vertices[v_keep].OutgoingHalfedge = h_rtn; // Next around vertex

            // Bypass both halfedges by linking prev directly to next for each
            this.MakeConsecutive(this[index].PrevHalfedge, next);
            this.MakeConsecutive(pair_prev, this[pair].NextHalfedge);

            // Update faces' first halfedges, if necessary
            int face = this[index].AdjacentFace;
            if (face != -1 && fs[face].FirstHalfedge == index)
                fs[face].FirstHalfedge = next;
            int pair_face = this[pair].AdjacentFace;
            if (pair_face != -1 && fs[pair_face].FirstHalfedge == pair)
                fs[pair_face].FirstHalfedge = this[pair].NextHalfedge;
            
            // If either adjacent face was triangular it will now only have two sides. If so,
            // try to merge faces into whatever is on the RIGHT of the associated halfedge.
            if (f > -1 && this.GetFaceCirculator(next).Count() < 3)
            {
                if (fs.MergeFaces(this.GetPairHalfedge(next)) < 0) { fs.RemoveFace(face); }
            }
            if (f_pair > -1 && this.GetFaceCirculator(pair_prev).Count() < 3)
            {
                if (fs.MergeFaces(this.GetPairHalfedge(pair_prev)) < 0) { fs.RemoveFace(pair_face); }
            }

            // Kill the halfedge pair and its end vertex
            this[index] = PlanktonHalfedge.Unset;
            this[pair] = PlanktonHalfedge.Unset;
            _mesh.Vertices[v_kill] = PlanktonVertex.Unset;

            return h_rtn;
        }
        #endregion
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
