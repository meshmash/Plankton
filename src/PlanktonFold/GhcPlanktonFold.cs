using System;
using System.Linq;
using System.Collections.Generic;
using Plankton;
using Grasshopper;
using PlanktonGh;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace PlanktonFold
{
    public class GhcPlanktonFold : GH_Component
    {

        public GhcPlanktonFold()
          : base("PlanktonFold", "PlanktonFold",
              "folding simulation with plankton mesh structure",
              "MT", "PlanktonFold")
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
            pManager.AddNumberParameter("i", "i", "i", GH_ParamAccess.item, 0);
            pManager[2].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);

            // 1
            pManager.AddPointParameter("cVertices", "cVertices", "cVertices", GH_ParamAccess.list); // inner vertices with constraints
           
            // 2
            pManager.AddLineParameter("NeighborEdges",  "NeighborEdges", "NeighborEdges", GH_ParamAccess.tree);

            // 3 
            pManager.AddNumberParameter("Sector Angles", "Sector Angles", "Sector Angles", GH_ParamAccess.tree);

            // 4
            pManager.AddCurveParameter("PolyLine", "PolyLine", "PolyLine", GH_ParamAccess.list);

            // 5
            pManager.AddCurveParameter("Edges", "Edges", "Edges", GH_ParamAccess.list);

            //pManager.AddVectorParameter("vNormals", "vNormals", "vertex normals", GH_ParamAccess.list);
            //pManager.AddVectorParameter("fNormals", "fNormals", "face normals", GH_ParamAccess.list);
            //pManager.AddPointParameter("Vertices", "Vertices", "Vertices", GH_ParamAccess.list);
            //pManager.AddPointParameter("bVertices", "bVertices", "bVertices", GH_ParamAccess.list);
            //pManager.AddGenericParameter("PMesh", "PMesh", "PMesh", GH_ParamAccess.item);
            //pManager.AddLineParameter("bEdges", "bEdges", "boundary edges", GH_ParamAccess.list);

            pManager.AddLineParameter("i-1HalfEdge", "i-1HalfEdge", "i-1HalfEdge", GH_ParamAccess.item);

            pManager.AddLineParameter("iHalfEdge", "iHalfEdge", "iHalfEdge", GH_ParamAccess.item);

            pManager.AddLineParameter("i+1HalfEdge", "i+1HalfEdge", "i+1HalfEdge", GH_ParamAccess.item);

        }

        Mesh M = new Mesh();
        PlanktonMesh P = new PlanktonMesh();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // define M & P
            List<Surface> surfaces = new List<Surface>();
            if (DA.GetDataList<Surface>("Surfaces", surfaces)) { M = RhinoSupport.SrfToRhinoMesh(surfaces); };

            DA.GetData<Mesh>("Mesh", ref M);

            P = RhinoSupport.ToPlanktonMesh(M);
            P.Faces.AssignFaceIndex();
            P.Halfedges.AssignHalfEdgeIndex();
            P.Vertices.AssignVertexIndex();

            List<Point3d> cVertices = RhinoSupport.GetConstraintVertices(P);
            List<int> cVertexIndices = RhinoSupport.GetConstraintVertexIndices(P);

            double i = 0;
            DA.GetData<double>("i", ref i);

            DataTree<Line> neighbourEdges = new DataTree<Line>();

            for  (int j = 0; j < cVertexIndices.Count(); j++)
            {
                GH_Path iPth = new GH_Path(j);
                neighbourEdges.AddRange( RhinoSupport.NeighborVertexEdges(P, cVertexIndices[j]), iPth);
            }

            DataTree<double> sectorAngles = new DataTree<double>();

            DA.SetData("Mesh", RhinoSupport.ToRhinoMesh(P));
            DA.SetDataList("cVertices", cVertices);
            DA.SetDataTree(2, neighbourEdges);
            //DA.SetDataTree(3, );

            List<Polyline> polyLines = new List<Polyline>();
            polyLines = RhinoSupport.ToPolylines(P).ToList();
            DA.SetDataList("PolyLine", RhinoSupport.ToPolylines(P));

            DA.SetDataList("Edges", P.Halfedges.Select(o => RhinoSupport.HalfEdgeToLine(P, o)));

            #region unused test
            //// Normals
            //List<Point3d> p_vertices = P.Vertices.GetPositions().ToList()
            //    .Select(o => RhinoSupport.ToPoint3d(o)).ToList();
            //List<Vector3f> vertexNormals = P.Vertices.GetNormals().ToList()
            //    .Select(o => RhinoSupport.ToVector3f(o)).ToList();

            //List<Line> bEdges = RhinoSupport.GetBoundaryEdges(P);
            //List<Point3d> bVertices = RhinoSupport.GetBoundaryVertices(P);

            //DA.SetDataList("Vertices", p_vertices);
            //DA.SetDataList("vNormals", vertexNormals);
            //DA.SetDataList("bVertices", bVertices);
            //DA.SetData("PMesh", P); 
            //DA.SetDataList("bEdges", bEdges);
            i = (i > P.Halfedges.Count() - 1) ? P.Halfedges.Count() - 1 : i;
            Line prevLine = RhinoSupport.HalfEdgeToLine(P, P.Halfedges[(int)i].PrevHalfedge);
            Line iLine = RhinoSupport.HalfEdgeToLine(P, P.Halfedges[(int)i]);
            Line nextLine = RhinoSupport.HalfEdgeToLine(P, P.Halfedges[(int)(i)].NextHalfedge);
            DA.SetData("i-1HalfEdge", prevLine);
            DA.SetData("iHalfEdge", iLine);
            DA.SetData("i+1HalfEdge", nextLine);


            #endregion

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{ae648a75-b82f-4d4e-b7ca-1f06abe896e4}"); }
        }
    }
}
