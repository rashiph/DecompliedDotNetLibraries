namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [Serializable]
    internal class CAGChildStats
    {
        internal int ExecutedCount;
        internal CAGChildState State;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CAGChildStats()
        {
        }
    }
}

