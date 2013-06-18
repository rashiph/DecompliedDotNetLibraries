namespace System.ServiceModel.Activation
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.Security.Permissions;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;
    using System.Transactions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;

    internal class HostedAspNetEnvironment : AspNetEnvironment
    {
        private static string cachedServiceReference;
        private bool? isWindowsAuthentication;

        private HostedAspNetEnvironment()
        {
        }

        public override void AddHostingBehavior(ServiceHostBase serviceHost, System.ServiceModel.Description.ServiceDescription description)
        {
            VirtualPathExtension virtualPathExtension = serviceHost.Extensions.Find<VirtualPathExtension>();
            if (virtualPathExtension != null)
            {
                description.Behaviors.Add(new HostedBindingBehavior(virtualPathExtension));
            }
        }

        public override void ApplyHostedContext(TransportChannelListener listener, BindingContext context)
        {
            VirtualPathExtension extension = context.BindingParameters.Find<VirtualPathExtension>();
            if (extension != null)
            {
                HostedMetadataBindingParameter parameter = context.BindingParameters.Find<HostedMetadataBindingParameter>();
                listener.ApplyHostedContext(extension.VirtualPath, parameter != null);
            }
        }

        public override void DecrementBusyCount()
        {
            HostingEnvironmentWrapper.DecrementBusyCount();
        }

        public static void Enable()
        {
            AspNetEnvironment environment = new HostedAspNetEnvironment();
            AspNetEnvironment.Current = environment;
        }

        public override void EnsureAllReferencedAssemblyLoaded()
        {
            BuildManager.GetReferencedAssemblies();
        }

        public override void EnsureCompatibilityRequirements(System.ServiceModel.Description.ServiceDescription description)
        {
            if (description.Behaviors.Find<AspNetCompatibilityRequirementsAttribute>() == null)
            {
                AspNetCompatibilityRequirementsAttribute item = new AspNetCompatibilityRequirementsAttribute();
                description.Behaviors.Add(item);
            }
        }

        public override string GetAnnotationFromHost(ServiceHostBase host)
        {
            if ((host != null) && (host.Extensions != null))
            {
                string str = (host.Description != null) ? host.Description.Name : string.Empty;
                string applicationVirtualPath = ServiceHostingEnvironment.ApplicationVirtualPath;
                string str3 = string.Empty;
                VirtualPathExtension extension = host.Extensions.Find<VirtualPathExtension>();
                if ((extension != null) && (extension.VirtualPath != null))
                {
                    str3 = extension.VirtualPath.Replace("~", applicationVirtualPath + "|");
                    return string.Format(CultureInfo.InvariantCulture, "{0}{1}|{2}", new object[] { ServiceHostingEnvironment.SiteName, str3, str });
                }
            }
            if (string.IsNullOrEmpty(cachedServiceReference))
            {
                cachedServiceReference = string.Format(CultureInfo.InvariantCulture, "{0}{1}", new object[] { ServiceHostingEnvironment.SiteName, ServiceHostingEnvironment.ApplicationVirtualPath });
            }
            return cachedServiceReference;
        }

        public override AuthenticationSchemes GetAuthenticationSchemes(Uri baseAddress)
        {
            string str3;
            string fileName = VirtualPathUtility.GetFileName(baseAddress.AbsolutePath);
            string currentVirtualPath = ServiceHostingEnvironment.CurrentVirtualPath;
            if ((currentVirtualPath != null) && currentVirtualPath.EndsWith("/", StringComparison.Ordinal))
            {
                str3 = currentVirtualPath + fileName;
            }
            else
            {
                str3 = string.Format(CultureInfo.InvariantCulture, "{0}/{1}", new object[] { currentVirtualPath, fileName });
            }
            AuthenticationSchemes authenticationSchemes = HostedTransportConfigurationManager.MetabaseSettings.GetAuthenticationSchemes(str3);
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost || (authenticationSchemes != (AuthenticationSchemes.Anonymous | AuthenticationSchemes.Ntlm)))
            {
                return authenticationSchemes;
            }
            if (this.IsWindowsAuthenticationConfigured())
            {
                return AuthenticationSchemes.Ntlm;
            }
            return AuthenticationSchemes.Anonymous;
        }

        public override BaseUriWithWildcard GetBaseUri(string transportScheme, Uri listenUri)
        {
            BaseUriWithWildcard wildcard = null;
            HostedTransportConfigurationBase configuration = HostedTransportConfigurationManager.GetConfiguration(transportScheme) as HostedTransportConfigurationBase;
            if (configuration != null)
            {
                wildcard = configuration.FindBaseAddress(listenUri);
                if (wildcard == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_TransportBindingNotFound(listenUri.ToString())));
                }
            }
            return wildcard;
        }

        public override object GetConfigurationSection(string sectionPath)
        {
            return GetSectionFromWebConfigurationManager(sectionPath, ServiceHostingEnvironment.FullVirtualPath);
        }

        public override IAspNetMessageProperty GetHostingProperty(Message message)
        {
            return this.GetHostingProperty(message, false);
        }

        private IAspNetMessageProperty GetHostingProperty(Message message, bool removeFromMessage)
        {
            IAspNetMessageProperty property = null;
            object obj2;
            if (message.Properties.TryGetValue(HostingMessageProperty.Name, out obj2))
            {
                property = (HostingMessageProperty) obj2;
                if (removeFromMessage)
                {
                    message.Properties.Remove(HostingMessageProperty.Name);
                }
            }
            return property;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static object GetSectionFromWebConfigurationManager(string sectionPath, string virtualPath)
        {
            if (virtualPath != null)
            {
                return WebConfigurationManager.GetSection(sectionPath, virtualPath);
            }
            return WebConfigurationManager.GetSection(sectionPath);
        }

        public override void IncrementBusyCount()
        {
            HostingEnvironmentWrapper.IncrementBusyCount();
        }

        public override bool IsWebConfigAboveApplication(object configHostingContext)
        {
            WebContext context = configHostingContext as WebContext;
            return ((context != null) && (context.ApplicationLevel == WebApplicationLevel.AboveApplication));
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        private bool IsWindowsAuthenticationConfigured()
        {
            if (!this.isWindowsAuthentication.HasValue)
            {
                AuthenticationSection section = (AuthenticationSection) this.UnsafeGetConfigurationSection("system.web/authentication");
                if (section != null)
                {
                    this.isWindowsAuthentication = new bool?(section.Mode == AuthenticationMode.Windows);
                }
                else
                {
                    this.isWindowsAuthentication = false;
                }
            }
            return this.isWindowsAuthentication.Value;
        }

        public override IAspNetMessageProperty PrepareMessageForDispatch(Message message)
        {
            ReceiveContext property = null;
            if (ReceiveContext.TryGet(message, out property) && !(property is ReceiveContextBusyCountWrapper))
            {
                ReceiveContextBusyCountWrapper wrapper = new ReceiveContextBusyCountWrapper(property);
                message.Properties.Remove(ReceiveContext.Name);
                message.Properties.Add(ReceiveContext.Name, wrapper);
            }
            return this.GetHostingProperty(message, true);
        }

        public override void ProcessBehaviorForMetadataExtension(IServiceBehavior serviceBehavior, BindingParameterCollection bindingParameters)
        {
            if (serviceBehavior is HostedBindingBehavior)
            {
                bindingParameters.Add(((HostedBindingBehavior) serviceBehavior).VirtualPathExtension);
                bindingParameters.Add(new HostedMetadataBindingParameter());
            }
        }

        public override void ProcessNotMatchedEndpointAddress(Uri uri, string endpointName)
        {
            if (!object.ReferenceEquals(uri.Scheme, Uri.UriSchemeHttp) && !object.ReferenceEquals(uri.Scheme, Uri.UriSchemeHttps))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_NonHTTPInCompatibilityMode(endpointName)));
            }
        }

        public override void TraceDecrementBusyCount(string data)
        {
            if (data == null)
            {
                data = System.ServiceModel.Activation.SR.DefaultBusyCountSource;
            }
            TD.DecrementBusyCount(data);
        }

        public override bool TraceDecrementBusyCountIsEnabled()
        {
            return TD.DecrementBusyCountIsEnabled();
        }

        public override void TraceIncrementBusyCount(string data)
        {
            if (data == null)
            {
                data = System.ServiceModel.Activation.SR.DefaultBusyCountSource;
            }
            TD.IncrementBusyCount(data);
        }

        public override bool TraceIncrementBusyCountIsEnabled()
        {
            return TD.IncrementBusyCountIsEnabled();
        }

        public override bool TryGetFullVirtualPath(out string virtualPath)
        {
            virtualPath = ServiceHostingEnvironment.FullVirtualPath;
            return true;
        }

        [SecurityCritical]
        public override object UnsafeGetConfigurationSection(string sectionPath)
        {
            return UnsafeGetSectionFromWebConfigurationManager(sectionPath, ServiceHostingEnvironment.FullVirtualPath);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical, ConfigurationPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static object UnsafeGetSectionFromWebConfigurationManager(string sectionPath, string virtualPath)
        {
            if (virtualPath != null)
            {
                return WebConfigurationManager.GetSection(sectionPath, virtualPath);
            }
            return WebConfigurationManager.GetSection(sectionPath);
        }

        public override void ValidateCompatibilityRequirements(AspNetCompatibilityRequirementsMode compatibilityMode)
        {
            if (compatibilityMode != AspNetCompatibilityRequirementsMode.Allowed)
            {
                if (ServiceHostingEnvironment.AspNetCompatibilityEnabled && (compatibilityMode == AspNetCompatibilityRequirementsMode.NotAllowed))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ServiceCompatibilityNotAllowed));
                }
                if (!ServiceHostingEnvironment.AspNetCompatibilityEnabled && (compatibilityMode == AspNetCompatibilityRequirementsMode.Required))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_ServiceCompatibilityRequire));
                }
            }
        }

        public override void ValidateHttpSettings(string virtualPath, bool isMetadataListener, bool usingDefaultSpnList, ref AuthenticationSchemes supportedSchemes, ref ExtendedProtectionPolicy extendedProtectionPolicy, ref string realm)
        {
            AuthenticationSchemes authenticationSchemes = HostedTransportConfigurationManager.MetabaseSettings.GetAuthenticationSchemes(virtualPath);
            if (((supportedSchemes == AuthenticationSchemes.Anonymous) && ((authenticationSchemes & AuthenticationSchemes.Anonymous) == AuthenticationSchemes.None)) && isMetadataListener)
            {
                if ((authenticationSchemes & AuthenticationSchemes.Negotiate) != AuthenticationSchemes.None)
                {
                    supportedSchemes = AuthenticationSchemes.Negotiate;
                }
                else
                {
                    supportedSchemes = authenticationSchemes;
                }
            }
            if ((supportedSchemes & authenticationSchemes) == AuthenticationSchemes.None)
            {
                if (AuthenticationSchemesHelper.IsWindowsAuth(supportedSchemes))
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_AuthSchemesRequireWindowsAuth));
                }
                throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_AuthSchemesRequireOtherAuth(((AuthenticationSchemes) supportedSchemes).ToString())));
            }
            if (supportedSchemes != AuthenticationSchemes.Anonymous)
            {
                ExtendedProtectionPolicy policy = HostedTransportConfigurationManager.MetabaseSettings.GetExtendedProtectionPolicy(virtualPath);
                if (policy == null)
                {
                    if (extendedProtectionPolicy.PolicyEnforcement == PolicyEnforcement.Always)
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.ExtendedProtectionNotSupported));
                    }
                }
                else if (isMetadataListener && ChannelBindingUtility.IsDefaultPolicy(extendedProtectionPolicy))
                {
                    extendedProtectionPolicy = policy;
                }
                else
                {
                    if (!ChannelBindingUtility.AreEqual(policy, extendedProtectionPolicy))
                    {
                        string extendedProtectionPolicyCustomChannelBindingMismatch;
                        if (policy.PolicyEnforcement != extendedProtectionPolicy.PolicyEnforcement)
                        {
                            extendedProtectionPolicyCustomChannelBindingMismatch = System.ServiceModel.Activation.SR.ExtendedProtectionPolicyEnforcementMismatch(policy.PolicyEnforcement, extendedProtectionPolicy.PolicyEnforcement);
                        }
                        else if (policy.ProtectionScenario != extendedProtectionPolicy.ProtectionScenario)
                        {
                            extendedProtectionPolicyCustomChannelBindingMismatch = System.ServiceModel.Activation.SR.ExtendedProtectionPolicyScenarioMismatch(policy.ProtectionScenario, extendedProtectionPolicy.ProtectionScenario);
                        }
                        else
                        {
                            extendedProtectionPolicyCustomChannelBindingMismatch = System.ServiceModel.Activation.SR.ExtendedProtectionPolicyCustomChannelBindingMismatch;
                        }
                        if (extendedProtectionPolicyCustomChannelBindingMismatch != null)
                        {
                            throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_ExtendedProtectionPoliciesMustMatch(extendedProtectionPolicyCustomChannelBindingMismatch)));
                        }
                    }
                    ServiceNameCollection subset = usingDefaultSpnList ? null : extendedProtectionPolicy.CustomServiceNames;
                    if (!ChannelBindingUtility.IsSubset(policy.CustomServiceNames, subset))
                    {
                        throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_ExtendedProtectionPoliciesMustMatch(System.ServiceModel.Activation.SR.Hosting_ExtendedProtectionSPNListNotSubset)));
                    }
                }
            }
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                realm = HostedTransportConfigurationManager.MetabaseSettings.GetRealm(virtualPath);
            }
        }

        public override bool ValidateHttpsSettings(string virtualPath, ref bool? requireClientCertificate)
        {
            bool flag = false;
            if (!ServiceHostingEnvironment.IsSimpleApplicationHost)
            {
                HttpAccessSslFlags accessSslFlags = HostedTransportConfigurationManager.MetabaseSettings.GetAccessSslFlags(virtualPath);
                HttpAccessSslFlags none = HttpAccessSslFlags.None;
                bool flag2 = false;
                if ((accessSslFlags & HttpAccessSslFlags.SslRequireCert) != HttpAccessSslFlags.None)
                {
                    if (requireClientCertificate.HasValue)
                    {
                        if (!requireClientCertificate.Value)
                        {
                            flag2 = true;
                        }
                    }
                    else
                    {
                        requireClientCertificate = 1;
                    }
                }
                else if (requireClientCertificate.GetValueOrDefault())
                {
                    none |= HttpAccessSslFlags.SslRequireCert;
                    flag2 = true;
                }
                if (!flag2 && ((accessSslFlags & HttpAccessSslFlags.SslMapCert) != HttpAccessSslFlags.None))
                {
                    flag = true;
                }
                if (flag2)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException(System.ServiceModel.Activation.SR.Hosting_SslSettingsMisconfigured(none.ToString(), accessSslFlags.ToString())));
                }
            }
            return flag;
        }

        public override bool AspNetCompatibilityEnabled
        {
            get
            {
                return ServiceHostingEnvironment.AspNetCompatibilityEnabled;
            }
        }

        public override string ConfigurationPath
        {
            get
            {
                if (ServiceHostingEnvironment.CurrentVirtualPath != null)
                {
                    return (ServiceHostingEnvironment.CurrentVirtualPath + "web.config");
                }
                return base.ConfigurationPath;
            }
        }

        public override string CurrentVirtualPath
        {
            get
            {
                return ServiceHostingEnvironment.CurrentVirtualPath;
            }
        }

        public override bool IsConfigurationBased
        {
            get
            {
                return ServiceHostingEnvironment.IsConfigurationBased;
            }
        }

        public override string XamlFileBaseLocation
        {
            get
            {
                return ServiceHostingEnvironment.XamlFileBaseLocation;
            }
        }

        private class HostedMetadataBindingParameter
        {
        }

        private class ReceiveContextBusyCountWrapper : ReceiveContext
        {
            private int ambientTransactionCount;
            private int busyCount;
            private ReceiveContext wrappedContext;

            internal ReceiveContextBusyCountWrapper(ReceiveContext context)
            {
                this.wrappedContext = context;
                this.wrappedContext.Faulted += new EventHandler(this.OnWrappedContextFaulted);
                AspNetEnvironment.Current.IncrementBusyCount();
                if (AspNetEnvironment.Current.TraceIncrementBusyCountIsEnabled())
                {
                    AspNetEnvironment.Current.TraceIncrementBusyCount(base.GetType().FullName);
                }
                Interlocked.Increment(ref this.busyCount);
            }

            private void DecrementBusyCount()
            {
                if (Interlocked.Exchange(ref this.busyCount, 0) == 1)
                {
                    AspNetEnvironment.Current.DecrementBusyCount();
                    if (AspNetEnvironment.Current.TraceDecrementBusyCountIsEnabled())
                    {
                        AspNetEnvironment.Current.TraceDecrementBusyCount(base.GetType().FullName);
                    }
                }
            }

            private void DecrementOnNoAmbientTransaction()
            {
                if (Interlocked.Exchange(ref this.ambientTransactionCount, 0) != 1)
                {
                    this.DecrementBusyCount();
                }
            }

            protected override void OnAbandon(TimeSpan timeout)
            {
                this.wrappedContext.Abandon(timeout);
                this.DecrementBusyCount();
            }

            protected override IAsyncResult OnBeginAbandon(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.wrappedContext.BeginAbandon(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginComplete(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.RegisterForTransactionNotification(Transaction.Current);
                return this.wrappedContext.BeginComplete(timeout, callback, state);
            }

            protected override void OnComplete(TimeSpan timeout)
            {
                this.RegisterForTransactionNotification(Transaction.Current);
                this.wrappedContext.Complete(timeout);
                this.DecrementOnNoAmbientTransaction();
            }

            protected override void OnEndAbandon(IAsyncResult result)
            {
                this.wrappedContext.EndAbandon(result);
                this.DecrementBusyCount();
            }

            protected override void OnEndComplete(IAsyncResult result)
            {
                this.wrappedContext.EndComplete(result);
                this.DecrementOnNoAmbientTransaction();
            }

            protected override void OnFaulted()
            {
                try
                {
                    this.wrappedContext.Fault();
                }
                finally
                {
                    base.OnFaulted();
                }
            }

            private void OnWrappedContextFaulted(object sender, EventArgs e)
            {
                try
                {
                    this.Fault();
                }
                finally
                {
                    this.DecrementBusyCount();
                }
            }

            private void RegisterForTransactionNotification(Transaction transaction)
            {
                if (Transaction.Current != null)
                {
                    ReceiveContextEnlistmentNotification enlistmentNotification = new ReceiveContextEnlistmentNotification(this);
                    transaction.EnlistVolatile(enlistmentNotification, EnlistmentOptions.None);
                    Interlocked.Increment(ref this.ambientTransactionCount);
                }
            }

            private class ReceiveContextEnlistmentNotification : IEnlistmentNotification
            {
                private HostedAspNetEnvironment.ReceiveContextBusyCountWrapper context;

                internal ReceiveContextEnlistmentNotification(HostedAspNetEnvironment.ReceiveContextBusyCountWrapper context)
                {
                    this.context = context;
                }

                public void Commit(Enlistment enlistment)
                {
                    this.context.DecrementBusyCount();
                    enlistment.Done();
                }

                public void InDoubt(Enlistment enlistment)
                {
                    this.context.DecrementBusyCount();
                    enlistment.Done();
                }

                public void Prepare(PreparingEnlistment preparingEnlistment)
                {
                    preparingEnlistment.Prepared();
                }

                public void Rollback(Enlistment enlistment)
                {
                    enlistment.Done();
                }
            }
        }
    }
}

