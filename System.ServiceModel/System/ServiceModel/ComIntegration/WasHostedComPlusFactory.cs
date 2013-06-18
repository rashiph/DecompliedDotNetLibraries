namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public sealed class WasHostedComPlusFactory : ServiceHostFactoryBase
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (!AspNetEnvironment.Enabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("Hosting_ProcessNotExecutingUnderHostedContext", new object[] { "WasHostedComPlusFactory.CreateServiceHost" })));
            }
            if (string.IsNullOrEmpty(constructorString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("Hosting_ServiceTypeNotProvided")));
            }
            return new WebHostedComPlusServiceHost(constructorString, baseAddresses);
        }
    }
}

