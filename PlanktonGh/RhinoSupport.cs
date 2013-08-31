using Plankton;
using Rhino.Geometry;
using System;

namespace PlanktonGh
{
    /// <summary>
    /// Provides static and extension methods to add support for Rhino geometry in <see cref="Plankton"/>.
    /// </summary>
    public static class RhinoSupport
    {
        public static string HelloWorld()
        {
            return "Hello World!";
        }
        
        /// <summary>
        /// Creates a Plankton halfedge mesh from a Rhino mesh.
        /// Uses the topology of the Rhino mesh directly.
        /// </summary>
        /// <returns>A <see cref="PlanktonMesh"/> which represents the topology and geometry of the source mesh.</returns>
        /// <param name="source">A Rhino mesh to convert from.</param>
        public static PlanktonMesh ToPlanktonMesh(this Mesh source)
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            source.Vertices.CombineIdentical(true, true);
            source.Vertices.CullUnused();
            source.UnifyNormals();
            source.Weld(Math.PI);

            foreach (Point3f v in source.TopologyVertices)
            {
                pMesh.Vertices.Add(v.X, v.Y, v.Z);
            }

            for (int i = 0; i < source.Faces.Count; i++)
            {
                pMesh.Faces.Add(new PlanktonFace());
            }

            for (int i = 0; i < source.TopologyEdges.Count; i++)
            {
                PlanktonHalfedge HalfA = new PlanktonHalfedge();

                HalfA.StartVertex = source.TopologyEdges.GetTopologyVertices(i).I;

                if (pMesh.Vertices [HalfA.StartVertex].OutgoingHalfedge == -1) {
                    pMesh.Vertices [HalfA.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count;
                }

                PlanktonHalfedge HalfB = new PlanktonHalfedge();

                HalfB.StartVertex = source.TopologyEdges.GetTopologyVertices(i).J;

                if (pMesh.Vertices [HalfB.StartVertex].OutgoingHalfedge == -1) {
                    pMesh.Vertices [HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;
                }

                bool[] Match;
                int[] ConnectedFaces = source.TopologyEdges.GetConnectedFaces(i, out Match);

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

                int VertA = source.TopologyVertices.TopologyVertexIndex(source.Faces[ConnectedFaces[0]].A);
                int VertB = source.TopologyVertices.TopologyVertexIndex(source.Faces[ConnectedFaces[0]].B);
                int VertC = source.TopologyVertices.TopologyVertexIndex(source.Faces[ConnectedFaces[0]].C);
                int VertD = source.TopologyVertices.TopologyVertexIndex(source.Faces[ConnectedFaces[0]].D);

                if ((VertA == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertB == source.TopologyEdges.GetTopologyVertices(i).J))
                { Match[0] = true;
                }
                if ((VertB == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertC == source.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertC == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertD == source.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertD == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertA == source.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                //I don't think these next 2 should ever be needed, but just in case:
                if ((VertC == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertA == source.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }
                if ((VertB == source.TopologyEdges.GetTopologyVertices(i).I)
                    && (VertD == source.TopologyEdges.GetTopologyVertices(i).J))
                {
                    Match[0] = true;
                }

                if (Match[0] == true)
                {
                    HalfA.AdjacentFace = ConnectedFaces[0];
                    if (pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge == -1) {
                        pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count;
                    }
                    if (ConnectedFaces.Length > 1)
                    {
                        HalfB.AdjacentFace = ConnectedFaces[1];
                        if (pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge == -1) {
                            pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count + 1;
                        }
                    }
                    else
                    {
                        HalfB.AdjacentFace = -1;
                    }
                }
                else
                {
                    HalfB.AdjacentFace = ConnectedFaces[0];

                    if (pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge == -1) {
                        pMesh.Faces[HalfB.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count + 1;
                    }

                    if (ConnectedFaces.Length > 1)
                    {
                        HalfA.AdjacentFace = ConnectedFaces[1];

                        if (pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge == -1) {
                            pMesh.Faces[HalfA.AdjacentFace].FirstHalfedge = pMesh.Halfedges.Count;
                        }
                    }
                    else
                    {
                        HalfA.AdjacentFace = -1;
                    }
                }
                pMesh.Halfedges.Add(HalfA);
                //pMesh.Halfedges[2 * i].Index = 2 * i; //
                pMesh.Halfedges.Add(HalfB);
                //pMesh.Halfedges[2 * i + 1].Index = 2 * i + 1; //
            }

            for (int i = 0; i < (pMesh.Halfedges.Count); i += 2)
            {
                int[] EndNeighbours = source.TopologyVertices.ConnectedTopologyVertices(pMesh.Halfedges[i + 1].StartVertex, true);
                for (int j = 0; j < EndNeighbours.Length; j++)
                {
                    if(EndNeighbours[j] == pMesh.Halfedges[i].StartVertex)
                    {
                        int EndOfNextHalfedge = EndNeighbours[(j - 1 + EndNeighbours.Length) % EndNeighbours.Length];
                        int StartOfPrevOfPairHalfedge = EndNeighbours[(j + 1) % EndNeighbours.Length];

                        int NextEdge = source.TopologyEdges.GetEdgeIndex(pMesh.Halfedges[i + 1].StartVertex,EndOfNextHalfedge);
                        int PrevPairEdge = source.TopologyEdges.GetEdgeIndex(pMesh.Halfedges[i + 1].StartVertex,StartOfPrevOfPairHalfedge);

                        if (source.TopologyEdges.GetTopologyVertices(NextEdge).I == pMesh.Halfedges[i + 1].StartVertex) {
                            pMesh.Halfedges[i].NextHalfedge = NextEdge * 2;
                        } else {
                            pMesh.Halfedges[i].NextHalfedge = NextEdge * 2 + 1;
                        }

                        if (source.TopologyEdges.GetTopologyVertices(PrevPairEdge).J == pMesh.Halfedges[i + 1].StartVertex) {
                            pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2;
                        } else {
                            pMesh.Halfedges[i + 1].PrevHalfedge = PrevPairEdge * 2+1;
                        }
                        break;
                    }
                }

                int[] StartNeighbours = source.TopologyVertices.ConnectedTopologyVertices(pMesh.Halfedges[i].StartVertex, true);
                for (int j = 0; j < StartNeighbours.Length; j++)
                {
                    if (StartNeighbours[j] == pMesh.Halfedges[i+1].StartVertex)
                    {
                        int EndOfNextOfPairHalfedge = StartNeighbours[(j - 1 + StartNeighbours.Length) % StartNeighbours.Length];
                        int StartOfPrevHalfedge = StartNeighbours[(j + 1) % StartNeighbours.Length];

                        int NextPairEdge = source.TopologyEdges.GetEdgeIndex(pMesh.Halfedges[i].StartVertex, EndOfNextOfPairHalfedge);
                        int PrevEdge = source.TopologyEdges.GetEdgeIndex(pMesh.Halfedges[i].StartVertex, StartOfPrevHalfedge);

                        if (source.TopologyEdges.GetTopologyVertices(NextPairEdge).I == pMesh.Halfedges[i].StartVertex) {
                            pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2;
                        } else {
                            pMesh.Halfedges[i + 1].NextHalfedge = NextPairEdge * 2 + 1;
                        }

                        if (source.TopologyEdges.GetTopologyVertices(PrevEdge).J == pMesh.Halfedges[i].StartVertex) {
                            pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2;
                        } else {
                            pMesh.Halfedges[i].PrevHalfedge = PrevEdge * 2 + 1;
                        }
                        break;
                    }
                }
            }

            return pMesh;
        }

        /// <summary>
        /// Creates a Rhino mesh from a Plankton halfedge mesh.
        /// Uses the face-vertex information available in the halfedge data structure.
        /// </summary>
        /// <returns>A <see cref="Mesh"/> which represents the source mesh (as best it can).</returns>
        /// <param name="source">A Plankton mesh to convert from.</param>
        /// <remarks>Any faces with five sides or more will be triangulated.</remarks>
        public static Mesh ToRhinoMesh(this PlanktonMesh source)
        {
            // could add different options for triangulating ngons later
            Mesh rMesh = new Mesh();
            foreach (PlanktonVertex v in source.Vertices)
            {
                rMesh.Vertices.Add(v.X, v.Y, v.Z);       
            }
            for (int i = 0; i < source.Faces.Count; i++)
            {
                int[] fvs = source.Faces.GetFaceVertices(i);
                if (fvs.Length == 3)
                {
                    rMesh.Faces.AddFace(fvs[0], fvs[1], fvs[2]);
                }
                else if (fvs.Length == 4)
                {
                    rMesh.Faces.AddFace(fvs[0], fvs[1], fvs[2], fvs[3]);
                }
                else if (fvs.Length > 4)
                {
                    // triangulate about face center (fan)
                    var fc = source.Faces.GetFaceCenter(i);
                    rMesh.Vertices.Add(fc.X, fc.Y, fc.Z);
                    for (int j = 0; j < fvs.Length; j++)
                    {
                        rMesh.Faces.AddFace(fvs[j], fvs[(j + 1) % fvs.Length], rMesh.Vertices.Count - 1);
                    }
                }            
            }
            return rMesh;
        }

        /// <summary>
        /// Converts each face to a closed polyline.
        /// </summary>
        /// <returns>A list of closed polylines representing the boundary edges of each face.</returns>
        /// <param name="source">A Plankton mesh.</param>
        public static Polyline[] ToPolylines(this PlanktonMesh source)
        {
            int n = source.Faces.Count;
            Polyline[] polylines = new Polyline[n];
            for (int i = 0; i < n; i++)
            {
                Polyline facePoly = new Polyline();
                int[] vs = source.Faces.GetFaceVertices(i);
                for (int j = 0; j <= vs.Length; j++)
                {
                    var v = source.Vertices[vs[j % vs.Length]];
                    facePoly.Add(v.X, v.Y, v.Z);
                }
                polylines[i] = facePoly;
            }
            
            return polylines;
        }
        
        /// <summary>
        /// Creates a Rhino Point3f from a Plankton vertex.
        /// </summary>
        /// <param name="vertex">A Plankton vertex</param>
        /// <returns>A Point3f with the same coordinates as the vertex.</returns>
        public static Point3f ToPoint3f(this PlanktonVertex vertex)
        {
            return new Point3f(vertex.X, vertex.Y, vertex.Z);
        }

        /// <summary>
        /// Creates a Rhino Point3d from a Plankton vertex.
        /// </summary>
        /// <param name="vertex">A Plankton vertex</param>
        /// <returns>A Point3d with the same coordinates as the vertex.</returns>
        public static Point3d ToPoint3d(this PlanktonVertex vertex)
        {
            return new Point3d(vertex.X, vertex.Y, vertex.Z);
        }
        
        /// <summary>
        /// Creates a Rhino Point3f from a Plankton vector.
        /// </summary>
        /// <param name="vector">A Plankton vector.</param>
        /// <returns>A Point3f with the same XYZ components as the vector.</returns>
        public static Point3f ToPoint3f(this PlanktonXYZ vector)
        {
            return new Point3f(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        /// Creates a Rhino Point3d from a Plankton vector.
        /// </summary>
        /// <param name="vector">A Plankton vector.</param>
        /// <returns>A Point3d with the same XYZ components as the vector.</returns>
        public static Point3d ToPoint3d(this PlanktonXYZ vector)
        {
            return new Point3d(vector.X, vector.Y, vector.Z);
        }
        
        /// <summary>
        /// Creates a Rhino Vector3f from a Plankton vector.
        /// </summary>
        /// <param name="vector">A Plankton vector.</param>
        /// <returns>A Vector3f with the same XYZ components as the vector.</returns>
        public static Vector3f ToVector3f(this PlanktonXYZ vector)
        {
            return new Vector3f(vector.X, vector.Y, vector.Z);
        }
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="index">Index of vertex to set.</param>
        /// <param name="vertex">Vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public static bool SetVertex(this PlanktonVertexList vertexList, int index, Point3f vertex)
        {
            return vertexList.SetVertex(index, vertex.X, vertex.Y, vertex.Z);
        }
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="index">Index of vertex to set.</param>
        /// <param name="vertex">Vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public static bool SetVertex(this PlanktonVertexList vertexList, int index, Point3d vertex)
        {
            return vertexList.SetVertex(index, vertex.X, vertex.Y, vertex.Z);
        }

        /// <summary>
        /// <para>Moves a vertex by a vector.</para>       
        /// </summary>
        /// <param name="index">Index of vertex to move.</param>
        /// <param name="vector">Vector to move by.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public static bool MoveVertex(this PlanktonVertexList vertexList, int index, Vector3d vector)
        {
            return vertexList.SetVertex(index, vertexList[index].X + vector.X, vertexList[index].Y + vector.Y, vertexList[index].Z + vector.Z);
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Location of new vertex.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public static int Add(this PlanktonVertexList vertexList, Point3f vertex)
        {
            return vertexList.Add(vertex.X, vertex.Y, vertex.Z);
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Location of new vertex.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public static int Add(this PlanktonVertexList vertexList, Point3d vertex)
        {
            return vertexList.Add(vertex.X, vertex.Y, vertex.Z);
        }
    }
}

