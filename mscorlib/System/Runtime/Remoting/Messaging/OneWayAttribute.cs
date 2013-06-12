namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Runtime.InteropServices;

    [AttributeUsage(AttributeTargets.Method), ComVisible(true)]
    public class OneWayAttribute : Attribute
    {
    }
}

