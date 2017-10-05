using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;

using Plankton;
using PlanktonFold;
using PlanktonGh;

using KangarooLib;
using KangarooSolver;

using MathNet.Numerics.LinearAlgebra;

namespace PlanktonFold
{
    public class PneumaticFold
    {
        public Mesh Pattern;
        public PlanktonMesh PMesh;

        // 几何
        public List<Point3d> ConstraintVertices;
        public List<List<Line>> FoldLines;
        public List<List<double>> SectorAngle;
        public List<List<double>> FoldAngle;

        // 
        public List<Matrix<double>> FMatrix; // should be identical matrix 


        public PneumaticFold(Mesh M) 
        {
            // plankton mesh prepared
            PMesh = RhinoSupport.ToPlanktonMesh(Pattern);
            PMesh.Faces.AssignFaceIndex();
            PMesh.Halfedges.AssignHalfEdgeIndex();
            PMesh.Vertices.AssignVertexIndex();

            // assign MV for each inner halfedge
            List<PlanktonHalfedge> innerEdges 
                = PMesh.Halfedges.ToList().Where(o => o.AdjacentFace != -1 && PMesh.Halfedges[PMesh.Halfedges.GetPairHalfedge(o.Index)].AdjacentFace != -1).ToList();

            foreach (PlanktonHalfedge e in innerEdges)
                e.MV = RhinoSupport.MVDetermination(PMesh, e.Index);

            // constraint vertices
            ConstraintVertices = RhinoSupport.GetConstraintVertices(PMesh);
            List<int> cVertexIndices = RhinoSupport.GetConstraintVertexIndices(PMesh);

            DataTree<Line> neighbourEdges = new DataTree<Line>();
            for (int j = 0; j < cVertexIndices.Count(); j++)
            {
                GH_Path jPth = new GH_Path(j);
                neighbourEdges.AddRange(RhinoSupport.NeighbourVertexEdges(PMesh, cVertexIndices[j])
                    .Select(o => RhinoSupport.HalfEdgeToLine(PMesh, o)).ToList(), jPth);
            }

        }




    }
}
