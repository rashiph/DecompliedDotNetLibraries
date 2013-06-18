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
    internal sealed class ListenToolboxItem : ActivityToolboxItem
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ListenToolboxItem(Type type) : base(type)
        {
        }

        private ListenToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity activity = new ListenActivity {
                Activities = { new EventDrivenActivity(), new EventDrivenActivity() }
            };
            return new IComponent[] { activity };
        }
    }
}

