namespace System.ServiceModel
{
    using System;

    internal static class TcpClientCredentialTypeHelper
    {
        internal static bool IsDefined(TcpClientCredentialType value)
        {
            if ((value != TcpClientCredentialType.None) && (value != TcpClientCredentialType.Windows))
            {
                return (value == TcpClientCredentialType.Certificate);
            }
            return true;
        }
    }
}

