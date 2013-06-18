namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Xml;

    public class ServiceBehaviorElement : NamedServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public ServiceBehaviorElement() : this(null)
        {
        }

        public ServiceBehaviorElement(string name) : base("behaviorExtensions", name)
        {
        }

        public override void Add(BehaviorExtensionElement element)
        {
            if (element != null)
            {
                if ((element is ClearBehaviorElement) || (element is RemoveBehaviorElement))
                {
                    base.AddItem(element);
                    return;
                }
                if (!typeof(IServiceBehavior).IsAssignableFrom(element.BehaviorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidServiceBehaviorType", new object[] { element.ConfigurationElementName, base.Name }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
                }
            }
            base.Add(element);
        }

        public override bool CanAdd(BehaviorExtensionElement element)
        {
            if (element != null)
            {
                if ((element is ClearBehaviorElement) || (element is RemoveBehaviorElement))
                {
                    return true;
                }
                if (!typeof(IServiceBehavior).IsAssignableFrom(element.BehaviorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidServiceBehaviorType", new object[] { element.ConfigurationElementName, base.Name }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
                }
            }
            return base.CanAdd(element);
        }

        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            base.DeserializeElement(reader, serializeCollectionKey);
        }
    }
}

