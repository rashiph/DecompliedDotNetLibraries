namespace System.Linq.Parallel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WrapperEqualityComparer<T> : IEqualityComparer<Wrapper<T>>
    {
        private IEqualityComparer<T> m_comparer;
        internal WrapperEqualityComparer(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
            {
                this.m_comparer = EqualityComparer<T>.Default;
            }
            else
            {
                this.m_comparer = comparer;
            }
        }

        public bool Equals(Wrapper<T> x, Wrapper<T> y)
        {
            return this.m_comparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(Wrapper<T> x)
        {
            return this.m_comparer.GetHashCode(x.Value);
        }
    }
}

