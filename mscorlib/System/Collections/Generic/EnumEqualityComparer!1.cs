namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    [Serializable]
    internal sealed class EnumEqualityComparer<T> : EqualityComparer<T> where T: struct
    {
        public override bool Equals(object obj)
        {
            EnumEqualityComparer<T> comparer = obj as EnumEqualityComparer<T>;
            return (comparer != null);
        }

        [SecuritySafeCritical]
        public override bool Equals(T x, T y)
        {
            int num = JitHelpers.UnsafeEnumCast<T>(x);
            int num2 = JitHelpers.UnsafeEnumCast<T>(y);
            return (num == num2);
        }

        public override int GetHashCode()
        {
            return base.GetType().Name.GetHashCode();
        }

        [SecuritySafeCritical]
        public override int GetHashCode(T obj)
        {
            return JitHelpers.UnsafeEnumCast<T>(obj).GetHashCode();
        }
    }
}

