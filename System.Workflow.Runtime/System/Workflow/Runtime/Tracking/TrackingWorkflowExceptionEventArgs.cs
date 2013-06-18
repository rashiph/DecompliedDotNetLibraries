namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;

    [Serializable]
    public class TrackingWorkflowExceptionEventArgs : EventArgs
    {
        private Guid _context;
        private string _currentPath;
        private System.Exception _e;
        private string _originalPath;
        private Guid _parentContext;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal TrackingWorkflowExceptionEventArgs(System.Exception exception, string currentPath, string originalPath, Guid contextGuid, Guid parentContextGuid)
        {
            this._e = exception;
            this._currentPath = currentPath;
            this._originalPath = originalPath;
            this._context = contextGuid;
            this._parentContext = parentContextGuid;
        }

        public Guid ContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._context;
            }
        }

        public string CurrentActivityPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._currentPath;
            }
        }

        public System.Exception Exception
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._e;
            }
        }

        public string OriginalActivityPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._originalPath;
            }
        }

        public Guid ParentContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._parentContext;
            }
        }
    }
}

