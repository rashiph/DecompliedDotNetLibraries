namespace System.Xaml.Schema
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xaml;

    [DebuggerDisplay("{{{Namespace}}}{Name}{TypeArgStringForDebugger}")]
    public class XamlTypeName
    {
        private List<XamlTypeName> _typeArguments;

        public XamlTypeName()
        {
        }

        public XamlTypeName(XamlType xamlType)
        {
            if (xamlType == null)
            {
                throw new ArgumentNullException("xamlType");
            }
            this.Name = xamlType.Name;
            this.Namespace = xamlType.GetXamlNamespaces()[0];
            if (xamlType.TypeArguments != null)
            {
                foreach (XamlType type in xamlType.TypeArguments)
                {
                    this.TypeArguments.Add(new XamlTypeName(type));
                }
            }
        }

        public XamlTypeName(string xamlNamespace, string name) : this(xamlNamespace, name, null)
        {
        }

        public XamlTypeName(string xamlNamespace, string name, IEnumerable<XamlTypeName> typeArguments)
        {
            this.Name = name;
            this.Namespace = xamlNamespace;
            if (typeArguments != null)
            {
                List<XamlTypeName> list = new List<XamlTypeName>(typeArguments);
                this._typeArguments = list;
            }
        }

        internal static string ConvertListToStringInternal(IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
        {
            StringBuilder result = new StringBuilder();
            ConvertListToStringInternal(result, typeNameList, prefixGenerator);
            return result.ToString();
        }

        internal static void ConvertListToStringInternal(StringBuilder result, IList<XamlTypeName> typeNameList, Func<string, string> prefixGenerator)
        {
            bool flag = true;
            foreach (XamlTypeName name in typeNameList)
            {
                if (!flag)
                {
                    result.Append(", ");
                }
                else
                {
                    flag = false;
                }
                name.ConvertToStringInternal(result, prefixGenerator);
            }
        }

        internal string ConvertToStringInternal(Func<string, string> prefixGenerator)
        {
            StringBuilder result = new StringBuilder();
            this.ConvertToStringInternal(result, prefixGenerator);
            return result.ToString();
        }

        internal void ConvertToStringInternal(StringBuilder result, Func<string, string> prefixGenerator)
        {
            if (this.Namespace == null)
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("XamlTypeNameNamespaceIsNull"));
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new InvalidOperationException(System.Xaml.SR.Get("XamlTypeNameNameIsNullOrEmpty"));
            }
            if (prefixGenerator == null)
            {
                result.Append("{");
                result.Append(this.Namespace);
                result.Append("}");
            }
            else
            {
                string str = prefixGenerator(this.Namespace);
                if (str == null)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlTypeNameCannotGetPrefix", new object[] { this.Namespace }));
                }
                if (str != string.Empty)
                {
                    result.Append(str);
                    result.Append(":");
                }
            }
            if (this.HasTypeArgs)
            {
                string str2;
                string str3 = GenericTypeNameScanner.StripSubscript(this.Name, out str2);
                result.Append(str3);
                result.Append("(");
                ConvertListToStringInternal(result, this.TypeArguments, prefixGenerator);
                result.Append(")");
                result.Append(str2);
            }
            else
            {
                result.Append(this.Name);
            }
        }

        public static XamlTypeName Parse(string typeName, IXamlNamespaceResolver namespaceResolver)
        {
            string str;
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            XamlTypeName name = ParseInternal(typeName, new Func<string, string>(namespaceResolver.GetNamespace), out str);
            if (name == null)
            {
                throw new FormatException(str);
            }
            return name;
        }

        internal static XamlTypeName ParseInternal(string typeName, Func<string, string> prefixResolver, out string error)
        {
            XamlTypeName name = GenericTypeNameParser.ParseIfTrivalName(typeName, prefixResolver, out error);
            if (name != null)
            {
                return name;
            }
            GenericTypeNameParser parser = new GenericTypeNameParser(prefixResolver);
            return parser.ParseName(typeName, out error);
        }

        public static IList<XamlTypeName> ParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver)
        {
            string str;
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            IList<XamlTypeName> list = ParseListInternal(typeNameList, new Func<string, string>(namespaceResolver.GetNamespace), out str);
            if (list == null)
            {
                throw new FormatException(str);
            }
            return list;
        }

        internal static IList<XamlTypeName> ParseListInternal(string typeNameList, Func<string, string> prefixResolver, out string error)
        {
            GenericTypeNameParser parser = new GenericTypeNameParser(prefixResolver);
            return parser.ParseList(typeNameList, out error);
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

        public string ToString(INamespacePrefixLookup prefixLookup)
        {
            if (prefixLookup == null)
            {
                return this.ConvertToStringInternal(null);
            }
            return this.ConvertToStringInternal(new Func<string, string>(prefixLookup.LookupPrefix));
        }

        public static string ToString(IList<XamlTypeName> typeNameList, INamespacePrefixLookup prefixLookup)
        {
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (prefixLookup == null)
            {
                throw new ArgumentNullException("prefixLookup");
            }
            return ConvertListToStringInternal(typeNameList, new Func<string, string>(prefixLookup.LookupPrefix));
        }

        public static bool TryParse(string typeName, IXamlNamespaceResolver namespaceResolver, out XamlTypeName result)
        {
            string str;
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            result = ParseInternal(typeName, new Func<string, string>(namespaceResolver.GetNamespace), out str);
            return (result != null);
        }

        public static bool TryParseList(string typeNameList, IXamlNamespaceResolver namespaceResolver, out IList<XamlTypeName> result)
        {
            string str;
            if (typeNameList == null)
            {
                throw new ArgumentNullException("typeNameList");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            result = ParseListInternal(typeNameList, new Func<string, string>(namespaceResolver.GetNamespace), out str);
            return (result != null);
        }

        internal bool HasTypeArgs
        {
            get
            {
                return ((this._typeArguments != null) && (this._typeArguments.Count > 0));
            }
        }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public IList<XamlTypeName> TypeArguments
        {
            get
            {
                if (this._typeArguments == null)
                {
                    this._typeArguments = new List<XamlTypeName>();
                }
                return this._typeArguments;
            }
        }
    }
}

