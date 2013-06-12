namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using System.Security;

    [Serializable]
    internal sealed class EnvoyInfo : IEnvoyInfo
    {
        private IMessageSink envoySinks;

        [SecurityCritical]
        private EnvoyInfo(IMessageSink sinks)
        {
            this.EnvoySinks = sinks;
        }

        [SecurityCritical]
        internal static IEnvoyInfo CreateEnvoyInfo(ServerIdentity serverID)
        {
            IEnvoyInfo info = null;
            if (serverID != null)
            {
                if (serverID.EnvoyChain == null)
                {
                    serverID.RaceSetEnvoyChain(serverID.ServerContext.CreateEnvoyChain(serverID.TPOrObject));
                }
                if (!(serverID.EnvoyChain is EnvoyTerminatorSink))
                {
                    info = new EnvoyInfo(serverID.EnvoyChain);
                }
            }
            return info;
        }

        public IMessageSink EnvoySinks
        {
            [SecurityCritical]
            get
            {
                return this.envoySinks;
            }
            [SecurityCritical]
            set
            {
                this.envoySinks = value;
            }
        }
    }
}

