namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.Activities.Common;

    internal sealed class WebServiceUrlPropertyDescriptor : DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WebServiceUrlPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor pd) : base(serviceProvider, pd)
        {
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
    }
}

