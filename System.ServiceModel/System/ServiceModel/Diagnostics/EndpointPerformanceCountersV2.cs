namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Security;

    internal sealed class EndpointPerformanceCountersV2 : EndpointPerformanceCountersBase
    {
        private CounterData[] counters;
        private static CounterSet endpointCounterSet;
        private static Guid endpointCounterSetId = new Guid("{16dcff2c-91a3-4e6a-8135-0a9e6681c1b5}");
        private CounterSetInstance endpointCounterSetInstance;
        private static Guid serviceModelProviderId = new Guid("{890c10c3-8c2a-4fe3-a36a-9eca153d47cb}");
        private static object syncRoot = new object();

        internal EndpointPerformanceCountersV2(string service, string contract, string uri) : base(service, contract, uri)
        {
            EnsureCounterSet();
            this.endpointCounterSetInstance = CreateCounterSetInstance(this.InstanceName);
            this.counters = new CounterData[0x13];
            for (int i = 0; i < 0x13; i++)
            {
                this.counters[i] = this.endpointCounterSetInstance.Counters[i];
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
            return new CounterSet(serviceModelProviderId, endpointCounterSetId, CounterSetInstanceType.Multiple);
        }

        [SecuritySafeCritical]
        private static CounterSetInstance CreateCounterSetInstance(string name)
        {
            return endpointCounterSet.CreateCounterSetInstance(name);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && PerformanceCounters.PerformanceCountersEnabled) && (this.endpointCounterSetInstance != null))
                {
                    this.endpointCounterSetInstance.Dispose();
                    this.endpointCounterSetInstance = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal static void EnsureCounterSet()
        {
            if (endpointCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (endpointCounterSet == null)
                    {
                        CounterSet set = CreateCounterSet();
                        set.AddCounter(0, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[0]);
                        set.AddCounter(1, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[1]);
                        set.AddCounter(2, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[2]);
                        set.AddCounter(3, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[3]);
                        set.AddCounter(4, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[4]);
                        set.AddCounter(5, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[5]);
                        set.AddCounter(6, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[6]);
                        set.AddCounter(8, CounterType.AverageBase, EndpointPerformanceCountersBase.perfCounterNames[8]);
                        set.AddCounter(7, CounterType.AverageTimer32, EndpointPerformanceCountersBase.perfCounterNames[7]);
                        set.AddCounter(9, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[9]);
                        set.AddCounter(10, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[10]);
                        set.AddCounter(11, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[11]);
                        set.AddCounter(12, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[12]);
                        set.AddCounter(13, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[13]);
                        set.AddCounter(14, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[14]);
                        set.AddCounter(15, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[15]);
                        set.AddCounter(0x10, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[0x10]);
                        set.AddCounter(0x11, CounterType.RawData32, EndpointPerformanceCountersBase.perfCounterNames[0x11]);
                        set.AddCounter(0x12, CounterType.RateOfCountPerSecond32, EndpointPerformanceCountersBase.perfCounterNames[0x12]);
                        endpointCounterSet = set;
                    }
                }
            }
        }

        internal override void MessageDropped()
        {
            this.counters[15].Increment();
            this.counters[0x10].Increment();
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

        internal override void SaveCallDuration(long time)
        {
            this.counters[7].IncrementBy(time);
            this.counters[8].Increment();
        }

        internal override void SessionFaulted()
        {
            this.counters[13].Increment();
            this.counters[14].Increment();
        }

        internal override void TxFlowed()
        {
            this.counters[0x11].Increment();
            this.counters[0x12].Increment();
        }

        internal override bool Initialized
        {
            get
            {
                return (this.endpointCounterSetInstance != null);
            }
        }
    }
}

