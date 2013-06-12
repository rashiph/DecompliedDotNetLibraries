namespace System.Web.Hosting
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security.Permissions;

    internal class ContextBase
    {
        internal static object SwitchContext(object newContext)
        {
            object hostContext = CallContext.HostContext;
            if (hostContext != newContext)
            {
                CallContext.HostContext = newContext;
            }
            return hostContext;
        }

        internal static object Current
        {
            get
            {
                return CallContext.HostContext;
            }
            [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
            set
            {
                CallContext.HostContext = value;
            }
        }
    }
}

