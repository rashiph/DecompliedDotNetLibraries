namespace System.Web.UI
{
    using System;
    using System.Collections;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public sealed class SupportsEventValidationAttribute : Attribute
    {
        private static Hashtable _typesSupportsEventValidation = Hashtable.Synchronized(new Hashtable());

        internal static bool SupportsEventValidation(Type type)
        {
            object obj2 = _typesSupportsEventValidation[type];
            if (obj2 != null)
            {
                return (bool) obj2;
            }
            object[] customAttributes = type.GetCustomAttributes(typeof(SupportsEventValidationAttribute), false);
            bool flag = (customAttributes != null) && (customAttributes.Length > 0);
            _typesSupportsEventValidation[type] = flag;
            return flag;
        }
    }
}

