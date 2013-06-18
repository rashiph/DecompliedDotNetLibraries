namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class UserTrackPointCollection : List<UserTrackPoint>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UserTrackPointCollection()
        {
        }

        public UserTrackPointCollection(IEnumerable<UserTrackPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            base.AddRange(points);
        }
    }
}

