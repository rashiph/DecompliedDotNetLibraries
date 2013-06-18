namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [Serializable]
    public class BuildEventContext
    {
        public const int InvalidNodeId = -2;
        public const int InvalidProjectContextId = -2;
        public const int InvalidProjectInstanceId = -1;
        public const int InvalidSubmissionId = -1;
        public const int InvalidTargetId = -1;
        public const int InvalidTaskId = -1;
        private int nodeId;
        private int projectContextId;
        private int projectInstanceId;
        private int submissionId;
        private int targetId;
        private int taskId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildEventContext(int nodeId, int targetId, int projectContextId, int taskId) : this(-1, nodeId, -1, projectContextId, targetId, taskId)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildEventContext(int nodeId, int projectInstanceId, int projectContextId, int targetId, int taskId) : this(-1, nodeId, projectInstanceId, projectContextId, targetId, taskId)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildEventContext(int submissionId, int nodeId, int projectInstanceId, int projectContextId, int targetId, int taskId)
        {
            this.submissionId = submissionId;
            this.nodeId = nodeId;
            this.targetId = targetId;
            this.projectContextId = projectContextId;
            this.taskId = taskId;
            this.projectInstanceId = projectInstanceId;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            if (base.GetType() != obj.GetType())
            {
                return false;
            }
            return this.InternalEquals((BuildEventContext) obj);
        }

        public override int GetHashCode()
        {
            return (this.ProjectContextId + (this.NodeId << 0x18));
        }

        private bool InternalEquals(BuildEventContext buildEventContext)
        {
            return ((((this.nodeId == buildEventContext.NodeId) && (this.projectContextId == buildEventContext.ProjectContextId)) && (this.targetId == buildEventContext.TargetId)) && (this.taskId == buildEventContext.TaskId));
        }

        public static bool operator ==(BuildEventContext left, BuildEventContext right)
        {
            if (object.ReferenceEquals(left, right))
            {
                return true;
            }
            if (object.ReferenceEquals(left, null))
            {
                return false;
            }
            return left.Equals(right);
        }

        public static bool operator !=(BuildEventContext left, BuildEventContext right)
        {
            return !(left == right);
        }

        public long BuildRequestId
        {
            get
            {
                return ((this.nodeId << 0x20) + this.projectContextId);
            }
        }

        public static BuildEventContext Invalid
        {
            get
            {
                return new BuildEventContext(-2, -1, -2, -1);
            }
        }

        public int NodeId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.nodeId;
            }
        }

        public int ProjectContextId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectContextId;
            }
        }

        public int ProjectInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectInstanceId;
            }
        }

        public int SubmissionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.submissionId;
            }
        }

        public int TargetId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetId;
            }
        }

        public int TaskId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.taskId;
            }
        }
    }
}

