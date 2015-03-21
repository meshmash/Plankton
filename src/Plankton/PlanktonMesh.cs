//using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plankton
{
    /// <summary>
    /// Description of PlanktonMesh.
    /// </summary>
    public class PlanktonMesh
    {
        private PlanktonVertexList _vertices;
        private PlanktonHalfEdgeList _halfedges;
        private PlanktonFaceList _faces;
        
        #region "constructors"
        public PlanktonMesh() //blank constructor
        {
        }
        
        public PlanktonMesh(PlanktonMesh source)
        {
            foreach (var v in source.Vertices)
            {
                this.Vertices.Add(new PlanktonVertex() {
                                      OutgoingHalfedge = v.OutgoingHalfedge,
                                      X = v.X,
                                      Y = v.Y,
                                      Z = v.Z
                                  });
            }
            foreach (var f in source.Faces)
            {
                this.Faces.Add(new PlanktonFace() { FirstHalfedge = f.FirstHalfedge });
            }
            foreach (var h in source.Halfedges)
            {
                this.Halfedges.Add(new PlanktonHalfedge() {
                                       StartVertex = h.StartVertex,
                                       AdjacentFace = h.AdjacentFace,
                                       NextHalfedge = h.NextHalfedge,
                                       PrevHalfedge = h.PrevHalfedge,
                                   });
            }
        }
        #endregion

        #region "properties"
        /// <summary>
        /// Gets access to the vertices collection in this mesh.
        /// </summary>
        public PlanktonVertexList Vertices
        {
            get { return _vertices ?? (_vertices = new PlanktonVertexList(this)); }
        }
        
        /// <summary>
        /// Gets access to the halfedges collection in this mesh.
        /// </summary>
        public PlanktonHalfEdgeList Halfedges
        {
            get { return _halfedges ?? (_halfedges = new PlanktonHalfEdgeList(this)); }
        }
        
        /// <summary>
        /// Gets access to the faces collection in this mesh.
        /// </summary>
        public PlanktonFaceList Faces
        {
            get { return _faces ?? (_faces = new PlanktonFaceList(this)); }
        }
        #endregion

        #region "general methods"

        /// <summary>
        /// Calculate the volume of the mesh
        /// </summary>
        public double Volume()
        {
            double VolumeSum = 0;
            for (int i = 0; i < this.Faces.Count; i++)
            {
                int[] FaceVerts = this.Faces.GetFaceVertices(i);
                int EdgeCount = FaceVerts.Length;
                if (EdgeCount == 3)
                {
                    PlanktonXYZ P = this.Vertices[FaceVerts[0]].ToXYZ();
                    PlanktonXYZ Q = this.Vertices[FaceVerts[1]].ToXYZ();
                    PlanktonXYZ R = this.Vertices[FaceVerts[2]].ToXYZ();
                    //get the signed volume of the tetrahedron formed by the triangle and the origin
                    VolumeSum += (1 / 6d) * (
                           P.X * Q.Y * R.Z +
                           P.Y * Q.Z * R.X +
                           P.Z * Q.X * R.Y -
                           P.X * Q.Z * R.Y -
                           P.Y * Q.X * R.Z -
                           P.Z * Q.Y * R.X);
                }
                else
                {
                    PlanktonXYZ P = this._faces.GetFaceCenter(i);
                    for (int j = 0; j < EdgeCount; j++)
                    {
                        PlanktonXYZ Q = this.Vertices[FaceVerts[j]].ToXYZ();
                        PlanktonXYZ R = this.Vertices[FaceVerts[(j + 1) % EdgeCount]].ToXYZ();
                        VolumeSum += (1 / 6d) * (
                            P.X * Q.Y * R.Z + 
                            P.Y * Q.Z * R.X + 
                            P.Z * Q.X * R.Y - 
                            P.X * Q.Z * R.Y - 
                            P.Y * Q.X * R.Z - 
                            P.Z * Q.Y * R.X);
                    }
                }
            }            
            return VolumeSum;
        }

        public PlanktonMesh PlanktonDual()
        {
            
            // now it handles open meshes by including boundaries, and works cleanly with closed meshes
            // should add functionality to remove boundaries in open meshes if desired
            // can later add options for other ways of defining face centres (barycenter/circumcenter etc)

            PlanktonMesh P = this;
            PlanktonMesh D = new PlanktonMesh();

            //for every primal face, add the barycenter to the dual's vertex list

            for (int i = 0; i < P.Faces.Count; i++)
            {
                var fc = P.Faces.GetFaceCenter(i);
                D.Vertices.Add(new PlanktonVertex(fc.X, fc.Y, fc.Z));

                int[] FaceHalfedges = P.Faces.GetHalfedges(i);
                D.Vertices[D.Vertices.Count - 1].OutgoingHalfedge = P.Halfedges.GetPairHalfedge(FaceHalfedges[0]);
            }

            //dual vertex outgoing HE is primal face's start HE
            //for every vertex of the primal, add a face to the dual
            //dual face's startHE is primal vertex's outgoing's pair

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                //add all vertices as faces
                int df = D.Faces.Add(PlanktonFace.Unset);
                D.Faces[df].FirstHalfedge = P.Vertices[i].OutgoingHalfedge;
            }

            //dual HalfEdge Start Vertex is primal Pair's AdjacentFace
            //dual HalfEdge AdjacentFace is primal Start Vertex
            //dual NextHE is primal's Pair's Prev
            //dual PrevHE is primal's Next's Pair

            //halfedge pairs stay the same

            //List of halfedges that define the ends of faces that
            //need to be closed with additional Halfedge Pairs
            List<int> FinishEdges = new List<int>();
            //Lookup list for removing naked faces if desired
            List<int> NakedFaces = new List<int>();

            //List of new boundary Halfedges to be appended to the end of the Halfedge List
            List<PlanktonHalfedge> NewBoundaries = new List<PlanktonHalfedge>();
            //Index counter for newly appended Halfedges
            int NewHECounter = P.Halfedges.Count;

            for (int i = 0; i < P.Halfedges.Count; i++)
            {

                int PairIndex = P.Halfedges.GetPairHalfedge(i);

                //These are internal and clean HalfEdges
                PlanktonHalfedge DualHE = PlanktonHalfedge.Unset;
                PlanktonHalfedge PrimalHE = P.Halfedges[i];

                DualHE.StartVertex = P.Halfedges[PairIndex].AdjacentFace;
                DualHE.AdjacentFace = PrimalHE.StartVertex;

                DualHE.PrevHalfedge = P.Halfedges[PairIndex].NextHalfedge;

                //The next Halfedges for primal naked edges when flipped
                //to the dual have to be created later
                if (PrimalHE.AdjacentFace < 0) { DualHE.NextHalfedge = -1; }
                else { DualHE.NextHalfedge = P.Halfedges.GetPairHalfedge(PrimalHE.PrevHalfedge); }

                D.Halfedges.Add(DualHE);

                //treatment of halfedges whose pair in the primal halfedge
                //is naked: 
                //first add two new vertices:
                // 1: at the midpoint of the primal halfedge
                // 2: at the start vertex of the primal halfedge
                //second add a new halfedge pair, with the 
                //with the first halfedge Previous to the current Dual
                // 1st HE: Starts at the primal start vertex
                // 1st HE: Next HE is the current Dual
                // 1st HE: Adjacent Face is the PrimalHE.StartVertex
                // 1st HE: Previous will be created and assigned later
                // 2nd HE: Starts at the new Midpoint Vertex
                // 2nd HE: Adjacent Face is -1
                // 2nd HE: Previous and Next will be created and assigned later

                if (DualHE.StartVertex < 0)
                {

                    NakedFaces.Add(DualHE.AdjacentFace);

                    PlanktonXYZ StartXYZ = P.Vertices[P.Halfedges[i].StartVertex].ToXYZ();
                    PlanktonXYZ EndXYZ = P.Vertices[P.Halfedges[PairIndex].StartVertex].ToXYZ();

                    //recreate the primal's Start Vertex
                    PlanktonVertex NewSV = new PlanktonVertex((float)StartXYZ.X, (float)StartXYZ.Y, (float)StartXYZ.Z);
                    //sets the new start vertex outgoing halfedge to be created next
                    NewSV.OutgoingHalfedge = NewHECounter;
                    D.Vertices.Add(NewSV);

                    //Create the End Vertex
                    PlanktonVertex NewMV = new PlanktonVertex((float)((StartXYZ.X + EndXYZ.X) * 0.5),
                        (float)((StartXYZ.Y + EndXYZ.Y) * 0.5), (float)((StartXYZ.Z + EndXYZ.Z) * 0.5));
                    NewMV.OutgoingHalfedge = i;
                    D.Vertices.Add(NewMV);

                    //Update the dual's Start Vertex to midpoint of the primal HE
                    //and the dual's previous Halfedge to the one newly created
                    DualHE.StartVertex = D.Vertices.Count - 1;
                    DualHE.PrevHalfedge = NewHECounter;

                    PlanktonHalfedge NewBoundary1 = PlanktonHalfedge.Unset;
                    NewBoundary1.StartVertex = D.Vertices.Count - 2; //from the primal start vertex
                    NewBoundary1.AdjacentFace = DualHE.AdjacentFace; //the current DualHE adjacent face
                    NewBoundary1.NextHalfedge = i; //the current DualHE

                    PlanktonHalfedge NewBoundary2 = PlanktonHalfedge.Unset;
                    NewBoundary2.StartVertex = D.Vertices.Count - 1; //the first new vertex

                    NewBoundaries.Add(NewBoundary1);
                    NewBoundaries.Add(NewBoundary2);

                    //Add the index for second round of updating after the first assignment of new halfedges
                    FinishEdges.Add(NewHECounter);
                    NewHECounter += 2;

                }
            }

            foreach (PlanktonHalfedge NewBoundary in NewBoundaries) { D.Halfedges.Add(NewBoundary); }

            foreach (int FinishEdge in FinishEdges)
            {

                int Exiter = 0; //fear the infinite loop
                int WorkingEdge = D.Halfedges[FinishEdge].NextHalfedge;

                //Cycle through the face until reaching the terminus
                //at a naked vertex
                do
                {
                    if (D.Halfedges[WorkingEdge].NextHalfedge < 0) { break; }
                    else { WorkingEdge = D.Halfedges[WorkingEdge].NextHalfedge; }
                    Exiter += 1;
                } while (Exiter < P.Halfedges.Count);

                int WorkingPair = D.Halfedges.GetPairHalfedge(WorkingEdge);

                //Build the new Halfedge in the same face
                PlanktonHalfedge NewBoundary1 = PlanktonHalfedge.Unset;
                NewBoundary1.StartVertex = D.Halfedges[WorkingPair].StartVertex;
                NewBoundary1.PrevHalfedge = WorkingEdge;
                NewBoundary1.NextHalfedge = FinishEdge;
                NewBoundary1.AdjacentFace = D.Halfedges[FinishEdge].AdjacentFace;

                //Build the naked pair
                PlanktonHalfedge NewBoundary2 = PlanktonHalfedge.Unset;
                NewBoundary2.StartVertex = D.Halfedges[FinishEdge].StartVertex;
                NewBoundary2.PrevHalfedge = D.Halfedges.GetPairHalfedge(FinishEdge);
                NewBoundary2.NextHalfedge = D.Halfedges.GetPairHalfedge(D.Halfedges[WorkingPair].PrevHalfedge);
                NewBoundary2.AdjacentFace = -1;

                D.Halfedges.Add(NewBoundary1);
                D.Halfedges.Add(NewBoundary2);

                //Set the next Halfedge of terminus to the new Halfedge on the interior face
                D.Halfedges[WorkingEdge].NextHalfedge = D.Halfedges.Count - 2;
                //Update the naked pair of the finish edge as connected to the new naked pair
                D.Halfedges[D.Halfedges.GetPairHalfedge(FinishEdge)].NextHalfedge = D.Halfedges.Count - 1;

            }

            return D;

        }

        public PlanktonMesh Dual()
        {
            // can later add options for other ways of defining face centres (barycenter/circumcenter etc)
            // won't work yet with naked boundaries

            PlanktonMesh P = this;
            PlanktonMesh D = new PlanktonMesh();

            //for every primal face, add the barycenter to the dual's vertex list
            //dual vertex outgoing HE is primal face's start HE
            //for every vertex of the primal, add a face to the dual
            //dual face's startHE is primal vertex's outgoing's pair

            for (int i = 0; i < P.Faces.Count; i++)
            {
                var fc = P.Faces.GetFaceCenter(i);
                D.Vertices.Add(new PlanktonVertex(fc.X, fc.Y, fc.Z));
                int[] FaceHalfedges = P.Faces.GetHalfedges(i);
                for (int j = 0; j < FaceHalfedges.Length; j++)
                {
                    if (P.Halfedges[P.Halfedges.GetPairHalfedge(FaceHalfedges[j])].AdjacentFace != -1)
                    {
                        // D.Vertices[i].OutgoingHalfedge = FaceHalfedges[j];
                        D.Vertices[D.Vertices.Count-1].OutgoingHalfedge = P.Halfedges.GetPairHalfedge(FaceHalfedges[j]);
                        break;
                    }
                }
            }

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                if (P.Vertices.NakedEdgeCount(i) == 0)
                {
                    int df = D.Faces.Add(PlanktonFace.Unset);
                    // D.Faces[i].FirstHalfedge = P.PairHalfedge(P.Vertices[i].OutgoingHalfedge);
                    D.Faces[df].FirstHalfedge = P.Vertices[i].OutgoingHalfedge;
                }
            }

            // dual halfedge start V is primal AdjacentFace
            // dual halfedge AdjacentFace is primal end V
            // dual nextHE is primal's pair's prev
            // dual prevHE is primal's next's pair

            // halfedge pairs stay the same

            for (int i = 0; i < P.Halfedges.Count; i++)
            {
                if ((P.Halfedges[i].AdjacentFace != -1) & (P.Halfedges[P.Halfedges.GetPairHalfedge(i)].AdjacentFace != -1))
                {
                    PlanktonHalfedge DualHE = PlanktonHalfedge.Unset;
                    PlanktonHalfedge PrimalHE = P.Halfedges[i];
                    //DualHE.StartVertex = PrimalHE.AdjacentFace;
                    DualHE.StartVertex = P.Halfedges[P.Halfedges.GetPairHalfedge(i)].AdjacentFace;

                    if (P.Vertices.NakedEdgeCount(PrimalHE.StartVertex) == 0)
                    {
                        //DualHE.AdjacentFace = P.Halfedges[P.PairHalfedge(i)].StartVertex;
                        DualHE.AdjacentFace = PrimalHE.StartVertex;
                    }
                    else { DualHE.AdjacentFace = -1; }
                    
                    //This will currently fail with open meshes...
                    //one option could be to build the dual with all halfedges, but mark some as dead
                    //if they connect to vertex -1
                    //mark the 'external' faces all as -1 (the ones that are dual to boundary verts)
                    //then go through and if any next or prevs are dead hes then replace them with the next one around
                    //this needs to be done repeatedly until no further change

                    //DualHE.NextHalfedge = P.Halfedges[P.PairHalfedge(i)].PrevHalfedge;
                    DualHE.NextHalfedge = P.Halfedges.GetPairHalfedge(PrimalHE.PrevHalfedge);

                    //DualHE.PrevHalfedge = P.PairHalfedge(PrimalHE.NextHalfedge);
                    DualHE.PrevHalfedge = P.Halfedges[P.Halfedges.GetPairHalfedge(i)].NextHalfedge;

                    D.Halfedges.Add(DualHE);
                }
            }
            return D;
        }

        /// <summary>
        /// Truncates the vertices of a mesh.
        /// </summary>
        /// <param name="t">Optional parameter for the normalised distance along each edge to control the amount of truncation.</param>
        /// <returns>A new mesh, the result of the truncation.</returns>
        public PlanktonMesh TruncateVertices(float t = 1f/3)
        {
            // TODO: handle special cases (t = 0.0, t = 0.5, t > 0.5)
            var tMesh = new PlanktonMesh(this);

            var vxyz = tMesh.Vertices.Select(v => v.ToXYZ()).ToArray();
            PlanktonXYZ v0, v1, v2;
            int[] oh;
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                oh = this.Vertices.GetHalfedges(i);
                tMesh.Vertices.TruncateVertex(i);
                foreach (var h in oh)
                {
                    v0 = vxyz[this.Halfedges[h].StartVertex];
                    v1 = vxyz[this.Halfedges.EndVertex(h)];
                    v2 = v0 + (v1 - v0) * t;
                    tMesh.Vertices.SetVertex(tMesh.Halfedges[h].StartVertex, v2.X, v2.Y, v2.Z);
                }
            }

            return tMesh;
        }

        /* Hide for the time being to avoid confusion...
        public void RefreshVertexNormals()
        {
        }
        public void RefreshFaceNormals()
        {
        }
        public void RefreshEdgeNormals()
        {
        }
        */

        /// <summary>
        /// Removes any unreferenced objects from arrays, reindexes as needed and shrinks arrays to minimum required size.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if halfedge count is odd after compaction.
        /// Most likely caused by only marking one of the halfedges in a pair for deletion.</exception>
        public void Compact()
        {
            // Compact vertices, faces and halfedges
            this.Vertices.CompactHelper();
            this.Faces.CompactHelper();
            this.Halfedges.CompactHelper();
        }

        //dihedral angle for an edge
        //

        //skeletonize - build a new mesh with 4 faces for each original edge

        #endregion
    }
}
