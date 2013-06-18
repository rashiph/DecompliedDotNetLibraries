namespace System.ServiceModel.ComIntegration
{
    using System;

    internal enum COMAdminIsolationLevel
    {
        Any,
        ReadUncommitted,
        ReadCommitted,
        RepeatableRead,
        Serializable
    }
}

