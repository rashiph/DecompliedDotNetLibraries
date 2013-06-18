namespace System.Workflow.Runtime
{
    using System;

    public interface IWorkBatch
    {
        void Add(IPendingWork work, object workItem);
    }
}

