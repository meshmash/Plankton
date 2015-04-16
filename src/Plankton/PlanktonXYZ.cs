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
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="v1">A vector.</param>
        /// <param name="v2">A second vector.</param>
        /// <returns>The first vector minus the second vector</returns>
        public static PlanktonXYZ operator -(PlanktonXYZ v1, PlanktonXYZ v2)
        {
            return new PlanktonXYZ(v1._x - v2._x, v1._y - v2._y, v1._z - v2._z);
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

        /// <summary>
        /// Computes the cross product (or vector product, or exterior product) of two vectors.
        /// <para>This operation is not commutative.</para>
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>A new vector that is perpendicular to both a and b,
        /// <para>has Length == a.Length * b.Length and</para>
        /// <para>with a result that is oriented following the right hand rule.</para>
        /// </returns>
        public static PlanktonXYZ CrossProduct(PlanktonXYZ a, PlanktonXYZ b)
        {
            return new PlanktonXYZ(a._y * b._z - b._y * a._z, a._z * b._x - b._z * a._x, a._x * b._y - b._x * a._y);
        }

        /// <summary>
        /// Get the length of a vector
        /// </summary>        
        /// <returns>The length</returns>
        public float Length
        {
            get { return (float)Math.Sqrt(this._x * this._x + this._y * this._y + this._z * this._z); }
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}, {2}", this._x, this._y, this._z);
        }

        /// <summary>
        /// Determines whether two vectors have equal values.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>true if the components of the two vectors are exactly equal; otherwise false.</returns>
        public static bool operator ==(PlanktonXYZ a, PlanktonXYZ b)
        {
            return (a._x == b._x && a._y == b._y && a._z == b._z);
        }

        /// <summary>
        /// Determines whether two vectors have different values.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        /// <returns>true if the two vectors differ in any component; false otherwise.</returns>
        public static bool operator !=(PlanktonXYZ a, PlanktonXYZ b)
        {
            return (a._x != b._x || a._y != b._y || a._z != b._z);
        }

        /// <summary>
        /// Determines whether the specified System.Object is a Vector3f and has the same values as the present vector.
        /// </summary>
        /// <param name="obj">The specified object.</param>
        /// <returns>true if obj is Vector3f and has the same components as this; otherwise false.</returns>
        public override bool Equals(object obj)
        {
            return (obj is PlanktonXYZ && this == (PlanktonXYZ)obj);
        }

        /// <summary>
        /// Determines whether the specified vector has the same values as the present vector.
        /// </summary>
        /// <param name="vector">The specified vector.</param>
        /// <returns>true if vector has the same components as this; otherwise false.</returns>
        public bool Equals(PlanktonXYZ vector)
        {
            return this == vector;
        }
    }
}

