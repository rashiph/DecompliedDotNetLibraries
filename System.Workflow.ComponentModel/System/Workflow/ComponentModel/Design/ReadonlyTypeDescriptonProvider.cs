namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    internal sealed class ReadonlyTypeDescriptonProvider : TypeDescriptionProvider
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ReadonlyTypeDescriptonProvider(TypeDescriptionProvider realProvider) : base(realProvider)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type type, object instance)
        {
            return new ReadonlyTypeDescriptor(base.GetTypeDescriptor(type, instance));
        }
    }
}

