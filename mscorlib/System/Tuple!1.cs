namespace System
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    [Serializable]
    public class Tuple<T1> : IStructuralEquatable, IStructuralComparable, IComparable, ITuple
    {
        private readonly T1 m_Item1;

        public Tuple(T1 item1)
        {
            this.m_Item1 = item1;
        }

        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable) this).Equals(obj, EqualityComparer<object>.Default);
        }

        public override int GetHashCode()
        {
            return ((IStructuralEquatable) this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }
            Tuple<T1> tuple = other as Tuple<T1>;
            if (tuple == null)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentException_TupleIncorrectType", new object[] { base.GetType().ToString() }), "other");
            }
            return comparer.Compare(this.m_Item1, tuple.m_Item1);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }
            Tuple<T1> tuple = other as Tuple<T1>;
            if (tuple == null)
            {
                return false;
            }
            return comparer.Equals(this.m_Item1, tuple.m_Item1);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            return comparer.GetHashCode(this.m_Item1);
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable) this).CompareTo(obj, Comparer<object>.Default);
        }

        int ITuple.GetHashCode(IEqualityComparer comparer)
        {
            return ((IStructuralEquatable) this).GetHashCode(comparer);
        }

        string ITuple.ToString(StringBuilder sb)
        {
            sb.Append(this.m_Item1);
            sb.Append(")");
            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            return ((ITuple) this).ToString(sb);
        }

        public T1 Item1
        {
            get
            {
                return this.m_Item1;
            }
        }

        int ITuple.Size
        {
            get
            {
                return 1;
            }
        }
    }
}

