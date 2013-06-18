namespace System.Workflow.ComponentModel.Design
{
    using System;

    public interface ITypeFilterProvider
    {
        bool CanFilterType(Type type, bool throwOnError);

        string FilterDescription { get; }
    }
}

