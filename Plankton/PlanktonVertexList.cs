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
        /// <param name="ownerMesh">The mesh to which this list of vertices belongs.</param>
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
            private set
            {
                this._list[index] = value;
            }
        }
        #endregion
        
        #region traversals
        /// <summary>
        /// Gets the halfedges which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The indices of halfedges incident to a particular vertex.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetHalfedges(int v)
        {
            return this.GetHalfedgesCirculator(v).ToArray();
        }
        
        /// <summary>
        /// Traverses the halfedge indices which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.</returns>
        public IEnumerable<int> GetHalfedgesCirculator(int v)
        {
            int he_first = this[v].OutgoingHalfedge;
            if (he_first < 0) yield break; // vertex has no connectivity, exit
            int he_current = he_first;
            var hs = _mesh.Halfedges;
            do
            {
                yield return he_current;
                he_current = hs[hs.PairHalfedge(he_current)].NextHalfedge;
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
                h = hs[hs.PairHalfedge(h)].NextHalfedge;
            }
            while (h != first);
        }
        #endregion

        #region adjacency queries
        /// <summary>
        /// Gets the halfedges which end at a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The opposing halfedge for each returned by <see cref="GetHalfedges(int)"/>.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetIncomingHalfedges(int v)
        {
            return this.GetHalfedgesCirculator(v)
                .Select(h => _mesh.Halfedges.PairHalfedge(h)).ToArray();
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
            return this.GetHalfedgesCirculator(v)
                .Select(h => hs[hs.PairHalfedge(h)].StartVertex).ToArray();
        }
        
        /// <summary>
        /// Gets faces incident to a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An array of face indices incident to the specified vertex.
        /// Ordered clockwise around the vertex</returns>
        public int[] GetVertexFaces(int v)
        {
            return this.GetHalfedgesCirculator(v)
                .Select(h => _mesh.Halfedges[h].AdjacentFace).ToArray();
        }

        /// <summary>
        /// Gets the first <b>incoming</b> halfedge for a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The index of the halfedge paired with the specified vertex's .</returns>
        public int GetIncomingHalfedge(int v)
        {
            return _mesh.Halfedges.PairHalfedge(this[v].OutgoingHalfedge);
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
            foreach (int i in this.GetHalfedgesCirculator(v))
            {
                if (hs[i].AdjacentFace == -1 || hs[hs.PairHalfedge(i)].AdjacentFace == -1)
                    nakedCount++;
            }
            return nakedCount;
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
            foreach (int h in this.GetHalfedgesCirculator(v_old, second))
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
            int h_new_pair = hs.PairHalfedge(h_new);
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
                foreach (int h in this.GetHalfedgesCirculator(v_old))
                {
                    if (hs[h].AdjacentFace == -1) { this[v_old].OutgoingHalfedge = h; }
                }
            }

            // return the new vertex which starts at the existing vertex
            return h_new;
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