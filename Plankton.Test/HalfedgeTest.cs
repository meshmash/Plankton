using System;
using System.Linq;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class HalfedgeTest
    {
        [Test]
        public void CanFlipEdge()
        {
            // Create a triangulated grid and flip one of the edges.
            // 
            //    Before  >>>  After
            // 
            //   2---5---8   2---5-
            //   |\  |  /|   |  /|
            //   | \ | / |   | / |
            //   |  \|/  |   |/  |/
            //   1---4---7   1---4-
            //   |  /|\  |   |  /|\
            //   | / | \ |
            //   |/  |  \|        (etc.)
            //   0---3---6
            //
            
            PlanktonMesh pMesh = new PlanktonMesh();
            
            pMesh.Vertices.Add(-0.5, -0.5, 0.0); // 0
            pMesh.Vertices.Add(-0.5, 0.0, 0.0);  // 1
            pMesh.Vertices.Add(-0.5, 0.5, 0.0);  // 2
            pMesh.Vertices.Add(0.0, -0.5, 0.0);  // 3
            pMesh.Vertices.Add(0.0, 0.0, 0.0);   // 4
            pMesh.Vertices.Add(0.0, 0.5, 0.0);   // 5
            pMesh.Vertices.Add(0.5, -0.5, 0.0);  // 6
            pMesh.Vertices.Add(0.5, 0.0, 0.0);   // 7
            pMesh.Vertices.Add(0.5, 0.5, 0.0);   // 8
            
            pMesh.Faces.AddFace(4, 1, 0); // 0
            pMesh.Faces.AddFace(4, 0, 3); // 1
            pMesh.Faces.AddFace(4, 3, 6); // 2
            pMesh.Faces.AddFace(4, 6, 7); // 3
            pMesh.Faces.AddFace(4, 7, 8); // 4
            pMesh.Faces.AddFace(4, 8, 5); // 5
            pMesh.Faces.AddFace(4, 5, 2); // 6
            pMesh.Faces.AddFace(4, 2, 1); // 7
            
            // Find the outgoing halfedge of Vertex #4 (center)

            int he = pMesh.Vertices[4].OutgoingHalfedge;

            Assert.AreEqual(29, he);

            Assert.IsTrue(pMesh.Halfedges.FlipEdge(he));
            
            // Check vertices for each face
            Assert.AreEqual(new int[]{ 1, 5, 2 }, pMesh.Faces.GetFaceVertices(6));
            Assert.AreEqual(new int[]{ 5, 1, 4 }, pMesh.Faces.GetFaceVertices(7));

            // Check outgoing he of Vertex #4 has been updated
            he = pMesh.Vertices[4].OutgoingHalfedge;
            Assert.AreNotEqual(29, he, "Vertex #4 should not be linked to Halfedge #29 post-flip");
            Assert.AreEqual(25, he);
            
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
            if (pMesh.Vertices.GetHalfedges(4).Contains(29))
                Assert.Fail("Vertex #4 should not be linked to Halfedge #29 post-flip");
            if (pMesh.Vertices.GetHalfedges(2).Contains(28))
                Assert.Fail("Vertex #2 should not be linked to Halfedge #28 post-flip");
            Assert.Contains(29, pMesh.Vertices.GetHalfedges(5),
                            "Vertex #5 should now be linked to Halfedge #29");
            Assert.Contains(28, pMesh.Vertices.GetHalfedges(1),
                            "Vertex #1 should now be linked to Halfedge #28");
        }
        
        [Test]
        public void CanSplitEdge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            var hs = pMesh.Halfedges;
            
            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            
            // Create two triangular faces
            pMesh.Faces.AddFace(0, 1, 2);
            pMesh.Faces.AddFace(2, 3, 0);
            
            // Split the diagonal edge
            int split_he = hs.FindHalfedge(0, 2);
            int new_he = hs.SplitEdge(split_he);
            
            // Returned halfedge should start at the new vertex
            Assert.AreEqual(4, hs[new_he].StartVertex);
            
            // Check that the 4 halfedges are all in the right places...
            // New ones are between new vertex and second vertex
            Assert.AreEqual(new_he, hs.FindHalfedge(4, 2));
            Assert.AreEqual(hs.PairHalfedge(new_he), hs.FindHalfedge(2, 4));
            // Existing ones are now between first vertex and new vertex
            Assert.AreEqual(split_he, hs.FindHalfedge(0, 4));
            Assert.AreEqual(hs.PairHalfedge(split_he), hs.FindHalfedge(4, 0));
            
            // New halfedges should have the same faces as the existing ones next to them
            Assert.AreEqual(hs[split_he].AdjacentFace, hs[new_he].AdjacentFace);
            Assert.AreEqual(hs[hs.PairHalfedge(split_he)].AdjacentFace,
                            hs[hs.PairHalfedge(new_he)].AdjacentFace);
            
            // New vertex's outgoing should be returned halfedge
            Assert.AreEqual(new_he, pMesh.Vertices[4].OutgoingHalfedge);
        }
    }
}
