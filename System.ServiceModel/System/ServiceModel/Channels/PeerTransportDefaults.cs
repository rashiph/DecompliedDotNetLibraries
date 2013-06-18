namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;

    internal static class PeerTransportDefaults
    {
        internal const IPAddress ListenIPAddress = null;
        internal const bool MessageAuthentication = false;
        internal const PeerAuthenticationMode PeerNodeAuthenticationMode = PeerAuthenticationMode.Password;
        internal const int Port = 0;
        internal const string ResolverTypeString = null;

        internal static PeerResolver CreateResolver()
        {
            return new PnrpPeerResolver();
        }

        internal static bool ResolverAvailable
        {
            get
            {
                return PnrpPeerResolver.IsPnrpAvailable;
            }
        }

        internal static System.Type ResolverBindingElementType
        {
            get
            {
                return typeof(PnrpPeerResolverBindingElement);
            }
        }

        internal static bool ResolverInstalled
        {
            get
            {
                return PnrpPeerResolver.IsPnrpInstalled;
            }
        }

        internal static System.Type ResolverType
        {
            get
            {
                return typeof(PnrpPeerResolver);
            }
        }
    }
}

