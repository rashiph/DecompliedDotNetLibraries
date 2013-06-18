namespace MS.Internal.Xaml.Parser
{
    using System;

    internal class XamlPropertyName : XamlName
    {
        public readonly XamlName Owner;

        private XamlPropertyName(XamlName owner, string prefix, string name) : base(name)
        {
            if (owner != null)
            {
                this.Owner = owner;
                base._prefix = owner.Prefix ?? string.Empty;
            }
            else
            {
                base._prefix = prefix ?? string.Empty;
            }
        }

        public static XamlPropertyName Parse(string longName)
        {
            string str;
            string str2;
            if (string.IsNullOrEmpty(longName))
            {
                return null;
            }
            if (!XamlQualifiedName.Parse(longName, out str, out str2))
            {
                return null;
            }
            int startIndex = 0;
            string str3 = string.Empty;
            int index = str2.IndexOf('.');
            if (index != -1)
            {
                str3 = str2.Substring(startIndex, index);
                if (string.IsNullOrEmpty(str3))
                {
                    return null;
                }
                startIndex = index + 1;
            }
            string str4 = (startIndex == 0) ? str2 : str2.Substring(startIndex);
            XamlQualifiedName owner = null;
            if (!string.IsNullOrEmpty(str3))
            {
                owner = new XamlQualifiedName(str, str3);
            }
            return new XamlPropertyName(owner, str, str4);
        }

        public static XamlPropertyName Parse(string longName, string namespaceURI)
        {
            XamlPropertyName name = Parse(longName);
            name._namespace = namespaceURI;
            return name;
        }

        public bool IsDotted
        {
            get
            {
                return (this.Owner != null);
            }
        }

        public string OwnerName
        {
            get
            {
                if (!this.IsDotted)
                {
                    return string.Empty;
                }
                return this.Owner.Name;
            }
        }

        public override string ScopedName
        {
            get
            {
                if (!this.IsDotted)
                {
                    return base.Name;
                }
                return (this.Owner.ScopedName + "." + base.Name);
            }
        }
    }
}

