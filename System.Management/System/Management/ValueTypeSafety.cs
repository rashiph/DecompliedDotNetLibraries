namespace System.Management
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ValueTypeSafety
    {
        public static object GetSafeObject(object theValue)
        {
            if (theValue == null)
            {
                return null;
            }
            if (theValue.GetType().IsPrimitive)
            {
                return ((IConvertible) theValue).ToType(typeof(object), null);
            }
            return RuntimeHelpers.GetObjectValue(theValue);
        }
    }
}

