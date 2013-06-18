namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal sealed class ServicePerformanceCounters : ServicePerformanceCountersBase
    {
        internal ServicePerformanceCounters(ServiceHostBase serviceHost) : base(serviceHost)
        {
            this.Counters = new PerformanceCounter[0x27];
            for (int i = 0; i < 0x27; i++)
            {
                PerformanceCounter servicePerformanceCounter = PerformanceCounters.GetServicePerformanceCounter(ServicePerformanceCountersBase.perfCounterNames[i], this.InstanceName);
                if (servicePerformanceCounter == null)
                {
                    break;
                }
                try
                {
                    servicePerformanceCounter.RawValue = 0L;
                    this.Counters[i] = servicePerformanceCounter;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x8003b, System.ServiceModel.SR.GetString("TraceCodePerformanceCountersFailedForService"), null, exception);
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

        internal override void DecrementThrottlePercent(int counterIndex)
        {
            this.Decrement(counterIndex);
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

        internal override void IncrementThrottlePercent(int counterIndex)
        {
            this.Increment(counterIndex);
        }

        internal override void MessageDropped()
        {
            this.Increment(0x11);
            this.Increment(0x12);
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

        internal override void MsmqDroppedMessage()
        {
            this.Increment(0x1f);
            this.Increment(0x20);
        }

        internal override void MsmqPoisonMessage()
        {
            this.Increment(0x1b);
            this.Increment(0x1c);
        }

        internal override void MsmqRejectedMessage()
        {
            this.Increment(0x1d);
            this.Increment(30);
        }

        internal override void SaveCallDuration(long time)
        {
            this.IncrementBy(7, time);
            this.Increment(8);
        }

        internal override void ServiceInstanceCreated()
        {
            this.Increment(13);
            this.Increment(14);
        }

        internal override void ServiceInstanceRemoved()
        {
            this.Decrement(13);
        }

        internal override void SessionFaulted()
        {
            this.Increment(15);
            this.Increment(0x10);
        }

        private void Set(int counter, long denominator)
        {
            this.Set(this.Counters, counter, denominator);
        }

        internal override void SetThrottleBase(int counterIndex, long denominator)
        {
            this.Set(counterIndex, denominator);
        }

        internal override void TxAborted(long count)
        {
            this.IncrementBy(0x17, count);
            this.IncrementBy(0x18, count);
        }

        internal override void TxCommitted(long count)
        {
            this.IncrementBy(0x15, count);
            this.IncrementBy(0x16, count);
        }

        internal override void TxFlowed()
        {
            this.Increment(0x13);
            this.Increment(20);
        }

        internal override void TxInDoubt(long count)
        {
            this.IncrementBy(0x19, count);
            this.IncrementBy(0x1a, count);
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

