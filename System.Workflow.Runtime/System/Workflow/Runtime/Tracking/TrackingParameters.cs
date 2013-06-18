namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class TrackingParameters
    {
        private IList<string> _activityCallPath;
        private Guid _callerContextGuid;
        private Guid _callerInstanceId;
        private Guid _callerParentContextGuid;
        private Guid _contextGuid;
        private Guid _instanceId;
        private Activity _rootActivity;
        private Type _workflowType;

        private TrackingParameters()
        {
            this._instanceId = Guid.Empty;
            this._callerInstanceId = Guid.Empty;
            this._contextGuid = Guid.Empty;
            this._callerContextGuid = Guid.Empty;
            this._callerParentContextGuid = Guid.Empty;
        }

        public TrackingParameters(Guid instanceId, Type workflowType, Activity rootActivity, IList<string> callPath, Guid callerInstanceId, Guid contextGuid, Guid callerContextGuid, Guid callerParentContextGuid)
        {
            this._instanceId = Guid.Empty;
            this._callerInstanceId = Guid.Empty;
            this._contextGuid = Guid.Empty;
            this._callerContextGuid = Guid.Empty;
            this._callerParentContextGuid = Guid.Empty;
            this._instanceId = instanceId;
            this._workflowType = workflowType;
            this._activityCallPath = callPath;
            this._callerInstanceId = callerInstanceId;
            this._contextGuid = contextGuid;
            this._callerContextGuid = callerContextGuid;
            this._callerParentContextGuid = callerParentContextGuid;
            this._rootActivity = rootActivity;
        }

        public Guid CallerContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._callerContextGuid;
            }
        }

        public Guid CallerInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._callerInstanceId;
            }
        }

        public Guid CallerParentContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._callerParentContextGuid;
            }
        }

        public IList<string> CallPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activityCallPath;
            }
        }

        public Guid ContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._contextGuid;
            }
        }

        public Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._instanceId;
            }
        }

        public Activity RootActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._rootActivity;
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

