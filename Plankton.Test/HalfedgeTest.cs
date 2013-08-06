using System;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class HalfedgeTest
    {
        [Test]
        public void CanFlipEdge()
        {
            // Create a simple square
            
            PlanktonMesh pMesh = new PlanktonMesh();
            
            pMesh.Vertices.Add(-0.5, -0.5, 0.0); // bottom-left
            pMesh.Vertices.Add(-0.5, 0.5, 0.0);  // top-left
            pMesh.Vertices.Add(0.5, 0.5, 0.0);   // top right
            pMesh.Vertices.Add(0.5, -0.5, 0.0);  // bottom-right
            
            pMesh.Faces.AddFace(new int[]{ 0, 2, 1 });
            pMesh.Faces.AddFace(new int[]{ 2, 0, 3 });
            
            // Flip zero-th halfedge
            
            Assert.IsTrue(pMesh.Halfedges.FlipEdge(0));
            
            // Check vertices for each face
            Assert.AreEqual(new int[]{ 3, 1, 0 }, pMesh.Faces.GetVertices(0));
            Assert.AreEqual(new int[]{ 1, 3, 2 }, pMesh.Faces.GetVertices(1));
            
            // Check adjacent face in each interior halfedge is correct
            foreach (int h in pMesh.Faces.GetHalfedgesCirculator(0))
            {
                Assert.AreEqual(0, pMesh.Halfedges[h].AdjacentFace);
            }
            foreach (int h in pMesh.Faces.GetHalfedgesCirculator(1))
            {
                Assert.AreEqual(1, pMesh.Halfedges[h].AdjacentFace);
            }
            
            // Check halfedges for each vertex
            Assert.AreEqual(new int[]{ 5, 6 }, pMesh.Vertices.GetHalfedges(0));
            Assert.AreEqual(new int[]{ 3, 1, 4 }, pMesh.Vertices.GetHalfedges(1));
            Assert.AreEqual(new int[]{ 9, 2 }, pMesh.Vertices.GetHalfedges(2));
            Assert.AreEqual(new int[]{ 7, 0, 8 }, pMesh.Vertices.GetHalfedges(3));
            
        }
    }
}
