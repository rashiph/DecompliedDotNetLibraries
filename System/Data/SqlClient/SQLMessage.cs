namespace System.Data.SqlClient
{
    using System;
    using System.Data;

    internal sealed class SQLMessage
    {
        private SQLMessage()
        {
        }

        internal static string CultureIdError()
        {
            return Res.GetString("SQL_CultureIdError");
        }

        internal static string EncryptionNotSupportedByClient()
        {
            return Res.GetString("SQL_EncryptionNotSupportedByClient");
        }

        internal static string EncryptionNotSupportedByServer()
        {
            return Res.GetString("SQL_EncryptionNotSupportedByServer");
        }

        internal static string OperationCancelled()
        {
            return Res.GetString("SQL_OperationCancelled");
        }

        internal static string SevereError()
        {
            return Res.GetString("SQL_SevereError");
        }

        internal static string SSPIGenerateError()
        {
            return Res.GetString("SQL_SSPIGenerateError");
        }

        internal static string SSPIInitializeError()
        {
            return Res.GetString("SQL_SSPIInitializeError");
        }

        internal static string Timeout()
        {
            return Res.GetString("SQL_Timeout");
        }

        internal static string UserInstanceFailure()
        {
            return Res.GetString("SQL_UserInstanceFailure");
        }
    }
}

