using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Plankton
{
    /// <summary>
    /// Provides access to the vertices and Vertex related functionality of a Mesh.
    /// </summary>
    public class PlanktonVertexList : IEnumerable<PlanktonVertex>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonVertex> _list;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonVertexList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="owner">The mesh to which this list of vertices belongs.</param>
        internal PlanktonVertexList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonVertex>();
            this._mesh = owner;
        }
        
        /// <summary>
        /// Gets the number of vertices.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }
        
        #region methods
        #region vertex access
        #region adding
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonVertex vertex)
        {
            if (vertex == null) return -1;
            this._list.Add(vertex);
            return this.Count - 1;
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonXYZ vertex)
        {            
            this._list.Add(new PlanktonVertex(vertex.X,vertex.Y,vertex.Z));
            return this.Count - 1;
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(double x, double y, double z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(float x, float y, float z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }
        #endregion
        
        /// <summary>
        /// Returns the vertex at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of vertex to get.
        /// Must be larger than or equal to zero and smaller than the Vertex Count of the mesh.
        /// </param>
        /// <returns>The vertex at the given index.</returns>
        public PlanktonVertex this[int index]
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
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, float x, float y, float z)
        {
            if (vertexIndex >= 0 && vertexIndex < _list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = x;
                v.Y = y;
                v.Z = z;
            }
            else if (vertexIndex == _list.Count)
            {
                this.Add(x, y, z);
            }
            else { return false; }
            
            return true;
        }
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, double x, double y, double z)
        {
            if (vertexIndex >= 0 && vertexIndex < _list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = (float)x;
                v.Y = (float)y;
                v.Z = (float)z;
            }
            else if (vertexIndex == _list.Count)
            {
                this.Add(x, y, z);
            }
            else { return false; }
            
            return true;
        }
        #endregion
        
        /// <summary>
        /// Helper method to remove dead vertices from the list, re-index and compact.
        /// </summary>
        internal int CompactHelper()
        {
            int marker = 0; // Location where the current vertex should be moved to
            
            // Run through all the vertices
            for (int iter = 0; iter < _list.Count; iter++)
            {
                // If vertex is alive, check if we need to shuffle it down the list
                if (!_list[iter].IsUnused)
                {
                    if (marker < iter)
                    {
                        // Room to shuffle. Copy current vertex to marked slot.
                        _list[marker] = _list[iter];
                        
                        // Update all halfedges which start here
                        int first = _list[marker].OutgoingHalfedge;
                        foreach (int h in _mesh.Halfedges.GetVertexCirculator(first))
                        {
                            _mesh.Halfedges[h].StartVertex = marker;
                        }
                    }
                    marker++; // That spot's filled. Advance the marker.
                }
            }
            
            // Trim list down to new size
            if (marker < _list.Count) { _list.RemoveRange(marker, _list.Count - marker); }
            
            return _list.Count - marker;
        }

        /// <summary>
        /// Removes all vertices that are currently not used by the Halfedge list.
        /// </summary>
        /// <returns>The number of unused vertices that were removed.</returns>
        public int CullUnused()
        {
            return this.CompactHelper();
        }
        
        #region traversals
        /// <summary>
        /// Traverses the halfedge indices which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.</returns>
        [Obsolete("GetHalfedgesCirculator(int) is deprecated, please use" +
                  "Halfedges.GetVertexCirculator(int) instead.")]
        public IEnumerable<int> GetHalfedgesCirculator(int v)
        {
            int he_first = this[v].OutgoingHalfedge;
            if (he_first < 0) yield break; // vertex has no connectivity, exit
            int he_current = he_first;
            var hs = _mesh.Halfedges;
            do
            {
                yield return he_current;
                he_current = hs[hs.GetPairHalfedge(he_current)].NextHalfedge;
            }
            while (he_current != he_first);
        }
        
        /// <summary>
        /// Traverses the halfedge indices which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <param name="first">A halfedge index. Halfedge must start at the specified vertex.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.
        /// The returned enumerable will start with the specified halfedge.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The specified halfedge does not originate from the specified vertex.
        /// </exception>
        [Obsolete("GetHalfedgesCirculator(int,int) is deprecated, please use" +
            "Halfedges.GetVertexCirculator(int) instead.")]
        public IEnumerable<int> GetHalfedgesCirculator(int v, int first)
        {
            if (_mesh.Halfedges[first].StartVertex != v)
                throw new ArgumentOutOfRangeException("Halfedge does not start at vertex.");
            // TODO: The code below is the same as above.
            // Can we refactor (without extra, unnecessary iterators)?
            int h = first;
            var hs = _mesh.Halfedges;
            do
            {
                yield return h;
                h = hs[hs.GetPairHalfedge(h)].NextHalfedge;
            }
            while (h != first);
        }
        #endregion

        #region adjacency queries
        /// <summary>
        /// Gets the halfedges which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The indices of halfedges incident to a particular vertex.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetHalfedges(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge).ToArray();
        }

        /// <summary>
        /// Gets the halfedges which end at a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The opposing halfedge for each returned by <see cref="GetHalfedges(int)"/>.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetIncomingHalfedges(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => _mesh.Halfedges.GetPairHalfedge(h)).ToArray();
        }
        
        /// <summary>
        /// Gets vertex neighbours (a.k.a. 1-ring).
        /// </summary>
        /// <param name="f">A vertex index.</param>
        /// <returns>An array of vertex indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetVertexNeighbours(int v)
        {
            var hs = _mesh.Halfedges;
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => hs[hs.GetPairHalfedge(h)].StartVertex).ToArray();
        }
        
        /// <summary>
        /// Gets faces incident to a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An array of face indices incident to the specified vertex.
        /// Ordered clockwise around the vertex</returns>
        public int[] GetVertexFaces(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => _mesh.Halfedges[h].AdjacentFace).ToArray();
        }

        /// <summary>
        /// Gets the first <b>incoming</b> halfedge for a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The index of the halfedge paired with the specified vertex's .</returns>
        public int GetIncomingHalfedge(int v)
        {
            return _mesh.Halfedges.GetPairHalfedge(this[v].OutgoingHalfedge);
        }
        #endregion

        /// <summary>
        /// Gets the number of naked edges incident to this vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The number of incident halfedges which lie on a boundary.</returns>
        public int NakedEdgeCount(int v)
        {
            int nakedCount = 0;
            var hs = _mesh.Halfedges;
            foreach (int i in _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge))
            {
                if (hs[i].AdjacentFace == -1 || hs[hs.GetPairHalfedge(i)].AdjacentFace == -1)
                    nakedCount++;
            }
            return nakedCount;
        }

        /// <summary>
        /// Gets the number of edges incident to this vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The number of incident edges.</returns>
        public int GetValence(int v)
        {
            int h = this[v].OutgoingHalfedge;
            return _mesh.Halfedges.GetVertexCirculator(h).Count();
        }

        /// <summary>
        /// A vertex is on a boundary if its outgoing halfedge has no adjacent face.
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        /// <returns><c>true</c> if the specified vertex is on a boundary; otherwise, <c>false</c>.
        /// Also returns <c>true</c> if the vertex is unused (i.e. no outgoing halfedge).</returns>
        public bool IsBoundary(int index)
        {
            int h = this[index].OutgoingHalfedge;
            return (h < -1 || _mesh.Halfedges[h].AdjacentFace == -1);
        }

        #region Euler operators
        /// <summary>
        /// <para>Merges two vertices by collapsing the pair of halfedges between them.</para>
        /// <seealso cref="PlanktonHalfedgeList.CollapseEdge"/>
        /// </summary>
        /// <param name="halfedge">The index of a halfedge between the two vertices to be merged.
        /// The starting vertex of this halfedge will be retained.</param>
        /// <returns>The successor of <paramref name="index"/> around its vertex, or -1 on failure.</returns>
        /// <remarks>The invariant <c>mesh.Vertices.MergeVertices(mesh.Vertices.SplitVertex(a, b))</c> will return a,
        /// leaving the mesh unchanged.</remarks>
        public int MergeVertices(int halfedge)
        {
            return _mesh.Halfedges.CollapseEdge(halfedge);

        }

        /// <summary>
        /// Splits the vertex into two, joined by a new pair of halfedges.
        /// </summary>
        /// <param name="first">The index of a halfedge which starts at the vertex to split.</param>
        /// <param name="second">The index of a second halfedge which starts at the vertex to split.</param>
        /// <returns>The new halfedge which starts at the existing vertex.</returns>
        /// <remarks>After the split, the <paramref name="second"/> halfedge will be starting at the newly added vertex.</remarks>
        public int SplitVertex(int first, int second)
        {
            var hs = _mesh.Halfedges;
            // Check that both halfedges start at the same vertex
            int v_old = hs[first].StartVertex;
            if (v_old != hs[second].StartVertex) { return -1; } // TODO: return ArgumentException instead?

            // Create a copy of the existing vertex (user can move it afterwards if needs be)
            int v_new = this.Add(this[v_old].ToXYZ()); // copy vertex by converting to XYZ and back

            // Go around outgoing halfedges, from 'second' to just before 'first'
            // Set start vertex to new vertex
            bool reset_v_old = false;
            foreach (int h in hs.GetVertexCirculator(second))
            {
                if (h == first) { break; }
                hs[h].StartVertex = v_new;
                // If new vertex has no outgoing yet and current he is naked...
                if (this[v_new].OutgoingHalfedge == -1 && hs[h].AdjacentFace == -1)
                    this[v_new].OutgoingHalfedge = h;
                // Also check whether existing vert's he is now incident to new one
                if (h == this[v_old].OutgoingHalfedge) { reset_v_old = true; }
            }
            // If no naked halfedges, just use 'second'
            if (this[v_new].OutgoingHalfedge == -1) { this[v_new].OutgoingHalfedge = second; }

            // Add the new pair of halfedges from old vertex to new
            int h_new = hs.AddPair(v_old, v_new, hs[second].AdjacentFace);
            int h_new_pair = hs.GetPairHalfedge(h_new);
            hs[h_new_pair].AdjacentFace = hs[first].AdjacentFace;

            // Link new pair into mesh
            hs.MakeConsecutive(hs[first].PrevHalfedge, h_new_pair);
            hs.MakeConsecutive(h_new_pair, first);
            hs.MakeConsecutive(hs[second].PrevHalfedge, h_new);
            hs.MakeConsecutive(h_new, second);

            // Re-set existing vertex's outgoing halfedge, if necessary
            if (reset_v_old)
            {
                this[v_old].OutgoingHalfedge = h_new;
                foreach (int h in hs.GetVertexCirculator(h_new))
                {
                    if (hs[h].AdjacentFace == -1) { this[v_old].OutgoingHalfedge = h; }
                }
            }

            // return the new vertex which starts at the existing vertex
            return h_new;
        }

        /// <summary>
        /// Erases a vertex and all incident halfedges by merging its incident faces.
        /// </summary>
        /// <param name="halfedgeIndex">The index of a halfedge which starts at the vertex to erase.
        /// The retained face will be the one adjacent to this halfedge.</param>
        /// <returns>The successor of <paramref name="halfedgeIndex"/> around its original face.</returns>
        public int EraseCenterVertex(int halfedgeIndex)
        {
            int vertexIndex = _mesh.Halfedges[halfedgeIndex].StartVertex;

            // Check that the vertex is completely surrounded by faces
            if (this.IsBoundary(vertexIndex))
                throw new ArgumentException("Center vertex must not be on a boundary");

            // Get outgoing halfedges around vertex, starting with specified halfedge
            int[] vertexHalfedges = _mesh.Halfedges.GetVertexCirculator(halfedgeIndex).ToArray();

            // Check for 2-valent vertices in the 1-ring (no antennas)
            int v;
            foreach (int h in vertexHalfedges)
            {
                v = _mesh.Halfedges.EndVertex(h);
                if (this.GetHalfedges(v).Length < 3)
                    throw new ArgumentException("Vertex in 1-ring is 2-valent");
            }

            // Store face to keep and set its first halfedge
            int faceIndex = _mesh.Halfedges[halfedgeIndex].AdjacentFace;
            int firstHalfedge = _mesh.Halfedges[halfedgeIndex].NextHalfedge;
            _mesh.Faces[faceIndex].FirstHalfedge = firstHalfedge;

            // Remove incident halfedges and mark faces for deletion (except first face)
            _mesh.Halfedges.RemovePairHelper(vertexHalfedges[0]);
            for (int i = 1; i < vertexHalfedges.Length; i++)
            {
                _mesh.Faces[_mesh.Halfedges[vertexHalfedges[i]].AdjacentFace] = PlanktonFace.Unset;
                _mesh.Halfedges.RemovePairHelper(vertexHalfedges[i]);
            }

            // Set adjacent face for all halfedges in hole
            foreach (int h in _mesh.Halfedges.GetFaceCirculator(firstHalfedge))
            {
                _mesh.Halfedges[h].AdjacentFace = faceIndex;
            }

            // Mark center vertex for deletion
            this[vertexIndex] = PlanktonVertex.Unset;

            return _mesh.Faces[faceIndex].FirstHalfedge;
        }
        #endregion
        #endregion
        
        #region IEnumerable implementation
        /// <summary>
        /// Gets an enumerator that yields all faces in this collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<PlanktonVertex> GetEnumerator()
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