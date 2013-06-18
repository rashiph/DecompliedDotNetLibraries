namespace System.Xaml.MS.Impl
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class XmlNsInfo
    {
        private WeakReference _assembly;
        private IList<CustomAttributeData> _attributeData;
        private ConcurrentDictionary<string, IList<string>> _clrToXmlNs;
        private bool _fullyQualifyAssemblyName;
        private ICollection<AssemblyName> _internalsVisibleTo;
        private IList<XmlNsDefinition> _nsDefs;
        private Dictionary<string, string> _oldToNewNs;
        private Dictionary<string, string> _prefixes;
        private string _rootNamespace;

        internal XmlNsInfo(System.Reflection.Assembly assembly, bool fullyQualifyAssemblyName)
        {
            this._assembly = new WeakReference(assembly);
            this._fullyQualifyAssemblyName = fullyQualifyAssemblyName;
        }

        private void EnsureReflectionOnlyAttributeData()
        {
            if (this._attributeData == null)
            {
                this._attributeData = this.Assembly.GetCustomAttributesData();
            }
        }

        internal static string GetPreferredPrefix(string prefix1, string prefix2)
        {
            if (prefix1.Length < prefix2.Length)
            {
                return prefix1;
            }
            if ((prefix2.Length >= prefix1.Length) && (StringComparer.Ordinal.Compare(prefix1, prefix2) < 0))
            {
                return prefix1;
            }
            return prefix2;
        }

        private ConcurrentDictionary<string, IList<string>> LoadClrToXmlNs()
        {
            ConcurrentDictionary<string, IList<string>> dict = XamlSchemaContext.CreateDictionary<string, IList<string>>();
            System.Reflection.Assembly assembly = this.Assembly;
            if (assembly != null)
            {
                foreach (XmlNsDefinition definition in this.NsDefs)
                {
                    IList<string> list;
                    if (!dict.TryGetValue(definition.ClrNamespace, out list))
                    {
                        list = new List<string>();
                        dict.TryAdd(definition.ClrNamespace, list);
                    }
                    list.Add(definition.XmlNamespace);
                }
                string assemblyName = this._fullyQualifyAssemblyName ? assembly.FullName : XamlSchemaContext.GetAssemblyShortName(assembly);
                foreach (KeyValuePair<string, IList<string>> pair in dict)
                {
                    List<string> list2 = (List<string>) pair.Value;
                    NamespaceComparer comparer = new NamespaceComparer(this, assembly);
                    list2.Sort(new Comparison<string>(comparer.CompareNamespacesByPreference));
                    string uri = ClrNamespaceUriParser.GetUri(pair.Key, assemblyName);
                    list2.Add(uri);
                }
                this.MakeListsImmutable(dict);
            }
            return dict;
        }

        private ICollection<AssemblyName> LoadInternalsVisibleTo()
        {
            List<AssemblyName> result = new List<AssemblyName>();
            System.Reflection.Assembly assembly = this.Assembly;
            if (assembly != null)
            {
                if (assembly.ReflectionOnly)
                {
                    this.EnsureReflectionOnlyAttributeData();
                    foreach (CustomAttributeData data in this._attributeData)
                    {
                        if (LooseTypeExtensions.AssemblyQualifiedNameEquals(data.Constructor.DeclaringType, typeof(InternalsVisibleToAttribute)))
                        {
                            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                            string assemblyName = argument.Value as string;
                            this.LoadInternalsVisibleToHelper(result, assemblyName, assembly);
                        }
                    }
                    return result;
                }
                foreach (InternalsVisibleToAttribute attribute in Attribute.GetCustomAttributes(assembly, typeof(InternalsVisibleToAttribute)))
                {
                    this.LoadInternalsVisibleToHelper(result, attribute.AssemblyName, assembly);
                }
            }
            return result;
        }

        private void LoadInternalsVisibleToHelper(List<AssemblyName> result, string assemblyName, System.Reflection.Assembly assembly)
        {
            if (assemblyName == null)
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadInternalsVisibleTo1", new object[] { assembly.FullName }));
            }
            try
            {
                result.Add(new AssemblyName(assemblyName));
            }
            catch (ArgumentException exception)
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadInternalsVisibleTo2", new object[] { assemblyName, assembly.FullName }), exception);
            }
            catch (FileLoadException exception2)
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadInternalsVisibleTo2", new object[] { assemblyName, assembly.FullName }), exception2);
            }
        }

        private void LoadNsDefHelper(IList<XmlNsDefinition> result, string xmlns, string clrns, System.Reflection.Assembly assembly)
        {
            if (string.IsNullOrEmpty(xmlns) || (clrns == null))
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadXmlnsDefinition", new object[] { assembly.FullName }));
            }
            XmlNsDefinition item = new XmlNsDefinition {
                ClrNamespace = clrns,
                XmlNamespace = xmlns
            };
            result.Add(item);
        }

        private IList<XmlNsDefinition> LoadNsDefs()
        {
            IList<XmlNsDefinition> result = new List<XmlNsDefinition>();
            System.Reflection.Assembly assembly = this.Assembly;
            if (assembly != null)
            {
                if (assembly.ReflectionOnly)
                {
                    this.EnsureReflectionOnlyAttributeData();
                    foreach (CustomAttributeData data in this._attributeData)
                    {
                        if (LooseTypeExtensions.AssemblyQualifiedNameEquals(data.Constructor.DeclaringType, typeof(XmlnsDefinitionAttribute)))
                        {
                            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                            string xmlns = argument.Value as string;
                            CustomAttributeTypedArgument argument2 = data.ConstructorArguments[1];
                            string clrns = argument2.Value as string;
                            this.LoadNsDefHelper(result, xmlns, clrns, assembly);
                        }
                    }
                    return result;
                }
                foreach (Attribute attribute in Attribute.GetCustomAttributes(assembly, typeof(XmlnsDefinitionAttribute)))
                {
                    XmlnsDefinitionAttribute attribute2 = (XmlnsDefinitionAttribute) attribute;
                    string xmlNamespace = attribute2.XmlNamespace;
                    string clrNamespace = attribute2.ClrNamespace;
                    this.LoadNsDefHelper(result, xmlNamespace, clrNamespace, assembly);
                }
            }
            return result;
        }

        private Dictionary<string, string> LoadOldToNewNs()
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            System.Reflection.Assembly assembly = this.Assembly;
            if (assembly != null)
            {
                if (assembly.ReflectionOnly)
                {
                    this.EnsureReflectionOnlyAttributeData();
                    foreach (CustomAttributeData data in this._attributeData)
                    {
                        if (LooseTypeExtensions.AssemblyQualifiedNameEquals(data.Constructor.DeclaringType, typeof(XmlnsCompatibleWithAttribute)))
                        {
                            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                            string oldns = argument.Value as string;
                            CustomAttributeTypedArgument argument2 = data.ConstructorArguments[1];
                            string newns = argument2.Value as string;
                            this.LoadOldToNewNsHelper(result, oldns, newns, assembly);
                        }
                    }
                    return result;
                }
                foreach (Attribute attribute in Attribute.GetCustomAttributes(assembly, typeof(XmlnsCompatibleWithAttribute)))
                {
                    XmlnsCompatibleWithAttribute attribute2 = (XmlnsCompatibleWithAttribute) attribute;
                    this.LoadOldToNewNsHelper(result, attribute2.OldNamespace, attribute2.NewNamespace, assembly);
                }
            }
            return result;
        }

        private void LoadOldToNewNsHelper(Dictionary<string, string> result, string oldns, string newns, System.Reflection.Assembly assembly)
        {
            if (string.IsNullOrEmpty(newns) || string.IsNullOrEmpty(oldns))
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadXmlnsCompat", new object[] { assembly.FullName }));
            }
            if (result.ContainsKey(oldns))
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("DuplicateXmlnsCompat", new object[] { assembly.FullName, oldns }));
            }
            result.Add(oldns, newns);
        }

        private Dictionary<string, string> LoadPrefixes()
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
            System.Reflection.Assembly assembly = this.Assembly;
            if (assembly != null)
            {
                if (assembly.ReflectionOnly)
                {
                    this.EnsureReflectionOnlyAttributeData();
                    foreach (CustomAttributeData data in this._attributeData)
                    {
                        if (LooseTypeExtensions.AssemblyQualifiedNameEquals(data.Constructor.DeclaringType, typeof(XmlnsPrefixAttribute)))
                        {
                            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                            string xmlns = argument.Value as string;
                            CustomAttributeTypedArgument argument2 = data.ConstructorArguments[1];
                            string prefix = argument2.Value as string;
                            this.LoadPrefixesHelper(result, xmlns, prefix, assembly);
                        }
                    }
                    return result;
                }
                foreach (Attribute attribute in Attribute.GetCustomAttributes(assembly, typeof(XmlnsPrefixAttribute)))
                {
                    XmlnsPrefixAttribute attribute2 = (XmlnsPrefixAttribute) attribute;
                    this.LoadPrefixesHelper(result, attribute2.XmlNamespace, attribute2.Prefix, assembly);
                }
            }
            return result;
        }

        private void LoadPrefixesHelper(Dictionary<string, string> result, string xmlns, string prefix, System.Reflection.Assembly assembly)
        {
            string str;
            if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(xmlns))
            {
                throw new XamlSchemaException(System.Xaml.SR.Get("BadXmlnsPrefix", new object[] { assembly.FullName }));
            }
            if (!result.TryGetValue(xmlns, out str) || (GetPreferredPrefix(str, prefix) == prefix))
            {
                result[xmlns] = prefix;
            }
        }

        private string LoadRootNamespace()
        {
            System.Reflection.Assembly element = this.Assembly;
            if (element != null)
            {
                if (element.ReflectionOnly)
                {
                    this.EnsureReflectionOnlyAttributeData();
                    foreach (CustomAttributeData data in this._attributeData)
                    {
                        if (LooseTypeExtensions.AssemblyQualifiedNameEquals(data.Constructor.DeclaringType, typeof(RootNamespaceAttribute)))
                        {
                            CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                            return (argument.Value as string);
                        }
                    }
                    return null;
                }
                RootNamespaceAttribute customAttribute = (RootNamespaceAttribute) Attribute.GetCustomAttribute(element, typeof(RootNamespaceAttribute));
                if (customAttribute != null)
                {
                    return customAttribute.Namespace;
                }
            }
            return null;
        }

        private void MakeListsImmutable(IDictionary<string, IList<string>> dict)
        {
            string[] array = new string[dict.Count];
            dict.Keys.CopyTo(array, 0);
            foreach (string str in array)
            {
                dict[str] = new ReadOnlyCollection<string>(dict[str]);
            }
        }

        internal System.Reflection.Assembly Assembly
        {
            get
            {
                return (System.Reflection.Assembly) this._assembly.Target;
            }
        }

        internal ConcurrentDictionary<string, IList<string>> ClrToXmlNs
        {
            get
            {
                if (this._clrToXmlNs == null)
                {
                    this._clrToXmlNs = this.LoadClrToXmlNs();
                }
                return this._clrToXmlNs;
            }
        }

        internal ICollection<AssemblyName> InternalsVisibleTo
        {
            get
            {
                if (this._internalsVisibleTo == null)
                {
                    this._internalsVisibleTo = this.LoadInternalsVisibleTo();
                }
                return this._internalsVisibleTo;
            }
        }

        internal IList<XmlNsDefinition> NsDefs
        {
            get
            {
                if (this._nsDefs == null)
                {
                    this._nsDefs = this.LoadNsDefs();
                }
                return this._nsDefs;
            }
        }

        internal Dictionary<string, string> OldToNewNs
        {
            get
            {
                if (this._oldToNewNs == null)
                {
                    this._oldToNewNs = this.LoadOldToNewNs();
                }
                return this._oldToNewNs;
            }
        }

        internal Dictionary<string, string> Prefixes
        {
            get
            {
                if (this._prefixes == null)
                {
                    this._prefixes = this.LoadPrefixes();
                }
                return this._prefixes;
            }
        }

        internal string RootNamespace
        {
            get
            {
                if (this._rootNamespace == null)
                {
                    this._rootNamespace = this.LoadRootNamespace() ?? string.Empty;
                }
                return this._rootNamespace;
            }
        }

        private class NamespaceComparer
        {
            private XmlNsInfo _nsInfo;
            private IDictionary<string, int> _subsumeCount;

            public NamespaceComparer(XmlNsInfo nsInfo, Assembly assembly)
            {
                this._nsInfo = nsInfo;
                this._subsumeCount = new Dictionary<string, int>(nsInfo.OldToNewNs.Count);
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                foreach (string str in nsInfo.OldToNewNs.Values)
                {
                    dictionary.Clear();
                    string key = str;
                    do
                    {
                        if (dictionary.ContainsKey(key))
                        {
                            throw new XamlSchemaException(System.Xaml.SR.Get("XmlnsCompatCycle", new object[] { assembly.FullName, key }));
                        }
                        dictionary.Add(key, null);
                        this.IncrementSubsumeCount(key);
                        key = this.GetNewNs(key);
                    }
                    while (key != null);
                }
            }

            public int CompareNamespacesByPreference(string ns1, string ns2)
            {
                string str;
                string str2;
                string str3;
                if (KS.Eq(ns1, ns2))
                {
                    return 0;
                }
                for (str = this.GetNewNs(ns1); str != null; str = this.GetNewNs(str))
                {
                    if (str == ns2)
                    {
                        return 1;
                    }
                }
                for (str = this.GetNewNs(ns2); str != null; str = this.GetNewNs(str))
                {
                    if (str == ns1)
                    {
                        return -1;
                    }
                }
                if (this.GetNewNs(ns1) == null)
                {
                    if (this.GetNewNs(ns2) != null)
                    {
                        return -1;
                    }
                }
                else if (this.GetNewNs(ns2) == null)
                {
                    return 1;
                }
                int num = 0;
                int num2 = 0;
                this._subsumeCount.TryGetValue(ns1, out num);
                this._subsumeCount.TryGetValue(ns2, out num2);
                if (num > num2)
                {
                    return -1;
                }
                if (num2 > num)
                {
                    return 1;
                }
                this._nsInfo.Prefixes.TryGetValue(ns1, out str2);
                this._nsInfo.Prefixes.TryGetValue(ns2, out str3);
                if (string.IsNullOrEmpty(str2))
                {
                    if (!string.IsNullOrEmpty(str3))
                    {
                        return 1;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(str3))
                    {
                        return -1;
                    }
                    if (str2.Length < str3.Length)
                    {
                        return -1;
                    }
                    if (str3.Length < str2.Length)
                    {
                        return 1;
                    }
                }
                return StringComparer.Ordinal.Compare(ns1, ns2);
            }

            private string GetNewNs(string oldNs)
            {
                string str;
                this._nsInfo.OldToNewNs.TryGetValue(oldNs, out str);
                return str;
            }

            private void IncrementSubsumeCount(string ns)
            {
                int num;
                this._subsumeCount.TryGetValue(ns, out num);
                num++;
                this._subsumeCount[ns] = num;
            }
        }

        internal class XmlNsDefinition
        {
            public string ClrNamespace { get; set; }

            public string XmlNamespace { get; set; }
        }
    }
}

