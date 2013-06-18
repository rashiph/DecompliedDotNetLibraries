namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    internal abstract class ComPlusServiceHost : ServiceHostBase
    {
        private ServiceInfo info;

        protected ComPlusServiceHost()
        {
        }

        protected override void ApplyConfiguration()
        {
        }

        protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            System.ServiceModel.Description.ServiceDescription description2;
            try
            {
                System.ServiceModel.Description.ServiceDescription description = new ComPlusServiceLoader(this.info).Load(this);
                implementedContracts = null;
                description2 = description;
            }
            catch (Exception exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610730), new string[] { this.info.AppID.ToString(), this.info.Clsid.ToString(), exception.ToString() });
                throw;
            }
            return description2;
        }

        protected void Initialize(Guid clsid, ServiceElement service, ComCatalogObject applicationObject, ComCatalogObject classObject, HostingMode hostingMode)
        {
            this.VerifyFunctionality();
            this.info = new ServiceInfo(clsid, service, applicationObject, classObject, hostingMode);
            base.InitializeDescription(new UriSchemeKeyedCollection(new Uri[0]));
        }

        protected override void InitializeRuntime()
        {
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, 0x50001, "TraceCodeComIntegrationServiceHostStartingService", this.info);
            try
            {
                new DispatcherBuilder().InitializeServiceHost(base.Description, this);
            }
            catch (Exception exception)
            {
                if (DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610730), new string[] { this.info.AppID.ToString(), this.info.Clsid.ToString(), exception.ToString() });
                }
                throw;
            }
            ComPlusServiceHostTrace.Trace(TraceEventType.Verbose, 0x50004, "TraceCodeComIntegrationServiceHostStartedServiceDetails", this.info, base.Description);
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, 0x50002, "TraceCodeComIntegrationServiceHostStartedService", this.info);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, 0x50006, "TraceCodeComIntegrationServiceHostStoppingService", this.info);
            base.OnClose(timeout);
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, 0x50007, "TraceCodeComIntegrationServiceHostStoppedService", this.info);
        }

        protected void VerifyFunctionality()
        {
            object obj2 = new CServiceConfig();
            if (!(obj2 is IServiceSysTxnConfig))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.QFENotPresent());
            }
        }
    }
}

