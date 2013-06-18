namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class WorkflowUnhandledExceptionBehavior : IServiceBehavior
    {
        private WorkflowUnhandledExceptionAction action = WorkflowUnhandledExceptionAction.AbandonAndSuspend;
        internal const WorkflowUnhandledExceptionAction defaultAction = WorkflowUnhandledExceptionAction.AbandonAndSuspend;

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost host = serviceHostBase as WorkflowServiceHost;
            if (host != null)
            {
                host.UnhandledExceptionAction = this.Action;
            }
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceDescription == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceDescription");
            }
            if (serviceHostBase == null)
            {
                throw FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
        }

        public WorkflowUnhandledExceptionAction Action
        {
            get
            {
                return this.action;
            }
            set
            {
                if (!WorkflowUnhandledExceptionActionHelper.IsDefined(value))
                {
                    throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("value"));
                }
                this.action = value;
            }
        }
    }
}

