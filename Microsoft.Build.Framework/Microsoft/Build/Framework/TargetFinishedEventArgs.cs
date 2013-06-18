namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class TargetFinishedEventArgs : BuildStatusEventArgs
    {
        private string projectFile;
        private bool succeeded;
        private string targetFile;
        private string targetName;
        private IEnumerable targetOutputs;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TargetFinishedEventArgs()
        {
        }

        public TargetFinishedEventArgs(string message, string helpKeyword, string targetName, string projectFile, string targetFile, bool succeeded) : this(message, helpKeyword, targetName, projectFile, targetFile, succeeded, DateTime.UtcNow, null)
        {
        }

        public TargetFinishedEventArgs(string message, string helpKeyword, string targetName, string projectFile, string targetFile, bool succeeded, IEnumerable targetOutputs) : this(message, helpKeyword, targetName, projectFile, targetFile, succeeded, DateTime.UtcNow, targetOutputs)
        {
        }

        public TargetFinishedEventArgs(string message, string helpKeyword, string targetName, string projectFile, string targetFile, bool succeeded, DateTime eventTimestamp, IEnumerable targetOutputs) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.targetName = targetName;
            this.succeeded = succeeded;
            this.projectFile = projectFile;
            this.targetFile = targetFile;
            this.targetOutputs = targetOutputs;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
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
                this.targetName = null;
            }
            else
            {
                this.targetName = reader.ReadString();
            }
            this.succeeded = reader.ReadBoolean();
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
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
            if (this.targetName == null)
            {
                writer.Write((byte) 0);
            }
            else
            {
                writer.Write((byte) 1);
                writer.Write(this.targetName);
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

        public IEnumerable TargetOutputs
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetOutputs;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.targetOutputs = value;
            }
        }
    }
}

