namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManagementNameAttribute : Attribute
    {
        private string _Name;

        public ManagementNameAttribute(string name)
        {
            this._Name = name;
        }

        public string Name
        {
            get
            {
                return this._Name;
            }
        }
    }
}

