namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum WindowsBuiltInRole
    {
        AccountOperator = 0x224,
        Administrator = 0x220,
        BackupOperator = 0x227,
        Guest = 0x222,
        PowerUser = 0x223,
        PrintOperator = 550,
        Replicator = 0x228,
        SystemOperator = 0x225,
        User = 0x221
    }
}

