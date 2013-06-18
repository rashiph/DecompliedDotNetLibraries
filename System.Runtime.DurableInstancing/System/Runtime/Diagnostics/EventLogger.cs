namespace System.Runtime.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Interop;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    internal sealed class EventLogger
    {
        private static bool canLogEvent = true;
        private DiagnosticTrace diagnosticTrace;
        [SecurityCritical]
        private string eventLogSourceName;
        private bool isInPartialTrust;
        [SecurityCritical]
        private static int logCountForPT;
        private const int MaxEventLogsInPT = 5;

        private EventLogger()
        {
            this.isInPartialTrust = this.IsInPartialTrust();
        }

        [Obsolete("For System.Runtime.dll use only. Call FxTrace.EventLog instead")]
        public EventLogger(string eventLogSourceName, DiagnosticTrace diagnosticTrace)
        {
            try
            {
                this.diagnosticTrace = diagnosticTrace;
                if (canLogEvent)
                {
                    this.SafeSetLogSourceName(eventLogSourceName);
                }
            }
            catch (SecurityException)
            {
                canLogEvent = false;
            }
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

        [SecuritySafeCritical]
        private bool IsInPartialTrust()
        {
            bool flag = false;
            try
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    flag = string.IsNullOrEmpty(process.ProcessName);
                }
            }
            catch (SecurityException)
            {
                flag = true;
            }
            return flag;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void LogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, params string[] values)
        {
            this.LogEvent(type, eventLogCategory, eventId, true, values);
        }

        public void LogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            if (canLogEvent)
            {
                try
                {
                    this.SafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
                }
                catch (SecurityException exception)
                {
                    canLogEvent = false;
                    if ((shouldTrace && (this.diagnosticTrace != null)) && TraceCore.HandledExceptionIsEnabled(this.diagnosticTrace))
                    {
                        TraceCore.HandledException(this.diagnosticTrace, exception);
                    }
                }
            }
        }

        private static string NormalizeEventLogParameter(string eventLogParameter)
        {
            if (eventLogParameter.IndexOf('%') < 0)
            {
                return eventLogParameter;
            }
            StringBuilder builder = null;
            int length = eventLogParameter.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = eventLogParameter[i];
                if (ch == '%')
                {
                    if ((i + 1) >= length)
                    {
                        if (builder != null)
                        {
                            builder.Append(ch);
                        }
                    }
                    else
                    {
                        if ((eventLogParameter[i + 1] >= '0') && (eventLogParameter[i + 1] <= '9'))
                        {
                            goto Label_0074;
                        }
                        if (builder != null)
                        {
                            builder.Append(ch);
                        }
                    }
                }
                else if (builder != null)
                {
                    builder.Append(ch);
                }
                continue;
            Label_0074:
                if (builder == null)
                {
                    builder = new StringBuilder(length + 2);
                    for (int j = 0; j < i; j++)
                    {
                        builder.Append(eventLogParameter[j]);
                    }
                }
                builder.Append(ch);
                builder.Append(' ');
            }
            if (builder == null)
            {
                return eventLogParameter;
            }
            return builder.ToString();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        private void SafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            this.UnsafeLogEvent(type, eventLogCategory, eventId, shouldTrace, values);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        private void SafeSetLogSourceName(string eventLogSourceName)
        {
            this.eventLogSourceName = eventLogSourceName;
        }

        [SecurityCritical]
        private void SetLogSourceName(string eventLogSourceName, DiagnosticTrace diagnosticTrace)
        {
            this.eventLogSourceName = eventLogSourceName;
            this.diagnosticTrace = diagnosticTrace;
        }

        [SecurityCritical]
        public static EventLogger UnsafeCreateEventLogger(string eventLogSourceName, DiagnosticTrace diagnosticTrace)
        {
            EventLogger logger = new EventLogger();
            logger.SetLogSourceName(eventLogSourceName, diagnosticTrace);
            return logger;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private int UnsafeGetProcessId()
        {
            using (Process process = Process.GetCurrentProcess())
            {
                return process.Id;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private string UnsafeGetProcessName()
        {
            using (Process process = Process.GetCurrentProcess())
            {
                return process.ProcessName;
            }
        }

        [SecurityCritical]
        public void UnsafeLogEvent(TraceEventType type, ushort eventLogCategory, uint eventId, bool shouldTrace, params string[] values)
        {
            if (logCountForPT >= 5)
            {
                return;
            }
            try
            {
                int num = 0;
                string[] logValues = new string[values.Length + 2];
                for (int i = 0; i < values.Length; i++)
                {
                    string str = values[i];
                    if (!string.IsNullOrEmpty(str))
                    {
                        str = NormalizeEventLogParameter(str);
                    }
                    else
                    {
                        str = string.Empty;
                    }
                    logValues[i] = str;
                    num += str.Length + 1;
                }
                string str2 = NormalizeEventLogParameter(this.UnsafeGetProcessName());
                logValues[logValues.Length - 2] = str2;
                num += str2.Length + 1;
                string str3 = this.UnsafeGetProcessId().ToString(CultureInfo.InvariantCulture);
                logValues[logValues.Length - 1] = str3;
                num += str3.Length + 1;
                if (num > 0x6400)
                {
                    int length = (0x6400 / logValues.Length) - 1;
                    for (int j = 0; j < logValues.Length; j++)
                    {
                        if (logValues[j].Length > length)
                        {
                            logValues[j] = logValues[j].Substring(0, length);
                        }
                    }
                }
                SecurityIdentifier user = WindowsIdentity.GetCurrent().User;
                byte[] binaryForm = new byte[user.BinaryLength];
                user.GetBinaryForm(binaryForm, 0);
                IntPtr[] ptrArray = new IntPtr[logValues.Length];
                GCHandle stringsRootHandle = new GCHandle();
                GCHandle[] handleArray = null;
                try
                {
                    stringsRootHandle = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
                    handleArray = new GCHandle[logValues.Length];
                    for (int k = 0; k < logValues.Length; k++)
                    {
                        handleArray[k] = GCHandle.Alloc(logValues[k], GCHandleType.Pinned);
                        ptrArray[k] = handleArray[k].AddrOfPinnedObject();
                    }
                    this.UnsafeWriteEventLog(type, eventLogCategory, eventId, logValues, binaryForm, stringsRootHandle);
                }
                finally
                {
                    if (stringsRootHandle.AddrOfPinnedObject() != IntPtr.Zero)
                    {
                        stringsRootHandle.Free();
                    }
                    if (handleArray != null)
                    {
                        foreach (GCHandle handle2 in handleArray)
                        {
                            handle2.Free();
                        }
                    }
                }
                if ((shouldTrace && (this.diagnosticTrace != null)) && ((TraceCore.TraceCodeEventLogCriticalIsEnabled(this.diagnosticTrace) || TraceCore.TraceCodeEventLogVerboseIsEnabled(this.diagnosticTrace)) || ((TraceCore.TraceCodeEventLogInfoIsEnabled(this.diagnosticTrace) || TraceCore.TraceCodeEventLogWarningIsEnabled(this.diagnosticTrace)) || TraceCore.TraceCodeEventLogErrorIsEnabled(this.diagnosticTrace))))
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(logValues.Length + 4);
                    dictionary["CategoryID.Name"] = "EventLogCategory";
                    dictionary["CategoryID.Value"] = eventLogCategory.ToString(CultureInfo.InvariantCulture);
                    dictionary["InstanceID.Name"] = "EventId";
                    dictionary["InstanceID.Value"] = eventId.ToString(CultureInfo.InvariantCulture);
                    for (int m = 0; m < values.Length; m++)
                    {
                        dictionary.Add("Value" + m.ToString(CultureInfo.InvariantCulture), (values[m] == null) ? string.Empty : DiagnosticTrace.XmlEncode(values[m]));
                    }
                    TraceRecord traceRecord = new DictionaryTraceRecord(dictionary);
                    switch (type)
                    {
                        case TraceEventType.Critical:
                            TraceCore.TraceCodeEventLogCritical(this.diagnosticTrace, traceRecord);
                            goto Label_035C;

                        case TraceEventType.Error:
                            TraceCore.TraceCodeEventLogError(this.diagnosticTrace, traceRecord);
                            goto Label_035C;

                        case (TraceEventType.Error | TraceEventType.Critical):
                            goto Label_035C;

                        case TraceEventType.Warning:
                            TraceCore.TraceCodeEventLogWarning(this.diagnosticTrace, traceRecord);
                            goto Label_035C;

                        case TraceEventType.Information:
                            TraceCore.TraceCodeEventLogInfo(this.diagnosticTrace, traceRecord);
                            goto Label_035C;

                        case TraceEventType.Verbose:
                            TraceCore.TraceCodeEventLogVerbose(this.diagnosticTrace, traceRecord);
                            goto Label_035C;
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
            }
        Label_035C:
            if (this.isInPartialTrust)
            {
                logCountForPT++;
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private void UnsafeWriteEventLog(TraceEventType type, ushort eventLogCategory, uint eventId, string[] logValues, byte[] sidBA, GCHandle stringsRootHandle)
        {
            using (SafeEventLogWriteHandle handle = SafeEventLogWriteHandle.RegisterEventSource(null, this.eventLogSourceName))
            {
                if (handle != null)
                {
                    HandleRef strings = new HandleRef(handle, stringsRootHandle.AddrOfPinnedObject());
                    System.Runtime.Interop.UnsafeNativeMethods.ReportEvent(handle, (ushort) EventLogEntryTypeFromEventType(type), eventLogCategory, eventId, sidBA, (ushort) logValues.Length, 0, strings, null);
                }
            }
        }
    }
}

