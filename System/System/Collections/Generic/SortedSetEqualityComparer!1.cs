namespace System.Collections.Generic
{
    using System;

    internal class SortedSetEqualityComparer<T> : IEqualityComparer<SortedSet<T>>
    {
        private IComparer<T> comparer;
        private IEqualityComparer<T> e_comparer;

        public SortedSetEqualityComparer() : this(null, null)
        {
        }

        public SortedSetEqualityComparer(IComparer<T> comparer) : this(comparer, null)
        {
        }

        public SortedSetEqualityComparer(IEqualityComparer<T> memberEqualityComparer) : this(null, memberEqualityComparer)
        {
        }

        public SortedSetEqualityComparer(IComparer<T> comparer, IEqualityComparer<T> memberEqualityComparer)
        {
            if (comparer == null)
            {
                this.comparer = Comparer<T>.Default;
            }
            else
            {
                this.comparer = comparer;
            }
            if (memberEqualityComparer == null)
            {
                this.e_comparer = EqualityComparer<T>.Default;
            }
            else
            {
                this.e_comparer = memberEqualityComparer;
            }
        }

        public override bool Equals(object obj)
        {
            SortedSetEqualityComparer<T> comparer = obj as SortedSetEqualityComparer<T>;
            if (comparer == null)
            {
                return false;
            }
            return (this.comparer == comparer.comparer);
        }

        public bool Equals(SortedSet<T> x, SortedSet<T> y)
        {
            return SortedSet<T>.SortedSetEquals(x, y, this.comparer);
        }

        public override int GetHashCode()
        {
            return (this.comparer.GetHashCode() ^ this.e_comparer.GetHashCode());
        }

        public int GetHashCode(SortedSet<T> obj)
        {
            int num = 0;
            if (obj != null)
            {
                foreach (T local in obj)
                {
                    num ^= this.e_comparer.GetHashCode(local) & 0x7fffffff;
                }
            }
            return num;
        }
    }
}

