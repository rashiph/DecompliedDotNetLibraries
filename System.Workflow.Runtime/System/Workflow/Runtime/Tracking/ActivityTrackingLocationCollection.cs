namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class ActivityTrackingLocationCollection : List<ActivityTrackingLocation>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityTrackingLocationCollection()
        {
        }

        public ActivityTrackingLocationCollection(IEnumerable<ActivityTrackingLocation> locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }
            base.AddRange(locations);
        }
    }
}

