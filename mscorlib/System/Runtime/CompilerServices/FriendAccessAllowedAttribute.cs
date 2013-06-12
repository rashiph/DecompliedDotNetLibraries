namespace System.Runtime.CompilerServices
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false, Inherited=false), FriendAccessAllowed]
    internal sealed class FriendAccessAllowedAttribute : Attribute
    {
    }
}

