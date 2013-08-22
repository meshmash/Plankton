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
    }
}
