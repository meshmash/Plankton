using Plankton;
using NUnit.Framework;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Plankton.Test
{
    [TestFixture]
    public class MeshTest
    {
        [Test]
        public void CanCreateFromFaceVertex()
        {
            // Create a simple cube
            
            var pts = new Point3d[]
            {
                new Point3d(-0.5, -0.5, 0.5),
                new Point3d(-0.5, -0.5, -0.5),
                new Point3d(-0.5, 0.5, -0.5),
                new Point3d(-0.5, 0.5, 0.5),
                new Point3d(0.5, -0.5, 0.5),
                new Point3d(0.5, -0.5, -0.5),
                new Point3d(0.5, 0.5, -0.5),
                new Point3d(0.5, 0.5, 0.5)
            };
            
            var fs = new int[][]
            {
                new int[]{ 3, 2, 1, 0 },
                new int[]{ 1, 5, 4, 0 },
                new int[]{ 2, 6, 5, 1 },
                new int[]{ 7, 6, 2, 3 },
                new int[]{ 4, 7, 3, 0 },
                new int[]{ 5, 6, 7, 4 }
            };
            
            // Create a PlanktonMesh from the points and face-vertex indices
            
            PlanktonMesh pMesh = new PlanktonMesh(pts, fs);
            
            Assert.AreEqual(24, pMesh.Halfedges.Count);
            
            // Check that half-edges have been linked up correctly
            
            // Get all outgoing halfedges from vertex #0 and compare against expected
            List<int> vertexZeroHalfedges = pMesh.VertexAllOutHE(0);
            int[] vertexZeroHalfedgesExpected = new int[]{ 13, 5, 6 };
            Assert.AreEqual(3, vertexZeroHalfedges.Count);
            foreach (int halfedge in vertexZeroHalfedgesExpected)
            {
                Assert.Contains(halfedge, vertexZeroHalfedges);
            }
            
            // Get all halfedges from face #2 and compare against expected
            List<int> faceTwoHalfedges = pMesh.FaceHEs(2);
            int[] faceTwoHalfedgesExpected = new int[]{ 14, 16, 9, 3 };
            Assert.AreEqual(4, faceTwoHalfedges.Count);
            foreach (int halfedge in faceTwoHalfedgesExpected)
            {
                Assert.Contains(halfedge, faceTwoHalfedges);
            }
            
            // Get all faces from vertex #1 and compare against expected
            List<int> vertexOneFaces = pMesh.VertexFaces(1);
            int[] vertexOneFacesExpected = new int[]{ 0, 1, 2 };
            Assert.AreEqual(3, vertexOneFaces.Count);
            foreach (int face in vertexOneFacesExpected)
            {
                Assert.Contains(face, vertexOneFaces);
            }
        }
    }
}
