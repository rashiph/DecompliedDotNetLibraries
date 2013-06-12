namespace System.Web.DataAccess
{
    using System;
    using System.DirectoryServices;
    using System.Web.Security;

    internal static class ActiveDirectoryConnectionHelper
    {
        internal static DirectoryEntryHolder GetDirectoryEntry(DirectoryInformation directoryInfo, string objectDN, bool revertImpersonation)
        {
            DirectoryEntryHolder holder = new DirectoryEntryHolder(new DirectoryEntry(directoryInfo.GetADsPath(objectDN), directoryInfo.GetUsername(), directoryInfo.GetPassword(), directoryInfo.AuthenticationTypes));
            holder.Open(null, revertImpersonation);
            return holder;
        }
    }
}

