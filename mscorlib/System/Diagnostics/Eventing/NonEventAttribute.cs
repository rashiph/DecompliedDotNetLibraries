namespace System.Diagnostics.Eventing
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Method), FriendAccessAllowed]
    internal sealed class NonEventAttribute : Attribute
    {
    }
}

