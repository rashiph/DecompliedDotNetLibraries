namespace Microsoft.Build.Framework
{
    using System;
    using System.Runtime;

    [Serializable]
    public abstract class CustomBuildEventArgs : LazyFormattedBuildEventArgs
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CustomBuildEventArgs()
        {
        }

        protected CustomBuildEventArgs(string message, string helpKeyword, string senderName) : this(message, helpKeyword, senderName, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CustomBuildEventArgs(string message, string helpKeyword, string senderName, DateTime eventTimestamp) : this(message, helpKeyword, senderName, eventTimestamp, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected CustomBuildEventArgs(string message, string helpKeyword, string senderName, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, senderName, eventTimestamp, messageArgs)
        {
        }
    }
}

