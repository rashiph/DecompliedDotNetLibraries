namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class UserTrackingLocationCollection : List<UserTrackingLocation>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UserTrackingLocationCollection()
        {
        }

        public UserTrackingLocationCollection(IEnumerable<UserTrackingLocation> locations)
        {
            if (locations == null)
            {
                throw new ArgumentNullException("locations");
            }
            base.AddRange(locations);
        }
    }
}

