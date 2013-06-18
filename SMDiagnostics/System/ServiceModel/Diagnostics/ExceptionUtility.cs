namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Diagnostics;
    using System.Threading;

    internal class ExceptionUtility
    {
        [ThreadStatic]
        private static Guid activityId;
        private System.ServiceModel.Diagnostics.DiagnosticTrace diagnosticTrace;
        private string eventSourceName;
        private const string ExceptionStackAsStringKey = "System.ServiceModel.Diagnostics.ExceptionUtility.ExceptionStackAsString";
        internal static ExceptionUtility mainInstance;
        private string name;
        [ThreadStatic]
        private static bool useStaticActivityId;

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
        internal ExceptionUtility(string name, string eventSourceName, object diagnosticTrace)
        {
            this.diagnosticTrace = (System.ServiceModel.Diagnostics.DiagnosticTrace) diagnosticTrace;
            this.name = name;
            this.eventSourceName = eventSourceName;
        }

        internal static void ClearActivityId()
        {
            useStaticActivityId = false;
            activityId = Guid.Empty;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static bool IsInfrastructureException(Exception exception)
        {
            if (exception == null)
            {
                return false;
            }
            return ((exception is ThreadAbortException) || (exception is AppDomainUnloadedException));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Exception ThrowHelper(Exception exception, TraceEventType eventType)
        {
            return this.ThrowHelper(exception, eventType, null);
        }

        internal Exception ThrowHelper(Exception exception, TraceEventType eventType, TraceRecord extendedData)
        {
            bool flag = (this.diagnosticTrace != null) && this.diagnosticTrace.ShouldTrace(eventType);
            if (flag)
            {
                using (useStaticActivityId ? Activity.CreateActivity(activityId) : null)
                {
                    this.diagnosticTrace.TraceEvent(eventType, 0x20003, System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "ThrowingException"), TraceSR.GetString("ThrowingException"), extendedData, exception, null);
                }
            }
            string str = flag ? exception.StackTrace : null;
            if (!string.IsNullOrEmpty(str))
            {
                IDictionary data = exception.Data;
                if (((data != null) && !data.IsReadOnly) && !data.IsFixedSize)
                {
                    object obj2 = data["System.ServiceModel.Diagnostics.ExceptionUtility.ExceptionStackAsString"];
                    string str2 = (obj2 == null) ? "" : (obj2 as string);
                    if (str2 != null)
                    {
                        str2 = str2 + ((str2.Length == 0) ? "" : Environment.NewLine) + "throw" + Environment.NewLine + str + Environment.NewLine + "catch" + Environment.NewLine;
                        data["System.ServiceModel.Diagnostics.ExceptionUtility.ExceptionStackAsString"] = str2;
                    }
                }
            }
            return exception;
        }

        internal ArgumentException ThrowHelperArgument(string message)
        {
            return (ArgumentException) this.ThrowHelperError(new ArgumentException(message));
        }

        internal ArgumentException ThrowHelperArgument(string paramName, string message)
        {
            return (ArgumentException) this.ThrowHelperError(new ArgumentException(message, paramName));
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName)
        {
            return (ArgumentNullException) this.ThrowHelperError(new ArgumentNullException(paramName));
        }

        internal ArgumentNullException ThrowHelperArgumentNull(string paramName, string message)
        {
            return (ArgumentNullException) this.ThrowHelperError(new ArgumentNullException(paramName, message));
        }

        internal Exception ThrowHelperCallback(Exception innerException)
        {
            return this.ThrowHelperCallback(TraceSR.GetString("GenericCallbackException"), innerException);
        }

        internal Exception ThrowHelperCallback(string message, Exception innerException)
        {
            return this.ThrowHelperCritical(new CallbackException(message, innerException));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Exception ThrowHelperCritical(Exception exception)
        {
            return this.ThrowHelper(exception, TraceEventType.Critical);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Exception ThrowHelperError(Exception exception)
        {
            return this.ThrowHelper(exception, TraceEventType.Error);
        }

        internal Exception ThrowHelperFatal(string message, Exception innerException)
        {
            return this.ThrowHelperError(new FatalException(message, innerException));
        }

        internal Exception ThrowHelperInternal(bool fatal)
        {
            if (!fatal)
            {
                return Fx.AssertAndThrow("InternalException should never be thrown.");
            }
            return Fx.AssertAndThrowFatal("Fatal InternalException should never be thrown.");
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Exception ThrowHelperWarning(Exception exception)
        {
            return this.ThrowHelper(exception, TraceEventType.Warning);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
        internal void TraceFailFast(string message)
        {
            System.ServiceModel.Diagnostics.EventLogger logger = null;
            try
            {
                logger = new System.ServiceModel.Diagnostics.EventLogger(this.eventSourceName, this.diagnosticTrace);
            }
            finally
            {
                TraceFailFast(message, logger);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
        internal static void TraceFailFast(string message, System.ServiceModel.Diagnostics.EventLogger logger)
        {
            try
            {
                if (logger != null)
                {
                    string str = null;
                    try
                    {
                        str = new StackTrace().ToString();
                    }
                    catch (Exception exception)
                    {
                        str = exception.Message;
                    }
                    finally
                    {
                        logger.LogEvent(TraceEventType.Critical, EventLogCategory.FailFast, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073676186), new string[] { message, str });
                    }
                }
            }
            catch (Exception exception2)
            {
                if (logger != null)
                {
                    logger.LogEvent(TraceEventType.Critical, EventLogCategory.FailFast, (System.ServiceModel.Diagnostics.EventLogEventId) (-1073676185), new string[] { exception2.ToString() });
                }
                throw;
            }
        }

        [Obsolete("For SMDiagnostics.dll use only. Call DiagnosticUtility.ExceptionUtility instead")]
        internal void TraceFailFastException(Exception exception)
        {
            this.TraceFailFast((exception == null) ? null : exception.ToString());
        }

        internal void TraceHandledException(Exception exception, TraceEventType eventType)
        {
            if ((this.diagnosticTrace != null) && this.diagnosticTrace.ShouldTrace(eventType))
            {
                using (useStaticActivityId ? Activity.CreateActivity(activityId) : null)
                {
                    this.diagnosticTrace.TraceEvent(eventType, 0x20004, System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("System.ServiceModel.Diagnostics", "TraceHandledException"), TraceSR.GetString("TraceHandledException"), null, exception, null);
                }
            }
        }

        internal static void UseActivityId(Guid activityId)
        {
            ExceptionUtility.activityId = activityId;
            useStaticActivityId = true;
        }
    }
}

