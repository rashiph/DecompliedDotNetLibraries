namespace System.ServiceModel.Security
{
    using System;

    [Flags]
    internal enum ReceiveSecurityHeaderBindingModes
    {
        Basic = 0x10,
        Endorsing = 2,
        Primary = 1,
        Signed = 4,
        SignedEndorsing = 8,
        Unknown = 0
    }
}

