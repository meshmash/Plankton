using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Plankton;

namespace PlanktonGh
{
 
    public class GHMeshToPMesh : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GHMeshToPMesh()
            : base("CreatePlankton", "CreatePlankton",
                "Create a new Plankton halfedge mesh from a Grasshopper Mesh",
                "Mesh", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {            
            pManager.AddMeshParameter("Mesh", "M", "Input Mesh",GH_ParamAccess.item);   
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
            Mesh M = new Mesh();          
            if (!DA.GetData(0, ref M)) return;

            PlanktonMesh pMesh = new PlanktonMesh(M);

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
                return PlanktonGh.Properties.Resources.plankton;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{047097d1-83bd-4f76-9994-dff721c24047}"); }
        }
    }
}
