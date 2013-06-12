namespace System.Security.Cryptography
{
    using System;

    internal static class Constants
    {
        internal const int ALG_CLASS_DATA_ENCRYPT = 0x6000;
        internal const int ALG_CLASS_HASH = 0x8000;
        internal const int ALG_CLASS_KEY_EXCHANGE = 0xa000;
        internal const int ALG_CLASS_SIGNATURE = 0x2000;
        internal const int ALG_TYPE_ANY = 0;
        internal const int ALG_TYPE_BLOCK = 0x600;
        internal const int ALG_TYPE_DSS = 0x200;
        internal const int ALG_TYPE_RSA = 0x400;
        internal const int ALG_TYPE_STREAM = 0x800;
        internal const int AT_KEYEXCHANGE = 1;
        internal const int AT_SIGNATURE = 2;
        internal const int CALG_3DES = 0x6603;
        internal const int CALG_3DES_112 = 0x6609;
        internal const int CALG_AES_128 = 0x660e;
        internal const int CALG_AES_192 = 0x660f;
        internal const int CALG_AES_256 = 0x6610;
        internal const int CALG_DES = 0x6601;
        internal const int CALG_DSS_SIGN = 0x2200;
        internal const int CALG_MD5 = 0x8003;
        internal const int CALG_RC2 = 0x6602;
        internal const int CALG_RC4 = 0x6801;
        internal const int CALG_RSA_KEYX = 0xa400;
        internal const int CALG_RSA_SIGN = 0x2400;
        internal const int CALG_SHA_256 = 0x800c;
        internal const int CALG_SHA_384 = 0x800d;
        internal const int CALG_SHA_512 = 0x800e;
        internal const int CALG_SHA1 = 0x8004;
        internal const uint CLR_ACCESSIBLE = 6;
        internal const uint CLR_ALGID = 9;
        internal const uint CLR_EXPORTABLE = 3;
        internal const uint CLR_HARDWARE = 5;
        internal const uint CLR_KEYLEN = 1;
        internal const uint CLR_PP_CLIENT_HWND = 10;
        internal const uint CLR_PP_PIN = 11;
        internal const uint CLR_PROTECTED = 7;
        internal const uint CLR_PUBLICKEYONLY = 2;
        internal const uint CLR_REMOVABLE = 4;
        internal const uint CLR_UNIQUE_CONTAINER = 8;
        internal const uint CRYPT_DELETEKEYSET = 0x10;
        internal const uint CRYPT_EXPORTABLE = 1;
        internal const uint CRYPT_MACHINE_KEYSET = 0x20;
        internal const uint CRYPT_NEWKEYSET = 8;
        internal const int CRYPT_OAEP = 0x40;
        internal const uint CRYPT_SILENT = 0x40;
        internal const uint CRYPT_VERIFYCONTEXT = 0xf0000000;
        internal const int KP_EFFECTIVE_KEYLEN = 0x13;
        internal const int KP_IV = 1;
        internal const int KP_MODE = 4;
        internal const int KP_MODE_BITS = 5;
        internal const int NTE_BAD_KEYSET = -2146893802;
        internal const int NTE_FILENOTFOUND = -2147024894;
        internal const int NTE_KEYSET_NOT_DEF = -2146893799;
        internal const int NTE_NO_KEY = -2146893811;
        internal const string OID_OIWSEC_desCBC = "1.3.14.3.2.7";
        internal const string OID_OIWSEC_RIPEMD160 = "1.3.36.3.2.1";
        internal const string OID_OIWSEC_SHA1 = "1.3.14.3.2.26";
        internal const string OID_OIWSEC_SHA256 = "2.16.840.1.101.3.4.2.1";
        internal const string OID_OIWSEC_SHA384 = "2.16.840.1.101.3.4.2.2";
        internal const string OID_OIWSEC_SHA512 = "2.16.840.1.101.3.4.2.3";
        internal const string OID_RSA_DES_EDE3_CBC = "1.2.840.113549.3.7";
        internal const string OID_RSA_MD5 = "1.2.840.113549.2.5";
        internal const string OID_RSA_RC2CBC = "1.2.840.113549.3.2";
        internal const string OID_RSA_SMIMEalgCMS3DESwrap = "1.2.840.113549.1.9.16.3.6";
        internal const int PRIVATEKEYBLOB = 7;
        internal const int PROV_DSS_DH = 13;
        internal const int PROV_RSA_AES = 0x18;
        internal const int PROV_RSA_FULL = 1;
        internal const int PUBLICKEYBLOB = 6;
        internal const int S_OK = 0;
    }
}

