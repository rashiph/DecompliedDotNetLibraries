namespace System.Security.Principal
{
    using System;

    [Serializable]
    internal enum KerbLogonSubmitType
    {
        KerbInteractiveLogon = 2,
        KerbProxyLogon = 9,
        KerbS4ULogon = 12,
        KerbSmartCardLogon = 6,
        KerbSmartCardUnlockLogon = 8,
        KerbTicketLogon = 10,
        KerbTicketUnlockLogon = 11,
        KerbWorkstationUnlockLogon = 7
    }
}

