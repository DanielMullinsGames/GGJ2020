using System;

namespace Peppermint.DataBinding
{
    public struct IndexRange : IEquatable<IndexRange>
    {
        private int min;
        private int max;

        public int Min
        {
            get { return min; }
        }

        public int Max
        {
            get { return max; }
        }

        public IndexRange(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Equals(IndexRange other)
        {
            return min == other.min && max == other.max;
        }

        public override bool Equals(object obj)
        {
            if (obj is IndexRange)
            {
                IndexRange other = (IndexRange)obj;
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode();
        }

        public static bool operator ==(IndexRange lhs, IndexRange rhs)
        {
            return lhs.min == rhs.min && lhs.max == rhs.max;
        }

        public static bool operator !=(IndexRange lhs, IndexRange rhs)
        {
            return !(lhs == rhs);
        }

        public static IndexRange Empty = new IndexRange(-1, -1);
    }
}
