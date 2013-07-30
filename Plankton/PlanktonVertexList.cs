using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;

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
        /// <param name="halfEdge">Vertex to add.</param>
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
        /// <param name="vertex">Location of new vertex.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(Point3d vertex)
        {
            return this.Add(vertex.X, vertex.Y, vertex.Z);
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Location of new vertex.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(Point3f vertex)
        {
            return this.Add(vertex.X, vertex.Y, vertex.Z);
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
        
        /// <summary>
        /// Adds a series of new vertices to the end of the vertex list.
        /// This overload accepts double-precision points.
        /// </summary>
        /// <param name="vertices">A list, an array or any enumerable set of Point3d.</param>
        public void AddVertices(IEnumerable<Point3d> vertices)
        {
            foreach (Point3d v in vertices)
            {
                this.Add(v);
            }
        }
        #endregion
        
        /// <summary>
        /// Adds a series of new vertices to the end of the vertex list.
        /// This overload accepts single-precision points.
        /// </summary>
        /// <param name="vertices">A list, an array or any enumerable set of Point3f.</param>
        public void AddVertices(IEnumerable<Point3f> vertices)
        {
            foreach (Point3f v in vertices)
            {
                this.Add(v);
            }
        }
        
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
        /// Gets the first <b>incoming</b> halfedge for a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The index of the halfedge paired with the specified vertex's .</returns>
        public int GetIncomingHalfedge(int v)
        {
            return _mesh.Halfedges.PairHalfedge(this[v].OutgoingHalfedge);
        }
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