using Rhino.Geometry;
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
        #region "members"
        public List<PlanktonVertex> Vertices;
        public PlanktonHalfEdgeList Halfedges;
        public PlanktonFaceList Faces;
        #endregion

        #region "constructors"
        public PlanktonMesh() //blank constructor
        {
            this.Faces = new PlanktonFaceList(this);
            this.Halfedges = new PlanktonHalfEdgeList(this);
            this.Vertices = new List<PlanktonVertex>();
        }
        public PlanktonMesh(Mesh M) //Create a Plankton Mesh from a Rhino Mesh
            : this()
        {

            M.Vertices.CombineIdentical(true, true);
            M.Vertices.CullUnused();
            M.UnifyNormals();
            M.Weld(Math.PI);

            for (int i = 0; i < M.Vertices.Count; i++)
            {
                Vertices.Add(new PlanktonVertex(M.TopologyVertices[i]));
            }
            
            for (int i = 0; i < M.Faces.Count; i++)
            {Faces.Add(new PlanktonFace()); }

            for (int i = 0; i < M.TopologyEdges.Count; i++)
            {
                PlanktonHalfedge HalfA = new PlanktonHalfedge();

                HalfA.StartVertex = M.TopologyEdges.GetTopologyVertices(i).I;

                if (Vertices[HalfA.StartVertex].OutgoingHalfedge == -1)
                { Vertices[HalfA.StartVertex].OutgoingHalfedge = Halfedges.Count; }

                PlanktonHalfedge HalfB = new PlanktonHalfedge();

                HalfB.StartVertex = M.TopologyEdges.GetTopologyVertices(i).J;

                if (Vertices[HalfB.StartVertex].OutgoingHalfedge == -1)
                { Vertices[HalfB.StartVertex].OutgoingHalfedge = Halfedges.Count + 1; }

                bool[] Match;
                int[] ConnectedFaces = M.TopologyEdges.GetConnectedFaces(i, out Match);

                //Note for Steve Baer : This Match bool doesn't seem to work on triangulated meshes - it often returns true
                //for both faces, even for a properly oriented manifold mesh, which can't be right
                //So - making our own check for matching:
                //(I suspect the problem is related to C being the same as D for triangles, so best to
                //deal with them separately just to make sure)
                //loop through the vertices of the face until finding the one which is the same as the start of the edge
                //iff the next vertex around the face is the end of the edge then it matches.

                Match[0] = false;
                if (Match.Length > 1)
                {Match[1] = true;}

                int VertA = M.TopologyVertices.TopologyVertexIndex(M.Faces[ConnectedFaces[0]].A);
                int VertB = M.TopologyVertices.TopologyVertexIndex(M.Faces[ConnectedFaces[0]].B);
                int VertC = M.TopologyVertices.TopologyVertexIndex(M.Faces[ConnectedFaces[0]].C);
                int VertD = M.TopologyVertices.TopologyVertexIndex(M.Faces[ConnectedFaces[0]].D);

                if ((VertA == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertB == M.TopologyEdges.GetTopologyVertices(i).J))
                { Match[0] = true;
                }
                if ((VertB == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertC == M.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertC == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertD == M.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertD == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertA == M.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                //I don't think these next 2 should ever be needed, but just in case:
                if ((VertC == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertA == M.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertB == M.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertD == M.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                
                if (Match[0] == true)
                {
                    HalfA.AdjacentFace = ConnectedFaces[0];
                    if (Faces[HalfA.AdjacentFace].FirstHalfedge == -1)
                    { Faces[HalfA.AdjacentFace].FirstHalfedge = Halfedges.Count; }
                    if (ConnectedFaces.Length > 1)
                    {
                        HalfB.AdjacentFace = ConnectedFaces[1];
                        if (Faces[HalfB.AdjacentFace].FirstHalfedge == -1)
                        { Faces[HalfB.AdjacentFace].FirstHalfedge = Halfedges.Count + 1; }
                    }
                    else
                    {
                        HalfB.AdjacentFace = -1;
                    }
                }
                else
                {
                    HalfB.AdjacentFace = ConnectedFaces[0];

                    if (Faces[HalfB.AdjacentFace].FirstHalfedge == -1)
                    { Faces[HalfB.AdjacentFace].FirstHalfedge = Halfedges.Count + 1; }

                    if (ConnectedFaces.Length > 1)
                    {
                        HalfA.AdjacentFace = ConnectedFaces[1];

                        if (Faces[HalfA.AdjacentFace].FirstHalfedge == -1)
                        { Faces[HalfA.AdjacentFace].FirstHalfedge = Halfedges.Count; }
                    }
                    else
                    {
                        HalfA.AdjacentFace = -1;
                    }
                }
                Halfedges.Add(HalfA);
                Halfedges[2 * i].Index = 2 * i; //
                Halfedges.Add(HalfB);
                Halfedges[2 * i + 1].Index = 2 * i + 1; //
            }

            for (int i = 0; i < (Halfedges.Count); i += 2)
            {
                int[] EndNeighbours = M.TopologyVertices.ConnectedTopologyVertices(Halfedges[i + 1].StartVertex, true);
                for (int j = 0; j < EndNeighbours.Length; j++)
                {
                    if(EndNeighbours[j]==Halfedges[i].StartVertex)
                    {
                        int EndOfNextHalfedge = EndNeighbours[(j - 1 + EndNeighbours.Length) % EndNeighbours.Length];
                        int StartOfPrevOfPairHalfedge = EndNeighbours[(j + 1) % EndNeighbours.Length];
                        
                        int NextEdge = M.TopologyEdges.GetEdgeIndex(Halfedges[i + 1].StartVertex,EndOfNextHalfedge);
                        int PrevPairEdge = M.TopologyEdges.GetEdgeIndex(Halfedges[i + 1].StartVertex,StartOfPrevOfPairHalfedge);

                        if (M.TopologyEdges.GetTopologyVertices(NextEdge).I == Halfedges[i + 1].StartVertex)
                        { Halfedges[i].NextHalfedge = NextEdge * 2; }
                        else
                        { Halfedges[i].NextHalfedge = NextEdge * 2 + 1; }

                        if (M.TopologyEdges.GetTopologyVertices(PrevPairEdge).J == Halfedges[i + 1].StartVertex)
                        { Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2; }
                        else
                        { Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2+1; }
                        break;
                    }
                }

                int[] StartNeighbours = M.TopologyVertices.ConnectedTopologyVertices(Halfedges[i].StartVertex, true);
                for (int j = 0; j < StartNeighbours.Length; j++)
                {
                    if (StartNeighbours[j] == Halfedges[i+1].StartVertex)
                    {
                        int EndOfNextOfPairHalfedge = StartNeighbours[(j - 1 + StartNeighbours.Length) % StartNeighbours.Length];
                        int StartOfPrevHalfedge = StartNeighbours[(j + 1) % StartNeighbours.Length];

                        int NextPairEdge = M.TopologyEdges.GetEdgeIndex(Halfedges[i].StartVertex, EndOfNextOfPairHalfedge);
                        int PrevEdge = M.TopologyEdges.GetEdgeIndex(Halfedges[i].StartVertex, StartOfPrevHalfedge);

                        if (M.TopologyEdges.GetTopologyVertices(NextPairEdge).I == Halfedges[i].StartVertex)
                        { Halfedges[i + 1].NextHalfedge = NextPairEdge * 2; }
                        else
                        { Halfedges[i + 1].NextHalfedge = NextPairEdge * 2 + 1; }

                        if (M.TopologyEdges.GetTopologyVertices(PrevEdge).J == Halfedges[i].StartVertex)
                        { Halfedges[i].PrevHalfedge = PrevEdge * 2; }
                        else
                        { Halfedges[i].PrevHalfedge = PrevEdge * 2 + 1; }
                        break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonMesh"/> class.
        /// Constructs a new halfedge mesh using the face-vertex representation of another mesh.
        /// </summary>
        /// <param name="pts">A list of cartesian coordinates.</param>
        /// <param name="faces">A list faces, described by the indices of their vertices
        /// (ordered anticlockwise around the face).</param>
        public PlanktonMesh(IEnumerable<Point3d> pts, IEnumerable<IEnumerable<int>> faces)
            : this()
        {
            // Add vertices
            foreach (Point3d pt in pts)
            {
                this.Vertices.Add(new PlanktonVertex(pt));
            }
            
            // Add faces (and half-edges)
            foreach (IEnumerable<int> face in faces)
            {
                this.Faces.AddFace(face);
            }
        }
        #endregion

        #region "general methods"
        public Mesh ToRhinoMesh()
        {
            // could add different options for triangulating ngons later
            PlanktonMesh P = this;
            Mesh M = new Mesh();
            for (int i = 0; i < P.Vertices.Count; i++)
            {
                M.Vertices.Add(P.Vertices[i].Position);
            }
            for (int i = 0; i < P.Faces.Count; i++)
            {
                int[] FaceVs = P.Faces.GetVertices(i);
                if (FaceVs.Length == 3)
                {
                    M.Faces.AddFace(FaceVs[0], FaceVs[1], FaceVs[2]);
                }
                if (FaceVs.Length == 4)
                {
                    M.Faces.AddFace(FaceVs[0], FaceVs[1], FaceVs[2], FaceVs[3]);
                }
                if (FaceVs.Length > 4)
                {
                    M.Vertices.Add(P.Faces.FaceCentroid(i));
                    for (int j = 0; j < FaceVs.Length; j++)
                    {
                        M.Faces.AddFace(FaceVs[j], FaceVs[(j + 1) % FaceVs.Length], M.Vertices.Count - 1);
                    }
                }
            }
            return M;
        }

        public List<Polyline> ToPolylines()
        {
            List<Polyline> Polylines = new List<Polyline>();
            for (int i = 0; i < Faces.Count; i++)
            {
                Polyline FacePoly = new Polyline();
                int[] VertIndexes = this.Faces.GetVertices(i);
                for (int j = 0; j <= VertIndexes.Length; j++)
                {
                    FacePoly.Add(Vertices[VertIndexes[j % VertIndexes.Length]].Position);
                }
                Polylines.Add(FacePoly);
            }
            return Polylines;
        }

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
                D.Vertices.Add(new PlanktonVertex(P.Faces.FaceCentroid(i)));
                int[] FaceHalfedges = P.Faces.GetHalfedges(i);
                for (int j = 0; j < FaceHalfedges.Length; j++)
                {
                    if (P.Halfedges[P.Halfedges.PairHalfedge(FaceHalfedges[j])].AdjacentFace != -1)
                    {
                        // D.Vertices[i].OutgoingHalfedge = FaceHalfedges[j];
                        D.Vertices[D.Vertices.Count-1].OutgoingHalfedge = P.Halfedges.PairHalfedge(FaceHalfedges[j]);
                        break;
                    }
                }
            }

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                if (P.VertexNakedEdgeCount(i) == 0)
                {
                    D.Faces.Add(new PlanktonFace());
                    // D.Faces[i].FirstHalfedge = P.PairHalfedge(P.Vertices[i].OutgoingHalfedge);
                    D.Faces[D.Faces.Count-1].FirstHalfedge = P.Vertices[i].OutgoingHalfedge;
                }
            }

            // dual halfedge start V is primal AdjacentFace
            // dual halfedge AdjacentFace is primal end V
            // dual nextHE is primal's pair's prev
            // dual prevHE is primal's next's pair

            // halfedge pairs stay the same

            int newIndex = 0;

            for (int i = 0; i < P.Halfedges.Count; i++)
            {
                if ((P.Halfedges[i].AdjacentFace != -1) & (P.Halfedges[P.Halfedges.PairHalfedge(i)].AdjacentFace != -1))
                {
                    PlanktonHalfedge DualHE = new PlanktonHalfedge();
                    PlanktonHalfedge PrimalHE = P.Halfedges[i];
                    //DualHE.StartVertex = PrimalHE.AdjacentFace;
                    DualHE.StartVertex = P.Halfedges[P.Halfedges.PairHalfedge(i)].AdjacentFace;

                    if (P.VertexNakedEdgeCount(PrimalHE.StartVertex) == 0)
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
                    DualHE.NextHalfedge = P.Halfedges.PairHalfedge(PrimalHE.PrevHalfedge);

                    //DualHE.PrevHalfedge = P.PairHalfedge(PrimalHE.NextHalfedge);
                    DualHE.PrevHalfedge = P.Halfedges[P.Halfedges.PairHalfedge(i)].NextHalfedge;

                    DualHE.Index = newIndex;

                    D.Halfedges.Add(DualHE);
                    newIndex += 1;
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

        //dihedral angle for an edge
        //

        //skeletonize - build a new mesh with 4 faces for each original edge

        #endregion

        #region "Adjacencies"
        
        public List<int> VertexNeighbours(int V)
            //get the vertices connected to a given vertex by an edge (aka 1-ring)
        {
            List<int> NeighbourVs = new List<int>();
            int FirstHalfedge = Halfedges.PairHalfedge(Vertices[V].OutgoingHalfedge);
            int CurrentHalfedge = FirstHalfedge;
            do{
                NeighbourVs.Add(Halfedges[CurrentHalfedge].StartVertex);
                CurrentHalfedge = Halfedges.PairHalfedge(Halfedges[CurrentHalfedge].NextHalfedge);
            } while (CurrentHalfedge != FirstHalfedge);
            return NeighbourVs;
        }

        public List<int> VertexFaces(int V)
            // get the faces which use this vertex
        {
            List<int> NeighbourFs = new List<int>();
            int FirstHalfedge = Vertices[V].OutgoingHalfedge;
            int CurrentHalfedge = FirstHalfedge;
            do
            {
                NeighbourFs.Add(Halfedges[CurrentHalfedge].AdjacentFace);
                CurrentHalfedge = Halfedges[Halfedges.PairHalfedge(CurrentHalfedge)].NextHalfedge;
            } while (CurrentHalfedge != FirstHalfedge);
            return NeighbourFs;
        }


        public List<int> VertexAllOutHE(int V) // all the outgoing Halfedges from a vertex
        {
            List<int> OutHEs = new List<int>();
            int FirstHalfedge = Vertices[V].OutgoingHalfedge;
            int CurrentHalfedge = FirstHalfedge;
            do
            {
                OutHEs.Add(CurrentHalfedge);
                CurrentHalfedge = Halfedges[Halfedges.PairHalfedge(CurrentHalfedge)].NextHalfedge;
            } while (CurrentHalfedge != FirstHalfedge);
            return OutHEs;
        }

        public List<int> VertexAllInHE(int V) // all the incoming Halfedges to a vertex
        {
            List<int> InHEs = new List<int>();
            int FirstHalfedge = Halfedges.PairHalfedge(Vertices[V].OutgoingHalfedge);
            int CurrentHalfedge = FirstHalfedge;
            do
            {
                InHEs.Add(CurrentHalfedge);
                CurrentHalfedge = Halfedges.PairHalfedge(Halfedges[CurrentHalfedge].NextHalfedge);
            } while (CurrentHalfedge != FirstHalfedge);
            return InHEs;
        }

        public int IncomingHalfedge(int I)
        {
            return Halfedges.PairHalfedge(Vertices[I].OutgoingHalfedge);
        }

        public int VertexNakedEdgeCount(int V)
            //the number of connected halfedges which are naked
            //(this should also be the number of naked connected actual *edges*
            // - because if the hemesh is good then there should never be a pair of 2 boundary halfedges)
        {
            int NakedCount = 0;
            List<int> Outgoing = VertexAllOutHE(V);
            for (int i = 0; i < Outgoing.Count; i++)
            {
                if (Halfedges[Outgoing[i]].AdjacentFace == -1) { NakedCount++; }
                if (Halfedges[Halfedges.PairHalfedge(Outgoing[i])].AdjacentFace == -1) { NakedCount++; }
            }
            return NakedCount;
        }

        #endregion
    }
}
