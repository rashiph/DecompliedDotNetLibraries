namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    internal static class PerformanceCountersFactory
    {
        internal static EndpointPerformanceCountersBase CreateEndpointCounters(string service, string contract, string uri)
        {
            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    return new EndpointPerformanceCountersV2(service, contract, uri);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x8003b, System.ServiceModel.SR.GetString("TraceCodePerformanceCountersFailedForService"), null, exception);
                    }
                    return null;
                }
            }
            return new EndpointPerformanceCounters(service, contract, uri);
        }

        internal static OperationPerformanceCountersBase CreateOperationCounters(string service, string contract, string operationName, string uri)
        {
            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    return new OperationPerformanceCountersV2(service, contract, operationName, uri);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x8003b, System.ServiceModel.SR.GetString("TraceCodePerformanceCountersFailedForService"), null, exception);
                    }
                    return null;
                }
            }
            return new OperationPerformanceCounters(service, contract, operationName, uri);
        }

        internal static ServicePerformanceCountersBase CreateServiceCounters(ServiceHostBase serviceHost)
        {
            if (OSEnvironmentHelper.IsVistaOrGreater)
            {
                try
                {
                    ServicePerformanceCountersV2 sv = new ServicePerformanceCountersV2(serviceHost);
                    EndpointPerformanceCountersV2.EnsureCounterSet();
                    OperationPerformanceCountersV2.EnsureCounterSet();
                    return sv;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    PerformanceCounters.Scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        TraceUtility.TraceEvent(TraceEventType.Error, 0x8003b, System.ServiceModel.SR.GetString("TraceCodePerformanceCountersFailedForService"), null, exception);
                    }
                    return null;
                }
            }
            return new ServicePerformanceCounters(serviceHost);
        }
    }
}

