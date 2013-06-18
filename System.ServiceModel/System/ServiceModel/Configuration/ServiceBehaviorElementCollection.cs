namespace System.ServiceModel.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel;
    using System.Xml;

    [ConfigurationCollection(typeof(ServiceBehaviorElement), AddItemName="behavior")]
    public sealed class ServiceBehaviorElementCollection : ServiceModelEnhancedConfigurationElementCollection<ServiceBehaviorElement>
    {
        public ServiceBehaviorElementCollection() : base("behavior")
        {
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ServiceBehaviorElement element2 = element as ServiceBehaviorElement;
            string name = element2.Name;
            ServiceBehaviorElement element3 = base.BaseGet(name) as ServiceBehaviorElement;
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

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            ServiceBehaviorElement element2 = (ServiceBehaviorElement) element;
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

