namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal static class MessageLogger
    {
        private static bool attemptedTraceSourceInitialization = false;
        private const string DefaultTraceListenerName = "Default";
        private static object filterLock = new object();
        private static bool initialized = false;
        private static bool initializing = false;
        private static bool inPartialTrust = false;
        private static bool lastWriteSucceeded = true;
        private static bool logKnownPii;
        private static bool logMessageBody = false;
        private static int maxMessageSize;
        private static int maxMessagesToLog;
        private static List<XPathMessageFilter> messageFilterTable;
        private static PiiTraceSource messageTraceSource;
        private const string MessageTraceSourceName = "System.ServiceModel.MessageLogging";
        private static int numberOfMessagesToLog;
        private static string[][] piiBodyPaths;
        private static string[][] piiHeadersPaths;
        private static string[] securityActions;
        private static MessageLoggingSource sources = MessageLoggingSource.None;
        private static object syncObject = new object();
        private const int Unlimited = -1;

        private static bool AddFilter(XPathMessageFilter filter)
        {
            if (filter == null)
            {
                filter = new XPathMessageFilter("");
            }
            Filters.Add(filter);
            return true;
        }

        internal static void EnsureInitialized()
        {
            lock (syncObject)
            {
                if (!initialized && !initializing)
                {
                    try
                    {
                        Initialize();
                    }
                    catch (SecurityException exception)
                    {
                        inPartialTrust = true;
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Warning, 0x20004, System.ServiceModel.SR.GetString("PartialTrustMessageLoggingNotEnabled"), null, exception);
                        }
                        LogNonFatalInitializationException(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustMessageLoggingNotEnabled"), exception));
                    }
                    initialized = true;
                }
            }
        }

        private static void EnsureMessageTraceSource()
        {
            if (!initialized)
            {
                EnsureInitialized();
            }
            if ((MessageTraceSource == null) && !attemptedTraceSourceInitialization)
            {
                InitializeMessageTraceSource();
            }
        }

        private static void ExitOrUnloadEventHandler(object sender, EventArgs e)
        {
            lock (syncObject)
            {
                if (MessageTraceSource != null)
                {
                    MessageTraceSource.Close();
                    messageTraceSource = null;
                }
            }
        }

        private static void FailedToLogMessage(Exception e)
        {
            bool flag = false;
            lock (syncObject)
            {
                if (lastWriteSucceeded)
                {
                    lastWriteSucceeded = false;
                    flag = true;
                }
            }
            if (flag)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.MessageLogging, (EventLogEventId) (-1073610747), new string[] { e.ToString() });
            }
        }

        private static bool HasSecurityAction(Message message)
        {
            string action = message.Headers.Action;
            if (string.IsNullOrEmpty(action))
            {
                return true;
            }
            foreach (string str2 in SecurityActions)
            {
                if (string.CompareOrdinal(action, str2) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static void IncrementLoggedMessagesCount(object data)
        {
            if (numberOfMessagesToLog > 0)
            {
                lock (syncObject)
                {
                    if (numberOfMessagesToLog > 0)
                    {
                        numberOfMessagesToLog--;
                        if ((numberOfMessagesToLog == 0) && DiagnosticUtility.ShouldTraceInformation)
                        {
                            TraceUtility.TraceEvent(TraceEventType.Information, 0x20009, System.ServiceModel.SR.GetString("TraceCodeMessageCountLimitExceeded"), data);
                        }
                    }
                }
            }
            lock (syncObject)
            {
                if (!lastWriteSucceeded)
                {
                    lastWriteSucceeded = true;
                }
            }
        }

        [SecuritySafeCritical]
        private static void Initialize()
        {
            initializing = true;
            DiagnosticSection section = DiagnosticSection.UnsafeGetSection();
            if (section != null)
            {
                LogKnownPii = section.MessageLogging.LogKnownPii && MachineSettingsSection.EnableLoggingKnownPii;
                LogMalformedMessages = section.MessageLogging.LogMalformedMessages;
                LogMessageBody = section.MessageLogging.LogEntireMessage;
                LogMessagesAtServiceLevel = section.MessageLogging.LogMessagesAtServiceLevel;
                LogMessagesAtTransportLevel = section.MessageLogging.LogMessagesAtTransportLevel;
                MaxNumberOfMessagesToLog = section.MessageLogging.MaxMessagesToLog;
                MaxMessageSize = section.MessageLogging.MaxSizeOfMessageToLog;
                ReadFiltersFromConfig(section);
            }
        }

        private static void InitializeMessageTraceSource()
        {
            try
            {
                attemptedTraceSourceInitialization = true;
                PiiTraceSource source = new PiiTraceSource("System.ServiceModel.MessageLogging", "System.ServiceModel 4.0.0.0") {
                    Switch = { Level = SourceLevels.Information }
                };
                source.Listeners.Remove("Default");
                if (source.Listeners.Count > 0)
                {
                    AppDomain.CurrentDomain.DomainUnload += new EventHandler(MessageLogger.ExitOrUnloadEventHandler);
                    AppDomain.CurrentDomain.ProcessExit += new EventHandler(MessageLogger.ExitOrUnloadEventHandler);
                    AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MessageLogger.ExitOrUnloadEventHandler);
                }
                else
                {
                    source = null;
                }
                messageTraceSource = source;
            }
            catch (ConfigurationErrorsException)
            {
                throw;
            }
            catch (SecurityException exception)
            {
                inPartialTrust = true;
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x20004, System.ServiceModel.SR.GetString("PartialTrustMessageLoggingNotEnabled"), null, exception);
                }
                LogNonFatalInitializationException(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustMessageLoggingNotEnabled"), exception));
            }
            catch (Exception exception2)
            {
                messageTraceSource = null;
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                LogNonFatalInitializationException(exception2);
            }
        }

        private static void LogInternal(MessageLogTraceRecord record)
        {
            PlainXmlWriter writer = new PlainXmlWriter(MaxMessageSize);
            try
            {
                record.WriteTo(writer);
                writer.Close();
                TraceXPathNavigator data = writer.Navigator;
                if (((messageTraceSource != null) && !messageTraceSource.ShouldLogPii) || !LogKnownPii)
                {
                    data.RemovePii(PiiHeadersPaths);
                    if ((LogMessageBody && (record.Message != null)) && HasSecurityAction(record.Message))
                    {
                        data.RemovePii(PiiBodyPaths);
                    }
                }
                LogInternal(record.MessageLoggingSource, data);
            }
            catch (PlainXmlWriter.MaxSizeExceededException)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x2000b, System.ServiceModel.SR.GetString("TraceCodeMessageNotLoggedQuotaExceeded"), record.Message);
                }
            }
        }

        private static void LogInternal(MessageLoggingSource source, object data)
        {
            if ((source & MessageLoggingSource.Malformed) != MessageLoggingSource.None)
            {
                if (!TD.MessageLogWarning(data.ToString()) && TD.MessageLogEventSizeExceededIsEnabled())
                {
                    TD.MessageLogEventSizeExceeded();
                }
            }
            else if (!TD.MessageLogInfo(data.ToString()) && TD.MessageLogEventSizeExceededIsEnabled())
            {
                TD.MessageLogEventSizeExceeded();
            }
            if (MessageTraceSource != null)
            {
                MessageTraceSource.TraceData(TraceEventType.Information, 0, data);
            }
            IncrementLoggedMessagesCount(data);
        }

        internal static void LogMessage(Stream stream, MessageLoggingSource source)
        {
            try
            {
                ThrowIfNotMalformed(source);
                if (ShouldLogMessages(source))
                {
                    LogInternal(new MessageLogTraceRecord(stream, source));
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                FailedToLogMessage(exception);
            }
        }

        internal static void LogMessage(ArraySegment<byte> buffer, MessageLoggingSource source)
        {
            try
            {
                ThrowIfNotMalformed(source);
                if (ShouldLogMessages(source))
                {
                    LogInternal(new MessageLogTraceRecord(buffer, source));
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                FailedToLogMessage(exception);
            }
        }

        internal static void LogMessage(ref Message message, MessageLoggingSource source)
        {
            LogMessage(ref message, null, source);
        }

        internal static void LogMessage(MessageLoggingSource source, string data)
        {
            try
            {
                if (ShouldLogMessages(MessageLoggingSource.Malformed))
                {
                    LogInternal(source, data);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                FailedToLogMessage(exception);
            }
        }

        internal static void LogMessage(ref Message message, XmlReader reader, MessageLoggingSource source)
        {
            try
            {
                if (ShouldLogMessages(source))
                {
                    LogMessageImpl(ref message, reader, source);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                FailedToLogMessage(exception);
            }
        }

        private static void LogMessageImpl(ref Message message, XmlReader reader, MessageLoggingSource source)
        {
            ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? TraceUtility.ExtractActivity(message) : null;
            using (ServiceModelActivity.BoundOperation(activity))
            {
                if (ShouldLogMessages(source) && ((numberOfMessagesToLog > 0) || (numberOfMessagesToLog == -1)))
                {
                    bool flag = ((source & MessageLoggingSource.LastChance) != MessageLoggingSource.None) || ((source & MessageLoggingSource.TransportSend) != MessageLoggingSource.None);
                    source &= ~MessageLoggingSource.LastChance;
                    if (((flag || (message is NullMessage)) || (message.Version.Addressing != AddressingVersion.None)) && (MatchFilters(message, source) && ((numberOfMessagesToLog == -1) || (numberOfMessagesToLog > 0))))
                    {
                        MessageLogTraceRecord record = new MessageLogTraceRecord(ref message, reader, source, LogMessageBody);
                        LogInternal(record);
                    }
                }
            }
        }

        [SecuritySafeCritical]
        private static void LogNonFatalInitializationException(Exception e)
        {
            DiagnosticUtility.UnsafeEventLog.UnsafeLogEvent(TraceEventType.Critical, EventLogCategory.MessageLogging, (EventLogEventId) (-1073610745), true, new string[] { e.ToString() });
        }

        private static bool MatchFilters(Message message, MessageLoggingSource source)
        {
            bool flag = true;
            if (FilterMessages && ((source & MessageLoggingSource.Malformed) == MessageLoggingSource.None))
            {
                flag = false;
                List<XPathMessageFilter> list = new List<XPathMessageFilter>();
                lock (syncObject)
                {
                    foreach (XPathMessageFilter filter in Filters)
                    {
                        try
                        {
                            if (filter.Match(message))
                            {
                                flag = true;
                                break;
                            }
                        }
                        catch (FilterInvalidBodyAccessException)
                        {
                            list.Add(filter);
                        }
                        catch (MessageFilterException exception)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                TraceUtility.TraceEvent(TraceEventType.Information, 0x20008, System.ServiceModel.SR.GetString("TraceCodeFilterNotMatchedNodeQuotaExceeded"), (Exception) exception, message);
                            }
                        }
                    }
                    foreach (XPathMessageFilter filter2 in list)
                    {
                        Filters.Remove(filter2);
                        PlainXmlWriter writer = new PlainXmlWriter();
                        filter2.WriteXPathTo(writer, null, "filter", null, true);
                        DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.MessageLogging, (EventLogEventId) (-1073610746), new string[] { writer.Navigator.ToString() });
                    }
                    if (FilterCount == 0)
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        private static void ProcessAudit(bool turningOn)
        {
            if (turningOn)
            {
                if (messageTraceSource != null)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.MessageLogging, (EventLogEventId) (-1073610744), new string[0]);
                }
            }
            else
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Information, EventLogCategory.MessageLogging, (EventLogEventId) (-1073610743), new string[0]);
            }
        }

        private static void ReadFiltersFromConfig(DiagnosticSection section)
        {
            for (int i = 0; i < section.MessageLogging.Filters.Count; i++)
            {
                XPathMessageFilterElement element = section.MessageLogging.Filters[i];
                AddFilter(element.Filter);
            }
        }

        private static bool ShouldLogMessages(MessageLoggingSource source)
        {
            if ((source & Sources) == MessageLoggingSource.None)
            {
                return false;
            }
            return (((MessageTraceSource != null) || (((source & MessageLoggingSource.Malformed) != MessageLoggingSource.None) && TD.MessageLogWarningIsEnabled())) || TD.MessageLogInfoIsEnabled());
        }

        private static bool ShouldProcessAudit(MessageLoggingSource source, bool turningOn)
        {
            if (turningOn)
            {
                return (sources == MessageLoggingSource.None);
            }
            return (sources == source);
        }

        private static void ThrowIfNotMalformed(MessageLoggingSource source)
        {
            if ((source & MessageLoggingSource.Malformed) == MessageLoggingSource.None)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("OnlyMalformedMessagesAreSupported"), "source"));
            }
        }

        private static int FilterCount
        {
            get
            {
                return Filters.Count;
            }
        }

        private static bool FilterMessages
        {
            get
            {
                if (FilterCount <= 0)
                {
                    return false;
                }
                if (numberOfMessagesToLog <= 0)
                {
                    return (numberOfMessagesToLog == -1);
                }
                return true;
            }
        }

        private static List<XPathMessageFilter> Filters
        {
            get
            {
                if (messageFilterTable == null)
                {
                    lock (filterLock)
                    {
                        if (messageFilterTable == null)
                        {
                            List<XPathMessageFilter> list = new List<XPathMessageFilter>();
                            messageFilterTable = list;
                        }
                    }
                }
                return messageFilterTable;
            }
        }

        internal static bool LoggingEnabled
        {
            get
            {
                return (Sources != MessageLoggingSource.None);
            }
        }

        internal static bool LogKnownPii
        {
            get
            {
                return logKnownPii;
            }
            set
            {
                logKnownPii = value;
            }
        }

        internal static bool LogMalformedMessages
        {
            get
            {
                return ((Sources & MessageLoggingSource.Malformed) != MessageLoggingSource.None);
            }
            set
            {
                lock (syncObject)
                {
                    bool flag = ShouldProcessAudit(MessageLoggingSource.Malformed, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!inPartialTrust)
                        {
                            sources |= MessageLoggingSource.Malformed;
                        }
                    }
                    else
                    {
                        sources &= 0x7ffffbff;
                    }
                    if (flag)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static bool LogMessageBody
        {
            get
            {
                return logMessageBody;
            }
            set
            {
                logMessageBody = value;
            }
        }

        internal static bool LogMessagesAtServiceLevel
        {
            get
            {
                return ((Sources & MessageLoggingSource.ServiceLevel) != MessageLoggingSource.None);
            }
            set
            {
                lock (syncObject)
                {
                    bool flag = ShouldProcessAudit(MessageLoggingSource.ServiceLevel, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!inPartialTrust)
                        {
                            sources |= MessageLoggingSource.ServiceLevel;
                        }
                    }
                    else
                    {
                        sources &= 0x7ffffc0f;
                    }
                    if (flag)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static bool LogMessagesAtTransportLevel
        {
            get
            {
                return ((Sources & MessageLoggingSource.Transport) != MessageLoggingSource.None);
            }
            set
            {
                lock (syncObject)
                {
                    bool flag = ShouldProcessAudit(MessageLoggingSource.Transport, value);
                    if (value)
                    {
                        EnsureMessageTraceSource();
                        if (!inPartialTrust)
                        {
                            sources |= MessageLoggingSource.Transport;
                        }
                    }
                    else
                    {
                        sources &= 0x7ffffff9;
                    }
                    if (flag)
                    {
                        ProcessAudit(value);
                    }
                }
            }
        }

        internal static int MaxMessageSize
        {
            get
            {
                return maxMessageSize;
            }
            set
            {
                maxMessageSize = value;
            }
        }

        internal static int MaxNumberOfMessagesToLog
        {
            get
            {
                return maxMessagesToLog;
            }
            set
            {
                lock (syncObject)
                {
                    maxMessagesToLog = value;
                    numberOfMessagesToLog = maxMessagesToLog;
                }
            }
        }

        internal static TraceSource MessageTraceSource
        {
            get
            {
                return messageTraceSource;
            }
        }

        private static string[][] PiiBodyPaths
        {
            get
            {
                if (piiBodyPaths == null)
                {
                    piiBodyPaths = new string[][] { new string[] { "MessageLogTraceRecord", "Envelope", "Body", "RequestSecurityToken" }, new string[] { "MessageLogTraceRecord", "Envelope", "Body", "RequestSecurityTokenResponse" }, new string[] { "MessageLogTraceRecord", "Envelope", "Body", "RequestSecurityTokenResponseCollection" } };
                }
                return piiBodyPaths;
            }
        }

        private static string[][] PiiHeadersPaths
        {
            get
            {
                if (piiHeadersPaths == null)
                {
                    piiHeadersPaths = new string[][] { new string[] { "MessageLogTraceRecord", "Envelope", "Header", "Security" }, new string[] { "MessageLogTraceRecord", "Envelope", "Header", "IssuedTokens" } };
                }
                return piiHeadersPaths;
            }
        }

        private static string[] SecurityActions
        {
            get
            {
                if (securityActions == null)
                {
                    securityActions = new string[] { 
                        "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Issue", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Issue", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Renew", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Renew", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Cancel", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Cancel", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/Validate", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/Validate", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Amend", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Amend", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Renew", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Renew", "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/SCT/Cancel", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/SCT/Cancel", 
                        "http://schemas.xmlsoap.org/ws/2005/02/trust/RST/KET", "http://schemas.xmlsoap.org/ws/2005/02/trust/RSTR/KET", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/SCT-Amend", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/SCT-Amend", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Issue", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Issue", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Renew", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Renew", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/Validate", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/Validate", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RST/KET", "http://schemas.xmlsoap.org/ws/2004/04/security/trust/RSTR/KET"
                     };
                }
                return securityActions;
            }
        }

        internal static bool ShouldLogMalformed
        {
            get
            {
                return ShouldLogMessages(MessageLoggingSource.Malformed);
            }
        }

        private static MessageLoggingSource Sources
        {
            get
            {
                if (!initialized)
                {
                    EnsureInitialized();
                }
                return sources;
            }
        }
    }
}

