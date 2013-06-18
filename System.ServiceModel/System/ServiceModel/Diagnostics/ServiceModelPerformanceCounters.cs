namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class ServiceModelPerformanceCounters
    {
        private SortedList<string, string> actionToOperation;
        private System.ServiceModel.Diagnostics.DefaultPerformanceCounters defaultPerfCounters;
        private EndpointPerformanceCountersBase endpointPerfCounters;
        private bool initialized;
        private Dictionary<string, OperationPerformanceCountersBase> operationPerfCounters;
        private string perfCounterId;
        private ServicePerformanceCountersBase servicePerfCounters;

        internal ServiceModelPerformanceCounters(ServiceHostBase serviceHost, ContractDescription contractDescription, EndpointDispatcher endpointDispatcher)
        {
            this.perfCounterId = endpointDispatcher.PerfCounterId;
            if (PerformanceCounters.Scope == PerformanceCounterScope.All)
            {
                this.operationPerfCounters = new Dictionary<string, OperationPerformanceCountersBase>(contractDescription.Operations.Count);
                this.actionToOperation = new SortedList<string, string>(contractDescription.Operations.Count);
                foreach (OperationDescription description in contractDescription.Operations)
                {
                    OperationPerformanceCountersBase base2;
                    if ((description.Messages[0].Action != null) && !this.actionToOperation.Keys.Contains(description.Messages[0].Action))
                    {
                        this.actionToOperation.Add(description.Messages[0].Action, description.Name);
                    }
                    if (!this.operationPerfCounters.TryGetValue(description.Name, out base2))
                    {
                        OperationPerformanceCountersBase base3 = PerformanceCountersFactory.CreateOperationCounters(serviceHost.Description.Name, contractDescription.Name, description.Name, endpointDispatcher.PerfCounterBaseId);
                        if ((base3 != null) && base3.Initialized)
                        {
                            this.operationPerfCounters.Add(description.Name, base3);
                        }
                        else
                        {
                            this.initialized = false;
                            return;
                        }
                    }
                }
                EndpointPerformanceCountersBase base4 = PerformanceCountersFactory.CreateEndpointCounters(serviceHost.Description.Name, contractDescription.Name, endpointDispatcher.PerfCounterBaseId);
                if ((base4 != null) && base4.Initialized)
                {
                    this.endpointPerfCounters = base4;
                }
            }
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.servicePerfCounters = serviceHost.Counters;
            }
            if (PerformanceCounters.MinimalPerformanceCountersEnabled)
            {
                this.defaultPerfCounters = serviceHost.DefaultCounters;
            }
            this.initialized = true;
        }

        internal OperationPerformanceCountersBase GetOperationPerformanceCounters(string operation)
        {
            OperationPerformanceCountersBase base2;
            Dictionary<string, OperationPerformanceCountersBase> operationPerfCounters = this.operationPerfCounters;
            if ((operationPerfCounters != null) && operationPerfCounters.TryGetValue(operation, out base2))
            {
                return base2;
            }
            return null;
        }

        internal OperationPerformanceCountersBase GetOperationPerformanceCountersFromMessage(Message message)
        {
            string str;
            if (this.actionToOperation.TryGetValue(message.Headers.Action, out str))
            {
                return this.GetOperationPerformanceCounters(str);
            }
            return null;
        }

        internal System.ServiceModel.Diagnostics.DefaultPerformanceCounters DefaultPerformanceCounters
        {
            get
            {
                return this.defaultPerfCounters;
            }
        }

        internal EndpointPerformanceCountersBase EndpointPerformanceCounters
        {
            get
            {
                return this.endpointPerfCounters;
            }
        }

        internal bool Initialized
        {
            get
            {
                return this.initialized;
            }
        }

        internal string PerfCounterId
        {
            get
            {
                return this.perfCounterId;
            }
        }

        internal ServicePerformanceCountersBase ServicePerformanceCounters
        {
            get
            {
                return this.servicePerfCounters;
            }
        }
    }
}

