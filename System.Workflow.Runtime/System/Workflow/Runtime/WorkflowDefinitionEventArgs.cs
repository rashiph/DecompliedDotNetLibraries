namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    internal sealed class WorkflowDefinitionEventArgs : EventArgs
    {
        private Type _workflowType;
        private byte[] _xomlHashCode;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowDefinitionEventArgs(Type scheduleType)
        {
            this._workflowType = scheduleType;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal WorkflowDefinitionEventArgs(byte[] scheduleDefHash)
        {
            this._xomlHashCode = scheduleDefHash;
        }

        public byte[] WorkflowDefinitionHashCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._xomlHashCode;
            }
        }

        public Type WorkflowType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._workflowType;
            }
        }
    }
}

