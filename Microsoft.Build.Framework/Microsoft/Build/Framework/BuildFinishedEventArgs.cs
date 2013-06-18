namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class BuildFinishedEventArgs : BuildStatusEventArgs
    {
        private bool succeeded;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected BuildFinishedEventArgs()
        {
        }

        public BuildFinishedEventArgs(string message, string helpKeyword, bool succeeded) : this(message, helpKeyword, succeeded, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildFinishedEventArgs(string message, string helpKeyword, bool succeeded, DateTime eventTimestamp) : this(message, helpKeyword, succeeded, eventTimestamp, null)
        {
        }

        public BuildFinishedEventArgs(string message, string helpKeyword, bool succeeded, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, "MSBuild", eventTimestamp, messageArgs)
        {
            this.succeeded = succeeded;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            this.succeeded = reader.ReadBoolean();
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            writer.Write(this.succeeded);
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

