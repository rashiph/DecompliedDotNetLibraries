namespace System.ServiceModel.Activities.Activation
{
    using System.ServiceModel.Activation;

    internal class ServiceModelActivitiesActivationHandler : HttpHandler, IServiceModelActivationHandler
    {
        public ServiceHostFactoryBase GetFactory()
        {
            return new WorkflowServiceHostFactory();
        }
    }
}

