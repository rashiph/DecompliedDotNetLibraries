namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    internal sealed class ActivityBindPathPropertyDescriptor : DynamicPropertyDescriptor
    {
        private ITypeDescriptorContext context;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityBindPathPropertyDescriptor(ITypeDescriptorContext context, PropertyDescriptor realPropertyDescriptor) : base(context, realPropertyDescriptor)
        {
            this.context = context;
        }

        internal ITypeDescriptorContext OuterPropertyContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.context;
            }
        }
    }
}

