namespace System.Web.Hosting
{
    using System;
    using System.Security.Permissions;

    public abstract class ProcessProtocolHandler : MarshalByRefObject
    {
        protected ProcessProtocolHandler()
        {
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.Infrastructure)]
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public abstract void StartListenerChannel(IListenerChannelCallback listenerChannelCallback, IAdphManager AdphManager);
        public abstract void StopListenerChannel(int listenerChannelId, bool immediate);
        public abstract void StopProtocol(bool immediate);
    }
}

