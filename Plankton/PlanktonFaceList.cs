using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Rhino.Geometry;

namespace Plankton
{
    /// <summary>
    /// Provides access to the faces and Face related functionality of a Mesh.
    /// </summary>
    public class PlanktonFaceList : IEnumerable<PlanktonFace>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonFace> _list;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonHalfEdgeList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="ownerMesh">The mesh to which this list of half-edges belongs.</param>
        internal PlanktonFaceList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonFace>();
            this._mesh = owner;
        }
        
        /// <summary>
        /// Gets the number of half-edges.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }
        
        #region methods
        #region face access
        /// <summary>
        /// Adds a new face to the end of the Face list.
        /// </summary>
        /// <param name="halfEdge">Face to add.</param>
        /// <returns>The index of the newly added face.</returns>
        internal int Add(PlanktonFace face)
        {
            if (face == null) return -1;
            this._list.Add(face);
            return this.Count - 1;
        }
        
        /// <summary>
        /// Adds a new face to the end of the Face list. Creates any halfedge pairs that are required.
        /// </summary>
        /// <param name="indices">The vertex indices which define this face, ordered anticlockwise.</param>
        /// <returns>The index of the newly added face.</returns>
        public int AddFace(IEnumerable<int> indices)
        {
            // This method always ensures that if a vertex lies on a boundary,
            // vertex -> outgoingHalfedge -> adjacentFace == -1
            
            int[] array = indices.ToArray(); // using Linq for convenience
            
            int n = array.Length;
            
            // Don't allow degenerate faces
            if (n < 3) return -1;
            
            // Check vertices
            foreach (int i in array)
            {
                // Check that all vertex indices exist in this mesh
                if (i < 0 || i >= _mesh.Vertices.Count)
                    throw new IndexOutOfRangeException("No vertex exists at this index.");
                // Check that all vertices are on a boundary
                int outgoing = _mesh.Vertices[i].OutgoingHalfedge;
                if (outgoing != -1 && _mesh.Halfedges[outgoing].AdjacentFace != -1)
                    return -1;
            }
            
            // For each pair of vertices, check for an existing halfedge
            // If it exists, check that it doesn't already have a face
            // If it doesn't exist, mark for creation of a new halfedge pair
            int[] loop = new int[n];
            bool[] is_new = new bool[n];
            List<int> newHalfedges = new List<int>();
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                int v1 = array[i], v2 = array[ii];
                is_new[i] = true;
                // Find existing edge, if it exists, by searching 'i'th vertex's neighbourhood
                if (_mesh.Vertices[v2].OutgoingHalfedge > -1)
                {
                    List<int> hs = _mesh.VertexAllOutHE(v2);
                    foreach (int h in hs)
                    {
                        if (v1 == _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(h)].StartVertex)
                        {
                            // Don't allow non-manifold edges
                            if (_mesh.Halfedges[_mesh.Halfedges.PairHalfedge(h)].AdjacentFace > -1) return -1;
                            loop[i] = _mesh.Halfedges.PairHalfedge(h);
                            is_new[i] = false;
                            break;
                        }
                    }
                }
            }
            
            // Now create any missing halfedge pairs...
            // (This could be done in the loop above but it avoids having to tidy up
            // any recently added halfedges should a non-manifold condition be found.)
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                if (is_new[i]) // new halfedge pair requireds
                {
                    int v1 = array[i], v2 = array[ii];
                    loop[i] = _mesh.Halfedges.Count;
                    is_new[i] = true;
                    // he->next = he->pair
                    _mesh.Halfedges.Add(new PlanktonHalfedge(v1, this.Count, loop[i] + 1));
                    _mesh.Halfedges.Add(new PlanktonHalfedge(v2, -1, loop[i]));
                    _mesh.Vertices[v2].OutgoingHalfedge = loop[i] + 1;
                }
                else
                {
                    // Link existing halfedge to new face
                    _mesh.Halfedges[loop[i]].AdjacentFace = this.Count;
                }
            }
            
            // Link halfedges
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                int v1 = array[i], v2 = array[ii];
                int id = 0;
                if (is_new[i])  id += 1;
                if (is_new[ii]) id += 2;
                
                if (id > 0) // At least one of the halfedge pairs is new...
                {
                    // Link inner halfedges
                    _mesh.Halfedges[loop[i]].NextHalfedge = loop[ii];
                    _mesh.Halfedges[loop[ii]].PrevHalfedge = loop[i];
                    
                    // Link outer halfedges
                    int firstHalfedge, currentHalfedge;
                    switch (id)
                    {
                        case 1: // first is new, second is old
                            // iterate through halfedges clockwise around vertex #v2 until boundary
                            firstHalfedge = loop[ii];
                            currentHalfedge = firstHalfedge;
                            do
                            {
                                int pair = _mesh.Halfedges.PairHalfedge(currentHalfedge);
                                currentHalfedge = _mesh.Halfedges[pair].NextHalfedge;
                            } while (_mesh.Halfedges[_mesh.Halfedges.PairHalfedge(currentHalfedge)].AdjacentFace > -1);
                            _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(currentHalfedge)].NextHalfedge = _mesh.Halfedges.PairHalfedge(loop[i]);
                            _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(loop[i])].PrevHalfedge = _mesh.Halfedges.PairHalfedge(currentHalfedge);
                            break;
                        case 2: // second is new, first is old
                            int outer_next = _mesh.Vertices[v2].OutgoingHalfedge;
                            _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(loop[ii])].NextHalfedge = outer_next;
                            _mesh.Halfedges[outer_next].PrevHalfedge = _mesh.Halfedges.PairHalfedge(loop[ii]);
                            break;
                        case 3: // both are new
                            _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(loop[ii])].NextHalfedge = _mesh.Halfedges.PairHalfedge(loop[i]);
                            _mesh.Halfedges[_mesh.Halfedges.PairHalfedge(loop[i])].PrevHalfedge = _mesh.Halfedges.PairHalfedge(loop[ii]);
                            break;
                    }
                }
            }
            
            PlanktonFace f = new PlanktonFace();
            f.FirstHalfedge = loop[0];
            
            return this.Add(f);
        }
        
        /// <summary>
        /// Returns the face at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of face to get.
        /// Must be larger than or equal to zero and smaller than the Face Count of the mesh.
        /// </param>
        /// <returns>The face at the given index.</returns>
        public PlanktonFace this[int index]
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
        /// Gets the halfedges which bound a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The indices of halfedges incident to a particular face.
        /// Ordered anticlockwise around the face.</returns>
        public int[] GetHalfedges(int f)
        {
            return this.GetHalfedgesCirculator(f).ToArray();
        }
        
        /// <summary>
        /// Traverses the halfedge indices which bound a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified face.
        /// Ordered anticlockwise around the face.</returns>
        public IEnumerable<int> GetHalfedgesCirculator(int f)
        {
            int he_first = this[f].FirstHalfedge;
            int he_current = he_first;
            do
            {
                yield return he_current;
                he_current = _mesh.Halfedges[he_current].NextHalfedge;
            }
            while (he_current != he_first);
        }
        #endregion
        
        /// <summary>
        /// Gets vertex indices of a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>An array of vertex indices incident to the specified face.
        /// Ordered anticlockwise around the face.</returns>
        public int[] GetVertices(int f)
        {
            return this.GetHalfedgesCirculator(f)
                .Select(h => _mesh.Halfedges[h].StartVertex).ToArray();
        }
        
        /// <summary>
        /// Gets the barycenter of a face's vertices.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The location of the specified face's barycenter.</returns>
        public Point3d FaceCentroid(int f)
        {
            int[] fvs = this.GetVertices(f);
            Point3d Centroid = new Point3d(0, 0, 0);
            foreach (int i in fvs)
            {
                Centroid += _mesh.Vertices[i].Position;
            }
            Centroid *= 1.0 / fvs.Length;
            return Centroid;
        }
        
        /// <summary>
        /// Gets the number of naked edges which bound this face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The number of halfedges for which the opposite halfedge has no face (i.e. adjacent face index is -1).</returns>
        public int NakedEdgeCount(int f)
        {
            int nakedCount = 0;
            foreach (int i in this.GetHalfedgesCirculator(f))
            {
                if (_mesh.Halfedges[_mesh.Halfedges.PairHalfedge(i)].AdjacentFace == -1) nakedCount++;
            }
            return nakedCount;
        }
        #endregion
        
        #region IEnumerable implementation
        /// <summary>
        /// Gets an enumerator that yields all faces in this collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<PlanktonFace> GetEnumerator()
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
