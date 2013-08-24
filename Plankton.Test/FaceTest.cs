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
            int new_face = pMesh.Faces.SplitFace(0, 2);
            
            // Traverse from new face to old face, via new halfedges
            int new_he_pair = pMesh.Faces[new_face].FirstHalfedge;
            int new_he = pMesh.Halfedges.PairHalfedge(new_he_pair);
            int old_face = pMesh.Halfedges[new_he].AdjacentFace;
            
            Assert.AreEqual(0, old_face);
            
            // Check that both faces are now triangular
            Assert.AreEqual(3, pMesh.Faces.GetFaceVertices(0).Length);
            Assert.AreEqual(3, pMesh.Faces.GetFaceVertices(1).Length);
            
            // Check the halfedges of each face
            Assert.AreEqual(new int[] { 8, 4, 6 }, pMesh.Faces.GetHalfedges(old_face));
            Assert.AreEqual(new int[] { 9, 0, 2 }, pMesh.Faces.GetHalfedges(new_face));
        }
    }
}
