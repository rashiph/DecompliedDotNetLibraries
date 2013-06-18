namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class TargetStartedEventArgs : BuildStatusEventArgs
    {
        private string parentTarget;
        private string projectFile;
        private string targetFile;
        private string targetName;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TargetStartedEventArgs()
        {
        }

        public TargetStartedEventArgs(string message, string helpKeyword, string targetName, string projectFile, string targetFile) : this(message, helpKeyword, targetName, projectFile, targetFile, string.Empty, DateTime.UtcNow)
        {
        }

        public TargetStartedEventArgs(string message, string helpKeyword, string targetName, string projectFile, string targetFile, string parentTarget, DateTime eventTimestamp) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.targetName = targetName;
            this.projectFile = projectFile;
            this.targetFile = targetFile;
            this.parentTarget = parentTarget;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            if (reader.ReadByte() == 0)
            {
                this.targetName = null;
            }
            else
            {
                this.targetName = reader.ReadString();
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
                this.targetFile = null;
            }
            else
            {
                this.targetFile = reader.ReadString();
            }
            if (reader.ReadByte() == 0)
            {
                this.parentTarget = null;
            }
            else
            {
                this.parentTarget = reader.ReadString();
            }
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            if (this.targetName == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.targetName);
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
            if (this.targetFile == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.targetFile);
            }
            if (this.parentTarget == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.parentTarget);
            }
        }

        public string ParentTarget
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parentTarget;
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

        public string TargetFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetFile;
            }
        }

        public string TargetName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetName;
            }
        }
    }
}

