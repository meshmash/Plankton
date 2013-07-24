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
        /// Initializes a new instance of the <see cref="PlanktonFaceList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="ownerMesh">The mesh to which this list of half-edges belongs.</param>
        internal PlanktonFaceList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonFace>();
            this._mesh = owner;
        }
        
        /// <summary>
        /// Gets the number of faces.
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
        /// <param name="indices">The vertex indices which define the face, ordered anticlockwise.</param>
        /// <returns>The index of the newly added face (-1 in the case that the face could not be added).</returns>
        /// <remarks>The mesh must remain 2-manifold and orientable at all times.</remarks>
        public int AddFace(IEnumerable<int> indices)
        {
            // This method always ensures that if a vertex lies on a boundary,
            // vertex -> outgoingHalfedge -> adjacentFace == -1
            
            int[] array = indices.ToArray(); // using Linq for convenience
            
            var hs = _mesh.Halfedges;
            var vs = _mesh.Vertices;
            int n = array.Length;
            
            // Don't allow degenerate faces
            if (n < 3) return -1;
            
            // Check vertices
            foreach (int i in array)
            {
                // Check that all vertex indices exist in this mesh
                if (i < 0 || i >= vs.Count)
                    throw new IndexOutOfRangeException("No vertex exists at this index.");
                // Check that all vertices are on a boundary
                int outgoing = vs[i].OutgoingHalfedge;
                if (outgoing != -1 && hs[outgoing].AdjacentFace != -1)
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

                // Find existing edge, if it exists
                int h = hs.FindHalfedge(v1, v2);
                if (h < 0)
                    // No halfedge found, mark for creation
                    is_new[i] = true;
                else if (hs[h].AdjacentFace > -1)
                    // Existing halfedge already has a face (non-manifold)
                    return -1;
                else
                    loop[i] = h;
            }
            
            // Now create any missing halfedge pairs...
            // (This could be done in the loop above but it avoids having to tidy up
            // any recently added halfedges should a non-manifold condition be found.)
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                if (is_new[i]) // new halfedge pair required
                {
                    int v1 = array[i], v2 = array[ii];
                    loop[i] = hs.AddPair(v1, v2, this.Count);
                    // ensure vertex->outgoing is boundary if vertex is boundary
                    vs[v2].OutgoingHalfedge = loop[i] + 1;
                }
                else
                {
                    // Link existing halfedge to new face
                    hs[loop[i]].AdjacentFace = this.Count;
                }
            }
            
            // Link halfedges
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                // TODO: consider case of non-manifold vertex
                // (i.e. vertex with 2+ outgoing boundary halfedges)
                
                int v1 = array[i], v2 = array[ii];
                int id = 0;
                if (is_new[i])  id += 1; // first is new
                if (is_new[ii]) id += 2; // second is new
                
                if (id > 0) // At least one of the halfedge pairs is new...
                {
                    // Link inner halfedges
                    hs[loop[i]].NextHalfedge = loop[ii];
                    hs[loop[ii]].PrevHalfedge = loop[i];
                    
                    // Link outer halfedges
                    int outer_prev = -1, outer_next = -1;
                    switch (id)
                    {
                        case 1: // first is new, second is old
                            // iterate through halfedges clockwise around vertex #v2 until boundary
                            outer_prev = hs.PairHalfedge(vs.GetHalfedgesCirculator(v2)
                                                         .First(h => hs[hs.PairHalfedge(h)].AdjacentFace < 0));
                            outer_next = hs.PairHalfedge(loop[i]);
                            break;
                        case 2: // second is new, first is old
                            outer_prev = hs.PairHalfedge(loop[ii]);
                            outer_next = vs[v2].OutgoingHalfedge;
                            break;
                        case 3: // both are new
                            outer_prev = hs.PairHalfedge(loop[ii]);
                            outer_next = hs.PairHalfedge(loop[i]);
                            break;
                    }
                    // outer_{prev,next} should now be set, so store links in HDS
                    if (outer_prev > -1 && outer_next > -1)
                    {
                        hs[outer_prev].NextHalfedge = outer_next;
                        hs[outer_next].PrevHalfedge = outer_prev;
                    }
                }
            }
            
            // Finally, add the face and return its index
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
            if (he_first < 0) yield break; // face has no connectivity, exit
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
