namespace System.ServiceModel
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.ServiceModel.Diagnostics;

    internal static class DiagnosticUtility
    {
        internal const string DefaultTraceListenerName = "Default";
        private static System.ServiceModel.Diagnostics.DiagnosticTrace diagnosticTrace = InitializeTracing();
        internal const string EventSourceName = "System.ServiceModel 4.0.0.0";
        private static System.ServiceModel.Diagnostics.ExceptionUtility exceptionUtility = null;
        private static SourceLevels level = SourceLevels.Off;
        private static object lockObject = new object();
        private static bool shouldTraceCritical = false;
        private static bool shouldTraceError = false;
        private static bool shouldTraceInformation = false;
        private static bool shouldTraceVerbose = false;
        private static bool shouldTraceWarning = false;
        private static bool shouldUseActivity = false;
        private const string TraceSourceName = "System.ServiceModel";
        private static bool tracingEnabled = false;
        private static System.ServiceModel.Diagnostics.Utility utility = null;

        [MethodImpl(MethodImplOptions.NoInlining), Conditional("DEBUG")]
        internal static void DebugAssert(string message)
        {
            AssertUtility.DebugAssertCore(message);
        }

        [Conditional("DEBUG")]
        internal static void DebugAssert(bool condition, string message)
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception FailFast(string message)
        {
            try
            {
                try
                {
                    ExceptionUtility.TraceFailFast(message);
                }
                finally
                {
                    Environment.FailFast(message);
                }
            }
            catch
            {
            }
            Environment.FailFast(message);
            return null;
        }

        private static System.ServiceModel.Diagnostics.ExceptionUtility GetExceptionUtility()
        {
            lock (lockObject)
            {
                if (exceptionUtility == null)
                {
                    exceptionUtility = new System.ServiceModel.Diagnostics.ExceptionUtility("System.ServiceModel", "System.ServiceModel 4.0.0.0", diagnosticTrace);
                }
            }
            return exceptionUtility;
        }

        private static System.ServiceModel.Diagnostics.Utility GetUtility()
        {
            lock (lockObject)
            {
                if (utility == null)
                {
                    utility = new System.ServiceModel.Diagnostics.Utility(ExceptionUtility);
                }
            }
            return utility;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        internal static void InitDiagnosticTraceImpl(TraceSourceKind sourceType, string traceSourceName)
        {
            diagnosticTrace = new System.ServiceModel.Diagnostics.DiagnosticTrace(sourceType, traceSourceName, "System.ServiceModel 4.0.0.0");
            UpdateLevel();
        }

        private static System.ServiceModel.Diagnostics.DiagnosticTrace InitializeTracing()
        {
            InitDiagnosticTraceImpl(TraceSourceKind.DiagnosticTraceSource, "System.ServiceModel");
            if (!diagnosticTrace.HaveListeners)
            {
                diagnosticTrace = null;
            }
            return diagnosticTrace;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Exception InvokeFinalHandler(Exception exception)
        {
            try
            {
                try
                {
                    ExceptionUtility.TraceFailFastException(exception);
                }
                finally
                {
                    Environment.FailFast(null);
                }
            }
            catch
            {
            }
            Environment.FailFast(null);
            return null;
        }

        internal static bool ShouldTrace(TraceEventType type)
        {
            bool flag = false;
            if (TracingEnabled)
            {
                switch (type)
                {
                    case TraceEventType.Critical:
                        return ShouldTraceCritical;

                    case TraceEventType.Error:
                        return ShouldTraceError;

                    case (TraceEventType.Error | TraceEventType.Critical):
                        return flag;

                    case TraceEventType.Warning:
                        return ShouldTraceWarning;

                    case TraceEventType.Information:
                        return ShouldTraceInformation;

                    case TraceEventType.Verbose:
                        return ShouldTraceVerbose;
                }
            }
            return flag;
        }

        private static void UpdateLevel()
        {
            level = DiagnosticTrace.Level;
            tracingEnabled = DiagnosticTrace.TracingEnabled;
            shouldTraceCritical = DiagnosticTrace.ShouldTrace(TraceEventType.Critical);
            shouldTraceError = DiagnosticTrace.ShouldTrace(TraceEventType.Error);
            shouldTraceInformation = DiagnosticTrace.ShouldTrace(TraceEventType.Information);
            shouldTraceWarning = DiagnosticTrace.ShouldTrace(TraceEventType.Warning);
            shouldTraceVerbose = DiagnosticTrace.ShouldTrace(TraceEventType.Verbose);
            shouldUseActivity = DiagnosticTrace.ShouldUseActivity;
            WaitCallbackActionItem.ShouldUseActivity = shouldUseActivity;
        }

        internal static System.ServiceModel.Diagnostics.DiagnosticTrace DiagnosticTrace
        {
            get
            {
                return diagnosticTrace;
            }
        }

        internal static EventLogger EventLog
        {
            get
            {
                return new EventLogger("System.ServiceModel 4.0.0.0", diagnosticTrace);
            }
        }

        internal static System.ServiceModel.Diagnostics.ExceptionUtility ExceptionUtility
        {
            get
            {
                return (exceptionUtility ?? GetExceptionUtility());
            }
        }

        internal static SourceLevels Level
        {
            get
            {
                return level;
            }
            set
            {
                if (diagnosticTrace != null)
                {
                    DiagnosticTrace.Level = value;
                    UpdateLevel();
                }
            }
        }

        internal static bool ShouldTraceCritical
        {
            get
            {
                return shouldTraceCritical;
            }
        }

        internal static bool ShouldTraceError
        {
            get
            {
                return shouldTraceError;
            }
        }

        internal static bool ShouldTraceInformation
        {
            get
            {
                return shouldTraceInformation;
            }
        }

        internal static bool ShouldTraceVerbose
        {
            get
            {
                return shouldTraceVerbose;
            }
        }

        internal static bool ShouldTraceWarning
        {
            get
            {
                return shouldTraceWarning;
            }
        }

        internal static bool ShouldUseActivity
        {
            get
            {
                return shouldUseActivity;
            }
            set
            {
                shouldUseActivity = value;
            }
        }

        internal static bool TracingEnabled
        {
            get
            {
                return tracingEnabled;
            }
            set
            {
                tracingEnabled = value;
            }
        }

        internal static EventLogger UnsafeEventLog
        {
            [SecuritySafeCritical]
            get
            {
                return EventLogger.UnsafeCreateEventLogger("System.ServiceModel 4.0.0.0", diagnosticTrace);
            }
        }

        internal static System.ServiceModel.Diagnostics.Utility Utility
        {
            get
            {
                return (utility ?? GetUtility());
            }
        }
    }
}

