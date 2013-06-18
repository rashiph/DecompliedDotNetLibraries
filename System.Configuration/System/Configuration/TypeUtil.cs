namespace System.Configuration
{
    using System;
    using System.Configuration.Internal;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    internal static class TypeUtil
    {
        private static PermissionSet s_fullTrustPermissionSet;

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static object CreateInstanceWithReflectionPermission(string typeString)
        {
            return Activator.CreateInstance(GetTypeImpl(typeString, true), true);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static object CreateInstanceWithReflectionPermission(Type type)
        {
            return Activator.CreateInstance(type, true);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.NoFlags)]
        internal static ConstructorInfo GetConstructorWithReflectionPermission(Type type, Type baseType, bool throwOnError)
        {
            type = VerifyAssignableType(baseType, type, throwOnError);
            if (type == null)
            {
                return null;
            }
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            ConstructorInfo info = type.GetConstructor(bindingAttr, null, CallingConventions.HasThis, Type.EmptyTypes, null);
            if ((info == null) && throwOnError)
            {
                throw new TypeLoadException(System.Configuration.SR.GetString("TypeNotPublic", new object[] { type.AssemblyQualifiedName }));
            }
            return info;
        }

        private static Type GetLegacyType(string typeString)
        {
            Type type = null;
            try
            {
                type = typeof(ConfigurationException).Assembly.GetType(typeString, false);
            }
            catch
            {
            }
            return type;
        }

        private static Type GetTypeImpl(string typeString, bool throwOnError)
        {
            Type legacyType = null;
            Exception exception = null;
            try
            {
                legacyType = Type.GetType(typeString, throwOnError);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            if (legacyType == null)
            {
                legacyType = GetLegacyType(typeString);
                if ((legacyType == null) && (exception != null))
                {
                    throw exception;
                }
            }
            return legacyType;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.NoFlags)]
        internal static Type GetTypeWithReflectionPermission(string typeString, bool throwOnError)
        {
            return GetTypeImpl(typeString, throwOnError);
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.NoFlags)]
        internal static Type GetTypeWithReflectionPermission(IInternalConfigHost host, string typeString, bool throwOnError)
        {
            Type configType = null;
            Exception exception = null;
            try
            {
                configType = host.GetConfigType(typeString, throwOnError);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            if (configType == null)
            {
                configType = GetLegacyType(typeString);
                if ((configType == null) && (exception != null))
                {
                    throw exception;
                }
            }
            return configType;
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        private static bool HasAptcaBit(Assembly assembly)
        {
            object[] customAttributes = assembly.GetCustomAttributes(typeof(AllowPartiallyTrustedCallersAttribute), false);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        [ReflectionPermission(SecurityAction.Assert, Flags=ReflectionPermissionFlag.MemberAccess)]
        internal static object InvokeCtorWithReflectionPermission(ConstructorInfo ctor)
        {
            return ctor.Invoke(null);
        }

        internal static bool IsTypeAllowedInConfig(Type t)
        {
            if (IsCallerFullTrust)
            {
                return true;
            }
            Assembly assembly = t.Assembly;
            return (!assembly.GlobalAssemblyCache || HasAptcaBit(assembly));
        }

        internal static bool IsTypeFromTrustedAssemblyWithoutAptca(Type type)
        {
            Assembly assembly = type.Assembly;
            return (assembly.GlobalAssemblyCache && !HasAptcaBit(assembly));
        }

        internal static Type VerifyAssignableType(Type baseType, Type type, bool throwOnError)
        {
            if (baseType.IsAssignableFrom(type))
            {
                return type;
            }
            if (throwOnError)
            {
                throw new TypeLoadException(System.Configuration.SR.GetString("Config_type_doesnt_inherit_from_type", new object[] { type.FullName, baseType.FullName }));
            }
            return null;
        }

        internal static bool IsCallerFullTrust
        {
            get
            {
                bool flag = false;
                try
                {
                    if (s_fullTrustPermissionSet == null)
                    {
                        s_fullTrustPermissionSet = new PermissionSet(PermissionState.Unrestricted);
                    }
                    s_fullTrustPermissionSet.Demand();
                    flag = true;
                }
                catch
                {
                }
                return flag;
            }
        }
    }
}

