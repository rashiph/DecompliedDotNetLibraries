namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;
    using System.Web.Services.Description;
    using System.Xml;
    using System.Xml.Schema;

    internal class WsdlServiceChannelBuilder : IProxyCreator, IDisposable, IProvideChannelBuilderSettings
    {
        private KeyedByTypeCollection<IEndpointBehavior> behaviors = new KeyedByTypeCollection<IEndpointBehavior>();
        private ContractDescription contractDescription;
        private Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable;
        private ServiceChannel serviceChannel;
        private ServiceChannelFactory serviceChannelFactory;
        private ServiceEndpoint serviceEndpoint;
        private bool useXmlSerializer;

        internal WsdlServiceChannelBuilder(Dictionary<MonikerHelper.MonikerAttribute, string> propertyTable)
        {
            this.propertyTable = propertyTable;
            this.ProcessWsdl();
        }

        private ServiceChannel CreateChannel()
        {
            Thread.MemoryBarrier();
            if (this.serviceChannel == null)
            {
                lock (this)
                {
                    Thread.MemoryBarrier();
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
                            this.serviceChannel = this.serviceChannelFactory.CreateServiceChannel(new EndpointAddress(this.serviceEndpoint.Address.Uri, this.serviceEndpoint.Address.Identity, this.serviceEndpoint.Address.Headers), this.serviceEndpoint.Address.Uri);
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

        private void ProcessWsdl()
        {
            string str;
            string str2;
            string str3;
            string str4;
            string str5 = null;
            string str6 = null;
            string str7 = null;
            EndpointIdentity identity = null;
            string str8 = null;
            string targetNamespace = null;
            string str10 = null;
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Wsdl, out str);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Contract, out str2);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Binding, out str3);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Address, out str4);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.SpnIdentity, out str5);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.UpnIdentity, out str6);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.DnsIdentity, out str7);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.Serializer, out str8);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.BindingNamespace, out str10);
            this.propertyTable.TryGetValue(MonikerHelper.MonikerAttribute.ContractNamespace, out targetNamespace);
            if (string.IsNullOrEmpty(str))
            {
                throw Fx.AssertAndThrow("Wsdl should not be null at this point");
            }
            if ((string.IsNullOrEmpty(str2) || string.IsNullOrEmpty(str3)) || string.IsNullOrEmpty(str4))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("ContractBindingAddressCannotBeNull")));
            }
            if (!string.IsNullOrEmpty(str5))
            {
                if (!string.IsNullOrEmpty(str6) || !string.IsNullOrEmpty(str7))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateSpnIdentity(str5);
            }
            else if (!string.IsNullOrEmpty(str6))
            {
                if (!string.IsNullOrEmpty(str5) || !string.IsNullOrEmpty(str7))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateUpnIdentity(str6);
            }
            else if (!string.IsNullOrEmpty(str7))
            {
                if (!string.IsNullOrEmpty(str5) || !string.IsNullOrEmpty(str6))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorrectServerIdentity")));
                }
                identity = EndpointIdentity.CreateDnsIdentity(str7);
            }
            else
            {
                identity = null;
            }
            bool flag = false;
            if (!string.IsNullOrEmpty(str8))
            {
                if (("xml" != str8) && ("datacontract" != str8))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("MonikerIncorectSerializer")));
                }
                if ("xml" == str8)
                {
                    this.useXmlSerializer = true;
                }
                else
                {
                    flag = true;
                }
            }
            TextReader textReader = new StringReader(str);
            try
            {
                WsdlImporter importer;
                System.Web.Services.Description.ServiceDescription serviceDescription = System.Web.Services.Description.ServiceDescription.Read(textReader);
                if (string.IsNullOrEmpty(targetNamespace))
                {
                    targetNamespace = serviceDescription.TargetNamespace;
                }
                if (string.IsNullOrEmpty(str10))
                {
                    str10 = serviceDescription.TargetNamespace;
                }
                ServiceDescriptionCollection wsdlDocuments = new ServiceDescriptionCollection();
                wsdlDocuments.Add(serviceDescription);
                XmlSchemaSet xmlSchemas = new XmlSchemaSet();
                foreach (System.Xml.Schema.XmlSchema schema in serviceDescription.Types.Schemas)
                {
                    xmlSchemas.Add(schema);
                }
                MetadataSet metaData = new MetadataSet(WsdlImporter.CreateMetadataDocuments(wsdlDocuments, xmlSchemas, null));
                if (this.useXmlSerializer)
                {
                    importer = this.CreateXmlSerializerImporter(metaData);
                }
                else if (flag)
                {
                    importer = this.CreateDataContractSerializerImporter(metaData);
                }
                else
                {
                    importer = new WsdlImporter(metaData);
                }
                XmlQualifiedName name = new XmlQualifiedName(str2, targetNamespace);
                XmlQualifiedName name2 = new XmlQualifiedName(str3, str10);
                PortType portType = wsdlDocuments.GetPortType(name);
                this.contractDescription = importer.ImportContract(portType);
                System.Web.Services.Description.Binding wsdlBinding = wsdlDocuments.GetBinding(name2);
                System.ServiceModel.Channels.Binding binding = importer.ImportBinding(wsdlBinding);
                EndpointAddress address = new EndpointAddress(new Uri(str4), identity, null);
                this.serviceEndpoint = new ServiceEndpoint(this.contractDescription, binding, address);
                ComPlusWsdlChannelBuilderTrace.Trace(TraceEventType.Verbose, 0x5001d, "TraceCodeComIntegrationWsdlChannelBuilderLoaded", name2, name, serviceDescription, this.contractDescription, binding, serviceDescription.Types.Schemas);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MonikerSyntaxException(System.ServiceModel.SR.GetString("FailedImportOfWsdl", new object[] { exception.Message })));
            }
            finally
            {
                IDisposable disposable = textReader;
                disposable.Dispose();
            }
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

