namespace Microsoft.VisualBasic.Logging
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class AspLog : Microsoft.VisualBasic.Logging.Log
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AspLog()
        {
        }

        [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public AspLog(string name) : base(name)
        {
        }

        [SecuritySafeCritical]
        protected internal override void InitializeWithDefaultsSinceNoConfigExists()
        {
            Type type = Type.GetType("System.Web.WebPageTraceListener, System.Web, Version=4.0.0.0, Culture=neutral, PublicKeyToken=B03F5F7F11D50A3A");
            if (type != null)
            {
                this.TraceSource.Listeners.Add((TraceListener) Activator.CreateInstance(type));
            }
            this.TraceSource.Switch.Level = SourceLevels.Information;
        }
    }
}

