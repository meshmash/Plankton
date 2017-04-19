using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Plankton;
using PlanktonGh;

namespace PlanktonFold
{
    public class GhcSubdivisionQuad : GH_Component
    {

        public GhcSubdivisionQuad()
          : base("GhcSubdivisionQuad", "GhcSubdivisionQuad",
              "subdiveide a suface based on input points positions",
              "MT", "Sofistik")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Geometry", "Geometry", "Geometry", GH_ParamAccess.item); // for now a plankton mesh
            pManager.AddPointParameter("Fix Points", "Fix Points", "Fix Points", GH_ParamAccess.list);
            pManager.AddNumberParameter("Tolerance", "Tolerance", "Tolerance", GH_ParamAccess.item);
            pManager.AddNumberParameter("Max Length", "Max Length", "Max Length", GH_ParamAccess.item);

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
            double tolerance = 0.1;
            DA.GetData<double>("Tolerance", ref tolerance);
            double maxLength = 0.1;
            DA.GetData<double>("Max Length", ref maxLength);



            // check if the mesh is fine enough. If yes then move the close vertices to their pair targets, which are the fix points 
            List<int> closeVertexIDs = null;
            bool IsVertexCloseToFixPoints = RhinoSupport.CheckFixPointVertex(P, fixPoints, tolerance, out closeVertexIDs); // check if the there are vertex close enough to the fix points
            bool IsFineEnough = P.Halfedges.GetLengths().Max() < maxLength; // check if the mesh is fine enough in general


            if (IsFineEnough)
            {
                if (IsVertexCloseToFixPoints)
                {
                    // move
                    RhinoSupport.MoveVertices(P, fixPoints, closeVertexIDs);
                    
                }

                //else
                //{
                //    // divide region
                //    do
                //    {
                //        RhinoSupport.SelectedQuadSubdivide(P, fixPoints);

                //    } while (!RhinoSupport.CheckFixPointVertex(P, fixPoints, tolerance, out closeVertexIDs));

                //    // move
                //    RhinoSupport.MoveVertices(P, fixPoints, closeVertexIDs);

                //}


            }

            else
            {
                // divide all faces 
                do
                {
                    P = RhinoSupport.QuadSubdivide(P);

                } while (P.Halfedges.GetLengths().Max() > maxLength);

                if (RhinoSupport.CheckFixPointVertex(P, fixPoints, tolerance, out closeVertexIDs))
                {
                    // move
                    RhinoSupport.MoveVertices(P, fixPoints, closeVertexIDs);

                }

                //else
                //{
                //    // divide region
                //    do
                //    {
                //        RhinoSupport.SelectedQuadSubdivide(P, fixPoints);

                //    } while (!RhinoSupport.CheckFixPointVertex(P, fixPoints, tolerance, out closeVertexIDs));

                //    // move
                //    RhinoSupport.MoveVertices(P, fixPoints, closeVertexIDs);

                //}
            }


            DA.SetData("Mesh", RhinoSupport.ToRhinoMesh(P));
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