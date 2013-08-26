using System;
using System.Linq;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class VertexTest
    {
        [Test]
        public void CanTraverseNakedVertex()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 1, 1);
            Assert.AreEqual(0, pMesh.Vertices.GetHalfedgesCirculator(0).Count());
        }

        [Test]
        public void CanFindHalfedge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 0, 0);
            pMesh.Vertices.Add(1, 1, 0);
            pMesh.Vertices.Add(0, 1, 0);
            pMesh.Faces.AddFace(0, 1, 2, 3);
            Assert.AreEqual(0, pMesh.Halfedges.FindHalfedge(0, 1));
            Assert.AreEqual(2, pMesh.Halfedges.FindHalfedge(1, 2));
            Assert.AreEqual(-1, pMesh.Halfedges.FindHalfedge(0, 2));
        }

        [Test]
        public void CanFindHalfedgeNakedVertex()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 1, 1);
            Assert.AreEqual(-1, pMesh.Halfedges.FindHalfedge(0, 1));
        }

        [Test]
        public void CanSplitVertex()
        {
            // Create a simple non-manifold vertex and split it to make it manifold.

            PlanktonMesh pMesh = new PlanktonMesh();

            // Create 'X' of vertices
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(2, 0, 0); // 1
            pMesh.Vertices.Add(2, 2, 0); // 2
            pMesh.Vertices.Add(0, 2, 0); // 3
            pMesh.Vertices.Add(1, 1, 0); // 4 (center)

            pMesh.Faces.AddFace(0, 4, 3);
            pMesh.Faces.AddFace(2, 4, 1);

            // Check we're using the correct halfedges to split vertex #4
            Assert.AreEqual(8, pMesh.Halfedges.FindHalfedge(4, 1));
            Assert.AreEqual(2, pMesh.Halfedges.FindHalfedge(4, 3));

            Assert.AreEqual(7, pMesh.Vertices[4].OutgoingHalfedge);

            // Split vertex #4 (center)
            int h_new = pMesh.Vertices.SplitVertex(8, 2);

            // Check the new halfedge...
            Assert.AreEqual(12, h_new);
            Assert.AreEqual(h_new, pMesh.Halfedges.FindHalfedge(4, 5));

            // Check old vertex
            Assert.AreEqual(1, pMesh.Vertices[4].OutgoingHalfedge);
            Assert.AreEqual(new int[] { 1, 12, 8 }, pMesh.Vertices.GetHalfedges(4));

            // Check new vertex
            Assert.AreEqual(new int[] { 7, 13, 2 }, pMesh.Vertices.GetHalfedges(5));
        }

        [Test]
        public void CanSplitMergeInvariant()
        {
            // TODO: draw figure here...

            PlanktonMesh pMesh = new PlanktonMesh();

            // Create 'X' of vertices
            pMesh.Vertices.Add(0, 2, 0); // 0
            pMesh.Vertices.Add(0, 1, 0); // 1
            pMesh.Vertices.Add(0, 0, 0); // 2
            pMesh.Vertices.Add(1, 2, 0); // 3
            pMesh.Vertices.Add(1, 1, 0); // 4 (center)
            pMesh.Vertices.Add(1, 0, 0); // 5
            pMesh.Vertices.Add(2, 2, 0); // 6
            pMesh.Vertices.Add(2, 1, 0); // 7
            pMesh.Vertices.Add(2, 0, 0); // 8

            pMesh.Faces.AddFace(2, 4, 1);
            pMesh.Faces.AddFace(7, 4, 8);
            pMesh.Faces.AddFace(0, 1, 4, 3);
            pMesh.Faces.AddFace(3, 4, 7, 6);

            int start_he = 8;
            Assert.AreEqual(start_he, pMesh.Halfedges.FindHalfedge(4, 8));
            Assert.AreEqual(2, pMesh.Halfedges.FindHalfedge(4, 1));

            // Split face into two triangles
            int new_he = pMesh.Vertices.SplitVertex(start_he, 2);

            // Merge them back again
            int old_he = pMesh.Vertices.MergeVertices(new_he);

            // We should be back where we started...
            Assert.AreEqual(start_he, old_he);
        }
    }
}
