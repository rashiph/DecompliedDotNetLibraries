namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class TrackingAnnotationCollection : List<string>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TrackingAnnotationCollection()
        {
        }

        public TrackingAnnotationCollection(IEnumerable<string> annotations)
        {
            if (annotations == null)
            {
                throw new ArgumentNullException("annotations");
            }
            base.AddRange(annotations);
        }
    }
}

