namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [Serializable]
    public sealed class InvokeWebServiceEventArgs : EventArgs
    {
        [NonSerialized]
        private object proxyInstance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvokeWebServiceEventArgs(object proxyInstance)
        {
            this.proxyInstance = proxyInstance;
        }

        public object WebServiceProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.proxyInstance;
            }
        }
    }
}

