namespace System.Dynamic.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
    {
        internal static readonly ReferenceEqualityComparer<T> Instance;

        static ReferenceEqualityComparer()
        {
            ReferenceEqualityComparer<T>.Instance = new ReferenceEqualityComparer<T>();
        }

        private ReferenceEqualityComparer()
        {
        }

        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

