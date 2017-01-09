using Plankton;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanktonGh
{
    /// <summary>
    /// Provides static and extension methods to add support for Rhino geometry in <see cref="Plankton"/>.
    /// </summary>
    static public class RhinoSupport
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
                        pMesh.Vertices[HalfB.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count + 1;
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
                        pMesh.Vertices[HalfA.StartVertex].OutgoingHalfedge = pMesh.Halfedges.Count;
                    }
                }
                pMesh.Halfedges.Add(HalfA);
                pMesh.Halfedges.Add(HalfB);
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
             
            pMesh.Halfedges.AssignHalfEdgeIndex(); // by dyliu
                  
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
            rMesh.Normals.ComputeNormals();
            return rMesh;
        }

        // !!!
        /// <summary>
        /// Replaces the vertices of a PlanktonMesh with a new list of points
        /// </summary>
        /// <returns>A list of closed polylines representing the boundary edges of each face.</returns>
        /// <param name="source">A Plankton mesh.</param>
        /// <param name="points">A list of points.</param>
        public static PlanktonMesh ReplaceVertices(this PlanktonMesh source, List<Point3d> points)
        {            
            PlanktonMesh pMesh = source;
            for (int i = 0; i < points.Count; i++)
            {
                pMesh.Vertices.SetVertex(i, points[i]);
            }
            return pMesh;
        }

        // !!!
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

        // !!!
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

        /// <summary>
        /// Gets positions of vertices
        /// </summary>
        /// <returns>A list of Point3d</returns>
        /// <param name="source">A Plankton mesh.</param>
        public static IEnumerable<Point3d> GetPositions(this PlanktonMesh source)
        {
            return Enumerable.Range(0, source.Vertices.Count).Select(i => source.Vertices[i].ToPoint3d());          
        }

        #region by dyliu

        /// <summary>
        /// Gets area of a planar quad
        /// </summary>
        /// <param name="srf"></param>
        /// <returns></returns>
        public static double QuadArea(Surface srf)
        {
            double area = 0;
            double width;
            double height;

            if (srf.GetSurfaceSize(out width, out height))
            {
                area = width * height;
            }

            return area;
        }

        /// <summary>
        /// Gets the area of a triangle
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        public static double TriangleArea(Point3d A, Point3d B, Point3d C)
        {
            double area;
            return area = Math.Abs((A.X * (B.Y - C.Y) + B.X * (A.Y - C.Y) + C.X * (A.Y - B.Y)) / 2);
        }

        /// <summary>
        /// Constructs a rhino mesh from a list of srfs
        /// </summary>
        /// <param name="srfs"></param>
        /// <returns></returns>
        public static Mesh SrfToRhinoMesh(List<Surface> srfs)
        {
            Mesh msh = new Mesh();

            int vertexCounter = 0;
            foreach (Surface srf in srfs)
            {
                srf.SetDomain(0, new Interval(0, 1));
                srf.SetDomain(1, new Interval(0, 1));

                Point3d cornerA = srf.PointAt(0, 0);
                Point3d cornerB = srf.PointAt(0, 1);
                Point3d cornerC = srf.PointAt(1, 0);
                Point3d cornerD = srf.PointAt(1, 1);

                if (RhinoSupport.QuadArea(srf) == RhinoSupport.TriangleArea(cornerA, cornerB, cornerC)) // triangle mesh face
                {
                    msh.Vertices.Add(cornerA);
                    msh.Vertices.Add(cornerB);
                    msh.Vertices.Add(cornerC);
                    msh.Faces.AddFace(vertexCounter, vertexCounter + 1, vertexCounter + 2);
                    vertexCounter += 3;
                }

                else // quad mesh face
                {
                    msh.Vertices.Add(cornerA);
                    msh.Vertices.Add(cornerB);
                    msh.Vertices.Add(cornerD);
                    msh.Vertices.Add(cornerC);
                    msh.Faces.AddFace(vertexCounter, vertexCounter + 1, vertexCounter + 2, vertexCounter + 3);
                    vertexCounter += 4;
                }
            }

            return msh;
        }

        /// <summary>
        /// get the boundary endges as a list of lines
        /// </summary>
        /// <param name="pmsh"></param>
        /// <returns></returns>
        public static List<Line> GetBoundaryEdges(PlanktonMesh pmsh)
        {
            List<Line> bEdges = new List<Line>();

            List<int> nakedEdgeIndex = pmsh.Halfedges.Where(o => o.AdjacentFace == -1).Select(o => o.Index).ToList();
            
            foreach (int i in nakedEdgeIndex)
            {
                int[] ends = pmsh.Halfedges.GetVertices(i);

                Point3d p1 = pmsh.Vertices[ends.First()].ToPoint3d();
                Point3d p2 = pmsh.Vertices[ends.Last()].ToPoint3d();
                bEdges.Add(new Line(p1, p2));
            }

            return bEdges;


        }

        /// <summary>
        ///  get boundary vertices as a list
        /// </summary>
        /// <param name="pmsh"></param>
        /// <returns></returns>
        public static List<Point3d> GetBoundaryVertices(PlanktonMesh pmsh)
        {
            List<int> bVerticesID = new List<int>();
            List<int> nakedEdgeIndex = pmsh.Halfedges.Where(o => o.AdjacentFace == -1).Select(o => o.Index).ToList();

            foreach (int i in nakedEdgeIndex)
            {
                int[] ends = pmsh.Halfedges.GetVertices(i);
                bVerticesID.AddRange(ends);
            }

            bVerticesID.Distinct().ToList().Sort();

            List<PlanktonVertex> bVertices = new List<PlanktonVertex>();
            foreach (int i in bVerticesID)
                bVertices.Add(pmsh.Vertices[i]);

            return bVertices.Select(o => o.ToPoint3d()).ToList();
        }

        /// <summary>
        /// get the inner/constraint vertices
        /// </summary>
        /// <param name="pmsh"></param>
        /// <returns></returns>
        public static List<Point3d> GetConstraintVertices(PlanktonMesh pmsh)
        {

            List<int> bVerticesID = new List<int>();
            List<int> nakedEdgeIndex = pmsh.Halfedges.Where(o => o.AdjacentFace == -1).Select(o => o.Index).ToList();

            foreach (int i in nakedEdgeIndex)
            {
                int[] ends = pmsh.Halfedges.GetVertices(i);
                bVerticesID.AddRange(ends);
            }

            bVerticesID.Distinct().ToList().Sort();

            List<PlanktonVertex> bVertices = new List<PlanktonVertex>();
            foreach (int i in bVerticesID)
                bVertices.Add(pmsh.Vertices[i]);

            List<PlanktonVertex> cVertices = pmsh.Vertices.ToList().Except(bVertices).ToList();
            return cVertices.Select(o => o.ToPoint3d()).ToList();
        }

        public static List<int> GetConstraintVertexIndices(PlanktonMesh pmsh)
        {

            List<int> bVerticesID = new List<int>();
            List<int> nakedEdgeIndex = pmsh.Halfedges.Where(o => o.AdjacentFace == -1).Select(o => o.Index).ToList();

            foreach (int i in nakedEdgeIndex)
            {
                int[] ends = pmsh.Halfedges.GetVertices(i);
                bVerticesID.AddRange(ends);
            }

            bVerticesID.Distinct().ToList().Sort();

            List<PlanktonVertex> bVertices = new List<PlanktonVertex>();
            foreach (int i in bVerticesID)
                bVertices.Add(pmsh.Vertices[i]);

            List<PlanktonVertex> cVertices = pmsh.Vertices.ToList().Except(bVertices).ToList();
            return cVertices.Select(o => o.Index).ToList();
        }

        /// <summary>
        /// get the neighbour edges of a vertex
        /// </summary>
        /// <param name="pmsh"></param>
        /// <param name="vIndex"></param>
        /// <returns></returns>
        public static List<PlanktonHalfedge> NeighbourVertexEdges(PlanktonMesh pmsh, int vIndex)
        {
            // index
            List<int> neighborEdgesIndices =    
                pmsh.Halfedges.GetVertexCirculator( 
                    pmsh.Vertices[vIndex].OutgoingHalfedge) 
                    .ToList();

            // plankton edges
            List<PlanktonHalfedge> neighborPEdges = 
                pmsh.Halfedges.ToList()
                .Where(o => neighborEdgesIndices.Contains(o.Index))
                .ToList();

            // sort counterclockwise
            List<PlanktonHalfedge> sortedNeighborPEdges = new List<PlanktonHalfedge>();

            sortedNeighborPEdges = NeighbourSortingHelper(pmsh, vIndex, neighborPEdges);

            return sortedNeighborPEdges;
        }

        public static List<PlanktonHalfedge> NeighbourSortingHelper(PlanktonMesh pmsh, int vIndex, List<PlanktonHalfedge> neighbourPEdges)
        {
            // halfedge to line
            List<Line> neighbourLines = new List<Line>();
            foreach (var e in neighbourPEdges)
            {
                neighbourLines.Add(RhinoSupport.HalfEdgeToLine(pmsh, e));
            }

            // construct ref plane
            Point3d origin = pmsh.Vertices[vIndex].ToPoint3d();
            Vector3d v = pmsh.Vertices.GetNormal(vIndex).ToVector3f();
            Plane refPlane = new Plane(origin, v);

            // project the other end point of neighbour edges to the plane
            neighbourLines.ForEach(o => o.Transform(Transform.PlanarProjection(refPlane)));

            // look for the other end of the line other than the center vertex
            for (int i = 0; i < neighbourLines.Count(); i++)
            {
                if (neighbourLines[i].PointAt(0).DistanceTo(origin) < neighbourLines[i].Length / 10000000)
                    ;
                else
                    neighbourLines[i].Flip();
            }

            Vector3d refPlaneX = refPlane.XAxis;
            Vector3d refPlaneY = refPlane.YAxis;
            List<Vector3d> unitV = neighbourLines.Select(o => o.UnitTangent).ToList();
            List<double> anglesToX = unitV.Select(o => Vector3d.VectorAngle(refPlaneX, o)).ToList();
            List<double> anglesToY = unitV.Select(o => Vector3d.VectorAngle(refPlaneY, o)).ToList();

            for (int i = 0; i < neighbourPEdges.Count(); i++)
            {
                neighbourPEdges[i].angleToX = anglesToX[i];
                neighbourPEdges[i].angleToY = anglesToY[i];
            }

            // sort by angles to x axis
            //Array.Sort(anglesToX.ToArray(), neighborPEdges.ToArray());

            List<PlanktonHalfedge> G1 = neighbourPEdges.Where(o => o.angleToX <= Math.PI / 2 && o.angleToY <= Math.PI / 2).ToList();
            List<PlanktonHalfedge> G2 = neighbourPEdges.Where(o => o.angleToX > Math.PI / 2 && o.angleToY < Math.PI / 2).ToList();
            List<PlanktonHalfedge> G3 = neighbourPEdges.Where(o => o.angleToX >= Math.PI / 2 && o.angleToY >= Math.PI / 2).ToList();
            List<PlanktonHalfedge> G4 = neighbourPEdges.Where(o => o.angleToX < Math.PI / 2 && o.angleToY > Math.PI / 2).ToList();

            G1 = G1.OrderBy(o => o.angleToX).ToList();
            G2 = G2.OrderBy(o => o.angleToX).ToList();
            G3 = G3.OrderByDescending(o => o.angleToX).ToList();
            G4 = G4.OrderByDescending(o => o.angleToX).ToList();

            return G1.Concat(G2).Concat(G3).Concat(G4)
                .ToList();
        }

        /// <summary>
        /// given pmesh and edge, gives a line of the edge
        /// </summary>
        /// <param name="pmsh"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public static Line HalfEdgeToLine(PlanktonMesh pmsh, PlanktonHalfedge e)
        {
            Point3d p1 = pmsh.Vertices[e.StartVertex].ToPoint3d();

            Point3d p2 = pmsh.Vertices[pmsh.Halfedges[pmsh.Halfedges.GetPairHalfedge(e.Index)].StartVertex].ToPoint3d();

            return new Line(p1, p2);
        }

        public static Line HalfEdgeToLine(PlanktonMesh pmsh, int e)
        {
            Point3d p1 = pmsh.Vertices[pmsh.Halfedges[e].StartVertex].ToPoint3d();

            Point3d p2 = pmsh.Vertices[pmsh.Halfedges[pmsh.Halfedges.GetPairHalfedge(e)].StartVertex].ToPoint3d();

            return new Line(p1, p2);
        }


        #endregion
    }
}

