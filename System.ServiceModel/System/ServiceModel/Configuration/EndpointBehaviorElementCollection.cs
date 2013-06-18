namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;

    [ConfigurationCollection(typeof(EndpointBehaviorElement), AddItemName="behavior")]
    public sealed class EndpointBehaviorElementCollection : ServiceModelEnhancedConfigurationElementCollection<EndpointBehaviorElement>
    {
        public EndpointBehaviorElementCollection() : base("behavior")
        {
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            EndpointBehaviorElement element2 = element as EndpointBehaviorElement;
            string name = element2.Name;
            EndpointBehaviorElement element3 = base.BaseGet(name) as EndpointBehaviorElement;
            List<BehaviorExtensionElement> parentExtensionElements = new List<BehaviorExtensionElement>();
            if (element3 != null)
            {
                foreach (BehaviorExtensionElement element4 in element3)
                {
                    parentExtensionElements.Add(element4);
                }
            }
            element2.MergeWith(parentExtensionElements);
            base.BaseAdd(element2);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            EndpointBehaviorElement element2 = (EndpointBehaviorElement) element;
            return element2.Name;
        }

        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    }
}

