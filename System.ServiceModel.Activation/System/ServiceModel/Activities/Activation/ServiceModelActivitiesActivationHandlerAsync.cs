namespace System.ServiceModel.Activities.Activation
{
    using System.ServiceModel.Activation;

    internal class ServiceModelActivitiesActivationHandlerAsync : ServiceHttpHandlerFactory, IServiceModelActivationHandler
    {
        public ServiceHostFactoryBase GetFactory()
        {
            return new WorkflowServiceHostFactory();
        }
    }
}

