namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [Serializable]
    public abstract class TrackingCondition
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TrackingCondition()
        {
        }

        internal abstract bool Match(object obj);

        public abstract string Member { get; set; }

        public abstract ComparisonOperator Operator { get; set; }

        public abstract string Value { get; set; }
    }
}

