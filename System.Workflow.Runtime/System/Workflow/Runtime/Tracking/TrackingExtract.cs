namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public abstract class TrackingExtract
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingExtract()
        {
        }

        internal abstract void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items);

        public abstract TrackingAnnotationCollection Annotations { get; }

        public abstract string Member { get; set; }
    }
}

