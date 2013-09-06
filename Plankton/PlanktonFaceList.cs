using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//using Rhino.Geometry;

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
        /// <param name="owner">The mesh to which this list of half-edges belongs.</param>
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
                //int v1 = array[i];
                int v2 = array[ii];
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
                            outer_next = hs.GetPairHalfedge(loop[i]);
                            break;
                        case 2: // second is new, first is old
                            outer_prev = hs.GetPairHalfedge(loop[ii]);
                            outer_next = hs[loop[i]].NextHalfedge;
                            break;
                        case 3: // both are new
                            outer_prev = hs.GetPairHalfedge(loop[ii]);
                            outer_next = hs.GetPairHalfedge(loop[i]);
                            break;
                        case 4: // both are new (non-manifold vertex)
                            // We have TWO boundaries to take care of here: first...
                            outer_prev = hs[vs[v2].OutgoingHalfedge].PrevHalfedge;
                            outer_next = hs.GetPairHalfedge(loop[i]);
                            hs[outer_prev].NextHalfedge = outer_next;
                            hs[outer_next].PrevHalfedge = outer_prev;
                            // and second...
                            outer_prev = hs.GetPairHalfedge(loop[ii]);
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
                    {
                        vs[v2].OutgoingHalfedge = loop[i] + 1;
                    }
                }
                else // both old (non-manifold vertex trickery below)
                {
                    // In the case that v2 links to the current second halfedge, creating a
                    // face here will redefine v2 as a non-boundary vertex. Do a quick lap of
                    // v2's other outgoing halfedges in case one of them is still a boundary
                    // (as will be the case if v2 was non-manifold).
                    if (vs[v2].OutgoingHalfedge == loop[ii])
                    {
                        foreach (int h in hs.GetVertexCirculator(loop[ii]).Skip(1))
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
                            int boundary = hs.GetVertexCirculator(loop[ii]).Skip(1)
                                .First(h => hs[h].AdjacentFace < 0);
                            hs.MakeConsecutive(loop[i], loop[ii]);
                            hs.MakeConsecutive(hs[boundary].PrevHalfedge, next);
                            hs.MakeConsecutive(prev, boundary);
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
            PlanktonFace f = new PlanktonFace() { FirstHalfedge = loop[0] };
            
            return this.Add(f);
        }

        /// <summary>
        /// Appends a new triangular face to the end of the mesh face list. Creates any halfedge pairs that are required.
        /// </summary>
        /// <returns>The index of the newly added face (-1 in the case that the face could not be added).</returns>
        /// <param name="a">Index of first corner.</param>
        /// <param name="b">Index of second corner.</param>
        /// <param name="c">Index of third corner.</param>
        /// <remarks>The mesh must remain 2-manifold and orientable at all times.</remarks>
        public int AddFace(int a, int b, int c)
        {
            return this.AddFace(new int[] { a, b, c });
        }

        /// <summary>
        /// Appends a new quadragular face to the end of the mesh face list. Creates any halfedge pairs that are required.
        /// </summary>
        /// <returns>The index of the newly added face (-1 in the case that the face could not be added).</returns>
        /// <param name="a">Index of first corner.</param>
        /// <param name="b">Index of second corner.</param>
        /// <param name="c">Index of third corner.</param>
        /// <param name="d">Index of fourth corner.</param> 
        /// <remarks>The mesh must remain 2-manifold and orientable at all times.</remarks>
        public int AddFace(int a, int b, int c, int d)
        {
            return this.AddFace(new int[] { a, b, c, d });
        }

        /// <summary>
        /// <para>Removes a face from the mesh without affecting the remaining geometry.</para>
        /// <para>Ensures that the topology of the halfedge mesh remains fully intact.</para>
        /// </summary>
        /// <param name="index">The index of the face to be removed.</param>
        public void RemoveFace(int index)
        {
            int[] fhs = this.GetHalfedges(index);
            foreach (int h in fhs)
            {
                if (_mesh.Halfedges.IsBoundary(h)) { _mesh.Halfedges.RemovePairHelper(h); }
                else { _mesh.Halfedges[h].AdjacentFace = -1; }
            }
            this[index] = PlanktonFace.Unset;
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
            internal set
            {
                this._list[index] = value;
            }
        }
        #endregion

        /// <summary>
        /// Helper method to remove dead faces from the list, re-index and compact.
        /// </summary>
        internal void CompactHelper()
        {
            int marker = 0; // Location where the current face should be moved to

            // Run through all the faces
            for (int iter = 0; iter < _list.Count; iter++)
            {
                // If face is alive, check if we need to shuffle it down the list
                if (!_list[iter].IsUnused)
                {
                    if (marker < iter)
                    {
                        // Room to shuffle. Copy current face to marked slot.
                        _list[marker] = _list[iter];

                        // Update all halfedges which are adjacent
                        int first = _list[marker].FirstHalfedge;
                        foreach (int h in _mesh.Halfedges.GetFaceCirculator(first))
                        {
                            _mesh.Halfedges[h].AdjacentFace = marker;
                        }
                    }
                    marker++; // That spot's filled. Advance the marker.
                }
            }

            // Trim list down to new size
            if (marker < _list.Count) { _list.RemoveRange(marker, _list.Count - marker); }
        }
        
        #region traversals
        /// <summary>
        /// Traverses the halfedge indices which bound a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified face.
        /// Ordered anticlockwise around the face.</returns>
        [Obsolete("GetHalfedgesCirculator(int) is deprecated, please use" +
            "Halfedges.GetFaceCirculator(int) instead.")]
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

        #region adjacency queries
        /// <summary>
        /// Gets the halfedges which bound a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The indices of halfedges incident to a particular face.
        /// Ordered anticlockwise around the face.</returns>
        public int[] GetHalfedges(int f)
        {
            return _mesh.Halfedges.GetFaceCirculator(this[f].FirstHalfedge).ToArray();
        }

        /// <summary>
        /// Gets vertex indices of a face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>An array of vertex indices incident to the specified face.
        /// Ordered anticlockwise around the face.</returns>
        public int[] GetFaceVertices(int f)
        {
            return _mesh.Halfedges.GetFaceCirculator(this[f].FirstHalfedge)
                .Select(h => _mesh.Halfedges[h].StartVertex).ToArray();
        }

        [Obsolete("GetVertices is deprecated, please use GetFaceVertices instead.")]
        public int[] GetVertices(int f)
        {
            return this.GetFaceVertices(f);
        }
        #endregion

        #region Euler operators
        /// <summary>
        /// <para>Split a face into two faces by inserting a new edge</para>
        /// <seealso cref="MergeFaces"/>
        /// </summary>
        /// <param name="to">The index of a second halfedge adjacent to the face to split.
        /// The new edge will end at the start of this halfedge.</param>
        /// <param name="from">The index of a halfedge adjacent to the face to split.
        /// The new edge will begin at the start of this halfedge.</param>
        /// <returns>The index of one of the newly created halfedges, or -1 on failure.
        /// The returned halfedge will be adjacent to the pre-existing face.</returns>
        public int SplitFace(int to, int from)
        {
            // split the adjacent face in 2
            // by creating a new edge from the start of the given halfedge
            // to another vertex around the face

            var hs = _mesh.Halfedges;

            // check preconditions
            int existing_face = hs[from].AdjacentFace;
            if (existing_face == -1 || existing_face != hs[to].AdjacentFace) { return -1; }
            if (from == to || hs[from].NextHalfedge == to || hs[to].NextHalfedge == from) { return -1; }

            // add the new halfedge pair
            int new_halfedge1 = hs.AddPair(hs[from].StartVertex, hs[to].StartVertex, existing_face);
            int new_halfedge2 = hs.GetPairHalfedge(new_halfedge1);

            // add a new face
            //PlanktonFace new_face = new PlanktonFace();
            int new_face_index = this.Add(PlanktonFace.Unset);

            //link everything up

            //prev of input he becomes prev of new_he1
            hs.MakeConsecutive(hs[from].PrevHalfedge, new_halfedge1);

            //prev of he_around becomes prev of new_he2
            hs.MakeConsecutive(hs[to].PrevHalfedge, new_halfedge2);
            
            //next of new_he1 becomes he_around
            hs.MakeConsecutive(new_halfedge1, to);

            //next of new_he2 becomes index
            hs.MakeConsecutive(new_halfedge2, from);

            //set the original face's first halfedge to new_he1
            this[existing_face].FirstHalfedge = new_halfedge1;
            //set the new face's first halfedge to new_he2
            this[new_face_index].FirstHalfedge = new_halfedge2;
            
            //set adjface of new face loop
            foreach (int h in _mesh.Halfedges.GetFaceCirculator(new_halfedge2))
            {
                hs[h].AdjacentFace = new_face_index;
            }

            //think thats all of it!           

            return new_halfedge1;
        }

        /// <summary>
        /// <para>Merges the two faces incident to the specified halfedge pair.</para>
        /// <seealso cref="SplitFace"/>
        /// </summary>
        /// <param name="index">The index of a halfedge inbetween the two faces to merge.
        /// The face adjacent to this halfedge will be retained.</param>        
        /// <returns>The successor of <paramref name="index"/> around the face, or -1 on failure.</returns>
        /// <remarks>
        /// The invariant <c>mesh.Faces.MergeFaces(mesh.Faces.SplitFace(a, b))</c> will return a,
        /// leaving the mesh unchanged.</remarks>
        public int MergeFaces(int index)
        {
            var hs = _mesh.Halfedges;
            int pair = hs.GetPairHalfedge(index);
            int face = hs[index].AdjacentFace;
            int pair_face = hs[pair].AdjacentFace;

            // Check for a face on both sides
            if (face == -1 || pair_face == -1) { return -1; }

            // Both vertices incident to given halfedge must have valence > 2
            if (3 > _mesh.Vertices.GetHalfedges(hs[index].StartVertex).Length) { return -1; }
            if (3 > _mesh.Vertices.GetHalfedges(hs[pair].StartVertex).Length) { return -1; }

            // Make combined face halfedges consecutive
            int index_prev = hs[index].PrevHalfedge;
            int index_next = hs[index].NextHalfedge;

            // Remove halfedges (handles re-linking at ends and re-assigning vertices' outgoing hes)
            hs.RemovePairHelper(index);

            // Update retained face's first halfedge, if necessary
            if (this[face].FirstHalfedge == index)
                this[face].FirstHalfedge = index_next;

            // Go around the dead face, reassigning adjacency
            foreach (int h in hs.GetFaceCirculator(index_next))
            {
                hs[h].AdjacentFace = face;
            }

            // Keep the adjacent face, but remove the pair's adjacent face
            this[pair_face] = PlanktonFace.Unset;

            return index_next;
        }

        /// <summary>
        /// Divides an n-sided face into n triangles, adding a new vertex in the center of the face.
        /// </summary>
        /// <param name="index">The index of the face to stellate</param>        
        /// <returns>The index of the central vertex</returns>
        public int Stellate(int f)
        {
            int central_vertex = _mesh.Vertices.Add(this.GetFaceCenter(f));
            int CountBefore = _mesh.Halfedges.Count();
            int[] FaceHalfEdges = this.GetHalfedges(f);
            for (int i = 0; i < FaceHalfEdges.Length; i++)
            {    
                int ThisHalfEdge = FaceHalfEdges[i];
                int TriangleFace;
                if (i == 0) {TriangleFace = f;}
                else {TriangleFace = this.Add(PlanktonFace.Unset);}                
                this[TriangleFace].FirstHalfedge = ThisHalfEdge;
                _mesh.Halfedges[ThisHalfEdge].AdjacentFace = TriangleFace;
                int OutSpoke = _mesh.Halfedges.AddPair(central_vertex, _mesh.Halfedges[ThisHalfEdge].StartVertex, TriangleFace);
                if (i == 0) { _mesh.Vertices[central_vertex].OutgoingHalfedge = OutSpoke; }
                _mesh.Halfedges.MakeConsecutive(OutSpoke,ThisHalfEdge);
            }
            for (int i = 0; i < FaceHalfEdges.Length; i++)
            {
                int ThisHalfEdge = FaceHalfEdges[i];
                if(i<FaceHalfEdges.Length-1)
                {
                    //link the edge to the ingoing spoke, and the ingoing spoke to the outgoing one
                    _mesh.Halfedges.MakeConsecutive(ThisHalfEdge, CountBefore + i*2 + 3);
                    _mesh.Halfedges.MakeConsecutive(CountBefore + (i*2) + 3, CountBefore + (i*2));
                    //set the AdjacentFace of the ingoing spoke                
                    _mesh.Halfedges[CountBefore + (i * 2) + 3].AdjacentFace = _mesh.Halfedges[ThisHalfEdge].AdjacentFace;
                }
                else
                {
                    _mesh.Halfedges.MakeConsecutive(ThisHalfEdge, CountBefore + 1);
                    _mesh.Halfedges.MakeConsecutive(CountBefore + 1, CountBefore + (i*2));
                    _mesh.Halfedges[CountBefore + 1].AdjacentFace = _mesh.Halfedges[ThisHalfEdge].AdjacentFace;
                }
            }            
            return central_vertex;
        }
        #endregion

        /// <summary>
        /// Gets the barycenter of a face's vertices.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The location of the specified face's barycenter.</returns>
        public PlanktonXYZ GetFaceCenter(int f)
        {
            PlanktonXYZ centroid = PlanktonXYZ.Zero;
            int count = 0;
            foreach (int i in this.GetFaceVertices(f))
            {
                centroid += _mesh.Vertices[i].ToXYZ();
                count++;
            }
            centroid *= 1f / count;
            return centroid;
        }
        
        [Obsolete("FaceCentroid is deprecated, please use GetFaceCenter instead.")]
        public PlanktonXYZ FaceCentroid(int f)
        {
            return this.GetFaceCenter(f);
        }
        
        /// <summary>
        /// Gets the number of naked edges which bound this face.
        /// </summary>
        /// <param name="f">A face index.</param>
        /// <returns>The number of halfedges for which the opposite halfedge has no face (i.e. adjacent face index is -1).</returns>
        public int NakedEdgeCount(int f)
        {
            int nakedCount = 0;
            foreach (int i in _mesh.Halfedges.GetFaceCirculator(this[f].FirstHalfedge))
            {
                if (_mesh.Halfedges[_mesh.Halfedges.GetPairHalfedge(i)].AdjacentFace == -1) nakedCount++;
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
