namespace System.Web.Services.Protocols
{
    using System;
    using System.Reflection;

    internal class MemberHelper
    {
        private static object[] emptyObjectArray = new object[0];

        private MemberHelper()
        {
        }

        internal static bool CanRead(MemberInfo memberInfo)
        {
            return ((memberInfo is FieldInfo) || ((PropertyInfo) memberInfo).CanRead);
        }

        internal static bool CanWrite(MemberInfo memberInfo)
        {
            return ((memberInfo is FieldInfo) || ((PropertyInfo) memberInfo).CanWrite);
        }

        internal static object GetValue(MemberInfo memberInfo, object target)
        {
            if (memberInfo is FieldInfo)
            {
                return ((FieldInfo) memberInfo).GetValue(target);
            }
            return ((PropertyInfo) memberInfo).GetValue(target, emptyObjectArray);
        }

        internal static bool IsStatic(MemberInfo memberInfo)
        {
            return ((memberInfo is FieldInfo) && ((FieldInfo) memberInfo).IsStatic);
        }

        internal static void SetValue(MemberInfo memberInfo, object target, object value)
        {
            if (memberInfo is FieldInfo)
            {
                ((FieldInfo) memberInfo).SetValue(target, value);
            }
            else
            {
                ((PropertyInfo) memberInfo).SetValue(target, value, emptyObjectArray);
            }
        }
    }
}

