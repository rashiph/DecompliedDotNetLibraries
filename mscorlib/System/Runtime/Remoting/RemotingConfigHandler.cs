namespace System.Runtime.Remoting
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Lifetime;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Util;

    internal static class RemotingConfigHandler
    {
        private static string _applicationName;
        private static bool _bMachineConfigLoaded = false;
        private static bool _bUrlObjRefMode = false;
        private static Queue _delayLoadChannelConfigQueue = new Queue();
        private static CustomErrorsModes _errorMode = CustomErrorsModes.RemoteOnly;
        private static bool _errorsModeSet = false;
        private const string _machineConfigFilename = "machine.config";
        public static RemotingConfigInfo Info = new RemotingConfigInfo();

        [SecurityCritical]
        private static void ConfigureChannels(RemotingXmlConfigFileData configData, bool ensureSecurity)
        {
            RemotingServices.RegisterWellKnownChannels();
            foreach (RemotingXmlConfigFileData.ChannelEntry entry in configData.ChannelEntries)
            {
                if (!entry.DelayLoad)
                {
                    ChannelServices.RegisterChannel(CreateChannelFromConfigEntry(entry), ensureSecurity);
                }
                else
                {
                    _delayLoadChannelConfigQueue.Enqueue(new DelayLoadClientChannelEntry(entry, ensureSecurity));
                }
            }
        }

        [SecurityCritical]
        private static void ConfigureRemoting(RemotingXmlConfigFileData configData, bool ensureSecurity)
        {
            try
            {
                string applicationName = configData.ApplicationName;
                if (applicationName != null)
                {
                    ApplicationName = applicationName;
                }
                if (configData.CustomErrors != null)
                {
                    _errorMode = configData.CustomErrors.Mode;
                }
                ConfigureChannels(configData, ensureSecurity);
                if (configData.Lifetime != null)
                {
                    if (configData.Lifetime.IsLeaseTimeSet)
                    {
                        LifetimeServices.LeaseTime = configData.Lifetime.LeaseTime;
                    }
                    if (configData.Lifetime.IsRenewOnCallTimeSet)
                    {
                        LifetimeServices.RenewOnCallTime = configData.Lifetime.RenewOnCallTime;
                    }
                    if (configData.Lifetime.IsSponsorshipTimeoutSet)
                    {
                        LifetimeServices.SponsorshipTimeout = configData.Lifetime.SponsorshipTimeout;
                    }
                    if (configData.Lifetime.IsLeaseManagerPollTimeSet)
                    {
                        LifetimeServices.LeaseManagerPollTime = configData.Lifetime.LeaseManagerPollTime;
                    }
                }
                _bUrlObjRefMode = configData.UrlObjRefMode;
                Info.StoreRemoteAppEntries(configData);
                Info.StoreActivatedExports(configData);
                Info.StoreInteropEntries(configData);
                Info.StoreWellKnownExports(configData);
                if (configData.ServerActivatedEntries.Count > 0)
                {
                    ActivationServices.StartListeningForRemoteRequests();
                }
            }
            catch (Exception exception)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_ConfigurationFailure"), new object[] { exception }));
            }
        }

        [SecurityCritical]
        internal static IChannel CreateChannelFromConfigEntry(RemotingXmlConfigFileData.ChannelEntry entry)
        {
            object[] objArray;
            Type c = RemotingConfigInfo.LoadType(entry.TypeName, entry.AssemblyName);
            bool flag = typeof(IChannelReceiver).IsAssignableFrom(c);
            bool flag2 = typeof(IChannelSender).IsAssignableFrom(c);
            IClientChannelSinkProvider provider = null;
            IServerChannelSinkProvider provider2 = null;
            if (entry.ClientSinkProviders.Count > 0)
            {
                provider = CreateClientChannelSinkProviderChain(entry.ClientSinkProviders);
            }
            if (entry.ServerSinkProviders.Count > 0)
            {
                provider2 = CreateServerChannelSinkProviderChain(entry.ServerSinkProviders);
            }
            if (flag && flag2)
            {
                objArray = new object[] { entry.Properties, provider, provider2 };
            }
            else if (flag)
            {
                objArray = new object[] { entry.Properties, provider2 };
            }
            else
            {
                if (!flag2)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidChannelType"), new object[] { c.FullName }));
                }
                objArray = new object[] { entry.Properties, provider };
            }
            IChannel channel = null;
            try
            {
                channel = (IChannel) Activator.CreateInstance(c, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, objArray, null, null);
            }
            catch (MissingMethodException)
            {
                string str = null;
                if (flag && flag2)
                {
                    str = "MyChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider, IServerChannelSinkProvider serverSinkProvider)";
                }
                else if (flag)
                {
                    str = "MyChannel(IDictionary properties, IServerChannelSinkProvider serverSinkProvider)";
                }
                else if (flag2)
                {
                    str = "MyChannel(IDictionary properties, IClientChannelSinkProvider clientSinkProvider)";
                }
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_ChannelMissingCtor"), new object[] { c.FullName, str }));
            }
            return channel;
        }

        [SecurityCritical]
        private static object CreateChannelSinkProvider(RemotingXmlConfigFileData.SinkProviderEntry entry, bool bServer)
        {
            object obj2 = null;
            Type c = RemotingConfigInfo.LoadType(entry.TypeName, entry.AssemblyName);
            if (bServer)
            {
                if (!typeof(IServerChannelSinkProvider).IsAssignableFrom(c))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidSinkProviderType"), new object[] { c.FullName, "IServerChannelSinkProvider" }));
                }
            }
            else if (!typeof(IClientChannelSinkProvider).IsAssignableFrom(c))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_InvalidSinkProviderType"), new object[] { c.FullName, "IClientChannelSinkProvider" }));
            }
            if (entry.IsFormatter && ((bServer && !typeof(IServerFormatterSinkProvider).IsAssignableFrom(c)) || (!bServer && !typeof(IClientFormatterSinkProvider).IsAssignableFrom(c))))
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_SinkProviderNotFormatter"), new object[] { c.FullName }));
            }
            object[] args = new object[] { entry.Properties, entry.ProviderData };
            try
            {
                obj2 = Activator.CreateInstance(c, BindingFlags.CreateInstance | BindingFlags.Public | BindingFlags.Instance, null, args, null, null);
            }
            catch (MissingMethodException)
            {
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_SinkProviderMissingCtor"), new object[] { c.FullName, "MySinkProvider(IDictionary properties, ICollection providerData)" }));
            }
            return obj2;
        }

        [SecurityCritical]
        private static IClientChannelSinkProvider CreateClientChannelSinkProviderChain(ArrayList entries)
        {
            IClientChannelSinkProvider provider = null;
            IClientChannelSinkProvider next = null;
            foreach (RemotingXmlConfigFileData.SinkProviderEntry entry in entries)
            {
                if (provider == null)
                {
                    provider = (IClientChannelSinkProvider) CreateChannelSinkProvider(entry, false);
                    next = provider;
                }
                else
                {
                    next.Next = (IClientChannelSinkProvider) CreateChannelSinkProvider(entry, false);
                    next = next.Next;
                }
            }
            return provider;
        }

        [SecurityCritical]
        private static IServerChannelSinkProvider CreateServerChannelSinkProviderChain(ArrayList entries)
        {
            IServerChannelSinkProvider provider = null;
            IServerChannelSinkProvider next = null;
            foreach (RemotingXmlConfigFileData.SinkProviderEntry entry in entries)
            {
                if (provider == null)
                {
                    provider = (IServerChannelSinkProvider) CreateChannelSinkProvider(entry, true);
                    next = provider;
                }
                else
                {
                    next.Next = (IServerChannelSinkProvider) CreateChannelSinkProvider(entry, true);
                    next = next.Next;
                }
            }
            return provider;
        }

        [SecurityCritical]
        internal static ServerIdentity CreateWellKnownObject(string uri)
        {
            uri = Identity.RemoveAppNameOrAppGuidIfNecessary(uri);
            return Info.StartupWellKnownObject(uri);
        }

        [SecurityCritical]
        internal static void DoConfiguration(string filename, bool ensureSecurity)
        {
            LoadMachineConfigIfNecessary();
            RemotingXmlConfigFileData configData = LoadConfigurationFromXmlFile(filename);
            if (configData != null)
            {
                ConfigureRemoting(configData, ensureSecurity);
            }
        }

        [SecurityCritical]
        internal static IMessageSink FindDelayLoadChannelForCreateMessageSink(string url, object data, out string objectURI)
        {
            LoadMachineConfigIfNecessary();
            objectURI = null;
            IMessageSink sink = null;
            foreach (DelayLoadClientChannelEntry entry in _delayLoadChannelConfigQueue)
            {
                IChannelSender channel = entry.Channel;
                if (channel != null)
                {
                    sink = channel.CreateMessageSink(url, data, out objectURI);
                    if (sink != null)
                    {
                        entry.RegisterChannel();
                        return sink;
                    }
                }
            }
            return null;
        }

        internal static ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
        {
            return Info.GetRegisteredActivatedClientTypes();
        }

        internal static ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
        {
            return Info.GetRegisteredActivatedServiceTypes();
        }

        internal static WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
        {
            return Info.GetRegisteredWellKnownClientTypes();
        }

        internal static WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
        {
            return Info.GetRegisteredWellKnownServiceTypes();
        }

        [SecurityCritical]
        internal static Type GetServerTypeForUri(string URI)
        {
            URI = Identity.RemoveAppNameOrAppGuidIfNecessary(URI);
            return Info.GetServerTypeForUri(URI);
        }

        internal static bool HasApplicationNameBeenSet()
        {
            return (_applicationName != null);
        }

        [SecurityCritical]
        internal static bool IsActivationAllowed(RuntimeType svrType)
        {
            if (svrType == null)
            {
                return false;
            }
            string simpleAssemblyName = InternalRemotingServices.GetReflectionCachedData(svrType).SimpleAssemblyName;
            return Info.ActivationAllowed(svrType.FullName, simpleAssemblyName);
        }

        [SecurityCritical]
        internal static bool IsActivationAllowed(string TypeName)
        {
            string str2;
            string str3;
            string typeNameFromQualifiedTypeName = RemotingServices.InternalGetTypeNameFromQualifiedTypeName(TypeName);
            if (typeNameFromQualifiedTypeName == null)
            {
                return false;
            }
            ParseType(typeNameFromQualifiedTypeName, out str2, out str3);
            if (str3 == null)
            {
                return false;
            }
            int index = str3.IndexOf(',');
            if (index != -1)
            {
                str3 = str3.Substring(0, index);
            }
            return Info.ActivationAllowed(str2, str3);
        }

        [SecurityCritical]
        internal static ActivatedClientTypeEntry IsRemotelyActivatedClientType(RuntimeType svrType)
        {
            RemotingTypeCachedData reflectionCachedData = InternalRemotingServices.GetReflectionCachedData(svrType);
            string simpleAssemblyName = reflectionCachedData.SimpleAssemblyName;
            ActivatedClientTypeEntry entry = Info.QueryRemoteActivate(svrType.FullName, simpleAssemblyName);
            if (entry == null)
            {
                string assemblyName = reflectionCachedData.AssemblyName;
                entry = Info.QueryRemoteActivate(svrType.FullName, assemblyName);
                if (entry == null)
                {
                    entry = Info.QueryRemoteActivate(svrType.Name, simpleAssemblyName);
                }
            }
            return entry;
        }

        internal static ActivatedClientTypeEntry IsRemotelyActivatedClientType(string typeName, string assemblyName)
        {
            return Info.QueryRemoteActivate(typeName, assemblyName);
        }

        [SecurityCritical]
        internal static WellKnownClientTypeEntry IsWellKnownClientType(RuntimeType svrType)
        {
            string simpleAssemblyName = InternalRemotingServices.GetReflectionCachedData(svrType).SimpleAssemblyName;
            WellKnownClientTypeEntry entry = Info.QueryConnect(svrType.FullName, simpleAssemblyName);
            if (entry == null)
            {
                entry = Info.QueryConnect(svrType.Name, simpleAssemblyName);
            }
            return entry;
        }

        internal static WellKnownClientTypeEntry IsWellKnownClientType(string typeName, string assemblyName)
        {
            return Info.QueryConnect(typeName, assemblyName);
        }

        private static RemotingXmlConfigFileData LoadConfigurationFromXmlFile(string filename)
        {
            RemotingXmlConfigFileData data;
            try
            {
                if (filename != null)
                {
                    return RemotingXmlConfigFileParser.ParseConfigFile(filename);
                }
                data = null;
            }
            catch (Exception exception)
            {
                Exception innerException = exception.InnerException as FileNotFoundException;
                if (innerException != null)
                {
                    exception = innerException;
                }
                throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_ReadFailure"), new object[] { filename, exception }));
            }
            return data;
        }

        [SecurityCritical]
        private static void LoadMachineConfigIfNecessary()
        {
            if (!_bMachineConfigLoaded)
            {
                lock (Info)
                {
                    if (!_bMachineConfigLoaded)
                    {
                        RemotingXmlConfigFileData configData = RemotingXmlConfigFileParser.ParseDefaultConfiguration();
                        if (configData != null)
                        {
                            ConfigureRemoting(configData, false);
                        }
                        string path = Config.MachineDirectory + "machine.config";
                        new FileIOPermission(FileIOPermissionAccess.Read, path).Assert();
                        configData = LoadConfigurationFromXmlFile(path);
                        if (configData != null)
                        {
                            ConfigureRemoting(configData, false);
                        }
                        _bMachineConfigLoaded = true;
                    }
                }
            }
        }

        private static void ParseGenericType(string typeAssem, int indexStart, out string typeName, out string assemName)
        {
            int length = typeAssem.Length;
            int num2 = 1;
            int startIndex = indexStart;
            while ((num2 > 0) && (++startIndex < (length - 1)))
            {
                if (typeAssem[startIndex] == '[')
                {
                    num2++;
                }
                else if (typeAssem[startIndex] == ']')
                {
                    num2--;
                }
            }
            if ((num2 > 0) || (startIndex >= length))
            {
                typeName = null;
                assemName = null;
            }
            else
            {
                startIndex = typeAssem.IndexOf(',', startIndex);
                if ((startIndex >= 0) && (startIndex < (length - 1)))
                {
                    typeName = typeAssem.Substring(0, startIndex).Trim();
                    assemName = typeAssem.Substring(startIndex + 1).Trim();
                }
                else
                {
                    typeName = null;
                    assemName = null;
                }
            }
        }

        internal static void ParseType(string typeAssem, out string typeName, out string assemName)
        {
            string str = typeAssem;
            int index = str.IndexOf("[");
            if ((index >= 0) && (index < (str.Length - 1)))
            {
                ParseGenericType(str, index, out typeName, out assemName);
            }
            else
            {
                int length = str.IndexOf(",");
                if ((length >= 0) && (length < (str.Length - 1)))
                {
                    typeName = str.Substring(0, length).Trim();
                    assemName = str.Substring(length + 1).Trim();
                }
                else
                {
                    typeName = null;
                    assemName = null;
                }
            }
        }

        internal static void RegisterActivatedClientType(ActivatedClientTypeEntry entry)
        {
            Info.AddActivatedClientType(entry);
        }

        internal static void RegisterActivatedServiceType(ActivatedServiceTypeEntry entry)
        {
            Info.AddActivatedType(entry.TypeName, entry.AssemblyName, entry.ContextAttributes);
        }

        internal static void RegisterWellKnownClientType(WellKnownClientTypeEntry entry)
        {
            Info.AddWellKnownClientType(entry);
        }

        [SecurityCritical]
        internal static void RegisterWellKnownServiceType(WellKnownServiceTypeEntry entry)
        {
            string typeName = entry.TypeName;
            string assemblyName = entry.AssemblyName;
            string objectUri = entry.ObjectUri;
            WellKnownObjectMode mode = entry.Mode;
            lock (Info)
            {
                Info.AddWellKnownEntry(entry);
            }
        }

        internal static string ApplicationName
        {
            get
            {
                if (_applicationName == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Config_NoAppName"));
                }
                return _applicationName;
            }
            set
            {
                if (_applicationName != null)
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_AppNameSet"), new object[] { _applicationName }));
                }
                _applicationName = value;
                char[] trimChars = new char[] { '/' };
                if (_applicationName.StartsWith("/", StringComparison.Ordinal))
                {
                    _applicationName = _applicationName.TrimStart(trimChars);
                }
                if (_applicationName.EndsWith("/", StringComparison.Ordinal))
                {
                    _applicationName = _applicationName.TrimEnd(trimChars);
                }
            }
        }

        internal static CustomErrorsModes CustomErrorsMode
        {
            get
            {
                return _errorMode;
            }
            set
            {
                if (_errorsModeSet)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_Config_ErrorsModeSet"));
                }
                _errorsModeSet = true;
                _errorMode = value;
            }
        }

        internal static bool UrlObjRefMode
        {
            get
            {
                return _bUrlObjRefMode;
            }
        }

        internal class RemotingConfigInfo
        {
            private Hashtable _exportableClasses = Hashtable.Synchronized(new Hashtable());
            private Hashtable _remoteAppInfo = Hashtable.Synchronized(new Hashtable());
            private Hashtable _remoteTypeInfo = Hashtable.Synchronized(new Hashtable());
            private Hashtable _wellKnownExportInfo = Hashtable.Synchronized(new Hashtable());
            private static PermissionSet s_fullTrust = new PermissionSet(PermissionState.Unrestricted);
            private static object s_wkoStartLock = new object();
            private static char[] SepEquals = new char[] { '=' };
            private static char[] SepPound = new char[] { '#' };
            private static char[] SepSemiColon = new char[] { ';' };
            private static char[] SepSpace = new char[] { ' ' };

            internal RemotingConfigInfo()
            {
            }

            internal bool ActivationAllowed(string typeName, string assemblyName)
            {
                return this._exportableClasses.ContainsKey(this.EncodeTypeAndAssemblyNames(typeName, assemblyName));
            }

            internal void AddActivatedClientType(ActivatedClientTypeEntry entry)
            {
                if (this.CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_TypeAlreadyRedirected"), new object[] { entry.TypeName, entry.AssemblyName }));
                }
                if (this.CheckForServiceEntryWithType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_CantRedirectActivationOfWellKnownService"), new object[] { entry.TypeName, entry.AssemblyName }));
                }
                string applicationUrl = entry.ApplicationUrl;
                System.Runtime.Remoting.RemoteAppEntry entry2 = (System.Runtime.Remoting.RemoteAppEntry) this._remoteAppInfo[applicationUrl];
                if (entry2 == null)
                {
                    entry2 = new System.Runtime.Remoting.RemoteAppEntry(applicationUrl, applicationUrl);
                    this._remoteAppInfo.Add(applicationUrl, entry2);
                }
                if (entry2 != null)
                {
                    entry.CacheRemoteAppEntry(entry2);
                }
                string key = this.EncodeTypeAndAssemblyNames(entry.TypeName, entry.AssemblyName);
                this._remoteTypeInfo.Add(key, entry);
            }

            internal void AddActivatedType(string typeName, string assemblyName, IContextAttribute[] contextAttributes)
            {
                if (typeName == null)
                {
                    throw new ArgumentNullException("typeName");
                }
                if (assemblyName == null)
                {
                    throw new ArgumentNullException("assemblyName");
                }
                if (this.CheckForRedirectedClientType(typeName, assemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_CantUseRedirectedTypeForWellKnownService"), new object[] { typeName, assemblyName }));
                }
                ActivatedServiceTypeEntry entry = new ActivatedServiceTypeEntry(typeName, assemblyName) {
                    ContextAttributes = contextAttributes
                };
                string key = this.EncodeTypeAndAssemblyNames(typeName, assemblyName);
                this._exportableClasses.Add(key, entry);
            }

            internal void AddWellKnownClientType(WellKnownClientTypeEntry entry)
            {
                if (this.CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_TypeAlreadyRedirected"), new object[] { entry.TypeName, entry.AssemblyName }));
                }
                if (this.CheckForServiceEntryWithType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_CantRedirectActivationOfWellKnownService"), new object[] { entry.TypeName, entry.AssemblyName }));
                }
                string applicationUrl = entry.ApplicationUrl;
                System.Runtime.Remoting.RemoteAppEntry entry2 = null;
                if (applicationUrl != null)
                {
                    entry2 = (System.Runtime.Remoting.RemoteAppEntry) this._remoteAppInfo[applicationUrl];
                    if (entry2 == null)
                    {
                        entry2 = new System.Runtime.Remoting.RemoteAppEntry(applicationUrl, applicationUrl);
                        this._remoteAppInfo.Add(applicationUrl, entry2);
                    }
                }
                if (entry2 != null)
                {
                    entry.CacheRemoteAppEntry(entry2);
                }
                string key = this.EncodeTypeAndAssemblyNames(entry.TypeName, entry.AssemblyName);
                this._remoteTypeInfo.Add(key, entry);
            }

            [SecurityCritical]
            internal void AddWellKnownEntry(WellKnownServiceTypeEntry entry)
            {
                this.AddWellKnownEntry(entry, true);
            }

            [SecurityCritical]
            internal void AddWellKnownEntry(WellKnownServiceTypeEntry entry, bool fReplace)
            {
                if (this.CheckForRedirectedClientType(entry.TypeName, entry.AssemblyName))
                {
                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Config_CantUseRedirectedTypeForWellKnownService"), new object[] { entry.TypeName, entry.AssemblyName }));
                }
                string key = entry.ObjectUri.ToLower(CultureInfo.InvariantCulture);
                if (fReplace)
                {
                    this._wellKnownExportInfo[key] = entry;
                    IdentityHolder.RemoveIdentity(entry.ObjectUri);
                }
                else
                {
                    this._wellKnownExportInfo.Add(key, entry);
                }
            }

            private bool CheckForRedirectedClientType(string typeName, string asmName)
            {
                int index = asmName.IndexOf(",");
                if (index != -1)
                {
                    asmName = asmName.Substring(0, index);
                }
                if (this.QueryRemoteActivate(typeName, asmName) == null)
                {
                    return (this.QueryConnect(typeName, asmName) != null);
                }
                return true;
            }

            private bool CheckForServiceEntryWithType(string typeName, string asmName)
            {
                if (!this.CheckForWellKnownServiceEntryWithType(typeName, asmName))
                {
                    return this.ActivationAllowed(typeName, asmName);
                }
                return true;
            }

            private bool CheckForWellKnownServiceEntryWithType(string typeName, string asmName)
            {
                foreach (DictionaryEntry entry in this._wellKnownExportInfo)
                {
                    WellKnownServiceTypeEntry entry2 = (WellKnownServiceTypeEntry) entry.Value;
                    if (typeName == entry2.TypeName)
                    {
                        bool flag = false;
                        if (asmName == entry2.AssemblyName)
                        {
                            flag = true;
                        }
                        else if ((string.Compare(entry2.AssemblyName, 0, asmName, 0, asmName.Length, StringComparison.OrdinalIgnoreCase) == 0) && (entry2.AssemblyName[asmName.Length] == ','))
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            private static IContextAttribute[] CreateContextAttributesFromConfigEntries(ArrayList contextAttributes)
            {
                int count = contextAttributes.Count;
                if (count == 0)
                {
                    return null;
                }
                IContextAttribute[] attributeArray = new IContextAttribute[count];
                int num2 = 0;
                foreach (RemotingXmlConfigFileData.ContextAttributeEntry entry in contextAttributes)
                {
                    Assembly assembly = Assembly.Load(entry.AssemblyName);
                    IContextAttribute attribute = null;
                    Hashtable properties = entry.Properties;
                    if ((properties != null) && (properties.Count > 0))
                    {
                        object[] args = new object[] { properties };
                        attribute = (IContextAttribute) Activator.CreateInstance(assembly.GetType(entry.TypeName, false, false), BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, args, null, null);
                    }
                    else
                    {
                        attribute = (IContextAttribute) Activator.CreateInstance(assembly.GetType(entry.TypeName, false, false), true);
                    }
                    attributeArray[num2++] = attribute;
                }
                return attributeArray;
            }

            private string EncodeTypeAndAssemblyNames(string typeName, string assemblyName)
            {
                return (typeName + ", " + assemblyName.ToLower(CultureInfo.InvariantCulture));
            }

            internal ActivatedClientTypeEntry[] GetRegisteredActivatedClientTypes()
            {
                int num = 0;
                foreach (DictionaryEntry entry in this._remoteTypeInfo)
                {
                    if (entry.Value is ActivatedClientTypeEntry)
                    {
                        num++;
                    }
                }
                ActivatedClientTypeEntry[] entryArray = new ActivatedClientTypeEntry[num];
                int num2 = 0;
                foreach (DictionaryEntry entry3 in this._remoteTypeInfo)
                {
                    ActivatedClientTypeEntry entry4 = entry3.Value as ActivatedClientTypeEntry;
                    if (entry4 != null)
                    {
                        string appUrl = null;
                        System.Runtime.Remoting.RemoteAppEntry remoteAppEntry = entry4.GetRemoteAppEntry();
                        if (remoteAppEntry != null)
                        {
                            appUrl = remoteAppEntry.GetAppURI();
                        }
                        ActivatedClientTypeEntry entry6 = new ActivatedClientTypeEntry(entry4.TypeName, entry4.AssemblyName, appUrl) {
                            ContextAttributes = entry4.ContextAttributes
                        };
                        entryArray[num2++] = entry6;
                    }
                }
                return entryArray;
            }

            internal ActivatedServiceTypeEntry[] GetRegisteredActivatedServiceTypes()
            {
                ActivatedServiceTypeEntry[] entryArray = new ActivatedServiceTypeEntry[this._exportableClasses.Count];
                int num = 0;
                foreach (DictionaryEntry entry in this._exportableClasses)
                {
                    entryArray[num++] = (ActivatedServiceTypeEntry) entry.Value;
                }
                return entryArray;
            }

            internal WellKnownClientTypeEntry[] GetRegisteredWellKnownClientTypes()
            {
                int num = 0;
                foreach (DictionaryEntry entry in this._remoteTypeInfo)
                {
                    if (entry.Value is WellKnownClientTypeEntry)
                    {
                        num++;
                    }
                }
                WellKnownClientTypeEntry[] entryArray = new WellKnownClientTypeEntry[num];
                int num2 = 0;
                foreach (DictionaryEntry entry3 in this._remoteTypeInfo)
                {
                    WellKnownClientTypeEntry entry4 = entry3.Value as WellKnownClientTypeEntry;
                    if (entry4 != null)
                    {
                        WellKnownClientTypeEntry entry5 = new WellKnownClientTypeEntry(entry4.TypeName, entry4.AssemblyName, entry4.ObjectUrl);
                        System.Runtime.Remoting.RemoteAppEntry remoteAppEntry = entry4.GetRemoteAppEntry();
                        if (remoteAppEntry != null)
                        {
                            entry5.ApplicationUrl = remoteAppEntry.GetAppURI();
                        }
                        entryArray[num2++] = entry5;
                    }
                }
                return entryArray;
            }

            internal WellKnownServiceTypeEntry[] GetRegisteredWellKnownServiceTypes()
            {
                WellKnownServiceTypeEntry[] entryArray = new WellKnownServiceTypeEntry[this._wellKnownExportInfo.Count];
                int num = 0;
                foreach (DictionaryEntry entry in this._wellKnownExportInfo)
                {
                    WellKnownServiceTypeEntry entry2 = (WellKnownServiceTypeEntry) entry.Value;
                    WellKnownServiceTypeEntry entry3 = new WellKnownServiceTypeEntry(entry2.TypeName, entry2.AssemblyName, entry2.ObjectUri, entry2.Mode) {
                        ContextAttributes = entry2.ContextAttributes
                    };
                    entryArray[num++] = entry3;
                }
                return entryArray;
            }

            [SecurityCritical]
            internal Type GetServerTypeForUri(string URI)
            {
                Type type = null;
                string str = URI.ToLower(CultureInfo.InvariantCulture);
                WellKnownServiceTypeEntry entry = (WellKnownServiceTypeEntry) this._wellKnownExportInfo[str];
                if (entry != null)
                {
                    type = LoadType(entry.TypeName, entry.AssemblyName);
                }
                return type;
            }

            [SecurityCritical]
            internal static Type LoadType(string typeName, string assemblyName)
            {
                Assembly assembly = null;
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (assembly == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_AssemblyLoadFailed", new object[] { assemblyName }));
                }
                Type type = assembly.GetType(typeName, false, false);
                if (type == null)
                {
                    throw new RemotingException(Environment.GetResourceString("Remoting_BadType", new object[] { typeName + ", " + assemblyName }));
                }
                return type;
            }

            internal WellKnownClientTypeEntry QueryConnect(string typeName, string assemblyName)
            {
                string str = this.EncodeTypeAndAssemblyNames(typeName, assemblyName);
                WellKnownClientTypeEntry entry = this._remoteTypeInfo[str] as WellKnownClientTypeEntry;
                if (entry == null)
                {
                    return null;
                }
                return entry;
            }

            internal ActivatedClientTypeEntry QueryRemoteActivate(string typeName, string assemblyName)
            {
                string str = this.EncodeTypeAndAssemblyNames(typeName, assemblyName);
                ActivatedClientTypeEntry entry = this._remoteTypeInfo[str] as ActivatedClientTypeEntry;
                if (entry == null)
                {
                    return null;
                }
                if (entry.GetRemoteAppEntry() == null)
                {
                    System.Runtime.Remoting.RemoteAppEntry entry2 = (System.Runtime.Remoting.RemoteAppEntry) this._remoteAppInfo[entry.ApplicationUrl];
                    if (entry2 == null)
                    {
                        throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Activation_MissingRemoteAppEntry"), new object[] { entry.ApplicationUrl }));
                    }
                    entry.CacheRemoteAppEntry(entry2);
                }
                return entry;
            }

            [SecurityCritical]
            internal ServerIdentity StartupWellKnownObject(string URI)
            {
                string str = URI.ToLower(CultureInfo.InvariantCulture);
                ServerIdentity identity = null;
                WellKnownServiceTypeEntry entry = (WellKnownServiceTypeEntry) this._wellKnownExportInfo[str];
                if (entry != null)
                {
                    identity = this.StartupWellKnownObject(entry.AssemblyName, entry.TypeName, entry.ObjectUri, entry.Mode);
                }
                return identity;
            }

            [SecurityCritical]
            internal ServerIdentity StartupWellKnownObject(string asmName, string svrTypeName, string URI, WellKnownObjectMode mode)
            {
                return this.StartupWellKnownObject(asmName, svrTypeName, URI, mode, false);
            }

            [SecurityCritical]
            internal ServerIdentity StartupWellKnownObject(string asmName, string svrTypeName, string URI, WellKnownObjectMode mode, bool fReplace)
            {
                lock (s_wkoStartLock)
                {
                    MarshalByRefObject obj2 = null;
                    ServerIdentity identity = null;
                    Type type = LoadType(svrTypeName, asmName);
                    if (!type.IsMarshalByRef)
                    {
                        throw new RemotingException(Environment.GetResourceString("Remoting_WellKnown_MustBeMBR", new object[] { svrTypeName }));
                    }
                    identity = (ServerIdentity) IdentityHolder.ResolveIdentity(URI);
                    if ((identity != null) && identity.IsRemoteDisconnected())
                    {
                        IdentityHolder.RemoveIdentity(URI);
                        identity = null;
                    }
                    if (identity == null)
                    {
                        s_fullTrust.Assert();
                        try
                        {
                            obj2 = (MarshalByRefObject) Activator.CreateInstance(type, true);
                            if (RemotingServices.IsClientProxy(obj2))
                            {
                                RedirectionProxy proxy = new RedirectionProxy(obj2, type) {
                                    ObjectMode = mode
                                };
                                RemotingServices.MarshalInternal(proxy, URI, type);
                                identity = (ServerIdentity) IdentityHolder.ResolveIdentity(URI);
                                identity.SetSingletonObjectMode();
                            }
                            else if (type.IsCOMObject && (mode == WellKnownObjectMode.Singleton))
                            {
                                ComRedirectionProxy proxy2 = new ComRedirectionProxy(obj2, type);
                                RemotingServices.MarshalInternal(proxy2, URI, type);
                                identity = (ServerIdentity) IdentityHolder.ResolveIdentity(URI);
                                identity.SetSingletonObjectMode();
                            }
                            else
                            {
                                if (RemotingServices.GetObjectUri(obj2) != null)
                                {
                                    throw new RemotingException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_WellKnown_CtorCantMarshal"), new object[] { URI }));
                                }
                                RemotingServices.MarshalInternal(obj2, URI, type);
                                identity = (ServerIdentity) IdentityHolder.ResolveIdentity(URI);
                                if (mode == WellKnownObjectMode.SingleCall)
                                {
                                    identity.SetSingleCallObjectMode();
                                }
                                else
                                {
                                    identity.SetSingletonObjectMode();
                                }
                            }
                        }
                        catch
                        {
                            throw;
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                    return identity;
                }
            }

            internal void StoreActivatedExports(RemotingXmlConfigFileData configData)
            {
                foreach (RemotingXmlConfigFileData.TypeEntry entry in configData.ServerActivatedEntries)
                {
                    ActivatedServiceTypeEntry entry2 = new ActivatedServiceTypeEntry(entry.TypeName, entry.AssemblyName) {
                        ContextAttributes = CreateContextAttributesFromConfigEntries(entry.ContextAttributes)
                    };
                    RemotingConfiguration.RegisterActivatedServiceType(entry2);
                }
            }

            [SecurityCritical]
            internal void StoreInteropEntries(RemotingXmlConfigFileData configData)
            {
                foreach (RemotingXmlConfigFileData.InteropXmlElementEntry entry in configData.InteropXmlElementEntries)
                {
                    Type type = Assembly.Load(entry.UrtAssemblyName).GetType(entry.UrtTypeName);
                    SoapServices.RegisterInteropXmlElement(entry.XmlElementName, entry.XmlElementNamespace, type);
                }
                foreach (RemotingXmlConfigFileData.InteropXmlTypeEntry entry2 in configData.InteropXmlTypeEntries)
                {
                    Type type2 = Assembly.Load(entry2.UrtAssemblyName).GetType(entry2.UrtTypeName);
                    SoapServices.RegisterInteropXmlType(entry2.XmlTypeName, entry2.XmlTypeNamespace, type2);
                }
                foreach (RemotingXmlConfigFileData.PreLoadEntry entry3 in configData.PreLoadEntries)
                {
                    Assembly assembly = Assembly.Load(entry3.AssemblyName);
                    if (entry3.TypeName != null)
                    {
                        SoapServices.PreLoad(assembly.GetType(entry3.TypeName));
                    }
                    else
                    {
                        SoapServices.PreLoad(assembly);
                    }
                }
            }

            internal void StoreRemoteAppEntries(RemotingXmlConfigFileData configData)
            {
                char[] trimChars = new char[] { '/' };
                foreach (RemotingXmlConfigFileData.RemoteAppEntry entry in configData.RemoteAppEntries)
                {
                    string appUri = entry.AppUri;
                    if ((appUri != null) && !appUri.EndsWith("/", StringComparison.Ordinal))
                    {
                        appUri = appUri.TrimEnd(trimChars);
                    }
                    foreach (RemotingXmlConfigFileData.TypeEntry entry2 in entry.ActivatedObjects)
                    {
                        ActivatedClientTypeEntry entry3 = new ActivatedClientTypeEntry(entry2.TypeName, entry2.AssemblyName, appUri) {
                            ContextAttributes = CreateContextAttributesFromConfigEntries(entry2.ContextAttributes)
                        };
                        RemotingConfiguration.RegisterActivatedClientType(entry3);
                    }
                    foreach (RemotingXmlConfigFileData.ClientWellKnownEntry entry4 in entry.WellKnownObjects)
                    {
                        WellKnownClientTypeEntry entry5 = new WellKnownClientTypeEntry(entry4.TypeName, entry4.AssemblyName, entry4.Url) {
                            ApplicationUrl = appUri
                        };
                        RemotingConfiguration.RegisterWellKnownClientType(entry5);
                    }
                }
            }

            [SecurityCritical]
            internal void StoreWellKnownExports(RemotingXmlConfigFileData configData)
            {
                foreach (RemotingXmlConfigFileData.ServerWellKnownEntry entry in configData.ServerWellKnownEntries)
                {
                    WellKnownServiceTypeEntry entry2 = new WellKnownServiceTypeEntry(entry.TypeName, entry.AssemblyName, entry.ObjectURI, entry.ObjectMode) {
                        ContextAttributes = null
                    };
                    RemotingConfigHandler.RegisterWellKnownServiceType(entry2);
                }
            }
        }
    }
}

