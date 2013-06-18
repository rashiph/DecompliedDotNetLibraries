namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class ExtractCollection : List<TrackingExtract>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExtractCollection()
        {
        }

        public ExtractCollection(IEnumerable<TrackingExtract> extracts)
        {
            if (extracts == null)
            {
                throw new ArgumentNullException("extracts");
            }
            base.AddRange(extracts);
        }
    }
}

