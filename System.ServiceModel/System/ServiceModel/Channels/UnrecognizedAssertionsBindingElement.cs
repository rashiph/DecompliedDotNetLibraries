namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    internal class UnrecognizedAssertionsBindingElement : BindingElement
    {
        private ICollection<XmlElement> bindingAsserions;
        private IDictionary<MessageDescription, ICollection<XmlElement>> messageAssertions;
        private IDictionary<OperationDescription, ICollection<XmlElement>> operationAssertions;
        private XmlQualifiedName wsdlBinding;

        protected UnrecognizedAssertionsBindingElement(UnrecognizedAssertionsBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
            this.wsdlBinding = elementToBeCloned.wsdlBinding;
            this.bindingAsserions = elementToBeCloned.bindingAsserions;
            this.operationAssertions = elementToBeCloned.operationAssertions;
            this.messageAssertions = elementToBeCloned.messageAssertions;
        }

        protected internal UnrecognizedAssertionsBindingElement(XmlQualifiedName wsdlBinding, ICollection<XmlElement> bindingAsserions)
        {
            this.wsdlBinding = wsdlBinding;
            this.bindingAsserions = bindingAsserions;
        }

        internal void Add(MessageDescription message, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> is2;
            if (!this.MessageAssertions.TryGetValue(message, out is2))
            {
                this.MessageAssertions.Add(message, assertions);
            }
            else
            {
                foreach (XmlElement element in assertions)
                {
                    is2.Add(element);
                }
            }
        }

        internal void Add(OperationDescription operation, ICollection<XmlElement> assertions)
        {
            ICollection<XmlElement> is2;
            if (!this.OperationAssertions.TryGetValue(operation, out is2))
            {
                this.OperationAssertions.Add(operation, assertions);
            }
            else
            {
                foreach (XmlElement element in assertions)
                {
                    is2.Add(element);
                }
            }
        }

        public override BindingElement Clone()
        {
            return new UnrecognizedAssertionsBindingElement(new XmlQualifiedName(this.wsdlBinding.Name, this.wsdlBinding.Namespace), null);
        }

        public override T GetProperty<T>(BindingContext context) where T: class
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            return context.GetInnerProperty<T>();
        }

        internal ICollection<XmlElement> BindingAsserions
        {
            get
            {
                if (this.bindingAsserions == null)
                {
                    this.bindingAsserions = new Collection<XmlElement>();
                }
                return this.bindingAsserions;
            }
        }

        internal IDictionary<MessageDescription, ICollection<XmlElement>> MessageAssertions
        {
            get
            {
                if (this.messageAssertions == null)
                {
                    this.messageAssertions = new Dictionary<MessageDescription, ICollection<XmlElement>>();
                }
                return this.messageAssertions;
            }
        }

        internal IDictionary<OperationDescription, ICollection<XmlElement>> OperationAssertions
        {
            get
            {
                if (this.operationAssertions == null)
                {
                    this.operationAssertions = new Dictionary<OperationDescription, ICollection<XmlElement>>();
                }
                return this.operationAssertions;
            }
        }

        internal XmlQualifiedName WsdlBinding
        {
            get
            {
                return this.wsdlBinding;
            }
        }
    }
}

