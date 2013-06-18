namespace System.Workflow.ComponentModel
{
    using System;
    using System.Workflow.ComponentModel.Compiler;

    public interface IDynamicPropertyTypeProvider
    {
        AccessTypes GetAccessType(IServiceProvider serviceProvider, string propertyName);
        Type GetPropertyType(IServiceProvider serviceProvider, string propertyName);
    }
}

