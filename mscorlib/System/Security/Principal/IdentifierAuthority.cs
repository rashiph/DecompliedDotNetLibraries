namespace System.Security.Principal
{
    using System;

    internal enum IdentifierAuthority : long
    {
        CreatorAuthority = 3L,
        ExchangeAuthority = 8L,
        InternetSiteAuthority = 7L,
        LocalAuthority = 2L,
        NonUniqueAuthority = 4L,
        NTAuthority = 5L,
        NullAuthority = 0L,
        ResourceManagerAuthority = 9L,
        SiteServerAuthority = 6L,
        WorldAuthority = 1L
    }
}

