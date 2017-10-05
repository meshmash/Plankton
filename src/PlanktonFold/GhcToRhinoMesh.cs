using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Plankton;
using PlanktonGh;
using PlanktonFold;


namespace PlanktonGh
{
    public class GhcToRhinoMesh : GH_Component
    {

        public GhcToRhinoMesh()
          : base("GhcToRhinoMesh", "GhcToRhinoMesh",
              "convert a PlanktonMesh to RhinoMesh",
              "MT", "Utility")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PlanktonMesh", "PlanktonMesh", "PlanktonMesh", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Rhino Mesh", "Rhino Mesh", "Rhino Mesh", GH_ParamAccess.item);

        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PlanktonMesh P = new PlanktonMesh();
            DA.GetData<PlanktonMesh>("PlanktonMesh", ref P);
            Mesh M = new Mesh();
            M = RhinoSupport.ToRhinoMesh(P);
            DA.SetData("Rhino Mesh", M);

        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return PlanktonFold.Properties.Resources.pmesh_to_mesh_07;
            }
        }

        /// <summary>   
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d8c26779-5a24-4047-bb74-e1d901c6a029}"); }
        }
    }
}