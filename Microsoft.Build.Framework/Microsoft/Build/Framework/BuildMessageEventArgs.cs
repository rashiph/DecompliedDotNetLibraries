namespace Microsoft.Build.Framework
{
    using System;
    using System.IO;
    using System.Runtime;

    [Serializable]
    public class BuildMessageEventArgs : LazyFormattedBuildEventArgs
    {
        private MessageImportance importance;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected BuildMessageEventArgs()
        {
        }

        public BuildMessageEventArgs(string message, string helpKeyword, string senderName, MessageImportance importance) : this(message, helpKeyword, senderName, importance, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildMessageEventArgs(string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp) : this(message, helpKeyword, senderName, importance, eventTimestamp, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildMessageEventArgs(string message, string helpKeyword, string senderName, MessageImportance importance, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, senderName, eventTimestamp, messageArgs)
        {
            this.importance = importance;
        }

        internal override void CreateFromStream(BinaryReader reader)
        {
            base.CreateFromStream(reader);
            this.importance = (MessageImportance) reader.ReadInt32();
        }

        internal override void WriteToStream(BinaryWriter writer)
        {
            base.WriteToStream(writer);
            writer.Write((int) this.importance);
        }

        public MessageImportance Importance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.importance;
            }
        }
    }
}

