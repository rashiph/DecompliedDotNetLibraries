namespace System.EnterpriseServices.Admin
{
    using System;

    [Serializable]
    internal enum ApplicationExportOptions
    {
        ApplicationProxy = 2,
        ForceOverwriteOfFiles = 4,
        NoUsers = 0,
        Users = 1
    }
}

