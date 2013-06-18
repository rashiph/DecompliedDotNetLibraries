namespace System.Workflow.ComponentModel
{
    using System;

    [Flags]
    public enum DependencyPropertyOptions : byte
    {
        Default = 1,
        DelegateProperty = 0x20,
        Metadata = 8,
        NonSerialized = 0x10,
        Optional = 4,
        ReadOnly = 2
    }
}

