namespace System.ServiceModel.ComIntegration
{
    using System;

    [Flags]
    internal enum ComRights
    {
        ACTIVATE_LOCAL = 8,
        ACTIVATE_REMOTE = 0x10,
        EXECUTE = 1,
        EXECUTE_LOCAL = 2,
        EXECUTE_REMOTE = 4
    }
}

