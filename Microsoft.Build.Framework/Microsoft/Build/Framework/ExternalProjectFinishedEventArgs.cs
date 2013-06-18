namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [Serializable]
    public class ExternalProjectFinishedEventArgs : CustomBuildEventArgs
    {
        private string projectFile;
        private bool succeeded;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ExternalProjectFinishedEventArgs()
        {
        }

        public ExternalProjectFinishedEventArgs(string message, string helpKeyword, string senderName, string projectFile, bool succeeded) : this(message, helpKeyword, senderName, projectFile, succeeded, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalProjectFinishedEventArgs(string message, string helpKeyword, string senderName, string projectFile, bool succeeded, DateTime eventTimestamp) : base(message, helpKeyword, senderName, eventTimestamp)
        {
            this.projectFile = projectFile;
            this.succeeded = succeeded;
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

