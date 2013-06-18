namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal sealed class OperationPerformanceCounters : OperationPerformanceCountersBase
    {
        internal OperationPerformanceCounters(string service, string contract, string operationName, string uri) : base(service, contract, operationName, uri)
        {
            this.Counters = new PerformanceCounter[15];
            for (int i = 0; i < 15; i++)
            {
                PerformanceCounter operationPerformanceCounter = PerformanceCounters.GetOperationPerformanceCounter(OperationPerformanceCountersBase.perfCounterNames[i], base.instanceName);
                if (operationPerformanceCounter == null)
                {
                    break;
                }
                try
                {
                    operationPerformanceCounter.RawValue = 0L;
                    this.Counters[i] = operationPerformanceCounter;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x80038, System.ServiceModel.SR.GetString("TraceCodePerformanceCounterFailedToLoad"), null, exception);
                    }
                    break;
                }
            }
        }

        internal override void AuthenticationFailed()
        {
            this.Increment(9);
            this.Increment(10);
        }

        internal override void AuthorizationFailed()
        {
            this.Increment(11);
            this.Increment(12);
        }

        private void Decrement(int counter)
        {
            this.Decrement(this.Counters, counter);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && PerformanceCounters.PerformanceCountersEnabled) && (this.Counters != null))
                {
                    for (int i = this.PerfCounterStart; i < this.PerfCounterEnd; i++)
                    {
                        PerformanceCounter counter = this.Counters[i];
                        if (counter != null)
                        {
                            PerformanceCounters.ReleasePerformanceCounter(ref counter);
                        }
                        this.Counters[i] = null;
                    }
                    this.Counters = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void Increment(int counter)
        {
            this.Increment(this.Counters, counter);
        }

        private void IncrementBy(int counter, long time)
        {
            this.IncrementBy(this.Counters, counter, time);
        }

        internal override void MethodCalled()
        {
            this.Increment(0);
            this.Increment(1);
            this.Increment(2);
        }

        internal override void MethodReturnedError()
        {
            this.Increment(3);
            this.Increment(4);
            this.Decrement(2);
        }

        internal override void MethodReturnedFault()
        {
            this.Increment(5);
            this.Increment(6);
            this.Decrement(2);
        }

        internal override void MethodReturnedSuccess()
        {
            this.Decrement(2);
        }

        internal override void SaveCallDuration(long time)
        {
            this.IncrementBy(7, time);
            this.Increment(8);
        }

        internal override void TxFlowed()
        {
            this.Increment(13);
            this.Increment(14);
        }

        internal PerformanceCounter[] Counters { get; set; }

        internal override bool Initialized
        {
            get
            {
                return (this.Counters != null);
            }
        }
    }
}

