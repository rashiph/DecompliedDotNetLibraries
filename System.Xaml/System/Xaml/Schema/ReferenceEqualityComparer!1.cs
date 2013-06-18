namespace System.Xaml.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class ReferenceEqualityComparer<T> : EqualityComparer<T> where T: class
    {
        internal static ReferenceEqualityComparer<T> Singleton;

        static ReferenceEqualityComparer()
        {
            ReferenceEqualityComparer<T>.Singleton = new ReferenceEqualityComparer<T>();
        }

        public override bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        public override int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

