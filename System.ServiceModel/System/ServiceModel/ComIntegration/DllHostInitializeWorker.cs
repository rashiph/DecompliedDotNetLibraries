namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal class DllHostInitializeWorker
    {
        private Guid applicationId;
        private List<ComPlusServiceHost> hosts = new List<ComPlusServiceHost>();

        public static void PingProc(object o)
        {
            IProcessInitControl control = o as IProcessInitControl;
            try
            {
                for (int i = 0; i < 200; i++)
                {
                    Thread.Sleep(0x2710);
                    control.ResetInitializerTimeout(30);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        public void Shutdown()
        {
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, 0x5000b, "TraceCodeComIntegrationDllHostInitializerStopping", this.applicationId);
            foreach (ComPlusServiceHost host in this.hosts)
            {
                host.Close();
            }
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, 0x5000c, "TraceCodeComIntegrationDllHostInitializerStopped", this.applicationId);
        }

        public void Startup(IProcessInitControl control)
        {
            this.applicationId = ContextUtil.ApplicationId;
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, 0x50008, "TraceCodeComIntegrationDllHostInitializerStarting", this.applicationId);
            Thread thread = null;
            try
            {
                thread = new Thread(new ParameterizedThreadStart(DllHostInitializeWorker.PingProc));
                thread.Start(control);
                ComCatalogObject applicationObject = CatalogUtil.FindApplication(this.applicationId);
                if (applicationObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("ApplicationNotFound", new object[] { this.applicationId.ToString("B").ToUpperInvariant() })));
                }
                if (((int) applicationObject.GetValue("ConcurrentApps")) > 1)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("PooledApplicationNotSupportedForComplusHostedScenarios", new object[] { this.applicationId.ToString("B").ToUpperInvariant() })));
                }
                if ((((((int) applicationObject.GetValue("RecycleLifetimeLimit")) > 0) || (((int) applicationObject.GetValue("RecycleCallLimit")) > 0)) || (((int) applicationObject.GetValue("RecycleActivationLimit")) > 0)) || (((int) applicationObject.GetValue("RecycleMemoryLimit")) > 0))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(System.ServiceModel.SR.GetString("RecycledApplicationNotSupportedForComplusHostedScenarios", new object[] { this.applicationId.ToString("B").ToUpperInvariant() })));
                }
                ComCatalogCollection collection = applicationObject.GetCollection("Components");
                ServicesSection section = ServicesSection.GetSection();
                bool flag3 = false;
                foreach (ServiceElement element in section.Services)
                {
                    Guid empty = Guid.Empty;
                    Guid result = Guid.Empty;
                    string[] strArray = element.Name.Split(new char[] { ',' });
                    if (strArray.Length != 2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OnlyClsidsAllowedForServiceType", new object[] { element.Name })));
                    }
                    if (!DiagnosticUtility.Utility.TryCreateGuid(strArray[0], out result))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OnlyClsidsAllowedForServiceType", new object[] { element.Name })));
                    }
                    if (!DiagnosticUtility.Utility.TryCreateGuid(strArray[1], out empty))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("OnlyClsidsAllowedForServiceType", new object[] { element.Name })));
                    }
                    flag3 = false;
                    ComCatalogCollection.Enumerator enumerator = collection.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        ComCatalogObject current = enumerator.Current;
                        Guid clsid = Fx.CreateGuid((string) current.GetValue("CLSID"));
                        if ((clsid == empty) && (this.applicationId == result))
                        {
                            flag3 = true;
                            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Verbose, 0x50009, "TraceCodeComIntegrationDllHostInitializerAddingHost", this.applicationId, clsid, element);
                            this.hosts.Add(new DllHostedComPlusServiceHost(clsid, element, applicationObject, current));
                        }
                    }
                    if (!flag3)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CannotFindClsidInApplication", new object[] { empty.ToString("B").ToUpperInvariant(), this.applicationId.ToString("B").ToUpperInvariant() })));
                    }
                }
                if (!flag3)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DllHostInitializerFoundNoServices());
                }
                foreach (ComPlusServiceHost host in this.hosts)
                {
                    host.Open();
                }
            }
            catch (Exception exception)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error, EventLogCategory.ComPlus, (EventLogEventId) (-1073610729), new string[] { this.applicationId.ToString(), exception.ToString() });
                throw;
            }
            finally
            {
                if (thread != null)
                {
                    thread.Abort();
                }
            }
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, 0x5000a, "TraceCodeComIntegrationDllHostInitializerStarted", this.applicationId);
        }
    }
}

