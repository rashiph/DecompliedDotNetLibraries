namespace System.Runtime
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;

    internal class ExceptionTrace
    {
        private string eventSourceName;
        private const ushort FailFastEventLogCategory = 6;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExceptionTrace(string eventSourceName)
        {
            this.eventSourceName = eventSourceName;
        }

        public ArgumentException Argument(string paramName, string message)
        {
            return this.TraceException<ArgumentException>(new ArgumentException(message, paramName));
        }

        public ArgumentNullException ArgumentNull(string paramName)
        {
            return this.TraceException<ArgumentNullException>(new ArgumentNullException(paramName));
        }

        public ArgumentNullException ArgumentNull(string paramName, string message)
        {
            return this.TraceException<ArgumentNullException>(new ArgumentNullException(paramName, message));
        }

        public ArgumentException ArgumentNullOrEmpty(string paramName)
        {
            return this.Argument(paramName, SRCore.ArgumentNullOrEmpty(paramName));
        }

        public ArgumentOutOfRangeException ArgumentOutOfRange(string paramName, object actualValue, string message)
        {
            return this.TraceException<ArgumentOutOfRangeException>(new ArgumentOutOfRangeException(paramName, actualValue, message));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Exception AsError(Exception exception)
        {
            return this.TraceException<Exception>(exception);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public Exception AsError(Exception exception, string eventSource)
        {
            return this.TraceException<Exception>(exception, eventSource);
        }

        public void AsInformation(Exception exception)
        {
            TraceCore.HandledException(Fx.Trace, exception);
        }

        public void AsWarning(Exception exception)
        {
            TraceCore.HandledExceptionWarning(Fx.Trace, exception);
        }

        [SecuritySafeCritical]
        private void BreakOnException(Exception exception)
        {
        }

        public ObjectDisposedException ObjectDisposed(string message)
        {
            return this.TraceException<ObjectDisposedException>(new ObjectDisposedException(null, message));
        }

        private TException TraceException<TException>(TException exception) where TException: Exception
        {
            return this.TraceException<TException>(exception, this.eventSourceName);
        }

        [SecuritySafeCritical]
        private TException TraceException<TException>(TException exception, string eventSource) where TException: Exception
        {
            TraceCore.ThrowingException(Fx.Trace, eventSource, exception);
            this.BreakOnException(exception);
            return exception;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void TraceFailFast(string message)
        {
            EventLogger logger = null;
            logger = new EventLogger(this.eventSourceName, Fx.Trace);
            this.TraceFailFast(message, logger);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal void TraceFailFast(string message, EventLogger logger)
        {
            if (logger != null)
            {
                try
                {
                    string str = null;
                    try
                    {
                        str = new StackTrace().ToString();
                    }
                    catch (Exception exception)
                    {
                        str = exception.Message;
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        logger.LogEvent(TraceEventType.Critical, 6, 0xc0010066, new string[] { message, str });
                    }
                }
                catch (Exception exception2)
                {
                    logger.LogEvent(TraceEventType.Critical, 6, 0xc0010067, new string[] { exception2.ToString() });
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                }
            }
        }

        public void TraceUnhandledException(Exception exception)
        {
            TraceCore.UnhandledException(Fx.Trace, exception);
        }
    }
}

