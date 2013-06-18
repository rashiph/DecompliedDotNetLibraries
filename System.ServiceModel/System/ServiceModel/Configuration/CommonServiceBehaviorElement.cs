namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public class CommonServiceBehaviorElement : ServiceModelExtensionCollectionElement<BehaviorExtensionElement>
    {
        public CommonServiceBehaviorElement() : base("behaviorExtensions")
        {
        }

        public override void Add(BehaviorExtensionElement element)
        {
            if ((element != null) && !typeof(IServiceBehavior).IsAssignableFrom(element.BehaviorType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCommonServiceBehaviorType", new object[] { element.ConfigurationElementName, typeof(IServiceBehavior).FullName }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
            }
            base.Add(element);
        }

        public override bool CanAdd(BehaviorExtensionElement element)
        {
            if ((element != null) && !typeof(IServiceBehavior).IsAssignableFrom(element.BehaviorType))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(System.ServiceModel.SR.GetString("ConfigInvalidCommonServiceBehaviorType", new object[] { element.ConfigurationElementName, typeof(IServiceBehavior).FullName }), element.ElementInformation.Source, element.ElementInformation.LineNumber));
            }
            return base.CanAdd(element);
        }
    }
}

