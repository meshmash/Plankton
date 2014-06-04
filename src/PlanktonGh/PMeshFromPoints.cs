using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Plankton;

namespace PlanktonGh
{

    public class PMeshFromPoints : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PMeshFromPoints()
            : base("PlanktonFromPoints", "PlanktonFromPoints",
                "Create a new Plankton mesh from an existing Plankton mesh and a list of points",
                "Mesh", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PMesh", "PMesh", "The input PlanktonMesh to use the topology from", GH_ParamAccess.item);
            pManager.AddPointParameter("Vertices", "Vertices", "The new list of vertex positions", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("PlanktonMesh", "P", "Plankton Mesh");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PlanktonMesh P = new PlanktonMesh();
            List<Point3d> Points = new List<Point3d>();
            if ((!DA.GetData<PlanktonMesh>(0, ref P)) || (!DA.GetDataList(1, Points))) return;
            PlanktonMesh pMesh = P.ReplaceVertices(Points);           
            DA.SetData(0, pMesh);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return PlanktonGh.Properties.Resources.plankton_verts;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{c9377989-c89e-477d-8dd2-b35af0c3b25d}"); }
        }
    }
}
