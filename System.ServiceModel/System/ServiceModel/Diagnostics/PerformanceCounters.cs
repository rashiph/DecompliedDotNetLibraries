namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal static class PerformanceCounters
    {
        private static bool endpointOOM = false;
        internal const int MaxInstanceNameLength = 0x7f;
        private static bool operationOOM = false;
        private static object perfCounterDictionarySyncObject = new object();
        private static Dictionary<string, ServiceModelPerformanceCounters> performanceCounters = null;
        private static Dictionary<string, ServiceModelPerformanceCountersEntry> performanceCountersBaseUri = null;
        private static List<ServiceModelPerformanceCounters> performanceCountersList = null;
        private static PerformanceCounterScope scope;
        private static bool serviceOOM = false;

        static PerformanceCounters()
        {
            PerformanceCounterScope performanceCountersFromConfig = GetPerformanceCountersFromConfig();
            if (performanceCountersFromConfig != PerformanceCounterScope.Off)
            {
                try
                {
                    if (performanceCountersFromConfig == PerformanceCounterScope.Default)
                    {
                        performanceCountersFromConfig = OSEnvironmentHelper.IsVistaOrGreater ? PerformanceCounterScope.ServiceOnly : PerformanceCounterScope.Off;
                    }
                    scope = performanceCountersFromConfig;
                }
                catch (SecurityException exception)
                {
                    scope = PerformanceCounterScope.Off;
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                        TraceUtility.TraceEvent(TraceEventType.Warning, 0x80038, System.ServiceModel.SR.GetString("PartialTrustPerformanceCountersNotEnabled"));
                    }
                }
            }
            else
            {
                scope = PerformanceCounterScope.Off;
            }
        }

        internal static void AddPerformanceCountersForEndpoint(ServiceHostBase serviceHost, ContractDescription contractDescription, EndpointDispatcher endpointDispatcher)
        {
            if ((PerformanceCountersEnabled || MinimalPerformanceCountersEnabled) && endpointDispatcher.SetPerfCounterId())
            {
                ServiceModelPerformanceCounters counters;
                lock (perfCounterDictionarySyncObject)
                {
                    if (!PerformanceCountersForEndpoint.TryGetValue(endpointDispatcher.PerfCounterId, out counters))
                    {
                        counters = new ServiceModelPerformanceCounters(serviceHost, contractDescription, endpointDispatcher);
                        if (!counters.Initialized)
                        {
                            return;
                        }
                        PerformanceCountersForEndpoint.Add(endpointDispatcher.PerfCounterId, counters);
                        int num = PerformanceCountersForEndpointList.FindIndex(c => c == null);
                        if (num >= 0)
                        {
                            PerformanceCountersForEndpointList[num] = counters;
                        }
                        else
                        {
                            PerformanceCountersForEndpointList.Add(counters);
                            num = PerformanceCountersForEndpointList.Count - 1;
                        }
                        endpointDispatcher.PerfCounterInstanceId = num;
                    }
                }
                lock (perfCounterDictionarySyncObject)
                {
                    ServiceModelPerformanceCountersEntry entry;
                    if (!PerformanceCountersForBaseUri.TryGetValue(endpointDispatcher.PerfCounterBaseId, out entry))
                    {
                        if (PerformanceCountersEnabled)
                        {
                            entry = new ServiceModelPerformanceCountersEntry(serviceHost.Counters);
                        }
                        else if (MinimalPerformanceCountersEnabled)
                        {
                            entry = new ServiceModelPerformanceCountersEntry(serviceHost.DefaultCounters);
                        }
                        PerformanceCountersForBaseUri.Add(endpointDispatcher.PerfCounterBaseId, entry);
                    }
                    entry.Add(counters);
                }
            }
        }

        internal static void AuthenticationFailed(Message message, Uri listenUri)
        {
            CallOnAllCounters("AuthenticationFailed", message, listenUri, true);
        }

        internal static void AuthorizationFailed(string operationName)
        {
            EndpointDispatcher endpointDispatcher = GetEndpointDispatcher();
            if (endpointDispatcher != null)
            {
                string perfCounterId = endpointDispatcher.PerfCounterId;
                if (Scope == PerformanceCounterScope.All)
                {
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(endpointDispatcher.PerfCounterInstanceId, operationName);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.AuthorizationFailed();
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                    if (endpointPerformanceCounters != null)
                    {
                        endpointPerformanceCounters.AuthorizationFailed();
                    }
                }
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.AuthorizationFailed();
                }
            }
        }

        private static void CallOnAllCounters(string methodName, Message message, Uri listenUri, bool includeOperations)
        {
            if (((message != null) && (message.Headers != null)) && ((null != message.Headers.To) && (null != listenUri)))
            {
                ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(listenUri.AbsoluteUri.ToUpperInvariant());
                if (serviceModelPerformanceCountersBaseUri != null)
                {
                    InvokeMethod(serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters, methodName);
                    if (Scope == PerformanceCounterScope.All)
                    {
                        foreach (ServiceModelPerformanceCounters counters in serviceModelPerformanceCountersBaseUri.CounterList)
                        {
                            if (counters.EndpointPerformanceCounters != null)
                            {
                                InvokeMethod(counters.EndpointPerformanceCounters, methodName);
                            }
                            if (includeOperations)
                            {
                                OperationPerformanceCountersBase operationPerformanceCountersFromMessage = counters.GetOperationPerformanceCountersFromMessage(message);
                                if (operationPerformanceCountersFromMessage != null)
                                {
                                    InvokeMethod(operationPerformanceCountersFromMessage, methodName);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static PerformanceCounter GetDefaultPerformanceCounter(string perfCounterName, string instanceName)
        {
            return GetPerformanceCounter("ServiceModelService 4.0.0.0", perfCounterName, instanceName, PerformanceCounterInstanceLifetime.Global);
        }

        internal static EndpointDispatcher GetEndpointDispatcher()
        {
            EndpointDispatcher endpointDispatcher = null;
            OperationContext current = OperationContext.Current;
            if ((current != null) && (current.InternalServiceChannel != null))
            {
                endpointDispatcher = current.EndpointDispatcher;
            }
            return endpointDispatcher;
        }

        internal static PerformanceCounter GetEndpointPerformanceCounter(string perfCounterName, string instanceName)
        {
            return GetPerformanceCounter("ServiceModelEndpoint 4.0.0.0", perfCounterName, instanceName, PerformanceCounterInstanceLifetime.Process);
        }

        private static EndpointPerformanceCountersBase GetEndpointPerformanceCounters(int perfCounterInstanceId)
        {
            ServiceModelPerformanceCounters serviceModelPerformanceCounters = GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (serviceModelPerformanceCounters != null)
            {
                return serviceModelPerformanceCounters.EndpointPerformanceCounters;
            }
            return null;
        }

        internal static PerformanceCounter GetOperationPerformanceCounter(string perfCounterName, string instanceName)
        {
            return GetPerformanceCounter("ServiceModelOperation 4.0.0.0", perfCounterName, instanceName, PerformanceCounterInstanceLifetime.Process);
        }

        private static OperationPerformanceCountersBase GetOperationPerformanceCounters(int perfCounterInstanceId, string operation)
        {
            ServiceModelPerformanceCounters serviceModelPerformanceCounters = GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (serviceModelPerformanceCounters != null)
            {
                return serviceModelPerformanceCounters.GetOperationPerformanceCounters(operation);
            }
            return null;
        }

        internal static PerformanceCounter GetPerformanceCounter(string categoryName, string perfCounterName, string instanceName, PerformanceCounterInstanceLifetime instanceLifetime)
        {
            PerformanceCounter counter = null;
            if (!PerformanceCountersEnabled && !MinimalPerformanceCountersEnabled)
            {
                return counter;
            }
            return GetPerformanceCounterInternal(categoryName, perfCounterName, instanceName, instanceLifetime);
        }

        internal static PerformanceCounter GetPerformanceCounterInternal(string categoryName, string perfCounterName, string instanceName, PerformanceCounterInstanceLifetime instanceLifetime)
        {
            PerformanceCounter counter = null;
            try
            {
                counter = new PerformanceCounter {
                    CategoryName = categoryName,
                    CounterName = perfCounterName,
                    InstanceName = instanceName,
                    ReadOnly = false,
                    InstanceLifetime = instanceLifetime
                };
                try
                {
                    long rawValue = counter.RawValue;
                }
                catch (InvalidOperationException)
                {
                    counter = null;
                    throw;
                }
                catch (SecurityException exception)
                {
                    if (MinimalPerformanceCountersEnabled)
                    {
                        scope = PerformanceCounterScope.Off;
                    }
                    if (DiagnosticUtility.ShouldTraceWarning)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustPerformanceCountersNotEnabled"), exception), TraceEventType.Warning);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityException(System.ServiceModel.SR.GetString("PartialTrustPerformanceCountersNotEnabled")));
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                if (counter != null)
                {
                    if (!counter.ReadOnly)
                    {
                        try
                        {
                            counter.RemoveInstance();
                        }
                        catch (Exception exception3)
                        {
                            if (Fx.IsFatal(exception3))
                            {
                                throw;
                            }
                        }
                    }
                    counter = null;
                }
                bool flag = true;
                if (categoryName == "ServiceModelService 4.0.0.0")
                {
                    if (!serviceOOM)
                    {
                        serviceOOM = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else if (categoryName == "ServiceModelOperation 4.0.0.0")
                {
                    if (!operationOOM)
                    {
                        operationOOM = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else if (categoryName == "ServiceModelEndpoint 4.0.0.0")
                {
                    if (!endpointOOM)
                    {
                        endpointOOM = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.PerformanceCounter, (EventLogEventId) (-1073610742), new string[] { categoryName, perfCounterName, exception2.ToString() });
                }
            }
            return counter;
        }

        [SecuritySafeCritical]
        private static PerformanceCounterScope GetPerformanceCountersFromConfig()
        {
            return DiagnosticSection.UnsafeGetSection().PerformanceCounters;
        }

        private static ServiceModelPerformanceCounters GetServiceModelPerformanceCounters(int perfCounterInstanceId)
        {
            if (PerformanceCountersForEndpointList.Count == 0)
            {
                return null;
            }
            return PerformanceCountersForEndpointList[perfCounterInstanceId];
        }

        private static ServiceModelPerformanceCountersEntry GetServiceModelPerformanceCountersBaseUri(string uri)
        {
            ServiceModelPerformanceCountersEntry entry = null;
            if (!string.IsNullOrEmpty(uri))
            {
                PerformanceCountersForBaseUri.TryGetValue(uri, out entry);
            }
            return entry;
        }

        internal static PerformanceCounter GetServicePerformanceCounter(string perfCounterName, string instanceName)
        {
            return GetPerformanceCounter("ServiceModelService 4.0.0.0", perfCounterName, instanceName, PerformanceCounterInstanceLifetime.Process);
        }

        private static ServicePerformanceCountersBase GetServicePerformanceCounters(int perfCounterInstanceId)
        {
            ServiceModelPerformanceCounters serviceModelPerformanceCounters = GetServiceModelPerformanceCounters(perfCounterInstanceId);
            if (serviceModelPerformanceCounters != null)
            {
                return serviceModelPerformanceCounters.ServicePerformanceCounters;
            }
            return null;
        }

        private static void InvokeMethod(object o, string methodName)
        {
            o.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(o, null);
        }

        internal static void MessageDropped(string uri)
        {
            ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(uri);
            if (serviceModelPerformanceCountersBaseUri != null)
            {
                serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters.MessageDropped();
                if (Scope == PerformanceCounterScope.All)
                {
                    foreach (ServiceModelPerformanceCounters counters in serviceModelPerformanceCountersBaseUri.CounterList)
                    {
                        if (counters.EndpointPerformanceCounters != null)
                        {
                            counters.EndpointPerformanceCounters.MessageDropped();
                        }
                    }
                }
            }
        }

        internal static void MethodCalled(string operationName)
        {
            EndpointDispatcher endpointDispatcher = GetEndpointDispatcher();
            if (endpointDispatcher != null)
            {
                if (Scope == PerformanceCounterScope.All)
                {
                    string perfCounterId = endpointDispatcher.PerfCounterId;
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(endpointDispatcher.PerfCounterInstanceId, operationName);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.MethodCalled();
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                    if (endpointPerformanceCounters != null)
                    {
                        endpointPerformanceCounters.MethodCalled();
                    }
                }
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.MethodCalled();
                }
            }
        }

        internal static void MethodReturnedError(string operationName)
        {
            MethodReturnedError(operationName, -1L);
        }

        internal static void MethodReturnedError(string operationName, long time)
        {
            EndpointDispatcher endpointDispatcher = GetEndpointDispatcher();
            if (endpointDispatcher != null)
            {
                if (Scope == PerformanceCounterScope.All)
                {
                    string perfCounterId = endpointDispatcher.PerfCounterId;
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(endpointDispatcher.PerfCounterInstanceId, operationName);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.MethodReturnedError();
                        if (time > 0L)
                        {
                            operationPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                    if (endpointPerformanceCounters != null)
                    {
                        endpointPerformanceCounters.MethodReturnedError();
                        if (time > 0L)
                        {
                            endpointPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.MethodReturnedError();
                    if (time > 0L)
                    {
                        servicePerformanceCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        internal static void MethodReturnedFault(string operationName)
        {
            MethodReturnedFault(operationName, -1L);
        }

        internal static void MethodReturnedFault(string operationName, long time)
        {
            EndpointDispatcher endpointDispatcher = GetEndpointDispatcher();
            if (endpointDispatcher != null)
            {
                if (Scope == PerformanceCounterScope.All)
                {
                    string perfCounterId = endpointDispatcher.PerfCounterId;
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(endpointDispatcher.PerfCounterInstanceId, operationName);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.MethodReturnedFault();
                        if (time > 0L)
                        {
                            operationPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                    if (endpointPerformanceCounters != null)
                    {
                        endpointPerformanceCounters.MethodReturnedFault();
                        if (time > 0L)
                        {
                            endpointPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.MethodReturnedFault();
                    if (time > 0L)
                    {
                        servicePerformanceCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        internal static void MethodReturnedSuccess(string operationName)
        {
            MethodReturnedSuccess(operationName, -1L);
        }

        internal static void MethodReturnedSuccess(string operationName, long time)
        {
            EndpointDispatcher endpointDispatcher = GetEndpointDispatcher();
            if (endpointDispatcher != null)
            {
                if (Scope == PerformanceCounterScope.All)
                {
                    string perfCounterId = endpointDispatcher.PerfCounterId;
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(endpointDispatcher.PerfCounterInstanceId, operationName);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.MethodReturnedSuccess();
                        if (time > 0L)
                        {
                            operationPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                    if (endpointPerformanceCounters != null)
                    {
                        endpointPerformanceCounters.MethodReturnedSuccess();
                        if (time > 0L)
                        {
                            endpointPerformanceCounters.SaveCallDuration(time);
                        }
                    }
                }
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(endpointDispatcher.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.MethodReturnedSuccess();
                    if (time > 0L)
                    {
                        servicePerformanceCounters.SaveCallDuration(time);
                    }
                }
            }
        }

        internal static void MsmqDroppedMessage(string uri)
        {
            if (Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(uri);
                if (serviceModelPerformanceCountersBaseUri != null)
                {
                    serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters.MsmqDroppedMessage();
                }
            }
        }

        internal static void MsmqPoisonMessage(string uri)
        {
            if (Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(uri);
                if (serviceModelPerformanceCountersBaseUri != null)
                {
                    serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters.MsmqPoisonMessage();
                }
            }
        }

        internal static void MsmqRejectedMessage(string uri)
        {
            if (Scope == PerformanceCounterScope.All)
            {
                ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(uri);
                if (serviceModelPerformanceCountersBaseUri != null)
                {
                    serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters.MsmqRejectedMessage();
                }
            }
        }

        internal static void ReleasePerformanceCounter(ref PerformanceCounter counter)
        {
            if (counter != null)
            {
                try
                {
                    counter.RemoveInstance();
                    counter = null;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
            }
        }

        internal static void ReleasePerformanceCountersForEndpoint(string id, string baseId)
        {
            if (PerformanceCountersEnabled)
            {
                lock (perfCounterDictionarySyncObject)
                {
                    ServiceModelPerformanceCounters counters;
                    if (!string.IsNullOrEmpty(id) && PerformanceCountersForEndpoint.TryGetValue(id, out counters))
                    {
                        PerformanceCountersForEndpoint.Remove(id);
                        int index = PerformanceCountersForEndpointList.IndexOf(counters);
                        PerformanceCountersForEndpointList[index] = null;
                    }
                    if (!string.IsNullOrEmpty(baseId))
                    {
                        PerformanceCountersForBaseUri.Remove(baseId);
                    }
                }
            }
        }

        internal static void SessionFaulted(string uri)
        {
            ServiceModelPerformanceCountersEntry serviceModelPerformanceCountersBaseUri = GetServiceModelPerformanceCountersBaseUri(uri);
            if (serviceModelPerformanceCountersBaseUri != null)
            {
                serviceModelPerformanceCountersBaseUri.ServicePerformanceCounters.SessionFaulted();
                if (Scope == PerformanceCounterScope.All)
                {
                    foreach (ServiceModelPerformanceCounters counters in serviceModelPerformanceCountersBaseUri.CounterList)
                    {
                        if (counters.EndpointPerformanceCounters != null)
                        {
                            counters.EndpointPerformanceCounters.SessionFaulted();
                        }
                    }
                }
            }
        }

        internal static void TracePerformanceCounterUpdateFailure(string instanceName, string perfCounterName)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                TraceUtility.TraceEvent(TraceEventType.Error, 0x8003a, System.ServiceModel.SR.GetString("TraceCodePerformanceCountersFailedDuringUpdate", new object[] { perfCounterName + "::" + instanceName }));
            }
        }

        internal static void TxAborted(EndpointDispatcher el, long count)
        {
            if (PerformanceCountersEnabled && (el != null))
            {
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.TxAborted(count);
                }
            }
        }

        internal static void TxCommitted(EndpointDispatcher el, long count)
        {
            if (PerformanceCountersEnabled && (el != null))
            {
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.TxCommitted(count);
                }
            }
        }

        internal static void TxFlowed(EndpointDispatcher el, string operation)
        {
            if (el != null)
            {
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.TxFlowed();
                }
                if (Scope == PerformanceCounterScope.All)
                {
                    OperationPerformanceCountersBase operationPerformanceCounters = GetOperationPerformanceCounters(el.PerfCounterInstanceId, operation);
                    if (operationPerformanceCounters != null)
                    {
                        operationPerformanceCounters.TxFlowed();
                    }
                    EndpointPerformanceCountersBase endpointPerformanceCounters = GetEndpointPerformanceCounters(el.PerfCounterInstanceId);
                    if (servicePerformanceCounters != null)
                    {
                        endpointPerformanceCounters.TxFlowed();
                    }
                }
            }
        }

        internal static void TxInDoubt(EndpointDispatcher el, long count)
        {
            if (PerformanceCountersEnabled && (el != null))
            {
                ServicePerformanceCountersBase servicePerformanceCounters = GetServicePerformanceCounters(el.PerfCounterInstanceId);
                if (servicePerformanceCounters != null)
                {
                    servicePerformanceCounters.TxInDoubt(count);
                }
            }
        }

        internal static bool MinimalPerformanceCountersEnabled
        {
            get
            {
                return (scope == PerformanceCounterScope.Default);
            }
        }

        internal static bool PerformanceCountersEnabled
        {
            get
            {
                return ((scope != PerformanceCounterScope.Off) && (scope != PerformanceCounterScope.Default));
            }
        }

        internal static Dictionary<string, ServiceModelPerformanceCountersEntry> PerformanceCountersForBaseUri
        {
            get
            {
                if (performanceCountersBaseUri == null)
                {
                    lock (perfCounterDictionarySyncObject)
                    {
                        if (performanceCountersBaseUri == null)
                        {
                            performanceCountersBaseUri = new Dictionary<string, ServiceModelPerformanceCountersEntry>();
                        }
                    }
                }
                return performanceCountersBaseUri;
            }
        }

        internal static Dictionary<string, ServiceModelPerformanceCounters> PerformanceCountersForEndpoint
        {
            get
            {
                if (performanceCounters == null)
                {
                    lock (perfCounterDictionarySyncObject)
                    {
                        if (performanceCounters == null)
                        {
                            performanceCounters = new Dictionary<string, ServiceModelPerformanceCounters>();
                        }
                    }
                }
                return performanceCounters;
            }
        }

        internal static List<ServiceModelPerformanceCounters> PerformanceCountersForEndpointList
        {
            get
            {
                if (performanceCountersList == null)
                {
                    lock (perfCounterDictionarySyncObject)
                    {
                        if (performanceCountersList == null)
                        {
                            performanceCountersList = new List<ServiceModelPerformanceCounters>();
                        }
                    }
                }
                return performanceCountersList;
            }
        }

        internal static PerformanceCounterScope Scope
        {
            get
            {
                return scope;
            }
            set
            {
                scope = value;
            }
        }
    }
}

