using System;
using System.Collections.Generic;
using Plankton;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PlanktonFold
{
    public class PlanktonFoldComponent : GH_Component
    {

        public PlanktonFoldComponent()
          : base("PlanktonFold", "PlanktonFold",
              "folding simulation with plankton mesh structure",
              "MT", "PlanktonFold")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
