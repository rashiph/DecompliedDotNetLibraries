namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class TaskStartedEventArgs : BuildStatusEventArgs
    {
        private string projectFile;
        private string taskFile;
        private string taskName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TaskStartedEventArgs()
        {
        }

        public TaskStartedEventArgs(string message, string helpKeyword, string projectFile, string taskFile, string taskName) : this(message, helpKeyword, projectFile, taskFile, taskName, DateTime.UtcNow)
        {
        }

        public TaskStartedEventArgs(string message, string helpKeyword, string projectFile, string taskFile, string taskName, DateTime eventTimestamp) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.taskName = taskName;
            this.projectFile = projectFile;
            this.taskFile = taskFile;
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
        }

        public string ProjectFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectFile;
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

