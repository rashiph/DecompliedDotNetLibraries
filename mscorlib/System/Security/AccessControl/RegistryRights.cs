namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum RegistryRights
    {
        ChangePermissions = 0x40000,
        CreateLink = 0x20,
        CreateSubKey = 4,
        Delete = 0x10000,
        EnumerateSubKeys = 8,
        ExecuteKey = 0x20019,
        FullControl = 0xf003f,
        Notify = 0x10,
        QueryValues = 1,
        ReadKey = 0x20019,
        ReadPermissions = 0x20000,
        SetValue = 2,
        TakeOwnership = 0x80000,
        WriteKey = 0x20006
    }
}

