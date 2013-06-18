namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal class DiagnosticTrace
    {
        private string AppDomainFriendlyName;
        private bool calledShutdown;
        private static object classLockObject = new object();
        private const SourceLevels DefaultLevel = SourceLevels.Off;
        private const string DefaultTraceListenerName = "Default";
        [SecurityCritical]
        private string eventSourceName = string.Empty;
        private bool haveListeners;
        private DateTime lastFailure = DateTime.MinValue;
        private SourceLevels level;
        private object localSyncObject = new object();
        private const int MaxTraceSize = 0xffff;
        private bool shouldUseActivity;
        private const string subType = "";
        private const int traceFailureLogThreshold = 1;
        private const string TraceRecordVersion = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";
        private PiiTraceSource traceSource;
        private string TraceSourceName = string.Empty;
        private TraceSourceKind traceSourceType = TraceSourceKind.PiiTraceSource;
        private bool tracingEnabled = true;
        private const string version = "1";

        [SecurityCritical, Obsolete("For SMDiagnostics.dll use only. Never 'new' this type up unless you are DiagnosticUtility.")]
        internal DiagnosticTrace(TraceSourceKind sourceType, string traceSourceName, string eventSourceName)
        {
            this.traceSourceType = sourceType;
            this.TraceSourceName = traceSourceName;
            this.eventSourceName = eventSourceName;
            this.AppDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
            try
            {
                this.CreateTraceSource();
                this.UnsafeAddDomainEventHandlersForCleanup();
            }
            catch (ConfigurationErrorsException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                new System.ServiceModel.Diagnostics.EventLogger(this.eventSourceName, null).LogEvent(TraceEventType.Error, EventLogCategory.Tracing, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073676188), false, new string[] { exception.ToString() });
            }
        }

        private void AddExceptionToTraceString(XmlWriter xml, Exception exception)
        {
            xml.WriteElementString("ExceptionType", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(exception.GetType().AssemblyQualifiedName));
            xml.WriteElementString("Message", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(exception.Message));
            xml.WriteElementString("StackTrace", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(this.StackTraceString(exception)));
            xml.WriteElementString("ExceptionString", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(exception.ToString()));
            Win32Exception exception2 = exception as Win32Exception;
            if (exception2 != null)
            {
                xml.WriteElementString("NativeErrorCode", exception2.NativeErrorCode.ToString("X", CultureInfo.InvariantCulture));
            }
            if ((exception.Data != null) && (exception.Data.Count > 0))
            {
                xml.WriteStartElement("DataItems");
                foreach (object obj2 in exception.Data.Keys)
                {
                    xml.WriteStartElement("Data");
                    xml.WriteElementString("Key", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(obj2.ToString()));
                    xml.WriteElementString("Value", System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(exception.Data[obj2].ToString()));
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            if (exception.InnerException != null)
            {
                xml.WriteStartElement("InnerException");
                this.AddExceptionToTraceString(xml, exception.InnerException);
                xml.WriteEndElement();
            }
        }

        private void BuildTrace(TraceEventType type, string msdnTraceCode, string description, TraceRecord trace, Exception exception, object source, out TraceXPathNavigator navigator)
        {
            PlainXmlWriter xml = new PlainXmlWriter(0xffff);
            navigator = xml.Navigator;
            this.BuildTrace(xml, type, msdnTraceCode, description, trace, exception, source);
            if (!this.TraceSource.ShouldLogPii)
            {
                navigator.RemovePii(System.ServiceModel.Diagnostics.DiagnosticStrings.HeadersPaths);
            }
        }

        private void BuildTrace(PlainXmlWriter xml, TraceEventType type, string msdnTraceCode, string description, TraceRecord trace, Exception exception, object source)
        {
            xml.WriteStartElement("TraceRecord");
            xml.WriteAttributeString("xmlns", "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord");
            xml.WriteAttributeString("Severity", LookupSeverity(type));
            xml.WriteElementString("TraceIdentifier", msdnTraceCode);
            xml.WriteElementString("Description", description);
            xml.WriteElementString("AppDomain", this.AppDomainFriendlyName);
            if (source != null)
            {
                xml.WriteElementString("Source", CreateSourceString(source));
            }
            if (trace != null)
            {
                xml.WriteStartElement("ExtendedData");
                xml.WriteAttributeString("xmlns", trace.EventId);
                trace.WriteTo(xml);
                xml.WriteEndElement();
            }
            if (exception != null)
            {
                xml.WriteStartElement("Exception");
                this.AddExceptionToTraceString(xml, exception);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
        }

        private static string CreateSourceString(object source)
        {
            return (source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture));
        }

        [SecuritySafeCritical]
        private void CreateTraceSource()
        {
            PiiTraceSource piiTraceSource = null;
            if (this.traceSourceType == TraceSourceKind.PiiTraceSource)
            {
                piiTraceSource = new PiiTraceSource(this.TraceSourceName, this.eventSourceName, SourceLevels.Off);
            }
            else
            {
                piiTraceSource = new System.ServiceModel.Diagnostics.DiagnosticTraceSource(this.TraceSourceName, this.eventSourceName, SourceLevels.Off);
            }
            this.UnsafeRemoveDefaultTraceListener(piiTraceSource);
            this.TraceSource = piiTraceSource;
        }

        private void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            this.ShutdownTracing();
        }

        private SourceLevels FixLevel(SourceLevels level)
        {
            if (((level & ~SourceLevels.Information) & SourceLevels.Verbose) != SourceLevels.Off)
            {
                level |= SourceLevels.Verbose;
            }
            else if (((level & ~SourceLevels.Warning) & SourceLevels.Information) != SourceLevels.Off)
            {
                level |= SourceLevels.Information;
            }
            else if (((level & ~SourceLevels.Error) & SourceLevels.Warning) != SourceLevels.Off)
            {
                level |= SourceLevels.Warning;
            }
            if (((level & ~SourceLevels.Critical) & SourceLevels.Error) != SourceLevels.Off)
            {
                level |= SourceLevels.Error;
            }
            if ((level & SourceLevels.Critical) != SourceLevels.Off)
            {
                level |= SourceLevels.Critical;
            }
            if (level == SourceLevels.ActivityTracing)
            {
                level = SourceLevels.Off;
            }
            return level;
        }

        internal static string GenerateMsdnTraceCode(string traceSource, string traceCodeString)
        {
            return string.Format(CultureInfo.InvariantCulture, "http://msdn.microsoft.com/{0}/library/{1}.{2}.aspx", new object[] { CultureInfo.CurrentCulture.Name, traceSource, traceCodeString });
        }

        [SecuritySafeCritical]
        private void LogTraceFailure(string traceString, Exception e)
        {
            TimeSpan span = TimeSpan.FromMinutes(10.0);
            try
            {
                lock (this.localSyncObject)
                {
                    if (DateTime.UtcNow.Subtract(this.LastFailure) >= span)
                    {
                        this.LastFailure = DateTime.UtcNow;
                        System.ServiceModel.Diagnostics.EventLogger logger = System.ServiceModel.Diagnostics.EventLogger.UnsafeCreateEventLogger(this.eventSourceName, this);
                        if (e == null)
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.Tracing, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073676184), false, new string[] { traceString });
                        }
                        else
                        {
                            logger.UnsafeLogEvent(TraceEventType.Error, EventLogCategory.Tracing, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073676183), false, new string[] { traceString, e.ToString() });
                        }
                    }
                }
            }
            catch
            {
            }
        }

        private static string LookupSeverity(TraceEventType type)
        {
            switch (type)
            {
                case TraceEventType.Critical:
                    return "Critical";

                case TraceEventType.Error:
                    return "Error";

                case TraceEventType.Warning:
                    return "Warning";

                case TraceEventType.Information:
                    return "Information";

                case TraceEventType.Verbose:
                    return "Verbose";

                case TraceEventType.Suspend:
                    return "Suspend";

                case TraceEventType.Transfer:
                    return "Transfer";

                case TraceEventType.Start:
                    return "Start";

                case TraceEventType.Stop:
                    return "Stop";
            }
            return type.ToString();
        }

        private void SetLevel(SourceLevels level)
        {
            SourceLevels levels = this.FixLevel(level);
            this.level = levels;
            if (this.TraceSource != null)
            {
                this.haveListeners = this.TraceSource.Listeners.Count > 0;
                if ((this.TraceSource.Switch.Level != SourceLevels.Off) && (level == SourceLevels.Off))
                {
                    System.Diagnostics.TraceSource traceSource = this.TraceSource;
                    this.CreateTraceSource();
                    traceSource.Close();
                }
                this.tracingEnabled = this.HaveListeners && (levels != SourceLevels.Off);
                this.TraceSource.Switch.Level = levels;
                this.shouldUseActivity = (levels & SourceLevels.ActivityTracing) != SourceLevels.Off;
            }
        }

        private void SetLevelThreadSafe(SourceLevels level)
        {
            lock (this.localSyncObject)
            {
                this.SetLevel(level);
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ShouldTrace instead")]
        internal bool ShouldTrace(TraceEventType type)
        {
            return ((this.TracingEnabled && (this.TraceSource != null)) && (((TraceEventType) 0) != (type & ((TraceEventType) ((int) this.Level)))));
        }

        private void ShutdownTracing()
        {
            if ((this.TraceSource != null) && !this.calledShutdown)
            {
                try
                {
                    if (this.Level != SourceLevels.Off)
                    {
                        if (this.ShouldTrace(TraceEventType.Information))
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                            dictionary["AppDomain.FriendlyName"] = AppDomain.CurrentDomain.FriendlyName;
                            dictionary["ProcessName"] = ProcessName;
                            dictionary["ProcessId"] = ProcessId.ToString(CultureInfo.CurrentCulture);
                            this.TraceEvent(TraceEventType.Information, 0x20001, GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "AppDomainUnload"), TraceSR.GetString("TraceCodeAppDomainUnload"), new DictionaryTraceRecord(dictionary), null, null);
                        }
                        this.calledShutdown = true;
                        this.TraceSource.Flush();
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

        private string StackTraceString(Exception exception)
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
                if (((str3 = name) != null) && (((str3 == "StackTraceString") || (str3 == "AddExceptionToTraceString")) || (((str3 == "BuildTrace") || (str3 == "TraceEvent")) || (str3 == "TraceException"))))
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

        internal void TraceEvent(TraceEventType type, int code, string msdnTraceCode, string description, TraceRecord trace, Exception exception, object source)
        {
            TraceXPathNavigator navigator = null;
            try
            {
                if ((this.TraceSource != null) && this.HaveListeners)
                {
                    try
                    {
                        this.BuildTrace(type, msdnTraceCode, description, trace, exception, source, out navigator);
                    }
                    catch (PlainXmlWriter.MaxSizeExceededException)
                    {
                        StringTraceRecord record = new StringTraceRecord("TruncatedTraceId", msdnTraceCode);
                        this.TraceEvent(type, 0x2000c, GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "TraceTruncatedQuotaExceeded"), TraceSR.GetString("TraceCodeTraceTruncatedQuotaExceeded"), record, null, null);
                    }
                    this.TraceSource.TraceData(type, code, navigator);
                    if (this.calledShutdown)
                    {
                        this.TraceSource.Flush();
                    }
                    this.LastFailure = DateTime.MinValue;
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                this.LogTraceFailure((navigator == null) ? string.Empty : navigator.ToString(), exception2);
            }
        }

        internal void TraceEvent(TraceEventType type, int code, string msdnTraceCode, string description, TraceRecord trace, Exception exception, Guid activityId, object source)
        {
            using ((this.ShouldUseActivity && (Guid.Empty != activityId)) ? Activity.CreateActivity(activityId) : null)
            {
                this.TraceEvent(type, code, msdnTraceCode, description, trace, exception, source);
            }
        }

        internal void TraceTransfer(Guid newId)
        {
            if (this.ShouldUseActivity)
            {
                Guid activityId = ActivityId;
                if ((newId != activityId) && this.HaveListeners)
                {
                    try
                    {
                        this.TraceSource.TraceTransfer(0, null, newId);
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
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exceptionObject = (Exception) args.ExceptionObject;
            this.TraceEvent(TraceEventType.Critical, 0x20005, "UnhandledException", TraceSR.GetString("UnhandledException"), null, exceptionObject, null);
            this.ShutdownTracing();
        }

        [Obsolete("For SMDiagnostics.dll use only"), SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private void UnsafeAddDomainEventHandlersForCleanup()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            this.haveListeners = this.TraceSource.Listeners.Count > 0;
            this.tracingEnabled = this.HaveListeners;
            if (this.TracingEnabled)
            {
                currentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.UnhandledExceptionHandler);
                this.SetLevel(this.TraceSource.Switch.Level);
                currentDomain.DomainUnload += new EventHandler(this.ExitOrUnloadEventHandler);
                currentDomain.ProcessExit += new EventHandler(this.ExitOrUnloadEventHandler);
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private void UnsafeRemoveDefaultTraceListener(PiiTraceSource piiTraceSource)
        {
            piiTraceSource.Listeners.Remove("Default");
        }

        internal static Guid ActivityId
        {
            get
            {
                return System.Runtime.Diagnostics.DiagnosticTrace.ActivityId;
            }
            set
            {
                System.Runtime.Diagnostics.DiagnosticTrace.ActivityId = value;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.HaveListeners instead")]
        internal bool HaveListeners
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.haveListeners;
            }
        }

        private DateTime LastFailure
        {
            get
            {
                return this.lastFailure;
            }
            set
            {
                this.lastFailure = value;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.Level instead")]
        internal SourceLevels Level
        {
            get
            {
                if ((this.TraceSource != null) && (this.TraceSource.Switch.Level != this.level))
                {
                    this.level = this.TraceSource.Switch.Level;
                }
                return this.level;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SetLevelThreadSafe(value);
            }
        }

        internal static int ProcessId
        {
            get
            {
                int id = -1;
                try
                {
                    using (Process process = Process.GetCurrentProcess())
                    {
                        id = process.Id;
                    }
                }
                catch (SecurityException)
                {
                }
                return id;
            }
        }

        internal static string ProcessName
        {
            get
            {
                string processName = null;
                try
                {
                    using (Process process = Process.GetCurrentProcess())
                    {
                        processName = process.ProcessName;
                    }
                }
                catch (SecurityException)
                {
                }
                return processName;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ShouldUseActivity instead")]
        internal bool ShouldUseActivity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.shouldUseActivity;
            }
        }

        internal PiiTraceSource TraceSource
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.traceSource;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.traceSource = value;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.TracingEnabled instead")]
        internal bool TracingEnabled
        {
            get
            {
                return (this.tracingEnabled && (this.traceSource != null));
            }
        }
    }
}

