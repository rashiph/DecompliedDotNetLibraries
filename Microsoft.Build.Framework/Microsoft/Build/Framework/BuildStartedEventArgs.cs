namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    public class BuildStartedEventArgs : BuildStatusEventArgs
    {
        private IDictionary<string, string> environmentOnBuildStart;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected BuildStartedEventArgs()
        {
        }

        public BuildStartedEventArgs(string message, string helpKeyword) : this(message, helpKeyword, DateTime.UtcNow)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public BuildStartedEventArgs(string message, string helpKeyword, DateTime eventTimestamp) : this(message, helpKeyword, eventTimestamp, null)
        {
        }

        public BuildStartedEventArgs(string message, string helpKeyword, IDictionary<string, string> environmentOfBuild) : this(message, helpKeyword, DateTime.UtcNow)
        {
            this.environmentOnBuildStart = environmentOfBuild;
        }

        public BuildStartedEventArgs(string message, string helpKeyword, DateTime eventTimestamp, params object[] messageArgs) : base(message, helpKeyword, "MSBuild", eventTimestamp, messageArgs)
        {
        }

        public IDictionary<string, string> BuildEnvironment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.environmentOnBuildStart;
            }
        }
    }
}

