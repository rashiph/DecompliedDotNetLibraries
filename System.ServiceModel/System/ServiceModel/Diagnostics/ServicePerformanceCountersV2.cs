namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Security;
    using System.ServiceModel;

    internal sealed class ServicePerformanceCountersV2 : ServicePerformanceCountersBase
    {
        private CounterData[] counters;
        private static CounterSet serviceCounterSet;
        private static Guid serviceCounterSetId = new Guid("{e829b6db-21ab-453b-83c9-d980ec708edd}");
        private CounterSetInstance serviceCounterSetInstance;
        private static Guid serviceModelProviderId = new Guid("{890c10c3-8c2a-4fe3-a36a-9eca153d47cb}");
        private static object syncRoot = new object();

        internal ServicePerformanceCountersV2(ServiceHostBase serviceHost) : base(serviceHost)
        {
            if (serviceCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (serviceCounterSet == null)
                    {
                        CounterSet set = CreateCounterSet();
                        set.AddCounter(0, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0]);
                        set.AddCounter(1, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[1]);
                        set.AddCounter(2, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[2]);
                        set.AddCounter(3, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[3]);
                        set.AddCounter(4, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[4]);
                        set.AddCounter(5, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[5]);
                        set.AddCounter(6, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[6]);
                        set.AddCounter(8, CounterType.AverageBase, ServicePerformanceCountersBase.perfCounterNames[8]);
                        set.AddCounter(7, CounterType.AverageTimer32, ServicePerformanceCountersBase.perfCounterNames[7]);
                        set.AddCounter(9, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[9]);
                        set.AddCounter(10, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[10]);
                        set.AddCounter(11, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[11]);
                        set.AddCounter(12, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[12]);
                        set.AddCounter(13, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[13]);
                        set.AddCounter(14, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[14]);
                        set.AddCounter(15, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[15]);
                        set.AddCounter(0x10, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x10]);
                        set.AddCounter(0x11, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x11]);
                        set.AddCounter(0x12, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x12]);
                        set.AddCounter(0x13, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x13]);
                        set.AddCounter(20, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[20]);
                        set.AddCounter(0x15, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x15]);
                        set.AddCounter(0x16, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x16]);
                        set.AddCounter(0x17, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x17]);
                        set.AddCounter(0x18, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x18]);
                        set.AddCounter(0x19, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x19]);
                        set.AddCounter(0x1a, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x1a]);
                        set.AddCounter(0x1b, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x1b]);
                        set.AddCounter(0x1c, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x1c]);
                        set.AddCounter(0x1d, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x1d]);
                        set.AddCounter(30, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[30]);
                        set.AddCounter(0x1f, CounterType.RawData32, ServicePerformanceCountersBase.perfCounterNames[0x1f]);
                        set.AddCounter(0x20, CounterType.RateOfCountPerSecond32, ServicePerformanceCountersBase.perfCounterNames[0x20]);
                        set.AddCounter(0x21, CounterType.RawFraction32, ServicePerformanceCountersBase.perfCounterNames[0x21]);
                        set.AddCounter(0x22, CounterType.RawBase32, ServicePerformanceCountersBase.perfCounterNames[0x22]);
                        set.AddCounter(0x23, CounterType.RawFraction32, ServicePerformanceCountersBase.perfCounterNames[0x23]);
                        set.AddCounter(0x24, CounterType.RawBase32, ServicePerformanceCountersBase.perfCounterNames[0x24]);
                        set.AddCounter(0x25, CounterType.RawFraction32, ServicePerformanceCountersBase.perfCounterNames[0x25]);
                        set.AddCounter(0x26, CounterType.RawBase32, ServicePerformanceCountersBase.perfCounterNames[0x26]);
                        serviceCounterSet = set;
                    }
                }
            }
            this.serviceCounterSetInstance = CreateCounterSetInstance(this.InstanceName);
            this.counters = new CounterData[0x27];
            for (int i = 0; i < 0x27; i++)
            {
                this.counters[i] = this.serviceCounterSetInstance.Counters[i];
                this.counters[i].Value = 0L;
            }
        }

        internal override void AuthenticationFailed()
        {
            this.counters[9].Increment();
            this.counters[10].Increment();
        }

        internal override void AuthorizationFailed()
        {
            this.counters[11].Increment();
            this.counters[12].Increment();
        }

        [SecuritySafeCritical]
        private static CounterSet CreateCounterSet()
        {
            return new CounterSet(serviceModelProviderId, serviceCounterSetId, CounterSetInstanceType.Multiple);
        }

        [SecuritySafeCritical]
        private static CounterSetInstance CreateCounterSetInstance(string name)
        {
            return serviceCounterSet.CreateCounterSetInstance(name);
        }

        internal override void DecrementThrottlePercent(int counterIndex)
        {
            this.counters[counterIndex].Decrement();
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && PerformanceCounters.PerformanceCountersEnabled) && (this.serviceCounterSetInstance != null))
                {
                    this.serviceCounterSetInstance.Dispose();
                    this.serviceCounterSetInstance = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal override void IncrementThrottlePercent(int counterIndex)
        {
            this.counters[counterIndex].Increment();
        }

        internal override void MessageDropped()
        {
            this.counters[0x11].Increment();
            this.counters[0x12].Increment();
        }

        internal override void MethodCalled()
        {
            this.counters[0].Increment();
            this.counters[1].Increment();
            this.counters[2].Increment();
        }

        internal override void MethodReturnedError()
        {
            this.counters[3].Increment();
            this.counters[4].Increment();
            this.counters[2].Decrement();
        }

        internal override void MethodReturnedFault()
        {
            this.counters[5].Increment();
            this.counters[6].Increment();
            this.counters[2].Decrement();
        }

        internal override void MethodReturnedSuccess()
        {
            this.counters[2].Decrement();
        }

        internal override void MsmqDroppedMessage()
        {
            this.counters[0x1f].Increment();
            this.counters[0x20].Increment();
        }

        internal override void MsmqPoisonMessage()
        {
            this.counters[0x1b].Increment();
            this.counters[0x1c].Increment();
        }

        internal override void MsmqRejectedMessage()
        {
            this.counters[0x1d].Increment();
            this.counters[30].Increment();
        }

        internal override void SaveCallDuration(long time)
        {
            this.counters[7].IncrementBy(time);
            this.counters[8].Increment();
        }

        internal override void ServiceInstanceCreated()
        {
            this.counters[13].Increment();
            this.counters[14].Increment();
        }

        internal override void ServiceInstanceRemoved()
        {
            this.counters[13].Decrement();
        }

        internal override void SessionFaulted()
        {
            this.counters[15].Increment();
            this.counters[0x10].Increment();
        }

        internal override void SetThrottleBase(int counterIndex, long denominator)
        {
            this.counters[counterIndex].Value = denominator;
        }

        internal override void TxAborted(long count)
        {
            this.counters[0x17].Increment();
            this.counters[0x18].Increment();
        }

        internal override void TxCommitted(long count)
        {
            this.counters[0x15].Increment();
            this.counters[0x16].Increment();
        }

        internal override void TxFlowed()
        {
            this.counters[0x13].Increment();
            this.counters[20].Increment();
        }

        internal override void TxInDoubt(long count)
        {
            this.counters[0x19].Increment();
            this.counters[0x1a].Increment();
        }

        internal override bool Initialized
        {
            get
            {
                return (this.serviceCounterSetInstance != null);
            }
        }
    }
}

