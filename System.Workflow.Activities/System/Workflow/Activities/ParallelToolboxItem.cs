namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [Serializable]
    internal sealed class ParallelToolboxItem : ActivityToolboxItem
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ParallelToolboxItem(Type type) : base(type)
        {
        }

        private ParallelToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity activity = new ParallelActivity {
                Activities = { new SequenceActivity(), new SequenceActivity() }
            };
            return new IComponent[] { activity };
        }
    }
}

