namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public class CommonEndpointBehaviorElement : ServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public CommonEndpointBehaviorElement() : base("behaviorExtensions")
        {
        }

        public override void Add(BehaviorExtensionElement element)
        {
            if ((element != null) && !typeof(IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCommonEndpointBehaviorType", new object[] { element.ConfigurationElementName, typeof(IEndpointBehavior).FullName }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
            }
            base.Add(element);
        }

        public override bool CanAdd(BehaviorExtensionElement element)
        {
            if ((element != null) && !typeof(IEndpointBehavior).IsAssignableFrom(element.BehaviorType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCommonEndpointBehaviorType", new object[] { element.ConfigurationElementName, typeof(IEndpointBehavior).FullName }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
            }
            return base.CanAdd(element);
        }
    }
}

