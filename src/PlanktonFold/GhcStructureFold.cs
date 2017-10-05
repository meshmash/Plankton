using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using PlanktonFold;
using Plankton;
using PlanktonGh;
// matrix
using MathNet.Numerics.LinearAlgebra;

namespace PlanktonFold
{
    public class GhcStructureFold : GH_Component
    {

        public GhcStructureFold()
          : base("GhcStructureFold", "GhcStructureFold",
              "mode analysise",
              "MT", "Analysis")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 0
            pManager.AddNumberParameter("kFold", "kFold", "kFold", GH_ParamAccess.item);
            pManager[0].Optional = true;

            // 1
            pManager.AddNumberParameter("kFace", "kFace", "kFace", GH_ParamAccess.item);
            pManager[1].Optional = true;

            // 2
            pManager.AddMeshParameter("triangulatedMesh", "triangulatedMesh", "triangulatedMesh", GH_ParamAccess.item);
            pManager[1].Optional = true;

            // 3 
            pManager.AddMeshParameter("originalMesh", "originalMesh", "originalMesh", GH_ParamAccess.item);


      

        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // 0
            pManager.AddMeshParameter("MorphingMesh", "MorphingMesh", "MorphingMesh", GH_ParamAccess.list);

            // 1
            pManager.AddGenericParameter("T Matrix", "T Matrix", "T Matrix", GH_ParamAccess.item);

        }

        Mesh triM = new Mesh();
        Mesh quadM = new Mesh();



        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // triangulated mesh
            Mesh tridMesh = new Mesh();
            if (DA.GetData<Mesh>("triangulatedMesh", ref tridMesh)) { triM = tridMesh; };

            // original quad mesh
            Mesh quadMesh = new Mesh();
            if (DA.GetData<Mesh>("originalMesh", ref quadMesh)) { quadM = quadMesh; };

            int bFold = quadM.TopologyEdges.Count; // fold bar
            int bAll = triM.TopologyEdges.Count;
            int bFace = bAll - bFold; // face bar

            // B = b + bb
            int n = triM.Vertices.Count;

            #region math

            /*
            Reference: Schenk and Guest 2010, Origami Folding: A Structural Engineering Approach
            
            equations:
            EQUIL: 
                  A            t   =    f
                [3n*b_all] [b_all*1]  [3n*1] 
            COM:
                  C           d    =    e
                [b_all*3n] [3n*1]  [b_all*1]
            MAT:
                 G_fold           e_fold    =   t_fold 
                 [bFold*bFold]   [bFold*1]     [bFold*1]
                 G_face     e_face    =    dtheta 
                 [G_face*G_face]    [G_face*1]     [G_face*1]
            
             what is J
             J = dF = dFdx*dx + dFdy*dy + dFdz*dz = dFdtheta* dtheta
            
             what is F
             F is additional constraints, F = sin(theta) = sin( theta(x,y,z) ), thus dFdx, dFdy, dFdz is solvable. 
             

             K =   C^T         G         C    +   
                [3n*bAll] [bAll*bAll] [bAll*3n]    
                   Ja^T         G_Ja        Ja   + 
                [3n*bFace] [bFace*bFace] [bFace*3n]
                   Jo^T   G_Jo    Jo   
                [3n*bFold] [bFold*bFold] [bFold*3n]
               
                
            */
            #endregion

            var doubleMatrix = Matrix<double>.Build;
            
            Matrix<double> K = doubleMatrix.DenseIdentity(3*n); // global stiffness matrix 
            // 3n * 3n

            #region axial
            Matrix<double> C = doubleMatrix.DenseIdentity(bAll, 3*n);
            // bFold * 3n

            Matrix<double> G = doubleMatrix.DenseIdentity(bAll, bAll);
            // bFold * bFold 

            // for a 3d bar with 6 DOF
            // global coordinate: xyz, local coordinate: x_hat, y_hat, z_hat

            // local K
            Matrix<double> K_hat;
            double[,] _K_hat =
            {
                {1,0,0,-1,0,0 },
                {0,0,0,0,0,0, },
                {-1,0,0,1,0,0 }
            };
            K_hat = doubleMatrix.SparseOfArray(_K_hat);


            // global coordinate
            Vector3d worldX = new Vector3d(1, 0, 0);
            Vector3d worldY = new Vector3d(0, 1, 0);
            Vector3d worldZ = new Vector3d(0, 0, 1);
            Plane worldCoor = new Plane(Point3d.Origin, worldX, worldY);

            // 
            List<Matrix<double>> globalAxialKes = new List<Matrix<double>>();
            for ( int i = 0; i < triM.TopologyEdges.Count; i++)
            {
                Line iBar = triM.TopologyEdges.EdgeLine(i);
                Matrix<double> iT = doubleMatrix.DenseOfArray(RhinoSupport.getTranforamtionArray(iBar, worldCoor));
                globalAxialKes.Add(iT);

            }

            // 3n * 3n
            Matrix<double> globalAxialK = doubleMatrix.Dense(triM.Vertices.Count * 3, triM.Vertices.Count * 3);

            // loop bars
            for (int i = 0; i < triM.TopologyEdges.Count; i++)
            {
                int startNode = triM.TopologyEdges.GetTopologyVertices(i).I;
                int endNode = triM.TopologyEdges.GetTopologyVertices(i).J;

                // element K of ith bar
                Matrix<double> iGlobalAxialKe = globalAxialKes[i]; // 6*6

                // extract element K of ith bar from global K
                Matrix<double> II_subM = globalAxialK.SubMatrix(startNode * 3, 3, startNode * 3, 3); // 3*3
                Matrix<double> II_subM_ = II_subM.Add(iGlobalAxialKe.SubMatrix(0, 3, 0, 3));
                globalAxialK.SetSubMatrix(startNode * 3, startNode * 3, II_subM_);

                Matrix<double> IJ_subM = globalAxialK.SubMatrix(startNode * 3, 3, endNode * 3, 3); // 3*3
                Matrix<double> IJ_subM_ = IJ_subM.Add(iGlobalAxialKe.SubMatrix(0, 3, 3, 3));
                globalAxialK.SetSubMatrix(startNode * 3, endNode * 3, IJ_subM_);

                Matrix<double> JI_subM = globalAxialK.SubMatrix(endNode * 3, 3, startNode * 3, 3); // 3*3
                Matrix<double> JI_subM_ = JI_subM.Add(iGlobalAxialKe.SubMatrix(3, 3, 0, 3));
                globalAxialK.SetSubMatrix(endNode * 3, startNode * 3, JI_subM_);

                Matrix<double> JJ_subM = globalAxialK.SubMatrix(endNode * 3, 3, endNode * 3, 3); // 3*3
                Matrix<double> JJ_subM_ = JJ_subM.Add(iGlobalAxialKe.SubMatrix(3, 3, 3, 3));
                globalAxialK.SetSubMatrix(endNode * 3, endNode * 3, JJ_subM_);

            }
            #endregion

            #region bending face
            Matrix <double> Ja = doubleMatrix.DenseIdentity(bFace, 3*n);
            // bFace * 3n

            Matrix<double> G_Ja = doubleMatrix.DenseIdentity(bFace, bFace);
            // bFace * bFace
            #endregion

            #region bending fold
            Matrix<double> Jo = doubleMatrix.DenseIdentity(bFold, 3 * n);
            // bFace * 3n

            Matrix<double> G_Jo = doubleMatrix.DenseIdentity(bFold, bFold);
            // bFace * bFace
            #endregion

            DA.SetData("T Matrix", globalAxialK);
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


        public override Guid ComponentGuid
        {
            get { return new Guid("7b56dc34-6bf0-4f0e-ac4f-42e11d62ebe9"); }
        }
    }
}