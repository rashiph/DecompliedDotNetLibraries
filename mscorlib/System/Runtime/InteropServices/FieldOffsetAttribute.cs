namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Security;

    [ComVisible(true), AttributeUsage(AttributeTargets.Field, Inherited=false)]
    public sealed class FieldOffsetAttribute : Attribute
    {
        internal int _val;

        public FieldOffsetAttribute(int offset)
        {
            this._val = offset;
        }

        [SecurityCritical]
        internal static Attribute GetCustomAttribute(RuntimeFieldInfo field)
        {
            int num;
            if ((field.DeclaringType != null) && field.GetRuntimeModule().MetadataImport.GetFieldOffset(field.DeclaringType.MetadataToken, field.MetadataToken, out num))
            {
                return new FieldOffsetAttribute(num);
            }
            return null;
        }

        [SecurityCritical]
        internal static bool IsDefined(RuntimeFieldInfo field)
        {
            return (GetCustomAttribute(field) != null);
        }

        public int Value
        {
            get
            {
                return this._val;
            }
        }
    }
}

