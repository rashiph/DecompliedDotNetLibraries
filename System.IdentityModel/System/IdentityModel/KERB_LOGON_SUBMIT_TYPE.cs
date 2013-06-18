namespace System.IdentityModel
{
    using System;

    internal enum KERB_LOGON_SUBMIT_TYPE
    {
        KerbCertificateLogon = 13,
        KerbCertificateS4ULogon = 14,
        KerbCertificateUnlockLogon = 15,
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

