namespace System.Security.Principal
{
    using System;

    [Serializable]
    internal enum TokenInformationClass
    {
        TokenDefaultDacl = 6,
        TokenGroups = 2,
        TokenGroupsAndPrivileges = 13,
        TokenImpersonationLevel = 9,
        TokenOwner = 4,
        TokenPrimaryGroup = 5,
        TokenPrivileges = 3,
        TokenRestrictedSids = 11,
        TokenSandBoxInert = 15,
        TokenSessionId = 12,
        TokenSessionReference = 14,
        TokenSource = 7,
        TokenStatistics = 10,
        TokenType = 8,
        TokenUser = 1
    }
}

