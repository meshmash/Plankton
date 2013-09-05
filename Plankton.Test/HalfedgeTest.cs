using System;
using System.Linq;
using NUnit.Framework;

namespace Plankton.Test
{
    [TestFixture]
    public class HalfedgeTest
    {
        [Test]
        public void CanFindHalfedge()
        {
            // Create a mesh with a single quad face
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 0, 0);
            pMesh.Vertices.Add(1, 1, 0);
            pMesh.Vertices.Add(0, 1, 0);
            pMesh.Faces.AddFace(0, 1, 2, 3);
            // Try and find some halfedges...
            Assert.AreEqual(0, pMesh.Halfedges.FindHalfedge(0, 1));
            Assert.AreEqual(2, pMesh.Halfedges.FindHalfedge(1, 2));
            Assert.AreEqual(-1, pMesh.Halfedges.FindHalfedge(0, 2));
        }

        [Test]
        public void CanFindHalfedgeUnusedVertices()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 1, 1);
            // Check for halfedge between v0 and v1
            // In fact, both are unused so we shouldn't find one
            Assert.AreEqual(-1, pMesh.Halfedges.FindHalfedge(0, 1));
        }

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
            foreach (int h in pMesh.Faces.GetHalfedges(0))
            {
                Assert.AreEqual(0, pMesh.Halfedges[h].AdjacentFace);
            }
            foreach (int h in pMesh.Faces.GetHalfedges(1))
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

            // Change outgoing of vert #2 so that we can check it updates
            pMesh.Vertices[2].OutgoingHalfedge = 4;
            
            // Split the diagonal edge
            int split_he = 5; // he from v #0 to #2
            int new_he = hs.SplitEdge(split_he);
            
            // Returned halfedge should start at the new vertex
            Assert.AreEqual(4, hs[new_he].StartVertex);
            
            // Check that the 4 halfedges are all in the right places...
            // New ones are between new vertex and second vertex
            Assert.AreEqual(new_he, hs.FindHalfedge(4, 2));
            Assert.AreEqual(hs.GetPairHalfedge(new_he), hs.FindHalfedge(2, 4));
            // Existing ones are now between first vertex and new vertex
            Assert.AreEqual(split_he, hs.FindHalfedge(0, 4));
            Assert.AreEqual(hs.GetPairHalfedge(split_he), hs.FindHalfedge(4, 0));
            
            // New halfedges should have the same faces as the existing ones next to them
            Assert.AreEqual(hs[split_he].AdjacentFace, hs[new_he].AdjacentFace);
            Assert.AreEqual(hs[hs.GetPairHalfedge(split_he)].AdjacentFace,
                            hs[hs.GetPairHalfedge(new_he)].AdjacentFace);
            
            // New vertex's outgoing should be returned halfedge
            Assert.AreEqual(new_he, pMesh.Vertices[4].OutgoingHalfedge);

            // New vertex should be 2-valent
            Assert.AreEqual(2, pMesh.Vertices.GetHalfedges(4).Length);

            // Check existing vertices...
            Assert.AreEqual(new int[] {9, 5, 0}, pMesh.Vertices.GetHalfedges(0));
            Assert.AreEqual(new int[] {11, 6, 3}, pMesh.Vertices.GetHalfedges(2));
        }

        [Test]
        public void CanCollapseBoundaryEdge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create one quadrangular face
            pMesh.Faces.AddFace(0, 1, 2, 3);

            int h_clps = 0;
            int h_clps_prev = pMesh.Halfedges[h_clps].PrevHalfedge;

            // Confirm face's first halfedge is the one to be collapsed
            Assert.AreEqual(h_clps, pMesh.Faces[0].FirstHalfedge);

            // Collapse edge
            int h_rtn = pMesh.Halfedges.CollapseEdge(h_clps);

            // Edge collapse should return successor around start vertex
            Assert.AreEqual(7, h_rtn);

            // Check face's first halfedge was updated
            Assert.AreNotEqual(h_clps, pMesh.Faces[0].FirstHalfedge);

            // Check for closed loop (without collapsed halfedge)
            Assert.AreEqual(new int[] { 2, 4, 6 }, pMesh.Faces.GetHalfedges(0));

            // Pair of predecessor to collapsed halfedge should now have its start vertex
            int h_clps_prev_pair = pMesh.Halfedges.GetPairHalfedge(h_clps_prev);
            Assert.AreEqual(0, pMesh.Halfedges[h_clps_prev_pair].StartVertex);
        }

        [Test]
        public void CanCollapseInternalEdge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create a three-by-three grid of vertices
            pMesh.Vertices.Add(-0.5, -0.5, 0.0); // 0
            pMesh.Vertices.Add(-0.5, 0.0, 0.0);  // 1
            pMesh.Vertices.Add(-0.5, 0.5, 0.0);  // 2
            pMesh.Vertices.Add(0.0, -0.5, 0.0);  // 3
            pMesh.Vertices.Add(0.0, 0.0, 0.0);   // 4
            pMesh.Vertices.Add(0.0, 0.5, 0.0);   // 5
            pMesh.Vertices.Add(0.5, -0.5, 0.0);  // 6
            pMesh.Vertices.Add(0.5, 0.0, 0.0);   // 7
            pMesh.Vertices.Add(0.5, 0.5, 0.0);   // 8

            // Create four quadrangular faces
            pMesh.Faces.AddFace(1, 4, 5, 2);
            pMesh.Faces.AddFace(0, 3, 4, 1);
            pMesh.Faces.AddFace(4, 7, 8, 5);
            pMesh.Faces.AddFace(3, 6, 7, 4);

            Assert.AreEqual(4, pMesh.Faces.Count);

            int h_clps = pMesh.Vertices[4].OutgoingHalfedge;
            int v_suc = pMesh.Vertices.GetHalfedges(4)[1];
            int h_boundary = pMesh.Vertices[3].OutgoingHalfedge;

            // Collapse center vertex's outgoing halfedge
            int h_rtn = pMesh.Halfedges.CollapseEdge(h_clps);

            // Check that center vertex's outgoing halfedge has been updated
            Assert.AreEqual(h_boundary, pMesh.Vertices[4].OutgoingHalfedge);

            // Edge collapse should return successor around start vertex
            Assert.AreEqual(v_suc, h_rtn);

            // Check for closed loops (without collapsed halfedge)
            Assert.AreEqual(4, pMesh.Faces.GetHalfedges(0).Length);
            Assert.AreEqual(3, pMesh.Faces.GetHalfedges(1).Length);
            Assert.AreEqual(4, pMesh.Faces.GetHalfedges(2).Length);
            Assert.AreEqual(3, pMesh.Faces.GetHalfedges(3).Length);

            // Check no halfedges reference removed vertex (#7)
            for (int h = 0; h < pMesh.Halfedges.Count; h++)
            {
                if (h == h_clps || h == pMesh.Halfedges.GetPairHalfedge(h_clps))
                    continue; // Skip removed halfedges
                Assert.AreNotEqual(3, pMesh.Halfedges[h].StartVertex);
            }
        }

        [Test]
        public void CannotCollapseNonManifoldVertex()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create vertices in 3x2 grid
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            pMesh.Vertices.Add(2, 0, 0); // 4
            pMesh.Vertices.Add(2, 1, 0); // 5

            // Create two quadrangular faces
            pMesh.Faces.AddFace(0, 1, 2, 3);
            pMesh.Faces.AddFace(1, 4, 5, 2);

            // Try to collapse edge between vertices #1 and #2
            // (which would make vertex #1 non-manifold)
            int h = pMesh.Halfedges.FindHalfedge(1, 2);
            Assert.AreEqual(-1, pMesh.Halfedges.CollapseEdge(h));

            // That's right, you can't!
        }

        [Test]
        public void CanCollapseAdjacentTriangles()
        {
            // TODO: draw figure here...

            PlanktonMesh pMesh = new PlanktonMesh();

            // Create several vertices
            pMesh.Vertices.Add(0, 3, 0); // 0
            pMesh.Vertices.Add(0, 2, 0); // 1
            pMesh.Vertices.Add(0, 1, 0); // 2
            pMesh.Vertices.Add(1, 3, 0); // 3
            pMesh.Vertices.Add(1, 2, 0); // 4
            pMesh.Vertices.Add(1, 1, 0); // 5
            pMesh.Vertices.Add(1, 0, 0); // 6
            pMesh.Vertices.Add(2, 2, 0); // 7
            pMesh.Vertices.Add(2, 1, 0); // 8

            // Create several faces
            pMesh.Faces.AddFace(0, 1, 4, 3); // 0
            pMesh.Faces.AddFace(1, 2, 5, 4); // 1
            pMesh.Faces.AddFace(3, 4, 7);    // 2
            pMesh.Faces.AddFace(4, 5, 7);    // 3
            pMesh.Faces.AddFace(7, 5, 6, 8); // 4

            // Try to collapse edge between vertices #4 and #7
            int h_clps = pMesh.Halfedges.FindHalfedge(4, 7);
            //int v_keep = pMesh.Halfedges[h_clps].StartVertex;
            int h_succ = pMesh.Halfedges.GetVertexCirculator(h_clps).ElementAt(1);
            Assert.AreEqual(h_succ, pMesh.Halfedges.CollapseEdge(h_clps));

            // Successor to h (around h's start vertex) should now be adjacent to face #4
            Assert.AreEqual(4, pMesh.Halfedges[h_succ].AdjacentFace);

            // Check new vertices of face #4
            Assert.AreEqual(new int[] { 5, 6, 8, 4 }, pMesh.Faces.GetFaceVertices(4));

            // Traverse around mesh boundary and count halfedges
            int count, he_first, he_current;
            count = 0;
            he_first = 1;
            he_current = he_first;
            do
            {
                count++;
                he_current = pMesh.Halfedges[he_current].NextHalfedge;
            }
            while (he_current != he_first);

            Assert.AreEqual(8, count);

            Assert.IsTrue(pMesh.Faces[2].IsUnused && pMesh.Faces[3].IsUnused);
        }

        [Test]
        public void CanCollapseValenceThreeVertex()
        {
            // Create five faces and collapse diagonal edge
            // (halfedge {4->8} - valence three vertex at end)
            //
            // 0---3---6
            // |   |   |
            // |   |   |
            // 1-- 4---7
            // |   |\  |
            // |   |  \|
            // 2---5---8

            PlanktonMesh pMesh = new PlanktonMesh();

            // Create mesh with one triangular face
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(2, 0, 0); // 1
            pMesh.Vertices.Add(1, 1.4, 0); // 2
            pMesh.Faces.AddFace(0, 1, 2);
            
            pMesh.Faces.Stellate(0);
            int h = pMesh.Vertices.GetIncomingHalfedge(3);
            
            Assert.AreEqual(3, pMesh.Faces.Count);
            
            Assert.GreaterOrEqual(0, pMesh.Halfedges.CollapseEdge(h));
            
            pMesh.Compact();
            
            Assert.AreEqual(1, pMesh.Faces.Count);
        }
        
        [Test]
        public void CannotCollapseNonManifoldEdge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create vertices in 3x2 grid
            pMesh.Vertices.Add(-1, 0, 0); // 0
            pMesh.Vertices.Add(0, -1, 0); // 1
            pMesh.Vertices.Add(1, 0, 0);  // 2
            pMesh.Vertices.Add(0, 1, 0);  // 3
            pMesh.Vertices.Add(-1, -2, 0); // 4
            pMesh.Vertices.Add(0, -3, 0);  // 5
            pMesh.Vertices.Add(1, -2, 0);  // 6

            // Create several triangular faces
            pMesh.Faces.AddFace(0, 1, 3);
            pMesh.Faces.AddFace(1, 2, 3);
            pMesh.Faces.AddFace(0, 4, 1);
            pMesh.Faces.AddFace(4, 5, 1);
            // And one quad face
            pMesh.Faces.AddFace(1, 5, 6, 2);
            
            pMesh.Faces.Stellate(0);
            pMesh.Faces.Stellate(1);
            
            Assert.AreEqual(9, pMesh.Faces.Count);
            
            Assert.AreEqual(-1, pMesh.Halfedges.CollapseEdge(2));
            Assert.AreEqual(-1, pMesh.Halfedges.CollapseEdge(6));
        }

        [Test]
        public void CanCompact()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create vertices in 3x2 grid
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            pMesh.Vertices.Add(2, 0, 0); // 4
            pMesh.Vertices.Add(2, 1, 0); // 5

            // Create two quadrangular faces
            pMesh.Faces.AddFace(0, 1, 2, 3);
            pMesh.Faces.AddFace(1, 4, 5, 2);

            // Remove the first face and compact
            pMesh.Faces.RemoveFace(0);
            pMesh.Halfedges.CompactHelper();

            // Check some things about the compacted mesh
            Assert.AreEqual(8, pMesh.Halfedges.Count);
            Assert.AreEqual(new int[] { 1, 4, 5, 2 }, pMesh.Faces.GetFaceVertices(1));
        }
        
        [Test]
        public void CannotTraverseUnusedHalfedge()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Halfedges.Add(PlanktonHalfedge.Unset);
            pMesh.Halfedges.Add(PlanktonHalfedge.Unset);
            
            // You shouldn't be able to enumerate a circulator for either of these unset halfedges
            Assert.Throws<InvalidOperationException>(() => pMesh.Halfedges.GetFaceCirculator(0).ToArray());
            Assert.Throws<InvalidOperationException>(
                delegate { foreach (int h in pMesh.Halfedges.GetVertexCirculator(1)) {} } );
        }
    }
}
