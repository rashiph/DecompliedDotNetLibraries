namespace System.Data.SqlClient
{
    using System;

    internal enum TdsParserState
    {
        Closed,
        OpenNotLoggedIn,
        OpenLoggedIn,
        Broken
    }
}

