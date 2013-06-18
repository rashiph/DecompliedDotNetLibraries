namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class TaskFinishedEventArgs : BuildStatusEventArgs
    {
        private string projectFile;
        private bool succeeded;
        private string taskFile;
        private string taskName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TaskFinishedEventArgs()
        {
        }

        public TaskFinishedEventArgs(string message, string helpKeyword, string projectFile, string taskFile, string taskName, bool succeeded) : this(message, helpKeyword, projectFile, taskFile, taskName, succeeded, DateTime.UtcNow)
        {
        }

        public TaskFinishedEventArgs(string message, string helpKeyword, string projectFile, string taskFile, string taskName, bool succeeded, DateTime eventTimestamp) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.taskName = taskName;
            this.taskFile = taskFile;
            this.succeeded = succeeded;
            this.projectFile = projectFile;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            if (reader.ReadByte() == 0)
            {
                this.taskName = null;
            }
            else
            {
                this.taskName = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.projectFile = null;
            }
            else
            {
                this.projectFile = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.taskFile = null;
            }
            else
            {
                this.taskFile = reader.ReadString();
            }
            this.succeeded = reader.ReadBoolean();
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            if (this.taskName == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.taskName);
            }
            if (this.projectFile == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.projectFile);
            }
            if (this.taskFile == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.taskFile);
            }
            writer.Write(this.succeeded);
        }

        public string ProjectFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectFile;
            }
        }

        public bool Succeeded
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.succeeded;
            }
        }

        public string TaskFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.taskFile;
            }
        }

        public string TaskName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.taskName;
            }
        }
    }
}

