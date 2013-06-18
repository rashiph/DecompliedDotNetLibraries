namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    public static class MetadataResolver
    {
        public static IAsyncResult BeginResolve(Type contract, EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            return BeginResolve(CreateContractCollection(contract), address, callback, asyncState);
        }

        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, AsyncCallback callback, object asyncState)
        {
            return BeginResolve(contracts, address, new MetadataExchangeClient(address), callback, asyncState);
        }

        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, MetadataExchangeClient client, AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);
            return new AsyncMetadataResolverHelper(address, MetadataExchangeClientMode.MetadataExchange, client, contracts, callback, asyncState);
        }

        public static IAsyncResult BeginResolve(Type contract, Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            return BeginResolve(CreateContractCollection(contract), address, mode, callback, asyncState);
        }

        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, AsyncCallback callback, object asyncState)
        {
            return BeginResolve(contracts, address, mode, new MetadataExchangeClient(address, mode), callback, asyncState);
        }

        public static IAsyncResult BeginResolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, MetadataExchangeClient client, AsyncCallback callback, object asyncState)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            MetadataExchangeClientModeHelper.Validate(mode);
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);
            return new AsyncMetadataResolverHelper(new EndpointAddress(address, new AddressHeader[0]), mode, client, contracts, callback, asyncState);
        }

        private static Collection<ContractDescription> CreateContractCollection(Type contract)
        {
            return new Collection<ContractDescription> { ContractDescription.GetContract(contract) };
        }

        public static ServiceEndpointCollection EndResolve(IAsyncResult result)
        {
            return AsyncMetadataResolverHelper.EndAsyncCall(result);
        }

        private static ServiceEndpointCollection ImportEndpoints(MetadataSet metadataSet, IEnumerable<ContractDescription> contracts, MetadataExchangeClient client)
        {
            ServiceEndpointCollection endpoints = new ServiceEndpointCollection();
            WsdlImporter importer = new WsdlImporter(metadataSet);
            importer.State.Add("MetadataExchangeClientKey", client);
            foreach (ContractDescription description in contracts)
            {
                importer.KnownContracts.Add(WsdlExporter.WsdlNamingHelper.GetPortTypeQName(description), description);
            }
            foreach (ContractDescription description2 in contracts)
            {
                foreach (ServiceEndpoint endpoint in importer.ImportEndpoints(description2))
                {
                    endpoints.Add(endpoint);
                }
            }
            if (importer.Errors.Count > 0)
            {
                TraceWsdlImportErrors(importer);
            }
            return endpoints;
        }

        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, EndpointAddress address)
        {
            return Resolve(contracts, address, new MetadataExchangeClient(address));
        }

        public static ServiceEndpointCollection Resolve(Type contract, EndpointAddress address)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            return Resolve(CreateContractCollection(contract), address);
        }

        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, EndpointAddress address, MetadataExchangeClient client)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);
            return ImportEndpoints(client.GetMetadata(address), contracts, client);
        }

        public static ServiceEndpointCollection Resolve(Type contract, Uri address, MetadataExchangeClientMode mode)
        {
            if (contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contract");
            }
            return Resolve(CreateContractCollection(contract), address, mode);
        }

        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode)
        {
            return Resolve(contracts, address, mode, new MetadataExchangeClient(address, mode));
        }

        public static ServiceEndpointCollection Resolve(IEnumerable<ContractDescription> contracts, Uri address, MetadataExchangeClientMode mode, MetadataExchangeClient client)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            MetadataExchangeClientModeHelper.Validate(mode);
            if (client == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("client");
            }
            if (contracts == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contracts");
            }
            ValidateContracts(contracts);
            return ImportEndpoints(client.GetMetadata(address, mode), contracts, client);
        }

        private static void TraceWsdlImportErrors(WsdlImporter importer)
        {
            foreach (MetadataConversionError error in importer.Errors)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    Hashtable hashtable2 = new Hashtable(2);
                    hashtable2.Add("IsWarning", error.IsWarning);
                    hashtable2.Add("Message", error.Message);
                    Hashtable dictionary = hashtable2;
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8003d, System.ServiceModel.SR.GetString("TraceCodeWsmexNonCriticalWsdlExportError"), new DictionaryTraceRecord(dictionary), null, null);
                }
            }
        }

        private static void ValidateContracts(IEnumerable<ContractDescription> contracts)
        {
            bool flag = true;
            Collection<XmlQualifiedName> collection = new Collection<XmlQualifiedName>();
            foreach (ContractDescription description in contracts)
            {
                flag = false;
                if (description == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMetadataResolverKnownContractsCannotContainNull"));
                }
                XmlQualifiedName portTypeQName = WsdlExporter.WsdlNamingHelper.GetPortTypeQName(description);
                if (collection.Contains(portTypeQName))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMetadataResolverKnownContractsUniqueQNames", new object[] { portTypeQName.Name, portTypeQName.Namespace }));
                }
                collection.Add(portTypeQName);
            }
            if (flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxMetadataResolverKnownContractsArgumentCannotBeEmpty"));
            }
        }

        private class AsyncMetadataResolverHelper : AsyncResult
        {
            private EndpointAddress address;
            private MetadataExchangeClient client;
            private ServiceEndpointCollection endpointCollection;
            private IEnumerable<ContractDescription> knownContracts;
            private MetadataExchangeClientMode mode;

            internal AsyncMetadataResolverHelper(EndpointAddress address, MetadataExchangeClientMode mode, MetadataExchangeClient client, IEnumerable<ContractDescription> knownContracts, AsyncCallback callback, object asyncState) : base(callback, asyncState)
            {
                this.address = address;
                this.client = client;
                this.mode = mode;
                this.knownContracts = knownContracts;
                this.GetMetadataSetAsync();
            }

            internal static ServiceEndpointCollection EndAsyncCall(IAsyncResult result)
            {
                return AsyncResult.End<MetadataResolver.AsyncMetadataResolverHelper>(result).endpointCollection;
            }

            internal void EndGetMetadataSet(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception exception = null;
                    try
                    {
                        this.HandleResult(result);
                    }
                    catch (Exception exception2)
                    {
                        if (Fx.IsFatal(exception2))
                        {
                            throw;
                        }
                        exception = exception2;
                    }
                    base.Complete(false, exception);
                }
            }

            internal void GetMetadataSetAsync()
            {
                IAsyncResult result;
                if (this.mode == MetadataExchangeClientMode.HttpGet)
                {
                    result = this.client.BeginGetMetadata(this.address.Uri, MetadataExchangeClientMode.HttpGet, Fx.ThunkCallback(new AsyncCallback(this.EndGetMetadataSet)), null);
                }
                else
                {
                    result = this.client.BeginGetMetadata(this.address, Fx.ThunkCallback(new AsyncCallback(this.EndGetMetadataSet)), null);
                }
                if (result.CompletedSynchronously)
                {
                    this.HandleResult(result);
                    base.Complete(true);
                }
            }

            private void HandleResult(IAsyncResult result)
            {
                MetadataSet metadataSet = this.client.EndGetMetadata(result);
                this.endpointCollection = MetadataResolver.ImportEndpoints(metadataSet, this.knownContracts, this.client);
            }
        }
    }
}

