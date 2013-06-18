namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.Windows.Markup;

    [ContentProperty("Elements")]
    public class CustomBinding : Binding
    {
        private BindingElementCollection bindingElements;

        public CustomBinding()
        {
            this.bindingElements = new BindingElementCollection();
        }

        public CustomBinding(params BindingElement[] bindingElementsInTopDownChannelStackOrder)
        {
            this.bindingElements = new BindingElementCollection();
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        public CustomBinding(Binding binding) : this(binding, SafeCreateBindingElements(binding))
        {
        }

        internal CustomBinding(BindingElementCollection bindingElements)
        {
            this.bindingElements = new BindingElementCollection();
            if (bindingElements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            for (int i = 0; i < bindingElements.Count; i++)
            {
                this.bindingElements.Add(bindingElements[i]);
            }
        }

        public CustomBinding(IEnumerable<BindingElement> bindingElementsInTopDownChannelStackOrder)
        {
            this.bindingElements = new BindingElementCollection();
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        public CustomBinding(string configurationName)
        {
            this.bindingElements = new BindingElementCollection();
            this.ApplyConfiguration(configurationName);
        }

        internal CustomBinding(Binding binding, BindingElementCollection elements)
        {
            this.bindingElements = new BindingElementCollection();
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (elements == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("elements");
            }
            base.Name = binding.Name;
            base.Namespace = binding.Namespace;
            base.CloseTimeout = binding.CloseTimeout;
            base.OpenTimeout = binding.OpenTimeout;
            base.ReceiveTimeout = binding.ReceiveTimeout;
            base.SendTimeout = binding.SendTimeout;
            for (int i = 0; i < elements.Count; i++)
            {
                this.bindingElements.Add(elements[i]);
            }
        }

        public CustomBinding(string name, string ns, params BindingElement[] bindingElementsInTopDownChannelStackOrder) : base(name, ns)
        {
            this.bindingElements = new BindingElementCollection();
            if (bindingElementsInTopDownChannelStackOrder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElements");
            }
            foreach (BindingElement element in bindingElementsInTopDownChannelStackOrder)
            {
                this.bindingElements.Add(element);
            }
        }

        private void ApplyConfiguration(string configurationName)
        {
            CustomBindingElement element2 = CustomBindingCollectionElement.GetBindingCollectionElement().Bindings[configurationName];
            if (element2 == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidBindingConfigurationName", new object[] { configurationName, "customBinding" })));
            }
            element2.ApplyConfiguration(this);
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return this.bindingElements.Clone();
        }

        private static BindingElementCollection SafeCreateBindingElements(Binding binding)
        {
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            return binding.CreateBindingElements();
        }

        public BindingElementCollection Elements
        {
            get
            {
                return this.bindingElements;
            }
        }

        public override string Scheme
        {
            get
            {
                TransportBindingElement element = this.bindingElements.Find<TransportBindingElement>();
                if (element == null)
                {
                    return string.Empty;
                }
                return element.Scheme;
            }
        }
    }
}

