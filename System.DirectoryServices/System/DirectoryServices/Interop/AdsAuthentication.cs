namespace System.DirectoryServices.Interop
{
    using System;

    internal enum AdsAuthentication
    {
        ADS_FAST_BIND = 0x20,
        ADS_NO_AUTHENTICATION = 0x10,
        ADS_PROMPT_CREDENTIALS = 8,
        ADS_READONLY_SERVER = 4,
        ADS_SECURE_AUTHENTICATION = 1,
        ADS_USE_ENCRYPTION = 2,
        ADS_USE_SEALING = 0x80,
        ADS_USE_SIGNING = 0x40,
        ADS_USE_SSL = 2
    }
}

