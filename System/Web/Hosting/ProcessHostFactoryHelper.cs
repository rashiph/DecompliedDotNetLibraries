namespace System.Web.Hosting
{
    using System;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public sealed class ProcessHostFactoryHelper : MarshalByRefObject, IProcessHostFactoryHelper
    {
        public object GetProcessHost(IProcessHostSupportFunctions functions)
        {
            object processHost;
            try
            {
                processHost = ProcessHost.GetProcessHost(functions);
            }
            catch (Exception exception)
            {
                Misc.ReportUnhandledException(exception, new string[] { System.Web.SR.GetString("Cant_Create_Process_Host") });
                throw;
            }
            return processHost;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}

