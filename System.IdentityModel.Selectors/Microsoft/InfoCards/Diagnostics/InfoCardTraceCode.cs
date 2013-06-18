namespace Microsoft.InfoCards.Diagnostics
{
    using System;

    internal enum InfoCardTraceCode
    {
        AgentInfoCardSelected = 0x4e21,
        AgentPiiDisclosed = 0x4e22,
        Client = 0x9c40,
        ClientInformation = 0x9c41,
        Engine = 0x7530,
        General = 0x1388,
        GeneralInformation = 0x1389,
        None = 0,
        Service = 0xc350,
        Store = 0x2710,
        StoreBeginTransaction = 0x2712,
        StoreClosing = 0x2715,
        StoreCommitTransaction = 0x2713,
        StoreDeleting = 0x2719,
        StoreFailedToOpenStore = 0x2716,
        StoreInvalidKey = 0x2718,
        StoreLoading = 0x2711,
        StoreRollbackTransaction = 0x2714,
        StoreSignatureNotValid = 0x2717,
        UIAgent = 0x4e20
    }
}

