namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class PerformanceCounterWrapper
    {
        private PerformanceCounter counter;

        public PerformanceCounterWrapper(string counterName)
        {
            Exception exception;
            string categoryName = "MSDTC Bridge 4.0.0.0";
            try
            {
                this.counter = new PerformanceCounter(categoryName, counterName, string.Empty, false);
                this.counter.RemoveInstance();
                this.counter = new PerformanceCounter(categoryName, counterName, string.Empty, false);
                exception = null;
            }
            catch (InvalidOperationException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                exception = exception2;
            }
            catch (Win32Exception exception3)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                exception = exception3;
            }
            if (exception != null)
            {
                if (DebugTrace.Error)
                {
                    DebugTrace.Trace(TraceLevel.Error, "Unable to initialize performance counter {0}: {1}", counterName, exception);
                }
                PerformanceCounterInitializationFailureRecord.TraceAndLog(PluggableProtocol10.ProtocolGuid, counterName, exception);
            }
        }

        public long Increment()
        {
            if (this.counter != null)
            {
                return this.counter.Increment();
            }
            return 0L;
        }

        public long IncrementBy(long value)
        {
            if (this.counter != null)
            {
                return this.counter.IncrementBy(value);
            }
            return 0L;
        }
    }
}

