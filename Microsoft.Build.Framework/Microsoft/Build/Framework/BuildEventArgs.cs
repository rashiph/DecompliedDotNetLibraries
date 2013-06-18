namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Threading;

    [Serializable]
    public abstract class BuildEventArgs : EventArgs
    {
        [OptionalField(VersionAdded=2)]
        private Microsoft.Build.Framework.BuildEventContext buildEventContext;
        private string helpKeyword;
        private string message;
        private string senderName;
        private int threadId;
        private DateTime timestamp;

        protected BuildEventArgs() : this(null, null, null, DateTime.UtcNow)
        {
        }

        protected BuildEventArgs(string message, string helpKeyword, string senderName) : this(message, helpKeyword, senderName, DateTime.UtcNow)
        {
        }

        protected BuildEventArgs(string message, string helpKeyword, string senderName, DateTime eventTimestamp)
        {
            this.message = message;
            this.helpKeyword = helpKeyword;
            this.senderName = senderName;
            this.timestamp = eventTimestamp;
            this.threadId = Thread.CurrentThread.GetHashCode();
        }

        internal virtual void CreateFromStream(BinaryReader reader)
        {
            if (reader.ReadByte() == 0)
            {
                this.message = null;
            }
            else
            {
                this.message = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.helpKeyword = null;
            }
            else
            {
                this.helpKeyword = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.senderName = null;
            }
            else
            {
                this.senderName = reader.ReadString();
            }
            DateTimeKind kind = (DateTimeKind) reader.ReadInt32();
            this.timestamp = new DateTime(reader.ReadInt64(), kind);
            this.threadId = reader.ReadInt32();
            if (reader.ReadByte() == 0)
            {
                this.buildEventContext = null;
            }
            else
            {
                int nodeId = reader.ReadInt32();
                int submissionId = reader.ReadInt32();
                int projectInstanceId = reader.ReadInt32();
                int projectContextId = reader.ReadInt32();
                int targetId = reader.ReadInt32();
                int taskId = reader.ReadInt32();
                this.buildEventContext = new Microsoft.Build.Framework.BuildEventContext(submissionId, nodeId, projectInstanceId, projectContextId, targetId, taskId);
            }
        }

        [OnDeserialized]
        private void SetBuildEventContextDefaultAfterSerialization(StreamingContext sc)
        {
            if (this.buildEventContext == null)
            {
                this.buildEventContext = new Microsoft.Build.Framework.BuildEventContext(-2, -1, -2, -1);
            }
        }

        [OnDeserializing]
        private void SetBuildEventContextDefaultBeforeSerialization(StreamingContext sc)
        {
            this.buildEventContext = null;
        }

        internal virtual void WriteToStream(BinaryWriter writer)
        {
            if (this.message == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.message);
            }
            if (this.helpKeyword == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.helpKeyword);
            }
            if (this.senderName == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.senderName);
            }
            writer.Write((int) this.timestamp.Kind);
            writer.Write(this.timestamp.Ticks);
            writer.Write(this.threadId);
            if (this.buildEventContext == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.buildEventContext.NodeId);
                writer.Write(this.buildEventContext.SubmissionId);
                writer.Write(this.buildEventContext.ProjectInstanceId);
                writer.Write(this.buildEventContext.ProjectContextId);
                writer.Write(this.buildEventContext.TargetId);
                writer.Write(this.buildEventContext.TaskId);
            }
        }

        public Microsoft.Build.Framework.BuildEventContext BuildEventContext
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buildEventContext;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.buildEventContext = value;
            }
        }

        public string HelpKeyword
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpKeyword;
            }
        }

        public virtual string Message
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.message;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected set
            {
                this.message = value;
            }
        }

        public string SenderName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.senderName;
            }
        }

        public int ThreadId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.threadId;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                if (this.timestamp.Kind == DateTimeKind.Utc)
                {
                    this.timestamp = this.timestamp.ToLocalTime();
                }
                return this.timestamp;
            }
        }
    }
}

