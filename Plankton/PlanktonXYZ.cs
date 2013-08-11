using System;

namespace Plankton
{
    /// <summary>
    /// Represents a vector in Euclidean space.
    /// </summary>
    public struct PlanktonXYZ
    {
        #region members
        internal float _x;
        internal float _y;
        internal float _z;
        #endregion

        #region constructors
        /// <summary>
        /// Constructs a new vector from 3 single precision numbers.
        /// </summary>
        /// <param name="x">X component of vector.</param>
        /// <param name="y">Y component of vector.</param>
        /// <param name="z">Z component of vector.</param>
        public PlanktonXYZ(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        #endregion

        #region static properties
        /// <summary>
        /// Gets the value of the vector with components 0,0,0.
        /// </summary>
        public static PlanktonXYZ Zero
        {
            get { return new PlanktonXYZ(); }
        }
        
        /// <summary>
        /// Gets the value of the vector with components 1,0,0.
        /// </summary>
        public static PlanktonXYZ XAxis
        {
            get { return new PlanktonXYZ(1f, 0f, 0f); }
        }
        
        /// <summary>
        /// Gets the value of the vector with components 0,1,0.
        /// </summary>
        public static PlanktonXYZ YAxis
        {
            get { return new PlanktonXYZ(0f, 1f, 0f); }
        }
        
        /// <summary>
        /// Gets the value of the vector with components 0,0,1.
        /// </summary>
        public static PlanktonXYZ ZAxis
        {
            get { return new PlanktonXYZ(0f, 0f, 1f); }
        }
        #endregion static properties

        #region properties
        /// <summary>
        /// Gets or sets the X (first) component of this vector.
        /// </summary>
        public float X { get { return _x; } set { _x = value; } }
        
        /// <summary>
        /// Gets or sets the Y (second) component of this vector.
        /// </summary>
        public float Y { get { return _y; } set { _y = value; } }
        
        /// <summary>
        /// Gets or sets the Z (third) component of this vector.
        /// </summary>
        public float Z { get { return _z; } set { _z = value; } }
        #endregion

        /// <summary>
        /// Computes a hash number that represents the current vector.
        /// </summary>
        /// <returns>A hash code that is not unique for each vector.</returns>
        public override int GetHashCode()
        {
            // MSDN docs recommend XOR'ing the internal values to get a hash code
            return _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode();
        }

        /// <summary>
        /// Sums up two vectors.
        /// </summary>
        /// <param name="v1">A vector.</param>
        /// <param name="v2">A second vector.</param>
        /// <returns>A new vector that results from the componentwise addition of the two vectors.</returns>
        public static PlanktonXYZ operator +(PlanktonXYZ v1, PlanktonXYZ v2)
        {
            return new PlanktonXYZ(v1._x + v2._x, v1._y + v2._y, v1._z + v2._z);
        }

        /// <summary>
        /// Multiplies a vector by a number, having the effect of scaling it.
        /// </summary>
        /// <param name="vector">A vector.</param>
        /// <param name="t">A number.</param>
        /// <returns>A new vector that is the original vector coordinatewise multiplied by t.</returns>
        public static PlanktonXYZ operator *(PlanktonXYZ vector, float t)
        {
            return new PlanktonXYZ(vector._x * t, vector._y * t, vector._z * t);
        }
    }
}

