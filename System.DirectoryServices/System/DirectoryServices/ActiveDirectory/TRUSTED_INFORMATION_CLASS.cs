namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    internal enum TRUSTED_INFORMATION_CLASS
    {
        TrustedControllersInformation = 2,
        TrustedDomainAuthInformation = 7,
        TrustedDomainAuthInformationInternal = 9,
        TrustedDomainFullInformation = 8,
        TrustedDomainFullInformation2Internal = 12,
        TrustedDomainFullInformationInternal = 10,
        TrustedDomainInformationBasic = 5,
        TrustedDomainInformationEx = 6,
        TrustedDomainInformationEx2Internal = 11,
        TrustedDomainNameInformation = 1,
        TrustedPasswordInformation = 4,
        TrustedPosixOffsetInformation = 3
    }
}

