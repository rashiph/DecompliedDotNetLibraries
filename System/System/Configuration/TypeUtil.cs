namespace System.Configuration
{
    using System;
    using System.Security.Permissions;

    internal static class TypeUtil
    {
        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static object CreateInstanceWithReflectionPermission(string typeString)
        {
            return Activator.CreateInstance(Type.GetType(typeString, true), true);
        }
    }
}

