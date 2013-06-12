namespace System.Web.Configuration
{
    using System;

    internal enum RpcLevel
    {
        Default,
        None,
        Connect,
        Call,
        Pkt,
        PktIntegrity,
        PktPrivacy
    }
}

