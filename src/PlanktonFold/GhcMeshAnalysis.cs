using System;
using System.Linq;
using System.Collections.Generic;
using Plankton;
using Grasshopper;
using PlanktonGh;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace PlanktonFold
{
    public class GhcMeshAnalysis : GH_Component
    {

        public GhcMeshAnalysis()
          : base("MeshAnalysis", "MeshAnalysis",
              "Analyse the input geometry, can be plankton mesh, rhino mesh, or a list of surfaces",
              "MT", "Analysis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0
            pManager.AddSurfaceParameter("Surfaces", "Surfaces", "Surfaces as list", GH_ParamAccess.list);
            pManager[0].Optional = true;

            // 1
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager[1].Optional = true;

            // 2
            pManager.AddGenericParameter("PlanktonMesh", "PlanktonMesh", "PlanktonMesh", GH_ParamAccess.item);
            pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);

            // 1
            pManager.AddPointParameter("cVertices", "cVertices", "cVertices", GH_ParamAccess.list); // inner vertices with constraints
           
            // 2
            pManager.AddGenericParameter("NeighborEdges",  "NeighborEdges", "NeighborEdges", GH_ParamAccess.tree);

            // 3 
            pManager.AddNumberParameter("Sector Angles", "Sector Angles", "theta", GH_ParamAccess.tree);

            // 4
            pManager.AddNumberParameter("Fold Angles", "Fold Angles", "rho", GH_ParamAccess.tree);

            // 5
            pManager.AddGenericParameter("F Matrix", "F Matrix", "F Matrix", GH_ParamAccess.list);

            // 6 
            pManager.AddPlaneParameter("Pln", "Pln", "Pln", GH_ParamAccess.tree);

            // 7 
            pManager.AddGenericParameter("PMesh", "PMesh", "PMesh", GH_ParamAccess.item);

            // 8 
            pManager.AddLineParameter("M", "M", "M", GH_ParamAccess.list);

            // 9 
            pManager.AddLineParameter("V", "V", "V", GH_ParamAccess.list);

        }

        Mesh M = new Mesh();
        PlanktonMesh P = new PlanktonMesh();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // define Mesh(M) & PlanktonMesh(P)
            List<Surface> surfaces = new List<Surface>();
            Mesh mesh = new Mesh();
            if (DA.GetDataList<Surface>("Surfaces", surfaces)) { M = RhinoSupport.SrfToRhinoMesh(surfaces); };
            if (DA.GetData<Mesh>("Mesh", ref mesh)) { M = mesh; };
            //if (DA.GetData<PlanktonMesh>("PlanktonMesh", ref )) { M = mesh; };
            P = RhinoSupport.ToPlanktonMesh(M);

            // assaign index to faces, half edges and vertices of the planktonmesh, so that it's easier to query from lists
            P.Faces.AssignFaceIndex();
            P.Halfedges.AssignHalfEdgeIndex();
            P.Vertices.AssignVertexIndex();

            // determine MV
            // select all inner edges of the mesh (boundary edges don't have MV properties)
            List<PlanktonHalfedge> innerEdges = P.Halfedges.ToList().Where(o => o.AdjacentFace != -1 &&
                P.Halfedges[P.Halfedges.GetPairHalfedge(o.Index)].AdjacentFace != -1
                ).ToList();
            foreach (PlanktonHalfedge e in innerEdges)
                e.MV = RhinoSupport.MVDetermination(P, e.Index);

            // get the inner vertices as index and point
            List<Point3d> cVertices = RhinoSupport.GetConstraintVertices(P);
            List<int> cVertexIndices = RhinoSupport.GetConstraintVertexIndices(P);

            // get the neighbour edges of all inner vertices in a datatree
            DataTree<Line> neighbourEdges = new DataTree<Line>();
            for (int j = 0; j < cVertexIndices.Count(); j++)
            {
                GH_Path jPth = new GH_Path(j);
                neighbourEdges.AddRange(RhinoSupport.NeighbourVertexEdges(P, cVertexIndices[j])
                    .Select(o => RhinoSupport.HalfEdgeToLine(P, o)).ToList(), jPth);
            }

            // get the sector angles of all inner vertices in a datatree
            DataTree<double> sectorAngles = new DataTree<double>();
            for (int j = 0; j < cVertexIndices.Count(); j++)
            {
                GH_Path jPth = new GH_Path(j);
                sectorAngles.AddRange(RhinoSupport.GetSectorAngles(P, cVertexIndices[j],
                    RhinoSupport.NeighbourVertexEdges(P, cVertexIndices[j]))
                    .ToList(), jPth);
            }
            
            // get the fold angles of all inner vertices in a datatree
            DataTree<double> foldAngles = new DataTree<double>();
            for (int j = 0; j < cVertexIndices.Count(); j++) // for the j th inner vertice
            {
                GH_Path jPth = new GH_Path(j);
                foldAngles.AddRange(RhinoSupport.GetFoldAngles(P, cVertexIndices[j],
                    RhinoSupport.NeighbourVertexEdges(P, cVertexIndices[j]))
                    .ToList(), jPth);
            }
            


            // compute F matrices for all inner vertices 
            // in the order of inner vertex, each one has a F matrix. A F matrix is a indentity matrix when this constraint is satisfied
            List<Matrix<double>> FMatrix = new List<Matrix<double>>(); 
            for (int j = 0; j < cVertexIndices.Count(); j++)
            {
                List<PlanktonHalfedge> edges = RhinoSupport.NeighbourVertexEdges(P, cVertexIndices[j]);
                List<double> rhos = RhinoSupport.GetFoldAngles(P, cVertexIndices[j], edges); // checked
                List<double> thetas = RhinoSupport.GetSectorAngles(P, cVertexIndices[j], edges); // checked
                FMatrix.Add(Solver.F(rhos, thetas));
            }
            
            // the coordinate system of all constraint vertices 
            DataTree<Plane> pln = new DataTree<Plane>();
            for (int i = 0; i < cVertexIndices.Count; i++)
            {
                int neighbourEdgeCount =
                    RhinoSupport.NeighbourVertexEdges(P, cVertexIndices[i]).Count;
                GH_Path iPth = new GH_Path(i);
                // xx pointing outwards along one foldline
                List<Vector3d> xx = neighbourEdges.Branch(iPth).Select(o => o.UnitTangent).ToList();
                // ff are the index of adjacent faces of a cVertice
                List<int> ff = RhinoSupport.NeighbourVertexEdges(P, P.Vertices[cVertexIndices[i]].Index).Select(o => o.AdjacentFace).ToList();
                // zz are the face normals 
                List<Vector3d> zz = ff.Select(o => RhinoSupport.GetFaceNormal(P, o).Last().UnitTangent).ToList();

                List<Plane> iPlanes = new List<Plane>();
                for (int j = 0; j < neighbourEdgeCount; j++)
                {
                    Plane jPln = new Plane(cVertices[i], xx[j], Vector3d.CrossProduct(zz[j], xx[j]));
                    iPlanes.Add(jPln);
                }
                pln.AddRange(iPlanes, iPth);

            }

            /*
            DataTree<double> foldAngles = new DataTree<double>();
            for (int j = 0; j < cVertexIndices.Count(); j++) // for the j th inner vertice
            {
                GH_Path jPth = new GH_Path(j);
                List<Plane> jVertexPlns = pln.Branches[j];

                List<double> ifoldAngles = new List<double>();

                for(int i = 0; i < jVertexPlns.Count; i++)
                {
                    if(i != jVertexPlns.Count - 1) // not the last plane
                        ifoldAngles.Add(Math.PI - Vector3d.VectorAngle(jVertexPlns[i].Normal, jVertexPlns[i+1].Normal));
                    else
                        ifoldAngles.Add(Math.PI - Vector3d.VectorAngle(jVertexPlns[i].Normal, jVertexPlns[0].Normal));

                }
                foldAngles.AddRange(ifoldAngles, jPth);
            }
            */
                
            DA.SetData("Mesh", RhinoSupport.ToRhinoMesh(P));
            DA.SetDataList("cVertices", cVertices);
            DA.SetDataTree(2, neighbourEdges);
            DA.SetDataTree(3, sectorAngles);
            DA.SetDataTree(4, foldAngles);
            DA.SetDataList("F Matrix", FMatrix);
            DA.SetDataTree(6, pln);
            DA.SetData(7, P);

            // M
            List<Line> mFoldLine = P.Halfedges.Where(o => o.MV == 1).ToList().Select(p => RhinoSupport.HalfEdgeToLine(P, p)).ToList();
            DA.SetDataList("M", mFoldLine);

            // V
            List<Line> vFoldLine = P.Halfedges.Where(o => o.MV == -1).ToList().Select(p => RhinoSupport.HalfEdgeToLine(P, p)).ToList();
            DA.SetDataList("V", vFoldLine);

            #region unused test

            #endregion
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Properties.Resources.mesh_analysis_Icon_06__06;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{ae648a75-b82f-4d4e-b7ca-1f06abe896e4}"); }
        }
    }
}
