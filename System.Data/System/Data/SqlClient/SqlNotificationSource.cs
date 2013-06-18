namespace System.Data.SqlClient
{
    using System;

    public enum SqlNotificationSource
    {
        Client = -2,
        Data = 0,
        Database = 3,
        Environment = 6,
        Execution = 7,
        Object = 2,
        Owner = 8,
        Statement = 5,
        System = 4,
        Timeout = 1,
        Unknown = -1
    }
}

