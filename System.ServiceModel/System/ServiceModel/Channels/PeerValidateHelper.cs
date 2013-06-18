namespace System.ServiceModel.Channels
{
    using System;
    using System.Net;
    using System.ServiceModel;

    internal static class PeerValidateHelper
    {
        public static void ValidateListenIPAddress(IPAddress address)
        {
            if ((address != null) && (((address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any)) || (address.Equals(IPAddress.IPv6None) || address.Equals(IPAddress.None))) || ((address.Equals(IPAddress.Broadcast) || address.IsIPv6Multicast) || IPAddress.IsLoopback(address))))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("PeerListenIPAddressInvalid", new object[] { address }), "address", null));
            }
        }

        public static void ValidateMaxMessageSize(long value)
        {
            if (value < 0x4000L)
            {
                string message = System.ServiceModel.SR.GetString("ArgumentOutOfRange", new object[] { 0x4000L, 0x7fffffffffffffffL });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
            }
        }

        public static void ValidatePort(int value)
        {
            if ((value < 0) || (value > 0xffff))
            {
                string message = System.ServiceModel.SR.GetString("ArgumentOutOfRange", new object[] { 0, 0xffff });
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, message));
            }
        }

        public static bool ValidNodeAddress(PeerNodeAddress address)
        {
            return (((((address != null) && (address.EndpointAddress != null)) && ((address.EndpointAddress.Uri != null) && (address.IPAddresses != null))) && (address.IPAddresses.Count > 0)) && (string.Compare(address.EndpointAddress.Uri.Scheme, Uri.UriSchemeNetTcp, StringComparison.OrdinalIgnoreCase) == 0));
        }

        public static bool ValidReferralNodeAddress(PeerNodeAddress address)
        {
            long scopeId = -1L;
            foreach (IPAddress address2 in address.IPAddresses)
            {
                if (address2.IsIPv6LinkLocal)
                {
                    if (scopeId == -1L)
                    {
                        scopeId = address2.ScopeId;
                    }
                    else if (scopeId != address2.ScopeId)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

