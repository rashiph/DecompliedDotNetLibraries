namespace System.Data.SqlClient
{
    using System;

    internal enum SniContext
    {
        Undefined,
        Snix_Connect,
        Snix_PreLoginBeforeSuccessfullWrite,
        Snix_PreLogin,
        Snix_LoginSspi,
        Snix_ProcessSspi,
        Snix_Login,
        Snix_EnableMars,
        Snix_AutoEnlist,
        Snix_GetMarsSession,
        Snix_Execute,
        Snix_Read,
        Snix_Close,
        Snix_SendRows
    }
}

