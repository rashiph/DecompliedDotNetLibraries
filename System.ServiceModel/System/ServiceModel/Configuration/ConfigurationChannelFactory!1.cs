namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public sealed class ConfigurationChannelFactory<TChannel> : ChannelFactory<TChannel>
    {
        public ConfigurationChannelFactory(string endpointConfigurationName, System.Configuration.Configuration configuration, EndpointAddress remoteAddress) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { typeof(TChannel).FullName }), ActivityType.Construct);
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }
                if (configuration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configuration");
                }
                base.InitializeEndpoint(endpointConfigurationName, remoteAddress, configuration);
            }
        }
    }
}

