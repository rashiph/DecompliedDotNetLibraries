namespace System.ServiceModel
{
    using System;

    internal static class PeerTransportCredentialTypeHelper
    {
        internal static bool IsDefined(PeerTransportCredentialType value)
        {
            if (value != PeerTransportCredentialType.Password)
            {
                return (value == PeerTransportCredentialType.Certificate);
            }
            return true;
        }
    }
}

