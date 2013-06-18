namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Configuration;

    public class WorkflowInstanceManagementElement : BehaviorExtensionElement
    {
        private const string authorizedWindowsGroup = "authorizedWindowsGroup";
        private ConfigurationPropertyCollection properties;

        protected internal override object CreateBehavior()
        {
            string authorizedWindowsGroup;
            if (!string.IsNullOrEmpty(this.AuthorizedWindowsGroup))
            {
                authorizedWindowsGroup = this.AuthorizedWindowsGroup;
            }
            else
            {
                authorizedWindowsGroup = WorkflowInstanceManagementBehavior.GetDefaultBuiltinAdministratorsGroup();
            }
            return new WorkflowInstanceManagementBehavior { WindowsGroup = authorizedWindowsGroup };
        }

        [ConfigurationProperty("authorizedWindowsGroup", IsRequired=false), StringValidator(MinLength=0)]
        public string AuthorizedWindowsGroup
        {
            get
            {
                return (string) base["authorizedWindowsGroup"];
            }
            set
            {
                base["authorizedWindowsGroup"] = value;
            }
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(WorkflowInstanceManagementBehavior);
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                if (this.properties == null)
                {
                    ConfigurationPropertyCollection propertys = new ConfigurationPropertyCollection();
                    propertys.Add(new ConfigurationProperty("authorizedWindowsGroup", typeof(string), string.Empty, null, new StringValidator(0, 0x7fffffff, null), ConfigurationPropertyOptions.None));
                    this.properties = propertys;
                }
                return this.properties;
            }
        }
    }
}

