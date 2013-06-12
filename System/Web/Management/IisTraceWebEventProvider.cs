namespace System.Web.Management
{
    using System;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Util;

    public sealed class IisTraceWebEventProvider : WebEventProvider
    {
        public IisTraceWebEventProvider()
        {
            HttpContext current = HttpContext.Current;
            if (((current != null) && !HttpRuntime.UseIntegratedPipeline) && !(current.WorkerRequest is ISAPIWorkerRequestInProcForIIS7))
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("Requires_Iis_7"));
            }
        }

        public override void Flush()
        {
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            HttpContext current = HttpContext.Current;
            if (current != null)
            {
                current.WorkerRequest.RaiseTraceEvent(eventRaised);
            }
        }

        public override void Shutdown()
        {
        }
    }
}

