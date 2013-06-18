namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel;

    internal class CompositeActivityTypeDescriptorProvider : TypeDescriptionProvider
    {
        public CompositeActivityTypeDescriptorProvider() : base(TypeDescriptor.GetProvider(typeof(CompositeActivity)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new CompositeActivityTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
        }
    }
}

