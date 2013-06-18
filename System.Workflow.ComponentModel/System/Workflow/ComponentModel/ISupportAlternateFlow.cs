namespace System.Workflow.ComponentModel
{
    using System.Collections.Generic;

    internal interface ISupportAlternateFlow
    {
        IList<Activity> AlternateFlowActivities { get; }
    }
}

