namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    internal enum DcEnumFlag
    {
        NotifyAfterSiteRecords = 2,
        OnlyDoSiteName = 1
    }
}

