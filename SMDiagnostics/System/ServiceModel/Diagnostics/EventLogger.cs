namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    internal class EventLogger
    {
        private static bool canLogEvent = true;
        private System.ServiceModel.Diagnostics.DiagnosticTrace diagnosticTrace;
        [SecurityCritical]
        private string eventLogSourceName;
        private bool isInPatialTrust;
        [SecurityCritical]
        private static int logCountForPT;
        internal const int MaxEventLogsInPT = 5;

        private EventLogger()
        {
            this.isInPatialTrust = this.IsInPartialTrust();
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.EventLog instead")]
        internal EventLogger(string eventLogSourceName, object diagnosticTrace)
        {
            try
            {
                this.diagnosticTrace = (System.ServiceModel.Diagnostics.DiagnosticTrace) diagnosticTrace;
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
        internal void LogEvent(TraceEventType type, EventLogCategory category, System.ServiceModel.Diagnostics.EventLogEventId eventId, params string[] values)
        {
            this.LogEvent(type, category, eventId, true, values);
        }

        internal void LogEvent(TraceEventType type, EventLogCategory category, System.ServiceModel.Diagnostics.EventLogEventId eventId, bool shouldTrace, params string[] values)
        {
            if (canLogEvent)
            {
                try
                {
                    this.SafeLogEvent(type, category, eventId, shouldTrace, values);
                }
                catch (SecurityException exception)
                {
                    canLogEvent = false;
                    if (shouldTrace && (this.diagnosticTrace != null))
                    {
                        this.diagnosticTrace.TraceEvent(TraceEventType.Warning, 0x20004, System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "TraceHandledException"), TraceSR.GetString("TraceHandledException"), null, exception, null);
                    }
                }
            }
        }

        internal static string NormalizeEventLogParameter(string param)
        {
            if (param.IndexOf('%') < 0)
            {
                return param;
            }
            StringBuilder builder = null;
            int length = param.Length;
            for (int i = 0; i < length; i++)
            {
                char ch = param[i];
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
                        if ((param[i + 1] >= '0') && (param[i + 1] <= '9'))
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
                        builder.Append(param[j]);
                    }
                }
                builder.Append(ch);
                builder.Append(' ');
            }
            if (builder == null)
            {
                return param;
            }
            return builder.ToString();
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        internal void SafeLogEvent(TraceEventType type, EventLogCategory category, System.ServiceModel.Diagnostics.EventLogEventId eventId, bool shouldTrace, params string[] values)
        {
            this.UnsafeLogEvent(type, category, eventId, shouldTrace, values);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        private void SafeSetLogSourceName(string eventLogSourceName)
        {
            this.eventLogSourceName = eventLogSourceName;
        }

        [SecurityCritical]
        private void SetLogSourceName(string eventLogSourceName, object diagnosticTrace)
        {
            this.eventLogSourceName = eventLogSourceName;
            this.diagnosticTrace = (System.ServiceModel.Diagnostics.DiagnosticTrace) diagnosticTrace;
        }

        [SecurityCritical]
        internal static System.ServiceModel.Diagnostics.EventLogger UnsafeCreateEventLogger(string eventLogSourceName, object diagnosticTrace)
        {
            System.ServiceModel.Diagnostics.EventLogger logger = new System.ServiceModel.Diagnostics.EventLogger();
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
        internal void UnsafeLogEvent(TraceEventType type, EventLogCategory category, System.ServiceModel.Diagnostics.EventLogEventId eventId, bool shouldTrace, params string[] values)
        {
            if (logCountForPT < 5)
            {
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
                        this.UnsafeWriteEventLog(type, category, eventId, logValues, binaryForm, stringsRootHandle);
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
                    if (shouldTrace && (this.diagnosticTrace != null))
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(logValues.Length + 4);
                        dictionary["CategoryID.Name"] = category.ToString();
                        dictionary["CategoryID.Value"] = ((uint) category).ToString(CultureInfo.InvariantCulture);
                        dictionary["InstanceID.Name"] = eventId.ToString();
                        dictionary["InstanceID.Value"] = ((uint) eventId).ToString(CultureInfo.InvariantCulture);
                        for (int m = 0; m < values.Length; m++)
                        {
                            dictionary.Add("Value" + m.ToString(CultureInfo.InvariantCulture), (values[m] == null) ? string.Empty : System.Runtime.Diagnostics.DiagnosticTrace.XmlEncode(values[m]));
                        }
                        this.diagnosticTrace.TraceEvent(type, 0x20002, System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "EventLog"), TraceSR.GetString("TraceCodeEventLog"), new DictionaryTraceRecord(dictionary), null, null);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
                if (this.isInPatialTrust)
                {
                    logCountForPT++;
                }
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, UnmanagedCode=true)]
        private void UnsafeWriteEventLog(TraceEventType type, EventLogCategory category, System.ServiceModel.Diagnostics.EventLogEventId eventId, string[] logValues, byte[] sidBA, GCHandle stringsRootHandle)
        {
            using (SafeEventLogWriteHandle handle = SafeEventLogWriteHandle.RegisterEventSource(null, this.eventLogSourceName))
            {
                if (handle != null)
                {
                    HandleRef strings = new HandleRef(handle, stringsRootHandle.AddrOfPinnedObject());
                    System.ServiceModel.Diagnostics.NativeMethods.ReportEvent(handle, (ushort) EventLogEntryTypeFromEventType(type), (ushort) category, (uint) eventId, sidBA, (ushort) logValues.Length, 0, strings, null);
                }
            }
        }
    }
}

