namespace System.Management.Instrumentation
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method)]
    public class IgnoreMemberAttribute : Attribute
    {
    }
}

