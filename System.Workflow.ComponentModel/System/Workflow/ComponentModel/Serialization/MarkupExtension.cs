namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Runtime;

    public abstract class MarkupExtension
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected MarkupExtension()
        {
        }

        public abstract object ProvideValue(IServiceProvider provider);
    }
}

