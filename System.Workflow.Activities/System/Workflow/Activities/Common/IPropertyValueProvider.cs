namespace System.Workflow.Activities.Common
{
    using System.Collections;
    using System.ComponentModel;

    internal interface IPropertyValueProvider
    {
        ICollection GetPropertyValues(ITypeDescriptorContext typeDescriptorContext);
    }
}

