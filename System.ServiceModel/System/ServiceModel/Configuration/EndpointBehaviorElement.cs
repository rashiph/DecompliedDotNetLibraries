namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public class EndpointBehaviorElement : NamedServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public EndpointBehaviorElement() : this(null)
        {
        }

        public EndpointBehaviorElement(string name) : base("behaviorExtensions", name)
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
                if (!typeof(IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidEndpointBehaviorType", new object[] { element.ConfigurationElementName, base.Name }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
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
                if (!typeof(IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidEndpointBehaviorType", new object[] { element.ConfigurationElementName, base.Name }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
                }
            }
            return base.CanAdd(element);
        }
    }
}

