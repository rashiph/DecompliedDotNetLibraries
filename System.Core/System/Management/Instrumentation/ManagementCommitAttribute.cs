namespace System.Management.Instrumentation
{
    using System;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Method), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class ManagementCommitAttribute : ManagementMemberAttribute
    {
    }
}

