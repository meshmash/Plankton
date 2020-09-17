using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Plankton
{
    /// <summary>
    /// Provides access to the vertices and <see cref="PlanktonVertex"/> related functionality of a Mesh.
    /// </summary>
    public class PlanktonVertexList : IEnumerable<PlanktonVertex>
    {
        private readonly PlanktonMesh _mesh;
        private List<PlanktonVertex> _list;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="PlanktonVertexList"/> class.
        /// Should be called from the mesh constructor.
        /// </summary>
        /// <param name="owner">The <see cref="PlanktonMesh"/> to which this list of vertices belongs.</param>
        internal PlanktonVertexList(PlanktonMesh owner)
        {
            this._list = new List<PlanktonVertex>();
            this._mesh = owner;
        }
        
        /// <summary>
        /// Gets the number of vertices.
        /// </summary>
        public int Count
        {
            get
            {
                return this._list.Count;
            }
        }
        
        #region methods
        #region vertex access
        #region adding
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonVertex vertex)
        {
            if (vertex == null) return -1;
            this._list.Add(vertex);
            return this.Count - 1;
        }

        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="vertex">Vertex to add.</param>
        /// <returns>The index of the newly added vertex.</returns>
        internal int Add(PlanktonXYZ vertex)
        {            
            this._list.Add(new PlanktonVertex(vertex.X,vertex.Y,vertex.Z));
            return this.Count - 1;
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(double x, double y, double z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }
        
        /// <summary>
        /// Adds a new vertex to the end of the Vertex list.
        /// </summary>
        /// <param name="x">X component of new vertex coordinate.</param>
        /// <param name="y">Y component of new vertex coordinate.</param>
        /// <param name="z">Z component of new vertex coordinate.</param>
        /// <returns>The index of the newly added vertex.</returns>
        public int Add(float x, float y, float z)
        {
            return this.Add(new PlanktonVertex(x, y, z));
        }
        #endregion

        /// <summary>
        /// Adds a series of new vertices to the end of the vertex list.
        /// </summary>
        /// <param name="vertices">A list, an array or any enumerable set of <see cref="PlanktonXYZ"/>.</param>
        /// <returns>Indices of the newly created vertices.</returns>
        public int[] AddVertices(IEnumerable<PlanktonXYZ> vertices)
        {
            return vertices.Select(v => this.Add(v)).ToArray();
        }
        
        /// <summary>
        /// Returns the <see cref="PlanktonVertex"/> at the given index.
        /// </summary>
        /// <param name="index">
        /// Index of vertex to get.
        /// Must be larger than or equal to zero and smaller than the Vertex Count of the mesh.
        /// </param>
        /// <returns>The vertex at the given index.</returns>
        public PlanktonVertex this[int index]
        {
            get
            {
                return this._list[index];
            }
            internal set
            {
                this._list[index] = value;
            }
        }
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, float x, float y, float z)
        {
            if (vertexIndex >= 0 && vertexIndex < _list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = x;
                v.Y = y;
                v.Z = z;
            }
            else if (vertexIndex == _list.Count)
            {
                this.Add(x, y, z);
            }
            else { return false; }
            
            return true;
        }
        
        /// <summary>
        /// <para>Sets or adds a vertex to the Vertex List.</para>
        /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
        /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para>
        /// <para>If [index] is larger than [Count], the function will return false.</para>
        /// </summary>
        /// <param name="vertexIndex">Index of vertex to set.</param>
        /// <param name="x">X component of vertex location.</param>
        /// <param name="y">Y component of vertex location.</param>
        /// <param name="z">Z component of vertex location.</param>
        /// <returns><c>true</c> on success, <c>false</c> on failure.</returns>
        public bool SetVertex(int vertexIndex, double x, double y, double z)
        {
            if (vertexIndex >= 0 && vertexIndex < _list.Count)
            {
                var v = this._list[vertexIndex];
                v.X = (float)x;
                v.Y = (float)y;
                v.Z = (float)z;
            }
            else if (vertexIndex == _list.Count)
            {
                this.Add(x, y, z);
            }
            else { return false; }
            
            return true;
        }
        #endregion
        
        /// <summary>
        /// Helper method to remove dead vertices from the list, re-index and compact.
        /// </summary>
        internal int CompactHelper()
        {
            int marker = 0; // Location where the current vertex should be moved to
            
            // Run through all the vertices
            for (int iter = 0; iter < _list.Count; iter++)
            {
                // If vertex is alive, check if we need to shuffle it down the list
                if (!_list[iter].IsUnused)
                {
                    if (marker < iter)
                    {
                        // Room to shuffle. Copy current vertex to marked slot.
                        _list[marker] = _list[iter];
                        
                        // Update all halfedges which start here
                        int first = _list[marker].OutgoingHalfedge;
                        foreach (int h in _mesh.Halfedges.GetVertexCirculator(first))
                        {
                            _mesh.Halfedges[h].StartVertex = marker;
                        }
                    }
                    marker++; // That spot's filled. Advance the marker.
                }
            }
            
            // Trim list down to new size
            if (marker < _list.Count) { _list.RemoveRange(marker, _list.Count - marker); }
            
            return _list.Count - marker;
        }

        /// <summary>
        /// Removes all vertices that are currently not used by the Halfedge list.
        /// </summary>
        /// <returns>The number of unused vertices that were removed.</returns>
        public int CullUnused()
        {
            return this.CompactHelper();
        }
        
        #region traversals
        /// <summary>
        /// Traverses the halfedge indices which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.</returns>
        [Obsolete("GetHalfedgesCirculator(int) is deprecated, please use" +
                  "Halfedges.GetVertexCirculator(int) instead.")]
        public IEnumerable<int> GetHalfedgesCirculator(int v)
        {
            int he_first = this[v].OutgoingHalfedge;
            if (he_first < 0) yield break; // vertex has no connectivity, exit
            int he_current = he_first;
            var hs = _mesh.Halfedges;
            do
            {
                yield return he_current;
                he_current = hs[hs.GetPairHalfedge(he_current)].NextHalfedge;
            }
            while (he_current != he_first);
        }
        
        /// <summary>
        /// Traverses the halfedge indices which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <param name="first">A halfedge index. Halfedge must start at the specified vertex.</param>
        /// <returns>An enumerable of halfedge indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.
        /// The returned enumerable will start with the specified halfedge.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The specified halfedge does not originate from the specified vertex.
        /// </exception>
        [Obsolete("GetHalfedgesCirculator(int,int) is deprecated, please use" +
            "Halfedges.GetVertexCirculator(int) instead.")]
        public IEnumerable<int> GetHalfedgesCirculator(int v, int first)
        {
            if (_mesh.Halfedges[first].StartVertex != v)
                throw new ArgumentOutOfRangeException("Halfedge does not start at vertex.");
            // TODO: The code below is the same as above.
            // Can we refactor (without extra, unnecessary iterators)?
            int h = first;
            var hs = _mesh.Halfedges;
            do
            {
                yield return h;
                h = hs[hs.GetPairHalfedge(h)].NextHalfedge;
            }
            while (h != first);
        }
        #endregion

        #region adjacency queries
        /// <summary>
        /// Gets the halfedges which originate from a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The indices of halfedges incident to a particular vertex.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetHalfedges(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge).ToArray();
        }

        /// <summary>
        /// Gets the halfedges which end at a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The opposing halfedge for each returned by <see cref="GetHalfedges(int)"/>.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetIncomingHalfedges(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => _mesh.Halfedges.GetPairHalfedge(h)).ToArray();
        }
        
        /// <summary>
        /// Gets vertex neighbours (a.k.a. 1-ring).
        /// </summary>
        /// <param name="f">A vertex index.</param>
        /// <returns>An array of vertex indices incident to the specified vertex.
        /// Ordered clockwise around the vertex.</returns>
        public int[] GetVertexNeighbours(int v)
        {
            var hs = _mesh.Halfedges;
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => hs[hs.GetPairHalfedge(h)].StartVertex).ToArray();
        }
        
        /// <summary>
        /// Gets faces incident to a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>An array of face indices incident to the specified vertex.
        /// Ordered clockwise around the vertex</returns>
        public int[] GetVertexFaces(int v)
        {
            return _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge)
                .Select(h => _mesh.Halfedges[h].AdjacentFace).ToArray();
        }

        /// <summary>
        /// Gets the first <b>incoming</b> halfedge for a vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The index of the halfedge paired with the specified vertex's .</returns>
        public int GetIncomingHalfedge(int v)
        {
            return _mesh.Halfedges.GetPairHalfedge(this[v].OutgoingHalfedge);
        }
        #endregion

        /// <summary>
        /// Gets the number of naked edges incident to this vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The number of incident halfedges which lie on a boundary.</returns>
        public int NakedEdgeCount(int v)
        {
            int nakedCount = 0;
            var hs = _mesh.Halfedges;
            foreach (int i in _mesh.Halfedges.GetVertexCirculator(this[v].OutgoingHalfedge))
            {
                if (hs[i].AdjacentFace == -1 || hs[hs.GetPairHalfedge(i)].AdjacentFace == -1)
                    nakedCount++;
            }
            return nakedCount;
        }

        /// <summary>
        /// Gets the number of edges incident to this vertex.
        /// </summary>
        /// <param name="v">A vertex index.</param>
        /// <returns>The number of incident edges.</returns>
        public int GetValence(int v)
        {
            int h = this[v].OutgoingHalfedge;
            return _mesh.Halfedges.GetVertexCirculator(h).Count();
        }

        /// <summary>
        /// A vertex is on a boundary if its outgoing halfedge has no adjacent face.
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        /// <returns><c>true</c> if the specified vertex is on a boundary; otherwise, <c>false</c>.
        /// Also returns <c>true</c> if the vertex is unused (i.e. no outgoing halfedge).</returns>
        public bool IsBoundary(int index)
        {
            int h = this[index].OutgoingHalfedge;
            return (h < -1 || _mesh.Halfedges[h].AdjacentFace == -1);
        }

        #region Curvature Operators
        /// <summary>
        /// Gets the Gaussian curvature at a vertex using the avarage Voronoi area of the neighbourhood
        /// as described in: M. Meyer, M. Desbrun, P. Schroeder, A.H. Barr, Discrete Differential-Geometry Operators for Triangulated 2-Manifolds
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        /// <returns>The Guassian curvature at the vertex.</returns>
        public double GetGaussianCurvature(int index)
        {
            PlanktonXYZ vertex = this[index].ToXYZ();

            int[] ring;
            //TODO: solve Gaussian curvature for boundary
            //if (this.IsBoundary(index) == false)
            {
                var ringList = this.GetVertexNeighbours(index).ToList();
                ringList.Insert(0, ringList[ringList.Count - 1]);
                ringList.Insert(ringList.Count, ringList[1]);
                ring = ringList.ToArray();
            }
            //else
            //{
            //    var ringList = this.GetVertexNeighbours(index).ToList();
            //    ringList.Insert(0, ringList[ringList.Count - 1]);
            //    //ringList.Insert(ringList.Count, ringList[1]);
            //    ring = ringList.ToArray();
            //}
            int n = ring.Length;

            double mixedArea = 0.0f;
            for (int i = 1; i < n - 1; i++)
            {
                double alpha = PlanktonXYZ.VectorAngle(vertex - this[ring[i - 1]].ToXYZ(), this[ring[i]].ToXYZ() - this[ring[i - 1]].ToXYZ());
                double beta = PlanktonXYZ.VectorAngle(vertex - this[ring[i + 1]].ToXYZ(), this[ring[i]].ToXYZ() - this[ring[i + 1]].ToXYZ());
                double norm = (vertex - this[ring[i]].ToXYZ()).Length;

                if (this.IsBoundary(index) && i == 1)
                    mixedArea += (1.0 / Math.Tan(beta)) * Math.Pow(norm, 2.0);
                else if (this.IsBoundary(index) && i == n - 2)
                    mixedArea += (1.0 / Math.Tan(alpha)) * Math.Pow(norm, 2.0);
                else
                    mixedArea += (1.0 / Math.Tan(alpha) + 1.0 / Math.Tan(beta)) * Math.Pow(norm, 2.0);
            }
            mixedArea /= 8.0;

            if (this.IsBoundary(index))
                n--;

            double gauss = 0.0;
            double ringAngleSum = 0.0;
            for (int i = 1; i < n - 1; i++)
            {
                double theta = PlanktonXYZ.VectorAngle(this[ring[i]].ToXYZ() - vertex, this[ring[i + 1]].ToXYZ() - vertex);
                ringAngleSum += theta;
            }

            if (this.IsBoundary(index))
                gauss = (Math.PI - ringAngleSum) / mixedArea;
            else
                gauss = (2 * Math.PI - ringAngleSum) / mixedArea;

            return gauss;
        }

        /// <summary>
        /// Gets the mean curvature normal at a vertex using the avarage Voronoi area of the neighbourhood
        /// as described in: M. Meyer, M. Desbrun, P. Schroeder, A.H. Barr, Discrete Differential-Geometry Operators for Triangulated 2-Manifolds
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        /// <returns>The mean curvature normal at the vertex.</returns>
        public PlanktonXYZ GetMeanCurvatureNormal(int index)
        {
            PlanktonXYZ vertex = this[index].ToXYZ();

            //TODO: solve mean curvature normal for boundary
            int[] ring;
            //if (this.IsBoundary(index) == false)
            {
                var ringList = this.GetVertexNeighbours(index).ToList();
                ringList.Insert(0, ringList[ringList.Count - 1]);
                ringList.Insert(ringList.Count, ringList[1]);
                ring = ringList.ToArray();
            }
            //else
            //{
            //    var ringList = this.GetVertexNeighbours(index).ToList();
            //    ringList.Insert(0, ringList[ringList.Count - 1]);
            //    //ringList.Insert(ringList.Count, ringList[1]);
            //    ring = ringList.ToArray();
            //}
            int n = ring.Length;

            //TODO: add Mixed Area for non safe Voronoi

            double mixedArea = 0.0f;
            List<double> alphaValues = new List<double>();
            List<double> betaValues = new List<double>();
            List<PlanktonXYZ> edgeVectors = new List<PlanktonXYZ>();
            for (int i = 1; i < n - 1; i++)
            {
                double alpha = PlanktonXYZ.VectorAngle(vertex - this[ring[i - 1]].ToXYZ(), this[ring[i]].ToXYZ() - this[ring[i - 1]].ToXYZ());
                double beta = PlanktonXYZ.VectorAngle(vertex - this[ring[i + 1]].ToXYZ(), this[ring[i]].ToXYZ() - this[ring[i + 1]].ToXYZ());
                PlanktonXYZ edge = (vertex - this[ring[i]].ToXYZ());
                double norm = edge.Length;

                alphaValues.Add(1.0 / Math.Tan(alpha));
                betaValues.Add(1.0 / Math.Tan(beta));
                edgeVectors.Add(edge);

                if (this.IsBoundary(index) && i == 1)
                    mixedArea += (1.0 / Math.Tan(beta)) * Math.Pow(norm, 2.0);
                else if (this.IsBoundary(index) && i == n - 2)
                    mixedArea += (1.0 / Math.Tan(alpha)) * Math.Pow(norm, 2.0);
                else
                    mixedArea += (1.0 / Math.Tan(alpha) + 1.0 / Math.Tan(beta)) * Math.Pow(norm, 2.0);
            }
            mixedArea /= 8.0;

            if (this.IsBoundary(index))
                n--;

            PlanktonXYZ meanNormal = new PlanktonXYZ();
            for (int i = 0; i < edgeVectors.Count; i++)
            {
                meanNormal += edgeVectors[i] * ((float)alphaValues[i] + (float)betaValues[i]);
            }
            meanNormal *= 1.0f / (float)(2.0 * mixedArea);

            return meanNormal;
        }

        /// <summary>
        /// Gets the principle curvature directions at a vertex using a least-square fitting
        /// as described in: M. Meyer, M. Desbrun, P. Schroeder, A.H. Barr, Discrete Differential-Geometry Operators for Triangulated 2-Manifolds
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        public void GetPrincipleCurvatureDirections(int index, out PlanktonXYZ principalMin, out PlanktonXYZ principalMax)
        {
            PlanktonXYZ vertex = this[index].ToXYZ();

            int[] ring;
            //TODO: solve principal curvatures for boundary
            //if (this.IsBoundary(index) == false)
            {
                var ringList = this.GetVertexNeighbours(index).ToList();
                ringList.Insert(0, ringList[ringList.Count - 1]);
                ringList.Insert(ringList.Count, ringList[1]);
                ring = ringList.ToArray();
            }
            int n = ring.Length;

            // Retrieve mean curvature normal and unitize the vector
            // TODO: solve for special case of zero mean curvature (flat plane or local saddle point)
            PlanktonXYZ normal = this.GetMeanCurvatureNormal(index);
            normal *= (1.0f / normal.Length);   // unitize vector

            // edgeVector => (x_i - x_j)
            List<PlanktonXYZ> edgeVectors = new List<PlanktonXYZ>();
            for (int i = 1; i < n - 1; i++)
            {
                PlanktonXYZ edge = (vertex - this[ring[i]].ToXYZ());
                edgeVectors.Add(edge);
            }

            // Retrieve d_ij the unit direction in the tangent plane of the edge x_i x_j
            List<PlanktonXYZ> ringVectors = new List<PlanktonXYZ>();
            foreach (PlanktonXYZ e in edgeVectors)
            {
                float prod = PlanktonXYZ.DotProduct((-1.0f * e), normal);
                PlanktonXYZ d = (-1.0f * e) - (PlanktonXYZ.DotProduct((-1.0f * e), normal)) * normal;
                d = d * (1.0f / d.Length);
                ringVectors.Add(d);
            }

            // Change basis
            // Take first vector of d_ij as x-axis
            // Retrieve y-axis of tangent plane -> cross-product normal x d_ij[0]
            PlanktonXYZ yAxis = PlanktonXYZ.CrossProduct(normal, ringVectors[0]);

            // Express d_ij in new basis
            List<VectorR> d_ij = new List<VectorR>();
            for (int i = 0; i < ringVectors.Count; i++)
            {
                // TODO: solve value/reference problem of MatrixR
                MatrixR baseTrans = new MatrixR(new double[2, 2]{
                    {ringVectors[0].X,  yAxis.X},
                    {ringVectors[0].Y,  yAxis.Y}
                });

                LinearSystem ls = new LinearSystem();
                VectorR v = new VectorR(new double[]{
                    ringVectors[i].X, 
                    ringVectors[i].Y
                });
                VectorR d = ls.GaussJordan(baseTrans, v);
                d_ij.Add(d);
            }

            // Compute the coefficients of matrix E
            double e00 = 0.0; double e01 = 0.0; double e02 = 0.0;
            double e11 = 0.0; double e12 = 0.0;
            double e22 = 0.0;
            for (int i = 0; i < d_ij.Count; i++)
            {
                e00 += 1.0 / n * 2 * Math.Pow(d_ij[i][0], 4.0);
                e01 += 1.0 / n * 4 * Math.Pow(d_ij[i][0], 3.0) * d_ij[i][1];
                e02 += 1.0 / n * 2 * Math.Pow(d_ij[i][0], 2.0) * Math.Pow(d_ij[i][1], 2.0);
                e11 += 1.0 / n * 8 * Math.Pow(d_ij[i][0], 2.0) * Math.Pow(d_ij[i][1], 2.0);
                e12 += 1.0 / n * 4 * d_ij[i][0] * Math.Pow(d_ij[i][1], 3.0);
                e22 += 1.0 / n * 2 * Math.Pow(d_ij[i][1], 4.0);
            }
            MatrixR E = new MatrixR(new double[3, 3]{
                {e00, e01, e02},
                {e01, e11, e12},
                {e02, e12, e22}});

            // Retrieve k_ij the summation of the estimate of the normal curvature in the direction of the edge x_i x_j
            //List<float> kN = new List<float>();
            List<double> k_ij = new List<double>();
            foreach (PlanktonXYZ e in edgeVectors)
            {
                k_ij.Add(2 * (PlanktonXYZ.DotProduct(e, normal) / (float)Math.Pow(e.Length, 2.0f)));
            }
            // Compute the coefficients of the vector kappaN
            double k0 = 0.0;
            double k1 = 0.0;
            double k2 = 0.0;
            for (int i = 0; i < d_ij.Count; i++)
            {
                k0 += 1.0 / n * 2 * Math.Pow(d_ij[i][0], 2.0) * k_ij[i];
                k1 += 1.0 / n * 4 * d_ij[i][0] * d_ij[i][1] * k_ij[i];
                k2 += 1.0 / n * 2 * Math.Pow(d_ij[i][1], 2.0) * k_ij[i];
            }
            // Create the solution vector kappaN
            VectorR kappaN = new VectorR(new double[]{
                k0,
                k1,
                k2
            });

            // Solve the linear system to find the coeficients a, b, c of the curvature matrix
            LinearSystem ls_D = new LinearSystem();
            VectorR b = ls_D.GaussJordan(E, kappaN);

            // Compute eigenvectors of the curvature matrix B
            MatrixR B = new MatrixR(new double[2, 2]{
                {b[0], b[1]},
                {b[1], b[2]},
            });
            Eigenvalues ev = new Eigenvalues();
            double lambda1, lambda2;
            ev.ComputeEigenvalues(B, out lambda1, out lambda2);
            VectorR eigenVec1, eigenVec2;
            ev.ComputeEigenvectors(B, lambda1, lambda2, out eigenVec1, out eigenVec2);

            // Change basis of eigenvectors to standard coordinates
            MatrixR standardTrans = new MatrixR(new double[3, 3]{
                    {ringVectors[0].X,  ringVectors[0].Y,  ringVectors[0].Z},
                    {         yAxis.X,           yAxis.Y,           yAxis.Z},
                    {        normal.X,          normal.Y,          normal.Z}
            });
            // TODO: fix multiplication order
            VectorR standardEigenVec1 = new VectorR(new double[] { eigenVec1[0], eigenVec1[1], 0.0 }) * standardTrans;
            VectorR standardEigenVec2 = new VectorR(new double[] { eigenVec2[0], eigenVec2[1], 0.0 }) * standardTrans;

            // Return result
            principalMin = new PlanktonXYZ((float)standardEigenVec1[0], (float)standardEigenVec1[1], (float)standardEigenVec1[2]);
            principalMax = new PlanktonXYZ((float)standardEigenVec2[0], (float)standardEigenVec2[1], (float)standardEigenVec2[2]);
        }
        #endregion

        /// <summary>
        /// Gets the normal vector at a vertex.
        /// </summary>
        /// <param name="index">The index of a vertex.</param>
        /// <returns>The area weighted vertex normal.</returns>
        public PlanktonXYZ GetNormal(int index)
        {
            PlanktonXYZ vertex = this[index].ToXYZ();
            PlanktonXYZ normal = new PlanktonXYZ();

            var ring = this.GetVertexNeighbours(index);
            int n = ring.Length;

            for (int i = 0; i < n-1; i++)
            {
                normal += PlanktonXYZ.CrossProduct(
                    this[ring[i]].ToXYZ() - vertex, 
                    this[ring[i+1]].ToXYZ() - vertex);
            }

            if (this.IsBoundary(index) == false)
            {
                normal += PlanktonXYZ.CrossProduct(
                    this[n-1].ToXYZ() - vertex,
                    this[0].ToXYZ() - vertex);
            }

            return normal * (-1.0f / normal.Length); // return unit vector
        }

        /// <summary>
        /// Gets the normal vectors for all vertices in the mesh.
        /// </summary>
        /// <returns>The area weighted vertex normals of all vertices in the mesh.</returns>
        /// <remarks>
        /// This will be accurate at the time of calling but will quickly
        /// become outdated if you start fiddling with the mesh.
        /// </remarks>
        public PlanktonXYZ[] GetNormals()
        {
            return Enumerable.Range(0, this.Count).Select(i => this.GetNormal(i)).ToArray();
        }

        /// <summary>
        /// Gets the positions of all vertices.
        /// </summary>
        /// <returns>The positions of all vertices in the mesh.</returns>
        public PlanktonXYZ[] GetPositions()
        {
            return Enumerable.Range(0, this.Count).Select(i => this[i].ToXYZ()).ToArray();
        }

        #region Euler operators
        /// <summary>
        /// <para>Merges two vertices by collapsing the pair of halfedges between them.</para>
        /// <seealso cref="PlanktonHalfedgeList.CollapseEdge"/>
        /// </summary>
        /// <param name="halfedge">The index of a halfedge between the two vertices to be merged.
        /// The starting vertex of this halfedge will be retained.</param>
        /// <returns>The successor of <paramref name="index"/> around its vertex, or -1 on failure.</returns>
        /// <remarks>The invariant <c>mesh.Vertices.MergeVertices(mesh.Vertices.SplitVertex(a, b))</c> will return a,
        /// leaving the mesh unchanged.</remarks>
        public int MergeVertices(int halfedge)
        {
            return _mesh.Halfedges.CollapseEdge(halfedge);

        }

        /// <summary>
        /// Splits the vertex into two, joined by a new pair of halfedges.
        /// </summary>
        /// <param name="first">The index of a halfedge which starts at the vertex to split.</param>
        /// <param name="second">The index of a second halfedge which starts at the vertex to split.</param>
        /// <returns>The new halfedge which starts at the existing vertex.</returns>
        /// <remarks>After the split, the <paramref name="second"/> halfedge will be starting at the newly added vertex.</remarks>
        public int SplitVertex(int first, int second)
        {
            var hs = _mesh.Halfedges;
            // Check that both halfedges start at the same vertex
            int v_old = hs[first].StartVertex;
            if (v_old != hs[second].StartVertex) { return -1; } // TODO: return ArgumentException instead?

            // Create a copy of the existing vertex (user can move it afterwards if needs be)
            int v_new = this.Add(this[v_old].ToXYZ()); // copy vertex by converting to XYZ and back

            // Go around outgoing halfedges, from 'second' to just before 'first'
            // Set start vertex to new vertex
            bool reset_v_old = false;
            foreach (int h in hs.GetVertexCirculator(second))
            {
                if (h == first) { break; }
                hs[h].StartVertex = v_new;
                // If new vertex has no outgoing yet and current he is naked...
                if (this[v_new].OutgoingHalfedge == -1 && hs[h].AdjacentFace == -1)
                    this[v_new].OutgoingHalfedge = h;
                // Also check whether existing vert's he is now incident to new one
                if (h == this[v_old].OutgoingHalfedge) { reset_v_old = true; }
            }
            // If no naked halfedges, just use 'second'
            if (this[v_new].OutgoingHalfedge == -1) { this[v_new].OutgoingHalfedge = second; }

            // Add the new pair of halfedges from old vertex to new
            int h_new = hs.AddPair(v_old, v_new, hs[second].AdjacentFace);
            int h_new_pair = hs.GetPairHalfedge(h_new);
            hs[h_new_pair].AdjacentFace = hs[first].AdjacentFace;

            // Link new pair into mesh
            hs.MakeConsecutive(hs[first].PrevHalfedge, h_new_pair);
            hs.MakeConsecutive(h_new_pair, first);
            hs.MakeConsecutive(hs[second].PrevHalfedge, h_new);
            hs.MakeConsecutive(h_new, second);

            // Re-set existing vertex's outgoing halfedge, if necessary
            if (reset_v_old)
            {
                this[v_old].OutgoingHalfedge = h_new;
                foreach (int h in hs.GetVertexCirculator(h_new))
                {
                    if (hs[h].AdjacentFace == -1) { this[v_old].OutgoingHalfedge = h; }
                }
            }

            // return the new vertex which starts at the existing vertex
            return h_new;
        }

        /// <summary>
        /// Erases a vertex and all incident halfedges by merging its incident faces.
        /// </summary>
        /// <param name="halfedgeIndex">The index of a halfedge which starts at the vertex to erase.
        /// The retained face will be the one adjacent to this halfedge.</param>
        /// <returns>The successor of <paramref name="halfedgeIndex"/> around its original face.</returns>
        public int EraseCenterVertex(int halfedgeIndex)
        {
            int vertexIndex = _mesh.Halfedges[halfedgeIndex].StartVertex;

            // Check that the vertex is completely surrounded by faces
            if (this.IsBoundary(vertexIndex))
                throw new ArgumentException("Center vertex must not be on a boundary");

            // Get outgoing halfedges around vertex, starting with specified halfedge
            int[] vertexHalfedges = _mesh.Halfedges.GetVertexCirculator(halfedgeIndex).ToArray();

            // Check for 2-valent vertices in the 1-ring (no antennas)
            int v;
            foreach (int h in vertexHalfedges)
            {
                v = _mesh.Halfedges.EndVertex(h);
                if (this.GetHalfedges(v).Length < 3)
                    throw new ArgumentException("Vertex in 1-ring is 2-valent");
            }

            // Store face to keep and set its first halfedge
            int faceIndex = _mesh.Halfedges[halfedgeIndex].AdjacentFace;
            int firstHalfedge = _mesh.Halfedges[halfedgeIndex].NextHalfedge;
            _mesh.Faces[faceIndex].FirstHalfedge = firstHalfedge;

            // Remove incident halfedges and mark faces for deletion (except first face)
            _mesh.Halfedges.RemovePairHelper(vertexHalfedges[0]);
            for (int i = 1; i < vertexHalfedges.Length; i++)
            {
                _mesh.Faces[_mesh.Halfedges[vertexHalfedges[i]].AdjacentFace] = PlanktonFace.Unset;
                _mesh.Halfedges.RemovePairHelper(vertexHalfedges[i]);
            }

            // Set adjacent face for all halfedges in hole
            foreach (int h in _mesh.Halfedges.GetFaceCirculator(firstHalfedge))
            {
                _mesh.Halfedges[h].AdjacentFace = faceIndex;
            }

            // Mark center vertex for deletion
            this[vertexIndex] = PlanktonVertex.Unset;

            return _mesh.Faces[faceIndex].FirstHalfedge;
        }
        #endregion

        /// <summary>
        /// Truncates a vertex by creating a face with vertices on each of the outgoing halfedges.
        /// </summary>
        /// <param name="v">The index of a vertex.</param>
        /// <returns>The index of the newly created face.</returns>
        public int TruncateVertex(int v)
        {
            var hs = this.GetHalfedges(v);
        
            // set h_new and move original vertex
            int h_new = hs[0];
        
            // circulate outgoing halfedges (clockwise, skip first)
            for (int i = 1; i < hs.Length; i++)
            {
                // split vertex
                int h_tmp = this.SplitVertex(hs[i], h_new);
                h_new = h_tmp; // tidy-up if 'vs' is removed
            }

            // split face to create new truncated face
            int splitH = this._mesh.Faces.SplitFace(hs[0], h_new);

            return this._mesh.Halfedges[this._mesh.Halfedges.GetPairHalfedge(splitH)].AdjacentFace;
        }
        #endregion
        
        #region IEnumerable implementation
        /// <summary>
        /// Gets an enumerator that yields all faces in this collection.
        /// </summary>
        /// <returns>An enumerator.</returns>
        public IEnumerator<PlanktonVertex> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }
}
