namespace System.Windows.Forms
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal static class SecurityUtils
    {
        private static ReflectionPermission memberAccessPermission;
        private static ReflectionPermission restrictedMemberAccessPermission;

        [SecuritySafeCritical]
        private static void DemandGrantSet(Assembly assembly)
        {
            PermissionSet permissionSet = assembly.PermissionSet;
            permissionSet.AddPermission(RestrictedMemberAccessPermission);
            permissionSet.Demand();
        }

        private static void DemandReflectionAccess(System.Type type)
        {
            try
            {
                MemberAccessPermission.Demand();
            }
            catch (SecurityException)
            {
                DemandGrantSet(type.Assembly);
            }
        }

        private static bool HasReflectionPermission(System.Type type)
        {
            try
            {
                DemandReflectionAccess(type);
                return true;
            }
            catch (SecurityException)
            {
            }
            return false;
        }

        internal static object SecureCreateInstance(System.Type type)
        {
            return SecureCreateInstance(type, null, false);
        }

        internal static object SecureCreateInstance(System.Type type, object[] args, bool allowNonPublic)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            BindingFlags bindingAttr = BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance;
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            else if (allowNonPublic && !HasReflectionPermission(type))
            {
                allowNonPublic = false;
            }
            if (allowNonPublic)
            {
                bindingAttr |= BindingFlags.NonPublic;
            }
            return Activator.CreateInstance(type, bindingAttr, null, args, null);
        }

        private static ReflectionPermission MemberAccessPermission
        {
            get
            {
                if (memberAccessPermission == null)
                {
                    memberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.MemberAccess);
                }
                return memberAccessPermission;
            }
        }

        private static ReflectionPermission RestrictedMemberAccessPermission
        {
            get
            {
                if (restrictedMemberAccessPermission == null)
                {
                    restrictedMemberAccessPermission = new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess);
                }
                return restrictedMemberAccessPermission;
            }
        }
    }
}

