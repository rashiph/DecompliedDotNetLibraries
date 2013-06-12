namespace System.Security
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Text;

    [Serializable, ComVisible(true)]
    public sealed class SecurityElement : ISecurityElementFactory
    {
        private const int c_AttributesTypical = 8;
        private const int c_ChildrenTypical = 1;
        internal ArrayList m_lAttributes;
        private ArrayList m_lChildren;
        internal string m_strTag;
        internal string m_strText;
        internal SecurityElementType m_type;
        private static readonly char[] s_escapeChars = new char[] { '<', '>', '"', '\'', '&' };
        private static readonly string[] s_escapeStringPairs = new string[] { "<", "&lt;", ">", "&gt;", "\"", "&quot;", "'", "&apos;", "&", "&amp;" };
        private const string s_strIndent = "   ";
        private static readonly char[] s_tagIllegalCharacters = new char[] { ' ', '<', '>' };
        private static readonly char[] s_textIllegalCharacters = new char[] { '<', '>' };
        private static readonly char[] s_valueIllegalCharacters = new char[] { '<', '>', '"' };

        internal SecurityElement()
        {
        }

        public SecurityElement(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }
            if (!IsValidTag(tag))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementTag"), new object[] { tag }));
            }
            this.m_strTag = tag;
            this.m_strText = null;
        }

        public SecurityElement(string tag, string text)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }
            if (!IsValidTag(tag))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementTag"), new object[] { tag }));
            }
            if ((text != null) && !IsValidText(text))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementText"), new object[] { text }));
            }
            this.m_strTag = tag;
            this.m_strText = text;
        }

        public void AddAttribute(string name, string value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!IsValidAttributeName(name))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementName"), new object[] { name }));
            }
            if (!IsValidAttributeValue(value))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementValue"), new object[] { value }));
            }
            this.AddAttributeSafe(name, value);
        }

        internal void AddAttributeSafe(string name, string value)
        {
            if (this.m_lAttributes == null)
            {
                this.m_lAttributes = new ArrayList(8);
            }
            else
            {
                int count = this.m_lAttributes.Count;
                for (int i = 0; i < count; i += 2)
                {
                    string a = (string) this.m_lAttributes[i];
                    if (string.Equals(a, name))
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_AttributeNamesMustBeUnique"));
                    }
                }
            }
            this.m_lAttributes.Add(name);
            this.m_lAttributes.Add(value);
        }

        internal void AddChild(ISecurityElementFactory child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (this.m_lChildren == null)
            {
                this.m_lChildren = new ArrayList(1);
            }
            this.m_lChildren.Add(child);
        }

        public void AddChild(SecurityElement child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (this.m_lChildren == null)
            {
                this.m_lChildren = new ArrayList(1);
            }
            this.m_lChildren.Add(child);
        }

        internal void AddChildNoDuplicates(ISecurityElementFactory child)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (this.m_lChildren == null)
            {
                this.m_lChildren = new ArrayList(1);
                this.m_lChildren.Add(child);
            }
            else
            {
                for (int i = 0; i < this.m_lChildren.Count; i++)
                {
                    if (this.m_lChildren[i] == child)
                    {
                        return;
                    }
                }
                this.m_lChildren.Add(child);
            }
        }

        public string Attribute(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (this.m_lAttributes != null)
            {
                int count = this.m_lAttributes.Count;
                for (int i = 0; i < count; i += 2)
                {
                    string a = (string) this.m_lAttributes[i];
                    if (string.Equals(a, name))
                    {
                        string str = (string) this.m_lAttributes[i + 1];
                        return Unescape(str);
                    }
                }
            }
            return null;
        }

        internal void ConvertSecurityElementFactories()
        {
            if (this.m_lChildren != null)
            {
                for (int i = 0; i < this.m_lChildren.Count; i++)
                {
                    ISecurityElementFactory factory = this.m_lChildren[i] as ISecurityElementFactory;
                    if ((factory != null) && !(this.m_lChildren[i] is SecurityElement))
                    {
                        this.m_lChildren[i] = factory.CreateSecurityElement();
                    }
                }
            }
        }

        [ComVisible(false)]
        public SecurityElement Copy()
        {
            return new SecurityElement(this.m_strTag, this.m_strText) { m_lChildren = (this.m_lChildren == null) ? null : new ArrayList(this.m_lChildren), m_lAttributes = (this.m_lAttributes == null) ? null : new ArrayList(this.m_lAttributes) };
        }

        public bool Equal(SecurityElement other)
        {
            if (other == null)
            {
                return false;
            }
            if (!string.Equals(this.m_strTag, other.m_strTag))
            {
                return false;
            }
            if (!string.Equals(this.m_strText, other.m_strText))
            {
                return false;
            }
            if ((this.m_lAttributes == null) || (other.m_lAttributes == null))
            {
                if (this.m_lAttributes != other.m_lAttributes)
                {
                    return false;
                }
            }
            else
            {
                int count = this.m_lAttributes.Count;
                if (count != other.m_lAttributes.Count)
                {
                    return false;
                }
                for (int i = 0; i < count; i++)
                {
                    string a = (string) this.m_lAttributes[i];
                    string b = (string) other.m_lAttributes[i];
                    if (!string.Equals(a, b))
                    {
                        return false;
                    }
                }
            }
            if ((this.m_lChildren == null) || (other.m_lChildren == null))
            {
                if (this.m_lChildren != other.m_lChildren)
                {
                    return false;
                }
            }
            else
            {
                if (this.m_lChildren.Count != other.m_lChildren.Count)
                {
                    return false;
                }
                this.ConvertSecurityElementFactories();
                other.ConvertSecurityElementFactories();
                IEnumerator enumerator = this.m_lChildren.GetEnumerator();
                IEnumerator enumerator2 = other.m_lChildren.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    enumerator2.MoveNext();
                    SecurityElement current = (SecurityElement) enumerator.Current;
                    SecurityElement element2 = (SecurityElement) enumerator2.Current;
                    if ((current == null) || !current.Equal(element2))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static string Escape(string str)
        {
            if (str == null)
            {
                return null;
            }
            StringBuilder builder = null;
            int length = str.Length;
            int startIndex = 0;
            while (true)
            {
                int num2 = str.IndexOfAny(s_escapeChars, startIndex);
                if (num2 == -1)
                {
                    if (builder == null)
                    {
                        return str;
                    }
                    builder.Append(str, startIndex, length - startIndex);
                    return builder.ToString();
                }
                if (builder == null)
                {
                    builder = new StringBuilder();
                }
                builder.Append(str, startIndex, num2 - startIndex);
                builder.Append(GetEscapeSequence(str[num2]));
                startIndex = num2 + 1;
            }
        }

        public static SecurityElement FromString(string xml)
        {
            if (xml == null)
            {
                throw new ArgumentNullException("xml");
            }
            return new Parser(xml).GetTopElement();
        }

        private static string GetEscapeSequence(char c)
        {
            int length = s_escapeStringPairs.Length;
            for (int i = 0; i < length; i += 2)
            {
                string str = s_escapeStringPairs[i];
                string str2 = s_escapeStringPairs[i + 1];
                if (str[0] == c)
                {
                    return str2;
                }
            }
            return c.ToString();
        }

        private static string GetUnescapeSequence(string str, int index, out int newIndex)
        {
            int num = str.Length - index;
            int length = s_escapeStringPairs.Length;
            for (int i = 0; i < length; i += 2)
            {
                string str2 = s_escapeStringPairs[i];
                string strA = s_escapeStringPairs[i + 1];
                int num4 = strA.Length;
                if ((num4 <= num) && (string.Compare(strA, 0, str, index, num4, StringComparison.Ordinal) == 0))
                {
                    newIndex = index + strA.Length;
                    return str2;
                }
            }
            newIndex = index + 1;
            char ch = str[index];
            return ch.ToString();
        }

        public static bool IsValidAttributeName(string name)
        {
            return IsValidTag(name);
        }

        public static bool IsValidAttributeValue(string value)
        {
            if (value == null)
            {
                return false;
            }
            return (value.IndexOfAny(s_valueIllegalCharacters) == -1);
        }

        public static bool IsValidTag(string tag)
        {
            if (tag == null)
            {
                return false;
            }
            return (tag.IndexOfAny(s_tagIllegalCharacters) == -1);
        }

        public static bool IsValidText(string text)
        {
            if (text == null)
            {
                return false;
            }
            return (text.IndexOfAny(s_textIllegalCharacters) == -1);
        }

        public SecurityElement SearchForChildByTag(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }
            if (this.m_lChildren != null)
            {
                IEnumerator enumerator = this.m_lChildren.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SecurityElement current = (SecurityElement) enumerator.Current;
                    if ((current != null) && string.Equals(current.Tag, tag))
                    {
                        return current;
                    }
                }
            }
            return null;
        }

        internal string SearchForTextOfLocalName(string strLocalName)
        {
            if (strLocalName == null)
            {
                throw new ArgumentNullException("strLocalName");
            }
            if (this.m_strTag != null)
            {
                if (this.m_strTag.Equals(strLocalName) || this.m_strTag.EndsWith(":" + strLocalName, StringComparison.Ordinal))
                {
                    return Unescape(this.m_strText);
                }
                if (this.m_lChildren != null)
                {
                    IEnumerator enumerator = this.m_lChildren.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string str = ((SecurityElement) enumerator.Current).SearchForTextOfLocalName(strLocalName);
                        if (str != null)
                        {
                            return str;
                        }
                    }
                }
            }
            return null;
        }

        public string SearchForTextOfTag(string tag)
        {
            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }
            if (string.Equals(this.m_strTag, tag))
            {
                return Unescape(this.m_strText);
            }
            if (this.m_lChildren != null)
            {
                IEnumerator enumerator = this.m_lChildren.GetEnumerator();
                this.ConvertSecurityElementFactories();
                while (enumerator.MoveNext())
                {
                    string str = ((SecurityElement) enumerator.Current).SearchForTextOfTag(tag);
                    if (str != null)
                    {
                        return str;
                    }
                }
            }
            return null;
        }

        string ISecurityElementFactory.Attribute(string attributeName)
        {
            return this.Attribute(attributeName);
        }

        object ISecurityElementFactory.Copy()
        {
            return this.Copy();
        }

        SecurityElement ISecurityElementFactory.CreateSecurityElement()
        {
            return this;
        }

        string ISecurityElementFactory.GetTag()
        {
            return this.Tag;
        }

        internal IPermission ToPermission(bool ignoreTypeLoadFailures)
        {
            IPermission perm = XMLUtil.CreatePermission(this, PermissionState.None, ignoreTypeLoadFailures);
            if (perm == null)
            {
                return null;
            }
            perm.FromXml(this);
            PermissionToken.GetToken(perm);
            return perm;
        }

        [SecurityCritical]
        internal object ToSecurityObject()
        {
            string str;
            if (((str = this.m_strTag) != null) && (str == "PermissionSet"))
            {
                PermissionSet set = new PermissionSet(PermissionState.None);
                set.FromXml(this);
                return set;
            }
            return this.ToPermission(false);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            this.ToString("", builder, new ToStringHelperFunc(SecurityElement.ToStringHelperStringBuilder));
            return builder.ToString();
        }

        private void ToString(string indent, object obj, ToStringHelperFunc func)
        {
            func(obj, "<");
            switch (this.m_type)
            {
                case SecurityElementType.Format:
                    func(obj, "?");
                    break;

                case SecurityElementType.Comment:
                    func(obj, "!");
                    break;
            }
            func(obj, this.m_strTag);
            if ((this.m_lAttributes != null) && (this.m_lAttributes.Count > 0))
            {
                func(obj, " ");
                int count = this.m_lAttributes.Count;
                for (int i = 0; i < count; i += 2)
                {
                    string str = (string) this.m_lAttributes[i];
                    string str2 = (string) this.m_lAttributes[i + 1];
                    func(obj, str);
                    func(obj, "=\"");
                    func(obj, str2);
                    func(obj, "\"");
                    if (i != (this.m_lAttributes.Count - 2))
                    {
                        if (this.m_type == SecurityElementType.Regular)
                        {
                            func(obj, Environment.NewLine);
                        }
                        else
                        {
                            func(obj, " ");
                        }
                    }
                }
            }
            if ((this.m_strText != null) || ((this.m_lChildren != null) && (this.m_lChildren.Count != 0)))
            {
                func(obj, ">");
                func(obj, this.m_strText);
                if (this.m_lChildren != null)
                {
                    this.ConvertSecurityElementFactories();
                    func(obj, Environment.NewLine);
                    for (int j = 0; j < this.m_lChildren.Count; j++)
                    {
                        ((SecurityElement) this.m_lChildren[j]).ToString("", obj, func);
                    }
                }
                func(obj, "</");
                func(obj, this.m_strTag);
                func(obj, ">");
                func(obj, Environment.NewLine);
            }
            else
            {
                switch (this.m_type)
                {
                    case SecurityElementType.Format:
                        func(obj, " ?>");
                        break;

                    case SecurityElementType.Comment:
                        func(obj, ">");
                        break;

                    default:
                        func(obj, "/>");
                        break;
                }
                func(obj, Environment.NewLine);
            }
        }

        private static void ToStringHelperStreamWriter(object obj, string str)
        {
            ((StreamWriter) obj).Write(str);
        }

        private static void ToStringHelperStringBuilder(object obj, string str)
        {
            ((StringBuilder) obj).Append(str);
        }

        internal void ToWriter(StreamWriter writer)
        {
            this.ToString("", writer, new ToStringHelperFunc(SecurityElement.ToStringHelperStreamWriter));
        }

        private static string Unescape(string str)
        {
            if (str == null)
            {
                return null;
            }
            StringBuilder builder = null;
            int length = str.Length;
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOf('&', startIndex);
                if (index == -1)
                {
                    if (builder == null)
                    {
                        return str;
                    }
                    builder.Append(str, startIndex, length - startIndex);
                    return builder.ToString();
                }
                if (builder == null)
                {
                    builder = new StringBuilder();
                }
                builder.Append(str, startIndex, index - startIndex);
                builder.Append(GetUnescapeSequence(str, index, out startIndex));
            }
        }

        public Hashtable Attributes
        {
            get
            {
                if ((this.m_lAttributes == null) || (this.m_lAttributes.Count == 0))
                {
                    return null;
                }
                Hashtable hashtable = new Hashtable(this.m_lAttributes.Count / 2);
                int count = this.m_lAttributes.Count;
                for (int i = 0; i < count; i += 2)
                {
                    hashtable.Add(this.m_lAttributes[i], this.m_lAttributes[i + 1]);
                }
                return hashtable;
            }
            set
            {
                if ((value == null) || (value.Count == 0))
                {
                    this.m_lAttributes = null;
                }
                else
                {
                    ArrayList list = new ArrayList(value.Count);
                    IDictionaryEnumerator enumerator = value.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        string key = (string) enumerator.Key;
                        string str2 = (string) enumerator.Value;
                        if (!IsValidAttributeName(key))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementName"), new object[] { (string) enumerator.Current }));
                        }
                        if (!IsValidAttributeValue(str2))
                        {
                            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementValue"), new object[] { (string) enumerator.Value }));
                        }
                        list.Add(key);
                        list.Add(str2);
                    }
                    this.m_lAttributes = list;
                }
            }
        }

        public ArrayList Children
        {
            get
            {
                this.ConvertSecurityElementFactories();
                return this.m_lChildren;
            }
            set
            {
                if (value != null)
                {
                    IEnumerator enumerator = value.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current == null)
                        {
                            throw new ArgumentException(Environment.GetResourceString("ArgumentNull_Child"));
                        }
                    }
                }
                this.m_lChildren = value;
            }
        }

        internal ArrayList InternalChildren
        {
            get
            {
                return this.m_lChildren;
            }
        }

        public string Tag
        {
            get
            {
                return this.m_strTag;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Tag");
                }
                if (!IsValidTag(value))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementTag"), new object[] { value }));
                }
                this.m_strTag = value;
            }
        }

        public string Text
        {
            get
            {
                return Unescape(this.m_strText);
            }
            set
            {
                if (value == null)
                {
                    this.m_strText = null;
                }
                else
                {
                    if (!IsValidText(value))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_InvalidElementTag"), new object[] { value }));
                    }
                    this.m_strText = value;
                }
            }
        }

        private delegate void ToStringHelperFunc(object obj, string str);
    }
}

