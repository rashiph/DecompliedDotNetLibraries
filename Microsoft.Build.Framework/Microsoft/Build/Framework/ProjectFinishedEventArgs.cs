namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class ProjectFinishedEventArgs : BuildStatusEventArgs
    {
        private string projectFile;
        private bool succeeded;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProjectFinishedEventArgs()
        {
        }

        public ProjectFinishedEventArgs(string message, string helpKeyword, string projectFile, bool succeeded) : this(message, helpKeyword, projectFile, succeeded, DateTime.UtcNow)
        {
        }

        public ProjectFinishedEventArgs(string message, string helpKeyword, string projectFile, bool succeeded, DateTime eventTimestamp) : base(message, helpKeyword, "MSBuild", eventTimestamp)
        {
            this.projectFile = projectFile;
            this.succeeded = succeeded;
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
    }
}

