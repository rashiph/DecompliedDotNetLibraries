namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManagementProbeAttribute : ManagementMemberAttribute
    {
        private Type _schema;

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

