using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plankton
{
    public struct PlanktonLine
    {
        internal PlanktonXYZ m_from;
        internal PlanktonXYZ m_to;
        public PlanktonXYZ From
        {
            get
            {
                return this.m_from;
            }
            set
            {
                this.m_from = value;
            }
        }
        public float FromX
        {
            get
            {
                return this.m_from.X;
            }
            set
            {
                this.m_from.X = value;
            }
        }
        public float FromY
        {
            get
            {
                return this.m_from.Y;
            }
            set
            {
                this.m_from.Y = value;
            }
        }
        public float FromZ
        {
            get
            {
                return this.m_from.Z;
            }
            set
            {
                this.m_from.Z = value;
            }
        }
        public PlanktonXYZ To
        {
            get
            {
                return this.m_to;
            }
            set
            {
                this.m_to = value;
            }
        }
        public float ToX
        {
            get
            {
                return this.m_to.X;
            }
            set
            {
                this.m_to.X = value;
            }
        }
        public float ToY
        {
            get
            {
                return this.m_to.Y;
            }
            set
            {
                this.m_to.Y = value;
            }
        }
        public float ToZ
        {
            get
            {
                return this.m_to.Z;
            }
            set
            {
                this.m_to.Z = value;
            }
        }
        public PlanktonLine(PlanktonXYZ from, PlanktonXYZ to)
        {
            this.m_from = from;
            this.m_to = to;
        }
        public PlanktonLine(float x0, float y0, float z0, float x1, float y1, float z1)
        {
            this.m_from = new PlanktonXYZ(x0, y0, z0);
            this.m_to = new PlanktonXYZ(x1, y1, z1);
        }
        public override string ToString()
        {
            return "[From]" + this.m_from.ToString() + "[to]" + this.m_to.ToString();
        }
        public float Length
        {
            get
            {
                return this.m_from.DistanceTo(this.m_to);
            }
        }
        public void Flip()
        {
            PlanktonXYZ from = this.From;
            this.From = this.To;
            this.To = from;
        }
        public PlanktonXYZ Direction()
        {
            return (m_to - m_from);
        }
        public PlanktonXYZ PointAt(float t)
        {
            float num = 1f - t;
            return new PlanktonXYZ((this.From.X == this.To.X) ? this.From.X : ((num * this.From.X) + (t * this.To.X)), (this.From.Y == this.To.Y) ? this.From.Y : ((num * this.From.Y) + (t * this.To.Y)), (this.From.Z == this.To.Z) ? this.From.Z : ((num * this.From.Z) + (t * this.To.Z)));
        }
        public float ClosestParameter(PlanktonXYZ point)
        {
            float t = 0;
            PlanktonXYZ D = m_to - m_from;
            float DoD = (D.X * D.X + D.Y * D.Y + D.Z * D.Z);
            if (DoD > 0f)
            {
                if (point.DistanceTo(m_from) <= point.DistanceTo(m_to))
                {
                    t = ((point - m_from) * D) / DoD;
                }
                else {
                    t = 1f + ((point - m_to) * D) / DoD;
                }
            }
            else {
                t = 0f;
            }
            return t;
        }
        public PlanktonXYZ ClosestPoint(PlanktonXYZ testPoint, bool limitToFiniteSegment)
        {
            float num = this.ClosestParameter(testPoint);
            if (limitToFiniteSegment)
            {
                num = Math.Min(Math.Max(num, 0f), 1f);
            }
            return this.PointAt(num);
        }
        private bool ClosestPointTo(PlanktonXYZ point, ref float t)
        {
            bool rc = false;
            PlanktonXYZ D = m_to - m_from;
            float DoD = (D.X * D.X + D.Y * D.Y + D.Z * D.Z);
            if (DoD > 0f)
            {
                if (point.DistanceTo(m_from) <= point.DistanceTo(m_to))
                {
                    t = ((point - m_from) * D) / DoD;
                }
                else {
                    t = 1f + ((point - m_to) * D) / DoD;
                }
                rc = true;
            }
            return rc;
        }
        public float DistanceTo(PlanktonXYZ testPoint, bool limitToFiniteSegment)
        {
            return this.ClosestPoint(testPoint, limitToFiniteSegment).DistanceTo(testPoint);
        }
        public float MinimumDistanceTo(PlanktonXYZ P)
        {
            float d = 0, t = 0;
            if (ClosestPointTo(P, ref t))
            {
                if (t < 0f) t = 0f; else if (t > 1f) t = 1f;
                d = PointAt(t).DistanceTo(P);
            }
            else
            {
                // degenerate line
                d = m_from.DistanceTo(P);
                t = m_to.DistanceTo(P);
                if (t < d)
                    d = t;
            }
            return d;
        }
        public float MinimumDistanceTo(PlanktonLine L)
        {
            PlanktonXYZ A, B;
            float a, b, t = 0, x = 0, d = 0;
            bool bCheckA, bCheckB;
            bool bGoodX = ON_Intersect(this, L, out a, out b);
            bCheckA = true;
            if (a < 0f) a = 0f; else if (a > 1f) a = 1f; else bCheckA = !bGoodX;
            bCheckB = true;
            if (b < 0f) b = 0f; else if (b > 1f) b = 1f; else bCheckB = !bGoodX;
            A = PointAt(a);
            B = L.PointAt(b);
            d = A.DistanceTo(B);
            if (bCheckA)
            {
                L.ClosestPointTo(A, ref t);
                if (t < 0f) t = 0f; else if (t > 1f) t = 1f;
                x = L.PointAt(t).DistanceTo(A);
                if (x < d)
                    d = x;
            }
            if (bCheckB)
            {
                ClosestPointTo(B, ref t);
                if (t < 0f) t = 0f; else if (t > 1f) t = 1f;
                x = PointAt(t).DistanceTo(B);
                if (x < d)
                    d = x;
            }
            return d;
        }
        public float MaximumDistanceTo(PlanktonXYZ P)
        {
            float a, b;
            a = m_from.DistanceTo(P);
            b = m_to.DistanceTo(P);
            return ((a < b) ? b : a);
        }
        public float MaximumDistanceTo(PlanktonLine L)
        {
            float a, b;
            a = MaximumDistanceTo(L.m_from);
            b = MaximumDistanceTo(L.m_to);
            return ((a < b) ? b : a);
        }
        public override bool Equals(object obj)
        {
            return ((obj is PlanktonLine) && (this == ((PlanktonLine)obj)));
        }
        public bool Equals(PlanktonLine other)
        {
            return (this == other);
        }
        public static bool operator ==(PlanktonLine a, PlanktonLine b)
        {
            return ((a.From == b.From) && (a.To == b.To));
        }
        public static bool operator !=(PlanktonLine a, PlanktonLine b)
        {
            if (!(a.From != b.From))
            {
                return (a.To != b.To);
            }
            return true;
        }

        public PlanktonXYZ this[int i]
        {
            get
            {
                return (i <= 0) ? m_from : m_to;
            }
        }
        public override int GetHashCode()
        {
            return (this.From.GetHashCode() ^ this.To.GetHashCode());
        }
        static bool ON_Intersect(PlanktonLine lineA, PlanktonLine lineB, out float lineA_parameter, out float lineB_parameter)
        {
            bool rc = true;
            if (lineA.m_from == lineB.m_from)
            {
                lineA_parameter = 0f;
                lineB_parameter = 0f;
            }
            else if (lineA.m_from == lineB.m_to)
            {
                lineA_parameter = 0f;
                lineB_parameter = 1f;
            }
            else if (lineA.m_to == lineB.m_from)
            {
                lineA_parameter = 1f;
                lineB_parameter = 0f;
            }
            else if (lineA.m_to == lineB.m_to)
            {
                lineA_parameter = 1f;
                lineB_parameter = 1f;
            }
            else
            {
                lineA_parameter = 0f;
                lineB_parameter = 0f;
                ////计算
            }
            return rc;

        }

    }
}