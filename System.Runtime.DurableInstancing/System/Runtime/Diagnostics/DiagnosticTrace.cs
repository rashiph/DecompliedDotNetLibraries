namespace System.Runtime.Diagnostics
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;
    using System.Xml;
    using System.Xml.XPath;

    internal sealed class DiagnosticTrace
    {
        private static string appDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
        private bool calledShutdown;
        [SecurityCritical]
        private static Guid defaultEtwProviderId = new Guid("{c651f5f6-1c0d-492e-8ae1-b4efd7c9d503}");
        private const string DefaultTraceListenerName = "Default";
        [SecurityCritical]
        private System.Runtime.Diagnostics.EtwProvider etwProvider;
        private static Hashtable etwProviderCache = new Hashtable();
        private Guid etwProviderId;
        [SecurityCritical]
        private string eventSourceName;
        private const string EventSourceVersion = "4.0.0.0";
        private bool haveListeners;
        private static bool isVistaOrGreater = (Environment.OSVersion.Version.Major >= 6);
        private SourceLevels level;
        private object thisLock;
        private static Func<string> traceAnnotation;
        private const string TraceRecordVersion = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";
        private DiagnosticTraceSource traceSource;
        private string TraceSourceName;
        private const ushort TracingEventLogCategory = 4;
        [SecurityCritical]
        private static System.Diagnostics.Eventing.EventDescriptor transferEventDescriptor = new System.Diagnostics.Eventing.EventDescriptor(0x1f3, 0, 0x12, 0, 0, 0, 0x20000000001a0065L);
        private const int WindowsVistaMajorNumber = 6;

        [SecuritySafeCritical]
        public DiagnosticTrace(string traceSourceName, Guid etwProviderId)
        {
            try
            {
                this.thisLock = new object();
                this.TraceSourceName = traceSourceName;
                this.eventSourceName = this.TraceSourceName + " " + "4.0.0.0";
                this.LastFailure = DateTime.MinValue;
                this.CreateTraceSource();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                new EventLogger(this.eventSourceName, null).LogEvent(TraceEventType.Error, 4, 0xc0010064, false, new string[] { exception.ToString() });
            }
            try
            {
                this.CreateEtwProvider(etwProviderId);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.etwProvider = null;
                new EventLogger(this.eventSourceName, null).LogEvent(TraceEventType.Error, 4, 0xc0010064, false, new string[] { exception2.ToString() });
            }
            if (this.TracingEnabled || this.EtwTracingEnabled)
            {
                this.AddDomainEventHandlersForCleanup();
            }
        }

        [SecuritySafeCritical, Obsolete("For SMDiagnostics.dll use only")]
        private void AddDomainEventHandlersForCleanup()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            if (this.TracingEnabled)
            {
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.UnhandledExceptionHandler);
                currentDomain.DomainUnload += new EventHandler(this.ExitOrUnloadEventHandler);
                currentDomain.ProcessExit += new EventHandler(this.ExitOrUnloadEventHandler);
            }
        }

        [SecurityCritical]
        private static string BuildTrace(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, string description, TracePayload payload)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture));
            writer.WriteStartElement("TraceRecord");
            writer.WriteAttributeString("xmlns", "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord");
            writer.WriteAttributeString("Severity", TraceLevelHelper.LookupSeverity((TraceEventLevel) eventDescriptor.Level, (TraceEventOpcode) eventDescriptor.Opcode));
            writer.WriteAttributeString("Channel", LookupChannel((TraceChannel) eventDescriptor.Channel));
            writer.WriteElementString("TraceIdentifier", GenerateTraceCode(ref eventDescriptor));
            writer.WriteElementString("Description", description);
            writer.WriteElementString("AppDomain", payload.AppDomainFriendlyName);
            if (!string.IsNullOrEmpty(payload.EventSource))
            {
                writer.WriteElementString("Source", payload.EventSource);
            }
            if (!string.IsNullOrEmpty(payload.ExtendedData))
            {
                writer.WriteRaw(payload.ExtendedData);
            }
            if (!string.IsNullOrEmpty(payload.SerializedException))
            {
                writer.WriteRaw(payload.SerializedException);
            }
            writer.WriteEndElement();
            return sb.ToString();
        }

        [SecuritySafeCritical]
        private void CreateEtwProvider(Guid etwProviderId)
        {
            if ((etwProviderId != Guid.Empty) && isVistaOrGreater)
            {
                this.etwProvider = (System.Runtime.Diagnostics.EtwProvider) etwProviderCache[etwProviderId];
                if (this.etwProvider == null)
                {
                    lock (etwProviderCache)
                    {
                        this.etwProvider = (System.Runtime.Diagnostics.EtwProvider) etwProviderCache[etwProviderId];
                        if (this.etwProvider == null)
                        {
                            this.etwProvider = new System.Runtime.Diagnostics.EtwProvider(etwProviderId);
                            etwProviderCache.Add(etwProviderId, this.etwProvider);
                        }
                    }
                }
                this.etwProviderId = etwProviderId;
            }
        }

        private static string CreateSourceString(object source)
        {
            return (source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture));
        }

        [SecuritySafeCritical]
        private void CreateTraceSource()
        {
            if (!string.IsNullOrEmpty(this.TraceSourceName))
            {
                this.traceSource = new DiagnosticTraceSource(this.TraceSourceName);
                if (this.traceSource != null)
                {
                    this.traceSource.Listeners.Remove("Default");
                    this.haveListeners = this.traceSource.Listeners.Count > 0;
                    this.level = this.traceSource.Switch.Level;
                }
            }
        }

        [SecurityCritical]
        public void Event(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, string description)
        {
            if (this.TracingEnabled)
            {
                TracePayload payload = this.GetSerializedPayload(null, null, null);
                this.WriteTraceSource(ref eventDescriptor, description, payload);
            }
        }

        [SecuritySafeCritical]
        public void Event(int eventId, TraceEventLevel traceEventLevel, TraceChannel channel, string description)
        {
            if (this.TracingEnabled)
            {
                System.Diagnostics.Eventing.EventDescriptor eventDescriptor = GetEventDescriptor(eventId, channel, traceEventLevel);
                this.Event(ref eventDescriptor, description);
            }
        }

        private static string ExceptionToTraceString(Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture));
            writer.WriteStartElement("Exception");
            writer.WriteElementString("ExceptionType", XmlEncode(exception.GetType().AssemblyQualifiedName));
            writer.WriteElementString("Message", XmlEncode(exception.Message));
            writer.WriteElementString("StackTrace", XmlEncode(StackTraceString(exception)));
            writer.WriteElementString("ExceptionString", XmlEncode(exception.ToString()));
            Win32Exception exception2 = exception as Win32Exception;
            if (exception2 != null)
            {
                writer.WriteElementString("NativeErrorCode", exception2.NativeErrorCode.ToString("X", CultureInfo.InvariantCulture));
            }
            if ((exception.Data != null) && (exception.Data.Count > 0))
            {
                writer.WriteStartElement("DataItems");
                foreach (object obj2 in exception.Data.Keys)
                {
                    writer.WriteStartElement("Data");
                    writer.WriteElementString("Key", XmlEncode(obj2.ToString()));
                    writer.WriteElementString("Value", XmlEncode(exception.Data[obj2].ToString()));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            if (exception.InnerException != null)
            {
                writer.WriteStartElement("InnerException");
                writer.WriteRaw(ExceptionToTraceString(exception.InnerException));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            return sb.ToString();
        }

        private void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            this.ShutdownTracing();
        }

        [SecurityCritical]
        private static string GenerateTraceCode(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor)
        {
            return eventDescriptor.EventId.ToString(CultureInfo.InvariantCulture);
        }

        [SecurityCritical]
        private static System.Diagnostics.Eventing.EventDescriptor GetEventDescriptor(int eventId, TraceChannel channel, TraceEventLevel traceEventLevel)
        {
            long keywords = 0L;
            if (channel == TraceChannel.Admin)
            {
                keywords |= -9223372036854775808L;
            }
            else if (channel == TraceChannel.Operational)
            {
                keywords |= 0x4000000000000000L;
            }
            else if (channel == TraceChannel.Analytic)
            {
                keywords |= 0x2000000000000000L;
            }
            else if (channel == TraceChannel.Debug)
            {
                keywords |= 0x100000000000000L;
            }
            else if (channel == TraceChannel.Perf)
            {
                keywords |= 0x800000000000000L;
            }
            return new System.Diagnostics.Eventing.EventDescriptor(eventId, 0, (byte) channel, (byte) traceEventLevel, 0, 0, keywords);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception)
        {
            return this.GetSerializedPayload(source, traceRecord, exception, false);
        }

        public TracePayload GetSerializedPayload(object source, TraceRecord traceRecord, Exception exception, bool getServiceReference)
        {
            string eventSource = null;
            string extendedData = null;
            string serializedException = null;
            if (source != null)
            {
                eventSource = CreateSourceString(source);
            }
            if (traceRecord != null)
            {
                StringBuilder sb = new StringBuilder();
                XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb, CultureInfo.CurrentCulture));
                writer.WriteStartElement("ExtendedData");
                traceRecord.WriteTo(writer);
                writer.WriteEndElement();
                extendedData = sb.ToString();
            }
            if (exception != null)
            {
                serializedException = ExceptionToTraceString(exception);
            }
            if (getServiceReference && (traceAnnotation != null))
            {
                return new TracePayload(serializedException, eventSource, appDomainFriendlyName, extendedData, traceAnnotation());
            }
            return new TracePayload(serializedException, eventSource, appDomainFriendlyName, extendedData, string.Empty);
        }

        [SecuritySafeCritical]
        public bool IsEtwEventEnabled(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor)
        {
            return (this.EtwTracingEnabled && this.etwProvider.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords));
        }

        [SecuritySafeCritical]
        private void LogTraceFailure(string traceString, Exception exception)
        {
            TimeSpan span = TimeSpan.FromMinutes(10.0);
            try
            {
                lock (this.thisLock)
                {
                    if (DateTime.UtcNow.Subtract(this.LastFailure) >= span)
                    {
                        this.LastFailure = DateTime.UtcNow;
                        EventLogger logger = EventLogger.UnsafeCreateEventLogger(this.eventSourceName, this);
                        if (exception == null)
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, 4, 0xc0010068, false, new string[] { traceString });
                        }
                        else
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, 4, 0xc0010069, false, new string[] { traceString, exception.ToString() });
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
            }
        }

        private static string LookupChannel(TraceChannel traceChannel)
        {
            switch (traceChannel)
            {
                case TraceChannel.Admin:
                    return "Admin";

                case TraceChannel.Operational:
                    return "Operational";

                case TraceChannel.Analytic:
                    return "Analytic";

                case TraceChannel.Debug:
                    return "Debug";

                case TraceChannel.Perf:
                    return "Perf";

                case TraceChannel.Application:
                    return "Application";
            }
            return traceChannel.ToString();
        }

        public void SetAndTraceTransfer(Guid newId, bool emitTransfer)
        {
            if (emitTransfer)
            {
                this.TraceTransfer(newId);
            }
            ActivityId = newId;
        }

        public void SetAnnotation(Func<string> annotation)
        {
            traceAnnotation = annotation;
        }

        public bool ShouldTrace(TraceEventLevel level)
        {
            if (!this.ShouldTraceToTraceSource(level))
            {
                return this.ShouldTraceToEtw(level);
            }
            return true;
        }

        [SecuritySafeCritical]
        public bool ShouldTraceToEtw(TraceEventLevel level)
        {
            return ((this.EtwProvider != null) && this.EtwProvider.IsEnabled((byte) level, 0L));
        }

        public bool ShouldTraceToTraceSource(TraceEventLevel level)
        {
            return ((this.HaveListeners && (this.TraceSource != null)) && (((TraceEventType) 0) != (TraceLevelHelper.GetTraceEventType(level) & ((TraceEventType) ((int) this.Level)))));
        }

        [SecuritySafeCritical]
        private void ShutdownEtwProvider()
        {
            try
            {
                if (this.etwProvider != null)
                {
                    this.etwProvider.Dispose();
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.LogTraceFailure(null, exception);
            }
        }

        private void ShutdownTraceSource()
        {
            try
            {
                if (TraceCore.AppDomainUnloadIsEnabled(this))
                {
                    TraceCore.AppDomainUnload(this, AppDomain.CurrentDomain.FriendlyName, ProcessName, ProcessId.ToString(CultureInfo.CurrentCulture));
                }
                this.TraceSource.Flush();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.LogTraceFailure(null, exception);
            }
        }

        private void ShutdownTracing()
        {
            if (!this.calledShutdown)
            {
                this.calledShutdown = true;
                this.ShutdownTraceSource();
                this.ShutdownEtwProvider();
            }
        }

        private static string StackTraceString(Exception exception)
        {
            string stackTrace = exception.StackTrace;
            if (!string.IsNullOrEmpty(stackTrace))
            {
                return stackTrace;
            }
            StackFrame[] frames = new StackTrace(false).GetFrames();
            int skipFrames = 0;
            bool flag = false;
            foreach (StackFrame frame in frames)
            {
                string str3;
                string name = frame.GetMethod().Name;
                if (((str3 = name) != null) && (((str3 == "StackTraceString") || (str3 == "AddExceptionToTraceString")) || (str3 == "GetAdditionalPayload")))
                {
                    skipFrames++;
                }
                else if (name.StartsWith("ThrowHelper", StringComparison.Ordinal))
                {
                    skipFrames++;
                }
                else
                {
                    flag = true;
                }
                if (flag)
                {
                    break;
                }
            }
            StackTrace trace = new StackTrace(skipFrames, false);
            return trace.ToString();
        }

        [SecuritySafeCritical]
        public void TraceTransfer(Guid newId)
        {
            Guid activityId = ActivityId;
            if (newId != activityId)
            {
                try
                {
                    if (this.HaveListeners)
                    {
                        this.TraceSource.TraceTransfer(0, null, newId);
                    }
                    if (this.IsEtwEventEnabled(ref transferEventDescriptor))
                    {
                        this.etwProvider.WriteTransferEvent(ref transferEventDescriptor, newId, (traceAnnotation == null) ? string.Empty : traceAnnotation(), appDomainFriendlyName);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.LogTraceFailure(null, exception);
                }
            }
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exceptionObject = (Exception) args.ExceptionObject;
            TraceCore.UnhandledException(this, exceptionObject);
            this.ShutdownTracing();
        }

        [SecurityCritical]
        public void WriteTraceSource(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, string description, TracePayload payload)
        {
            if (this.TracingEnabled)
            {
                XPathNavigator data = null;
                try
                {
                    string xml = BuildTrace(ref eventDescriptor, description, payload);
                    XmlDocument document = new XmlDocument();
                    document.LoadXml(xml);
                    data = document.CreateNavigator();
                    this.TraceSource.TraceData(TraceLevelHelper.GetTraceEventType(eventDescriptor.Level, eventDescriptor.Opcode), eventDescriptor.EventId, data);
                    if (this.calledShutdown)
                    {
                        this.TraceSource.Flush();
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.LogTraceFailure((data == null) ? string.Empty : data.ToString(), exception);
                }
            }
        }

        public static string XmlEncode(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            int length = text.Length;
            StringBuilder builder = new StringBuilder(length + 8);
            for (int i = 0; i < length; i++)
            {
                char ch = text[i];
                switch (ch)
                {
                    case '<':
                    {
                        builder.Append("&lt;");
                        continue;
                    }
                    case '>':
                    {
                        builder.Append("&gt;");
                        continue;
                    }
                    case '&':
                    {
                        builder.Append("&amp;");
                        continue;
                    }
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        public static Guid ActivityId
        {
            [SecuritySafeCritical]
            get
            {
                object activityId = Trace.CorrelationManager.ActivityId;
                if (activityId != null)
                {
                    return (Guid) activityId;
                }
                return Guid.Empty;
            }
            [SecuritySafeCritical]
            set
            {
                Trace.CorrelationManager.ActivityId = value;
            }
        }

        public static Guid DefaultEtwProviderId
        {
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return defaultEtwProviderId;
            }
            [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                defaultEtwProviderId = value;
            }
        }

        public System.Runtime.Diagnostics.EtwProvider EtwProvider
        {
            [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.etwProvider;
            }
        }

        private bool EtwTracingEnabled
        {
            [SecuritySafeCritical]
            get
            {
                return (this.etwProvider != null);
            }
        }

        public bool HaveListeners
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.haveListeners;
            }
        }

        public bool IsEtwProviderEnabled
        {
            [SecuritySafeCritical]
            get
            {
                return (this.EtwTracingEnabled && this.etwProvider.IsEnabled());
            }
        }

        private DateTime LastFailure { get; set; }

        public SourceLevels Level
        {
            get
            {
                if (this.TraceSource != null)
                {
                    this.level = this.TraceSource.Switch.Level;
                }
                return this.level;
            }
        }

        private static int ProcessId
        {
            [SecuritySafeCritical]
            get
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.Id;
                }
            }
        }

        private static string ProcessName
        {
            [SecuritySafeCritical]
            get
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.ProcessName;
                }
            }
        }

        public Action RefreshState
        {
            [SecuritySafeCritical]
            get
            {
                return this.EtwProvider.ControllerCallBack;
            }
            [SecuritySafeCritical]
            set
            {
                this.EtwProvider.ControllerCallBack = value;
            }
        }

        public DiagnosticTraceSource TraceSource
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.traceSource;
            }
        }

        public bool TracingEnabled
        {
            get
            {
                return (this.traceSource != null);
            }
        }
    }
}

