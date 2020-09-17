using System;

namespace Plankton
{
    // This classes add functionality to solve linear systems of equations and eigenvalue problems

    #region Vector
    /// <summary>
    /// Defines a vector of real values.
    /// </summary>
    public struct VectorR
    {
        #region Members
        private int size;
        public double[] vector;
        #endregion

        #region Constructors
        public VectorR(int size)
        {
            this.size = size;
            this.vector = new double[size];
            for(int i = 0; i < size; i++)
            {
                vector[i] = 0.0f;
            }
        }

        public VectorR(double[] vector)
        {
            this.size = vector.Length;
            this.vector = vector;
        }
        #endregion

        #region Properties
        public int GetSize()
        {
            return size;
        }

        /// <summary>
        /// Indexing property of the vector.
        /// </summary>
        /// <param name="n">The index of the vector.</param>
        public double this[int n]
        {
            get
            {
            if (n < 0 || n > size)
                {
                    throw new ArgumentOutOfRangeException("n", n, "n is out of range!");
                }
                return vector[n];
            }
            set { vector[n] = value; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            string s = "(";
            for (int i = 0; i < size - 1; i++)
            {
                s += vector[i].ToString() + ", ";
            }
            s += vector[size - 1].ToString() + ")";
            return s;
        }

        /// <summary>
        /// Swaps two elements of a vector.
        /// </summary>
        /// <param name="m">The first element to be swapped.</param>
        /// <param name="n">The second element to be swapped.</param>
        /// <returns>The vector with swapped elements.</returns>
        public VectorR GetSwap(int m, int n)
        {
            double temp = vector[m];
            vector[m] = vector[n];
            vector[n] = temp;
            return new VectorR(vector);
        }

        /// <summary>
        /// Computes the dot product (or scalar product, or interior product) of two vectors.
        /// <para>This operation is commutative.</para>
        /// </summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>A scalar representing the dot product of the two vectors.</returns>
        public static double DotProduct(VectorR v1, VectorR v2)
        {
            double result = 0.0;
            for (int i = 0; i < v1.size; i++)
            {
                result += v1[i] * v2[i];
            }
            return result;
        }
        #endregion
    }
    #endregion

    #region Matrix
    /// <summary>
    /// Defines a matrix of real values.
    /// </summary>
    public struct MatrixR
    {
        #region Members

        private int Rows;
        private int Cols;
        private double[,] matrix;

        #endregion

        #region Constructors
        public MatrixR(int rows, int cols)
        {
            this.Rows = rows;
            this.Cols = cols;
            this.matrix = new double[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = 0.0;
                }
            }
        }

        public MatrixR(double[,] matrix)
        {
            this.Rows = matrix.GetLength(0);
            this.Cols = matrix.GetLength(1);
            this.matrix = matrix;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Indexing property of the matrix.
        /// </summary>
        /// <param name="m">The row of the matrix.</param>
        /// <param name="n">The column of the matrix.</param>
        public double this[int m, int n]
        {
            get
            {
                if (m < 0 || m > Rows)
                {
                    throw new ArgumentOutOfRangeException("m", m, "m is out of range!");
                }
                if (n < 0 || n > Cols)
                {
                    throw new ArgumentOutOfRangeException("n", n, "n is out of range!");
                }
                return matrix[m, n];
            }
            set { matrix[m, n] = value; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            string ss = "(";
            for (int i = 0; i < Rows; i++)
            {
                string s = "";
                for(int j = 0; j < Cols - 1; j++)
                {
                    s += matrix[i, j].ToString() + ", ";
                }
                s += matrix[i, Cols - 1].ToString();
                if (i != Rows - 1 && i == 0)
                    ss += s + "\n";
                else if (i != Rows - 1 && i != 0)
                    ss += " " + s + "\n";
                else
                    ss += " " + s + "\n";
            }
            return ss;
        }

        /// <summary>
        /// Returns the identity matrix.
        /// </summary>
        /// <returns>The identity matrix.</returns>
        public MatrixR Identity()
        {
            MatrixR m = new MatrixR(Rows, Cols);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Rows; j++)
                {
                    if (i == j)
                        m[i, j] = 1;
                }
            }
            return m;
        }

        /// <summary>
        /// Swaps the rows of the matrix.
        /// </summary>
        /// <param name="m">The row to be swapped.</param>
        /// <param name="n">The colum to be swapped.</param>
        /// <returns>The matrix with swapped row and column.</returns>
        public MatrixR GetRowSwap(int m, int n)
        {
            double temp = 0.0;
            for (int i = 0; i < Cols; i++)
            {
                temp = matrix[m, i];
                matrix[m, i] = matrix[n, i];
                matrix[n, i] = temp;
            }
            return new MatrixR(matrix);
        }

        /// <summary>
        /// Get the row size of the matrix.
        /// </summary>
        /// <returns>The row size.</returns>
        public int GetRows()
        {
            return Rows;
        }

        /// <summary>
        /// Get the column size of the matrix.
        /// </summary>
        /// <returns>The column size.</returns>
        public int GetCols()
        {
            return Cols;
        }

        // TODO: comment
        public bool IsSquared()
        {
            if (Rows == Cols)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Get the row vector at a specific index.
        /// </summary>
        /// <param name="m">The row index.</param>
        /// <returns>The row vector.</returns>
        public VectorR GetRowVector(int m)
        {
            if(m < 0 || m > Rows)
            {
                throw new ArgumentOutOfRangeException("m", m, "m is out of range!");
            }
            VectorR v = new VectorR(Cols);
            for(int i = 0; i < Cols; i++)
            {
                v[i] = matrix[m, i];
            }
            return v;
        }

        /// <summary>
        /// Get the column vector at a specific index.
        /// </summary>
        /// <param name="m">The column index.</param>
        /// <returns>The column vector.</returns>
        public VectorR GetColVector(int m)
        {
            if (m < 0 || m > Cols)
            {
                throw new ArgumentOutOfRangeException("m", m, "m is out of range!");
            }
            VectorR v = new VectorR(Rows);
            for (int i = 0; i < Rows; i++)
            {
                v[i] = matrix[i, m];
            }
            return v;
        }

        // TODO: comment
        public static MatrixR Minor(MatrixR m, int row, int col)
        {
            MatrixR mm = new MatrixR(m.GetRows() - 1, m.GetCols() - 1);
            int ii = 0, jj = 0;
            for(int i = 0; i < m.GetRows(); i++)
            {
                if (i == row)
                    continue;
                jj = 0;
                for(int j = 0; j < m.GetCols(); j++)
                {
                    if (j == col)
                        continue;
                    m[ii, jj] = m[i, j];
                    jj++;
                }
                ii++;
            }
            return mm;
        }

        public static double Determinant(MatrixR m)
        {
            double result = 0.0;
            if(!m.IsSquared())
            {
                throw new ArgumentOutOfRangeException("Dimension", m.GetRows(), "The matrix must be squared!");
            }
            if (m.GetRows() == 1)
                result = m[0, 0];
            else
            {
                for (int i = 0; i < m.GetRows(); i++)
                {
                    result += Math.Pow(-1, i) * m[0, i] * Determinant(MatrixR.Minor(m, 0, i));
                }
            }
            return result;
        }

        public static MatrixR Inverse(MatrixR m)
        {
            if(Determinant(m) == 0.0)
            {
                throw new DivideByZeroException("Cannot inverse a matrix with zero determinant!");
            }
            return (Adjoint(m) / Determinant(m));
        }

        public static MatrixR Adjoint(MatrixR m)
        {
            if(!m.IsSquared())
            {
                throw new ArgumentOutOfRangeException("Dimension", m.GetRows(), "The matrix must be squared!");
            }
            MatrixR ma = new MatrixR(m.GetRows(), m.GetCols());
            for(int i = 0; i < m.GetRows(); i++)
            {
                for(int j = 0; j < m.GetCols(); j++)
                {
                    ma[i, j] = Math.Pow(-1, i + j) * (Determinant(Minor(m, i, j)));
                }
            }
            return ma.GetTranspose();
        }

        public MatrixR GetTranspose()
        {
            MatrixR v = this;
            v.Transpose();
            return v;
        }

        public void Transpose()
        {
            MatrixR m = new MatrixR(Cols, Rows);
            for(int i = 0; i < Rows; i++)
            {
                for(int j = 0; j < Cols; j++)
                {
                    m[j, i] = matrix[i, j];
                }
            }
            this = m;
        }
        #endregion

        #region Operators
        public static MatrixR operator * (MatrixR m1, MatrixR m2)
        {
            if (m1.GetCols() != m2.GetRows())
            {
                throw new ArgumentOutOfRangeException("Columns", m1,
                    "The numbers of columns of the first matrix must be " +
                    "equal to the size of the second matrix!");
            }
            MatrixR result = new MatrixR(m1.GetRows(), m2.GetCols());
            VectorR v1 = new VectorR(m1.GetCols());
            VectorR v2 = new VectorR(m2.GetRows());
            for (int i = 0; i < m1.GetRows(); i++ )
            {
                v1 = m1.GetRowVector(i);
                for (int j = 0; j < m2.GetCols(); j++)
                {
                    v2 = m2.GetColVector(j);
                    result[i, j] = VectorR.DotProduct(v1, v2);
                }
            }
            return result;
        }

        public static MatrixR operator / (MatrixR m, double d)
        {
            MatrixR result = new MatrixR(m.GetRows(), m.GetCols());
            for(int i = 0; i < m.GetRows(); i++)
            {
                for(int j = 0; j < m.GetCols(); j++)
                {
                    result[i, j] = m[i, j] / d;
                }
            }
            return result;
        }

        public static MatrixR operator / (double d, MatrixR m)
        {
            MatrixR result = new MatrixR(m.GetRows(), m.GetCols());
            for (int i = 0; i < m.GetRows(); i++)
            {
                for (int j = 0; j < m.GetCols(); j++)
                {
                    result[i, j] = d / m[i, j];
                }
            }
            return result;
        }

        // TODO: correct
        public static VectorR operator * (MatrixR M, VectorR v)
        {
            MatrixR V = new MatrixR(v.GetSize(), 1);
            for (int i = 0; i < v.GetSize(); i++)
                V[i,0] = v[i];

            MatrixR matrixResult = M * V;
            VectorR result = new VectorR(matrixResult.GetRows());
            for (int i = 0; i < matrixResult.GetRows(); i++)
                result[i] = matrixResult[i, 0];

            return result;
        }

        public static VectorR operator * (VectorR v, MatrixR M)
        {
            MatrixR V = new MatrixR(1, v.GetSize());
            for (int i = 0; i < v.GetSize(); i++)
                V[0, i] = v[i];

            MatrixR matrixResult = V * M;
            VectorR result = new VectorR(matrixResult.GetCols());
            for (int i = 0; i < matrixResult.GetCols(); i++)
                result[i] = matrixResult[0, i];

            return result;
        }


        #endregion
    }
    #endregion

    #region LinearSystem
    /// <summary>
    /// Defines a system of linear equations to be solved through Gauss-Jordan elimination.
    /// </summary>
    public class LinearSystem
    {
        double epsilon = 1.0e-800;

        /// <summary>
        /// Performs Gauss-Jordan elimination on a system of linear equations.
        /// </summary>
        /// <param name="A">The matrix of real values.</param>
        /// <param name="b">The vector of real values.</param>
        /// <returns>The solution vector.</returns>
        public VectorR GaussJordan(MatrixR A, VectorR b)
        {
            // TODO: MatrixR is a value type, not reference! By performing operations on MatrixA the matrix will change!

            Triangulate(A, b);
            int n = b.GetSize();
            VectorR x = new VectorR(n);
            // Perform back-substitution
            for(int i = n - 1; i >= 0 ; i--)
            {
                double d = A[i, i];
                if (Math.Abs(d) < epsilon)
                    throw new ArgumentException("Diagonal element is too small!");
                x[i] = (b[i] - VectorR.DotProduct(A.GetRowVector(i), x)) / d;
            }
            return x;
        }

        /// <summary>
        /// Transforms a matrix into triangular form along with the corresponding solution vector.
        /// </summary>
        /// <param name="A">The matrix of real values.</param>
        /// <param name="b">The vector of real values.</param>
        private void Triangulate(MatrixR A, VectorR b)
        {
            int n = A.GetRows();
            VectorR v = new VectorR(n);
            for (int i = 0; i < n - 1; i++)
            {
                double d = PivotGJ(A, b, i);
                if (Math.Abs(d) < epsilon)
                    throw new ArgumentException("Diagonal element is too small!");
                for (int j = i + 1; j < n; j++)
                {
                    double dd = A[j, i] / d;
                    for (int k = i + 1; k < n; k++)
                    {
                        A[j, k] -= dd * A[i, k];
                    }
                    b[j] -= dd * b[i];
                }
            }
        }
        

        /// <summary>
        /// Finds the largest available diagonal element obtained by rearranging the equations.
        /// </summary>
        /// <param name="A">The matrix of real values.</param>
        /// <param name="b">The vector of real values.</param>
        /// <param name="q">The row of the matrix.</param>
        /// <returns>The largest available diagonal element after rearrangement.</returns>
        private double PivotGJ(MatrixR A, VectorR b, int q)
        {
            int n = b.GetSize();
            int i = q;
            double d = 0.0;
            for (int j = q; j < n; j++)
            {
                double dd = Math.Abs(A[j, q]);
                if(dd > d)
                {
                    d = dd;
                    i = j;
                }
            }
            if (i > q)
            {
                A.GetRowSwap(q, i);
                b.GetSwap(q, i);
            }
            return A[q, q];
        }
    }
    #endregion

    #region Eigenvalues
    public class Eigenvalues
    {
        public void ComputeEigenvalues(MatrixR A, out double lambda1, out double lambda2)
        {
            lambda1 = 0.5 * ((A[0, 0] + A[1, 1]) + Math.Sqrt(Math.Pow((A[0, 0] - A[1, 1]), 2.0) + 4.0 * Math.Pow(A[0, 1], 2.0)));
            lambda2 = 0.5 * ((A[0, 0] + A[1, 1]) - Math.Sqrt(Math.Pow((A[0, 0] - A[1, 1]), 2.0) + 4.0 * Math.Pow(A[0, 1], 2.0)));

            // Sort eigenvalues
            if(lambda1 > lambda2)
            {
                double temp = lambda1;
                lambda1 = lambda2;
                lambda2 = temp;
            }
        }

        public void ComputeEigenvectors(MatrixR A, double lambda1, double lambda2, out VectorR eigenVector1, out VectorR eigenVector2)
        {
            VectorR ev1 = new VectorR(3);
            VectorR ev2 = new VectorR(3);

            if (Math.Abs(A[0, 1]) > 1.0e-6)
            {
                ev1[0] = A[0, 1] / (lambda1 - A[0, 0]);
                ev1[1] = 1.0;

                ev2[0] = A[0, 1] / (lambda2 - A[0, 0]);
                ev2[1] = 1.0;
            }
            else
            {
                if (Math.Abs(lambda1 - A[0, 0]) < 1.0e-6)
                {
                    ev1[0] = 1.0;
                    ev1[1] = 0.0;

                    ev2[0] = 0.0;
                    ev2[1] = 1.0;
                }
                else if (Math.Abs(lambda1 - A[1, 1]) < 1.0e-6)
                {
                    ev1[0] = 0.0;
                    ev1[1] = 1.0;

                    ev2[0] = 1.0;
                    ev2[1] = 0.0;
                }
            }

            eigenVector1 = ev1;
            eigenVector2 = ev2;
        }
    }
    #endregion
}