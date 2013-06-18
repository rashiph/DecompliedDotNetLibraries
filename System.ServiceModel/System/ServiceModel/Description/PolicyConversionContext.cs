namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    public abstract class PolicyConversionContext
    {
        private readonly ContractDescription contract;

        protected PolicyConversionContext(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.contract = endpoint.Contract;
        }

        internal static XmlElement FindAssertion(ICollection<XmlElement> assertions, string localName, string namespaceUri, bool remove)
        {
            XmlElement item = null;
            foreach (XmlElement element2 in assertions)
            {
                if ((element2.LocalName == localName) && ((namespaceUri == null) || (element2.NamespaceURI == namespaceUri)))
                {
                    item = element2;
                    if (remove)
                    {
                        assertions.Remove(item);
                    }
                    return item;
                }
            }
            return item;
        }

        public abstract PolicyAssertionCollection GetBindingAssertions();
        public abstract PolicyAssertionCollection GetFaultBindingAssertions(FaultDescription fault);
        public abstract PolicyAssertionCollection GetMessageBindingAssertions(MessageDescription message);
        public abstract PolicyAssertionCollection GetOperationBindingAssertions(OperationDescription operation);

        public abstract BindingElementCollection BindingElements { get; }

        public ContractDescription Contract
        {
            get
            {
                return this.contract;
            }
        }
    }
}

