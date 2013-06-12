namespace System.Data.SqlClient
{
    using System;

    internal enum PreLoginHandshakeStatus
    {
        Successful,
        SphinxFailure,
        InstanceFailure
    }
}

