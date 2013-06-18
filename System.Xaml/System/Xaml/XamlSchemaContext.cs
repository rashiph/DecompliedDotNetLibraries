namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;
    using System.Xaml.MS.Impl;
    using System.Xaml.Schema;

    public class XamlSchemaContext
    {
        private AssemblyLoadHandler _assemblyLoadHandler;
        private bool _isGCCallbackPending;
        private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> _masterMemberList;
        private ConcurrentDictionary<Type, XamlType> _masterTypeList;
        private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> _masterValueConverterList;
        private ConcurrentDictionary<string, XamlNamespace> _namespaceByUriList;
        private IList<string> _nonClrNamespaces;
        private ConcurrentDictionary<string, string> _preferredPrefixes;
        private readonly ReadOnlyCollection<Assembly> _referenceAssemblies;
        private AssemblyName[] _referenceAssemblyNames;
        private readonly XamlSchemaContextSettings _settings;
        private object _syncAccessingUnexaminedAssemblies;
        private object _syncExaminingAssemblies;
        private IList<Assembly> _unexaminedAssemblies;
        private ConcurrentDictionary<string, string> _xmlNsCompatDict;
        private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfo;
        private ConcurrentDictionary<WeakRefKey, XmlNsInfo> _xmlnsInfoForDynamicAssemblies;
        private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfoForUnreferencedAssemblies;
        private const int ConcurrencyLevel = 1;
        private const int DictionaryCapacity = 0x11;

        public XamlSchemaContext() : this(null, null)
        {
        }

        public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies) : this(referenceAssemblies, null)
        {
        }

        public XamlSchemaContext(XamlSchemaContextSettings settings) : this(null, settings)
        {
        }

        public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings)
        {
            if (referenceAssemblies != null)
            {
                List<Assembly> list = new List<Assembly>(referenceAssemblies);
                this._referenceAssemblies = new ReadOnlyCollection<Assembly>(list);
            }
            this._settings = (settings != null) ? new XamlSchemaContextSettings(settings) : new XamlSchemaContextSettings();
            this._syncExaminingAssemblies = new object();
            this.InitializeAssemblyLoadHook();
        }

        internal bool AreInternalsVisibleTo(Assembly fromAssembly, Assembly toAssembly)
        {
            if (fromAssembly.Equals(toAssembly))
            {
                return true;
            }
            ICollection<AssemblyName> internalsVisibleTo = this.GetXmlNsInfo(fromAssembly).InternalsVisibleTo;
            if (internalsVisibleTo.Count != 0)
            {
                AssemblyName name = new AssemblyName(toAssembly.FullName);
                foreach (AssemblyName name2 in internalsVisibleTo)
                {
                    if (name2.Name == name.Name)
                    {
                        byte[] publicKeyToken = name2.GetPublicKeyToken();
                        if (publicKeyToken == null)
                        {
                            return true;
                        }
                        byte[] curKeyToken = name.GetPublicKeyToken();
                        return SafeSecurityHelper.IsSameKeyToken(publicKeyToken, curKeyToken);
                    }
                }
            }
            return false;
        }

        private static bool AssemblySatisfiesReference(AssemblyName assemblyName, AssemblyName reference)
        {
            if (reference.Name != assemblyName.Name)
            {
                return false;
            }
            if ((reference.Version != null) && !reference.Version.Equals(assemblyName.Version))
            {
                return false;
            }
            if ((reference.CultureInfo != null) && !reference.CultureInfo.Equals(assemblyName.CultureInfo))
            {
                return false;
            }
            byte[] publicKeyToken = reference.GetPublicKeyToken();
            if (publicKeyToken != null)
            {
                byte[] curKeyToken = assemblyName.GetPublicKeyToken();
                if (!SafeSecurityHelper.IsSameKeyToken(publicKeyToken, curKeyToken))
                {
                    return false;
                }
            }
            return true;
        }

        private void CleanupCollectedAssemblies()
        {
            bool flag = false;
            lock (this._syncAccessingUnexaminedAssemblies)
            {
                this._isGCCallbackPending = false;
                if (this._unexaminedAssemblies is WeakReferenceList<Assembly>)
                {
                    for (int i = this._unexaminedAssemblies.Count - 1; i >= 0; i--)
                    {
                        Assembly assembly = this._unexaminedAssemblies[i];
                        if (assembly == null)
                        {
                            this._unexaminedAssemblies.RemoveAt(i);
                        }
                        else if (assembly.IsDynamic)
                        {
                            flag = true;
                        }
                    }
                }
            }
            lock (this._syncExaminingAssemblies)
            {
                if (this._xmlnsInfoForDynamicAssemblies != null)
                {
                    foreach (WeakRefKey key in this._xmlnsInfoForDynamicAssemblies.Keys)
                    {
                        if (key.IsAlive)
                        {
                            flag = true;
                        }
                        else
                        {
                            XmlNsInfo info;
                            this._xmlnsInfoForDynamicAssemblies.TryRemove(key, out info);
                        }
                    }
                }
            }
            if (flag)
            {
                this.RegisterAssemblyCleanup();
            }
        }

        private static void CleanupCollectedAssemblies(object schemaContextWeakRef)
        {
            WeakReference reference = (WeakReference) schemaContextWeakRef;
            XamlSchemaContext target = reference.Target as XamlSchemaContext;
            if (target != null)
            {
                target.CleanupCollectedAssemblies();
            }
        }

        internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>()
        {
            return new ConcurrentDictionary<K, V>(1, 0x11);
        }

        internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>(IEqualityComparer<K> comparer)
        {
            return new ConcurrentDictionary<K, V>(1, 0x11, comparer);
        }

        private IEnumerable<XmlNsInfo> EnumerateStaticAndDynamicXmlnsInfos()
        {
            foreach (XmlNsInfo iteratorVariable0 in this.XmlnsInfo.Values)
            {
                yield return iteratorVariable0;
            }
            foreach (XmlNsInfo iteratorVariable1 in this.XmlnsInfoForDynamicAssemblies.Values)
            {
                yield return iteratorVariable1;
            }
        }

        private IEnumerable<XmlNsInfo> EnumerateXmlnsInfos()
        {
            if (this._xmlnsInfoForDynamicAssemblies == null)
            {
                return this.XmlnsInfo.Values;
            }
            return this.EnumerateStaticAndDynamicXmlnsInfos();
        }

        ~XamlSchemaContext()
        {
            try
            {
                if ((this._assemblyLoadHandler != null) && !Environment.HasShutdownStarted)
                {
                    this._assemblyLoadHandler.Unhook();
                }
            }
            catch
            {
            }
        }

        public virtual IEnumerable<string> GetAllXamlNamespaces()
        {
            this.UpdateXmlNsInfo();
            IList<string> list = this._nonClrNamespaces;
            if (list == null)
            {
                lock (this._syncExaminingAssemblies)
                {
                    list = new List<string>();
                    foreach (KeyValuePair<string, XamlNamespace> pair in this.NamespaceByUriList)
                    {
                        if (pair.Value.IsResolved && !pair.Value.IsClrNamespace)
                        {
                            list.Add(pair.Key);
                        }
                    }
                    list = new ReadOnlyCollection<string>(list);
                    this._nonClrNamespaces = list;
                }
            }
            return list;
        }

        public virtual ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
        {
            this.UpdateXmlNsInfo();
            return this.GetXamlNamespace(xamlNamespace).GetAllXamlTypes();
        }

        internal static string GetAssemblyShortName(Assembly assembly)
        {
            string fullName = assembly.FullName;
            return fullName.Substring(0, fullName.IndexOf(','));
        }

        internal virtual XamlMember GetAttachableEvent(string name, MethodInfo adder)
        {
            XamlMember member;
            ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(adder, null);
            if (!this.MasterMemberList.TryGetValue(key, out member))
            {
                member = new XamlMember(name, adder, this);
                member = TryAdd<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(this.MasterMemberList, key, member);
            }
            return member;
        }

        internal virtual XamlMember GetAttachableProperty(string name, MethodInfo getter, MethodInfo setter)
        {
            XamlMember member;
            ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(getter, setter);
            if (!this.MasterMemberList.TryGetValue(key, out member))
            {
                member = new XamlMember(name, getter, setter, this);
                member = TryAdd<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(this.MasterMemberList, key, member);
            }
            return member;
        }

        private string GetCompatibleNamespace(string oldNs)
        {
            string str = null;
            Assembly assembly = null;
            lock (this._syncExaminingAssemblies)
            {
                foreach (XmlNsInfo info in this.EnumerateXmlnsInfos())
                {
                    string str2;
                    Assembly assembly2 = info.Assembly;
                    if (assembly2 == null)
                    {
                        continue;
                    }
                    IDictionary<string, string> oldToNewNs = null;
                    if (this.ReferenceAssemblies == null)
                    {
                        try
                        {
                            oldToNewNs = info.OldToNewNs;
                            goto Label_006D;
                        }
                        catch (Exception exception)
                        {
                            if (CriticalExceptions.IsCriticalException(exception))
                            {
                                throw;
                            }
                            continue;
                        }
                    }
                    oldToNewNs = info.OldToNewNs;
                Label_006D:
                    if (oldToNewNs.TryGetValue(oldNs, out str2))
                    {
                        if ((str != null) && (str != str2))
                        {
                            throw new XamlSchemaException(System.Xaml.SR.Get("DuplicateXmlnsCompatAcrossAssemblies", new object[] { assembly.FullName, assembly2.FullName, oldNs }));
                        }
                        str = str2;
                        assembly = assembly2;
                    }
                }
            }
            return str;
        }

        internal virtual XamlMember GetEvent(EventInfo ei)
        {
            XamlMember member;
            ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(ei, null);
            if (!this.MasterMemberList.TryGetValue(key, out member))
            {
                member = new XamlMember(ei, this);
                member = TryAdd<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(this.MasterMemberList, key, member);
            }
            return member;
        }

        public virtual string GetPreferredPrefix(string xmlns)
        {
            string prefixForClrNs;
            if (xmlns == null)
            {
                throw new ArgumentNullException("xmlns");
            }
            this.UpdateXmlNsInfo();
            if (this._preferredPrefixes == null)
            {
                this.InitializePreferredPrefixes();
            }
            if (this._preferredPrefixes.TryGetValue(xmlns, out prefixForClrNs))
            {
                return prefixForClrNs;
            }
            if (XamlLanguage.XamlNamespaces.Contains(xmlns))
            {
                prefixForClrNs = "x";
            }
            else
            {
                string str2;
                string str3;
                if (ClrNamespaceUriParser.TryParseUri(xmlns, out str2, out str3))
                {
                    prefixForClrNs = this.GetPrefixForClrNs(str2, str3);
                }
                else
                {
                    prefixForClrNs = "p";
                }
            }
            return TryAdd<string, string>(this._preferredPrefixes, xmlns, prefixForClrNs);
        }

        private string GetPrefixForClrNs(string clrNs, string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return "local";
            }
            StringBuilder builder = new StringBuilder();
            foreach (string str in clrNs.Split(new char[] { '.' }))
            {
                if (!string.IsNullOrEmpty(str))
                {
                    builder.Append(char.ToLower(str[0], TypeConverterHelper.InvariantEnglishUS));
                }
            }
            if (builder.Length <= 0)
            {
                return "local";
            }
            string a = builder.ToString();
            if (KS.Eq(a, "x"))
            {
                return "p";
            }
            if (KS.Eq(a, "xml"))
            {
                return "p";
            }
            return a;
        }

        internal virtual XamlMember GetProperty(PropertyInfo pi)
        {
            XamlMember member;
            ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(pi, null);
            if (!this.MasterMemberList.TryGetValue(key, out member))
            {
                member = new XamlMember(pi, this);
                member = TryAdd<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(this.MasterMemberList, key, member);
            }
            return member;
        }

        internal string GetRootNamespace(Assembly asm)
        {
            return this.GetXmlNsInfo(asm).RootNamespace;
        }

        protected internal XamlValueConverter<TConverterBase> GetValueConverter<TConverterBase>(Type converterType, XamlType targetType) where TConverterBase: class
        {
            object obj2;
            ReferenceEqualityTuple<Type, XamlType, Type> key = new ReferenceEqualityTuple<Type, XamlType, Type>(converterType, targetType, typeof(TConverterBase));
            if (!this.MasterValueConverterList.TryGetValue(key, out obj2))
            {
                obj2 = new XamlValueConverter<TConverterBase>(converterType, targetType);
                obj2 = TryAdd<ReferenceEqualityTuple<Type, XamlType, Type>, object>(this.MasterValueConverterList, key, obj2);
            }
            return (XamlValueConverter<TConverterBase>) obj2;
        }

        public virtual XamlDirective GetXamlDirective(string xamlNamespace, string name)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (XamlLanguage.XamlNamespaces.Contains(xamlNamespace))
            {
                return XamlLanguage.LookupXamlDirective(name);
            }
            if (XamlLanguage.XmlNamespaces.Contains(xamlNamespace))
            {
                return XamlLanguage.LookupXmlDirective(name);
            }
            return null;
        }

        private XamlNamespace GetXamlNamespace(string xmlns)
        {
            XamlNamespace namespace2 = null;
            string str;
            string str2;
            if (this.NamespaceByUriList.TryGetValue(xmlns, out namespace2))
            {
                return namespace2;
            }
            if (ClrNamespaceUriParser.TryParseUri(xmlns, out str, out str2))
            {
                namespace2 = new XamlNamespace(this, str, str2);
            }
            else
            {
                namespace2 = new XamlNamespace(this);
            }
            return TryAdd<string, XamlNamespace>(this.NamespaceByUriList, xmlns, namespace2);
        }

        internal ReadOnlyCollection<string> GetXamlNamespaces(XamlType type)
        {
            Type underlyingType = type.UnderlyingType;
            if ((underlyingType == null) || (underlyingType.Assembly == null))
            {
                return null;
            }
            if (XamlLanguage.AllTypes.Contains(type))
            {
                IList<string> xmlNsMappings = this.GetXmlNsMappings(underlyingType.Assembly, underlyingType.Namespace);
                List<string> list2 = new List<string>();
                list2.AddRange(XamlLanguage.XamlNamespaces);
                list2.AddRange(xmlNsMappings);
                return list2.AsReadOnly();
            }
            return this.GetXmlNsMappings(underlyingType.Assembly, underlyingType.Namespace);
        }

        public virtual XamlType GetXamlType(Type type)
        {
            return this.GetXamlType(type, XamlLanguage.TypeAlias(type));
        }

        public XamlType GetXamlType(XamlTypeName xamlTypeName)
        {
            if (xamlTypeName == null)
            {
                throw new ArgumentNullException("xamlTypeName");
            }
            if (xamlTypeName.Name == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ReferenceIsNull", new object[] { "xamlTypeName.Name" }), "xamlTypeName");
            }
            if (xamlTypeName.Namespace == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ReferenceIsNull", new object[] { "xamlTypeName.Namespace" }), "xamlTypeName");
            }
            XamlType[] typeArguments = null;
            if (xamlTypeName.HasTypeArgs)
            {
                typeArguments = new XamlType[xamlTypeName.TypeArguments.Count];
                for (int i = 0; i < xamlTypeName.TypeArguments.Count; i++)
                {
                    if (xamlTypeName.TypeArguments[i] == null)
                    {
                        throw new ArgumentException(System.Xaml.SR.Get("CollectionCannotContainNulls", new object[] { "xamlTypeName.TypeArguments" }));
                    }
                    typeArguments[i] = this.GetXamlType(xamlTypeName.TypeArguments[i]);
                    if (typeArguments[i] == null)
                    {
                        return null;
                    }
                }
            }
            return this.GetXamlType(xamlTypeName.Namespace, xamlTypeName.Name, typeArguments);
        }

        internal XamlType GetXamlType(Type type, string alias)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            XamlType type2 = null;
            if (!this.MasterTypeList.TryGetValue(type, out type2))
            {
                type2 = new XamlType(alias, type, this, null, null);
                type2 = TryAdd<Type, XamlType>(this.MasterTypeList, type, type2);
            }
            return type2;
        }

        protected internal virtual XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (typeArguments != null)
            {
                foreach (XamlType type in typeArguments)
                {
                    if (type == null)
                    {
                        throw new ArgumentException(System.Xaml.SR.Get("CollectionCannotContainNulls", new object[] { "typeArguments" }));
                    }
                    if (type.UnderlyingType == null)
                    {
                        return null;
                    }
                }
            }
            XamlType xamlType = null;
            if ((typeArguments == null) || (typeArguments.Length == 0))
            {
                xamlType = XamlLanguage.LookupXamlType(xamlNamespace, name);
                if (xamlType != null)
                {
                    if (this.FullyQualifyAssemblyNamesInClrNamespaces)
                    {
                        xamlType = this.GetXamlType(xamlType.UnderlyingType);
                    }
                    return xamlType;
                }
            }
            XamlNamespace namespace2 = this.GetXamlNamespace(xamlNamespace);
            int revisionNumber = namespace2.RevisionNumber;
            xamlType = namespace2.GetXamlType(name, typeArguments);
            if ((xamlType == null) && !namespace2.IsClrNamespace)
            {
                this.UpdateXmlNsInfo();
                if (namespace2.RevisionNumber > revisionNumber)
                {
                    xamlType = namespace2.GetXamlType(name, typeArguments);
                }
            }
            return xamlType;
        }

        private XmlNsInfo GetXmlNsInfo(Assembly assembly)
        {
            XmlNsInfo info;
            if ((this.XmlnsInfo.TryGetValue(assembly, out info) || (((this._xmlnsInfoForDynamicAssemblies != null) && assembly.IsDynamic) && this._xmlnsInfoForDynamicAssemblies.TryGetValue(new WeakRefKey(assembly), out info))) || ((this._xmlnsInfoForUnreferencedAssemblies != null) && this._xmlnsInfoForUnreferencedAssemblies.TryGetValue(assembly, out info)))
            {
                return info;
            }
            bool flag = false;
            if (this._referenceAssemblies != null)
            {
                foreach (Assembly assembly2 in this._referenceAssemblies)
                {
                    if (object.ReferenceEquals(assembly2, assembly))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            else
            {
                flag = !assembly.ReflectionOnly && typeof(object).Assembly.GetType().IsAssignableFrom(assembly.GetType());
            }
            info = new XmlNsInfo(assembly, this.FullyQualifyAssemblyNamesInClrNamespaces);
            if (flag)
            {
                if (assembly.IsDynamic && (this._referenceAssemblies == null))
                {
                    info = TryAdd<WeakRefKey, XmlNsInfo>(this.XmlnsInfoForDynamicAssemblies, new WeakRefKey(assembly), info);
                    this.RegisterAssemblyCleanup();
                    return info;
                }
                return TryAdd<Assembly, XmlNsInfo>(this.XmlnsInfo, assembly, info);
            }
            return TryAdd<Assembly, XmlNsInfo>(this.XmlnsInfoForUnreferencedAssemblies, assembly, info);
        }

        private ReadOnlyCollection<string> GetXmlNsMappings(Assembly assembly, string clrNs)
        {
            IList<string> list;
            ConcurrentDictionary<string, IList<string>> clrToXmlNs = this.GetXmlNsInfo(assembly).ClrToXmlNs;
            clrNs = clrNs ?? string.Empty;
            if (!clrToXmlNs.TryGetValue(clrNs, out list))
            {
                string assemblyName = this.FullyQualifyAssemblyNamesInClrNamespaces ? assembly.FullName : GetAssemblyShortName(assembly);
                string uri = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
                list = new List<string> { uri }.AsReadOnly();
                TryAdd<string, IList<string>>(clrToXmlNs, clrNs, list);
            }
            return (ReadOnlyCollection<string>) list;
        }

        private void InitializeAssemblyLoadHook()
        {
            this._syncAccessingUnexaminedAssemblies = new object();
            if (this.ReferenceAssemblies == null)
            {
                this._assemblyLoadHandler = new AssemblyLoadHandler(this);
                this._assemblyLoadHandler.Hook();
                lock (this._syncAccessingUnexaminedAssemblies)
                {
                    Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    this._unexaminedAssemblies = new WeakReferenceList<Assembly>(assemblies.Length);
                    bool flag = false;
                    foreach (Assembly assembly in assemblies)
                    {
                        this._unexaminedAssemblies.Add(assembly);
                        if (assembly.IsDynamic)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        this.RegisterAssemblyCleanup();
                    }
                    return;
                }
            }
            this._unexaminedAssemblies = new List<Assembly>(this.ReferenceAssemblies);
        }

        private void InitializePreferredPrefixes()
        {
            lock (this._syncExaminingAssemblies)
            {
                ConcurrentDictionary<string, string> prefixDict = CreateDictionary<string, string>();
                foreach (XmlNsInfo info in this.EnumerateXmlnsInfos())
                {
                    this.UpdatePreferredPrefixes(info, prefixDict);
                }
                this._preferredPrefixes = prefixDict;
            }
        }

        protected internal virtual Assembly OnAssemblyResolve(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return null;
            }
            if (this._referenceAssemblies != null)
            {
                return this.ResolveReferenceAssembly(assemblyName);
            }
            return this.ResolveAssembly(assemblyName);
        }

        private void RegisterAssemblyCleanup()
        {
            lock (this._syncAccessingUnexaminedAssemblies)
            {
                if (!this._isGCCallbackPending)
                {
                    GCNotificationToken.RegisterCallback(new WaitCallback(XamlSchemaContext.CleanupCollectedAssemblies), new WeakReference(this));
                    this._isGCCallbackPending = true;
                }
            }
        }

        private Assembly ResolveAssembly(string assemblyName)
        {
            AssemblyName name = new AssemblyName(assemblyName);
            Assembly loadedAssembly = SafeSecurityHelper.GetLoadedAssembly(name);
            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }
            try
            {
                byte[] publicKeyToken = name.GetPublicKeyToken();
                if (((name.Version != null) || (name.CultureInfo != null)) || (publicKeyToken != null))
                {
                    try
                    {
                        return Assembly.Load(assemblyName);
                    }
                    catch (Exception exception)
                    {
                        if (CriticalExceptions.IsCriticalException(exception))
                        {
                            throw;
                        }
                        AssemblyName assemblyRef = new AssemblyName(name.Name);
                        if (publicKeyToken != null)
                        {
                            assemblyRef.SetPublicKeyToken(publicKeyToken);
                        }
                        return Assembly.Load(assemblyRef);
                    }
                }
                return Assembly.LoadWithPartialName(assemblyName);
            }
            catch (Exception exception2)
            {
                if (CriticalExceptions.IsCriticalException(exception2))
                {
                    throw;
                }
                return null;
            }
        }

        private Assembly ResolveReferenceAssembly(string assemblyName)
        {
            AssemblyName reference = new AssemblyName(assemblyName);
            if (this._referenceAssemblyNames == null)
            {
                AssemblyName[] nameArray = new AssemblyName[this._referenceAssemblies.Count];
                Interlocked.CompareExchange<AssemblyName[]>(ref this._referenceAssemblyNames, nameArray, null);
            }
            for (int i = 0; i < this._referenceAssemblies.Count; i++)
            {
                AssemblyName name1 = this._referenceAssemblyNames[i];
                if (this._referenceAssemblyNames[i] == null)
                {
                    this._referenceAssemblyNames[i] = new AssemblyName(this._referenceAssemblies[i].FullName);
                }
                if (AssemblySatisfiesReference(this._referenceAssemblyNames[i], reference))
                {
                    return this._referenceAssemblies[i];
                }
            }
            return null;
        }

        private void SchemaContextAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
        {
            lock (this._syncAccessingUnexaminedAssemblies)
            {
                if (!args.LoadedAssembly.ReflectionOnly && !this._unexaminedAssemblies.Contains(args.LoadedAssembly))
                {
                    this._unexaminedAssemblies.Add(args.LoadedAssembly);
                    if (args.LoadedAssembly.IsDynamic)
                    {
                        this.RegisterAssemblyCleanup();
                    }
                }
            }
        }

        internal static V TryAdd<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value)
        {
            if (dictionary.TryAdd(key, value))
            {
                return value;
            }
            return dictionary[key];
        }

        public virtual bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
        {
            if (xamlNamespace == null)
            {
                throw new ArgumentNullException("xamlNamespace");
            }
            if (this.XmlNsCompatDict.TryGetValue(xamlNamespace, out compatibleNamespace))
            {
                return true;
            }
            this.UpdateXmlNsInfo();
            compatibleNamespace = this.GetCompatibleNamespace(xamlNamespace);
            if (compatibleNamespace == null)
            {
                compatibleNamespace = xamlNamespace;
            }
            if (this.GetXamlNamespace(compatibleNamespace).IsResolved)
            {
                compatibleNamespace = TryAdd<string, string>(this.XmlNsCompatDict, xamlNamespace, compatibleNamespace);
                return true;
            }
            compatibleNamespace = null;
            return false;
        }

        internal static V TryUpdate<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value, V comparand)
        {
            if (dictionary.TryUpdate(key, value, comparand))
            {
                return value;
            }
            return dictionary[key];
        }

        private bool UpdateNamespaceByUriList(XmlNsInfo nsInfo)
        {
            bool flag = false;
            foreach (XmlNsInfo.XmlNsDefinition definition in nsInfo.NsDefs)
            {
                AssemblyNamespacePair pair = new AssemblyNamespacePair(nsInfo.Assembly, definition.ClrNamespace);
                this.GetXamlNamespace(definition.XmlNamespace).AddAssemblyNamespacePair(pair);
                flag = true;
            }
            return flag;
        }

        private void UpdatePreferredPrefixes(XmlNsInfo newNamespaces, ConcurrentDictionary<string, string> prefixDict)
        {
            foreach (KeyValuePair<string, string> pair in newNamespaces.Prefixes)
            {
                string str;
                string preferredPrefix = pair.Value;
                if (!prefixDict.TryGetValue(pair.Key, out str))
                {
                    str = TryAdd<string, string>(prefixDict, pair.Key, preferredPrefix);
                }
                while (str != preferredPrefix)
                {
                    preferredPrefix = XmlNsInfo.GetPreferredPrefix(str, preferredPrefix);
                    if (!KS.Eq(preferredPrefix, str))
                    {
                        str = TryUpdate<string, string>(prefixDict, pair.Key, preferredPrefix, str);
                    }
                }
            }
        }

        private void UpdateXmlNsInfo()
        {
            bool flag = false;
            lock (this._syncExaminingAssemblies)
            {
                IList<Assembly> list;
                lock (this._syncAccessingUnexaminedAssemblies)
                {
                    list = this._unexaminedAssemblies;
                    this._unexaminedAssemblies = new WeakReferenceList<Assembly>(0);
                }
                bool flag2 = this.ReferenceAssemblies != null;
                for (int i = 0; i < list.Count; i++)
                {
                    Assembly assembly = list[i];
                    if (assembly != null)
                    {
                        XmlNsInfo xmlNsInfo = this.GetXmlNsInfo(assembly);
                        try
                        {
                            if (this.UpdateXmlNsInfo(xmlNsInfo))
                            {
                                flag = true;
                            }
                        }
                        catch (Exception exception)
                        {
                            if (flag2 || CriticalExceptions.IsCriticalException(exception))
                            {
                                lock (this._syncAccessingUnexaminedAssemblies)
                                {
                                    for (int j = i; j < list.Count; j++)
                                    {
                                        this._unexaminedAssemblies.Add(list[j]);
                                    }
                                }
                                throw;
                            }
                        }
                    }
                }
                if (flag && (this._nonClrNamespaces != null))
                {
                    this._nonClrNamespaces = null;
                }
            }
        }

        private bool UpdateXmlNsInfo(XmlNsInfo nsInfo)
        {
            bool flag = this.UpdateNamespaceByUriList(nsInfo);
            if (this._preferredPrefixes != null)
            {
                this.UpdatePreferredPrefixes(nsInfo, this._preferredPrefixes);
            }
            return flag;
        }

        public bool FullyQualifyAssemblyNamesInClrNamespaces
        {
            get
            {
                return this._settings.FullyQualifyAssemblyNamesInClrNamespaces;
            }
        }

        private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> MasterMemberList
        {
            get
            {
                if (this._masterMemberList == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>>(ref this._masterMemberList, CreateDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(), null);
                }
                return this._masterMemberList;
            }
        }

        private ConcurrentDictionary<Type, XamlType> MasterTypeList
        {
            get
            {
                if (this._masterTypeList == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<Type, XamlType>>(ref this._masterTypeList, CreateDictionary<Type, XamlType>(ReferenceEqualityComparer<Type>.Singleton), null);
                }
                return this._masterTypeList;
            }
        }

        private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> MasterValueConverterList
        {
            get
            {
                if (this._masterValueConverterList == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object>>(ref this._masterValueConverterList, CreateDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object>(), null);
                }
                return this._masterValueConverterList;
            }
        }

        private ConcurrentDictionary<string, XamlNamespace> NamespaceByUriList
        {
            get
            {
                if (this._namespaceByUriList == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<string, XamlNamespace>>(ref this._namespaceByUriList, CreateDictionary<string, XamlNamespace>(), null);
                }
                return this._namespaceByUriList;
            }
        }

        public IList<Assembly> ReferenceAssemblies
        {
            get
            {
                return this._referenceAssemblies;
            }
        }

        public bool SupportMarkupExtensionsWithDuplicateArity
        {
            get
            {
                return this._settings.SupportMarkupExtensionsWithDuplicateArity;
            }
        }

        private ConcurrentDictionary<string, string> XmlNsCompatDict
        {
            get
            {
                if (this._xmlNsCompatDict == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<string, string>>(ref this._xmlNsCompatDict, CreateDictionary<string, string>(), null);
                }
                return this._xmlNsCompatDict;
            }
        }

        private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfo
        {
            get
            {
                if (this._xmlnsInfo == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<Assembly, XmlNsInfo>>(ref this._xmlnsInfo, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
                }
                return this._xmlnsInfo;
            }
        }

        private ConcurrentDictionary<WeakRefKey, XmlNsInfo> XmlnsInfoForDynamicAssemblies
        {
            get
            {
                if (this._xmlnsInfoForDynamicAssemblies == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<WeakRefKey, XmlNsInfo>>(ref this._xmlnsInfoForDynamicAssemblies, CreateDictionary<WeakRefKey, XmlNsInfo>(), null);
                }
                return this._xmlnsInfoForDynamicAssemblies;
            }
        }

        private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfoForUnreferencedAssemblies
        {
            get
            {
                if (this._xmlnsInfoForUnreferencedAssemblies == null)
                {
                    Interlocked.CompareExchange<ConcurrentDictionary<Assembly, XmlNsInfo>>(ref this._xmlnsInfoForUnreferencedAssemblies, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
                }
                return this._xmlnsInfoForUnreferencedAssemblies;
            }
        }


        private class AssemblyLoadHandler
        {
            private WeakReference schemaContextRef;

            public AssemblyLoadHandler(XamlSchemaContext schemaContext)
            {
                this.schemaContextRef = new WeakReference(schemaContext);
            }

            [SecuritySafeCritical]
            public void Hook()
            {
                AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(this.OnAssemblyLoad);
            }

            private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
            {
                XamlSchemaContext target = (XamlSchemaContext) this.schemaContextRef.Target;
                if (target != null)
                {
                    target.SchemaContextAssemblyLoadEventHandler(sender, args);
                }
            }

            [SecuritySafeCritical]
            public void Unhook()
            {
                AppDomain.CurrentDomain.AssemblyLoad -= new AssemblyLoadEventHandler(this.OnAssemblyLoad);
            }
        }

        private class WeakReferenceList<T> : List<WeakReference>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T: class
        {
            public WeakReferenceList(int capacity) : base(capacity)
            {
            }

            private IEnumerable<T> Enumerate()
            {
                foreach (WeakReference iteratorVariable0 in this)
                {
                    yield return (T) iteratorVariable0.Target;
                }
            }

            void ICollection<T>.Add(T item)
            {
                base.Add(new WeakReference(item));
            }

            bool ICollection<T>.Contains(T item)
            {
                foreach (WeakReference reference in this)
                {
                    if (item == reference.Target)
                    {
                        return true;
                    }
                }
                return false;
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                for (int i = 0; i < base.Count; i++)
                {
                    array[i + arrayIndex] = (T) base[i].Target;
                }
            }

            bool ICollection<T>.Remove(T item)
            {
                throw new NotSupportedException();
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.Enumerate().GetEnumerator();
            }

            int IList<T>.IndexOf(T item)
            {
                throw new NotSupportedException();
            }

            void IList<T>.Insert(int index, T item)
            {
                base.Insert(index, new WeakReference(item));
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>) this).GetEnumerator();
            }

            bool ICollection<T>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            T IList<T>.this[int index]
            {
                get
                {
                    return (T) base[index].Target;
                }
                set
                {
                    base[index] = new WeakReference(value);
                }
            }

            [CompilerGenerated]
            private sealed class <Enumerate>d__14 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
            {
                private int <>1__state;
                private T <>2__current;
                public XamlSchemaContext.WeakReferenceList<T> <>4__this;
                public IEnumerator<WeakReference> <>7__wrap16;
                private int <>l__initialThreadId;
                public WeakReference <weakRef>5__15;

                [DebuggerHidden]
                public <Enumerate>d__14(int <>1__state)
                {
                    this.<>1__state = <>1__state;
                    this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
                }

                private void <>m__Finally17()
                {
                    this.<>1__state = -1;
                    if (this.<>7__wrap16 != null)
                    {
                        this.<>7__wrap16.Dispose();
                    }
                }

                private bool MoveNext()
                {
                    bool flag;
                    try
                    {
                        switch (this.<>1__state)
                        {
                            case 0:
                                this.<>1__state = -1;
                                this.<>7__wrap16 = this.<>4__this.GetEnumerator();
                                this.<>1__state = 1;
                                goto Label_0075;

                            case 2:
                                this.<>1__state = 1;
                                goto Label_0075;

                            default:
                                goto Label_0088;
                        }
                    Label_003C:
                        this.<weakRef>5__15 = this.<>7__wrap16.Current;
                        this.<>2__current = (T) this.<weakRef>5__15.Target;
                        this.<>1__state = 2;
                        return true;
                    Label_0075:
                        if (this.<>7__wrap16.MoveNext())
                        {
                            goto Label_003C;
                        }
                        this.<>m__Finally17();
                    Label_0088:
                        flag = false;
                    }
                    fault
                    {
                        this.System.IDisposable.Dispose();
                    }
                    return flag;
                }

                [DebuggerHidden]
                IEnumerator<T> IEnumerable<T>.GetEnumerator()
                {
                    if ((Thread.CurrentThread.ManagedThreadId == this.<>l__initialThreadId) && (this.<>1__state == -2))
                    {
                        this.<>1__state = 0;
                        return (XamlSchemaContext.WeakReferenceList<T>.<Enumerate>d__14) this;
                    }
                    return new XamlSchemaContext.WeakReferenceList<T>.<Enumerate>d__14(0) { <>4__this = this.<>4__this };
                }

                [DebuggerHidden]
                IEnumerator IEnumerable.GetEnumerator()
                {
                    return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
                }

                [DebuggerHidden]
                void IEnumerator.Reset()
                {
                    throw new NotSupportedException();
                }

                void IDisposable.Dispose()
                {
                    switch (this.<>1__state)
                    {
                        case 1:
                        case 2:
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally17();
                            }
                            return;
                    }
                }

                T IEnumerator<T>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.<>2__current;
                    }
                }
            }
        }
    }
}

