namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [Serializable]
    public class ExternalProjectStartedEventArgs : CustomBuildEventArgs
    {
        private string projectFile;
        private string targetNames;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ExternalProjectStartedEventArgs()
        {
        }

        public ExternalProjectStartedEventArgs(string message, string helpKeyword, string senderName, string projectFile, string targetNames) : this(message, helpKeyword, senderName, projectFile, targetNames, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalProjectStartedEventArgs(string message, string helpKeyword, string senderName, string projectFile, string targetNames, DateTime eventTimestamp) : base(message, helpKeyword, senderName, eventTimestamp)
        {
            this.projectFile = projectFile;
            this.targetNames = targetNames;
        }

        public string ProjectFile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.projectFile;
            }
        }

        public string TargetNames
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetNames;
            }
        }
    }
}

