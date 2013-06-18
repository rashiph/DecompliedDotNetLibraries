namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Runtime;

    public abstract class Logger : ILogger
    {
        private string parameters;
        private LoggerVerbosity verbosity;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected Logger()
        {
        }

        public virtual string FormatErrorEvent(BuildErrorEventArgs args)
        {
            return EventArgsFormatting.FormatEventMessage(args);
        }

        public virtual string FormatWarningEvent(BuildWarningEventArgs args)
        {
            return EventArgsFormatting.FormatEventMessage(args);
        }

        public abstract void Initialize(IEventSource eventSource);
        public bool IsVerbosityAtLeast(LoggerVerbosity checkVerbosity)
        {
            return (this.verbosity >= checkVerbosity);
        }

        public virtual void Shutdown()
        {
        }

        public virtual string Parameters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parameters;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.parameters = value;
            }
        }

        public virtual LoggerVerbosity Verbosity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.verbosity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.verbosity = value;
            }
        }
    }
}

