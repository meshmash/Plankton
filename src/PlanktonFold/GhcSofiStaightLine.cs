using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PlanktonFold
{
    public class GhcSofiStaightLine : GH_Component
    {

        public GhcSofiStaightLine()
          : base("GhcSofiStaightLine", "Sofistik streight line",
              "generate the geometry for straight line folding sofistik simulation",
              "MT", "Sofistik")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);

            // 1
            pManager.AddLineParameter("Fold Lines", "Fold Lines", "Fold Lines", GH_ParamAccess.list);

            // 2
            pManager.AddNumberParameter("MV Assignment", "MV Assignment", "MV Assignment", GH_ParamAccess.list);
            pManager[1].Optional = true;

            // 3
            pManager.AddNumberParameter("Actuation Assignment", "Actuation Assignment", "Actuation Assignment", GH_ParamAccess.list); // 1 as actuation, 0 as no actuation

            // 4
            pManager.AddNumberParameter("Maximum Edge Length", "Maximum Edge Length", "Maximum Edge Length", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Nodes", "Nodes", "Nodes", GH_ParamAccess.list);
            pManager.AddPointParameter("Pretension Cable Start", "Pretension Cable Start", "Pretension Cable Start", GH_ParamAccess.list);
            pManager.AddPointParameter("Pretension Cable End", "Pretension Cable End", "Pretension Cable End", GH_ParamAccess.list);
            pManager.AddPointParameter("Long Connection Start", "Long Connection Start", "Long Connection Start", GH_ParamAccess.list);
            pManager.AddPointParameter("Long Connection End", "Long Connection End", "Long Connection End", GH_ParamAccess.list);
            pManager.AddPointParameter("Short Connection Start", "Short Connection Start", "Short Connection Start", GH_ParamAccess.list);
            pManager.AddPointParameter("Short Connection End", "Short Connection End", "Short Connection End", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh M = new Mesh();
            DA.GetData<Mesh>("Mesh", ref M);

            List<Line> foldLines = new List<Line>();
            DA.GetDataList<Line>("Fold Lines", foldLines);

            List<double> mvAssignment = new List<double>();
            DA.GetDataList<double>("MV Assignment", mvAssignment);

            List<double> actuationAssignment = new List<double>();
            DA.GetDataList<double>("Actuation Assignment", actuationAssignment);

            double maxEdgeLength = 0.1;

            // remesh of the input Mesh
            Brep brepFromMesh = Brep.CreateFromMesh(M, false);
            MeshingParameters meshParam = new MeshingParameters();
            meshParam.MaximumEdgeLength = maxEdgeLength;
            Mesh.CreateFromBrep(brepFromMesh, meshParam);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return null;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("{4170ec9e-4508-43c7-802c-0b00b4d3969d}"); }
        }
    }
}