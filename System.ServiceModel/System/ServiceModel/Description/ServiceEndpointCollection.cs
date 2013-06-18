namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.Xml;

    public class ServiceEndpointCollection : Collection<ServiceEndpoint>
    {
        internal ServiceEndpointCollection()
        {
        }

        public ServiceEndpoint Find(Type contractType)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }
            foreach (ServiceEndpoint endpoint in this)
            {
                if ((endpoint != null) && (endpoint.Contract.ContractType == contractType))
                {
                    return endpoint;
                }
            }
            return null;
        }

        public ServiceEndpoint Find(Uri address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            foreach (ServiceEndpoint endpoint in this)
            {
                if ((endpoint != null) && (endpoint.Address.Uri == address))
                {
                    return endpoint;
                }
            }
            return null;
        }

        public ServiceEndpoint Find(XmlQualifiedName contractName)
        {
            if (contractName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractName");
            }
            foreach (ServiceEndpoint endpoint in this)
            {
                if (((endpoint != null) && (endpoint.Contract.Name == contractName.Name)) && (endpoint.Contract.Namespace == contractName.Namespace))
                {
                    return endpoint;
                }
            }
            return null;
        }

        public ServiceEndpoint Find(Type contractType, XmlQualifiedName bindingName)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }
            if (bindingName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingName");
            }
            foreach (ServiceEndpoint endpoint in this)
            {
                if (((endpoint != null) && (endpoint.Contract.ContractType == contractType)) && ((endpoint.Binding.Name == bindingName.Name) && (endpoint.Binding.Namespace == bindingName.Namespace)))
                {
                    return endpoint;
                }
            }
            return null;
        }

        public ServiceEndpoint Find(XmlQualifiedName contractName, XmlQualifiedName bindingName)
        {
            if (contractName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractName");
            }
            if (bindingName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingName");
            }
            foreach (ServiceEndpoint endpoint in this)
            {
                if ((((endpoint != null) && (endpoint.Contract.Name == contractName.Name)) && ((endpoint.Contract.Namespace == contractName.Namespace) && (endpoint.Binding.Name == bindingName.Name))) && (endpoint.Binding.Namespace == bindingName.Namespace))
                {
                    return endpoint;
                }
            }
            return null;
        }

        public Collection<ServiceEndpoint> FindAll(Type contractType)
        {
            if (contractType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractType");
            }
            Collection<ServiceEndpoint> collection = new Collection<ServiceEndpoint>();
            foreach (ServiceEndpoint endpoint in this)
            {
                if ((endpoint != null) && (endpoint.Contract.ContractType == contractType))
                {
                    collection.Add(endpoint);
                }
            }
            return collection;
        }

        public Collection<ServiceEndpoint> FindAll(XmlQualifiedName contractName)
        {
            if (contractName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractName");
            }
            Collection<ServiceEndpoint> collection = new Collection<ServiceEndpoint>();
            foreach (ServiceEndpoint endpoint in this)
            {
                if (((endpoint != null) && (endpoint.Contract.Name == contractName.Name)) && (endpoint.Contract.Namespace == contractName.Namespace))
                {
                    collection.Add(endpoint);
                }
            }
            return collection;
        }

        protected override void InsertItem(int index, ServiceEndpoint item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, ServiceEndpoint item)
        {
            if (item == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item");
            }
            base.SetItem(index, item);
        }
    }
}

