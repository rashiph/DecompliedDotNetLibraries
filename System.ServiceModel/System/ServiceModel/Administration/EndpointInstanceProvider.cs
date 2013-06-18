namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.MsmqIntegration;
    using System.Xml;

    internal class EndpointInstanceProvider : ProviderBase, IWmiProvider
    {
        internal static string EndpointReference(Uri uri, string contractName)
        {
            return EndpointReference((null != uri) ? uri.ToString() : string.Empty, contractName, true);
        }

        internal static string EndpointReference(string address, string contractName, bool local)
        {
            Uri uri;
            string str = string.Format(CultureInfo.InvariantCulture, "Endpoint.ListenUri='{0}',ContractName='{1}',AppDomainId='{2}',ProcessId={3}", new object[] { address, (contractName != null) ? contractName : string.Empty, AppDomainInfo.Current.Id, AppDomainInfo.Current.ProcessId });
            if (!local && Uri.TryCreate(address, UriKind.Absolute, out uri))
            {
                string host = uri.Host;
                if (!"localhost".Equals(host, StringComparison.OrdinalIgnoreCase) && !AppDomainInfo.Current.MachineName.Equals(host, StringComparison.OrdinalIgnoreCase))
                {
                    str = string.Format(CultureInfo.InvariantCulture, @"\\{0}\root\ServiceModel:", new object[] { host }) + str;
                }
            }
            return str;
        }

        private static void FillAddressInfo(System.ServiceModel.Administration.EndpointInfo endpoint, IWmiInstance instance)
        {
            string[] info = new string[endpoint.Headers.Count];
            int num = 0;
            foreach (AddressHeader header in endpoint.Headers)
            {
                PlainXmlWriter writer = new PlainXmlWriter();
                header.WriteAddressHeader(writer);
                info[num++] = writer.ToString();
            }
            ProviderBase.FillCollectionInfo(info, instance, "AddressHeaders");
            instance.SetProperty("Address", (endpoint.Address == null) ? string.Empty : endpoint.Address.ToString());
            instance.SetProperty("ListenUri", (endpoint.ListenUri == null) ? string.Empty : endpoint.ListenUri.ToString());
            instance.SetProperty("AddressIdentity", (endpoint.Identity == null) ? string.Empty : endpoint.Identity.ToString());
        }

        private static void FillBehaviorInfo(IEndpointBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            instance = null;
            if (behavior is ClientCredentials)
            {
                instance = existingInstance.NewInstance("ClientCredentials");
                ClientCredentials credentials = (ClientCredentials) behavior;
                instance.SetProperty("SupportInteractive", credentials.SupportInteractive);
                if ((credentials.ClientCertificate != null) && (credentials.ClientCertificate.Certificate != null))
                {
                    instance.SetProperty("ClientCertificate", credentials.ClientCertificate.Certificate.ToString());
                }
                if (credentials.IssuedToken != null)
                {
                    string str = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { "CacheIssuedTokens", credentials.IssuedToken.CacheIssuedTokens });
                    instance.SetProperty("IssuedToken", str);
                }
                if (credentials.HttpDigest != null)
                {
                    string str2 = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { "AllowedImpersonationLevel", credentials.HttpDigest.AllowedImpersonationLevel.ToString() });
                    instance.SetProperty("HttpDigest", str2);
                }
                if ((credentials.Peer != null) && (credentials.Peer.Certificate != null))
                {
                    instance.SetProperty("Peer", credentials.Peer.Certificate.ToString(true));
                }
                if (credentials.UserName != null)
                {
                    instance.SetProperty("UserName", "********");
                }
                if (credentials.Windows != null)
                {
                    string str3 = string.Format(CultureInfo.InvariantCulture, "{0}: {1}, {2}: {3}", new object[] { "AllowedImpersonationLevel", credentials.Windows.AllowedImpersonationLevel.ToString(), "AllowNtlm", credentials.Windows.AllowNtlm });
                    instance.SetProperty("Windows", str3);
                }
            }
            else if (behavior is MustUnderstandBehavior)
            {
                instance = existingInstance.NewInstance("MustUnderstandBehavior");
            }
            else if (behavior is SynchronousReceiveBehavior)
            {
                instance = existingInstance.NewInstance("SynchronousReceiveBehavior");
            }
            else if (behavior is DispatcherSynchronizationBehavior)
            {
                instance = existingInstance.NewInstance("DispatcherSynchronizationBehavior");
            }
            else if (behavior is TransactedBatchingBehavior)
            {
                instance = existingInstance.NewInstance("TransactedBatchingBehavior");
                instance.SetProperty("MaxBatchSize", ((TransactedBatchingBehavior) behavior).MaxBatchSize);
            }
            else if (behavior is ClientViaBehavior)
            {
                instance = existingInstance.NewInstance("ClientViaBehavior");
                instance.SetProperty("Uri", ((ClientViaBehavior) behavior).Uri.ToString());
            }
            else if (behavior is IWmiInstanceProvider)
            {
                IWmiInstanceProvider provider = (IWmiInstanceProvider) behavior;
                instance = existingInstance.NewInstance(provider.GetInstanceType());
                provider.FillInstance(instance);
            }
            else
            {
                instance = existingInstance.NewInstance("Behavior");
            }
            if (instance != null)
            {
                instance.SetProperty("Type", behavior.GetType().FullName);
            }
        }

        private static void FillBehaviorsInfo(System.ServiceModel.Administration.EndpointInfo info, IWmiInstance instance)
        {
            List<IWmiInstance> list = new List<IWmiInstance>(info.Behaviors.Count);
            foreach (IEndpointBehavior behavior in info.Behaviors)
            {
                IWmiInstance instance2;
                FillBehaviorInfo(behavior, instance, out instance2);
                if (instance2 != null)
                {
                    list.Add(instance2);
                }
            }
            instance.SetProperty("Behaviors", list.ToArray());
        }

        private static void FillBindingInfo(System.ServiceModel.Administration.EndpointInfo endpoint, IWmiInstance instance)
        {
            IWmiInstance instance2 = instance.NewInstance("Binding");
            IWmiInstance[] instanceArray = new IWmiInstance[endpoint.Binding.Elements.Count];
            for (int i = 0; i < instanceArray.Length; i++)
            {
                instanceArray[i] = instance2;
                FillBindingInfo(endpoint.Binding.Elements[i], ref instanceArray[i]);
            }
            instance2.SetProperty("BindingElements", instanceArray);
            instance2.SetProperty("Name", endpoint.Binding.Name);
            instance2.SetProperty("Namespace", endpoint.Binding.Namespace);
            instance2.SetProperty("CloseTimeout", endpoint.Binding.CloseTimeout);
            instance2.SetProperty("Scheme", endpoint.Binding.Scheme);
            instance2.SetProperty("OpenTimeout", endpoint.Binding.OpenTimeout);
            instance2.SetProperty("ReceiveTimeout", endpoint.Binding.ReceiveTimeout);
            instance2.SetProperty("SendTimeout", endpoint.Binding.SendTimeout);
            instance.SetProperty("Binding", instance2);
        }

        private static void FillBindingInfo(BindingElement bindingElement, ref IWmiInstance instance)
        {
            if (bindingElement is IWmiInstanceProvider)
            {
                IWmiInstanceProvider provider = (IWmiInstanceProvider) bindingElement;
                instance = instance.NewInstance(provider.GetInstanceType());
                provider.FillInstance(instance);
            }
            else
            {
                System.Type serviceModelBaseType = AdministrationHelpers.GetServiceModelBaseType(bindingElement.GetType());
                if (null != serviceModelBaseType)
                {
                    instance = instance.NewInstance(serviceModelBaseType.Name);
                    if (bindingElement is TransportBindingElement)
                    {
                        TransportBindingElement element = (TransportBindingElement) bindingElement;
                        instance.SetProperty("ManualAddressing", element.ManualAddressing);
                        instance.SetProperty("MaxReceivedMessageSize", element.MaxReceivedMessageSize);
                        instance.SetProperty("MaxBufferPoolSize", element.MaxBufferPoolSize);
                        instance.SetProperty("Scheme", element.Scheme);
                        if (bindingElement is ConnectionOrientedTransportBindingElement)
                        {
                            ConnectionOrientedTransportBindingElement element2 = (ConnectionOrientedTransportBindingElement) bindingElement;
                            instance.SetProperty("ConnectionBufferSize", element2.ConnectionBufferSize);
                            instance.SetProperty("HostNameComparisonMode", element2.HostNameComparisonMode.ToString());
                            instance.SetProperty("ChannelInitializationTimeout", element2.ChannelInitializationTimeout);
                            instance.SetProperty("MaxBufferSize", element2.MaxBufferSize);
                            instance.SetProperty("MaxPendingConnections", element2.MaxPendingConnections);
                            instance.SetProperty("MaxOutputDelay", element2.MaxOutputDelay);
                            instance.SetProperty("MaxPendingAccepts", element2.MaxPendingAccepts);
                            instance.SetProperty("TransferMode", element2.TransferMode.ToString());
                            if (bindingElement is TcpTransportBindingElement)
                            {
                                TcpTransportBindingElement element3 = (TcpTransportBindingElement) bindingElement;
                                instance.SetProperty("ListenBacklog", element3.ListenBacklog);
                                instance.SetProperty("PortSharingEnabled", element3.PortSharingEnabled);
                                instance.SetProperty("TeredoEnabled", element3.TeredoEnabled);
                                IWmiInstance instance2 = instance.NewInstance("TcpConnectionPoolSettings");
                                instance2.SetProperty("GroupName", element3.ConnectionPoolSettings.GroupName);
                                instance2.SetProperty("IdleTimeout", element3.ConnectionPoolSettings.IdleTimeout);
                                instance2.SetProperty("LeaseTimeout", element3.ConnectionPoolSettings.LeaseTimeout);
                                instance2.SetProperty("MaxOutboundConnectionsPerEndpoint", element3.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);
                                instance.SetProperty("ConnectionPoolSettings", instance2);
                                FillExtendedProtectionPolicy(instance, element3.ExtendedProtectionPolicy);
                            }
                            else if (bindingElement is NamedPipeTransportBindingElement)
                            {
                                NamedPipeTransportBindingElement element4 = (NamedPipeTransportBindingElement) bindingElement;
                                IWmiInstance instance3 = instance.NewInstance("NamedPipeConnectionPoolSettings");
                                instance3.SetProperty("GroupName", element4.ConnectionPoolSettings.GroupName);
                                instance3.SetProperty("IdleTimeout", element4.ConnectionPoolSettings.IdleTimeout);
                                instance3.SetProperty("MaxOutboundConnectionsPerEndpoint", element4.ConnectionPoolSettings.MaxOutboundConnectionsPerEndpoint);
                                instance.SetProperty("ConnectionPoolSettings", instance3);
                            }
                        }
                        else if (!(bindingElement is HttpTransportBindingElement))
                        {
                            if (!(bindingElement is MsmqBindingElementBase))
                            {
                                if (bindingElement is PeerTransportBindingElement)
                                {
                                    PeerTransportBindingElement element9 = (PeerTransportBindingElement) bindingElement;
                                    instance.SetProperty("ListenIPAddress", element9.ListenIPAddress);
                                    instance.SetProperty("Port", element9.Port);
                                    IWmiInstance instance4 = instance.NewInstance("PeerSecuritySettings");
                                    instance4.SetProperty("Mode", element9.Security.Mode.ToString());
                                    IWmiInstance instance5 = instance4.NewInstance("PeerTransportSecuritySettings");
                                    instance5.SetProperty("CredentialType", element9.Security.Transport.CredentialType.ToString());
                                    instance4.SetProperty("Transport", instance5);
                                    instance.SetProperty("Security", instance4);
                                }
                            }
                            else
                            {
                                MsmqBindingElementBase base2 = (MsmqBindingElementBase) bindingElement;
                                if (null != base2.CustomDeadLetterQueue)
                                {
                                    instance.SetProperty("CustomDeadLetterQueue", base2.CustomDeadLetterQueue.AbsoluteUri.ToString());
                                }
                                instance.SetProperty("DeadLetterQueue", base2.DeadLetterQueue);
                                instance.SetProperty("Durable", base2.Durable);
                                instance.SetProperty("ExactlyOnce", base2.ExactlyOnce);
                                instance.SetProperty("MaxRetryCycles", base2.MaxRetryCycles);
                                instance.SetProperty("ReceiveContextEnabled", base2.ReceiveContextEnabled);
                                instance.SetProperty("ReceiveErrorHandling", base2.ReceiveErrorHandling);
                                instance.SetProperty("ReceiveRetryCount", base2.ReceiveRetryCount);
                                instance.SetProperty("RetryCycleDelay", base2.RetryCycleDelay);
                                instance.SetProperty("TimeToLive", base2.TimeToLive);
                                instance.SetProperty("UseSourceJournal", base2.UseSourceJournal);
                                instance.SetProperty("UseMsmqTracing", base2.UseMsmqTracing);
                                instance.SetProperty("ValidityDuration", base2.ValidityDuration);
                                MsmqTransportBindingElement element7 = base2 as MsmqTransportBindingElement;
                                if (element7 != null)
                                {
                                    instance.SetProperty("MaxPoolSize", element7.MaxPoolSize);
                                    instance.SetProperty("QueueTransferProtocol", element7.QueueTransferProtocol);
                                    instance.SetProperty("UseActiveDirectory", element7.UseActiveDirectory);
                                }
                                MsmqIntegrationBindingElement element8 = base2 as MsmqIntegrationBindingElement;
                                if (element8 != null)
                                {
                                    instance.SetProperty("SerializationFormat", element8.SerializationFormat.ToString());
                                }
                            }
                        }
                        else
                        {
                            HttpTransportBindingElement element5 = (HttpTransportBindingElement) bindingElement;
                            instance.SetProperty("AllowCookies", element5.AllowCookies);
                            instance.SetProperty("AuthenticationScheme", element5.AuthenticationScheme.ToString());
                            instance.SetProperty("BypassProxyOnLocal", element5.BypassProxyOnLocal);
                            instance.SetProperty("DecompressionEnabled", element5.DecompressionEnabled);
                            instance.SetProperty("HostNameComparisonMode", element5.HostNameComparisonMode.ToString());
                            instance.SetProperty("KeepAliveEnabled", element5.KeepAliveEnabled);
                            instance.SetProperty("MaxBufferSize", element5.MaxBufferSize);
                            if (null != element5.ProxyAddress)
                            {
                                instance.SetProperty("ProxyAddress", element5.ProxyAddress.AbsoluteUri.ToString());
                            }
                            instance.SetProperty("ProxyAuthenticationScheme", element5.ProxyAuthenticationScheme.ToString());
                            instance.SetProperty("Realm", element5.Realm);
                            instance.SetProperty("TransferMode", element5.TransferMode.ToString());
                            instance.SetProperty("UnsafeConnectionNtlmAuthentication", element5.UnsafeConnectionNtlmAuthentication);
                            instance.SetProperty("UseDefaultWebProxy", element5.UseDefaultWebProxy);
                            FillExtendedProtectionPolicy(instance, element5.ExtendedProtectionPolicy);
                            if (bindingElement is HttpsTransportBindingElement)
                            {
                                HttpsTransportBindingElement element6 = (HttpsTransportBindingElement) bindingElement;
                                instance.SetProperty("RequireClientCertificate", element6.RequireClientCertificate);
                            }
                        }
                    }
                    else if (bindingElement is PeerResolverBindingElement)
                    {
                        PeerResolverBindingElement element10 = (PeerResolverBindingElement) bindingElement;
                        instance.SetProperty("ReferralPolicy", element10.ReferralPolicy.ToString());
                        if (bindingElement is PeerCustomResolverBindingElement)
                        {
                            PeerCustomResolverBindingElement element11 = (PeerCustomResolverBindingElement) bindingElement;
                            if (element11.Address != null)
                            {
                                instance.SetProperty("Address", element11.Address.ToString());
                            }
                            if (element11.Binding != null)
                            {
                                instance.SetProperty("Binding", element11.Binding.ToString());
                            }
                        }
                    }
                    else if (bindingElement is ReliableSessionBindingElement)
                    {
                        ReliableSessionBindingElement element12 = (ReliableSessionBindingElement) bindingElement;
                        instance.SetProperty("AcknowledgementInterval", element12.AcknowledgementInterval);
                        instance.SetProperty("FlowControlEnabled", element12.FlowControlEnabled);
                        instance.SetProperty("InactivityTimeout", element12.InactivityTimeout);
                        instance.SetProperty("MaxPendingChannels", element12.MaxPendingChannels);
                        instance.SetProperty("MaxRetryCount", element12.MaxRetryCount);
                        instance.SetProperty("MaxTransferWindowSize", element12.MaxTransferWindowSize);
                        instance.SetProperty("Ordered", element12.Ordered);
                        instance.SetProperty("ReliableMessagingVersion", element12.ReliableMessagingVersion.ToString());
                    }
                    else if (bindingElement is SecurityBindingElement)
                    {
                        SecurityBindingElement element13 = (SecurityBindingElement) bindingElement;
                        instance.SetProperty("AllowInsecureTransport", element13.AllowInsecureTransport);
                        instance.SetProperty("DefaultAlgorithmSuite", element13.DefaultAlgorithmSuite.ToString());
                        instance.SetProperty("EnableUnsecuredResponse", element13.EnableUnsecuredResponse);
                        instance.SetProperty("IncludeTimestamp", element13.IncludeTimestamp);
                        instance.SetProperty("KeyEntropyMode", element13.KeyEntropyMode.ToString());
                        instance.SetProperty("SecurityHeaderLayout", element13.SecurityHeaderLayout.ToString());
                        instance.SetProperty("MessageSecurityVersion", element13.MessageSecurityVersion.ToString());
                        IWmiInstance instance6 = instance.NewInstance("LocalServiceSecuritySettings");
                        instance6.SetProperty("DetectReplays", element13.LocalServiceSettings.DetectReplays);
                        instance6.SetProperty("InactivityTimeout", element13.LocalServiceSettings.InactivityTimeout);
                        instance6.SetProperty("IssuedCookieLifetime", element13.LocalServiceSettings.IssuedCookieLifetime);
                        instance6.SetProperty("MaxCachedCookies", element13.LocalServiceSettings.MaxCachedCookies);
                        instance6.SetProperty("MaxClockSkew", element13.LocalServiceSettings.MaxClockSkew);
                        instance6.SetProperty("MaxPendingSessions", element13.LocalServiceSettings.MaxPendingSessions);
                        instance6.SetProperty("MaxStatefulNegotiations", element13.LocalServiceSettings.MaxStatefulNegotiations);
                        instance6.SetProperty("NegotiationTimeout", element13.LocalServiceSettings.NegotiationTimeout);
                        instance6.SetProperty("ReconnectTransportOnFailure", element13.LocalServiceSettings.ReconnectTransportOnFailure);
                        instance6.SetProperty("ReplayCacheSize", element13.LocalServiceSettings.ReplayCacheSize);
                        instance6.SetProperty("ReplayWindow", element13.LocalServiceSettings.ReplayWindow);
                        instance6.SetProperty("SessionKeyRenewalInterval", element13.LocalServiceSettings.SessionKeyRenewalInterval);
                        instance6.SetProperty("SessionKeyRolloverInterval", element13.LocalServiceSettings.SessionKeyRolloverInterval);
                        instance6.SetProperty("TimestampValidityDuration", element13.LocalServiceSettings.TimestampValidityDuration);
                        instance.SetProperty("LocalServiceSecuritySettings", instance6);
                        if (bindingElement is AsymmetricSecurityBindingElement)
                        {
                            AsymmetricSecurityBindingElement element14 = (AsymmetricSecurityBindingElement) bindingElement;
                            instance.SetProperty("MessageProtectionOrder", element14.MessageProtectionOrder.ToString());
                            instance.SetProperty("RequireSignatureConfirmation", element14.RequireSignatureConfirmation);
                        }
                        else if (bindingElement is SymmetricSecurityBindingElement)
                        {
                            SymmetricSecurityBindingElement element15 = (SymmetricSecurityBindingElement) bindingElement;
                            instance.SetProperty("MessageProtectionOrder", element15.MessageProtectionOrder.ToString());
                            instance.SetProperty("RequireSignatureConfirmation", element15.RequireSignatureConfirmation);
                        }
                    }
                    else if (bindingElement is WindowsStreamSecurityBindingElement)
                    {
                        WindowsStreamSecurityBindingElement element16 = (WindowsStreamSecurityBindingElement) bindingElement;
                        instance.SetProperty("ProtectionLevel", element16.ProtectionLevel.ToString());
                    }
                    else if (bindingElement is SslStreamSecurityBindingElement)
                    {
                        SslStreamSecurityBindingElement element17 = (SslStreamSecurityBindingElement) bindingElement;
                        instance.SetProperty("RequireClientCertificate", element17.RequireClientCertificate);
                    }
                    else if (bindingElement is CompositeDuplexBindingElement)
                    {
                        CompositeDuplexBindingElement element18 = (CompositeDuplexBindingElement) bindingElement;
                        if (element18.ClientBaseAddress != null)
                        {
                            instance.SetProperty("ClientBaseAddress", element18.ClientBaseAddress.AbsoluteUri);
                        }
                    }
                    else if (bindingElement is OneWayBindingElement)
                    {
                        OneWayBindingElement element19 = (OneWayBindingElement) bindingElement;
                        IWmiInstance instance7 = instance.NewInstance("ChannelPoolSettings");
                        instance7.SetProperty("IdleTimeout", element19.ChannelPoolSettings.IdleTimeout);
                        instance7.SetProperty("LeaseTimeout", element19.ChannelPoolSettings.LeaseTimeout);
                        instance7.SetProperty("MaxOutboundChannelsPerEndpoint", element19.ChannelPoolSettings.MaxOutboundChannelsPerEndpoint);
                        instance.SetProperty("ChannelPoolSettings", instance7);
                        instance.SetProperty("PacketRoutable", element19.PacketRoutable);
                        instance.SetProperty("MaxAcceptedChannels", element19.MaxAcceptedChannels);
                    }
                    else if (bindingElement is MessageEncodingBindingElement)
                    {
                        MessageEncodingBindingElement element20 = (MessageEncodingBindingElement) bindingElement;
                        instance.SetProperty("MessageVersion", element20.MessageVersion.ToString());
                        if (bindingElement is BinaryMessageEncodingBindingElement)
                        {
                            BinaryMessageEncodingBindingElement element21 = (BinaryMessageEncodingBindingElement) bindingElement;
                            instance.SetProperty("MaxSessionSize", element21.MaxSessionSize);
                            instance.SetProperty("MaxReadPoolSize", element21.MaxReadPoolSize);
                            instance.SetProperty("MaxWritePoolSize", element21.MaxWritePoolSize);
                            if (element21.ReaderQuotas != null)
                            {
                                FillReaderQuotas(instance, element21.ReaderQuotas);
                            }
                        }
                        else if (!(bindingElement is TextMessageEncodingBindingElement))
                        {
                            if (bindingElement is MtomMessageEncodingBindingElement)
                            {
                                MtomMessageEncodingBindingElement element23 = (MtomMessageEncodingBindingElement) bindingElement;
                                instance.SetProperty("Encoding", element23.WriteEncoding.WebName);
                                instance.SetProperty("MessageVersion", element23.MessageVersion.ToString());
                                instance.SetProperty("MaxReadPoolSize", element23.MaxReadPoolSize);
                                instance.SetProperty("MaxWritePoolSize", element23.MaxWritePoolSize);
                                if (element23.ReaderQuotas != null)
                                {
                                    FillReaderQuotas(instance, element23.ReaderQuotas);
                                }
                            }
                        }
                        else
                        {
                            TextMessageEncodingBindingElement element22 = (TextMessageEncodingBindingElement) bindingElement;
                            instance.SetProperty("Encoding", element22.WriteEncoding.WebName);
                            instance.SetProperty("MaxReadPoolSize", element22.MaxReadPoolSize);
                            instance.SetProperty("MaxWritePoolSize", element22.MaxWritePoolSize);
                            if (element22.ReaderQuotas != null)
                            {
                                FillReaderQuotas(instance, element22.ReaderQuotas);
                            }
                        }
                    }
                    else if (bindingElement is TransactionFlowBindingElement)
                    {
                        TransactionFlowBindingElement element24 = (TransactionFlowBindingElement) bindingElement;
                        instance.SetProperty("TransactionFlow", element24.Transactions);
                        instance.SetProperty("TransactionProtocol", element24.TransactionProtocol.ToString());
                        instance.SetProperty("AllowWildcardAction", element24.AllowWildcardAction);
                    }
                    else if (bindingElement is PrivacyNoticeBindingElement)
                    {
                        PrivacyNoticeBindingElement element25 = (PrivacyNoticeBindingElement) bindingElement;
                        instance.SetProperty("Url", element25.Url.ToString());
                        instance.SetProperty("PrivacyNoticeVersion", element25.Version);
                    }
                }
            }
        }

        private static void FillContractInfo(System.ServiceModel.Administration.EndpointInfo endpoint, IWmiInstance instance)
        {
            instance.SetProperty("Contract", ContractInstanceProvider.ContractReference(endpoint.Contract.Name));
        }

        internal static void FillEndpointInfo(System.ServiceModel.Administration.EndpointInfo endpoint, IWmiInstance instance)
        {
            instance.SetProperty("CounterInstanceName", PerformanceCounters.PerformanceCountersEnabled ? EndpointPerformanceCountersBase.CreateFriendlyInstanceName(endpoint.ServiceName, endpoint.Contract.Name, endpoint.Address.AbsoluteUri.ToUpperInvariant()) : string.Empty);
            instance.SetProperty("Name", endpoint.Name);
            instance.SetProperty("ContractName", endpoint.Contract.Name);
            FillAddressInfo(endpoint, instance);
            FillContractInfo(endpoint, instance);
            FillBindingInfo(endpoint, instance);
            FillBehaviorsInfo(endpoint, instance);
        }

        private static void FillExtendedProtectionPolicy(IWmiInstance instance, ExtendedProtectionPolicy policy)
        {
            IWmiInstance instance2 = instance.NewInstance("ExtendedProtectionPolicy");
            instance2.SetProperty("PolicyEnforcement", policy.PolicyEnforcement.ToString());
            instance2.SetProperty("ProtectionScenario", policy.ProtectionScenario.ToString());
            if (policy.CustomServiceNames != null)
            {
                List<string> list = new List<string>(policy.CustomServiceNames.Count);
                foreach (string str in policy.CustomServiceNames)
                {
                    list.Add(str);
                }
                instance2.SetProperty("CustomServiceNames", list.ToArray());
            }
            if (policy.CustomChannelBinding != null)
            {
                instance2.SetProperty("CustomChannelBinding", policy.CustomChannelBinding.GetType().ToString());
            }
            instance.SetProperty("ExtendedProtectionPolicy", instance2);
        }

        private static void FillReaderQuotas(IWmiInstance instance, XmlDictionaryReaderQuotas readerQuotas)
        {
            IWmiInstance instance2 = instance.NewInstance("XmlDictionaryReaderQuotas");
            instance2.SetProperty("MaxArrayLength", readerQuotas.MaxArrayLength);
            instance2.SetProperty("MaxBytesPerRead", readerQuotas.MaxBytesPerRead);
            instance2.SetProperty("MaxDepth", readerQuotas.MaxDepth);
            instance2.SetProperty("MaxNameTableCharCount", readerQuotas.MaxNameTableCharCount);
            instance2.SetProperty("MaxStringContentLength", readerQuotas.MaxStringContentLength);
            instance.SetProperty("ReaderQuotas", instance2);
        }

        private System.ServiceModel.Administration.EndpointInfo FindEndpoint(string address, string contractName)
        {
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (System.ServiceModel.Administration.EndpointInfo info2 in info.Endpoints)
                {
                    if ((((null != info2.ListenUri) && string.Equals(info2.ListenUri.ToString(), address, StringComparison.OrdinalIgnoreCase)) && ((info2.Contract != null) && (info2.Contract.Name != null))) && (string.CompareOrdinal(info2.Contract.Name, contractName) == 0))
                    {
                        return info2;
                    }
                }
            }
            return null;
        }

        private string GetOperationCounterInstanceName(string operationName, IWmiInstance endpointInstance)
        {
            string property = (string) endpointInstance.GetProperty("ListenUri");
            string contractName = (string) endpointInstance.GetProperty("ContractName");
            System.ServiceModel.Administration.EndpointInfo info = this.FindEndpoint(property, contractName);
            string str3 = string.Empty;
            if (PerformanceCounters.PerformanceCountersEnabled && (info != null))
            {
                str3 = OperationPerformanceCountersBase.CreateFriendlyInstanceName(info.ServiceName, info.Contract.Name, operationName, info.Address.AbsoluteUri.ToUpperInvariant());
            }
            return str3;
        }

        private bool OwnInstance(IWmiInstance instance)
        {
            return ((((int) instance.GetProperty("ProcessId")) == AppDomainInfo.Current.ProcessId) && (((int) instance.GetProperty("AppDomainId")) == AppDomainInfo.Current.Id));
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            int processId = AppDomainInfo.Current.ProcessId;
            int id = AppDomainInfo.Current.Id;
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                foreach (System.ServiceModel.Administration.EndpointInfo info2 in info.Endpoints)
                {
                    IWmiInstance instance = instances.NewInstance(null);
                    instance.SetProperty("ProcessId", processId);
                    instance.SetProperty("AppDomainId", id);
                    FillEndpointInfo(info2, instance);
                    instances.AddInstance(instance);
                }
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            bool flag = false;
            if (this.OwnInstance(instance))
            {
                string property = (string) instance.GetProperty("ListenUri");
                string contractName = (string) instance.GetProperty("ContractName");
                System.ServiceModel.Administration.EndpointInfo endpoint = this.FindEndpoint(property, contractName);
                if (endpoint != null)
                {
                    FillEndpointInfo(endpoint, instance);
                    flag = true;
                }
            }
            return flag;
        }

        bool IWmiProvider.InvokeMethod(IWmiMethodContext method)
        {
            bool flag = this.OwnInstance(method.Instance);
            if (flag)
            {
                if (!(method.MethodName == "GetOperationCounterInstanceName"))
                {
                    throw new WbemInvalidMethodException();
                }
                string parameter = method.GetParameter("Operation") as string;
                if (string.IsNullOrEmpty(parameter))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WbemInvalidParameterException("Operation"));
                }
                string operationCounterInstanceName = this.GetOperationCounterInstanceName(parameter, method.Instance);
                method.ReturnParameter = operationCounterInstanceName;
            }
            return flag;
        }
    }
}

