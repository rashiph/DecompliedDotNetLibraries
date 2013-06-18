namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [ProvideProperty("WhenCondition", typeof(Activity)), ProvideProperty("UnlessCondition", typeof(Activity))]
    internal sealed class ConditionPropertyProviderExtender : IExtenderProvider
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ConditionPropertyProviderExtender()
        {
        }

        public bool CanExtend(object extendee)
        {
            return (((extendee != this) && (extendee is Activity)) && (((Activity) extendee).Parent is ConditionedActivityGroup));
        }

        [SRCategory("ConditionedActivityConditions"), SRDescription("WhenConditionDescr"), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityCondition GetWhenCondition(Activity activity)
        {
            if (activity.Parent is ConditionedActivityGroup)
            {
                return (activity.GetValue(ConditionedActivityGroup.WhenConditionProperty) as ActivityCondition);
            }
            return null;
        }

        [SRDescription("WhenConditionDescr"), SRCategory("ConditionedActivityConditions")]
        public void SetWhenCondition(Activity activity, ActivityCondition handler)
        {
            if (activity.Parent is ConditionedActivityGroup)
            {
                activity.SetValue(ConditionedActivityGroup.WhenConditionProperty, handler);
            }
        }
    }
}

