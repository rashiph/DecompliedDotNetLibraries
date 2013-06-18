namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal static class SystemDiagnosticsPerformanceCountersExtension
    {
        internal static void Decrement(this PerformanceCountersBase thisPtr, PerformanceCounter[] counters, int counterIndex)
        {
            PerformanceCounter counter = null;
            try
            {
                if (counters != null)
                {
                    counter = counters[counterIndex];
                    if (counter != null)
                    {
                        counter.Decrement();
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                PerformanceCounters.TracePerformanceCounterUpdateFailure(thisPtr.InstanceName, thisPtr.CounterNames[counterIndex]);
                if (counters != null)
                {
                    counters[counterIndex] = null;
                    PerformanceCounters.ReleasePerformanceCounter(ref counter);
                }
            }
        }

        internal static void Increment(this PerformanceCountersBase thisPtr, PerformanceCounter[] counters, int counterIndex)
        {
            PerformanceCounter counter = null;
            try
            {
                if (counters != null)
                {
                    counter = counters[counterIndex];
                    if (counter != null)
                    {
                        counter.Increment();
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                PerformanceCounters.TracePerformanceCounterUpdateFailure(thisPtr.InstanceName, thisPtr.CounterNames[counterIndex]);
                if (counters != null)
                {
                    counters[counterIndex] = null;
                    PerformanceCounters.ReleasePerformanceCounter(ref counter);
                }
            }
        }

        internal static void IncrementBy(this PerformanceCountersBase thisPtr, PerformanceCounter[] counters, int counterIndex, long time)
        {
            PerformanceCounter counter = null;
            try
            {
                if (counters != null)
                {
                    counter = counters[counterIndex];
                    if (counter != null)
                    {
                        counter.IncrementBy(time);
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                PerformanceCounters.TracePerformanceCounterUpdateFailure(thisPtr.InstanceName, thisPtr.CounterNames[counterIndex]);
                if (counters != null)
                {
                    counters[counterIndex] = null;
                    PerformanceCounters.ReleasePerformanceCounter(ref counter);
                }
            }
        }

        internal static void Set(this PerformanceCountersBase thisPtr, PerformanceCounter[] counters, int counterIndex, long value)
        {
            PerformanceCounter counter = null;
            try
            {
                if (counters != null)
                {
                    counter = counters[counterIndex];
                    if (counter != null)
                    {
                        counter.RawValue = value;
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                PerformanceCounters.TracePerformanceCounterUpdateFailure(thisPtr.InstanceName, thisPtr.CounterNames[counterIndex]);
                counters[counterIndex] = null;
                PerformanceCounters.ReleasePerformanceCounter(ref counter);
            }
        }
    }
}

