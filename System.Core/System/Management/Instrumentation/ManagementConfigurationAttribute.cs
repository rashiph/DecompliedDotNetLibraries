namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManagementConfigurationAttribute : ManagementMemberAttribute
    {
        private Type _schema;
        private ManagementConfigurationType updateMode = ManagementConfigurationType.Apply;

        public ManagementConfigurationType Mode
        {
            get
            {
                return this.updateMode;
            }
            set
            {
                this.updateMode = value;
            }
        }

        public Type Schema
        {
            get
            {
                return this._schema;
            }
            set
            {
                this._schema = value;
            }
        }
    }
}

