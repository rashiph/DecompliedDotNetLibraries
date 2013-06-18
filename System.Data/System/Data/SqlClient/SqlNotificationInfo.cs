namespace System.Data.SqlClient
{
    using System;

    public enum SqlNotificationInfo
    {
        AlreadyChanged = -2,
        Alter = 5,
        Delete = 3,
        Drop = 4,
        Error = 7,
        Expired = 12,
        Insert = 1,
        Invalid = 9,
        Isolation = 11,
        Merge = 0x10,
        Options = 10,
        PreviousFire = 14,
        Query = 8,
        Resource = 13,
        Restart = 6,
        TemplateLimit = 15,
        Truncate = 0,
        Unknown = -1,
        Update = 2
    }
}

