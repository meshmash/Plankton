using System;
using System.Linq;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class VertexTest
    {
        [Test]
        public void CanTraverseUnusedVertex()
        {
            // Getting halfedges for unused vertex should return empty
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            Assert.IsEmpty(pMesh.Vertices.GetHalfedges(0));
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

            // Create 3x3 grid of vertices
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

        [Test]
        public void CanEraseCenterVertex()
        {
            // TODO: draw figure here...

            PlanktonMesh pMesh = new PlanktonMesh();

            // Create 3x3 grid of vertices
            pMesh.Vertices.Add(0, 2, 0); // 0
            pMesh.Vertices.Add(0, 1, 0); // 1
            pMesh.Vertices.Add(0, 0, 0); // 2
            pMesh.Vertices.Add(1, 2, 0); // 3
            pMesh.Vertices.Add(1, 1, 0); // 4 (center)
            pMesh.Vertices.Add(1, 0, 0); // 5
            pMesh.Vertices.Add(2, 2, 0); // 6
            pMesh.Vertices.Add(2, 1, 0); // 7
            pMesh.Vertices.Add(2, 0, 0); // 8

            // Create four quadrangular faces
            pMesh.Faces.AddFace(0, 1, 4, 3);
            pMesh.Faces.AddFace(3, 4, 7, 6);
            pMesh.Faces.AddFace(1, 2, 5, 4);
            pMesh.Faces.AddFace(4, 5, 8, 7);

            Assert.AreEqual(4, pMesh.Halfedges[4].StartVertex);
            Assert.AreEqual(0, pMesh.Halfedges[4].AdjacentFace);

            // Erase center vertex
            pMesh.Vertices.EraseCenterVertex(4);

            Assert.IsFalse(pMesh.Faces[0].IsUnused);
            int[] faceHalfedges = pMesh.Faces.GetHalfedges(0);
            int[] expected = new int[] { 6, 0, 14, 16, 20, 22, 10, 12 };
            Assert.AreEqual(8, faceHalfedges.Length);
            Assert.AreEqual(expected, faceHalfedges);
            Assert.AreEqual(-1, pMesh.Vertices[4].OutgoingHalfedge);
        }
        
        [Test]
        public void CanCompact()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create 3x3 grid of vertices
            pMesh.Vertices.Add(0, 2, 0); // 0
            pMesh.Vertices.Add(0, 1, 0); // 1
            pMesh.Vertices.Add(0, 0, 0); // 2
            pMesh.Vertices.Add(1, 2, 0); // 3
            pMesh.Vertices.Add(1, 1, 0); // 4 (center)
            pMesh.Vertices.Add(1, 0, 0); // 5
            pMesh.Vertices.Add(2, 2, 0); // 6
            pMesh.Vertices.Add(2, 1, 0); // 7
            pMesh.Vertices.Add(2, 0, 0); // 8

            // Create four quadrangular faces
            pMesh.Faces.AddFace(0, 1, 4, 3);
            pMesh.Faces.AddFace(3, 4, 7, 6);
            pMesh.Faces.AddFace(1, 2, 5, 4);
            pMesh.Faces.AddFace(4, 5, 8, 7);
            
            int vertexCount = pMesh.Vertices.Count;
            
            // Collapse a couple of edges, thus removing two vertices (0 and 2)
            pMesh.Halfedges.CollapseEdge(1);
            pMesh.Halfedges.CollapseEdge(14);
            
            // Compact vertex list
            pMesh.Vertices.CompactHelper();
            
            // Check new size of vertex list
            Assert.AreEqual(vertexCount - 2, pMesh.Vertices.Count);
            
            // Check we can still traverse from vertices correctly (5 used to be 7)
            Assert.AreEqual(new int[] { -1, 3, 1 }, pMesh.Vertices.GetVertexFaces(5));
        }

        [Test]
        public void CanCullUnused()
        {
            // Create a mesh and add some vertices, but don't connect anything to them!
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 1, 1);

            // Cull unused vertices and check count
            pMesh.Vertices.CullUnused();
            Assert.AreEqual(0, pMesh.Vertices.Count);
        }
        
        [Test]
        public void CanSetVertex()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            
            Assert.IsTrue(pMesh.Vertices.SetVertex(0, 1, 1, 1));
            
            PlanktonVertex v;
            v = pMesh.Vertices[0];
            Assert.AreEqual(1, v.X);
            Assert.AreEqual(1, v.Y);
            Assert.AreEqual(1, v.Z);
            
            Assert.IsTrue(pMesh.Vertices.SetVertex(1, 2f, 2f, 2f));
            
            v = pMesh.Vertices[1];
            Assert.AreEqual(2, v.X);
            Assert.AreEqual(2, v.Y);
            Assert.AreEqual(2, v.Z);
            
            Assert.IsFalse(pMesh.Vertices.SetVertex(3, 0, 0, 0));
        }
    }
}
