namespace System.Resources
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class ResourceManager
    {
        [OptionalField(VersionAdded=1)]
        private Assembly _callingAssembly;
        private static bool _checkedConfigFile;
        [OptionalField]
        private UltimateResourceFallbackLocation _fallbackLoc;
        private bool _ignoreCase;
        private static Hashtable _installedSatelliteInfo;
        private Type _locationInfo;
        [OptionalField]
        private bool _lookedForSatelliteContractVersion;
        private static readonly Type _minResourceSet = typeof(ResourceSet);
        private CultureInfo _neutralResourcesCulture;
        [NonSerialized]
        private Dictionary<string, ResourceSet> _resourceSets;
        [OptionalField]
        private Version _satelliteContractVersion;
        private Type _userResourceSet;
        protected string BaseNameField;
        internal static readonly int DEBUG = 0;
        public static readonly int HeaderVersionNumber = 1;
        [OptionalField(VersionAdded=4)]
        private RuntimeAssembly m_callingAssembly;
        public static readonly int MagicNumber = -1091581234;
        protected Assembly MainAssembly;
        private string moduleDir;
        internal static readonly string MscorlibName = typeof(ResourceReader).Assembly.FullName;
        internal const string ResFileExtension = ".resources";
        internal const int ResFileExtensionLength = 10;
        [NonSerialized]
        private IResourceGroveler resourceGroveler;
        [Obsolete("call InternalGetResourceSet instead")]
        protected Hashtable ResourceSets;
        internal static readonly string ResReaderTypeName = typeof(ResourceReader).FullName;
        internal static readonly string ResSetTypeName = typeof(RuntimeResourceSet).FullName;
        private bool UseManifest;
        [OptionalField(VersionAdded=1)]
        private bool UseSatelliteAssem;

        protected ResourceManager()
        {
            this.Init();
            ResourceManagerMediator mediator = new ResourceManagerMediator(this);
            this.resourceGroveler = new ManifestBasedResourceGroveler(mediator);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ResourceManager(Type resourceSource)
        {
            if (null == resourceSource)
            {
                throw new ArgumentNullException("resourceSource");
            }
            if (!(resourceSource is RuntimeType))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            this._locationInfo = resourceSource;
            this.MainAssembly = this._locationInfo.Assembly;
            this.BaseNameField = resourceSource.Name;
            this.CommonSatelliteAssemblyInit();
            this.m_callingAssembly = (RuntimeAssembly) Assembly.GetCallingAssembly();
            if ((this.MainAssembly == typeof(object).Assembly) && (this.m_callingAssembly != this.MainAssembly))
            {
                this.m_callingAssembly = null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ResourceManager(string baseName, Assembly assembly)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException("baseName");
            }
            if (null == assembly)
            {
                throw new ArgumentNullException("assembly");
            }
            if (!(assembly is RuntimeAssembly))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            this.MainAssembly = assembly;
            this.BaseNameField = baseName;
            this.CommonSatelliteAssemblyInit();
            this.m_callingAssembly = (RuntimeAssembly) Assembly.GetCallingAssembly();
            if ((assembly == typeof(object).Assembly) && (this.m_callingAssembly != assembly))
            {
                this.m_callingAssembly = null;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ResourceManager(string baseName, Assembly assembly, Type usingResourceSet)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException("baseName");
            }
            if (null == assembly)
            {
                throw new ArgumentNullException("assembly");
            }
            if (!(assembly is RuntimeAssembly))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeAssembly"));
            }
            this.MainAssembly = assembly;
            this.BaseNameField = baseName;
            if (((usingResourceSet != null) && (usingResourceSet != _minResourceSet)) && !usingResourceSet.IsSubclassOf(_minResourceSet))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ResMgrNotResSet"), "usingResourceSet");
            }
            this._userResourceSet = usingResourceSet;
            this.CommonSatelliteAssemblyInit();
            this.m_callingAssembly = (RuntimeAssembly) Assembly.GetCallingAssembly();
            if ((assembly == typeof(object).Assembly) && (this.m_callingAssembly != assembly))
            {
                this.m_callingAssembly = null;
            }
        }

        private ResourceManager(string baseName, string resourceDir, Type usingResourceSet)
        {
            if (baseName == null)
            {
                throw new ArgumentNullException("baseName");
            }
            if (resourceDir == null)
            {
                throw new ArgumentNullException("resourceDir");
            }
            this.BaseNameField = baseName;
            this.moduleDir = resourceDir;
            this._userResourceSet = usingResourceSet;
            this.ResourceSets = new Hashtable();
            this._resourceSets = new Dictionary<string, ResourceSet>();
            this.UseManifest = false;
            ResourceManagerMediator mediator = new ResourceManagerMediator(this);
            this.resourceGroveler = new FileBasedResourceGroveler(mediator);
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled())
            {
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                string resourceFileName = this.GetResourceFileName(invariantCulture);
                if (this.resourceGroveler.HasNeutralResources(invariantCulture, resourceFileName))
                {
                    FrameworkEventSource.Log.ResourceManagerNeutralResourcesFound(this.BaseNameField, this.MainAssembly, resourceFileName);
                }
                else
                {
                    FrameworkEventSource.Log.ResourceManagerNeutralResourcesNotFound(this.BaseNameField, this.MainAssembly, resourceFileName);
                }
            }
        }

        private static void AddResourceSet(Dictionary<string, ResourceSet> localResourceSets, string cultureName, ref ResourceSet rs)
        {
            lock (localResourceSets)
            {
                ResourceSet set;
                if (localResourceSets.TryGetValue(cultureName, out set))
                {
                    if (!object.ReferenceEquals(set, rs))
                    {
                        if (!localResourceSets.ContainsValue(rs))
                        {
                            rs.Dispose();
                        }
                        rs = set;
                    }
                }
                else
                {
                    localResourceSets.Add(cultureName, rs);
                }
            }
        }

        [SecuritySafeCritical]
        private void CommonSatelliteAssemblyInit()
        {
            this.UseManifest = true;
            this._resourceSets = new Dictionary<string, ResourceSet>();
            this._fallbackLoc = UltimateResourceFallbackLocation.MainAssembly;
            ResourceManagerMediator mediator = new ResourceManagerMediator(this);
            this.resourceGroveler = new ManifestBasedResourceGroveler(mediator);
            this._neutralResourcesCulture = ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(this.MainAssembly, ref this._fallbackLoc);
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled())
            {
                CultureInfo invariantCulture = CultureInfo.InvariantCulture;
                string resourceFileName = this.GetResourceFileName(invariantCulture);
                if (this.resourceGroveler.HasNeutralResources(invariantCulture, resourceFileName))
                {
                    FrameworkEventSource.Log.ResourceManagerNeutralResourcesFound(this.BaseNameField, this.MainAssembly, resourceFileName);
                }
                else
                {
                    string resName = resourceFileName;
                    if ((this._locationInfo != null) && (this._locationInfo.Namespace != null))
                    {
                        resName = this._locationInfo.Namespace + Type.Delimiter + resourceFileName;
                    }
                    FrameworkEventSource.Log.ResourceManagerNeutralResourcesNotFound(this.BaseNameField, this.MainAssembly, resName);
                }
            }
            this.ResourceSets = new Hashtable();
        }

        internal static bool CompareNames(string asmTypeName1, string typeName2, AssemblyName asmName2)
        {
            int index = asmTypeName1.IndexOf(',');
            if (((index == -1) ? asmTypeName1.Length : index) != typeName2.Length)
            {
                return false;
            }
            if (string.Compare(asmTypeName1, 0, typeName2, 0, typeName2.Length, StringComparison.Ordinal) != 0)
            {
                return false;
            }
            if (index != -1)
            {
                while (char.IsWhiteSpace(asmTypeName1[++index]))
                {
                }
                AssemblyName name = new AssemblyName(asmTypeName1.Substring(index));
                if (string.Compare(name.Name, asmName2.Name, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return false;
                }
                if (string.Compare(name.Name, "mscorlib", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }
                if (((name.CultureInfo != null) && (asmName2.CultureInfo != null)) && (name.CultureInfo.LCID != asmName2.CultureInfo.LCID))
                {
                    return false;
                }
                byte[] publicKeyToken = name.GetPublicKeyToken();
                byte[] buffer2 = asmName2.GetPublicKeyToken();
                if ((publicKeyToken != null) && (buffer2 != null))
                {
                    if (publicKeyToken.Length != buffer2.Length)
                    {
                        return false;
                    }
                    for (int i = 0; i < publicKeyToken.Length; i++)
                    {
                        if (publicKeyToken[i] != buffer2[i])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static ResourceManager CreateFileBasedResourceManager(string baseName, string resourceDir, Type usingResourceSet)
        {
            return new ResourceManager(baseName, resourceDir, usingResourceSet);
        }

        [SecuritySafeCritical]
        protected static CultureInfo GetNeutralResourcesLanguage(Assembly a)
        {
            UltimateResourceFallbackLocation mainAssembly = UltimateResourceFallbackLocation.MainAssembly;
            return ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(a, ref mainAssembly);
        }

        public virtual object GetObject(string name)
        {
            return this.GetObject(name, null, true);
        }

        public virtual object GetObject(string name, CultureInfo culture)
        {
            return this.GetObject(name, culture, true);
        }

        [SecuritySafeCritical]
        private object GetObject(string name, CultureInfo culture, bool wrapUnmanagedMemStream)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }
            if (FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerLookupStarted(this.BaseNameField, this.MainAssembly, culture.Name);
            }
            ResourceFallbackManager manager = new ResourceFallbackManager(culture, this._neutralResourcesCulture, true);
            ResourceSet set = null;
            foreach (CultureInfo info in manager)
            {
                ResourceSet set2 = this.InternalGetResourceSet(info, true, true);
                if (set2 == null)
                {
                    break;
                }
                if (set2 != set)
                {
                    object obj2 = set2.GetObject(name, this._ignoreCase);
                    if (obj2 != null)
                    {
                        UnmanagedMemoryStream stream = obj2 as UnmanagedMemoryStream;
                        if ((stream != null) && wrapUnmanagedMemStream)
                        {
                            return new UnmanagedMemoryStreamWrapper(stream);
                        }
                        return obj2;
                    }
                    set = set2;
                }
            }
            if (FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerLookupFailed(this.BaseNameField, this.MainAssembly, culture.Name);
            }
            return null;
        }

        protected virtual string GetResourceFileName(CultureInfo culture)
        {
            StringBuilder builder = new StringBuilder(0xff);
            builder.Append(this.BaseNameField);
            if (!culture.HasInvariantCultureName)
            {
                CultureInfo.VerifyCultureName(culture.Name, true);
                builder.Append('.');
                builder.Append(culture.Name);
            }
            builder.Append(".resources");
            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public virtual ResourceSet GetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            ResourceSet set;
            if (culture == null)
            {
                throw new ArgumentNullException("culture");
            }
            Dictionary<string, ResourceSet> localResourceSets = this._resourceSets;
            if (localResourceSets != null)
            {
                lock (localResourceSets)
                {
                    if (localResourceSets.TryGetValue(culture.Name, out set))
                    {
                        return set;
                    }
                }
            }
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            if (this.UseManifest && culture.HasInvariantCultureName)
            {
                string resourceFileName = this.GetResourceFileName(culture);
                Stream store = ((RuntimeAssembly) this.MainAssembly).GetManifestResourceStream(this._locationInfo, resourceFileName, this.m_callingAssembly == this.MainAssembly, ref lookForMyCaller);
                if (createIfNotExists && (store != null))
                {
                    set = ((ManifestBasedResourceGroveler) this.resourceGroveler).CreateResourceSet(store, this.MainAssembly);
                    AddResourceSet(localResourceSets, culture.Name, ref set);
                    return set;
                }
            }
            return this.InternalGetResourceSet(culture, createIfNotExists, tryParents);
        }

        [SecurityCritical]
        private Hashtable GetSatelliteAssembliesFromConfig()
        {
            string configurationFileInternal = AppDomain.CurrentDomain.FusionStore.ConfigurationFileInternal;
            if (configurationFileInternal == null)
            {
                return null;
            }
            if (((configurationFileInternal.Length >= 2) && ((configurationFileInternal[1] == Path.VolumeSeparatorChar) || ((configurationFileInternal[0] == Path.DirectorySeparatorChar) && (configurationFileInternal[1] == Path.DirectorySeparatorChar)))) && !File.InternalExists(configurationFileInternal))
            {
                return null;
            }
            ConfigTreeParser parser = new ConfigTreeParser();
            string configPath = "/configuration/satelliteassemblies";
            ConfigNode node = null;
            try
            {
                node = parser.Parse(configurationFileInternal, configPath, true);
            }
            catch (Exception)
            {
            }
            if (node == null)
            {
                return null;
            }
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (ConfigNode node2 in node.Children)
            {
                if (!string.Equals(node2.Name, "assembly"))
                {
                    throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTag", new object[] { Path.GetFileName(configurationFileInternal), node2.Name }));
                }
                if (node2.Attributes.Count == 0)
                {
                    throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTagNoAttr", new object[] { Path.GetFileName(configurationFileInternal) }));
                }
                DictionaryEntry entry = node2.Attributes[0];
                string str3 = (string) entry.Value;
                if ((!object.Equals(entry.Key, "name") || string.IsNullOrEmpty(str3)) || (node2.Attributes.Count > 1))
                {
                    throw new ApplicationException(Environment.GetResourceString("XMLSyntax_InvalidSyntaxSatAssemTagBadAttr", new object[] { Path.GetFileName(configurationFileInternal), entry.Key, entry.Value }));
                }
                ArrayList list = new ArrayList(5);
                foreach (ConfigNode node3 in node2.Children)
                {
                    if (node3.Value != null)
                    {
                        list.Add(node3.Value);
                    }
                }
                string[] strArray = new string[list.Count];
                for (int i = 0; i < strArray.Length; i++)
                {
                    string cultureName = (string) list[i];
                    strArray[i] = cultureName;
                    if (FrameworkEventSource.IsInitialized)
                    {
                        FrameworkEventSource.Log.ResourceManagerAddingCultureFromConfigFile(this.BaseNameField, this.MainAssembly, cultureName);
                    }
                }
                hashtable.Add(str3, strArray);
            }
            return hashtable;
        }

        [SecuritySafeCritical]
        protected static Version GetSatelliteContractVersion(Assembly a)
        {
            Version version;
            if (a == null)
            {
                throw new ArgumentNullException("a", Environment.GetResourceString("ArgumentNull_Assembly"));
            }
            string str = null;
            if (!a.ReflectionOnly)
            {
                object[] customAttributes = a.GetCustomAttributes(typeof(SatelliteContractVersionAttribute), false);
                if (customAttributes.Length == 0)
                {
                    return null;
                }
                str = ((SatelliteContractVersionAttribute) customAttributes[0]).Version;
            }
            else
            {
                foreach (CustomAttributeData data in CustomAttributeData.GetCustomAttributes(a))
                {
                    if (data.Constructor.DeclaringType == typeof(SatelliteContractVersionAttribute))
                    {
                        CustomAttributeTypedArgument argument = data.ConstructorArguments[0];
                        str = (string) argument.Value;
                        break;
                    }
                }
                if (str == null)
                {
                    return null;
                }
            }
            try
            {
                version = new Version(str);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                if (a != typeof(object).Assembly)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidSatelliteContract_Asm_Ver", new object[] { a.ToString(), str }), exception);
                }
                return null;
            }
            return version;
        }

        [ComVisible(false)]
        public UnmanagedMemoryStream GetStream(string name)
        {
            return this.GetStream(name, null);
        }

        [ComVisible(false)]
        public UnmanagedMemoryStream GetStream(string name, CultureInfo culture)
        {
            object obj2 = this.GetObject(name, culture, false);
            UnmanagedMemoryStream stream = obj2 as UnmanagedMemoryStream;
            if ((stream == null) && (obj2 != null))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResourceNotStream_Name", new object[] { name }));
            }
            return stream;
        }

        public virtual string GetString(string name)
        {
            return this.GetString(name, null);
        }

        [SecuritySafeCritical]
        public virtual string GetString(string name, CultureInfo culture)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }
            if (FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerLookupStarted(this.BaseNameField, this.MainAssembly, culture.Name);
            }
            ResourceFallbackManager manager = new ResourceFallbackManager(culture, this._neutralResourcesCulture, true);
            ResourceSet set = null;
            foreach (CultureInfo info in manager)
            {
                ResourceSet set2 = this.InternalGetResourceSet(info, true, true);
                if (set2 == null)
                {
                    break;
                }
                if (set2 != set)
                {
                    string str = set2.GetString(name, this._ignoreCase);
                    if (str != null)
                    {
                        return str;
                    }
                    set = set2;
                }
            }
            if (FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerLookupFailed(this.BaseNameField, this.MainAssembly, culture.Name);
            }
            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        private void Init()
        {
            this.m_callingAssembly = (RuntimeAssembly) Assembly.GetCallingAssembly();
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        protected virtual ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.InternalGetResourceSet(culture, createIfNotExists, tryParents, ref lookForMyCaller);
        }

        [SecurityCritical]
        private ResourceSet InternalGetResourceSet(CultureInfo requestedCulture, bool createIfNotExists, bool tryParents, ref StackCrawlMark stackMark)
        {
            Dictionary<string, ResourceSet> localResourceSets = this._resourceSets;
            ResourceFallbackManager manager = new ResourceFallbackManager(requestedCulture, this._neutralResourcesCulture, tryParents);
            ResourceSet set = null;
            CultureInfo info = null;
            foreach (CultureInfo info2 in manager)
            {
                if (FrameworkEventSource.IsInitialized)
                {
                    FrameworkEventSource.Log.ResourceManagerLookingForResourceSet(this.BaseNameField, this.MainAssembly, info2.Name);
                }
                lock (localResourceSets)
                {
                    if (localResourceSets.TryGetValue(info2.Name, out set))
                    {
                        if (FrameworkEventSource.IsInitialized)
                        {
                            FrameworkEventSource.Log.ResourceManagerFoundResourceSetInCache(this.BaseNameField, this.MainAssembly, info2.Name);
                        }
                        break;
                    }
                }
                set = this.resourceGroveler.GrovelForResourceSet(info2, localResourceSets, tryParents, createIfNotExists, ref stackMark);
                if (set != null)
                {
                    info = info2;
                    break;
                }
            }
            if ((set != null) && (info != null))
            {
                foreach (CultureInfo info3 in manager)
                {
                    AddResourceSet(localResourceSets, info3.Name, ref set);
                    if (info3 == info)
                    {
                        return set;
                    }
                }
            }
            return set;
        }

        [SecuritySafeCritical, OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            this._resourceSets = new Dictionary<string, ResourceSet>();
            ResourceManagerMediator mediator = new ResourceManagerMediator(this);
            if (this.UseManifest)
            {
                this.resourceGroveler = new ManifestBasedResourceGroveler(mediator);
            }
            else
            {
                this.resourceGroveler = new FileBasedResourceGroveler(mediator);
            }
            if (this.m_callingAssembly == null)
            {
                this.m_callingAssembly = (RuntimeAssembly) this._callingAssembly;
            }
            if (this.UseManifest && (this._neutralResourcesCulture == null))
            {
                this._neutralResourcesCulture = ManifestBasedResourceGroveler.GetNeutralResourcesLanguage(this.MainAssembly, ref this._fallbackLoc);
            }
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this._resourceSets = null;
            this.resourceGroveler = null;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext ctx)
        {
            this._callingAssembly = this.m_callingAssembly;
            this.UseSatelliteAssem = this.UseManifest;
            this.ResourceSets = new Hashtable();
        }

        public virtual void ReleaseAllResources()
        {
            if (FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerReleasingResources(this.BaseNameField, this.MainAssembly);
            }
            IDictionaryEnumerator enumerator = this._resourceSets.GetEnumerator();
            this._resourceSets = new Dictionary<string, ResourceSet>();
            IDictionaryEnumerator enumerator2 = null;
            if (this.ResourceSets != null)
            {
                enumerator2 = this.ResourceSets.GetEnumerator();
            }
            this.ResourceSets = new Hashtable();
            while (enumerator.MoveNext())
            {
                ((ResourceSet) enumerator.Value).Close();
            }
            if (enumerator2 != null)
            {
                while (enumerator2.MoveNext())
                {
                    ((ResourceSet) enumerator2.Value).Close();
                }
            }
        }

        [SecurityCritical]
        private bool TryLookingForSatellite(CultureInfo lookForCulture)
        {
            if (!_checkedConfigFile)
            {
                lock (this)
                {
                    if (!_checkedConfigFile)
                    {
                        _checkedConfigFile = true;
                        _installedSatelliteInfo = this.GetSatelliteAssembliesFromConfig();
                    }
                }
            }
            if (_installedSatelliteInfo == null)
            {
                return true;
            }
            string[] array = (string[]) _installedSatelliteInfo[this.MainAssembly.FullName];
            if (array == null)
            {
                return true;
            }
            int index = Array.IndexOf<string>(array, lookForCulture.Name);
            if (FrameworkEventSource.IsInitialized && FrameworkEventSource.Log.IsEnabled())
            {
                if (index < 0)
                {
                    FrameworkEventSource.Log.ResourceManagerCultureNotFoundInConfigFile(this.BaseNameField, this.MainAssembly, lookForCulture.Name);
                }
                else
                {
                    FrameworkEventSource.Log.ResourceManagerCultureFoundInConfigFile(this.BaseNameField, this.MainAssembly, lookForCulture.Name);
                }
            }
            return (index >= 0);
        }

        public virtual string BaseName
        {
            get
            {
                return this.BaseNameField;
            }
        }

        protected UltimateResourceFallbackLocation FallbackLocation
        {
            get
            {
                return this._fallbackLoc;
            }
            set
            {
                this._fallbackLoc = value;
            }
        }

        public virtual bool IgnoreCase
        {
            get
            {
                return this._ignoreCase;
            }
            set
            {
                this._ignoreCase = value;
            }
        }

        public virtual Type ResourceSetType
        {
            get
            {
                if (this._userResourceSet != null)
                {
                    return this._userResourceSet;
                }
                return typeof(RuntimeResourceSet);
            }
        }

        internal class ResourceManagerMediator
        {
            private ResourceManager _rm;

            internal ResourceManagerMediator(ResourceManager rm)
            {
                if (rm == null)
                {
                    throw new ArgumentNullException("rm");
                }
                this._rm = rm;
            }

            internal string GetResourceFileName(CultureInfo culture)
            {
                return this._rm.GetResourceFileName(culture);
            }

            internal Version ObtainSatelliteContractVersion(Assembly a)
            {
                return ResourceManager.GetSatelliteContractVersion(a);
            }

            [SecurityCritical]
            internal bool TryLookingForSatellite(CultureInfo lookForCulture)
            {
                return this._rm.TryLookingForSatellite(lookForCulture);
            }

            internal string BaseName
            {
                get
                {
                    return this._rm.BaseName;
                }
            }

            internal string BaseNameField
            {
                get
                {
                    return this._rm.BaseNameField;
                }
            }

            internal RuntimeAssembly CallingAssembly
            {
                get
                {
                    return this._rm.m_callingAssembly;
                }
            }

            internal UltimateResourceFallbackLocation FallbackLoc
            {
                get
                {
                    return this._rm.FallbackLocation;
                }
                set
                {
                    this._rm._fallbackLoc = value;
                }
            }

            internal Type LocationInfo
            {
                get
                {
                    return this._rm._locationInfo;
                }
            }

            internal bool LookedForSatelliteContractVersion
            {
                get
                {
                    return this._rm._lookedForSatelliteContractVersion;
                }
                set
                {
                    this._rm._lookedForSatelliteContractVersion = value;
                }
            }

            internal RuntimeAssembly MainAssembly
            {
                get
                {
                    return (RuntimeAssembly) this._rm.MainAssembly;
                }
            }

            internal string ModuleDir
            {
                get
                {
                    return this._rm.moduleDir;
                }
            }

            internal CultureInfo NeutralResourcesCulture
            {
                get
                {
                    return this._rm._neutralResourcesCulture;
                }
                set
                {
                    this._rm._neutralResourcesCulture = value;
                }
            }

            internal Version SatelliteContractVersion
            {
                get
                {
                    return this._rm._satelliteContractVersion;
                }
                set
                {
                    this._rm._satelliteContractVersion = value;
                }
            }

            internal Type UserResourceSet
            {
                get
                {
                    return this._rm._userResourceSet;
                }
            }
        }
    }
}

