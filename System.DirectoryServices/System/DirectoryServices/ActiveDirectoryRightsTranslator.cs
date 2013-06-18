namespace System.DirectoryServices
{
    using System;

    internal sealed class ActiveDirectoryRightsTranslator
    {
        internal static int AccessMaskFromRights(ActiveDirectoryRights adRights)
        {
            return (int) adRights;
        }

        internal static ActiveDirectoryRights RightsFromAccessMask(int accessMask)
        {
            return (ActiveDirectoryRights) accessMask;
        }
    }
}

