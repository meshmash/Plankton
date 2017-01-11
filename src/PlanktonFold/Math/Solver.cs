using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;



namespace PlanktonFold
{
    public static class Solver
    {
        /// <summary>
        /// fold line rotation matrix, input angle in radian
        /// </summary>
        /// <param name="rho"></param>
        /// <returns></returns>
        public static Matrix<double> C(double rho)
        {
            Matrix<double> cMatrix = DenseMatrix.OfArray(new double[,]
            {
                {1, 0, 0},
                {0, Trig.Cos(rho), - Trig.Sin(rho)},
                {0, Trig.Sin(rho), Trig.Cos(rho)}
            });
            return cMatrix;
        }

        /// <summary>
        /// sector angle matrix, input angle in radian
        /// </summary>
        /// <param name="theta"></param>
        /// <returns></returns>
        public static Matrix<double> B(double theta)
        {
            Matrix<double> bMatrix = DenseMatrix.OfArray(new double[,]
            {
                {Trig.Cos(theta), - Trig.Sin(theta), 0},
                {Trig.Sin(theta), Trig.Cos(theta), 0},
                {0, 0, 1}

            });
            return bMatrix;
        }

        /// <summary>
        /// rotation matrix for a frame
        /// </summary>
        /// <param name="c"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Matrix<double> Chi(Matrix<double> c, Matrix<double> b)
        {
            Matrix<double> chi = c.Multiply(b);
            return chi;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rhos"></param>
        /// <param name="thetas"></param>
        /// <returns></returns>
        public static Matrix<double> F(List<double> rhos, List<double> thetas)
        {
            var M = Matrix<double>.Build;
            Matrix<double> F = M.DenseIdentity(3);

            for (int i = 0; i < rhos.Count(); i++) { F = F.Multiply( Chi( C(rhos[i]), B(thetas[i]) )); };
                
            return F;
        }

        public static List<Vector3d> GetVectors(Plane p, List<double> rhos, List<double> thetas)
        {
            List<Vector3d> vectors = new List<Vector3d>();

            var M = Matrix<double>.Build;
            Matrix<double> F = M.DenseIdentity(3);

            for (int i = 0; i < rhos.Count(); i++)
            {
                p.Transform(Transform.Rotation(rhos[i], p.XAxis, p.Origin));

                F = F.Multiply(Chi(C(rhos[i]), B(thetas[i])));
            }

            return vectors;
        }
    }
}
