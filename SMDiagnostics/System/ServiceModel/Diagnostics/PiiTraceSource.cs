namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Configuration;

    internal class PiiTraceSource : TraceSource
    {
        private string eventSourceName;
        private bool initialized;
        private object localSyncObject;
        internal const string LogPii = "logKnownPii";
        private bool shouldLogPii;

        internal PiiTraceSource(string name, string eventSourceName) : base(name)
        {
            this.eventSourceName = string.Empty;
            this.localSyncObject = new object();
            this.eventSourceName = eventSourceName;
        }

        internal PiiTraceSource(string name, string eventSourceName, SourceLevels levels) : base(name, levels)
        {
            this.eventSourceName = string.Empty;
            this.localSyncObject = new object();
            this.eventSourceName = eventSourceName;
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { "logKnownPii" };
        }

        private void Initialize()
        {
            if (!this.initialized)
            {
                lock (this.localSyncObject)
                {
                    if (!this.initialized)
                    {
                        string str = base.Attributes["logKnownPii"];
                        bool result = false;
                        if (!string.IsNullOrEmpty(str) && !bool.TryParse(str, out result))
                        {
                            result = false;
                        }
                        if (result)
                        {
                            EventLogger logger = new EventLogger(this.eventSourceName, null);
                            if (MachineSettingsSection.EnableLoggingKnownPii)
                            {
                                logger.LogEvent(TraceEventType.Information, EventLogCategory.MessageLogging, (EventLogEventId) (-1073676181), false, new string[0]);
                                this.shouldLogPii = true;
                            }
                            else
                            {
                                logger.LogEvent(TraceEventType.Error, EventLogCategory.MessageLogging, (EventLogEventId) (-1073676180), false, new string[0]);
                            }
                        }
                        this.initialized = true;
                    }
                }
            }
        }

        internal bool ShouldLogPii
        {
            get
            {
                if (!this.initialized)
                {
                    this.Initialize();
                }
                return this.shouldLogPii;
            }
            set
            {
                this.initialized = true;
                this.shouldLogPii = value;
            }
        }
    }
}

