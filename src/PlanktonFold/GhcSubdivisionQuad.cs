using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Plankton;
using PlanktonGh;

/// <summary>
/// current state:
/// subdivision based on how many subdivision loops to run
/// </summary>
namespace PlanktonFold
{
    public class GhcSubdivisionQuad : GH_Component
    {

        public GhcSubdivisionQuad()
          : base("GhcSubdivisionQuad", "Subdivision",
              "quad-subdivide a suface based on input point positions, so that the subdivided mesh has vertex on these points",
              "MT", "Sofistik")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.item); // for now a plankton mesh
            pManager.AddPointParameter("Fix Points", "Fix Points", "Fix Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Subdivide Count", "Subdivide Count", "Subdivide Count", GH_ParamAccess.item);

            //pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Max Length", "Max Length", "Max Length", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PlanktonMesh P = new PlanktonMesh();
            DA.GetData<PlanktonMesh>("Geometry", ref P);
            List<Point3d> fixPoints = new List<Point3d>();
            DA.GetDataList<Point3d>("Fix Points", fixPoints);
            double maxSubdivision = 0.0;
            DA.GetData<double>("Subdivide Count", ref maxSubdivision);

            // subdivide the mesh accorading to count
            int count = 0;
            do
            {
                P = RhinoSupport.QuadSubdivide(P);
                count += 1;

            } while (count < maxSubdivision); 

            // move
            RhinoSupport.MoveVertices(P, fixPoints);

            Mesh M = RhinoSupport.ToRhinoMesh(P);
            List<MeshFace> meshFaces = M.Faces.ToList();

            DA.SetData("Mesh", M);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7b41bdaf-162f-4549-ae17-799c1b1710ee}"); }
        }
    }
}