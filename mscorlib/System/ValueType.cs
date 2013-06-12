namespace System
{
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public abstract class ValueType
    {
        protected ValueType()
        {
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern bool CanCompareBits(object obj);
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            RuntimeType type = (RuntimeType) base.GetType();
            RuntimeType type2 = (RuntimeType) obj.GetType();
            if (type2 != type)
            {
                return false;
            }
            object a = this;
            if (CanCompareBits(this))
            {
                return FastEqualsCheck(a, obj);
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                object obj3 = ((RtFieldInfo) fields[i]).InternalGetValue(a, false);
                object obj4 = ((RtFieldInfo) fields[i]).InternalGetValue(obj, false);
                if (obj3 == null)
                {
                    if (obj4 != null)
                    {
                        return false;
                    }
                }
                else if (!obj3.Equals(obj4))
                {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        private static extern bool FastEqualsCheck(object a, object b);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public override extern int GetHashCode();
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern int GetHashCodeOfPtr(IntPtr ptr);
        public override string ToString()
        {
            return base.GetType().ToString();
        }
    }
}

