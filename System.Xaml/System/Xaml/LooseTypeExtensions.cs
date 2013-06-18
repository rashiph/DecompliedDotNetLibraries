namespace System.Xaml
{
    using System;
    using System.Reflection;
    using System.Windows.Markup;

    internal static class LooseTypeExtensions
    {
        private const string WindowsBase = "WindowsBase";
        private static readonly byte[] WindowsBaseToken = new byte[] { 0x31, 0xbf, 0x38, 0x56, 0xad, 0x36, 0x4e, 0x35 };

        internal static bool AssemblyQualifiedNameEquals(Type t1, Type t2)
        {
            if (object.ReferenceEquals(t1, null))
            {
                return object.ReferenceEquals(t2, null);
            }
            if (object.ReferenceEquals(t2, null))
            {
                return false;
            }
            if (t1.FullName != t2.FullName)
            {
                return false;
            }
            if (t1.Assembly.FullName == t2.Assembly.FullName)
            {
                return true;
            }
            AssemblyName name = new AssemblyName(t1.Assembly.FullName);
            AssemblyName name2 = new AssemblyName(t2.Assembly.FullName);
            if (!(name.Name == name2.Name))
            {
                return IsWindowsBaseToSystemXamlComparison(t1.Assembly, t2.Assembly, name, name2);
            }
            return (name.CultureInfo.Equals(name2.CultureInfo) && SafeSecurityHelper.IsSameKeyToken(name.GetPublicKeyToken(), name2.GetPublicKeyToken()));
        }

        internal static bool IsAssemblyQualifiedNameAssignableFrom(Type t1, Type t2)
        {
            if ((t1 == null) || (t2 == null))
            {
                return false;
            }
            if (!AssemblyQualifiedNameEquals(t1, t2))
            {
                if (IsLooseSubClassOf(t2, t1))
                {
                    return true;
                }
                if (t1.IsInterface)
                {
                    return LooselyImplementInterface(t2, t1);
                }
                if (!t1.IsGenericParameter)
                {
                    return false;
                }
                Type[] genericParameterConstraints = t1.GetGenericParameterConstraints();
                for (int i = 0; i < genericParameterConstraints.Length; i++)
                {
                    if (!IsAssemblyQualifiedNameAssignableFrom(genericParameterConstraints[i], t2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool IsLooseSubClassOf(Type t1, Type t2)
        {
            if (((t1 != null) && (t2 != null)) && !AssemblyQualifiedNameEquals(t1, t2))
            {
                for (Type type = t1.BaseType; type != null; type = type.BaseType)
                {
                    if (AssemblyQualifiedNameEquals(type, t2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsWindowsBaseToSystemXamlComparison(Assembly a1, Assembly a2, AssemblyName name1, AssemblyName name2)
        {
            AssemblyName name = null;
            if ((name1.Name == "WindowsBase") && (a2 == typeof(MarkupExtension).Assembly))
            {
                name = name1;
            }
            else if ((name2.Name == "WindowsBase") && (a1 == typeof(MarkupExtension).Assembly))
            {
                name = name2;
            }
            return ((name != null) && SafeSecurityHelper.IsSameKeyToken(name.GetPublicKeyToken(), WindowsBaseToken));
        }

        private static bool LooselyImplementInterface(Type t, Type interfaceType)
        {
            for (Type type = t; type != null; type = type.BaseType)
            {
                Type[] interfaces = type.GetInterfaces();
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (AssemblyQualifiedNameEquals(interfaces[i], interfaceType) || LooselyImplementInterface(interfaces[i], interfaceType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

