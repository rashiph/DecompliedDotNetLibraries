namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Activities.DurableInstancing;
    using System.ComponentModel;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public class SqlWorkflowInstanceStoreElement : BehaviorExtensionElement
    {
        private const string connectionString = "connectionString";
        private const string connectionStringName = "connectionStringName";
        private const string defaultConnectionStringName = "DefaultSqlWorkflowInstanceStoreConnectionString";
        private const string hostLockRenewalPeriodParameter = "hostLockRenewalPeriod";
        private const string instanceCompletionAction = "instanceCompletionAction";
        private const string instanceEncodingOption = "instanceEncodingOption";
        private const string instanceLockedExceptionAction = "instanceLockedExceptionAction";
        private const string maxConnectionRetries = "maxConnectionRetries";
        private const string runnableInstancesDetectionPeriodParameter = "runnableInstancesDetectionPeriod";

        protected internal override object CreateBehavior()
        {
            string connectionString;
            bool flag = false;
            if (string.IsNullOrEmpty(this.ConnectionString) && string.IsNullOrEmpty(this.ConnectionStringName))
            {
                flag = true;
            }
            if (!string.IsNullOrEmpty(this.ConnectionString) && !string.IsNullOrEmpty(this.ConnectionStringName))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceException(System.ServiceModel.Activities.SR.CannotSpecifyBothConnectionStringAndName));
            }
            if (!string.IsNullOrEmpty(this.ConnectionStringName) || flag)
            {
                string str2 = flag ? "DefaultSqlWorkflowInstanceStoreConnectionString" : this.ConnectionStringName;
                ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[str2];
                if (settings == null)
                {
                    if (flag)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InstancePersistenceException(System.ServiceModel.Activities.SR.MustSpecifyConnectionStringOrName));
                    }
                    throw System.ServiceModel.Activities.FxTrace.Exception.Argument("connectionStringName", System.ServiceModel.Activities.SR.ConnectionStringNameWrong(this.ConnectionStringName));
                }
                connectionString = settings.ConnectionString;
            }
            else
            {
                connectionString = this.ConnectionString;
            }
            return new SqlWorkflowInstanceStoreBehavior { ConnectionString = connectionString, HostLockRenewalPeriod = this.HostLockRenewalPeriod, InstanceEncodingOption = this.InstanceEncodingOption, InstanceCompletionAction = this.InstanceCompletionAction, InstanceLockedExceptionAction = this.InstanceLockedExceptionAction, RunnableInstancesDetectionPeriod = this.RunnableInstancesDetectionPeriod, MaxConnectionRetries = this.MaxConnectionRetries };
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(SqlWorkflowInstanceStoreBehavior);
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("connectionString", IsRequired=false)]
        public string ConnectionString
        {
            get
            {
                return (string) base["connectionString"];
            }
            set
            {
                base["connectionString"] = value;
            }
        }

        [StringValidator(MinLength=0), ConfigurationProperty("connectionStringName", IsRequired=false)]
        public string ConnectionStringName
        {
            get
            {
                return (string) base["connectionStringName"];
            }
            set
            {
                base["connectionStringName"] = value;
            }
        }

        [ConfigurationProperty("hostLockRenewalPeriod", IsRequired=false, DefaultValue="00:00:30.0"), PositiveTimeSpanValidator, TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan HostLockRenewalPeriod
        {
            get
            {
                return (TimeSpan) base["hostLockRenewalPeriod"];
            }
            set
            {
                base["hostLockRenewalPeriod"] = value;
            }
        }

        [ConfigurationProperty("instanceCompletionAction", IsRequired=false, DefaultValue=1)]
        public System.Activities.DurableInstancing.InstanceCompletionAction InstanceCompletionAction
        {
            get
            {
                return (System.Activities.DurableInstancing.InstanceCompletionAction) base["instanceCompletionAction"];
            }
            set
            {
                base["instanceCompletionAction"] = value;
            }
        }

        [ConfigurationProperty("instanceEncodingOption", IsRequired=false, DefaultValue=1)]
        public System.Activities.DurableInstancing.InstanceEncodingOption InstanceEncodingOption
        {
            get
            {
                return (System.Activities.DurableInstancing.InstanceEncodingOption) base["instanceEncodingOption"];
            }
            set
            {
                base["instanceEncodingOption"] = value;
            }
        }

        [ConfigurationProperty("instanceLockedExceptionAction", IsRequired=false, DefaultValue=0)]
        public System.Activities.DurableInstancing.InstanceLockedExceptionAction InstanceLockedExceptionAction
        {
            get
            {
                return (System.Activities.DurableInstancing.InstanceLockedExceptionAction) base["instanceLockedExceptionAction"];
            }
            set
            {
                base["instanceLockedExceptionAction"] = value;
            }
        }

        [ConfigurationProperty("maxConnectionRetries", IsRequired=false, DefaultValue=4), IntegerValidator(MinValue=0)]
        public int MaxConnectionRetries
        {
            get
            {
                return (int) base["maxConnectionRetries"];
            }
            set
            {
                base["maxConnectionRetries"] = value;
            }
        }

        [ConfigurationProperty("runnableInstancesDetectionPeriod", IsRequired=false, DefaultValue="00:00:05.0"), TypeConverter(typeof(TimeSpanOrInfiniteConverter)), PositiveTimeSpanValidator]
        public TimeSpan RunnableInstancesDetectionPeriod
        {
            get
            {
                return (TimeSpan) base["runnableInstancesDetectionPeriod"];
            }
            set
            {
                base["runnableInstancesDetectionPeriod"] = value;
            }
        }
    }
}

