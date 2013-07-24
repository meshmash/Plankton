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
            int[] vertexZeroHalfedges = pMesh.Vertices.GetHalfedges(0);
            int[] vertexZeroHalfedgesExpected = new int[]{ 13, 5, 6 };
            Assert.AreEqual(3, vertexZeroHalfedges.Length);
            foreach (int halfedge in vertexZeroHalfedgesExpected)
            {
                Assert.Contains(halfedge, vertexZeroHalfedges);
            }
            // Check that none of these edges are on a boundary (closed mesh)
            Assert.AreEqual(0, pMesh.Vertices.NakedEdgeCount(0));
            
            // Get all halfedges from face #2 and compare against expected
            int[] faceTwoHalfedges = pMesh.Faces.GetHalfedges(2);
            int[] faceTwoHalfedgesExpected = new int[]{ 14, 16, 9, 3 };
            Assert.AreEqual(4, faceTwoHalfedges.Length);
            foreach (int halfedge in faceTwoHalfedgesExpected)
            {
                Assert.Contains(halfedge, faceTwoHalfedges);
            }
            // Check that none of these edges are on a boundary (closed mesh)
            Assert.AreEqual(0, pMesh.Faces.NakedEdgeCount(2));
            
            // Get all vertices from face #4 and compare against expected
            int[] faceFourVertices = pMesh.Faces.GetVertices(4);
            int[] faceFourVerticesExpected = new int[]{ 4, 7, 3, 0 };
            Assert.AreEqual(faceFourVerticesExpected, faceFourVertices);
            
            // Get all faces from vertex #1 and compare against expected
            int[] vertexOneFaces = pMesh.Vertices.GetVertexFaces(1);
            int[] vertexOneFacesExpected = new int[]{ 0, 1, 2 };
            Assert.AreEqual(3, vertexOneFaces.Length);
            foreach (int face in vertexOneFacesExpected)
            {
                Assert.Contains(face, vertexOneFaces);
            }
            
            // Get all vertex neighbours from vertex #0 and compare against expected
            int[] vertexZeroNeighbours = pMesh.Vertices.GetVertexNeighbours(0);
            int[] vertexZeroNeighboursExpected = new int[]{ 4, 1, 3 };
            Assert.AreEqual(3, vertexZeroNeighbours.Length);
            foreach (int vertex in vertexZeroNeighboursExpected)
            {
                Assert.Contains(vertex, vertexZeroNeighbours);
            }
        }
    }
}
