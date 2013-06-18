namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AspNetCompatibilityRequirementsAttribute : Attribute, IServiceBehavior
    {
        private AspNetCompatibilityRequirementsMode requirementsMode;

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            AspNetEnvironment.Current.ValidateCompatibilityRequirements(this.RequirementsMode);
        }

        public AspNetCompatibilityRequirementsMode RequirementsMode
        {
            get
            {
                return this.requirementsMode;
            }
            set
            {
                AspNetCompatibilityRequirementsModeHelper.Validate(value);
                this.requirementsMode = value;
            }
        }
    }
}

