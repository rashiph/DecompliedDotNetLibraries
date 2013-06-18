namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [Serializable]
    public class TaskCommandLineEventArgs : BuildMessageEventArgs
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TaskCommandLineEventArgs()
        {
        }

        public TaskCommandLineEventArgs(string commandLine, string taskName, MessageImportance importance) : this(commandLine, taskName, importance, DateTime.UtcNow)
        {
        }

        public TaskCommandLineEventArgs(string commandLine, string taskName, MessageImportance importance, DateTime eventTimestamp) : base(commandLine, null, taskName, importance, eventTimestamp)
        {
        }

        public string CommandLine
        {
            get
            {
                return this.Message;
            }
        }

        public string TaskName
        {
            get
            {
                return base.SenderName;
            }
        }
    }
}

