namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class TrackingConditionCollection : List<TrackingCondition>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TrackingConditionCollection()
        {
        }

        public TrackingConditionCollection(IEnumerable<TrackingCondition> conditions)
        {
            if (conditions == null)
            {
                throw new ArgumentNullException("conditions");
            }
            base.AddRange(conditions);
        }
    }
}

