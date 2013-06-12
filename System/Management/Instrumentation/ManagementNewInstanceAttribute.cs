namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple=false), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class ManagementNewInstanceAttribute : ManagementMemberAttribute
    {
        protected ManagementNewInstanceAttribute()
        {
        }
    }
}

