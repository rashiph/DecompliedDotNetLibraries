namespace System.EnterpriseServices
{
    using System;

    [Serializable]
    public enum AuthenticationOption
    {
        Default,
        None,
        Connect,
        Call,
        Packet,
        Integrity,
        Privacy
    }
}

