namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics.PerformanceData;
    using System.Security;

    internal sealed class OperationPerformanceCountersV2 : OperationPerformanceCountersBase
    {
        private CounterData[] counters;
        private static CounterSet operationCounterSet;
        private static Guid operationCounterSetId = new Guid("{8ebb0470-da6d-485b-8441-8e06b049157a}");
        private CounterSetInstance operationCounterSetInstance;
        private static Guid serviceModelProviderId = new Guid("{890c10c3-8c2a-4fe3-a36a-9eca153d47cb}");
        private static object syncRoot = new object();

        internal OperationPerformanceCountersV2(string service, string contract, string operationName, string uri) : base(service, contract, operationName, uri)
        {
            EnsureCounterSet();
            this.operationCounterSetInstance = CreateCounterSetInstance(this.InstanceName);
            this.counters = new CounterData[15];
            for (int i = 0; i < 15; i++)
            {
                this.counters[i] = this.operationCounterSetInstance.Counters[i];
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
            return new CounterSet(serviceModelProviderId, operationCounterSetId, CounterSetInstanceType.Multiple);
        }

        [SecuritySafeCritical]
        private static CounterSetInstance CreateCounterSetInstance(string name)
        {
            return operationCounterSet.CreateCounterSetInstance(name);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && PerformanceCounters.PerformanceCountersEnabled) && (this.operationCounterSetInstance != null))
                {
                    this.operationCounterSetInstance.Dispose();
                    this.operationCounterSetInstance = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal static void EnsureCounterSet()
        {
            if (operationCounterSet == null)
            {
                lock (syncRoot)
                {
                    if (operationCounterSet == null)
                    {
                        CounterSet set = CreateCounterSet();
                        set.AddCounter(0, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[0]);
                        set.AddCounter(1, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[1]);
                        set.AddCounter(2, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[2]);
                        set.AddCounter(3, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[3]);
                        set.AddCounter(4, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[4]);
                        set.AddCounter(5, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[5]);
                        set.AddCounter(6, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[6]);
                        set.AddCounter(8, CounterType.AverageBase, OperationPerformanceCountersBase.perfCounterNames[8]);
                        set.AddCounter(7, CounterType.AverageTimer32, OperationPerformanceCountersBase.perfCounterNames[7]);
                        set.AddCounter(9, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[9]);
                        set.AddCounter(10, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[10]);
                        set.AddCounter(11, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[11]);
                        set.AddCounter(12, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[12]);
                        set.AddCounter(13, CounterType.RawData32, OperationPerformanceCountersBase.perfCounterNames[13]);
                        set.AddCounter(14, CounterType.RateOfCountPerSecond32, OperationPerformanceCountersBase.perfCounterNames[14]);
                        operationCounterSet = set;
                    }
                }
            }
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

        internal override void TxFlowed()
        {
            this.counters[13].Increment();
            this.counters[14].Increment();
        }

        internal override bool Initialized
        {
            get
            {
                return (this.operationCounterSetInstance != null);
            }
        }
    }
}

