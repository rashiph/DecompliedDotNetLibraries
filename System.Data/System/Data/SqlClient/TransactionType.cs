namespace System.Data.SqlClient
{
    using System;

    internal enum TransactionType
    {
        Context = 5,
        Delegated = 3,
        Distributed = 4,
        LocalFromAPI = 2,
        LocalFromTSQL = 1
    }
}

