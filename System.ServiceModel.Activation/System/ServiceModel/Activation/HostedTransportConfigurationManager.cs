namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Activation.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;

    internal class HostedTransportConfigurationManager
    {
        private IDictionary<string, HostedTransportConfiguration> configurations;
        private const string CreateMetabaseSettingsIis7MethodName = "CreateMetabaseSettings";
        private bool initialized;
        private System.ServiceModel.Activation.MetabaseSettings metabaseSettings;
        private const string MetabaseSettingsIis7FactoryTypeName = "System.ServiceModel.WasHosting.MetabaseSettingsIis7Factory, System.ServiceModel.WasHosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
        private static HostedTransportConfigurationManager singleton;
        private static object syncRoot = new object();
        private const string WasHostingAssemblyName = "System.ServiceModel.WasHosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

        private HostedTransportConfigurationManager()
        {
            this.configurations = new Dictionary<string, HostedTransportConfiguration>(StringComparer.Ordinal);
            if (!Iis7Helper.IsIis7)
            {
                this.metabaseSettings = new MetabaseSettingsIis6();
            }
            else
            {
                this.metabaseSettings = CreateWasHostingMetabaseSettings();
            }
        }

        private HostedTransportConfigurationManager(System.ServiceModel.Activation.MetabaseSettings metabaseSettings)
        {
            this.configurations = new Dictionary<string, HostedTransportConfiguration>(StringComparer.Ordinal);
            this.metabaseSettings = metabaseSettings;
        }

        private void AddHostedTransportConfigurationIis7(string protocol)
        {
            HostedTransportConfiguration configuration = null;
            try
            {
                ServiceHostingEnvironmentSection section = ServiceHostingEnvironmentSection.GetSection();
                if (!section.TransportConfigurationTypes.ContainsKey(protocol))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ProtocolNoConfiguration(protocol)));
                }
                TransportConfigurationTypeElement element = section.TransportConfigurationTypes[protocol];
                configuration = Activator.CreateInstance(Type.GetType(element.TransportConfigurationType)) as HostedTransportConfiguration;
                this.configurations.Add(protocol, configuration);
            }
            catch (Exception exception)
            {
                if (!Fx.IsFatal(exception) && DiagnosticUtility.ShouldTraceError)
                {
                    System.ServiceModel.Activation.Diagnostics.TraceUtility.TraceEvent(TraceEventType.Error, 0x90006, System.ServiceModel.Activation.SR.TraceCodeWebHostProtocolMisconfigured, new StringTraceRecord("Protocol", protocol), this, exception);
                }
                throw;
            }
        }

        [SecurityCritical]
        private static MetabaseSettingsIis CreateMetabaseSettings(Type type)
        {
            object obj2 = null;
            MethodInfo method = type.GetMethod("CreateMetabaseSettings", BindingFlags.NonPublic | BindingFlags.Static);
            try
            {
                new PermissionSet(PermissionState.Unrestricted).Assert();
                obj2 = method.Invoke(null, null);
            }
            finally
            {
                PermissionSet.RevertAssert();
            }
            if (!(obj2 is MetabaseSettingsIis))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_BadMetabaseSettingsIis7Type(type.AssemblyQualifiedName)));
            }
            return (MetabaseSettingsIis) obj2;
        }

        [SecuritySafeCritical]
        private static MetabaseSettingsIis CreateWasHostingMetabaseSettings()
        {
            Type type = Type.GetType("System.ServiceModel.WasHosting.MetabaseSettingsIis7Factory, System.ServiceModel.WasHosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", false);
            if (type == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_MetabaseSettingsIis7TypeNotFound("System.ServiceModel.WasHosting.MetabaseSettingsIis7Factory, System.ServiceModel.WasHosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "System.ServiceModel.WasHosting, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")));
            }
            return CreateMetabaseSettings(type);
        }

        private void EnsureInitialized()
        {
            if (!this.initialized)
            {
                lock (this.ThisLock)
                {
                    if (!this.initialized)
                    {
                        foreach (string str in this.metabaseSettings.GetProtocols())
                        {
                            if ((string.CompareOrdinal(str, Uri.UriSchemeHttp) == 0) || (string.CompareOrdinal(str, Uri.UriSchemeHttps) == 0))
                            {
                                HttpHostedTransportConfiguration configuration = null;
                                if (string.CompareOrdinal(str, Uri.UriSchemeHttp) == 0)
                                {
                                    configuration = new HttpHostedTransportConfiguration();
                                }
                                else
                                {
                                    configuration = new HttpsHostedTransportConfiguration();
                                }
                                this.configurations.Add(str, configuration);
                            }
                            else
                            {
                                if (!Iis7Helper.IsIis7)
                                {
                                    throw Fx.AssertAndThrowFatal("HostedTransportConfigurationManager.EnsureInitialized() protocols other than http and https can only be configured in IIS7");
                                }
                                if (AspNetPartialTrustHelpers.NeedPartialTrustInvoke)
                                {
                                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.PartialTrustNonHttpActivation(str, HostingEnvironmentWrapper.ApplicationVirtualPath)));
                                }
                                this.AddHostedTransportConfigurationIis7(str);
                            }
                        }
                        this.initialized = true;
                    }
                }
            }
        }

        internal static void EnsureInitializedForSimpleApplicationHost(HostedHttpRequestAsyncResult result)
        {
            if (singleton == null)
            {
                lock (syncRoot)
                {
                    if (singleton == null)
                    {
                        singleton = new HostedTransportConfigurationManager(new MetabaseSettingsCassini(result));
                    }
                }
            }
        }

        internal static Uri[] GetBaseAddresses(string virtualPath)
        {
            return Value.InternalGetBaseAddresses(virtualPath);
        }

        internal static HostedTransportConfiguration GetConfiguration(string scheme)
        {
            return Value.InternalGetConfiguration(scheme);
        }

        private Uri[] InternalGetBaseAddresses(string virtualPath)
        {
            this.EnsureInitialized();
            List<Uri> list = new List<Uri>();
            foreach (HostedTransportConfiguration configuration in this.configurations.Values)
            {
                list.AddRange(configuration.GetBaseAddresses(virtualPath));
            }
            return list.ToArray();
        }

        private HostedTransportConfiguration InternalGetConfiguration(string scheme)
        {
            this.EnsureInitialized();
            if (!this.configurations.ContainsKey(scheme))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_NotSupportedProtocol(scheme)));
            }
            return this.configurations[scheme];
        }

        internal static System.ServiceModel.Activation.MetabaseSettings MetabaseSettings
        {
            get
            {
                return Value.metabaseSettings;
            }
        }

        private object ThisLock
        {
            get
            {
                return this;
            }
        }

        private static HostedTransportConfigurationManager Value
        {
            get
            {
                if (singleton == null)
                {
                    lock (syncRoot)
                    {
                        if (singleton == null)
                        {
                            System.ServiceModel.Diagnostics.TraceUtility.SetEtwProviderId();
                            if (TD.HostedTransportConfigurationManagerConfigInitStartIsEnabled())
                            {
                                TD.HostedTransportConfigurationManagerConfigInitStart();
                            }
                            ServiceHostingEnvironment.EnsureInitialized();
                            singleton = new HostedTransportConfigurationManager();
                            if (TD.HostedTransportConfigurationManagerConfigInitStopIsEnabled())
                            {
                                TD.HostedTransportConfigurationManagerConfigInitStop();
                            }
                        }
                    }
                }
                return singleton;
            }
        }
    }
}

