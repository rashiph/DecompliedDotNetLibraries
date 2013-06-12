namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManagementEnumeratorAttribute : ManagementNewInstanceAttribute
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

