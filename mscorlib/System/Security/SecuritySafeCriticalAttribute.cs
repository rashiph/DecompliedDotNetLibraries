namespace System.Security
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class SecuritySafeCriticalAttribute : Attribute
    {
    }
}

