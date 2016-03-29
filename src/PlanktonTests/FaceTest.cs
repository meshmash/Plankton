using System;
using System.Collections.Generic;
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
            int new_he_pair = pMesh.Halfedges.GetPairHalfedge(new_he);
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
        public void CannotSplitFaceBadArguments()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create one quadrangular face
            pMesh.Faces.AddFace(0, 1, 2, 3);

            // First halfedge is a boundary
            Assert.AreEqual(-1, pMesh.Faces.SplitFace(1, 4));

            // Second halfedge is a boundary
            Assert.AreEqual(-1, pMesh.Faces.SplitFace(4, 1));

            // Same halfedge used for both arguments
            Assert.AreEqual(-1, pMesh.Faces.SplitFace(0, 0));

            // Second halfedge is successor to first
            Assert.AreEqual(-1, pMesh.Faces.SplitFace(0, 2));

            // Second halfedge is predecessor to first
            Assert.AreEqual(-1, pMesh.Faces.SplitFace(0, 6));
        }

        [Test]
        public void CanMergeFaces()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create two triangular faces
            pMesh.Faces.AddFace(0, 1, 2);
            pMesh.Faces.AddFace(2, 3, 0);

            // Force merge to update outgoing halfedge of vertex #2
            pMesh.Vertices[2].OutgoingHalfedge = 4;

            // Merge faces
            int h_rtn = pMesh.Faces.MergeFaces(4);

            // Check that the correct face was retained
            int f = pMesh.Halfedges[h_rtn].AdjacentFace;
            Assert.AreEqual(0, f);

            // Check face halfedges
            int[] fhs = pMesh.Faces.GetHalfedges(f);
            Assert.AreEqual(new int[] { 0, 2, 6, 8 }, fhs);
            foreach (int h in fhs)
            {
                Assert.AreEqual(f, pMesh.Halfedges[h].AdjacentFace);
            }

            // Check that outgoing halfedge of vertex #2 was updated correctly
            Assert.AreEqual(6, pMesh.Vertices[2].OutgoingHalfedge);
            Assert.AreEqual(f, pMesh.Halfedges[6].AdjacentFace);
        }

        [Test]
        public void CannotMergeFacesBoundary()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create one quadrangular face
            pMesh.Faces.AddFace(0, 1, 2, 3);

            Assert.AreEqual(-1, pMesh.Faces.MergeFaces(0));
        }

        [Test]
        public void CannotMergeFacesAntenna()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            pMesh.Vertices.Add(0.5, 0.5, 0); // 4

            // Create two quadrangular faces
            pMesh.Faces.AddFace(0, 1, 2, 4);
            pMesh.Faces.AddFace(2, 3, 0, 4);

            // Merge should fail (faces are joined by two edges)
            Assert.AreEqual(-1, pMesh.Faces.MergeFaces(4));
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

            int start_he = 0;

            // Split face into two triangles
            int new_he = pMesh.Faces.SplitFace(start_he, 4);

            // Merge them back again
            int old_he = pMesh.Faces.MergeFaces(new_he);

            // We should be back where we started...
            Assert.AreEqual(start_he, old_he);
        }

        [Test]
        public void CanRemoveFace()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3
            pMesh.Vertices.Add(2, 0, 0); // 4
            pMesh.Vertices.Add(2, 1, 0); // 5

            // Create two quadrangular faces
            pMesh.Faces.AddFace(0, 1, 2, 3);
            pMesh.Faces.AddFace(1, 4, 5, 2);

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

            Assert.AreEqual(6, count);

            // Remove the second face
            pMesh.Faces.RemoveFace(1);

            // Count again...
            count = 0;
            he_first = 1;
            he_current = he_first;
            do
            {
                count++;
                he_current = pMesh.Halfedges[he_current].NextHalfedge;
            }
            while (he_current != he_first);

            Assert.AreEqual(4, count);
        }

        [Test]
        public void CanCompact()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create two triangular faces
            pMesh.Faces.AddFace(0, 1, 2);
            pMesh.Faces.AddFace(2, 3, 0);

            // Merge faces and compact (squashing face #0)
            pMesh.Faces.MergeFaces(4);
            pMesh.Faces.CompactHelper();

            // Check some things about the compacted mesh
            Assert.AreEqual(1, pMesh.Faces.Count);
            Assert.AreEqual(new int[] { 0, 1, 2, 3 }, pMesh.Faces.GetFaceVertices(0));
        }
        
        [Test]
        public void CanTraverseUnusedFace()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            
            // Add a single unset face
            pMesh.Faces.Add(PlanktonFace.Unset);
            
            Assert.IsEmpty(pMesh.Faces.GetHalfedges(0));
        }

        [Test]
        public void CanAddManyFaces()
        {
            PlanktonMesh pMesh = new PlanktonMesh();

            // Create one vertex for each corner of a square
            pMesh.Vertices.Add(0, 0, 0); // 0
            pMesh.Vertices.Add(1, 0, 0); // 1
            pMesh.Vertices.Add(1, 1, 0); // 2
            pMesh.Vertices.Add(0, 1, 0); // 3

            // Create two triangular faces
            var faces = new int[][]
            {
                new int[] { 0, 1, 2 },
                new int[] { 3, 2, 1 }
            };
            var retval = pMesh.Faces.AddFaces(faces);

            Assert.AreEqual(new int[] { 0, 1 }, retval);
            Assert.AreEqual(2, pMesh.Faces.Count);
            Assert.AreEqual(faces[1], pMesh.Faces.GetFaceVertices(1));
        }

        [Test]
        public void CanDeleteFaceAndAddFace()
        {
          /*

          0 - 4 - 8 - 12
          | 0 | 3 | 6 |
          1 - 5 - 9 - 13
          | 1 | 4 | 7 |
          2 - 6 - 10- 14
          | 2 | 5 | 8 |
          3 - 7 - 11- 15

          */

          PlanktonMesh pMesh = new PlanktonMesh();

          // Create 4x4 grid of vertices
          pMesh.Vertices.Add(0, 3, 0); //  0
          pMesh.Vertices.Add(0, 2, 0); //  1
          pMesh.Vertices.Add(0, 1, 0); //  2
          pMesh.Vertices.Add(0, 0, 0); //  3
          pMesh.Vertices.Add(1, 3, 0); //  4
          pMesh.Vertices.Add(1, 2, 0); //  5
          pMesh.Vertices.Add(1, 1, 0); //  6
          pMesh.Vertices.Add(1, 0, 0); //  7
          pMesh.Vertices.Add(2, 3, 0); //  8
          pMesh.Vertices.Add(2, 2, 0); //  9
          pMesh.Vertices.Add(2, 1, 0); // 10
          pMesh.Vertices.Add(2, 0, 0); // 11
          pMesh.Vertices.Add(3, 3, 0); // 12
          pMesh.Vertices.Add(3, 2, 0); // 13
          pMesh.Vertices.Add(3, 1, 0); // 14
          pMesh.Vertices.Add(3, 0, 0); // 15

          // Create nine quadrangular faces
          pMesh.Faces.AddFace(0, 1, 5, 4);     // 0
          pMesh.Faces.AddFace(1, 2, 6, 5);     // 1
          pMesh.Faces.AddFace(2, 3, 7, 6);     // 2
          pMesh.Faces.AddFace(4, 5, 9, 8);     // 3
          pMesh.Faces.AddFace(5, 6, 10, 9);    // 4
          pMesh.Faces.AddFace(6, 7, 11, 10);   // 5
          pMesh.Faces.AddFace(8, 9, 13, 12);   // 6
          pMesh.Faces.AddFace(9, 10, 14, 13);  // 7
          pMesh.Faces.AddFace(10, 11, 15, 14); // 8

          int id = 4; // center face

          // Get old face info
          var faceCirculator = pMesh.Halfedges.GetFaceCirculator(pMesh.Faces[id].FirstHalfedge);
          var faceVerts = new List<int>();
          foreach (var i in faceCirculator)
          {
            faceVerts.Add(pMesh.Halfedges[i].StartVertex);
          }

          // All face vertices should be internal (not boundary)
          foreach (int i in faceVerts)
          {
            Assert.IsFalse(pMesh.Vertices.IsBoundary(i));
          }

          // Delete old face
          pMesh.Faces.RemoveFace(id);

          // All face vertices should now be on a boundary
          foreach (int i in faceVerts)
          {
            Assert.IsTrue(pMesh.Vertices.IsBoundary(i));
          }

          // pMesh.Compact();

          // Re-add face
          var res = pMesh.Faces.AddFace(faceVerts);
          Assert.AreNotEqual(-1, res);
          Assert.AreEqual(9, res); // res == 8 if mesh compacted beforehand
        }
    }
}
