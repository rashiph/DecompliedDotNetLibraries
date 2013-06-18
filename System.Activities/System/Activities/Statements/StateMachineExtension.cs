namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;

    internal class StateMachineExtension : IWorkflowInstanceExtension
    {
        private WorkflowInstanceProxy instance;

        public IEnumerable<object> GetAdditionalExtensions()
        {
            return null;
        }

        public void ResumeBookmark(Bookmark bookmark)
        {
            this.instance.BeginResumeBookmark(bookmark, null, null, null);
        }

        public void SetInstance(WorkflowInstanceProxy instance)
        {
            this.instance = instance;
        }
    }
}

