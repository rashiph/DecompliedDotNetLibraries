namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRCategory("Standard"), SRDescription("EventDrivenActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), ActivityValidator(typeof(EventDrivenValidator)), Designer(typeof(EventDrivenDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(EventDrivenActivity), "Resources.EventDriven.png")]
    public sealed class EventDrivenActivity : SequenceActivity
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventDrivenActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventDrivenActivity(string name) : base(name)
        {
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public IEventActivity EventActivity
        {
            get
            {
                ReadOnlyCollection<Activity> enabledActivities = base.EnabledActivities;
                if (enabledActivities.Count == 0)
                {
                    return null;
                }
                return (enabledActivities[0] as IEventActivity);
            }
        }
    }
}

