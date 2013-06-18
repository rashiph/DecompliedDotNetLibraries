namespace System.Activities.Debugger
{
    using System.Activities;

    public interface IDebuggableWorkflowTree
    {
        Activity GetWorkflowRoot();
    }
}

