using System;
using System.Collections.Generic;
using System.Text;

namespace Peppermint.DataBinding
{
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 item1, T2 item2)
        {
            return new Tuple<T1, T2>(item1, item2);
        }

        public static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }
    }

    public class Tuple<T1, T2> : IComparable
    {
        private readonly T1 item1;
        private readonly T2 item2;

        public T1 Item1 { get { return item1; } }
        public T2 Item2 { get { return item2; } }

        public Tuple(T1 item1, T2 item2)
        {
            this.item1 = item1;
            this.item2 = item2;
        }

        public override bool Equals(object other)
        {
            if (other == null)
            {
                return false;
            }

            var objTuple = other as Tuple<T1, T2>;
            if (objTuple == null)
            {
                return false;
            }

            var comparer = EqualityComparer<object>.Default;
            return comparer.Equals(item1, objTuple.item1) && comparer.Equals(item2, objTuple.item2);
        }

        public int CompareTo(object other)
        {
            if (other == null)
            {
                return 1;
            }

            var objTuple = other as Tuple<T1, T2>;
            if (objTuple == null)
            {
                throw new ArgumentException("Incorrect type");
            }

            var comparer = Comparer<object>.Default;

            int c = comparer.Compare(item1, objTuple.item1);
            if (c != 0)
            {
                return c;
            }

            return comparer.Compare(item2, objTuple.item2);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<object>.Default;
            return Tuple.CombineHashCodes(comparer.GetHashCode(item1), comparer.GetHashCode(item2));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.Append("(");
            sb.Append(item1);
            sb.Append(", ");
            sb.Append(item2);
            sb.Append(")");

            return sb.ToString();
        }
    }
}
