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
                // NOTE: To PREVENT non-manifold vertices, uncomment the line below...
                //if(is_new[i] && is_new[(i+n-1)%n] && vs[v1].OutgoingHalfedge > -1) return -1;
            }
            
            // Now create any missing halfedge pairs...
            // (This could be done in the loop above but it avoids having to tidy up
            // any recently added halfedges should a non-manifold edge be found.)
            for (int i = 0, ii = 1; i < n; i++, ii++, ii %= n)
            {
                if (is_new[i]) // new halfedge pair required
                {
                    int v1 = array[i], v2 = array[ii];
                    loop[i] = hs.AddPair(v1, v2, this.Count);
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
                int v1 = array[i], v2 = array[ii];
                int id = 0;
                if (is_new[i])  id += 1; // first is new
                if (is_new[ii]) id += 2; // second is new
                
                // Check for non-manifold vertex case, i.e. both current halfedges are new
                // but the vertex between them is already part of another face. This vertex
                // will have TWO OR MORE outgoing boundary halfedges! (Not strictly allowed,
                // but it could happen if faces are added in an UGLY order.)
                // TODO: If a mesh has non-manifold vertices perhaps it should be considered
                // INVALID. Any operations performed on such a mesh cannot be relied upon to
                // perform correctly as the adjacency information may not be correct.
                // (More reading: http://www.pointclouds.org/blog/nvcs/)
                if (id == 3 && vs[v2].OutgoingHalfedge > -1) id++; // id == 4
                
                if (id > 0) // At least one of the halfedge pairs is new...
                {
                    // Link outer halfedges
                    int outer_prev = -1, outer_next = -1;
                    switch (id)
                    {
                        case 1: // first is new, second is old
                            // iterate through halfedges clockwise around vertex #v2 until boundary
                            outer_prev = hs[loop[ii]].PrevHalfedge;
                            outer_next = hs.PairHalfedge(loop[i]);
                            break;
                        case 2: // second is new, first is old
                            outer_prev = hs.PairHalfedge(loop[ii]);
                            outer_next = hs[loop[i]].NextHalfedge;
                            break;
                        case 3: // both are new
                            outer_prev = hs.PairHalfedge(loop[ii]);
                            outer_next = hs.PairHalfedge(loop[i]);
                            break;
                        case 4: // both are new (non-manifold vertex)
                            // We have TWO boundaries to take care of here: first...
                            outer_prev = hs[vs[v2].OutgoingHalfedge].PrevHalfedge;
                            outer_next = hs.PairHalfedge(loop[i]);
                            hs[outer_prev].NextHalfedge = outer_next;
                            hs[outer_next].PrevHalfedge = outer_prev;
                            // and second...
                            outer_prev = hs.PairHalfedge(loop[ii]);
                            outer_next = vs[v2].OutgoingHalfedge;
                            break;
                    }
                    // outer_{prev,next} should now be set, so store links in HDS
                    if (outer_prev > -1 && outer_next > -1)
                    {
                        hs[outer_prev].NextHalfedge = outer_next;
                        hs[outer_next].PrevHalfedge = outer_prev;
                    }
                    
                    // Link inner halfedges
                    hs[loop[i]].NextHalfedge = loop[ii];
                    hs[loop[ii]].PrevHalfedge = loop[i];
                    
                    // ensure vertex->outgoing is boundary if vertex is boundary
                    if (is_new[i]) // first is new
                        vs[v2].OutgoingHalfedge = loop[i] + 1;
                }
                else // both old (non-manifold vertex trickery below)
                {
                    // In the case that v2 links to the current second halfedge, creating a
                    // face here will redefine v2 as a non-boundary vertex. Do a quick lap of
                    // v2's other outgoing halfedges in case one of them is still a boundary
                    // (as will be the case if v2 was non-manifold).
                    if (vs[v2].OutgoingHalfedge == loop[ii])
                    {
                        foreach (int h in vs.GetHalfedgesCirculator(v2).Skip(1))
                        {
                            if (hs[h].AdjacentFace < 0)
                            {
                                vs[v2].OutgoingHalfedge = h;
                                break;
                            }
                        }
                    }
                    // If inner loop exists, but for some reason it's not already linked
                    // (non-manifold vertex) make loop[i] adjacent to loop[ii]. Tidy up other
                    // halfedge links such that all outgoing halfedges remain visible to v2.
                    if (hs[loop[i]].NextHalfedge != loop[ii] || hs[loop[ii]].PrevHalfedge != loop[i])
                    {
                        int next = hs[loop[i]].NextHalfedge;
                        int prev = hs[loop[ii]].PrevHalfedge;
                        // Find another boundary at this vertex to link 'next' and 'prev' into.
                        try
                        {
                            int boundary = vs.GetHalfedgesCirculator(v2, loop[ii]).Skip(1)
                                .First(h => hs[h].AdjacentFace < 0);
                            hs.MakeAdjacent(loop[i], loop[ii]);
                            hs.MakeAdjacent(hs[boundary].PrevHalfedge, next);
                            hs.MakeAdjacent(prev, boundary);
                        }
                        // If no other boundary is found, something must be wrong...
                        catch (InvalidOperationException)
                        {
                            throw new InvalidOperationException(string.Format(
                                "Failed to relink halfedges around vertex #{0} during creation of face #{1}", v2, this.Count));
                        }
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
