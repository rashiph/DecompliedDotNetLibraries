namespace System.Workflow.Activities
{
    using System;

    [AttributeUsage(AttributeTargets.Event | AttributeTargets.Method, AllowMultiple=false)]
    public sealed class CorrelationInitializerAttribute : Attribute
    {
    }
}

