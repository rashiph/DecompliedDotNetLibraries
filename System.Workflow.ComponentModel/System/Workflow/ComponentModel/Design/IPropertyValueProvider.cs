namespace System.Workflow.ComponentModel.Design
{
    using System.Collections;
    using System.ComponentModel;

    internal interface IPropertyValueProvider
    {
        ICollection GetPropertyValues(ITypeDescriptorContext typeDescriptorContext);
    }
}

