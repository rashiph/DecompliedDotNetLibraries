namespace System.Security.Util
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;

    internal static class XMLUtil
    {
        private const string BuiltInApplicationSecurityManager = "System.Security.Policy.";
        private const string BuiltInCodeGroup = "System.Security.Policy.";
        private const string BuiltInMembershipCondition = "System.Security.Policy.";
        private const string BuiltInPermission = "System.Security.Permissions.";
        private static readonly char[] sepChar = new char[] { ',', ' ' };

        public static void AddClassAttribute(SecurityElement element, Type type, string typename)
        {
            if (typename == null)
            {
                typename = type.FullName;
            }
            element.AddAttribute("class", typename + ", " + type.Module.Assembly.FullName.Replace('"', '\''));
        }

        public static string BitFieldEnumToString(Type type, object value)
        {
            int num = (int) value;
            if (num == 0)
            {
                return Enum.GetName(type, 0);
            }
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            int num2 = 1;
            for (int i = 1; i < 0x20; i++)
            {
                if ((num2 & num) != 0)
                {
                    string name = Enum.GetName(type, num2);
                    if (name == null)
                    {
                        continue;
                    }
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(name);
                    flag = false;
                }
                num2 = num2 << 1;
            }
            return builder.ToString();
        }

        [SecuritySafeCritical]
        public static CodeGroup CreateCodeGroup(SecurityElement el)
        {
            string str;
            int num;
            int num2;
            if ((el == null) || !el.Tag.Equals("CodeGroup"))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongElementType"), new object[] { "<CodeGroup>" }));
            }
            if (ParseElementForObjectCreation(el, "System.Security.Policy.", out str, out num2, out num))
            {
                switch (num)
                {
                    case 12:
                        if (string.Compare(str, num2, "NetCodeGroup", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new NetCodeGroup();

                    case 13:
                        if (string.Compare(str, num2, "FileCodeGroup", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new FileCodeGroup();

                    case 14:
                        if (string.Compare(str, num2, "UnionCodeGroup", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new UnionCodeGroup();

                    case 0x13:
                        if (string.Compare(str, num2, "FirstMatchCodeGroup", 0, num, StringComparison.Ordinal) == 0)
                        {
                            return new FirstMatchCodeGroup();
                        }
                        break;
                }
            }
            Type c = null;
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            c = GetClassFromElement(el, true);
            if (c == null)
            {
                return null;
            }
            if (!typeof(CodeGroup).IsAssignableFrom(c))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotACodeGroupType"));
            }
            return (CodeGroup) Activator.CreateInstance(c, true);
        }

        [SecurityCritical]
        internal static IMembershipCondition CreateMembershipCondition(SecurityElement el)
        {
            string str;
            int num;
            int num2;
            if ((el == null) || !el.Tag.Equals("IMembershipCondition"))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_WrongElementType"), new object[] { "<IMembershipCondition>" }));
            }
            if (ParseElementForObjectCreation(el, "System.Security.Policy.", out str, out num, out num2))
            {
                switch (num2)
                {
                    case 0x16:
                        if (str[num] != 'A')
                        {
                            if (string.Compare(str, num, "UrlMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                            {
                                break;
                            }
                            return new UrlMembershipCondition();
                        }
                        if (string.Compare(str, num, "AllMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new AllMembershipCondition();

                    case 0x17:
                        if (str[num] != 'H')
                        {
                            if (str[num] == 'S')
                            {
                                if (string.Compare(str, num, "SiteMembershipCondition", 0, num2, StringComparison.Ordinal) == 0)
                                {
                                    return new SiteMembershipCondition();
                                }
                                break;
                            }
                            if (string.Compare(str, num, "ZoneMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                            {
                                break;
                            }
                            return new ZoneMembershipCondition();
                        }
                        if (string.Compare(str, num, "HashMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new HashMembershipCondition();

                    case 0x1c:
                        if (string.Compare(str, num, "PublisherMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new PublisherMembershipCondition();

                    case 0x1d:
                        if (string.Compare(str, num, "StrongNameMembershipCondition", 0, num2, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new StrongNameMembershipCondition();

                    case 0x27:
                        if (string.Compare(str, num, "ApplicationDirectoryMembershipCondition", 0, num2, StringComparison.Ordinal) == 0)
                        {
                            return new ApplicationDirectoryMembershipCondition();
                        }
                        break;
                }
            }
            Type c = null;
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            c = GetClassFromElement(el, true);
            if (c == null)
            {
                return null;
            }
            if (!typeof(IMembershipCondition).IsAssignableFrom(c))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotAMembershipCondition"));
            }
            return (IMembershipCondition) Activator.CreateInstance(c, true);
        }

        [SecuritySafeCritical]
        public static IPermission CreatePermission(SecurityElement el, PermissionState permState, bool ignoreTypeLoadFailures)
        {
            string str;
            int num;
            int num2;
            if ((el == null) || (!el.Tag.Equals("Permission") && !el.Tag.Equals("IPermission")))
            {
                throw new ArgumentException(string.Format(null, Environment.GetResourceString("Argument_WrongElementType"), new object[] { "<Permission>" }));
            }
            if (ParseElementForObjectCreation(el, "System.Security.Permissions.", out str, out num2, out num))
            {
                switch (num)
                {
                    case 12:
                        if (string.Compare(str, num2, "UIPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new UIPermission(permState);

                    case 0x10:
                        if (string.Compare(str, num2, "FileIOPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new FileIOPermission(permState);

                    case 0x12:
                        if (str[num2] != 'R')
                        {
                            if (string.Compare(str, num2, "SecurityPermission", 0, num, StringComparison.Ordinal) != 0)
                            {
                                break;
                            }
                            return new SecurityPermission(permState);
                        }
                        if (string.Compare(str, num2, "RegistryPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new RegistryPermission(permState);

                    case 0x13:
                        if (string.Compare(str, num2, "PrincipalPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new PrincipalPermission(permState);

                    case 20:
                        if (str[num2] != 'R')
                        {
                            if (string.Compare(str, num2, "FileDialogPermission", 0, num, StringComparison.Ordinal) != 0)
                            {
                                break;
                            }
                            return new FileDialogPermission(permState);
                        }
                        if (string.Compare(str, num2, "ReflectionPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new ReflectionPermission(permState);

                    case 0x15:
                        if (str[num2] != 'E')
                        {
                            if (str[num2] == 'U')
                            {
                                if (string.Compare(str, num2, "UrlIdentityPermission", 0, num, StringComparison.Ordinal) == 0)
                                {
                                    return new UrlIdentityPermission(permState);
                                }
                                break;
                            }
                            if (string.Compare(str, num2, "GacIdentityPermission", 0, num, StringComparison.Ordinal) != 0)
                            {
                                break;
                            }
                            return new GacIdentityPermission(permState);
                        }
                        if (string.Compare(str, num2, "EnvironmentPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new EnvironmentPermission(permState);

                    case 0x16:
                        if (str[num2] != 'S')
                        {
                            if (str[num2] == 'Z')
                            {
                                if (string.Compare(str, num2, "ZoneIdentityPermission", 0, num, StringComparison.Ordinal) == 0)
                                {
                                    return new ZoneIdentityPermission(permState);
                                }
                            }
                            else if (string.Compare(str, num2, "KeyContainerPermission", 0, num, StringComparison.Ordinal) == 0)
                            {
                                return new KeyContainerPermission(permState);
                            }
                            break;
                        }
                        if (string.Compare(str, num2, "SiteIdentityPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new SiteIdentityPermission(permState);

                    case 0x18:
                        if (string.Compare(str, num2, "HostProtectionPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new HostProtectionPermission(permState);

                    case 0x1b:
                        if (string.Compare(str, num2, "PublisherIdentityPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new PublisherIdentityPermission(permState);

                    case 0x1c:
                        if (string.Compare(str, num2, "StrongNameIdentityPermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new StrongNameIdentityPermission(permState);

                    case 0x1d:
                        if (string.Compare(str, num2, "IsolatedStorageFilePermission", 0, num, StringComparison.Ordinal) != 0)
                        {
                            break;
                        }
                        return new IsolatedStorageFilePermission(permState);
                }
            }
            object[] args = new object[] { permState };
            Type c = null;
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            c = GetClassFromElement(el, ignoreTypeLoadFailures);
            if (c == null)
            {
                return null;
            }
            if (!typeof(IPermission).IsAssignableFrom(c))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotAPermissionType"));
            }
            return (IPermission) Activator.CreateInstance(c, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, args, null);
        }

        internal static Type GetClassFromElement(SecurityElement el, bool ignoreTypeLoadFailures)
        {
            string typeName = el.Attribute("class");
            if (typeName == null)
            {
                if (!ignoreTypeLoadFailures)
                {
                    throw new ArgumentException(string.Format(null, Environment.GetResourceString("Argument_InvalidXMLMissingAttr"), new object[] { "class" }));
                }
                return null;
            }
            if (ignoreTypeLoadFailures)
            {
                try
                {
                    return Type.GetType(typeName, false, false);
                }
                catch (SecurityException)
                {
                    return null;
                }
            }
            return Type.GetType(typeName, true, false);
        }

        public static bool IsPermissionElement(IPermission ip, SecurityElement el)
        {
            if (!el.Tag.Equals("Permission") && !el.Tag.Equals("IPermission"))
            {
                return false;
            }
            return true;
        }

        public static bool IsUnrestricted(SecurityElement el)
        {
            string str = el.Attribute("Unrestricted");
            if (str == null)
            {
                return false;
            }
            if (!str.Equals("true") && !str.Equals("TRUE"))
            {
                return str.Equals("True");
            }
            return true;
        }

        public static SecurityElement NewPermissionElement(IPermission ip)
        {
            return NewPermissionElement(ip.GetType().FullName);
        }

        public static SecurityElement NewPermissionElement(string name)
        {
            SecurityElement element = new SecurityElement("Permission");
            element.AddAttribute("class", name);
            return element;
        }

        internal static bool ParseElementForAssemblyIdentification(SecurityElement el, out string className, out string assemblyName, out string assemblyVersion)
        {
            className = null;
            assemblyName = null;
            assemblyVersion = null;
            string str = el.Attribute("class");
            if (str == null)
            {
                return false;
            }
            if (str.IndexOf('\'') >= 0)
            {
                str = str.Replace('\'', '"');
            }
            int index = str.IndexOf(',');
            if (index == -1)
            {
                return false;
            }
            int length = index;
            className = str.Substring(0, length);
            AssemblyName name = new AssemblyName(str.Substring(index + 1));
            assemblyName = name.Name;
            assemblyVersion = name.Version.ToString();
            return true;
        }

        [SecurityCritical]
        private static bool ParseElementForObjectCreation(SecurityElement el, string requiredNamespace, out string className, out int classNameStart, out int classNameLength)
        {
            className = null;
            classNameStart = 0;
            classNameLength = 0;
            int length = requiredNamespace.Length;
            string str = el.Attribute("class");
            if (str == null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NoClass"));
            }
            if (str.IndexOf('\'') >= 0)
            {
                str = str.Replace('\'', '"');
            }
            if (PermissionToken.IsMscorlibClassName(str))
            {
                int num3;
                int index = str.IndexOf(',');
                if (index == -1)
                {
                    num3 = str.Length;
                }
                else
                {
                    num3 = index;
                }
                if ((num3 > length) && str.StartsWith(requiredNamespace, StringComparison.Ordinal))
                {
                    className = str;
                    classNameLength = num3 - length;
                    classNameStart = length;
                    return true;
                }
            }
            return false;
        }

        public static string SecurityObjectToXmlString(object ob)
        {
            if (ob == null)
            {
                return "";
            }
            PermissionSet set = ob as PermissionSet;
            if (set != null)
            {
                return set.ToXml().ToString();
            }
            return ((IPermission) ob).ToXml().ToString();
        }

        [SecurityCritical]
        public static object XmlStringToSecurityObject(string s)
        {
            if (s == null)
            {
                return null;
            }
            if (s.Length < 1)
            {
                return null;
            }
            return SecurityElement.FromString(s).ToSecurityObject();
        }
    }
}

