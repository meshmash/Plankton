using System;

namespace Plankton
{
    /// <summary>
    /// Represents a face in Plankton's halfedge mesh data structure.
    /// </summary>
    public class PlanktonFace
    {
        public int FirstHalfedge;
        
        public PlanktonFace()
        {
            FirstHalfedge = -1;
        }
    }
}
