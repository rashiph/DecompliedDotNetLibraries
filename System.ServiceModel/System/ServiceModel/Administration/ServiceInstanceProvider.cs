namespace System.ServiceModel.Administration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal class ServiceInstanceProvider : ProviderBase, IWmiProvider
    {
        private void FillBehaviorInfo(IServiceBehavior behavior, IWmiInstance existingInstance, out IWmiInstance instance)
        {
            instance = null;
            if (behavior is AspNetCompatibilityRequirementsAttribute)
            {
                instance = existingInstance.NewInstance("AspNetCompatibilityRequirementsAttribute");
                AspNetCompatibilityRequirementsAttribute attribute = (AspNetCompatibilityRequirementsAttribute) behavior;
                instance.SetProperty("RequirementsMode", attribute.RequirementsMode.ToString());
            }
            else if (behavior is ServiceCredentials)
            {
                instance = existingInstance.NewInstance("ServiceCredentials");
                ServiceCredentials credentials = (ServiceCredentials) behavior;
                if ((credentials.ClientCertificate != null) && (credentials.ClientCertificate.Certificate != null))
                {
                    string str = string.Empty + string.Format(CultureInfo.InvariantCulture, "Certificate: {0}\n", new object[] { credentials.ClientCertificate.Certificate });
                    instance.SetProperty("ClientCertificate", str);
                }
                if ((credentials.IssuedTokenAuthentication != null) && (credentials.IssuedTokenAuthentication.KnownCertificates != null))
                {
                    string str2 = (string.Empty + string.Format(CultureInfo.InvariantCulture, "AllowUntrustedRsaIssuers: {0}\n", new object[] { credentials.IssuedTokenAuthentication.AllowUntrustedRsaIssuers }) + string.Format(CultureInfo.InvariantCulture, "CertificateValidationMode: {0}\n", new object[] { credentials.IssuedTokenAuthentication.CertificateValidationMode })) + string.Format(CultureInfo.InvariantCulture, "RevocationMode: {0}\n", new object[] { credentials.IssuedTokenAuthentication.RevocationMode }) + string.Format(CultureInfo.InvariantCulture, "TrustedStoreLocation: {0}\n", new object[] { credentials.IssuedTokenAuthentication.TrustedStoreLocation });
                    foreach (X509Certificate2 certificate in credentials.IssuedTokenAuthentication.KnownCertificates)
                    {
                        if (certificate != null)
                        {
                            str2 = str2 + string.Format(CultureInfo.InvariantCulture, "Known certificate: {0}\n", new object[] { certificate.FriendlyName });
                        }
                    }
                    str2 = str2 + string.Format(CultureInfo.InvariantCulture, "AudienceUriMode: {0}\n", new object[] { credentials.IssuedTokenAuthentication.AudienceUriMode });
                    if (credentials.IssuedTokenAuthentication.AllowedAudienceUris != null)
                    {
                        foreach (string str3 in credentials.IssuedTokenAuthentication.AllowedAudienceUris)
                        {
                            if (str3 != null)
                            {
                                str2 = str2 + string.Format(CultureInfo.InvariantCulture, "Allowed Uri: {0}\n", new object[] { str3 });
                            }
                        }
                    }
                    instance.SetProperty("IssuedTokenAuthentication", str2);
                }
                if ((credentials.Peer != null) && (credentials.Peer.Certificate != null))
                {
                    string str4 = string.Empty + string.Format(CultureInfo.InvariantCulture, "Certificate: {0}\n", new object[] { credentials.Peer.Certificate.ToString(true) });
                    instance.SetProperty("Peer", str4);
                }
                if ((credentials.SecureConversationAuthentication != null) && (credentials.SecureConversationAuthentication.SecurityContextClaimTypes != null))
                {
                    string str5 = string.Empty;
                    foreach (System.Type type in credentials.SecureConversationAuthentication.SecurityContextClaimTypes)
                    {
                        if (type != null)
                        {
                            str5 = str5 + string.Format(CultureInfo.InvariantCulture, "ClaimType: {0}\n", new object[] { type });
                        }
                    }
                    instance.SetProperty("SecureConversationAuthentication", str5);
                }
                if ((credentials.ServiceCertificate != null) && (credentials.ServiceCertificate.Certificate != null))
                {
                    instance.SetProperty("ServiceCertificate", credentials.ServiceCertificate.Certificate.ToString());
                }
                if (credentials.UserNameAuthentication != null)
                {
                    instance.SetProperty("UserNameAuthentication", string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { "ValidationMode", credentials.UserNameAuthentication.UserNamePasswordValidationMode.ToString() }));
                }
                if (credentials.WindowsAuthentication != null)
                {
                    instance.SetProperty("WindowsAuthentication", string.Format(CultureInfo.InvariantCulture, "{0}: {1}", new object[] { "AllowAnonymous", credentials.WindowsAuthentication.AllowAnonymousLogons.ToString() }));
                }
            }
            else if (behavior is ServiceAuthorizationBehavior)
            {
                instance = existingInstance.NewInstance("ServiceAuthorizationBehavior");
                ServiceAuthorizationBehavior behavior2 = (ServiceAuthorizationBehavior) behavior;
                instance.SetProperty("ImpersonateCallerForAllOperations", behavior2.ImpersonateCallerForAllOperations);
                if (behavior2.RoleProvider != null)
                {
                    instance.SetProperty("RoleProvider", behavior2.RoleProvider.ToString());
                }
                if (behavior2.ServiceAuthorizationManager != null)
                {
                    instance.SetProperty("ServiceAuthorizationManager", behavior2.ServiceAuthorizationManager.ToString());
                }
                instance.SetProperty("PrincipalPermissionMode", behavior2.PrincipalPermissionMode.ToString());
            }
            else if (behavior is ServiceSecurityAuditBehavior)
            {
                instance = existingInstance.NewInstance("ServiceSecurityAuditBehavior");
                ServiceSecurityAuditBehavior behavior3 = (ServiceSecurityAuditBehavior) behavior;
                instance.SetProperty("AuditLogLocation", behavior3.AuditLogLocation.ToString());
                instance.SetProperty("SuppressAuditFailure", behavior3.SuppressAuditFailure);
                instance.SetProperty("ServiceAuthorizationAuditLevel", behavior3.ServiceAuthorizationAuditLevel.ToString());
                instance.SetProperty("MessageAuthenticationAuditLevel", behavior3.MessageAuthenticationAuditLevel.ToString());
            }
            else if (behavior is ServiceBehaviorAttribute)
            {
                instance = existingInstance.NewInstance("ServiceBehaviorAttribute");
                ServiceBehaviorAttribute attribute2 = (ServiceBehaviorAttribute) behavior;
                instance.SetProperty("AddressFilterMode", attribute2.AddressFilterMode.ToString());
                instance.SetProperty("AutomaticSessionShutdown", attribute2.AutomaticSessionShutdown);
                instance.SetProperty("ConcurrencyMode", attribute2.ConcurrencyMode.ToString());
                instance.SetProperty("ConfigurationName", attribute2.ConfigurationName);
                instance.SetProperty("IgnoreExtensionDataObject", attribute2.IgnoreExtensionDataObject);
                instance.SetProperty("IncludeExceptionDetailInFaults", attribute2.IncludeExceptionDetailInFaults);
                instance.SetProperty("InstanceContextMode", attribute2.InstanceContextMode.ToString());
                instance.SetProperty("MaxItemsInObjectGraph", attribute2.MaxItemsInObjectGraph);
                instance.SetProperty("Name", attribute2.Name);
                instance.SetProperty("Namespace", attribute2.Namespace);
                instance.SetProperty("ReleaseServiceInstanceOnTransactionComplete", attribute2.ReleaseServiceInstanceOnTransactionComplete);
                instance.SetProperty("TransactionAutoCompleteOnSessionClose", attribute2.TransactionAutoCompleteOnSessionClose);
                instance.SetProperty("TransactionIsolationLevel", attribute2.TransactionIsolationLevel.ToString());
                if (attribute2.TransactionTimeoutSet)
                {
                    instance.SetProperty("TransactionTimeout", attribute2.TransactionTimeoutTimespan);
                }
                instance.SetProperty("UseSynchronizationContext", attribute2.UseSynchronizationContext);
                instance.SetProperty("ValidateMustUnderstand", attribute2.ValidateMustUnderstand);
            }
            else if (behavior is ServiceDebugBehavior)
            {
                instance = existingInstance.NewInstance("ServiceDebugBehavior");
                ServiceDebugBehavior behavior4 = (ServiceDebugBehavior) behavior;
                if (null != behavior4.HttpHelpPageUrl)
                {
                    instance.SetProperty("HttpHelpPageUrl", behavior4.HttpHelpPageUrl.ToString());
                }
                instance.SetProperty("HttpHelpPageEnabled", behavior4.HttpHelpPageEnabled);
                if (null != behavior4.HttpsHelpPageUrl)
                {
                    instance.SetProperty("HttpsHelpPageUrl", behavior4.HttpsHelpPageUrl.ToString());
                }
                instance.SetProperty("HttpsHelpPageEnabled", behavior4.HttpsHelpPageEnabled);
                instance.SetProperty("IncludeExceptionDetailInFaults", behavior4.IncludeExceptionDetailInFaults);
            }
            else if (behavior is ServiceMetadataBehavior)
            {
                instance = existingInstance.NewInstance("ServiceMetadataBehavior");
                ServiceMetadataBehavior behavior5 = (ServiceMetadataBehavior) behavior;
                if (null != behavior5.ExternalMetadataLocation)
                {
                    instance.SetProperty("ExternalMetadataLocation", behavior5.ExternalMetadataLocation.ToString());
                }
                instance.SetProperty("HttpGetEnabled", behavior5.HttpGetEnabled);
                if (null != behavior5.HttpGetUrl)
                {
                    instance.SetProperty("HttpGetUrl", behavior5.HttpGetUrl.ToString());
                }
                instance.SetProperty("HttpsGetEnabled", behavior5.HttpsGetEnabled);
                if (null != behavior5.HttpsGetUrl)
                {
                    instance.SetProperty("HttpsGetUrl", behavior5.HttpsGetUrl.ToString());
                }
                this.FillMetadataExporterInfo(instance, behavior5.MetadataExporter);
            }
            else if (behavior is ServiceThrottlingBehavior)
            {
                instance = existingInstance.NewInstance("ServiceThrottlingBehavior");
                ServiceThrottlingBehavior behavior6 = (ServiceThrottlingBehavior) behavior;
                instance.SetProperty("MaxConcurrentCalls", behavior6.MaxConcurrentCalls);
                instance.SetProperty("MaxConcurrentSessions", behavior6.MaxConcurrentSessions);
                instance.SetProperty("MaxConcurrentInstances", behavior6.MaxConcurrentInstances);
            }
            else if (behavior is ServiceTimeoutsBehavior)
            {
                instance = existingInstance.NewInstance("ServiceTimeoutsBehavior");
                ServiceTimeoutsBehavior behavior7 = (ServiceTimeoutsBehavior) behavior;
                instance.SetProperty("TransactionTimeout", behavior7.TransactionTimeout);
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

        private void FillBehaviorsInfo(ServiceInfo info, IWmiInstance instance)
        {
            List<IWmiInstance> list = new List<IWmiInstance>(info.Behaviors.Count);
            foreach (IServiceBehavior behavior in info.Behaviors)
            {
                IWmiInstance instance2;
                this.FillBehaviorInfo(behavior, instance, out instance2);
                if (instance2 != null)
                {
                    list.Add(instance2);
                }
            }
            instance.SetProperty("Behaviors", list.ToArray());
        }

        private void FillChannelInfo(IChannel channel, IWmiInstance instance)
        {
            instance.SetProperty("Type", channel.GetType().ToString());
            ServiceChannel serviceChannel = ServiceChannelFactory.GetServiceChannel(channel);
            if (serviceChannel != null)
            {
                string str = (serviceChannel.RemoteAddress == null) ? string.Empty : serviceChannel.RemoteAddress.ToString();
                instance.SetProperty("RemoteAddress", str);
                string contractName = (serviceChannel.ClientRuntime != null) ? serviceChannel.ClientRuntime.ContractName : string.Empty;
                string str3 = EndpointInstanceProvider.EndpointReference(str, contractName, false);
                instance.SetProperty("RemoteEndpoint", str3);
                instance.SetProperty("LocalAddress", (serviceChannel.LocalAddress == null) ? string.Empty : serviceChannel.LocalAddress.ToString());
                instance.SetProperty("SessionId", ((IContextChannel) serviceChannel).SessionId);
            }
        }

        private void FillChannelsInfo(ServiceInfo info, IWmiInstance instance)
        {
            int num = 0;
            List<IWmiInstance> list = new List<IWmiInstance>();
            foreach (System.ServiceModel.InstanceContext context in info.Service.GetInstanceContexts())
            {
                lock (context.ThisLock)
                {
                    num += context.WmiChannels.Count;
                    foreach (IChannel channel in context.WmiChannels)
                    {
                        IWmiInstance instance2 = instance.NewInstance("Channel");
                        this.FillChannelInfo(channel, instance2);
                        list.Add(instance2);
                    }
                }
            }
            instance.SetProperty("OutgoingChannels", list.ToArray());
        }

        private static void FillExtensionsInfo(ServiceInfo info, IWmiInstance instance)
        {
            ProviderBase.FillCollectionInfo(info.Service.Extensions, instance, "Extensions");
        }

        private void FillMetadataExporterInfo(IWmiInstance instance, MetadataExporter exporter)
        {
            DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.Wmi, (EventLogEventId) (-1073610744), true, new string[] { "metadata exporter called" });
            IWmiInstance instance2 = instance.NewInstance("MetadataExporter");
            instance2.SetProperty("PolicyVersion", exporter.PolicyVersion.ToString());
            instance.SetProperty("MetadataExportInfo", instance2);
        }

        private void FillServiceInfo(ServiceInfo info, IWmiInstance instance)
        {
            ProviderBase.FillCollectionInfo((ICollection) info.Service.BaseAddresses, instance, "BaseAddresses");
            instance.SetProperty("CounterInstanceName", PerformanceCounters.PerformanceCountersEnabled ? ServicePerformanceCountersBase.CreateFriendlyInstanceName(info.Service) : string.Empty);
            instance.SetProperty("ConfigurationName", info.ConfigurationName);
            instance.SetProperty("DistinguishedName", info.DistinguishedName);
            instance.SetProperty("Name", info.Name);
            instance.SetProperty("Namespace", info.Namespace);
            instance.SetProperty("Metadata", info.Metadata);
            instance.SetProperty("Opened", ManagementExtension.GetTimeOpened(info.Service));
            this.FillBehaviorsInfo(info, instance);
            FillExtensionsInfo(info, instance);
            this.FillChannelsInfo(info, instance);
        }

        internal static IWmiInstance GetAppDomainInfo(IWmiInstance instance)
        {
            IWmiInstance instance2 = instance.NewInstance("AppDomainInfo");
            if (instance2 != null)
            {
                AppDomainInstanceProvider.FillAppDomainInfo(instance2);
            }
            return instance2;
        }

        internal static string GetReference(ServiceInfo serviceInfo)
        {
            return string.Format(CultureInfo.InvariantCulture, "Service.DistinguishedName='{0}',ProcessId={1}", new object[] { serviceInfo.DistinguishedName, AppDomainInfo.Current.ProcessId });
        }

        void IWmiProvider.EnumInstances(IWmiInstances instances)
        {
            int processId = AppDomainInfo.Current.ProcessId;
            foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
            {
                IWmiInstance instance = instances.NewInstance(null);
                instance.SetProperty("DistinguishedName", info.DistinguishedName);
                instance.SetProperty("ProcessId", processId);
                this.FillServiceInfo(info, instance);
                instances.AddInstance(instance);
            }
        }

        bool IWmiProvider.GetInstance(IWmiInstance instance)
        {
            if (((int) instance.GetProperty("ProcessId")) == AppDomainInfo.Current.ProcessId)
            {
                foreach (ServiceInfo info in new ServiceInfoCollection(ManagementExtension.Services))
                {
                    if (string.Equals((string) instance.GetProperty("DistinguishedName"), info.DistinguishedName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.FillServiceInfo(info, instance);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

