using System;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class FaceTest
    {
        [Test]
        public void CanSplitFace()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            
            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            
            // Create one quadrangular face
            pMesh.Faces.AddFace(0, 1, 2, 3);
            
            // Split face into two triangles
            int new_he = pMesh.Faces.SplitFace(0, 4);

            // Returned halfedge should be adjacent to old face (#0)
            Assert.AreEqual(0, pMesh.Halfedges[new_he].AdjacentFace);

            // Traverse from returned halfedge to new face
            int new_he_pair = pMesh.Halfedges.PairHalfedge(new_he);
            int new_face = pMesh.Halfedges[new_he_pair].AdjacentFace;
            
            Assert.AreEqual(1, new_face);
            
            // Check that both faces are now triangular
            Assert.AreEqual(3, pMesh.Faces.GetFaceVertices(0).Length);
            Assert.AreEqual(3, pMesh.Faces.GetFaceVertices(1).Length);
            
            // Check the halfedges of each face
            Assert.AreEqual(new int[] { 8, 0, 2 }, pMesh.Faces.GetHalfedges(0));
            Assert.AreEqual(new int[] { 9, 4, 6 }, pMesh.Faces.GetHalfedges(1));
        }

        [Test]
        public void CanSplitMergeInvariant()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create one quadrangular face
            pMesh.Faces.AddFace(0, 1, 2, 3);

            // Split face into two triangles
            int new_he = pMesh.Faces.SplitFace(0, 4);

            // Merge them back again
            int old_he = pMesh.Faces.MergeFaces(new_he);

            // We should be back where we started...
            Assert.AreEqual(0, old_he);
        }
    }
}
