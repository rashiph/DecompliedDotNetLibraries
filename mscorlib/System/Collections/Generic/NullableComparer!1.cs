namespace System.Collections.Generic
{
    using System;

    [Serializable]
    internal class NullableComparer<T> : Comparer<T?> where T: struct, IComparable<T>
    {
        public override int Compare(T? x, T? y)
        {
            if (x.HasValue)
            {
                if (y.HasValue)
                {
                    return x.value.CompareTo(y.value);
                }
                return 1;
            }
            if (y.HasValue)
            {
                return -1;
            }
            return 0;
        }

        public override bool Equals(object obj)
        {
            NullableComparer<T> comparer = obj as NullableComparer<T>;
            return (comparer != null);
        }

        public override int GetHashCode()
        {
            return base.GetType().Name.GetHashCode();
        }
    }
}

