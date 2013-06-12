namespace System.Runtime.ExceptionServices
{
    using System;

    [AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
    public sealed class HandleProcessCorruptedStateExceptionsAttribute : Attribute
    {
    }
}

