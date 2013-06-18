namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Activities.DurableInstancing;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class SqlWorkflowInstanceStoreBehavior : IServiceBehavior
    {
        internal const System.Activities.DurableInstancing.InstanceEncodingOption defaultEncodingOption = System.Activities.DurableInstancing.InstanceEncodingOption.GZip;
        private static TimeSpan defaultHostRenewalPeriod = TimeSpan.Parse("00:00:30.0", CultureInfo.InvariantCulture);
        internal const string defaultHostRenewalString = "00:00:30.0";
        internal const System.Activities.DurableInstancing.InstanceCompletionAction defaultInstanceCompletionAction = System.Activities.DurableInstancing.InstanceCompletionAction.DeleteAll;
        internal const System.Activities.DurableInstancing.InstanceLockedExceptionAction defaultInstanceLockedExceptionAction = System.Activities.DurableInstancing.InstanceLockedExceptionAction.NoRetry;
        internal const int defaultMaximumRetries = 4;
        private static TimeSpan defaultRunnableInstancesDetectionPeriod = TimeSpan.Parse("00:00:05.0", CultureInfo.InvariantCulture);
        internal const string defaultRunnableInstancesDetectionPeriodString = "00:00:05.0";

        public SqlWorkflowInstanceStoreBehavior() : this(null)
        {
        }

        public SqlWorkflowInstanceStoreBehavior(string connectionString)
        {
            System.Activities.DurableInstancing.SqlWorkflowInstanceStore store = new System.Activities.DurableInstancing.SqlWorkflowInstanceStore(connectionString) {
                InstanceEncodingOption = System.Activities.DurableInstancing.InstanceEncodingOption.GZip,
                InstanceCompletionAction = System.Activities.DurableInstancing.InstanceCompletionAction.DeleteAll,
                InstanceLockedExceptionAction = System.Activities.DurableInstancing.InstanceLockedExceptionAction.NoRetry,
                HostLockRenewalPeriod = defaultHostRenewalPeriod,
                RunnableInstancesDetectionPeriod = defaultRunnableInstancesDetectionPeriod,
                EnqueueRunCommands = true
            };
            this.SqlWorkflowInstanceStore = store;
        }

        public void AddBindingParameters(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            if (serviceHostBase == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceHostBase");
            }
            WorkflowServiceHost host = serviceHostBase as WorkflowServiceHost;
            if (host != null)
            {
                host.DurableInstancingOptions.InstanceStore = this.SqlWorkflowInstanceStore;
            }
        }

        public void Promote(string name, IEnumerable<XName> promoteAsSqlVariant, IEnumerable<XName> promoteAsBinary)
        {
            this.SqlWorkflowInstanceStore.Promote(name, promoteAsSqlVariant, promoteAsBinary);
        }

        public void Validate(System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        public string ConnectionString
        {
            get
            {
                return this.SqlWorkflowInstanceStore.ConnectionString;
            }
            set
            {
                this.SqlWorkflowInstanceStore.ConnectionString = value;
            }
        }

        public TimeSpan HostLockRenewalPeriod
        {
            get
            {
                return this.SqlWorkflowInstanceStore.HostLockRenewalPeriod;
            }
            set
            {
                TimeoutHelper.ThrowIfNonPositiveArgument(value);
                this.SqlWorkflowInstanceStore.HostLockRenewalPeriod = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceCompletionAction InstanceCompletionAction
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceCompletionAction;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceCompletionAction = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceEncodingOption InstanceEncodingOption
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceEncodingOption;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceEncodingOption = value;
            }
        }

        public System.Activities.DurableInstancing.InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get
            {
                return this.SqlWorkflowInstanceStore.InstanceLockedExceptionAction;
            }
            set
            {
                this.SqlWorkflowInstanceStore.InstanceLockedExceptionAction = value;
            }
        }

        public int MaxConnectionRetries
        {
            get
            {
                if (this.SqlWorkflowInstanceStore != null)
                {
                    return this.SqlWorkflowInstanceStore.MaxConnectionRetries;
                }
                return 4;
            }
            set
            {
                if (this.SqlWorkflowInstanceStore != null)
                {
                    this.SqlWorkflowInstanceStore.MaxConnectionRetries = value;
                }
            }
        }

        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get
            {
                return this.SqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod;
            }
            set
            {
                TimeoutHelper.ThrowIfNonPositiveArgument(value);
                this.SqlWorkflowInstanceStore.RunnableInstancesDetectionPeriod = value;
            }
        }

        private System.Activities.DurableInstancing.SqlWorkflowInstanceStore SqlWorkflowInstanceStore { get; set; }
    }
}

