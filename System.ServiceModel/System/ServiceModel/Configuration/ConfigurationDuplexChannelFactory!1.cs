namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public sealed class ConfigurationDuplexChannelFactory<TChannel> : DuplexChannelFactory<TChannel>
    {
        public ConfigurationDuplexChannelFactory(object callbackObject, string endpointConfigurationName, EndpointAddress remoteAddress, System.Configuration.Configuration configuration) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { TraceUtility.CreateSourceString(this) }), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }
                if (configuration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configuration");
                }
                base.CheckAndAssignCallbackInstance(callbackObject);
                base.InitializeEndpoint(endpointConfigurationName, remoteAddress, configuration);
            }
        }
    }
}

