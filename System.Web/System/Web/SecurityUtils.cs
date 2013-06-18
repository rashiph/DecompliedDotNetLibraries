namespace System.Web
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal static class SecurityUtils
    {
        private static ReflectionPermission memberAccessPermission;
        private static ReflectionPermission restrictedMemberAccessPermission;

        internal static object ArrayCreateInstance(Type type, int length)
        {
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            return Array.CreateInstance(type, length);
        }

        internal static object ConstructorInfoInvoke(ConstructorInfo ctor, object[] args)
        {
            Type declaringType = ctor.DeclaringType;
            if ((declaringType != null) && (!declaringType.IsVisible || !ctor.IsPublic))
            {
                DemandReflectionAccess(declaringType);
            }
            return ctor.Invoke(args);
        }

        [SecuritySafeCritical]
        private static void DemandGrantSet(Assembly assembly)
        {
            PermissionSet permissionSet = assembly.PermissionSet;
            permissionSet.AddPermission(RestrictedMemberAccessPermission);
            permissionSet.Demand();
        }

        private static void DemandReflectionAccess(Type type)
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

        internal static object FieldInfoGetValue(FieldInfo field, object target)
        {
            Type declaringType = field.DeclaringType;
            if (declaringType == null)
            {
                if (!field.IsPublic)
                {
                    DemandGrantSet(field.Module.Assembly);
                }
            }
            else if (((declaringType == null) || !declaringType.IsVisible) || !field.IsPublic)
            {
                DemandReflectionAccess(declaringType);
            }
            return field.GetValue(target);
        }

        private static bool GenericArgumentsAreVisible(MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                foreach (Type type in method.GetGenericArguments())
                {
                    if (!type.IsVisible)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool HasReflectionPermission(Type type)
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

        internal static object MethodInfoInvoke(MethodInfo method, object target, object[] args)
        {
            Type declaringType = method.DeclaringType;
            if (declaringType == null)
            {
                if (!method.IsPublic || !GenericArgumentsAreVisible(method))
                {
                    DemandGrantSet(method.Module.Assembly);
                }
            }
            else if ((!declaringType.IsVisible || !method.IsPublic) || !GenericArgumentsAreVisible(method))
            {
                DemandReflectionAccess(declaringType);
            }
            return method.Invoke(target, args);
        }

        internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic)
        {
            return SecureConstructorInvoke(type, argTypes, args, allowNonPublic, BindingFlags.Default);
        }

        internal static object SecureConstructorInvoke(Type type, Type[] argTypes, object[] args, bool allowNonPublic, BindingFlags extraFlags)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsVisible)
            {
                DemandReflectionAccess(type);
            }
            else if (allowNonPublic && !HasReflectionPermission(type))
            {
                allowNonPublic = false;
            }
            BindingFlags bindingAttr = (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance) | extraFlags;
            if (!allowNonPublic)
            {
                bindingAttr &= ~BindingFlags.NonPublic;
            }
            ConstructorInfo info = type.GetConstructor(bindingAttr, null, argTypes, null);
            if (info != null)
            {
                return info.Invoke(args);
            }
            return null;
        }

        internal static object SecureCreateInstance(Type type)
        {
            return SecureCreateInstance(type, null, false);
        }

        internal static object SecureCreateInstance(Type type, object[] args)
        {
            return SecureCreateInstance(type, args, false);
        }

        internal static object SecureCreateInstance(Type type, object[] args, bool allowNonPublic)
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

