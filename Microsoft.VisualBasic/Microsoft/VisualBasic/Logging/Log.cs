namespace Microsoft.VisualBasic.Logging
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Log
    {
        private const string DEFAULT_FILE_LOG_TRACE_LISTENER_NAME = "FileLog";
        private static Dictionary<TraceEventType, int> m_IdHash = InitializeIDHash();
        private DefaultTraceSource m_TraceSource;
        private const string WINAPP_SOURCE_NAME = "DefaultSource";

        [SecuritySafeCritical]
        public Log()
        {
            this.m_TraceSource = new DefaultTraceSource("DefaultSource");
            if (!this.m_TraceSource.HasBeenConfigured)
            {
                this.InitializeWithDefaultsSinceNoConfigExists();
            }
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(this.CloseOnProcessExit);
        }

        [SecuritySafeCritical]
        public Log(string name)
        {
            this.m_TraceSource = new DefaultTraceSource(name);
            if (!this.m_TraceSource.HasBeenConfigured)
            {
                this.InitializeWithDefaultsSinceNoConfigExists();
            }
        }

        [SecuritySafeCritical]
        private void CloseOnProcessExit(object sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit -= new EventHandler(this.CloseOnProcessExit);
            this.TraceSource.Close();
        }

        private static Dictionary<TraceEventType, int> InitializeIDHash()
        {
            Dictionary<TraceEventType, int> dictionary2 = new Dictionary<TraceEventType, int>(10);
            Dictionary<TraceEventType, int> dictionary3 = dictionary2;
            dictionary3.Add(TraceEventType.Information, 0);
            dictionary3.Add(TraceEventType.Warning, 1);
            dictionary3.Add(TraceEventType.Error, 2);
            dictionary3.Add(TraceEventType.Critical, 3);
            dictionary3.Add(TraceEventType.Start, 4);
            dictionary3.Add(TraceEventType.Stop, 5);
            dictionary3.Add(TraceEventType.Suspend, 6);
            dictionary3.Add(TraceEventType.Resume, 7);
            dictionary3.Add(TraceEventType.Verbose, 8);
            dictionary3.Add(TraceEventType.Transfer, 9);
            dictionary3 = null;
            return dictionary2;
        }

        [SecuritySafeCritical]
        protected internal virtual void InitializeWithDefaultsSinceNoConfigExists()
        {
            this.m_TraceSource.Listeners.Add(new FileLogTraceListener("FileLog"));
            this.m_TraceSource.Switch.Level = SourceLevels.Information;
        }

        private int TraceEventTypeToId(TraceEventType traceEventValue)
        {
            if (m_IdHash.ContainsKey(traceEventValue))
            {
                return m_IdHash[traceEventValue];
            }
            return 0;
        }

        public void WriteEntry(string message)
        {
            this.WriteEntry(message, TraceEventType.Information, this.TraceEventTypeToId(TraceEventType.Information));
        }

        public void WriteEntry(string message, TraceEventType severity)
        {
            this.WriteEntry(message, severity, this.TraceEventTypeToId(severity));
        }

        public void WriteEntry(string message, TraceEventType severity, int id)
        {
            if (message == null)
            {
                message = "";
            }
            this.m_TraceSource.TraceEvent(severity, id, message);
        }

        public void WriteException(Exception ex)
        {
            this.WriteException(ex, TraceEventType.Error, "", this.TraceEventTypeToId(TraceEventType.Error));
        }

        public void WriteException(Exception ex, TraceEventType severity, string additionalInfo)
        {
            this.WriteException(ex, severity, additionalInfo, this.TraceEventTypeToId(severity));
        }

        public void WriteException(Exception ex, TraceEventType severity, string additionalInfo, int id)
        {
            if (ex == null)
            {
                throw ExceptionUtils.GetArgumentNullException("ex");
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(ex.Message);
            if (additionalInfo != "")
            {
                builder.Append(" ");
                builder.Append(additionalInfo);
            }
            this.m_TraceSource.TraceEvent(severity, id, builder.ToString());
        }

        public FileLogTraceListener DefaultFileLogWriter
        {
            [SecuritySafeCritical]
            get
            {
                return (FileLogTraceListener) this.TraceSource.Listeners["FileLog"];
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public System.Diagnostics.TraceSource TraceSource
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_TraceSource;
            }
        }

        internal sealed class DefaultTraceSource : TraceSource
        {
            private StringDictionary listenerAttributes;
            private bool m_HasBeenInitializedFromConfigFile;

            public DefaultTraceSource(string name) : base(name)
            {
            }

            protected override string[] GetSupportedAttributes()
            {
                this.m_HasBeenInitializedFromConfigFile = true;
                return base.GetSupportedAttributes();
            }

            public bool HasBeenConfigured
            {
                get
                {
                    if (this.listenerAttributes == null)
                    {
                        this.listenerAttributes = this.Attributes;
                    }
                    return this.m_HasBeenInitializedFromConfigFile;
                }
            }
        }
    }
}

