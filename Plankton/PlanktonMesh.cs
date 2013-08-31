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
        // public void ReIndex() //clear away all the dead elements to save space
        // //maybe it is better to just create a fresh one rather than trying to shuffle the existing
        // {
        // }

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

        public void RefreshVertexNormals()
        {
        }
        public void RefreshFaceNormals()
        {
        }
        public void RefreshEdgeNormals()
        {
        }

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
