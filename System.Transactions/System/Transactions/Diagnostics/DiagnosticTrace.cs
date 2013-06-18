namespace System.Transactions.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Transactions;
    using System.Xml;
    using System.Xml.XPath;

    internal static class DiagnosticTrace
    {
        private static string AppDomainFriendlyName = null;
        private static bool calledShutdown = false;
        internal const string DefaultTraceListenerName = "Default";
        internal static Guid EmptyGuid = Guid.Empty;
        private const string EventLogSourceName = ".NET Runtime";
        private static bool haveListeners = false;
        private static SourceLevels level;
        private static object localSyncObject = new object();
        private static bool shouldCorrelate = false;
        private static bool shouldTraceCritical = false;
        private static bool shouldTraceError = false;
        private static bool shouldTraceInformation = false;
        private static bool shouldTraceVerbose = false;
        private static bool shouldTraceWarning = false;
        private const string subType = "";
        private static Dictionary<int, string> traceEventTypeNames;
        private static int traceFailureCount = 0;
        private const int traceFailureLogThreshold = 10;
        private static int traceFailureThreshold = 0;
        private const string TraceRecordVersion = "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord";
        private static System.Diagnostics.TraceSource traceSource = null;
        private const string TraceSourceName = "System.Transactions";
        private static bool tracingEnabled = true;
        private const string version = "1";

        static DiagnosticTrace()
        {
            AppDomainFriendlyName = AppDomain.CurrentDomain.FriendlyName;
            traceEventTypeNames = new Dictionary<int, string>();
            traceEventTypeNames[1] = "Critical";
            traceEventTypeNames[2] = "Error";
            traceEventTypeNames[4] = "Warning";
            traceEventTypeNames[8] = "Information";
            traceEventTypeNames[0x10] = "Verbose";
            traceEventTypeNames[0x800] = "Resume";
            traceEventTypeNames[0x100] = "Start";
            traceEventTypeNames[0x200] = "Stop";
            traceEventTypeNames[0x400] = "Suspend";
            traceEventTypeNames[0x1000] = "Transfer";
            TraceFailureThreshold = 10;
            TraceFailureCount = TraceFailureThreshold + 1;
            try
            {
                traceSource = new System.Diagnostics.TraceSource("System.Transactions", SourceLevels.Critical);
                AppDomain currentDomain = AppDomain.CurrentDomain;
                if (TraceSource.Switch.ShouldTrace(TraceEventType.Critical))
                {
                    currentDomain.UnhandledException += new UnhandledExceptionEventHandler(DiagnosticTrace.UnhandledExceptionHandler);
                }
                currentDomain.DomainUnload += new EventHandler(DiagnosticTrace.ExitOrUnloadEventHandler);
                currentDomain.ProcessExit += new EventHandler(DiagnosticTrace.ExitOrUnloadEventHandler);
                haveListeners = TraceSource.Listeners.Count > 0;
                SetLevel(TraceSource.Switch.Level);
            }
            catch (ConfigurationErrorsException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (TraceSource == null)
                {
                    LogEvent(TraceEventType.Error, string.Format(CultureInfo.CurrentCulture, System.Transactions.SR.GetString("FailedToCreateTraceSource"), new object[] { exception }), true);
                }
                else
                {
                    TraceSource = null;
                    LogEvent(TraceEventType.Error, string.Format(CultureInfo.CurrentCulture, System.Transactions.SR.GetString("FailedToInitializeTraceSource"), new object[] { exception }), true);
                }
            }
        }

        private static void AddExceptionToTraceString(XmlWriter xml, Exception exception)
        {
            xml.WriteElementString("ExceptionType", XmlEncode(exception.GetType().AssemblyQualifiedName));
            xml.WriteElementString("Message", XmlEncode(exception.Message));
            xml.WriteElementString("StackTrace", XmlEncode(StackTraceString(exception)));
            xml.WriteElementString("ExceptionString", XmlEncode(exception.ToString()));
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
                    if ((obj2 != null) && (exception.Data[obj2] != null))
                    {
                        xml.WriteElementString("Key", XmlEncode(obj2.ToString()));
                        xml.WriteElementString("Value", XmlEncode(exception.Data[obj2].ToString()));
                    }
                    xml.WriteEndElement();
                }
                xml.WriteEndElement();
            }
            if (exception.InnerException != null)
            {
                xml.WriteStartElement("InnerException");
                AddExceptionToTraceString(xml, exception.InnerException);
                xml.WriteEndElement();
            }
        }

        private static XPathNavigator BuildTraceString(TraceEventType type, string code, string description, TraceRecord trace, Exception exception, object source)
        {
            return BuildTraceString(new PlainXmlWriter(), type, code, description, trace, exception, source);
        }

        private static XPathNavigator BuildTraceString(PlainXmlWriter xml, TraceEventType type, string code, string description, TraceRecord trace, Exception exception, object source)
        {
            xml.WriteStartElement("TraceRecord");
            xml.WriteAttributeString("xmlns", "http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord");
            xml.WriteAttributeString("Severity", LookupSeverity(type));
            xml.WriteElementString("TraceIdentifier", code);
            xml.WriteElementString("Description", description);
            xml.WriteElementString("AppDomain", AppDomainFriendlyName);
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
                AddExceptionToTraceString(xml, exception);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
            return xml.ToNavigator();
        }

        private static string CreateSourceString(object source)
        {
            return (source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture));
        }

        private static EventLogEntryType EventLogEntryTypeFromEventType(TraceEventType type)
        {
            EventLogEntryType information = EventLogEntryType.Information;
            switch (type)
            {
                case TraceEventType.Critical:
                case TraceEventType.Error:
                    return EventLogEntryType.Error;

                case (TraceEventType.Error | TraceEventType.Critical):
                    return information;

                case TraceEventType.Warning:
                    return EventLogEntryType.Warning;
            }
            return information;
        }

        private static void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            ShutdownTracing();
        }

        private static SourceLevels FixLevel(SourceLevels level)
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
            if ((level & ~SourceLevels.Warning) == SourceLevels.Off)
            {
                return level;
            }
            return (level | SourceLevels.ActivityTracing);
        }

        internal static Guid GetActivityId()
        {
            object activityId = Trace.CorrelationManager.ActivityId;
            if (activityId != null)
            {
                return (Guid) activityId;
            }
            return Guid.Empty;
        }

        internal static void GetActivityId(ref Guid guid)
        {
            if (ShouldCorrelate)
            {
                guid = GetActivityId();
            }
        }

        internal static void LogEvent(TraceEventType type, string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message) && (message.Length >= 0x2000))
                {
                    message = message.Substring(0, 0x1fff);
                }
                EventLog.WriteEntry(".NET Runtime", message, EventLogEntryTypeFromEventType(type));
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch
            {
            }
        }

        internal static void LogEvent(TraceEventType type, string message, bool addProcessInfo)
        {
            if (addProcessInfo)
            {
                message = string.Format(CultureInfo.CurrentCulture, "{0}: {1}\n{2}: {3}\n{4}", new object[] { "ProcessName", ProcessName, "ProcessId", ProcessId, message });
            }
            LogEvent(type, message);
        }

        private static void LogEvent(TraceEventType type, string code, string description, TraceRecord trace, Exception exception, object source)
        {
            StringBuilder builder = new StringBuilder(System.Transactions.SR.GetString("EventLogValue", new object[] { ProcessName, ProcessId.ToString(CultureInfo.CurrentCulture), code, description }));
            if (source != null)
            {
                builder.AppendLine(System.Transactions.SR.GetString("EventLogSourceValue", new object[] { CreateSourceString(source) }));
            }
            if (exception != null)
            {
                builder.AppendLine(System.Transactions.SR.GetString("EventLogExceptionValue", new object[] { exception.ToString() }));
            }
            if (trace != null)
            {
                builder.AppendLine(System.Transactions.SR.GetString("EventLogEventIdValue", new object[] { trace.EventId }));
                builder.AppendLine(System.Transactions.SR.GetString("EventLogTraceValue", new object[] { trace.ToString() }));
            }
            LogEvent(type, builder.ToString(), false);
        }

        private static void LogTraceFailure(string traceString, Exception e)
        {
            if (e != null)
            {
                traceString = string.Format(CultureInfo.CurrentCulture, System.Transactions.SR.GetString("FailedToTraceEvent"), new object[] { e, (traceString != null) ? traceString : "" });
            }
            lock (localSyncObject)
            {
                if (TraceFailureCount > TraceFailureThreshold)
                {
                    TraceFailureCount = 1;
                    TraceFailureThreshold *= 2;
                    LogEvent(TraceEventType.Error, traceString, true);
                }
                else
                {
                    TraceFailureCount++;
                }
            }
        }

        private static string LookupSeverity(TraceEventType type)
        {
            int num = ((int) type) & 0x1f;
            if ((type & (TraceEventType.Stop | TraceEventType.Start)) != ((TraceEventType) 0))
            {
                num = (int) type;
            }
            else if (num == 0)
            {
                num = 0x10;
            }
            return TraceEventTypeNames[num];
        }

        internal static void SetActivityId(Guid id)
        {
            Trace.CorrelationManager.ActivityId = id;
        }

        private static void SetLevel(SourceLevels level)
        {
            SourceLevels levels = FixLevel(level);
            DiagnosticTrace.level = levels;
            if (TraceSource != null)
            {
                TraceSource.Switch.Level = levels;
                shouldCorrelate = ShouldTrace(TraceEventType.Transfer);
                shouldTraceVerbose = ShouldTrace(TraceEventType.Verbose);
                shouldTraceInformation = ShouldTrace(TraceEventType.Information);
                shouldTraceWarning = ShouldTrace(TraceEventType.Warning);
                shouldTraceError = ShouldTrace(TraceEventType.Error);
                shouldTraceCritical = ShouldTrace(TraceEventType.Critical);
            }
        }

        private static void SetLevelThreadSafe(SourceLevels level)
        {
            if (TracingEnabled && (level != Level))
            {
                lock (localSyncObject)
                {
                    SetLevel(level);
                }
            }
        }

        internal static bool ShouldTrace(TraceEventType type)
        {
            return ((((type & ((TraceEventType) ((int) Level))) != ((TraceEventType) 0)) && (TraceSource != null)) && HaveListeners);
        }

        private static void ShutdownTracing()
        {
            if (TraceSource != null)
            {
                try
                {
                    if (Level != SourceLevels.Off)
                    {
                        if (Information)
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                            dictionary["AppDomain.FriendlyName"] = AppDomain.CurrentDomain.FriendlyName;
                            dictionary["ProcessName"] = ProcessName;
                            dictionary["ProcessId"] = ProcessId.ToString(CultureInfo.CurrentCulture);
                            TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/TraceCodes/System/ActivityTracing/2004/07/Diagnostics/AppDomainUnload", System.Transactions.SR.GetString("TraceCodeAppDomainUnloading"), new DictionaryTraceRecord(dictionary), null, ref EmptyGuid, false, null);
                        }
                        calledShutdown = true;
                        TraceSource.Flush();
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (StackOverflowException)
                {
                    throw;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    LogTraceFailure(null, exception);
                }
            }
        }

        private static string StackTraceString(Exception exception)
        {
            string stackTrace = exception.StackTrace;
            if (!string.IsNullOrEmpty(stackTrace))
            {
                return stackTrace;
            }
            StackFrame[] frames = new StackTrace(true).GetFrames();
            int skipFrames = 0;
            foreach (StackFrame frame in frames)
            {
                if (!(frame.GetMethod().DeclaringType == typeof(DiagnosticTrace)))
                {
                    break;
                }
                skipFrames++;
            }
            StackTrace trace = new StackTrace(skipFrames);
            return trace.ToString();
        }

        internal static void TraceAndLogEvent(TraceEventType type, string code, string description, TraceRecord trace, Exception exception, ref Guid activityId, object source)
        {
            bool flag = ShouldTrace(type);
            string traceString = null;
            try
            {
                LogEvent(type, code, description, trace, exception, source);
                if (flag)
                {
                    TraceEvent(type, code, description, trace, exception, ref activityId, false, source);
                }
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (StackOverflowException)
            {
                throw;
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                LogTraceFailure(traceString, exception2);
            }
        }

        internal static void TraceEvent(TraceEventType type, string code, string description)
        {
            TraceEvent(type, code, description, null, null, ref EmptyGuid, false, null);
        }

        internal static void TraceEvent(TraceEventType type, string code, string description, TraceRecord trace)
        {
            TraceEvent(type, code, description, trace, null, ref EmptyGuid, false, null);
        }

        internal static void TraceEvent(TraceEventType type, string code, string description, TraceRecord trace, Exception exception)
        {
            TraceEvent(type, code, description, trace, exception, ref EmptyGuid, false, null);
        }

        internal static void TraceEvent(TraceEventType type, string code, string description, TraceRecord trace, Exception exception, ref Guid activityId, bool emitTransfer, object source)
        {
            if (ShouldTrace(type))
            {
                using (Activity.CreateActivity(activityId, emitTransfer))
                {
                    XPathNavigator data = BuildTraceString(type, code, description, trace, exception, source);
                    try
                    {
                        TraceSource.TraceData(type, 0, data);
                        if (calledShutdown)
                        {
                            TraceSource.Flush();
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        throw;
                    }
                    catch (StackOverflowException)
                    {
                        throw;
                    }
                    catch (ThreadAbortException)
                    {
                        throw;
                    }
                    catch (Exception exception2)
                    {
                        LogTraceFailure(System.Transactions.SR.GetString("TraceFailure", new object[] { type.ToString(), code, description, (source == null) ? string.Empty : CreateSourceString(source) }), exception2);
                    }
                }
            }
        }

        internal static void TraceTransfer(Guid newId)
        {
            Guid activityId = GetActivityId();
            if ((ShouldCorrelate && (newId != activityId)) && HaveListeners)
            {
                try
                {
                    if (newId != activityId)
                    {
                        TraceSource.TraceTransfer(0, null, newId);
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (StackOverflowException)
                {
                    throw;
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    LogTraceFailure(null, exception);
                }
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception exceptionObject = (Exception) args.ExceptionObject;
            TraceEvent(TraceEventType.Critical, "http://msdn.microsoft.com/TraceCodes/System/ActivityTracing/2004/07/Reliability/Exception/Unhandled", System.Transactions.SR.GetString("UnhandledException"), null, exceptionObject, ref EmptyGuid, false, null);
            ShutdownTracing();
        }

        internal static string XmlEncode(string text)
        {
            if (text == null)
            {
                return null;
            }
            int length = text.Length;
            StringBuilder builder = new StringBuilder(length + 8);
            for (int i = 0; i < length; i++)
            {
                char ch2 = text[i];
                switch (ch2)
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
                builder.Append(ch2);
            }
            return builder.ToString();
        }

        internal static bool Critical
        {
            get
            {
                return shouldTraceCritical;
            }
        }

        internal static bool Error
        {
            get
            {
                return shouldTraceError;
            }
        }

        internal static bool HaveListeners
        {
            get
            {
                return haveListeners;
            }
        }

        internal static bool Information
        {
            get
            {
                return shouldTraceInformation;
            }
        }

        internal static SourceLevels Level
        {
            get
            {
                if ((TraceSource != null) && (TraceSource.Switch.Level != level))
                {
                    level = TraceSource.Switch.Level;
                }
                return level;
            }
            set
            {
                SetLevelThreadSafe(value);
            }
        }

        private static int ProcessId
        {
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
            get
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    return process.ProcessName;
                }
            }
        }

        internal static bool ShouldCorrelate
        {
            get
            {
                return shouldCorrelate;
            }
        }

        private static Dictionary<int, string> TraceEventTypeNames
        {
            get
            {
                return traceEventTypeNames;
            }
        }

        private static int TraceFailureCount
        {
            get
            {
                return traceFailureCount;
            }
            set
            {
                traceFailureCount = value;
            }
        }

        private static int TraceFailureThreshold
        {
            get
            {
                return traceFailureThreshold;
            }
            set
            {
                traceFailureThreshold = value;
            }
        }

        private static System.Diagnostics.TraceSource TraceSource
        {
            get
            {
                return traceSource;
            }
            set
            {
                traceSource = value;
            }
        }

        internal static bool TracingEnabled
        {
            get
            {
                return (tracingEnabled && (traceSource != null));
            }
        }

        internal static bool Verbose
        {
            get
            {
                return shouldTraceVerbose;
            }
        }

        internal static bool Warning
        {
            get
            {
                return shouldTraceWarning;
            }
        }
    }
}

