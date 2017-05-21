using Plankton;
using System;
using System.Collections.Generic;

namespace PlanktonGeoTools
{
    public class PMeshCreation
    {
        public PMeshCreation() { }
        #region basic
        public PlanktonMesh TriangleMeshFromPoints(List<PlanktonXYZ> pl, int t1, int t2)
        {
            PlanktonMesh mesh = new PlanktonMesh();
            if (t1 < 1) return mesh;
            if (t2 <= t1) return mesh;
            int n = ((t1 + t2) * (t2 - t1 + 1)) / 2;
            if (n > pl.Count) return mesh;
            mesh.Vertices.AddVertices(pl);
            List<int> layer1; List<int> layer2 = new List<int>();
            for (int i = 0; i < t1; i++)
            {
                layer2.Add(i);
            }
            for (int i = t1 - 1; i < t2; i++)
            {
                layer1 = new List<int>(layer2);
                for (int j = 0; j < layer2.Count; j++)
                {
                    layer2[j] += i + 1;
                }
                layer2.Add(layer2[layer2.Count - 1] + 1);

                if (layer1.Count > 1)
                {
                    for (int j = 0; j < layer1.Count - 1; j++)
                    {
                        mesh.Faces.AddFace(layer1[j], layer1[j + 1], layer2[j + 1]);
                    }
                }
                for (int j = 0; j < layer1.Count; j++)
                {
                    mesh.Faces.AddFace(layer2[j], layer1[j], layer2[j + 1]);
                }
            }
            return mesh;
        }
        public PlanktonMesh TriangleMeshFromPoints(List<PlanktonXYZ> pl)
        {
            //triangle MeshTopo Points From topo of the pyramid to the base
            double t = pl.Count;
            double l = Math.Sqrt(t * 8 + 1) - 1;
            l /= 2;
            return TriangleMeshFromPoints(pl, (int)l);
        }
        public PlanktonMesh TriangleMeshFromPoints(List<PlanktonXYZ> pl, int t)
        {
            //triangle MeshTopo Points From topo of the pyramid to the base
            PlanktonMesh mesh = new PlanktonMesh();
            if (t < 2) return mesh;
            int n = ((1 + t) * t) / 2;
            if (n > pl.Count) return mesh;

            mesh.Vertices.AddVertices(pl);
            List<int> layer1; List<int> layer2 = new List<int>();
            layer2.Add(0);
            for (int i = 0; i < t - 1; i++)
            {
                layer1 = new List<int>(layer2);
                for (int j = 0; j < layer2.Count; j++)
                {
                    layer2[j] += i + 1;
                }
                layer2.Add(layer2[layer2.Count - 1] + 1);


                if (layer1.Count > 1)
                {
                    for (int j = 0; j < layer1.Count - 1; j++)
                    {
                        mesh.Faces.AddFace(layer1[j], layer1[j + 1], layer2[j + 1]);
                    }
                }
                for (int j = 0; j < layer1.Count; j++)
                {
                    mesh.Faces.AddFace(layer2[j], layer1[j], layer2[j + 1]);
                }
            }
            return mesh;
        }
        public PlanktonMesh MeshFromPoints(List<PlanktonXYZ> pl, int u, int v)
        {
            if (u * v > pl.Count || u < 2 || v < 2) return null;
            PlanktonMesh mesh = new PlanktonMesh();
            for (int i = 0; i < pl.Count; i++)
            {
                mesh.Vertices.Add(pl[i]);
            }
            for (int i = 1; i < u; i++)
            {
                for (int j = 1; j < v; j++)
                {
                    mesh.Faces.AddFace(
                    (j - 1) * u + i - 1,
                    (j - 1) * u + i,
                    (j) * u + i,
                    (j) * u + i - 1);
                }
            }
            return mesh;
        }
        public PlanktonMesh MeshFromPoints(PlanktonXYZ p1, PlanktonXYZ p2, PlanktonXYZ p3, PlanktonXYZ p4)
        {
            PlanktonMesh mesh = new PlanktonMesh();
            mesh.Vertices.Add(p1);
            mesh.Vertices.Add(p2);
            mesh.Vertices.Add(p3);
            mesh.Vertices.Add(p4);
            mesh.Faces.AddFace(0, 1, 2, 3);
            return mesh;
        }
        public PlanktonMesh MeshFromPoints(PlanktonXYZ p1, PlanktonXYZ p2, PlanktonXYZ p3)
        {
            PlanktonMesh mesh = new PlanktonMesh();
            mesh.Vertices.Add(p1);
            mesh.Vertices.Add(p2);
            mesh.Vertices.Add(p3);
            mesh.Faces.AddFace(0, 1, 2);
            return mesh;
        }
        #endregion
        #region ID
        public static List<string> PrintVertices(PlanktonMesh mesh)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                string str = "Vertices[" + i.ToString() + "]=";
                str += mesh.Vertices[i].X.ToString() +
                    "," + mesh.Vertices[i].Y.ToString() +
                    "," + mesh.Vertices[i].Z.ToString();
                output.Add(str);
            }
            return output;
        }
        public static List<string> PrintFaces(PlanktonMesh mesh)
        {
            List<string> output = new List<string>();
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                string str = "Faces[" + i.ToString() + "]=";
                int[] findex = mesh.Faces.GetFaceVertices(i);
                for (int j = 0; j < findex.Length; j++)
                {
                    if (j > 0) str += ",";
                    str += findex[j].ToString();
                }
                output.Add(str);
            }
            return output;
        }
        public static List<string> PrintHalfedges(PlanktonMesh mesh)
        {
            List<string> output = new List<string>();
            output.Add("Format: StartVertex,AdjacentFace,NextHalfedge,PrevHalfedge");
            for (int i = 0; i < mesh.Halfedges.Count; i++)
            {
                string str = "Halfedges[" + i.ToString() + "]=";
                str += mesh.Halfedges[i].StartVertex.ToString() + "," +
                     mesh.Halfedges[i].AdjacentFace.ToString() + "," +
                      mesh.Halfedges[i].NextHalfedge.ToString() + "," +
                       mesh.Halfedges[i].PrevHalfedge.ToString();
                output.Add(str);
            }
            return output;
        }
        #endregion

    }
}
