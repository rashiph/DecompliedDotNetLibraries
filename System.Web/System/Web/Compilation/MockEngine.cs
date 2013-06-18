namespace System.Web.Compilation
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class MockEngine : IBuildEngine
    {
        private List<CustomBuildEventArgs> customEvents = new List<CustomBuildEventArgs>();
        private List<BuildErrorEventArgs> errors = new List<BuildErrorEventArgs>();
        private List<BuildMessageEventArgs> messages = new List<BuildMessageEventArgs>();
        private List<BuildWarningEventArgs> warnings = new List<BuildWarningEventArgs>();

        internal MockEngine()
        {
        }

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs)
        {
            throw new NotImplementedException();
        }

        public virtual void LogCustomEvent(CustomBuildEventArgs eventArgs)
        {
            this.customEvents.Add(eventArgs);
        }

        public virtual void LogErrorEvent(BuildErrorEventArgs eventArgs)
        {
            this.errors.Add(eventArgs);
        }

        public virtual void LogMessageEvent(BuildMessageEventArgs eventArgs)
        {
            this.messages.Add(eventArgs);
        }

        public virtual void LogWarningEvent(BuildWarningEventArgs eventArgs)
        {
            this.warnings.Add(eventArgs);
        }

        public int ColumnNumberOfTaskNode
        {
            get
            {
                return 0;
            }
        }

        public bool ContinueOnError
        {
            get
            {
                return false;
            }
        }

        internal ICollection<CustomBuildEventArgs> CustomEvents
        {
            get
            {
                return this.customEvents;
            }
        }

        internal ICollection<BuildErrorEventArgs> Errors
        {
            get
            {
                return this.errors;
            }
        }

        public int LineNumberOfTaskNode
        {
            get
            {
                return 0;
            }
        }

        internal ICollection<BuildMessageEventArgs> Messages
        {
            get
            {
                return this.messages;
            }
        }

        public string ProjectFileOfTaskNode
        {
            get
            {
                return string.Empty;
            }
        }

        internal ICollection<BuildWarningEventArgs> Warnings
        {
            get
            {
                return this.warnings;
            }
        }
    }
}

