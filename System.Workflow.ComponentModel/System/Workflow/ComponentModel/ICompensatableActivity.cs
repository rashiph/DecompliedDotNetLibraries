namespace System.Workflow.ComponentModel
{
    public interface ICompensatableActivity
    {
        ActivityExecutionStatus Compensate(ActivityExecutionContext executionContext);
    }
}

