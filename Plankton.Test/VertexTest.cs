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
        public void CanFindHalfedgeNakedVertex()
        {
            PlanktonMesh pMesh = new PlanktonMesh();
            pMesh.Vertices.Add(0, 0, 0);
            pMesh.Vertices.Add(1, 1, 1);
            Assert.AreEqual(-1, pMesh.Halfedges.FindHalfedge(0, 1));
        }
    }
}
