namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public class ExtensionsSection : ConfigurationSection
    {
        private ConfigurationPropertyCollection properties;

        private static string GetExtensionType(Type extensionType)
        {
            string str = string.Empty;
            if (extensionType.IsSubclassOf(typeof(BehaviorExtensionElement)))
            {
                return "behaviorExtensions";
            }
            if (extensionType.IsSubclassOf(typeof(BindingElementExtensionElement)))
            {
                return "bindingElementExtensions";
            }
            if (extensionType.IsSubclassOf(typeof(BindingCollectionElement)))
            {
                return "bindingExtensions";
            }
            if (extensionType.IsSubclassOf(typeof(EndpointCollectionElement)))
            {
                return "endpointExtensions";
            }
            DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "{0} is not a type supported by the ServiceModelExtensionsSection collections.", new object[] { extensionType.AssemblyQualifiedName }));
            return str;
        }

        private void InitializeBehaviorElementExtensions()
        {
            this.BehaviorExtensions.Add(new ExtensionElement("clientCredentials", typeof(ClientCredentialsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceCredentials", typeof(ServiceCredentialsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("callbackDebug", typeof(CallbackDebugElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("clientVia", typeof(ClientViaElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("synchronousReceive", typeof(SynchronousReceiveElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("dispatcherSynchronization", typeof(DispatcherSynchronizationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceMetadata", typeof(ServiceMetadataPublishingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceDebug", typeof(ServiceDebugElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceAuthenticationManager", typeof(ServiceAuthenticationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceAuthorization", typeof(ServiceAuthorizationElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceSecurityAudit", typeof(ServiceSecurityAuditElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceThrottling", typeof(ServiceThrottlingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("transactedBatching", typeof(TransactedBatchingElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("dataContractSerializer", typeof(DataContractSerializerElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("serviceTimeouts", typeof(ServiceTimeoutsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("callbackTimeouts", typeof(CallbackTimeoutsElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("useRequestHeadersForMetadataAddress", typeof(UseRequestHeadersForMetadataAddressElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("clear", typeof(ClearBehaviorElement).AssemblyQualifiedName));
            this.BehaviorExtensions.Add(new ExtensionElement("remove", typeof(RemoveBehaviorElement).AssemblyQualifiedName));
        }

        private void InitializeBindingElementExtenions()
        {
            this.BindingElementExtensions.Add(new ExtensionElement("binaryMessageEncoding", typeof(BinaryMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("compositeDuplex", typeof(CompositeDuplexElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("oneWay", typeof(OneWayElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("transactionFlow", typeof(TransactionFlowElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("httpsTransport", typeof(HttpsTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("httpTransport", typeof(HttpTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("msmqIntegration", typeof(MsmqIntegrationElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("msmqTransport", typeof(MsmqTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("mtomMessageEncoding", typeof(MtomMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("namedPipeTransport", typeof(NamedPipeTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("peerTransport", typeof(PeerTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("pnrpPeerResolver", typeof(PnrpPeerResolverElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("privacyNoticeAt", typeof(PrivacyNoticeElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("reliableSession", typeof(ReliableSessionElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("security", typeof(System.ServiceModel.Configuration.SecurityElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("sslStreamSecurity", typeof(SslStreamSecurityElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("tcpTransport", typeof(TcpTransportElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("textMessageEncoding", typeof(TextMessageEncodingElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("unrecognizedPolicyAssertions", typeof(UnrecognizedPolicyAssertionElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("useManagedPresentation", typeof(UseManagedPresentationElement).AssemblyQualifiedName));
            this.BindingElementExtensions.Add(new ExtensionElement("windowsStreamSecurity", typeof(WindowsStreamSecurityElement).AssemblyQualifiedName));
        }

        private void InitializeBindingExtensions()
        {
            this.BindingExtensions.Add(new ExtensionElement("basicHttpBinding", typeof(BasicHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("customBinding", typeof(CustomBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("msmqIntegrationBinding", typeof(MsmqIntegrationBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("netMsmqBinding", typeof(NetMsmqBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("netNamedPipeBinding", typeof(NetNamedPipeBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("netPeerTcpBinding", typeof(NetPeerTcpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("netTcpBinding", typeof(NetTcpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("wsDualHttpBinding", typeof(WSDualHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("wsFederationHttpBinding", typeof(WSFederationHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("ws2007FederationHttpBinding", typeof(WS2007FederationHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("wsHttpBinding", typeof(WSHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("ws2007HttpBinding", typeof(WS2007HttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("mexHttpBinding", typeof(MexHttpBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("mexHttpsBinding", typeof(MexHttpsBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("mexNamedPipeBinding", typeof(MexNamedPipeBindingCollectionElement).AssemblyQualifiedName));
            this.BindingExtensions.Add(new ExtensionElement("mexTcpBinding", typeof(MexTcpBindingCollectionElement).AssemblyQualifiedName));
        }

        protected override void InitializeDefault()
        {
            this.InitializeBehaviorElementExtensions();
            this.InitializeBindingElementExtenions();
            this.InitializeBindingExtensions();
            this.InitializeEndpointExtensions();
        }

        private void InitializeEndpointExtensions()
        {
            this.EndpointExtensions.Add(new ExtensionElement("mexEndpoint", typeof(ServiceMetadataEndpointCollectionElement).AssemblyQualifiedName));
        }

        internal static ExtensionElementCollection LookupAssociatedCollection(Type extensionType, ContextInformation evaluationContext, out string collectionName)
        {
            collectionName = GetExtensionType(extensionType);
            return LookupCollection(collectionName, evaluationContext);
        }

        internal static ExtensionElementCollection LookupCollection(string collectionName, ContextInformation evaluationContext)
        {
            ExtensionsSection associatedSection = null;
            if (evaluationContext != null)
            {
                associatedSection = (ExtensionsSection) ConfigurationHelpers.GetAssociatedSection(evaluationContext, ConfigurationStrings.ExtensionsSectionPath);
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x80018, System.ServiceModel.SR.GetString("TraceCodeEvaluationContextNotFound"), null, (Exception) null);
                }
                associatedSection = (ExtensionsSection) ConfigurationHelpers.GetSection(ConfigurationStrings.ExtensionsSectionPath);
            }
            switch (collectionName)
            {
                case "behaviorExtensions":
                    return associatedSection.BehaviorExtensions;

                case "bindingElementExtensions":
                    return associatedSection.BindingElementExtensions;

                case "bindingExtensions":
                    return associatedSection.BindingExtensions;

                case "endpointExtensions":
                    return associatedSection.EndpointExtensions;
            }
            DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", new object[] { collectionName }));
            return null;
        }

        [SecurityCritical]
        internal static ExtensionElementCollection UnsafeLookupAssociatedCollection(Type extensionType, ContextInformation evaluationContext, out string collectionName)
        {
            collectionName = GetExtensionType(extensionType);
            return UnsafeLookupCollection(collectionName, evaluationContext);
        }

        [SecurityCritical]
        internal static ExtensionElementCollection UnsafeLookupCollection(string collectionName, ContextInformation evaluationContext)
        {
            ExtensionsSection section = null;
            if (evaluationContext != null)
            {
                section = (ExtensionsSection) ConfigurationHelpers.UnsafeGetAssociatedSection(evaluationContext, ConfigurationStrings.ExtensionsSectionPath);
            }
            else
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x80018, System.ServiceModel.SR.GetString("TraceCodeEvaluationContextNotFound"), null, (Exception) null);
                }
                section = (ExtensionsSection) ConfigurationHelpers.UnsafeGetSection(ConfigurationStrings.ExtensionsSectionPath);
            }
            switch (collectionName)
            {
                case "behaviorExtensions":
                    return section.BehaviorExtensions;

                case "bindingElementExtensions":
                    return section.BindingElementExtensions;

                case "bindingExtensions":
                    return section.BindingExtensions;

                case "endpointExtensions":
                    return section.EndpointExtensions;
            }
            DiagnosticUtility.FailFast(string.Format(CultureInfo.InvariantCulture, "{0} is not a valid ServiceModelExtensionsSection collection name.", new object[] { collectionName }));
            return null;
        }

        [ConfigurationProperty("behaviorExtensions")]
        public ExtensionElementCollection BehaviorExtensions
        {
            get
            {
                return (ExtensionElementCollection) base["behaviorExtensions"];
            }
        }

        [ConfigurationProperty("bindingElementExtensions")]
        public ExtensionElementCollection BindingElementExtensions
        {
            get
            {
                return (ExtensionElementCollection) base["bindingElementExtensions"];
            }
        }

        [ConfigurationProperty("bindingExtensions")]
        public ExtensionElementCollection BindingExtensions
        {
            get
            {
                return (ExtensionElementCollection) base["bindingExtensions"];
            }
        }

        [ConfigurationProperty("endpointExtensions")]
        public ExtensionElementCollection EndpointExtensions
        {
            get
            {
                return (ExtensionElementCollection) base["endpointExtensions"];
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("behaviorExtensions", typeof(ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bindingElementExtensions", typeof(ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("bindingExtensions", typeof(ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("endpointExtensions", typeof(ExtensionElementCollection), null, null, null, ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

