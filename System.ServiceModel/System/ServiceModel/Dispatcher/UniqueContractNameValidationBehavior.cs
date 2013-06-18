namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class UniqueContractNameValidationBehavior : IServiceBehavior
    {
        private Dictionary<XmlQualifiedName, ContractDescription> contracts = new Dictionary<XmlQualifiedName, ContractDescription>();

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceHostBase");
            }
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                XmlQualifiedName key = new XmlQualifiedName(endpoint.Contract.Name, endpoint.Contract.Namespace);
                if (!this.contracts.ContainsKey(key))
                {
                    this.contracts.Add(key, endpoint.Contract);
                }
                else if (this.contracts[key] != endpoint.Contract)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultipleContractsWithSameName", new object[] { key.Name, key.Namespace })));
                }
            }
        }
    }
}

