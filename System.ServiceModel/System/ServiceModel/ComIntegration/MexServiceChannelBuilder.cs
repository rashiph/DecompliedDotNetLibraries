namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Web.Services.Description;
    using System.Web.Services.Discovery;
    using System.Xml;
    using System.Xml.Schema;

    internal class MexServiceChannelBuilder : IProxyCreator, IDisposable, IProvideChannelBuilderSettings
    {
        private KeyedByTypeCollection<IEndpointBehavior> behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
        private ContractDescription contractDescription;
        private Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable;
        private ServiceChannel serviceChannel;
        private ServiceChannelFactory serviceChannelFactory;
        private ServiceEndpoint serviceEndpoint;
        private bool useXmlSerializer;

        internal MexServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            this.propertyTable = propertyTable;
            this.DoMex();
        }

        private void AddDocumentToSet(MetadataSet metadataSet, object document)
        {
            System.Web.Services.Description.ServiceDescription serviceDescription = document as System.Web.Services.Description.ServiceDescription;
            System.Xml.Schema.XmlSchema schema = document as System.Xml.Schema.XmlSchema;
            XmlElement policy = document as XmlElement;
            if (serviceDescription != null)
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromServiceDescription(serviceDescription));
            }
            else if (schema != null)
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromSchema(schema));
            }
            else if ((policy != null) && MetadataSection.IsPolicyElement(policy))
            {
                metadataSet.MetadataSections.Add(MetadataSection.CreateFromPolicy(policy, null));
            }
            else
            {
                MetadataSection item = new MetadataSection {
                    Metadata = document
                };
                metadataSet.MetadataSections.Add(item);
            }
        }

        private ServiceChannel CreateChannel()
        {
            if (this.serviceChannel == null)
            {
                lock (this)
                {
                    if (this.serviceChannel == null)
                    {
                        try
                        {
                            if (this.serviceChannelFactory == null)
                            {
                                this.FaultInserviceChannelFactory();
                            }
                            if (this.serviceChannelFactory == null)
                            {
                                throw Fx.AssertAndThrow("ServiceChannelFactory cannot be null at this point");
                            }
                            this.serviceChannelFactory.Open();
                            if (this.serviceEndpoint == null)
                            {
                                throw Fx.AssertAndThrow("ServiceEndpoint cannot be null");
                            }
                            ServiceChannel channel = this.serviceChannelFactory.CreateServiceChannel(new EndpointAddress(this.serviceEndpoint.Address.Uri, this.serviceEndpoint.Address.Identity, this.serviceEndpoint.Address.Headers), this.serviceEndpoint.Address.Uri);
                            Thread.MemoryBarrier();
                            this.serviceChannel = channel;
                            ComPlusChannelCreatedTrace.Trace(TraceEventType.Verbose, 0x5001f, "TraceCodeComIntegrationChannelCreated", this.serviceEndpoint.Address.Uri, this.contractDescription.ContractType);
                            if (this.serviceChannel == null)
                            {
                                throw Fx.AssertAndThrow("serviceProxy MUST derive from RealProxy");
                            }
                        }
                        finally
                        {
                            if ((this.serviceChannel == null) && (this.serviceChannelFactory != null))
                            {
                                this.serviceChannelFactory.Close();
                            }
                        }
                    }
                }
            }
            return this.serviceChannel;
        }

        public WsdlImporter CreateDataContractSerializerImporter(MetadataSet metaData)
        {
            Collection<IWsdlImportExtension> wsdlImportExtensions = ClientSection.GetSection().Metadata.LoadWsdlImportExtensions();
            for (int i = 0; i < wsdlImportExtensions.Count; i++)
            {
                if (wsdlImportExtensions[i].GetType() == typeof(XmlSerializerMessageContractImporter))
                {
                    wsdlImportExtensions.RemoveAt(i);
                }
            }
            return new WsdlImporter(metaData, null, wsdlImportExtensions);
        }

        private ServiceChannelFactory CreateServiceChannelFactory()
        {
            this.serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(this.serviceEndpoint);
            if (this.serviceChannelFactory == null)
            {
                throw Fx.AssertAndThrow("We should get a ServiceChannelFactory back");
            }
            this.FixupProxyBehavior();
            return this.serviceChannelFactory;
        }

        public WsdlImporter CreateXmlSerializerImporter(MetadataSet metaData)
        {
            Collection<IWsdlImportExtension> wsdlImportExtensions = ClientSection.GetSection().Metadata.LoadWsdlImportExtensions();
            for (int i = 0; i < wsdlImportExtensions.Count; i++)
            {
                if (wsdlImportExtensions[i].GetType() == typeof(DataContractSerializerMessageContractImporter))
                {
                    wsdlImportExtensions.RemoveAt(i);
                }
            }
            return new WsdlImporter(metaData, null, wsdlImportExtensions);
        }

        private void DoMex()
        {
            string str;
            string str2;
            string str3;
            string str4;
            string str5;
            string str6;
            string str7;
            string str8;
            string str9 = null;
            string str10 = null;
            string str11 = null;
            string str12 = null;
            string str13 = null;
            string str14 = null;
            string str15 = null;
            EndpointIdentity identity = null;
            EndpointIdentity identity2 = null;
            WsdlImporter importer;
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out str4);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out str5);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out str7);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out str6);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexAddress, out str);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBinding, out str2);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexBindingConfiguration, out str3);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out str8);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out str9);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out str10);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out str11);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexSpnIdentity, out str12);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexUpnIdentity, out str13);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.MexDnsIdentity, out str14);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Serializer, out str15);
            if (string.IsNullOrEmpty(str))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerMexAddressNotSpecified")));
            }
            if (!string.IsNullOrEmpty(str12))
            {
                if (!string.IsNullOrEmpty(str13) || !string.IsNullOrEmpty(str14))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentityForMex")));
                }
                identity2 = EndpointIdentity.CreateSpnIdentity(str12);
            }
            else if (!string.IsNullOrEmpty(str13))
            {
                if (!string.IsNullOrEmpty(str12) || !string.IsNullOrEmpty(str14))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentityForMex")));
                }
                identity2 = EndpointIdentity.CreateUpnIdentity(str13);
            }
            else if (!string.IsNullOrEmpty(str14))
            {
                if (!string.IsNullOrEmpty(str12) || !string.IsNullOrEmpty(str13))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentityForMex")));
                }
                identity2 = EndpointIdentity.CreateDnsIdentity(str14);
            }
            else
            {
                identity2 = null;
            }
            if (string.IsNullOrEmpty(str8))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerAddressNotSpecified")));
            }
            if (string.IsNullOrEmpty(str4))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerContractNotSpecified")));
            }
            if (string.IsNullOrEmpty(str6))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerBindingNotSpecified")));
            }
            if (string.IsNullOrEmpty(str7))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerBindingNamespacetNotSpecified")));
            }
            if (!string.IsNullOrEmpty(str9))
            {
                if (!string.IsNullOrEmpty(str10) || !string.IsNullOrEmpty(str11))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateSpnIdentity(str9);
            }
            else if (!string.IsNullOrEmpty(str10))
            {
                if (!string.IsNullOrEmpty(str9) || !string.IsNullOrEmpty(str11))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateUpnIdentity(str10);
            }
            else if (!string.IsNullOrEmpty(str11))
            {
                if (!string.IsNullOrEmpty(str9) || !string.IsNullOrEmpty(str10))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateDnsIdentity(str11);
            }
            else
            {
                identity = null;
            }
            MetadataExchangeClient client = null;
            EndpointAddress address = new EndpointAddress(new Uri(str), identity2, new AddressHeader[0]);
            if (!string.IsNullOrEmpty(str2))
            {
                System.ServiceModel.Channels.Binding mexBinding = null;
                try
                {
                    mexBinding = ConfigLoader.LookupBinding(str2, str3);
                }
                catch (ConfigurationErrorsException)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MexBindingNotFoundInConfig", new object[] { str2 })));
                }
                if (mexBinding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MexBindingNotFoundInConfig", new object[] { str2 })));
                }
                client = new MetadataExchangeClient(mexBinding);
            }
            else
            {
                if (!string.IsNullOrEmpty(str3))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerMexBindingSectionNameNotSpecified")));
                }
                client = new MetadataExchangeClient(address);
            }
            if (identity2 != null)
            {
                client.SoapCredentials.Windows.AllowNtlm = false;
            }
            bool flag = false;
            if (!string.IsNullOrEmpty(str15))
            {
                if (("xml" != str15) && ("datacontract" != str15))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorectSerializer")));
                }
                if ("xml" == str15)
                {
                    this.useXmlSerializer = true;
                }
                else
                {
                    flag = true;
                }
            }
            ServiceEndpoint endpoint = null;
            ServiceEndpointCollection serviceEndpointsRetrieved = null;
            try
            {
                MetadataSet metadata = client.GetMetadata(address);
                if (this.useXmlSerializer)
                {
                    importer = this.CreateXmlSerializerImporter(metadata);
                }
                else if (flag)
                {
                    importer = this.CreateDataContractSerializerImporter(metadata);
                }
                else
                {
                    importer = new WsdlImporter(metadata);
                }
                serviceEndpointsRetrieved = this.ImportWsdlPortType(new XmlQualifiedName(str4, str5), importer);
                ComPlusMexChannelBuilderMexCompleteTrace.Trace(TraceEventType.Verbose, 0x50024, "TraceCodeComIntegrationMexMonikerMetadataExchangeComplete", serviceEndpointsRetrieved);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (UriSchemeSupportsDisco(address.Uri))
                {
                    try
                    {
                        DiscoveryClientProtocol protocol = new DiscoveryClientProtocol {
                            UseDefaultCredentials = true,
                            AllowAutoRedirect = true
                        };
                        protocol.DiscoverAny(address.Uri.AbsoluteUri);
                        protocol.ResolveAll();
                        MetadataSet metadataSet = new MetadataSet();
                        foreach (object obj2 in protocol.Documents.Values)
                        {
                            this.AddDocumentToSet(metadataSet, obj2);
                        }
                        if (this.useXmlSerializer)
                        {
                            importer = this.CreateXmlSerializerImporter(metadataSet);
                        }
                        else if (flag)
                        {
                            importer = this.CreateDataContractSerializerImporter(metadataSet);
                        }
                        else
                        {
                            importer = new WsdlImporter(metadataSet);
                        }
                        serviceEndpointsRetrieved = this.ImportWsdlPortType(new XmlQualifiedName(str4, str5), importer);
                        ComPlusMexChannelBuilderMexCompleteTrace.Trace(TraceEventType.Verbose, 0x50024, "TraceCodeComIntegrationMexMonikerMetadataExchangeComplete", serviceEndpointsRetrieved);
                        goto Label_0634;
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerFailedToDoMexRetrieve", new object[] { exception2.Message })));
                    }
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerFailedToDoMexRetrieve", new object[] { exception.Message })));
            }
        Label_0634:
            if (serviceEndpointsRetrieved.Count == 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerContractNotFoundInRetreivedMex")));
            }
            foreach (ServiceEndpoint endpoint2 in serviceEndpointsRetrieved)
            {
                System.ServiceModel.Channels.Binding binding = endpoint2.Binding;
                if ((binding.Name == str6) && (binding.Namespace == str7))
                {
                    endpoint = endpoint2;
                    break;
                }
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerNoneOfTheBindingMatchedTheSpecifiedBinding")));
            }
            this.contractDescription = endpoint.Contract;
            this.serviceEndpoint = new ServiceEndpoint(this.contractDescription, endpoint.Binding, new EndpointAddress(new Uri(str8), identity, null));
            ComPlusMexChannelBuilderTrace.Trace(TraceEventType.Verbose, 0x50025, "TraceCodeComIntegrationMexChannelBuilderLoaded", endpoint.Contract, endpoint.Binding, str8);
        }

        private void FaultInserviceChannelFactory()
        {
            if (this.propertyTable == null)
            {
                throw Fx.AssertAndThrow("PropertyTable should not be null");
            }
            foreach (IEndpointBehavior behavior in this.behaviors)
            {
                this.serviceEndpoint.Behaviors.Add(behavior);
            }
            this.serviceChannelFactory = this.CreateServiceChannelFactory();
        }

        private void FixupProxyBehavior()
        {
            ClientOperation operation = null;
            if (this.useXmlSerializer)
            {
                XmlSerializerOperationBehavior.AddBehaviors(this.contractDescription);
            }
            foreach (OperationDescription description in this.contractDescription.Operations)
            {
                operation = this.serviceChannelFactory.ClientRuntime.Operations[description.Name];
                operation.SerializeRequest = true;
                operation.DeserializeReply = true;
                if (this.useXmlSerializer)
                {
                    operation.Formatter = XmlSerializerOperationBehavior.CreateOperationFormatter(description);
                }
                else
                {
                    operation.Formatter = new DataContractSerializerOperationFormatter(description, TypeLoader.DefaultDataContractFormatAttribute, null);
                }
            }
        }

        private ServiceEndpointCollection ImportWsdlPortType(XmlQualifiedName portTypeQName, WsdlImporter importer)
        {
            foreach (System.Web.Services.Description.ServiceDescription description in importer.WsdlDocuments)
            {
                if (description.TargetNamespace == portTypeQName.Namespace)
                {
                    PortType wsdlPortType = description.PortTypes[portTypeQName.Name];
                    if (wsdlPortType != null)
                    {
                        return importer.ImportEndpoints(wsdlPortType);
                    }
                }
            }
            return new ServiceEndpointCollection();
        }

        void IDisposable.Dispose()
        {
            if (this.serviceChannel != null)
            {
                this.serviceChannel.Close();
            }
        }

        ComProxy IProxyCreator.CreateProxy(IntPtr outer, ref Guid riid)
        {
            if (riid != InterfaceID.idIDispatch)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidCastException(System.ServiceModel.SR.GetString("NoInterface", new object[] { (Guid) riid })));
            }
            if (this.contractDescription == null)
            {
                throw Fx.AssertAndThrow("ContractDescription should not be null at this point");
            }
            return DispatchProxy.Create(outer, this.contractDescription, this);
        }

        bool IProxyCreator.SupportsDispatch()
        {
            return true;
        }

        bool IProxyCreator.SupportsErrorInfo(ref Guid riid)
        {
            if (riid != InterfaceID.idIDispatch)
            {
                return false;
            }
            return true;
        }

        bool IProxyCreator.SupportsIntrinsics()
        {
            return true;
        }

        private static bool UriSchemeSupportsDisco(Uri serviceUri)
        {
            if (!(serviceUri.Scheme == Uri.UriSchemeHttp))
            {
                return (serviceUri.Scheme == Uri.UriSchemeHttps);
            }
            return true;
        }

        KeyedByTypeCollection<IEndpointBehavior> IProvideChannelBuilderSettings.Behaviors
        {
            get
            {
                if (this.serviceChannel != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("TooLate"), HR.RPC_E_TOO_LATE));
                }
                return this.behaviors;
            }
        }

        ServiceChannel IProvideChannelBuilderSettings.ServiceChannel
        {
            get
            {
                return this.CreateChannel();
            }
        }

        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadOnly
        {
            get
            {
                return this.serviceChannelFactory;
            }
        }

        ServiceChannelFactory IProvideChannelBuilderSettings.ServiceChannelFactoryReadWrite
        {
            get
            {
                if (this.serviceChannel != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException(System.ServiceModel.SR.GetString("TooLate"), HR.RPC_E_TOO_LATE));
                }
                return this.serviceChannelFactory;
            }
        }
    }
}

