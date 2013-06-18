namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal enum DS_NAME_ERROR
    {
        DS_NAME_NO_ERROR,
        DS_NAME_ERROR_RESOLVING,
        DS_NAME_ERROR_NOT_FOUND,
        DS_NAME_ERROR_NOT_UNIQUE,
        DS_NAME_ERROR_NO_MAPPING,
        DS_NAME_ERROR_DOMAIN_ONLY,
        DS_NAME_ERROR_NO_SYNTACTICAL_MAPPING,
        DS_NAME_ERROR_TRUST_REFERRAL
    }
}

