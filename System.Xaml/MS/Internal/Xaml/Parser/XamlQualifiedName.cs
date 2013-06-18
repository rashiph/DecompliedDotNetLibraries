namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Runtime.InteropServices;

    internal class XamlQualifiedName : XamlName
    {
        public XamlQualifiedName(string prefix, string name) : base(prefix, name)
        {
        }

        internal static bool IsNameValid(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!XamlName.IsValidNameStartChar(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!XamlName.IsValidQualifiedNameChar(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsNameValid_WithPlus(string name)
        {
            if (name.Length == 0)
            {
                return false;
            }
            if (!XamlName.IsValidNameStartChar(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                if (!XamlName.IsValidQualifiedNameCharPlus(name[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool Parse(string longName, out string prefix, out string name)
        {
            int startIndex = 0;
            int index = longName.IndexOf(':');
            prefix = string.Empty;
            name = string.Empty;
            if (index != -1)
            {
                prefix = longName.Substring(startIndex, index);
                if (string.IsNullOrEmpty(prefix) || !IsNameValid(prefix))
                {
                    return false;
                }
                startIndex = index + 1;
            }
            name = (startIndex == 0) ? longName : longName.Substring(startIndex);
            return (!string.IsNullOrEmpty(name) && IsNameValid_WithPlus(name));
        }

        public override string ScopedName
        {
            get
            {
                if (!string.IsNullOrEmpty(base.Prefix))
                {
                    return (base.Prefix + ":" + base.Name);
                }
                return base.Name;
            }
        }
    }
}

