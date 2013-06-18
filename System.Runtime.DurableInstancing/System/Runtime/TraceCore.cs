namespace System.Runtime
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.Diagnostics;
    using System.Security;

    internal class TraceCore
    {
        [SecurityCritical]
        private static EventDescriptor[] eventDescriptors;
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private TraceCore()
        {
        }

        internal static void AppDomainUnload(DiagnosticTrace trace, string param0, string param1, string param2)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 0))
            {
                WriteEtwEvent(trace, 0, param0, param1, param2, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                string description = string.Format(Culture, ResourceManager.GetString("AppDomainUnload", Culture), new object[] { param0, param1, param2 });
                WriteTraceSource(trace, 0, description, payload);
            }
        }

        internal static bool AppDomainUnloadIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Informational))
            {
                return IsEtwEventEnabled(trace, 0);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void EnsureEventDescriptors()
        {
            if (object.ReferenceEquals(eventDescriptors, null))
            {
                eventDescriptors = new EventDescriptor[] { new EventDescriptor(0xe031, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe032, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe033, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe034, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe035, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe036, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe037, 0, 0x13, 1, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe038, 0, 0x13, 2, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe039, 0, 0x13, 4, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe03a, 0, 0x13, 5, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe03b, 0, 0x13, 3, 0, 0, 0x1000000000000000L), new EventDescriptor(0xe03c, 0, 0x13, 3, 0, 0, 0x1000000000000000L) };
            }
        }

        internal static void HandledException(DiagnosticTrace trace, Exception exception)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(trace, 1))
            {
                WriteEtwEvent(trace, 1, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                string description = string.Format(Culture, ResourceManager.GetString("HandledException", Culture), new object[0]);
                WriteTraceSource(trace, 1, description, payload);
            }
        }

        internal static bool HandledExceptionIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Informational))
            {
                return IsEtwEventEnabled(trace, 1);
            }
            return true;
        }

        internal static void HandledExceptionWarning(DiagnosticTrace trace, Exception exception)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(trace, 11))
            {
                WriteEtwEvent(trace, 11, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
            {
                string description = string.Format(Culture, ResourceManager.GetString("HandledExceptionWarning", Culture), new object[0]);
                WriteTraceSource(trace, 11, description, payload);
            }
        }

        internal static bool HandledExceptionWarningIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Warning))
            {
                return IsEtwEventEnabled(trace, 11);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool IsEtwEventEnabled(DiagnosticTrace trace, int eventIndex)
        {
            EnsureEventDescriptors();
            return trace.IsEtwEventEnabled(ref eventDescriptors[eventIndex]);
        }

        internal static void MaxInstancesExceeded(DiagnosticTrace trace, int limit)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 5))
            {
                WriteEtwEvent(trace, 5, limit, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
            {
                string description = string.Format(Culture, ResourceManager.GetString("MaxInstancesExceeded", Culture), new object[] { limit });
                WriteTraceSource(trace, 5, description, payload);
            }
        }

        internal static bool MaxInstancesExceededIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Warning))
            {
                return IsEtwEventEnabled(trace, 5);
            }
            return true;
        }

        internal static void ShipAssertExceptionMessage(DiagnosticTrace trace, string param0)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, null);
            if (IsEtwEventEnabled(trace, 2))
            {
                WriteEtwEvent(trace, 2, param0, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
            {
                string description = string.Format(Culture, ResourceManager.GetString("ShipAssertExceptionMessage", Culture), new object[] { param0 });
                WriteTraceSource(trace, 2, description, payload);
            }
        }

        internal static bool ShipAssertExceptionMessageIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Error))
            {
                return IsEtwEventEnabled(trace, 2);
            }
            return true;
        }

        internal static void ThrowingException(DiagnosticTrace trace, string param0, Exception exception)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(trace, 3))
            {
                WriteEtwEvent(trace, 3, param0, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
            {
                string description = string.Format(Culture, ResourceManager.GetString("ThrowingException", Culture), new object[] { param0 });
                WriteTraceSource(trace, 3, description, payload);
            }
        }

        internal static bool ThrowingExceptionIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Error))
            {
                return IsEtwEventEnabled(trace, 3);
            }
            return true;
        }

        internal static void TraceCodeEventLogCritical(DiagnosticTrace trace, TraceRecord traceRecord)
        {
            TracePayload payload = trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(trace, 6))
            {
                WriteEtwEvent(trace, 6, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Critical))
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCodeEventLogCritical", Culture), new object[0]);
                WriteTraceSource(trace, 6, description, payload);
            }
        }

        internal static bool TraceCodeEventLogCriticalIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Critical))
            {
                return IsEtwEventEnabled(trace, 6);
            }
            return true;
        }

        internal static void TraceCodeEventLogError(DiagnosticTrace trace, TraceRecord traceRecord)
        {
            TracePayload payload = trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(trace, 7))
            {
                WriteEtwEvent(trace, 7, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCodeEventLogError", Culture), new object[0]);
                WriteTraceSource(trace, 7, description, payload);
            }
        }

        internal static bool TraceCodeEventLogErrorIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Error))
            {
                return IsEtwEventEnabled(trace, 7);
            }
            return true;
        }

        internal static void TraceCodeEventLogInfo(DiagnosticTrace trace, TraceRecord traceRecord)
        {
            TracePayload payload = trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(trace, 8))
            {
                WriteEtwEvent(trace, 8, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Informational))
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCodeEventLogInfo", Culture), new object[0]);
                WriteTraceSource(trace, 8, description, payload);
            }
        }

        internal static bool TraceCodeEventLogInfoIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Informational))
            {
                return IsEtwEventEnabled(trace, 8);
            }
            return true;
        }

        internal static void TraceCodeEventLogVerbose(DiagnosticTrace trace, TraceRecord traceRecord)
        {
            TracePayload payload = trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(trace, 9))
            {
                WriteEtwEvent(trace, 9, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Verbose))
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCodeEventLogVerbose", Culture), new object[0]);
                WriteTraceSource(trace, 9, description, payload);
            }
        }

        internal static bool TraceCodeEventLogVerboseIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Verbose))
            {
                return IsEtwEventEnabled(trace, 9);
            }
            return true;
        }

        internal static void TraceCodeEventLogWarning(DiagnosticTrace trace, TraceRecord traceRecord)
        {
            TracePayload payload = trace.GetSerializedPayload(null, traceRecord, null);
            if (IsEtwEventEnabled(trace, 10))
            {
                WriteEtwEvent(trace, 10, payload.ExtendedData, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Warning))
            {
                string description = string.Format(Culture, ResourceManager.GetString("TraceCodeEventLogWarning", Culture), new object[0]);
                WriteTraceSource(trace, 10, description, payload);
            }
        }

        internal static bool TraceCodeEventLogWarningIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Warning))
            {
                return IsEtwEventEnabled(trace, 10);
            }
            return true;
        }

        internal static void UnhandledException(DiagnosticTrace trace, Exception exception)
        {
            TracePayload payload = trace.GetSerializedPayload(null, null, exception);
            if (IsEtwEventEnabled(trace, 4))
            {
                WriteEtwEvent(trace, 4, payload.SerializedException, payload.AppDomainFriendlyName);
            }
            if (trace.ShouldTraceToTraceSource(TraceEventLevel.Error))
            {
                string description = string.Format(Culture, ResourceManager.GetString("UnhandledException", Culture), new object[0]);
                WriteTraceSource(trace, 4, description, payload);
            }
        }

        internal static bool UnhandledExceptionIsEnabled(DiagnosticTrace trace)
        {
            if (!trace.ShouldTrace(TraceEventLevel.Error))
            {
                return IsEtwEventEnabled(trace, 4);
            }
            return true;
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, int eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], new object[] { eventParam0, eventParam1 });
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, string eventParam0, string eventParam1)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, string eventParam0, string eventParam1, string eventParam2)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2);
        }

        [SecuritySafeCritical]
        private static bool WriteEtwEvent(DiagnosticTrace trace, int eventIndex, string eventParam0, string eventParam1, string eventParam2, string eventParam3)
        {
            EnsureEventDescriptors();
            return trace.EtwProvider.WriteEvent(ref eventDescriptors[eventIndex], eventParam0, eventParam1, eventParam2, eventParam3);
        }

        [SecuritySafeCritical]
        private static void WriteTraceSource(DiagnosticTrace trace, int eventIndex, string description, TracePayload payload)
        {
            EnsureEventDescriptors();
            trace.WriteTraceSource(ref eventDescriptors[eventIndex], description, payload);
        }

        internal static CultureInfo Culture
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return resourceCulture;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                resourceCulture = value;
            }
        }

        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    resourceManager = new System.Resources.ResourceManager("System.Runtime.TraceCore", typeof(TraceCore).Assembly);
                }
                return resourceManager;
            }
        }
    }
}

