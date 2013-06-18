namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal class DefaultPerformanceCounters : PerformanceCountersBase
    {
        private const int hashLength = 2;
        private string instanceName;
        private const int maxCounterLength = 0x40;
        private string[] perfCounterNames = new string[] { "Instances" };

        internal DefaultPerformanceCounters(ServiceHostBase serviceHost)
        {
            this.instanceName = CreateFriendlyInstanceName(serviceHost);
            this.Counters = new PerformanceCounter[1];
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    PerformanceCounter defaultPerformanceCounter = PerformanceCounters.GetDefaultPerformanceCounter(this.perfCounterNames[i], this.instanceName);
                    if (defaultPerformanceCounter != null)
                    {
                        this.Counters[i] = defaultPerformanceCounter;
                        continue;
                    }
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
                }
                break;
            }
        }

        internal static string CreateFriendlyInstanceName(ServiceHostBase serviceHost)
        {
            return "_WCF_Admin";
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

        internal override string[] CounterNames
        {
            get
            {
                return this.perfCounterNames;
            }
        }

        internal PerformanceCounter[] Counters { get; set; }

        internal override bool Initialized
        {
            get
            {
                return (this.Counters != null);
            }
        }

        internal override string InstanceName
        {
            get
            {
                return this.instanceName;
            }
        }

        internal override int PerfCounterEnd
        {
            get
            {
                return 1;
            }
        }

        internal override int PerfCounterStart
        {
            get
            {
                return 0;
            }
        }

        private enum PerfCounters
        {
            Instances,
            TotalCounters
        }

        [Flags]
        private enum truncOptions : uint
        {
            NoBits = 0,
            service32 = 1,
            uri31 = 4
        }
    }
}

