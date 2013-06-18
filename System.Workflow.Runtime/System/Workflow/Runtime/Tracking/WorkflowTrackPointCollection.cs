namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class WorkflowTrackPointCollection : List<WorkflowTrackPoint>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowTrackPointCollection()
        {
        }

        public WorkflowTrackPointCollection(IEnumerable<WorkflowTrackPoint> points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }
            base.AddRange(points);
        }
    }
}

