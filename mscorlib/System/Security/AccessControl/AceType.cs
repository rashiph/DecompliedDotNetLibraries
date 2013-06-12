namespace System.Security.AccessControl
{
    using System;

    public enum AceType : byte
    {
        AccessAllowed = 0,
        AccessAllowedCallback = 9,
        AccessAllowedCallbackObject = 11,
        AccessAllowedCompound = 4,
        AccessAllowedObject = 5,
        AccessDenied = 1,
        AccessDeniedCallback = 10,
        AccessDeniedCallbackObject = 12,
        AccessDeniedObject = 6,
        MaxDefinedAceType = 0x10,
        SystemAlarm = 3,
        SystemAlarmCallback = 14,
        SystemAlarmCallbackObject = 0x10,
        SystemAlarmObject = 8,
        SystemAudit = 2,
        SystemAuditCallback = 13,
        SystemAuditCallbackObject = 15,
        SystemAuditObject = 7
    }
}

