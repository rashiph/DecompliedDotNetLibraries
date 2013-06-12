namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.All), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class ManagementMemberAttribute : Attribute
    {
        private string _Name;

        protected ManagementMemberAttribute()
        {
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
            }
        }
    }
}

