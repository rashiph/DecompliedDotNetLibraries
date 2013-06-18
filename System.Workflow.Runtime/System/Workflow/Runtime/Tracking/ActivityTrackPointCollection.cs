namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class ActivityTrackPointCollection : List<ActivityTrackPoint>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ActivityTrackPointCollection()
        {
        }

        public ActivityTrackPointCollection(IEnumerable<ActivityTrackPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            base.AddRange(points);
        }
    }
}

