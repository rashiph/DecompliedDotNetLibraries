namespace System.Activities.DurableInstancing
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;

    internal static class FxTrace
    {
        private const string baseEventSourceName = "System.Activities.DurableInstancing";
        private static DiagnosticTrace diagnosticTrace;
        private static bool[] enabledEvents;
        private static Guid etwProviderId;
        [SecurityCritical]
        private static EventDescriptor[] eventDescriptors;
        private static string eventSourceName;
        private const string EventSourceVersion = "4.0.0.0";
        private static ExceptionTrace exceptionTrace;
        private static object lockObject = new object();
        private static bool shouldTraceCritical = true;
        private static bool shouldTraceCriticalToTraceSource = true;
        private static bool shouldTraceError = true;
        private static bool shouldTraceErrorToTraceSource = true;
        private static bool shouldTraceInformation = true;
        private static bool shouldTraceInformationToTraceSource = true;
        private static bool shouldTraceVerbose = true;
        private static bool shouldTraceVerboseToTraceSource = true;
        private static bool shouldTraceWarning = true;
        private static bool shouldTraceWarningToTraceSource = true;
        private static bool tracingEnabled = true;

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public static System.Exception FailFast(string message)
        {
            try
            {
                try
                {
                    Exception.TraceFailFast(message);
                }
                finally
                {
                    Environment.FailFast(message);
                }
            }
            catch
            {
                throw;
            }
            return null;
        }

        [SecuritySafeCritical]
        private static DiagnosticTrace InitializeTracing()
        {
            etwProviderId = DiagnosticTrace.DefaultEtwProviderId;
            DiagnosticTrace trace = new DiagnosticTrace("System.Activities.DurableInstancing", etwProviderId);
            if (trace.EtwProvider != null)
            {
                trace.RefreshState = (Action) Delegate.Combine(trace.RefreshState, delegate {
                    UpdateLevel();
                });
            }
            UpdateLevel(trace);
            return trace;
        }

        public static bool IsEventEnabled(int index)
        {
            if (enabledEvents != null)
            {
                return enabledEvents[index];
            }
            return true;
        }

        [SecuritySafeCritical]
        private static void UpdateEnabledEventsList()
        {
            if (eventDescriptors != null)
            {
                if (enabledEvents == null)
                {
                    enabledEvents = new bool[eventDescriptors.Length];
                }
                for (int i = 0; i < enabledEvents.Length; i++)
                {
                    enabledEvents[i] = Trace.IsEtwEventEnabled(ref eventDescriptors[i]);
                }
            }
        }

        [SecuritySafeCritical]
        public static void UpdateEventDefinitions(EventDescriptor[] eventDescriptors)
        {
            FxTrace.eventDescriptors = eventDescriptors;
            UpdateEnabledEventsList();
        }

        private static void UpdateLevel()
        {
            UpdateLevel(Trace);
        }

        private static void UpdateLevel(DiagnosticTrace trace)
        {
            tracingEnabled = trace.TracingEnabled;
            shouldTraceCriticalToTraceSource = trace.ShouldTraceToTraceSource(TraceEventLevel.Critical);
            shouldTraceErrorToTraceSource = trace.ShouldTraceToTraceSource(TraceEventLevel.Error);
            shouldTraceWarningToTraceSource = trace.ShouldTraceToTraceSource(TraceEventLevel.Warning);
            shouldTraceInformationToTraceSource = trace.ShouldTraceToTraceSource(TraceEventLevel.Informational);
            shouldTraceVerboseToTraceSource = trace.ShouldTraceToTraceSource(TraceEventLevel.Verbose);
            shouldTraceCritical = shouldTraceCriticalToTraceSource || trace.ShouldTraceToEtw(TraceEventLevel.Critical);
            shouldTraceError = shouldTraceErrorToTraceSource || trace.ShouldTraceToEtw(TraceEventLevel.Error);
            shouldTraceWarning = shouldTraceWarningToTraceSource || trace.ShouldTraceToEtw(TraceEventLevel.Warning);
            shouldTraceInformation = shouldTraceInformationToTraceSource || trace.ShouldTraceToEtw(TraceEventLevel.Informational);
            shouldTraceVerbose = shouldTraceVerboseToTraceSource || trace.ShouldTraceToEtw(TraceEventLevel.Verbose);
            UpdateEnabledEventsList();
        }

        public static EventLogger EventLog
        {
            get
            {
                return new EventLogger(EventSourceName, Trace);
            }
        }

        private static string EventSourceName
        {
            get
            {
                if (eventSourceName == null)
                {
                    eventSourceName = "System.Activities.DurableInstancing" + " " + "4.0.0.0";
                }
                return eventSourceName;
            }
        }

        public static ExceptionTrace Exception
        {
            get
            {
                if (exceptionTrace == null)
                {
                    exceptionTrace = new ExceptionTrace(EventSourceName);
                }
                return exceptionTrace;
            }
        }

        public static bool ShouldTraceCritical
        {
            get
            {
                return shouldTraceCritical;
            }
        }

        public static bool ShouldTraceCriticalToTraceSource
        {
            get
            {
                return shouldTraceCriticalToTraceSource;
            }
        }

        public static bool ShouldTraceError
        {
            get
            {
                return shouldTraceError;
            }
        }

        public static bool ShouldTraceErrorToTraceSource
        {
            get
            {
                return shouldTraceErrorToTraceSource;
            }
        }

        public static bool ShouldTraceInformation
        {
            get
            {
                return shouldTraceInformation;
            }
        }

        public static bool ShouldTraceInformationToTraceSource
        {
            get
            {
                return shouldTraceInformationToTraceSource;
            }
        }

        public static bool ShouldTraceVerbose
        {
            get
            {
                return shouldTraceVerbose;
            }
        }

        public static bool ShouldTraceVerboseToTraceSource
        {
            get
            {
                return shouldTraceVerboseToTraceSource;
            }
        }

        public static bool ShouldTraceWarning
        {
            get
            {
                return shouldTraceWarning;
            }
        }

        public static bool ShouldTraceWarningToTraceSource
        {
            get
            {
                return shouldTraceWarningToTraceSource;
            }
        }

        public static DiagnosticTrace Trace
        {
            get
            {
                if (diagnosticTrace == null)
                {
                    lock (lockObject)
                    {
                        if (diagnosticTrace == null)
                        {
                            diagnosticTrace = InitializeTracing();
                        }
                    }
                }
                return diagnosticTrace;
            }
        }

        public static bool TracingEnabled
        {
            get
            {
                return tracingEnabled;
            }
        }
    }
}

