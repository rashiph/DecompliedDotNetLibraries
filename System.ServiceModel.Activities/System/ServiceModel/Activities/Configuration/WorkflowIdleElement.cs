namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class WorkflowIdleElement : BehaviorExtensionElement
    {
        private ConfigurationPropertyCollection properties;
        private const string TimeToPersistString = "timeToPersist";
        private const string TimeToUnloadString = "timeToUnload";

        protected internal override object CreateBehavior()
        {
            return new WorkflowIdleBehavior { TimeToPersist = this.TimeToPersist, TimeToUnload = this.TimeToUnload };
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(WorkflowIdleBehavior);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("timeToPersist", typeof(TimeSpan), TimeSpan.MaxValue, new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                    propertys.Add(new ConfigurationProperty("timeToUnload", typeof(TimeSpan), TimeSpan.Parse("00:01:00", CultureInfo.InvariantCulture), new TimeSpanOrInfiniteConverter(), new TimeSpanOrInfiniteValidator(TimeSpan.Zero, TimeSpan.MaxValue), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }

        [ServiceModelTimeSpanValidator(MinValueString="00:00:00"), ConfigurationProperty("timeToPersist", DefaultValue="Infinite"), TypeConverter(typeof(TimeSpanOrInfiniteConverter))]
        public TimeSpan TimeToPersist
        {
            get
            {
                return (TimeSpan) base["timeToPersist"];
            }
            set
            {
                base["timeToPersist"] = value;
            }
        }

        [TypeConverter(typeof(TimeSpanOrInfiniteConverter)), ConfigurationProperty("timeToUnload", DefaultValue="00:01:00"), ServiceModelTimeSpanValidator(MinValueString="00:00:00")]
        public TimeSpan TimeToUnload
        {
            get
            {
                return (TimeSpan) base["timeToUnload"];
            }
            set
            {
                base["timeToUnload"] = value;
            }
        }
    }
}

