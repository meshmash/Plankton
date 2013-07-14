using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;

namespace Plankton
{
    public class P_mesh
    {
        #region "members"
        public List<P_vertex> Vertices;
        public List<P_halfedge> HalfEdges;        
        public List<P_face> Faces;
        #endregion

        #region "constructors"
        public P_mesh() //blank constructor
        {
            this.Faces = new List<P_face>();
            this.HalfEdges = new List<P_halfedge>();
            this.Vertices = new List<P_vertex>();
        }
        public P_mesh(Mesh M) //Create a Plankton Mesh from a Rhino Mesh
        {

            M.Vertices.CombineIdentical(true, true);
            M.Vertices.CullUnused();
            M.UnifyNormals();
            M.Weld(Math.PI);            

            this.Faces = new List<P_face>();
            this.HalfEdges = new List<P_halfedge>();
            this.Vertices = new List<P_vertex>();

            for (int i = 0; i < M.Vertices.Count; i++)
            {
                Vertices.Add(new P_vertex(M.TopologyVertices[i]));
            }
          
            for (int i = 0; i < M.Faces.Count; i++)
            {Faces.Add(new P_face()); }

            for (int i = 0; i < M.TopologyEdges.Count; i++)
            {
                P_halfedge HalfA = new P_halfedge();

                HalfA.StartVertex = M.TopologyEdges.GetTopologyVertices(i).I;

                if (Vertices[HalfA.StartVertex].OutgoingHalfEdge == -1)
                { Vertices[HalfA.StartVertex].OutgoingHalfEdge = HalfEdges.Count; }

                P_halfedge HalfB = new P_halfedge();

                HalfB.StartVertex = M.TopologyEdges.GetTopologyVertices(i).J;

                if (Vertices[HalfB.StartVertex].OutgoingHalfEdge == -1)
                { Vertices[HalfB.StartVertex].OutgoingHalfEdge = HalfEdges.Count + 1; }

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
                    if (Faces[HalfA.AdjacentFace].FirstHalfEdge == -1)
                    { Faces[HalfA.AdjacentFace].FirstHalfEdge = HalfEdges.Count; }
                    if (ConnectedFaces.Length > 1)
                    {
                        HalfB.AdjacentFace = ConnectedFaces[1];
                        if (Faces[HalfB.AdjacentFace].FirstHalfEdge == -1)
                        { Faces[HalfB.AdjacentFace].FirstHalfEdge = HalfEdges.Count + 1; }
                    }
                    else
                    {
                        HalfB.AdjacentFace = -1;
                    }
                }
                else
                {
                    HalfB.AdjacentFace = ConnectedFaces[0];

                    if (Faces[HalfB.AdjacentFace].FirstHalfEdge == -1)
                    { Faces[HalfB.AdjacentFace].FirstHalfEdge = HalfEdges.Count + 1; }

                    if (ConnectedFaces.Length > 1)
                    {
                        HalfA.AdjacentFace = ConnectedFaces[1];

                        if (Faces[HalfA.AdjacentFace].FirstHalfEdge == -1)
                        { Faces[HalfA.AdjacentFace].FirstHalfEdge = HalfEdges.Count; }
                    }
                    else
                    {
                        HalfA.AdjacentFace = -1;
                    }
                }
                HalfEdges.Add(HalfA);
                HalfEdges[2 * i].Index = 2 * i; //
                HalfEdges.Add(HalfB);
                HalfEdges[2 * i + 1].Index = 2 * i + 1; //
            }

            for (int i = 0; i < (HalfEdges.Count); i += 2)
            {
                int[] EndNeighbours = M.TopologyVertices.ConnectedTopologyVertices(HalfEdges[i + 1].StartVertex, true);
                for (int j = 0; j < EndNeighbours.Length; j++)
                {
                    if(EndNeighbours[j]==HalfEdges[i].StartVertex)
                    {
                        int EndOfNextHalfEdge = EndNeighbours[(j - 1 + EndNeighbours.Length) % EndNeighbours.Length];
                        int StartOfPrevOfPairHalfEdge = EndNeighbours[(j + 1) % EndNeighbours.Length];
                       
                        int NextEdge = M.TopologyEdges.GetEdgeIndex(HalfEdges[i + 1].StartVertex,EndOfNextHalfEdge);
                        int PrevPairEdge = M.TopologyEdges.GetEdgeIndex(HalfEdges[i + 1].StartVertex,StartOfPrevOfPairHalfEdge);

                        if (M.TopologyEdges.GetTopologyVertices(NextEdge).I == HalfEdges[i + 1].StartVertex)
                        { HalfEdges[i].NextHalfEdge = NextEdge * 2; }
                        else
                        { HalfEdges[i].NextHalfEdge = NextEdge * 2 + 1; }

                        if (M.TopologyEdges.GetTopologyVertices(PrevPairEdge).J == HalfEdges[i + 1].StartVertex)
                        { HalfEdges[i + 1].PrevHalfEdge = PrevPairEdge * 2; }
                        else
                        { HalfEdges[i + 1].PrevHalfEdge = PrevPairEdge * 2+1; }                     
                        break;
                    }                    
                }

                int[] StartNeighbours = M.TopologyVertices.ConnectedTopologyVertices(HalfEdges[i].StartVertex, true);
                for (int j = 0; j < StartNeighbours.Length; j++)
                {
                    if (StartNeighbours[j] == HalfEdges[i+1].StartVertex)
                    {
                        int EndOfNextOfPairHalfEdge = StartNeighbours[(j - 1 + StartNeighbours.Length) % StartNeighbours.Length];
                        int StartOfPrevHalfEdge = StartNeighbours[(j + 1) % StartNeighbours.Length];

                        int NextPairEdge = M.TopologyEdges.GetEdgeIndex(HalfEdges[i].StartVertex, EndOfNextOfPairHalfEdge);
                        int PrevEdge = M.TopologyEdges.GetEdgeIndex(HalfEdges[i].StartVertex, StartOfPrevHalfEdge);

                        if (M.TopologyEdges.GetTopologyVertices(NextPairEdge).I == HalfEdges[i].StartVertex)
                        { HalfEdges[i + 1].NextHalfEdge = NextPairEdge * 2; }
                        else
                        { HalfEdges[i + 1].NextHalfEdge = NextPairEdge * 2 + 1; }

                        if (M.TopologyEdges.GetTopologyVertices(PrevEdge).J == HalfEdges[i].StartVertex)
                        { HalfEdges[i].PrevHalfEdge = PrevEdge * 2; }
                        else
                        { HalfEdges[i].PrevHalfEdge = PrevEdge * 2 + 1; }
                        break;
                    }
                } 
            }            
        }

        #endregion

        #region "general methods"

        public Mesh ToRhinoMesh()
        {
            // could add different options for triangulating ngons later
            P_mesh P = this;
            Mesh M = new Mesh();
            for (int i = 0; i < P.Vertices.Count; i++)
            {
                M.Vertices.Add(P.Vertices[i].Position);
            }
            for (int i = 0; i < P.Faces.Count; i++)
            {
                List<int> FaceVs = P.FaceVertices(i);
                if (FaceVs.Count == 3)
                {
                    M.Faces.AddFace(FaceVs[0], FaceVs[1], FaceVs[2]);
                }
                if (FaceVs.Count == 4)
                {
                    M.Faces.AddFace(FaceVs[0], FaceVs[1], FaceVs[2], FaceVs[3]);
                }
                if (FaceVs.Count > 4)
                {
                    M.Vertices.Add(P.FaceCentroid(i));
                    for (int j = 0; j < FaceVs.Count; j++)
                    {
                        M.Faces.AddFace(FaceVs[j], FaceVs[(j + 1) % FaceVs.Count], M.Vertices.Count - 1);
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
                List<int> VertIndexes = FaceVertices(i);
                for (int j = 0; j <= VertIndexes.Count; j++)
                {
                    FacePoly.Add(Vertices[VertIndexes[j % VertIndexes.Count]].Position);
                }
                Polylines.Add(FacePoly);
            }
            return Polylines;
        }

       // public void ReIndex() //clear away all the dead elements to save space      
       // //maybe it is better to just create a fresh one rather than trying to shuffle the existing
       // {
       // }

        public P_mesh Dual()
        {
            // can later add options for other ways of defining face centres (barycenter/circumcenter etc)
            // won't work yet with naked boundaries

            P_mesh P = this;
            P_mesh D = new P_mesh();

            //for every primal face, add the barycenter to the dual's vertex list
            //dual vertex outgoing HE is primal face's start HE
            //for every vertex of the primal, add a face to the dual
            //dual face's startHE is primal vertex's outgoing's pair

            for (int i = 0; i < P.Faces.Count; i++)
            {                               
                    D.Vertices.Add(new P_vertex(P.FaceCentroid(i)));                   
                    List<int> FaceHalfedges = P.FaceHEs(i);
                    for (int j = 0; j < FaceHalfedges.Count; j++)
                    {
                        if (P.HalfEdges[P.PairHalfEdge(FaceHalfedges[j])].AdjacentFace != -1)
                        {
                         // D.Vertices[i].OutgoingHalfEdge = FaceHalfedges[j];
                            D.Vertices[D.Vertices.Count-1].OutgoingHalfEdge = P.PairHalfEdge(FaceHalfedges[j]);
                            break;
                        }
                    }                                   
            }

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                if (P.VertexNakedEdgeCount(i) == 0)
                {
                    D.Faces.Add(new P_face());
                 // D.Faces[i].FirstHalfEdge = P.PairHalfEdge(P.Vertices[i].OutgoingHalfEdge);
                    D.Faces[D.Faces.Count-1].FirstHalfEdge = P.Vertices[i].OutgoingHalfEdge;
                }   
            }

            // dual halfedge start V is primal AdjacentFace
            // dual halfedge AdjacentFace is primal end V            
            // dual nextHE is primal's pair's prev
            // dual prevHE is primal's next's pair

            // halfedge pairs stay the same

            int newIndex = 0;

            for (int i = 0; i < P.HalfEdges.Count; i++)
            {
                if ((P.HalfEdges[i].AdjacentFace != -1) & (P.HalfEdges[P.PairHalfEdge(i)].AdjacentFace != -1))
                {
                    P_halfedge DualHE = new P_halfedge();                    
                    P_halfedge PrimalHE = P.HalfEdges[i];
                    //DualHE.StartVertex = PrimalHE.AdjacentFace;
                    DualHE.StartVertex = P.HalfEdges[P.PairHalfEdge(i)].AdjacentFace;

                    if (P.VertexNakedEdgeCount(PrimalHE.StartVertex) == 0)
                    {
                        //DualHE.AdjacentFace = P.HalfEdges[P.PairHalfEdge(i)].StartVertex;
                        DualHE.AdjacentFace = PrimalHE.StartVertex;
                    }
                    else { DualHE.AdjacentFace = -1; }
                                   
                    //This will currently fail with open meshes...
                    //one option could be to build the dual with all halfedges, but mark some as dead
                    //if they connect to vertex -1
                    //mark the 'external' faces all as -1 (the ones that are dual to boundary verts)
                    //then go through and if any next or prevs are dead hes then replace them with the next one around
                    //this needs to be done repeatedly until no further change

                    //DualHE.NextHalfEdge = P.HalfEdges[P.PairHalfEdge(i)].PrevHalfEdge;
                    DualHE.NextHalfEdge = P.PairHalfEdge(PrimalHE.PrevHalfEdge);

                    //DualHE.PrevHalfEdge = P.PairHalfEdge(PrimalHE.NextHalfEdge);
                    DualHE.PrevHalfEdge = P.HalfEdges[P.PairHalfEdge(i)].NextHalfEdge;

                    DualHE.Index = newIndex;

                    D.HalfEdges.Add(DualHE);
                    newIndex += 1;
                }               
            }            
            return D;
        }

        public void FlipEdge(int E)
        {

            // to flip an edge
            // update 2 start verts
            // 2 adjacentfaces
            // 6 nexts
            // 6 prevs
            // for each vert, check if need to update outgoing
            // for each face, check if need to update start he
        }

        public void SplitEdge(int E)
        {
            // add a new vertex
            // add 2 new faces
        }

        public void CollapseEdge(int E)
        {
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
            int FirstHalfEdge = PairHalfEdge(Vertices[V].OutgoingHalfEdge);
            int CurrentHalfEdge = FirstHalfEdge;            
            do{
                NeighbourVs.Add(HalfEdges[CurrentHalfEdge].StartVertex);
                CurrentHalfEdge = PairHalfEdge(HalfEdges[CurrentHalfEdge].NextHalfEdge);
              } while (CurrentHalfEdge != FirstHalfEdge);             
            return NeighbourVs;
        }

        public List<int> VertexFaces(int V)
        // get the faces which use this vertex
        {
            List<int> NeighbourFs = new List<int>();            
            int FirstHalfEdge = Vertices[V].OutgoingHalfEdge;
            int CurrentHalfEdge = FirstHalfEdge;
            do
            {
                NeighbourFs.Add(HalfEdges[CurrentHalfEdge].AdjacentFace);                
                CurrentHalfEdge = HalfEdges[PairHalfEdge(CurrentHalfEdge)].NextHalfEdge;
            } while (CurrentHalfEdge != FirstHalfEdge);            
            return NeighbourFs;
        }


        public List<int> VertexAllOutHE(int V) // all the outgoing Halfedges from a vertex
        {
            List<int> OutHEs = new List<int>();
            int FirstHalfEdge = Vertices[V].OutgoingHalfEdge;
            int CurrentHalfEdge = FirstHalfEdge;
            do
            {
                OutHEs.Add(CurrentHalfEdge);             
                CurrentHalfEdge = HalfEdges[PairHalfEdge(CurrentHalfEdge)].NextHalfEdge;
            } while (CurrentHalfEdge != FirstHalfEdge);
            return OutHEs;
        }

        public List<int> VertexAllInHE(int V) // all the incoming Halfedges to a vertex
        {
            List<int> InHEs = new List<int>();
            int FirstHalfEdge = PairHalfEdge(Vertices[V].OutgoingHalfEdge);
            int CurrentHalfEdge = FirstHalfEdge;
            do
            {
                InHEs.Add(CurrentHalfEdge);
                CurrentHalfEdge = PairHalfEdge(HalfEdges[CurrentHalfEdge].NextHalfEdge);
            } while (CurrentHalfEdge != FirstHalfEdge);
            return InHEs;
        }

        public List<int> FaceHEs(int F)
        {
            List<int> HEs = new List<int>();
            int FirstHalfEdge = Faces[F].FirstHalfEdge;
            int CurrentHalfEdge = FirstHalfEdge;
            do
            {
                HEs.Add(CurrentHalfEdge);
                CurrentHalfEdge = HalfEdges[CurrentHalfEdge].NextHalfEdge;
            }
            while (CurrentHalfEdge != FirstHalfEdge);
            return HEs;
        }

        public List<int> FaceVertices(int F)
        //get the vertices making up a face
        {
            List<int> FaceVs = new List<int>();
            int FirstHalfEdge = Faces[F].FirstHalfEdge;
            int CurrentHalfEdge = FirstHalfEdge;
            do
            {
                FaceVs.Add(HalfEdges[CurrentHalfEdge].StartVertex);
                CurrentHalfEdge = HalfEdges[CurrentHalfEdge].NextHalfEdge;
            }
            while (CurrentHalfEdge != FirstHalfEdge);
            return FaceVs;
        }

        public Point3d FaceCentroid(int F)
        //the barycenter of a face's vertices
        {
            List<int> FaceVs = FaceVertices(F);
            Point3d Centroid = new Point3d(0, 0, 0);
            foreach (int i in FaceVs)
            {
                Centroid = Centroid + Vertices[i].Position;
            }
            Centroid *= 1.0 / FaceVs.Count;
            return Centroid;
        }

        public int PairHalfEdge(int I)
        {
            if (I % 2 == 0)
            { return I + 1; }
            else
            { return I - 1; }
        }

        public int IncomingHalfEdge(int I)
        {
            return PairHalfEdge(Vertices[I].OutgoingHalfEdge);
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
                if (HalfEdges[Outgoing[i]].AdjacentFace == -1) { NakedCount++; }
                if (HalfEdges[PairHalfEdge(Outgoing[i])].AdjacentFace == -1) { NakedCount++; }
            }
            return NakedCount;
        }

        public int FaceNakedEdgeCount(int F)
        {
            int NakedCount = 0;
            List<int> FaceHEdges = FaceHEs(F);
            foreach (int i in FaceHEdges)
            {
                if (HalfEdges[PairHalfEdge(i)].AdjacentFace == -1) { NakedCount++; }
            }
            return NakedCount;
        }

        #endregion

    }
    public class P_vertex
    {
        public Point3d Position;
        public int OutgoingHalfEdge;
        //       
        public bool Dead;
        public Vector3d Normal;
        public P_vertex()
        {
            OutgoingHalfEdge = -1;
        }
        public P_vertex(Point3f V)
        {
            Position = (Point3d)V;
            OutgoingHalfEdge = -1;
        }
        public P_vertex(Point3d V)
        {
            Position = V;
            OutgoingHalfEdge = -1;
        }
    }
    public class P_halfedge
    {
        //primary properties - the minimum ones needed for the halfedge structure
        public int StartVertex;
        public int AdjacentFace;
        public int NextHalfEdge;
        //secondary properties - these should still be kept updated if you change the topology
        public int PrevHalfEdge;
        public int Index;
        //tertiary properties - less vital, calculate or refresh only as needed
        public Vector3d Normal;

        public P_halfedge()
        {
            StartVertex = -1;
            AdjacentFace = -1;
            NextHalfEdge = -1;
            PrevHalfEdge = -1;
        }
        public P_halfedge(int Start, int AdjFace, int Next)
        {
            StartVertex = Start;
            AdjacentFace = AdjFace;
            NextHalfEdge = Next;
        }
        public int Pair()
        {
            if (Index % 2 == 0)
            { return Index + 1; }
            else
            { return Index - 1; }
        }

    }
    public class P_face
    {
        public int FirstHalfEdge;
        //
        public int EdgeCount;
        public Vector3d Normal;
        public double Area;
        public P_face()
        {
            FirstHalfEdge = -1;
        }
    }   
}
