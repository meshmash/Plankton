using System;
using System.Linq;
using System.Collections.Generic;
using Plankton;
using PlanktonGh;
using Grasshopper.Kernel;
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
            pManager.AddSurfaceParameter("Surfaces", "Surfaces", "Surfaces as list", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            pManager.AddGenericParameter("PMesh", "PMesh", "PMesh", GH_ParamAccess.item);
            pManager.AddPointParameter("Vertex", "Vertex", "Vertex", GH_ParamAccess.item);
            pManager.AddVectorParameter("Normals", "Normals", "Normals", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            
            List<Surface> surfaces = new List<Surface>();
            DA.GetDataList<Surface>("Surfaces", surfaces);

            // surfaces to p-mesh
            Mesh msh = RhinoSupport.SrfToRhinoMesh(surfaces);

            PlanktonMesh singleFold = new PlanktonMesh(RhinoSupport.ToPlanktonMesh(msh));
           
            
            
            
            
            
            /*
            int maxValence = 0;
            for (int i = 0; i < singleFold.Vertices.Count; i++)
            {
                int iValence = singleFold.Vertices.GetValence(i);
                if (iValence > maxValence)
                {
                    maxValence = iValence;
                    singleFold.Vertices. = new Point3d(singleFold.PMesh.Vertices[i].X, singleFold.PMesh.Vertices[i].Y, singleFold.PMesh.Vertices[i].Z);
                }
            }
            */

            List<PlanktonXYZ> p_normals = singleFold.Vertices.GetNormals().ToList();
            List<Vector3f> normals = p_normals.Select(o => RhinoSupport.ToVector3f(o)).ToList();


            DA.SetData("Mesh", RhinoSupport.ToRhinoMesh(singleFold));
            DA.SetData("PMesh", singleFold);
            //DA.SetData("Vertex", singleFold.Vertex);
            DA.SetDataList("Normals", normals);

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
