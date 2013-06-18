namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class WorkflowIdleBehavior : IServiceBehavior
    {
        internal static TimeSpan defaultTimeToPersist = TimeSpan.MaxValue;
        internal const string defaultTimeToPersistString = "Infinite";
        internal static TimeSpan defaultTimeToUnload = TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture);
        internal const string defaultTimeToUnloadString = "00:01:00";
        private TimeSpan timeToPersist = defaultTimeToPersist;
        private TimeSpan timeToUnload = defaultTimeToUnload;

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            WorkflowServiceHost host = serviceHostBase as WorkflowServiceHost;
            if (host != null)
            {
                host.IdleTimeToPersist = this.TimeToPersist;
                host.IdleTimeToUnload = this.TimeToUnload;
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

        public TimeSpan TimeToPersist
        {
            get
            {
                return this.timeToPersist;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, System.ServiceModel.Activities.SR.ErrorTimeToPersistLessThanZero);
                }
                this.timeToPersist = value;
            }
        }

        public TimeSpan TimeToUnload
        {
            get
            {
                return this.timeToUnload;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw FxTrace.Exception.ArgumentOutOfRange("value", value, System.ServiceModel.Activities.SR.ErrorTimeToUnloadLessThanZero);
                }
                this.timeToUnload = value;
            }
        }
    }
}

