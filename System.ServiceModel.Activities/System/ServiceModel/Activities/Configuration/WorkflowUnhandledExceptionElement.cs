namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public sealed class WorkflowUnhandledExceptionElement : BehaviorExtensionElement
    {
        private const string action = "action";
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            return new WorkflowUnhandledExceptionBehavior { Action = this.Action };
        }

        [ConfigurationProperty("action", DefaultValue=3), ServiceModelEnumValidator(typeof(WorkflowUnhandledExceptionActionHelper))]
        public WorkflowUnhandledExceptionAction Action
        {
            get
            {
                return (WorkflowUnhandledExceptionAction) base["action"];
            }
            set
            {
                base["action"] = value;
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(WorkflowUnhandledExceptionBehavior);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("action", typeof(WorkflowUnhandledExceptionAction), WorkflowUnhandledExceptionAction.AbandonAndSuspend, null, new ServiceModelEnumValidator(typeof(WorkflowUnhandledExceptionActionHelper)), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

