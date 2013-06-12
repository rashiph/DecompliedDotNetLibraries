namespace System.Security
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Security.Util;

    [Serializable]
    internal sealed class PermissionToken : ISecurityEncodable
    {
        private const string c_mscorlibName = "mscorlib";
        internal int m_index;
        internal string m_strTypeName;
        internal PermissionTokenType m_type;
        private static ReflectionPermission s_reflectPerm = null;
        private static readonly PermissionTokenFactory s_theTokenFactory = new PermissionTokenFactory(4);
        internal static TokenBasedSet s_tokenSet = new TokenBasedSet();

        internal PermissionToken()
        {
        }

        internal PermissionToken(int index, PermissionTokenType type, string strTypeName)
        {
            this.m_index = index;
            this.m_type = type;
            this.m_strTypeName = strTypeName;
        }

        [SecuritySafeCritical]
        public static PermissionToken FindToken(Type cls)
        {
            if (cls == null)
            {
                return null;
            }
            if (cls.GetInterface("System.Security.Permissions.IBuiltInPermission") == null)
            {
                return s_theTokenFactory.FindToken(cls);
            }
            if (s_reflectPerm == null)
            {
                s_reflectPerm = new ReflectionPermission(PermissionState.Unrestricted);
            }
            s_reflectPerm.Assert();
            RuntimeMethodInfo method = cls.GetMethod("GetTokenIndex", BindingFlags.NonPublic | BindingFlags.Static) as RuntimeMethodInfo;
            int index = (int) method.Invoke(null, BindingFlags.Default, null, null, null, true);
            return s_theTokenFactory.BuiltInGetToken(index, null, cls);
        }

        public static PermissionToken FindTokenByIndex(int i)
        {
            return s_theTokenFactory.FindTokenByIndex(i);
        }

        [SecuritySafeCritical]
        public void FromXml(SecurityElement elRoot)
        {
            PermissionToken token;
            elRoot.Tag.Equals("PermissionToken");
            string typeStr = elRoot.Attribute("Name");
            if (typeStr != null)
            {
                token = GetToken(typeStr, true);
            }
            else
            {
                token = FindTokenByIndex(int.Parse(elRoot.Attribute("Index"), CultureInfo.InvariantCulture));
            }
            this.m_index = token.m_index;
            this.m_type = (PermissionTokenType) Enum.Parse(typeof(PermissionTokenType), elRoot.Attribute("Type"));
            this.m_strTypeName = token.m_strTypeName;
        }

        public static PermissionToken GetToken(IPermission perm)
        {
            if (perm == null)
            {
                return null;
            }
            IBuiltInPermission permission = perm as IBuiltInPermission;
            if (permission != null)
            {
                return s_theTokenFactory.BuiltInGetToken(permission.GetTokenIndex(), perm, null);
            }
            return s_theTokenFactory.GetToken(perm.GetType(), perm);
        }

        public static PermissionToken GetToken(string typeStr)
        {
            return GetToken(typeStr, false);
        }

        [SecurityCritical]
        public static PermissionToken GetToken(Type cls)
        {
            if (cls == null)
            {
                return null;
            }
            if (cls.GetInterface("System.Security.Permissions.IBuiltInPermission") == null)
            {
                return s_theTokenFactory.GetToken(cls, null);
            }
            if (s_reflectPerm == null)
            {
                s_reflectPerm = new ReflectionPermission(PermissionState.Unrestricted);
            }
            s_reflectPerm.Assert();
            RuntimeMethodInfo method = cls.GetMethod("GetTokenIndex", BindingFlags.NonPublic | BindingFlags.Static) as RuntimeMethodInfo;
            int index = (int) method.Invoke(null, BindingFlags.Default, null, null, null, true);
            return s_theTokenFactory.BuiltInGetToken(index, null, cls);
        }

        public static PermissionToken GetToken(string typeStr, bool bCreateMscorlib)
        {
            if (typeStr == null)
            {
                return null;
            }
            if (!IsMscorlibClassName(typeStr))
            {
                return s_theTokenFactory.GetToken(typeStr);
            }
            if (!bCreateMscorlib)
            {
                return null;
            }
            return FindToken(Type.GetType(typeStr));
        }

        internal static bool IsMscorlibClassName(string className)
        {
            if (className.IndexOf(',') == -1)
            {
                return true;
            }
            int num = className.LastIndexOf(']');
            if (num == -1)
            {
                num = 0;
            }
            for (int i = num; i < className.Length; i++)
            {
                if (((className[i] == 'm') || (className[i] == 'M')) && (string.Compare(className, i, "mscorlib", 0, "mscorlib".Length, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    return true;
                }
            }
            return false;
        }

        [SecuritySafeCritical]
        public static bool IsTokenProperlyAssigned(IPermission perm, PermissionToken token)
        {
            PermissionToken token2 = GetToken(perm);
            if (token2.m_index != token.m_index)
            {
                return false;
            }
            if (token.m_type != token2.m_type)
            {
                return false;
            }
            if ((perm.GetType().Module.Assembly == Assembly.GetExecutingAssembly()) && (token2.m_index >= 0x11))
            {
                return false;
            }
            return true;
        }

        public SecurityElement ToXml()
        {
            SecurityElement element = new SecurityElement("PermissionToken");
            if ((this.m_type & PermissionTokenType.BuiltIn) != 0)
            {
                element.AddAttribute("Index", this.m_index);
            }
            else
            {
                element.AddAttribute("Name", SecurityElement.Escape(this.m_strTypeName));
            }
            element.AddAttribute("Type", this.m_type.ToString("F"));
            return element;
        }
    }
}

