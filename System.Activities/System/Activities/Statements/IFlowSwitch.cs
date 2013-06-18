namespace System.Activities.Statements
{
    using System;
    using System.Activities;

    internal interface IFlowSwitch
    {
        bool Execute(NativeActivityContext context, Flowchart parent);
        FlowNode GetNextNode(object value);
    }
}

