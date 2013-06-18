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
    internal sealed class IfElseToolboxItem : ActivityToolboxItem
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IfElseToolboxItem(Type type) : base(type)
        {
        }

        private IfElseToolboxItem(SerializationInfo info, StreamingContext context)
        {
            this.Deserialize(info, context);
        }

        protected override IComponent[] CreateComponentsCore(IDesignerHost designerHost)
        {
            CompositeActivity activity = new IfElseActivity {
                Activities = { new IfElseBranchActivity(), new IfElseBranchActivity() }
            };
            return new IComponent[] { activity };
        }
    }
}

