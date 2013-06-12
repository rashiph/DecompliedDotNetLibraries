namespace System
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public static class Nullable
    {
        [ComVisible(true)]
        public static int Compare<T>(T? n1, T? n2) where T: struct
        {
            if (n1.HasValue)
            {
                if (n2.HasValue)
                {
                    return Comparer<T>.Default.Compare(n1.value, n2.value);
                }
                return 1;
            }
            if (n2.HasValue)
            {
                return -1;
            }
            return 0;
        }

        [ComVisible(true)]
        public static bool Equals<T>(T? n1, T? n2) where T: struct
        {
            if (n1.HasValue)
            {
                return (n2.HasValue && EqualityComparer<T>.Default.Equals(n1.value, n2.value));
            }
            if (n2.HasValue)
            {
                return false;
            }
            return true;
        }

        public static Type GetUnderlyingType(Type nullableType)
        {
            if (nullableType == null)
            {
                throw new ArgumentNullException("nullableType");
            }
            Type type = null;
            if ((nullableType.IsGenericType && !nullableType.IsGenericTypeDefinition) && object.ReferenceEquals(nullableType.GetGenericTypeDefinition(), typeof(Nullable<>)))
            {
                type = nullableType.GetGenericArguments()[0];
            }
            return type;
        }
    }
}

