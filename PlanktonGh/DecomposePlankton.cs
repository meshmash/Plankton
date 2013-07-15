using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Plankton;

namespace PlanktonGh
{
    public class DecomposePlankton : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DecomposePlankton class.
        /// </summary>
        public DecomposePlankton()
            : base("DeconstructPlankton", "DeconstructPlankton",
                "Decompose a plankton mesh into its topology information",
                "Mesh", "Triangulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("PMesh", "PMesh", "The input PlanktonMesh to decompose", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Vertex_Points", "V", "Vertex point positions", GH_ParamAccess.list);
            pManager.Register_IntegerParam("Vertex_Outgoing_Halfedge", "V_He", "One of the outgoing halfedges for each vertex", GH_ParamAccess.list);
            pManager.Register_IntegerParam("HalfEdge_StartVertex", "He_V", "The starting vertex of each halfedge", GH_ParamAccess.list);
            pManager.Register_IntegerParam("HalfEdge_AdjacentFace", "He_F", "The face bordered by each halfedge (or -1 if it is adjacent to a boundary)", GH_ParamAccess.list);
            pManager.Register_IntegerParam("HalfEdge_NextHalfEdge", "He_Nxt", "The next halfedge around the same face", GH_ParamAccess.list);
            pManager.Register_IntegerParam("HalfEdge_PrevHalfEdge", "He_Prv", "The previous halfedge around the same face", GH_ParamAccess.list);
            pManager.Register_IntegerParam("HalfEdge_Pair", "He_P", "The halfedge joining the same 2 vertices in the opposite direction", GH_ParamAccess.list);
            pManager.Register_IntegerParam("Face_HalfEdge", "F_He", "The first halfedge of each face", GH_ParamAccess.list);              
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            P_mesh P = new P_mesh();
            if (!DA.GetData<P_mesh>(0, ref P)) return;

            List<Point3d> Positions = new List<Point3d>();
            List<int> OutHEdge = new List<int>();

            for (int i = 0; i < P.Vertices.Count; i++)
            {
                Positions.Add(P.Vertices[i].Position);
                OutHEdge.Add(P.Vertices[i].OutgoingHalfEdge);
            }
           
            List<int> StartV = new List<int>();
            List<int> AdjF = new List<int>();
            List<int> Next = new List<int>();
            List<int> Prev = new List<int>();
            List<int> Pair = new List<int>();

            for (int i = 0; i < P.HalfEdges.Count; i++)
            {
                StartV.Add(P.HalfEdges[i].StartVertex);
                AdjF.Add(P.HalfEdges[i].AdjacentFace);
                Next.Add(P.HalfEdges[i].NextHalfEdge);
                Prev.Add(P.HalfEdges[i].PrevHalfEdge);
                Pair.Add(P.PairHalfEdge(i));
            }
     
            List<int> FaceEdge = new List<int>();
            for (int i = 0; i < P.Faces.Count; i++)
            {
                FaceEdge.Add(P.Faces[i].FirstHalfEdge);
            }

            DA.SetDataList(0, Positions);
            DA.SetDataList(1, OutHEdge);

            DA.SetDataList(2, StartV);
            DA.SetDataList(3, AdjF);
            DA.SetDataList(4, Next);
            DA.SetDataList(5, Prev);
            DA.SetDataList(6, Pair);

            DA.SetDataList(7, FaceEdge);    

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;                
                return PlanktonGh.Properties.Resources.plankton_decon;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{97c28a7c-5d5a-4b3d-a935-b8730e88749b}"); }
        }
    }
}