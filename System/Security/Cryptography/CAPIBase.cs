namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    internal abstract class CAPIBase
    {
        internal const string ADVAPI32 = "advapi32.dll";
        internal const uint ALG_CLASS_ALL = 0xe000;
        internal const uint ALG_CLASS_ANY = 0;
        internal const uint ALG_CLASS_DATA_ENCRYPT = 0x6000;
        internal const uint ALG_CLASS_HASH = 0x8000;
        internal const uint ALG_CLASS_KEY_EXCHANGE = 0xa000;
        internal const uint ALG_CLASS_MSG_ENCRYPT = 0x4000;
        internal const uint ALG_CLASS_SIGNATURE = 0x2000;
        internal const uint ALG_SID_3DES = 3;
        internal const uint ALG_SID_3DES_112 = 9;
        internal const uint ALG_SID_AES = 0x11;
        internal const uint ALG_SID_AES_128 = 14;
        internal const uint ALG_SID_AES_192 = 15;
        internal const uint ALG_SID_AES_256 = 0x10;
        internal const uint ALG_SID_AGREED_KEY_ANY = 3;
        internal const uint ALG_SID_ANY = 0;
        internal const uint ALG_SID_CAST = 6;
        internal const uint ALG_SID_CYLINK_MEK = 12;
        internal const uint ALG_SID_DES = 1;
        internal const uint ALG_SID_DESX = 4;
        internal const uint ALG_SID_DH_EPHEM = 2;
        internal const uint ALG_SID_DH_SANDF = 1;
        internal const uint ALG_SID_DSS_ANY = 0;
        internal const uint ALG_SID_DSS_DMS = 2;
        internal const uint ALG_SID_DSS_PKCS = 1;
        internal const uint ALG_SID_HASH_REPLACE_OWF = 11;
        internal const uint ALG_SID_HMAC = 9;
        internal const uint ALG_SID_IDEA = 5;
        internal const uint ALG_SID_KEA = 4;
        internal const uint ALG_SID_MAC = 5;
        internal const uint ALG_SID_MD2 = 1;
        internal const uint ALG_SID_MD4 = 2;
        internal const uint ALG_SID_MD5 = 3;
        internal const uint ALG_SID_PCT1_MASTER = 4;
        internal const uint ALG_SID_RC2 = 2;
        internal const uint ALG_SID_RC4 = 1;
        internal const uint ALG_SID_RC5 = 13;
        internal const uint ALG_SID_RIPEMD = 6;
        internal const uint ALG_SID_RIPEMD160 = 7;
        internal const uint ALG_SID_RSA_ANY = 0;
        internal const uint ALG_SID_RSA_ENTRUST = 3;
        internal const uint ALG_SID_RSA_MSATWORK = 2;
        internal const uint ALG_SID_RSA_PGP = 4;
        internal const uint ALG_SID_RSA_PKCS = 1;
        internal const uint ALG_SID_SAFERSK128 = 8;
        internal const uint ALG_SID_SAFERSK64 = 7;
        internal const uint ALG_SID_SCHANNEL_ENC_KEY = 7;
        internal const uint ALG_SID_SCHANNEL_MAC_KEY = 3;
        internal const uint ALG_SID_SCHANNEL_MASTER_HASH = 2;
        internal const uint ALG_SID_SEAL = 2;
        internal const uint ALG_SID_SHA = 4;
        internal const uint ALG_SID_SHA1 = 4;
        internal const uint ALG_SID_SKIPJACK = 10;
        internal const uint ALG_SID_SSL2_MASTER = 5;
        internal const uint ALG_SID_SSL3_MASTER = 1;
        internal const uint ALG_SID_SSL3SHAMD5 = 8;
        internal const uint ALG_SID_TEK = 11;
        internal const uint ALG_SID_TLS1_MASTER = 6;
        internal const uint ALG_SID_TLS1PRF = 10;
        internal const uint ALG_TYPE_ANY = 0;
        internal const uint ALG_TYPE_BLOCK = 0x600;
        internal const uint ALG_TYPE_DH = 0xa00;
        internal const uint ALG_TYPE_DSS = 0x200;
        internal const uint ALG_TYPE_RSA = 0x400;
        internal const uint ALG_TYPE_SECURECHANNEL = 0xc00;
        internal const uint ALG_TYPE_STREAM = 0x800;
        internal const uint ASN_TAG_NULL = 5;
        internal const uint ASN_TAG_OBJID = 6;
        internal const uint CALG_3DES = 0x6603;
        internal const uint CALG_3DES_112 = 0x6609;
        internal const uint CALG_AES = 0x6611;
        internal const uint CALG_AES_128 = 0x660e;
        internal const uint CALG_AES_192 = 0x660f;
        internal const uint CALG_AES_256 = 0x6610;
        internal const uint CALG_AGREEDKEY_ANY = 0xaa03;
        internal const uint CALG_CYLINK_MEK = 0x660c;
        internal const uint CALG_DES = 0x6601;
        internal const uint CALG_DESX = 0x6604;
        internal const uint CALG_DH_EPHEM = 0xaa02;
        internal const uint CALG_DH_SF = 0xaa01;
        internal const uint CALG_DSS_SIGN = 0x2200;
        internal const uint CALG_HASH_REPLACE_OWF = 0x800b;
        internal const uint CALG_HMAC = 0x8009;
        internal const uint CALG_HUGHES_MD5 = 0xa003;
        internal const uint CALG_KEA_KEYX = 0xaa04;
        internal const uint CALG_MAC = 0x8005;
        internal const uint CALG_MD2 = 0x8001;
        internal const uint CALG_MD4 = 0x8002;
        internal const uint CALG_MD5 = 0x8003;
        internal const uint CALG_NO_SIGN = 0x2000;
        internal const uint CALG_PCT1_MASTER = 0x4c04;
        internal const uint CALG_RC2 = 0x6602;
        internal const uint CALG_RC4 = 0x6801;
        internal const uint CALG_RC5 = 0x660d;
        internal const uint CALG_RSA_KEYX = 0xa400;
        internal const uint CALG_RSA_SIGN = 0x2400;
        internal const uint CALG_SCHANNEL_ENC_KEY = 0x4c07;
        internal const uint CALG_SCHANNEL_MAC_KEY = 0x4c03;
        internal const uint CALG_SCHANNEL_MASTER_HASH = 0x4c02;
        internal const uint CALG_SEAL = 0x6802;
        internal const uint CALG_SHA = 0x8004;
        internal const uint CALG_SHA1 = 0x8004;
        internal const uint CALG_SKIPJACK = 0x660a;
        internal const uint CALG_SSL2_MASTER = 0x4c05;
        internal const uint CALG_SSL3_MASTER = 0x4c01;
        internal const uint CALG_SSL3_SHAMD5 = 0x8008;
        internal const uint CALG_TEK = 0x660b;
        internal const uint CALG_TLS1_MASTER = 0x4c06;
        internal const uint CALG_TLS1PRF = 0x800a;
        internal const uint CERT_ACCESS_STATE_PROP_ID = 14;
        internal const uint CERT_ALT_NAME_DIRECTORY_NAME = 5;
        internal const uint CERT_ALT_NAME_DNS_NAME = 3;
        internal const uint CERT_ALT_NAME_EDI_PARTY_NAME = 6;
        internal const uint CERT_ALT_NAME_IP_ADDRESS = 8;
        internal const uint CERT_ALT_NAME_OTHER_NAME = 1;
        internal const uint CERT_ALT_NAME_REGISTERED_ID = 9;
        internal const uint CERT_ALT_NAME_RFC822_NAME = 2;
        internal const uint CERT_ALT_NAME_URL = 7;
        internal const uint CERT_ALT_NAME_X400_ADDRESS = 4;
        internal const uint CERT_ARCHIVED_KEY_HASH_PROP_ID = 0x41;
        internal const uint CERT_ARCHIVED_PROP_ID = 0x13;
        internal const uint CERT_AUTO_ENROLL_PROP_ID = 0x15;
        internal const uint CERT_CA_SUBJECT_FLAG = 0x80;
        internal const uint CERT_CHAIN_POLICY_ALLOW_UNKNOWN_CA_FLAG = 0x10;
        internal const uint CERT_CHAIN_POLICY_AUTHENTICODE = 2;
        internal const uint CERT_CHAIN_POLICY_AUTHENTICODE_TS = 3;
        internal const uint CERT_CHAIN_POLICY_BASE = 1;
        internal const uint CERT_CHAIN_POLICY_BASIC_CONSTRAINTS = 5;
        internal const uint CERT_CHAIN_POLICY_IGNORE_ALL_REV_UNKNOWN_FLAGS = 0xf00;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CA_REV_UNKNOWN_FLAG = 0x400;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_NOT_TIME_VALID_FLAG = 2;
        internal const uint CERT_CHAIN_POLICY_IGNORE_CTL_SIGNER_REV_UNKNOWN_FLAG = 0x200;
        internal const uint CERT_CHAIN_POLICY_IGNORE_END_REV_UNKNOWN_FLAG = 0x100;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_BASIC_CONSTRAINTS_FLAG = 8;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_NAME_FLAG = 0x40;
        internal const uint CERT_CHAIN_POLICY_IGNORE_INVALID_POLICY_FLAG = 0x80;
        internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_NESTED_FLAG = 4;
        internal const uint CERT_CHAIN_POLICY_IGNORE_NOT_TIME_VALID_FLAG = 1;
        internal const uint CERT_CHAIN_POLICY_IGNORE_ROOT_REV_UNKNOWN_FLAG = 0x800;
        internal const uint CERT_CHAIN_POLICY_IGNORE_WRONG_USAGE_FLAG = 0x20;
        internal const uint CERT_CHAIN_POLICY_MICROSOFT_ROOT = 7;
        internal const uint CERT_CHAIN_POLICY_NT_AUTH = 6;
        internal const uint CERT_CHAIN_POLICY_SSL = 4;
        internal const uint CERT_CHAIN_REVOCATION_ACCUMULATIVE_TIMEOUT = 0x8000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CACHE_ONLY = 0x80000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN = 0x20000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x40000000;
        internal const uint CERT_CHAIN_REVOCATION_CHECK_END_CERT = 0x10000000;
        internal const uint CERT_COMPARE_ANY = 0;
        internal const uint CERT_COMPARE_ATTR = 3;
        internal const uint CERT_COMPARE_CERT_ID = 0x10;
        internal const uint CERT_COMPARE_CROSS_CERT_DIST_POINTS = 0x11;
        internal const uint CERT_COMPARE_CTL_USAGE = 10;
        internal const uint CERT_COMPARE_ENHKEY_USAGE = 10;
        internal const uint CERT_COMPARE_EXISTING = 13;
        internal const uint CERT_COMPARE_HASH = 1;
        internal const uint CERT_COMPARE_ISSUER_OF = 12;
        internal const uint CERT_COMPARE_KEY_IDENTIFIER = 15;
        internal const uint CERT_COMPARE_KEY_SPEC = 9;
        internal const uint CERT_COMPARE_MASK = 0xffff;
        internal const uint CERT_COMPARE_MD5_HASH = 4;
        internal const uint CERT_COMPARE_NAME = 2;
        internal const uint CERT_COMPARE_NAME_STR_A = 7;
        internal const uint CERT_COMPARE_NAME_STR_W = 8;
        internal const uint CERT_COMPARE_PROPERTY = 5;
        internal const uint CERT_COMPARE_PUBKEY_MD5_HASH = 0x12;
        internal const uint CERT_COMPARE_PUBLIC_KEY = 6;
        internal const uint CERT_COMPARE_SHA1_HASH = 1;
        internal const uint CERT_COMPARE_SHIFT = 0x10;
        internal const uint CERT_COMPARE_SIGNATURE_HASH = 14;
        internal const uint CERT_COMPARE_SUBJECT_CERT = 11;
        internal const uint CERT_CRL_SIGN_KEY_USAGE = 2;
        internal const uint CERT_CROSS_CERT_DIST_POINTS_PROP_ID = 0x17;
        internal const uint CERT_CTL_USAGE_PROP_ID = 9;
        internal const uint CERT_DATA_ENCIPHERMENT_KEY_USAGE = 0x10;
        internal const uint CERT_DATE_STAMP_PROP_ID = 0x1b;
        internal const uint CERT_DECIPHER_ONLY_KEY_USAGE = 0x8000;
        internal const uint CERT_DELETE_KEYSET_PROP_ID = 0x65;
        internal const uint CERT_DESCRIPTION_PROP_ID = 13;
        internal const uint CERT_DIGITAL_SIGNATURE_KEY_USAGE = 0x80;
        internal const int CERT_E_CHAINING = -2146762486;
        internal const int CERT_E_EXPIRED = -2146762495;
        internal const int CERT_E_INVALID_NAME = -2146762476;
        internal const int CERT_E_INVALID_POLICY = -2146762477;
        internal const int CERT_E_REVOCATION_FAILURE = -2146762482;
        internal const int CERT_E_REVOKED = -2146762484;
        internal const int CERT_E_UNTRUSTEDROOT = -2146762487;
        internal const int CERT_E_UNTRUSTEDTESTROOT = -2146762483;
        internal const int CERT_E_VALIDITYPERIODNESTING = -2146762494;
        internal const int CERT_E_WRONG_USAGE = -2146762480;
        internal const uint CERT_EFS_PROP_ID = 0x11;
        internal const uint CERT_ENCIPHER_ONLY_KEY_USAGE = 1;
        internal const uint CERT_END_ENTITY_SUBJECT_FLAG = 0x40;
        internal const uint CERT_ENHKEY_USAGE_PROP_ID = 9;
        internal const uint CERT_ENROLLMENT_PROP_ID = 0x1a;
        internal const uint CERT_EXTENDED_ERROR_INFO_PROP_ID = 30;
        internal const uint CERT_FIND_ANY = 0;
        internal const uint CERT_FIND_CERT_ID = 0x100000;
        internal const uint CERT_FIND_CROSS_CERT_DIST_POINTS = 0x110000;
        internal const uint CERT_FIND_CTL_USAGE = 0xa0000;
        internal const uint CERT_FIND_ENHKEY_USAGE = 0xa0000;
        internal const uint CERT_FIND_EXISTING = 0xd0000;
        internal const uint CERT_FIND_HASH = 0x10000;
        internal const uint CERT_FIND_ISSUER_ATTR = 0x30004;
        internal const uint CERT_FIND_ISSUER_NAME = 0x20004;
        internal const uint CERT_FIND_ISSUER_OF = 0xc0000;
        internal const uint CERT_FIND_ISSUER_STR = 0x80004;
        internal const uint CERT_FIND_ISSUER_STR_A = 0x70004;
        internal const uint CERT_FIND_ISSUER_STR_W = 0x80004;
        internal const uint CERT_FIND_KEY_IDENTIFIER = 0xf0000;
        internal const uint CERT_FIND_KEY_SPEC = 0x90000;
        internal const uint CERT_FIND_MD5_HASH = 0x40000;
        internal const uint CERT_FIND_PROPERTY = 0x50000;
        internal const uint CERT_FIND_PUBKEY_MD5_HASH = 0x120000;
        internal const uint CERT_FIND_PUBLIC_KEY = 0x60000;
        internal const uint CERT_FIND_SHA1_HASH = 0x10000;
        internal const uint CERT_FIND_SIGNATURE_HASH = 0xe0000;
        internal const uint CERT_FIND_SUBJECT_ATTR = 0x30007;
        internal const uint CERT_FIND_SUBJECT_CERT = 0xb0000;
        internal const uint CERT_FIND_SUBJECT_NAME = 0x20007;
        internal const uint CERT_FIND_SUBJECT_STR = 0x80007;
        internal const uint CERT_FIND_SUBJECT_STR_A = 0x70007;
        internal const uint CERT_FIND_SUBJECT_STR_W = 0x80007;
        internal const uint CERT_FIRST_RESERVED_PROP_ID = 0x42;
        internal const uint CERT_FORTEZZA_DATA_PROP_ID = 0x12;
        internal const uint CERT_FRIENDLY_NAME_PROP_ID = 11;
        internal const uint CERT_HASH_PROP_ID = 3;
        internal const uint CERT_ID_ISSUER_SERIAL_NUMBER = 1;
        internal const uint CERT_ID_KEY_IDENTIFIER = 2;
        internal const uint CERT_ID_SHA1_HASH = 3;
        internal const uint CERT_IE30_RESERVED_PROP_ID = 7;
        internal const uint CERT_INFO_EXTENSION_FLAG = 11;
        internal const uint CERT_INFO_ISSUER_FLAG = 4;
        internal const uint CERT_INFO_ISSUER_UNIQUE_ID_FLAG = 9;
        internal const uint CERT_INFO_NOT_AFTER_FLAG = 6;
        internal const uint CERT_INFO_NOT_BEFORE_FLAG = 5;
        internal const uint CERT_INFO_SERIAL_NUMBER_FLAG = 2;
        internal const uint CERT_INFO_SIGNATURE_ALGORITHM_FLAG = 3;
        internal const uint CERT_INFO_SUBJECT_FLAG = 7;
        internal const uint CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG = 8;
        internal const uint CERT_INFO_SUBJECT_UNIQUE_ID_FLAG = 10;
        internal const uint CERT_INFO_VERSION_FLAG = 1;
        internal const uint CERT_ISSUER_PUBLIC_KEY_MD5_HASH_PROP_ID = 0x18;
        internal const uint CERT_ISSUER_SERIAL_NUMBER_MD5_HASH_PROP_ID = 0x1c;
        internal const uint CERT_KEY_AGREEMENT_KEY_USAGE = 8;
        internal const uint CERT_KEY_CERT_SIGN_KEY_USAGE = 4;
        internal const uint CERT_KEY_CONTEXT_PROP_ID = 5;
        internal const uint CERT_KEY_ENCIPHERMENT_KEY_USAGE = 0x20;
        internal const uint CERT_KEY_IDENTIFIER_PROP_ID = 20;
        internal const uint CERT_KEY_PROV_HANDLE_PROP_ID = 1;
        internal const uint CERT_KEY_PROV_INFO_PROP_ID = 2;
        internal const uint CERT_KEY_SPEC_PROP_ID = 6;
        internal const uint CERT_MD5_HASH_PROP_ID = 4;
        internal const uint CERT_NAME_ATTR_TYPE = 3;
        internal const uint CERT_NAME_DNS_TYPE = 6;
        internal const uint CERT_NAME_EMAIL_TYPE = 1;
        internal const uint CERT_NAME_FRIENDLY_DISPLAY_TYPE = 5;
        internal const uint CERT_NAME_ISSUER_FLAG = 1;
        internal const uint CERT_NAME_RDN_TYPE = 2;
        internal const uint CERT_NAME_SIMPLE_DISPLAY_TYPE = 4;
        internal const uint CERT_NAME_STR_COMMA_FLAG = 0x4000000;
        internal const uint CERT_NAME_STR_CRLF_FLAG = 0x8000000;
        internal const uint CERT_NAME_STR_DISABLE_IE4_UTF8_FLAG = 0x10000;
        internal const uint CERT_NAME_STR_ENABLE_T61_UNICODE_FLAG = 0x20000;
        internal const uint CERT_NAME_STR_ENABLE_UTF8_UNICODE_FLAG = 0x40000;
        internal const uint CERT_NAME_STR_FORCE_UTF8_DIR_STR_FLAG = 0x80000;
        internal const uint CERT_NAME_STR_NO_PLUS_FLAG = 0x20000000;
        internal const uint CERT_NAME_STR_NO_QUOTING_FLAG = 0x10000000;
        internal const uint CERT_NAME_STR_REVERSE_FLAG = 0x2000000;
        internal const uint CERT_NAME_STR_SEMICOLON_FLAG = 0x40000000;
        internal const uint CERT_NAME_UPN_TYPE = 8;
        internal const uint CERT_NAME_URL_TYPE = 7;
        internal const uint CERT_NEXT_UPDATE_LOCATION_PROP_ID = 10;
        internal const uint CERT_NON_REPUDIATION_KEY_USAGE = 0x40;
        internal const uint CERT_OID_NAME_STR = 2;
        internal const uint CERT_PUBKEY_ALG_PARA_PROP_ID = 0x16;
        internal const uint CERT_PUBKEY_HASH_RESERVED_PROP_ID = 8;
        internal const uint CERT_PVK_FILE_PROP_ID = 12;
        internal const uint CERT_QUERY_CONTENT_CERT = 1;
        internal const uint CERT_QUERY_CONTENT_CERT_PAIR = 13;
        internal const uint CERT_QUERY_CONTENT_CRL = 3;
        internal const uint CERT_QUERY_CONTENT_CTL = 2;
        internal const uint CERT_QUERY_CONTENT_FLAG_ALL = 0x3ffe;
        internal const uint CERT_QUERY_CONTENT_FLAG_CERT = 2;
        internal const uint CERT_QUERY_CONTENT_FLAG_CERT_PAIR = 0x2000;
        internal const uint CERT_QUERY_CONTENT_FLAG_CRL = 8;
        internal const uint CERT_QUERY_CONTENT_FLAG_CTL = 4;
        internal const uint CERT_QUERY_CONTENT_FLAG_PFX = 0x1000;
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS10 = 0x800;
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED = 0x100;
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED = 0x400;
        internal const uint CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED = 0x200;
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT = 0x20;
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL = 0x80;
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL = 0x40;
        internal const uint CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE = 0x10;
        internal const uint CERT_QUERY_CONTENT_PFX = 12;
        internal const uint CERT_QUERY_CONTENT_PKCS10 = 11;
        internal const uint CERT_QUERY_CONTENT_PKCS7_SIGNED = 8;
        internal const uint CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10;
        internal const uint CERT_QUERY_CONTENT_PKCS7_UNSIGNED = 9;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CERT = 5;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CRL = 7;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_CTL = 6;
        internal const uint CERT_QUERY_CONTENT_SERIALIZED_STORE = 4;
        internal const uint CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3;
        internal const uint CERT_QUERY_FORMAT_BASE64_ENCODED = 2;
        internal const uint CERT_QUERY_FORMAT_BINARY = 1;
        internal const uint CERT_QUERY_FORMAT_FLAG_ALL = 14;
        internal const uint CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED = 8;
        internal const uint CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED = 4;
        internal const uint CERT_QUERY_FORMAT_FLAG_BINARY = 2;
        internal const uint CERT_QUERY_OBJECT_BLOB = 2;
        internal const uint CERT_QUERY_OBJECT_FILE = 1;
        internal const uint CERT_RDN_ANY_TYPE = 0;
        internal const uint CERT_RDN_BMP_STRING = 12;
        internal const uint CERT_RDN_ENCODED_BLOB = 1;
        internal const uint CERT_RDN_FLAGS_MASK = 0xff000000;
        internal const uint CERT_RDN_GENERAL_STRING = 10;
        internal const uint CERT_RDN_GRAPHIC_STRING = 8;
        internal const uint CERT_RDN_IA5_STRING = 7;
        internal const uint CERT_RDN_INT4_STRING = 11;
        internal const uint CERT_RDN_ISO646_STRING = 9;
        internal const uint CERT_RDN_NUMERIC_STRING = 3;
        internal const uint CERT_RDN_OCTET_STRING = 2;
        internal const uint CERT_RDN_PRINTABLE_STRING = 4;
        internal const uint CERT_RDN_T61_STRING = 5;
        internal const uint CERT_RDN_TELETEX_STRING = 5;
        internal const uint CERT_RDN_TYPE_MASK = 0xff;
        internal const uint CERT_RDN_UNICODE_STRING = 12;
        internal const uint CERT_RDN_UNIVERSAL_STRING = 11;
        internal const uint CERT_RDN_UTF8_STRING = 13;
        internal const uint CERT_RDN_VIDEOTEX_STRING = 6;
        internal const uint CERT_RDN_VISIBLE_STRING = 9;
        internal const uint CERT_RENEWAL_PROP_ID = 0x40;
        internal const uint CERT_SET_PROPERTY_IGNORE_PERSIST_ERROR_FLAG = 0x80000000;
        internal const uint CERT_SET_PROPERTY_INHIBIT_PERSIST_FLAG = 0x40000000;
        internal const uint CERT_SHA1_HASH_PROP_ID = 3;
        internal const uint CERT_SIGNATURE_HASH_PROP_ID = 15;
        internal const uint CERT_SIMPLE_NAME_STR = 1;
        internal const uint CERT_SMART_CARD_DATA_PROP_ID = 0x10;
        internal const uint CERT_STORE_ADD_ALWAYS = 4;
        internal const uint CERT_STORE_ADD_NEW = 1;
        internal const uint CERT_STORE_ADD_NEWER = 6;
        internal const uint CERT_STORE_ADD_NEWER_INHERIT_PROPERTIES = 7;
        internal const uint CERT_STORE_ADD_REPLACE_EXISTING = 3;
        internal const uint CERT_STORE_ADD_REPLACE_EXISTING_INHERIT_PROPERTIES = 5;
        internal const uint CERT_STORE_ADD_USE_EXISTING = 2;
        internal const uint CERT_STORE_BACKUP_RESTORE_FLAG = 0x800;
        internal const uint CERT_STORE_CREATE_NEW_FLAG = 0x2000;
        internal const uint CERT_STORE_CTRL_AUTO_RESYNC = 4;
        internal const uint CERT_STORE_CTRL_CANCEL_NOTIFY = 5;
        internal const uint CERT_STORE_CTRL_COMMIT = 3;
        internal const uint CERT_STORE_CTRL_NOTIFY_CHANGE = 2;
        internal const uint CERT_STORE_CTRL_RESYNC = 1;
        internal const uint CERT_STORE_DEFER_CLOSE_UNTIL_LAST_FREE_FLAG = 4;
        internal const uint CERT_STORE_DELETE_FLAG = 0x10;
        internal const uint CERT_STORE_ENUM_ARCHIVED_FLAG = 0x200;
        internal const uint CERT_STORE_MANIFOLD_FLAG = 0x100;
        internal const uint CERT_STORE_MAXIMUM_ALLOWED_FLAG = 0x1000;
        internal const uint CERT_STORE_NO_CRYPT_RELEASE_FLAG = 1;
        internal const uint CERT_STORE_OPEN_EXISTING_FLAG = 0x4000;
        internal const uint CERT_STORE_PROV_COLLECTION = 11;
        internal const uint CERT_STORE_PROV_FILE = 3;
        internal const uint CERT_STORE_PROV_FILENAME = 8;
        internal const uint CERT_STORE_PROV_FILENAME_A = 7;
        internal const uint CERT_STORE_PROV_FILENAME_W = 8;
        internal const uint CERT_STORE_PROV_LDAP = 0x10;
        internal const uint CERT_STORE_PROV_LDAP_W = 0x10;
        internal const uint CERT_STORE_PROV_MEMORY = 2;
        internal const uint CERT_STORE_PROV_MSG = 1;
        internal const uint CERT_STORE_PROV_PHYSICAL = 14;
        internal const uint CERT_STORE_PROV_PHYSICAL_W = 14;
        internal const uint CERT_STORE_PROV_PKCS7 = 5;
        internal const uint CERT_STORE_PROV_REG = 4;
        internal const uint CERT_STORE_PROV_SERIALIZED = 6;
        internal const uint CERT_STORE_PROV_SMART_CARD = 15;
        internal const uint CERT_STORE_PROV_SMART_CARD_W = 15;
        internal const uint CERT_STORE_PROV_SYSTEM = 10;
        internal const uint CERT_STORE_PROV_SYSTEM_A = 9;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY = 13;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY_A = 12;
        internal const uint CERT_STORE_PROV_SYSTEM_REGISTRY_W = 13;
        internal const uint CERT_STORE_PROV_SYSTEM_W = 10;
        internal const uint CERT_STORE_READONLY_FLAG = 0x8000;
        internal const uint CERT_STORE_SAVE_AS_PKCS7 = 2;
        internal const uint CERT_STORE_SAVE_AS_STORE = 1;
        internal const uint CERT_STORE_SAVE_TO_FILE = 1;
        internal const uint CERT_STORE_SAVE_TO_FILENAME = 4;
        internal const uint CERT_STORE_SAVE_TO_FILENAME_A = 3;
        internal const uint CERT_STORE_SAVE_TO_FILENAME_W = 4;
        internal const uint CERT_STORE_SAVE_TO_MEMORY = 2;
        internal const uint CERT_STORE_SET_LOCALIZED_NAME_FLAG = 2;
        internal const uint CERT_STORE_SHARE_CONTEXT_FLAG = 0x80;
        internal const uint CERT_STORE_SHARE_STORE_FLAG = 0x40;
        internal const uint CERT_STORE_UPDATE_KEYID_FLAG = 0x400;
        internal const uint CERT_SUBJECT_NAME_MD5_HASH_PROP_ID = 0x1d;
        internal const uint CERT_SUBJECT_PUBLIC_KEY_MD5_HASH_PROP_ID = 0x19;
        internal const uint CERT_SYSTEM_STORE_CURRENT_SERVICE = 0x40000;
        internal const uint CERT_SYSTEM_STORE_CURRENT_SERVICE_ID = 4;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER = 0x10000;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY = 0x70000;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_GROUP_POLICY_ID = 7;
        internal const uint CERT_SYSTEM_STORE_CURRENT_USER_ID = 1;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE = 0x20000;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE = 0x90000;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ENTERPRISE_ID = 9;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY = 0x80000;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_GROUP_POLICY_ID = 8;
        internal const uint CERT_SYSTEM_STORE_LOCAL_MACHINE_ID = 2;
        internal const uint CERT_SYSTEM_STORE_LOCATION_MASK = 0xff0000;
        internal const uint CERT_SYSTEM_STORE_LOCATION_SHIFT = 0x10;
        internal const uint CERT_SYSTEM_STORE_SERVICES = 0x50000;
        internal const uint CERT_SYSTEM_STORE_SERVICES_ID = 5;
        internal const uint CERT_SYSTEM_STORE_UNPROTECTED_FLAG = 0x40000000;
        internal const uint CERT_SYSTEM_STORE_USERS = 0x60000;
        internal const uint CERT_SYSTEM_STORE_USERS_ID = 6;
        internal const uint CERT_TRUST_CTL_IS_NOT_SIGNATURE_VALID = 0x40000;
        internal const uint CERT_TRUST_CTL_IS_NOT_TIME_VALID = 0x20000;
        internal const uint CERT_TRUST_CTL_IS_NOT_VALID_FOR_USAGE = 0x80000;
        internal const uint CERT_TRUST_HAS_EXACT_MATCH_ISSUER = 1;
        internal const uint CERT_TRUST_HAS_EXCLUDED_NAME_CONSTRAINT = 0x8000;
        internal const uint CERT_TRUST_HAS_ISSUANCE_CHAIN_POLICY = 0x200;
        internal const uint CERT_TRUST_HAS_KEY_MATCH_ISSUER = 2;
        internal const uint CERT_TRUST_HAS_NAME_MATCH_ISSUER = 4;
        internal const uint CERT_TRUST_HAS_NOT_DEFINED_NAME_CONSTRAINT = 0x2000;
        internal const uint CERT_TRUST_HAS_NOT_PERMITTED_NAME_CONSTRAINT = 0x4000;
        internal const uint CERT_TRUST_HAS_NOT_SUPPORTED_NAME_CONSTRAINT = 0x1000;
        internal const uint CERT_TRUST_HAS_PREFERRED_ISSUER = 0x100;
        internal const uint CERT_TRUST_HAS_VALID_NAME_CONSTRAINTS = 0x400;
        internal const uint CERT_TRUST_INVALID_BASIC_CONSTRAINTS = 0x400;
        internal const uint CERT_TRUST_INVALID_EXTENSION = 0x100;
        internal const uint CERT_TRUST_INVALID_NAME_CONSTRAINTS = 0x800;
        internal const uint CERT_TRUST_INVALID_POLICY_CONSTRAINTS = 0x200;
        internal const uint CERT_TRUST_IS_COMPLEX_CHAIN = 0x10000;
        internal const uint CERT_TRUST_IS_CYCLIC = 0x80;
        internal const uint CERT_TRUST_IS_NOT_SIGNATURE_VALID = 8;
        internal const uint CERT_TRUST_IS_NOT_TIME_NESTED = 2;
        internal const uint CERT_TRUST_IS_NOT_TIME_VALID = 1;
        internal const uint CERT_TRUST_IS_NOT_VALID_FOR_USAGE = 0x10;
        internal const uint CERT_TRUST_IS_OFFLINE_REVOCATION = 0x1000000;
        internal const uint CERT_TRUST_IS_PARTIAL_CHAIN = 0x10000;
        internal const uint CERT_TRUST_IS_REVOKED = 4;
        internal const uint CERT_TRUST_IS_SELF_SIGNED = 8;
        internal const uint CERT_TRUST_IS_UNTRUSTED_ROOT = 0x20;
        internal const uint CERT_TRUST_NO_ERROR = 0;
        internal const uint CERT_TRUST_NO_ISSUANCE_CHAIN_POLICY = 0x2000000;
        internal const uint CERT_TRUST_REVOCATION_STATUS_UNKNOWN = 0x40;
        internal const uint CERT_X500_NAME_STR = 3;
        internal const uint CMS_SIGNER_INFO = 0x1f5;
        internal const uint CMSG_ATTR_CERT_COUNT_PARAM = 0x1f;
        internal const uint CMSG_ATTR_CERT_PARAM = 0x20;
        internal const uint CMSG_AUTHENTICATED_ATTRIBUTES_FLAG = 8;
        internal const uint CMSG_BARE_CONTENT_FLAG = 1;
        internal const uint CMSG_BARE_CONTENT_PARAM = 3;
        internal const uint CMSG_CERT_COUNT_PARAM = 11;
        internal const uint CMSG_CERT_PARAM = 12;
        internal const uint CMSG_CMS_RECIPIENT_COUNT_PARAM = 0x21;
        internal const uint CMSG_CMS_RECIPIENT_ENCRYPTED_KEY_INDEX_PARAM = 0x23;
        internal const uint CMSG_CMS_RECIPIENT_INDEX_PARAM = 0x22;
        internal const uint CMSG_CMS_RECIPIENT_INFO_PARAM = 0x24;
        internal const uint CMSG_CMS_SIGNER_INFO_PARAM = 0x27;
        internal const uint CMSG_COMPUTED_HASH_PARAM = 0x16;
        internal const uint CMSG_CONTENT_PARAM = 2;
        internal const uint CMSG_CONTENTS_OCTETS_FLAG = 0x10;
        internal const uint CMSG_CRL_COUNT_PARAM = 13;
        internal const uint CMSG_CRL_PARAM = 14;
        internal const uint CMSG_CTRL_ADD_ATTR_CERT = 14;
        internal const uint CMSG_CTRL_ADD_CERT = 10;
        internal const uint CMSG_CTRL_ADD_CMS_SIGNER_INFO = 20;
        internal const uint CMSG_CTRL_ADD_CRL = 12;
        internal const uint CMSG_CTRL_ADD_SIGNER = 6;
        internal const uint CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR = 8;
        internal const uint CMSG_CTRL_DECRYPT = 2;
        internal const uint CMSG_CTRL_DEL_ATTR_CERT = 15;
        internal const uint CMSG_CTRL_DEL_CERT = 11;
        internal const uint CMSG_CTRL_DEL_CRL = 13;
        internal const uint CMSG_CTRL_DEL_SIGNER = 7;
        internal const uint CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR = 9;
        internal const uint CMSG_CTRL_KEY_AGREE_DECRYPT = 0x11;
        internal const uint CMSG_CTRL_KEY_TRANS_DECRYPT = 0x10;
        internal const uint CMSG_CTRL_MAIL_LIST_DECRYPT = 0x12;
        internal const uint CMSG_CTRL_VERIFY_HASH = 5;
        internal const uint CMSG_CTRL_VERIFY_SIGNATURE = 1;
        internal const uint CMSG_CTRL_VERIFY_SIGNATURE_EX = 0x13;
        internal const uint CMSG_DATA = 1;
        internal const uint CMSG_DETACHED_FLAG = 4;
        internal const uint CMSG_ENCODED_MESSAGE = 0x1d;
        internal const uint CMSG_ENCODED_SIGNER = 0x1c;
        internal const uint CMSG_ENCRYPT_PARAM = 0x1a;
        internal const uint CMSG_ENCRYPTED = 6;
        internal const uint CMSG_ENCRYPTED_DIGEST = 0x1b;
        internal const uint CMSG_ENVELOPE_ALGORITHM_PARAM = 15;
        internal const uint CMSG_ENVELOPED = 3;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V0 = 0;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V2 = 2;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V3 = 3;
        internal const uint CMSG_ENVELOPED_RECIPIENT_V4 = 4;
        internal const uint CMSG_HASH_ALGORITHM_PARAM = 20;
        internal const uint CMSG_HASH_DATA_PARAM = 0x15;
        internal const uint CMSG_HASHED = 5;
        internal const uint CMSG_INNER_CONTENT_TYPE_PARAM = 4;
        internal const uint CMSG_KEY_AGREE_EPHEMERAL_KEY_CHOICE = 1;
        internal const uint CMSG_KEY_AGREE_ORIGINATOR_CERT = 1;
        internal const uint CMSG_KEY_AGREE_ORIGINATOR_PUBLIC_KEY = 2;
        internal const uint CMSG_KEY_AGREE_RECIPIENT = 2;
        internal const uint CMSG_KEY_AGREE_STATIC_KEY_CHOICE = 2;
        internal const uint CMSG_KEY_AGREE_VERSION = 3;
        internal const uint CMSG_KEY_TRANS_CMS_VERSION = 2;
        internal const uint CMSG_KEY_TRANS_PKCS_1_5_VERSION = 0;
        internal const uint CMSG_KEY_TRANS_RECIPIENT = 1;
        internal const uint CMSG_LENGTH_ONLY_FLAG = 2;
        internal const uint CMSG_MAIL_LIST_RECIPIENT = 3;
        internal const uint CMSG_MAIL_LIST_VERSION = 4;
        internal const uint CMSG_MAX_LENGTH_FLAG = 0x20;
        internal const uint CMSG_RECIPIENT_COUNT_PARAM = 0x11;
        internal const uint CMSG_RECIPIENT_INDEX_PARAM = 0x12;
        internal const uint CMSG_RECIPIENT_INFO_PARAM = 0x13;
        internal const uint CMSG_SIGNED = 2;
        internal const uint CMSG_SIGNED_AND_ENVELOPED = 4;
        internal const uint CMSG_SIGNER_AUTH_ATTR_PARAM = 9;
        internal const uint CMSG_SIGNER_CERT_ID_PARAM = 0x26;
        internal const uint CMSG_SIGNER_CERT_INFO_PARAM = 7;
        internal const uint CMSG_SIGNER_COUNT_PARAM = 5;
        internal const uint CMSG_SIGNER_HASH_ALGORITHM_PARAM = 8;
        internal const uint CMSG_SIGNER_INFO_PARAM = 6;
        internal const uint CMSG_SIGNER_UNAUTH_ATTR_PARAM = 10;
        internal const uint CMSG_TYPE_PARAM = 1;
        internal const uint CMSG_UNPROTECTED_ATTR_PARAM = 0x25;
        internal const uint CMSG_VERIFY_SIGNER_CERT = 2;
        internal const uint CMSG_VERIFY_SIGNER_CHAIN = 3;
        internal const uint CMSG_VERIFY_SIGNER_NULL = 4;
        internal const uint CMSG_VERIFY_SIGNER_PUBKEY = 1;
        internal const uint CMSG_VERSION_PARAM = 30;
        internal const uint CRYPT_ACQUIRE_CACHE_FLAG = 1;
        internal const uint CRYPT_ACQUIRE_COMPARE_KEY_FLAG = 4;
        internal const uint CRYPT_ACQUIRE_SILENT_FLAG = 0x40;
        internal const uint CRYPT_ACQUIRE_USE_PROV_INFO_FLAG = 2;
        internal const uint CRYPT_ARCHIVABLE = 0x4000;
        internal const uint CRYPT_ASN_ENCODING = 1;
        internal const uint CRYPT_CREATE_IV = 0x200;
        internal const uint CRYPT_CREATE_SALT = 4;
        internal const uint CRYPT_DATA_KEY = 0x800;
        internal const uint CRYPT_DELETEKEYSET = 0x10;
        internal const int CRYPT_E_ASN1_BADTAG = -2146881269;
        internal const int CRYPT_E_ATTRIBUTES_MISSING = -2146889713;
        internal const int CRYPT_E_BAD_ENCODE = -2146885630;
        internal const int CRYPT_E_INVALID_MSG_TYPE = -2146889724;
        internal const int CRYPT_E_ISSUER_SERIALNUMBER = -2146889715;
        internal const int CRYPT_E_MSG_ERROR = -2146889727;
        internal const int CRYPT_E_NO_MATCH = -2146885623;
        internal const int CRYPT_E_NO_REVOCATION_CHECK = -2146885614;
        internal const int CRYPT_E_NO_SIGNER = -2146885618;
        internal const int CRYPT_E_NOT_FOUND = -2146885628;
        internal const int CRYPT_E_RECIPIENT_NOT_FOUND = -2146889717;
        internal const int CRYPT_E_REVOCATION_OFFLINE = -2146885613;
        internal const int CRYPT_E_REVOKED = -2146885616;
        internal const int CRYPT_E_SIGNER_NOT_FOUND = -2146889714;
        internal const int CRYPT_E_UNKNOWN_ALGO = -2146889726;
        internal const uint CRYPT_ENCRYPT_ALG_OID_GROUP_ID = 2;
        internal const uint CRYPT_ENHKEY_USAGE_OID_GROUP_ID = 7;
        internal const uint CRYPT_EXPORTABLE = 1;
        internal const uint CRYPT_EXT_OR_ATTR_OID_GROUP_ID = 6;
        internal const uint CRYPT_FIRST = 1;
        internal const uint CRYPT_FIRST_ALG_OID_GROUP_ID = 1;
        internal const uint CRYPT_FORMAT_STR_MULTI_LINE = 1;
        internal const uint CRYPT_FORMAT_STR_NO_HEX = 0x10;
        internal const uint CRYPT_HASH_ALG_OID_GROUP_ID = 1;
        internal const uint CRYPT_INITIATOR = 0x40;
        internal const uint CRYPT_KEK = 0x400;
        internal const uint CRYPT_LAST_ALG_OID_GROUP_ID = 4;
        internal const uint CRYPT_LAST_OID_GROUP_ID = 9;
        internal const uint CRYPT_MACHINE_KEYSET = 0x20;
        internal const uint CRYPT_NDR_ENCODING = 2;
        internal const uint CRYPT_NEWKEYSET = 8;
        internal const uint CRYPT_NEXT = 2;
        internal const uint CRYPT_NO_SALT = 0x10;
        internal const uint CRYPT_OID_INFO_ALGID_KEY = 3;
        internal const uint CRYPT_OID_INFO_NAME_KEY = 2;
        internal const uint CRYPT_OID_INFO_OID_KEY = 1;
        internal const uint CRYPT_OID_INFO_SIGN_KEY = 4;
        internal const uint CRYPT_ONLINE = 0x80;
        internal const uint CRYPT_POLICY_OID_GROUP_ID = 8;
        internal const uint CRYPT_PREGEN = 0x40;
        internal const uint CRYPT_PUBKEY_ALG_OID_GROUP_ID = 3;
        internal const uint CRYPT_RC2_128BIT_VERSION = 0x3a;
        internal const uint CRYPT_RC2_40BIT_VERSION = 160;
        internal const uint CRYPT_RC2_56BIT_VERSION = 0x34;
        internal const uint CRYPT_RC2_64BIT_VERSION = 120;
        internal const uint CRYPT_RDN_ATTR_OID_GROUP_ID = 5;
        internal const uint CRYPT_RECIPIENT = 0x10;
        internal const uint CRYPT_SF = 0x100;
        internal const uint CRYPT_SGCKEY = 0x2000;
        internal const uint CRYPT_SIGN_ALG_OID_GROUP_ID = 4;
        internal const uint CRYPT_SILENT = 0x40;
        internal const uint CRYPT_TEMPLATE_OID_GROUP_ID = 9;
        internal const uint CRYPT_UPDATE_KEY = 8;
        internal const uint CRYPT_USER_KEYSET = 0x1000;
        internal const uint CRYPT_USER_PROTECTED = 2;
        internal const uint CRYPT_VERIFYCONTEXT = 0xf0000000;
        internal const uint CRYPT_VOLATILE = 0x1000;
        internal const string CRYPT32 = "crypt32.dll";
        internal const byte CUR_BLOB_VERSION = 2;
        internal const uint DSS_MAGIC = 0x31535344;
        internal const uint DSS_PRIV_MAGIC_VER3 = 0x34535344;
        internal const uint DSS_PRIVATE_MAGIC = 0x32535344;
        internal const uint DSS_PUB_MAGIC_VER3 = 0x33535344;
        internal const string DummySignerCommonName = "CN=Dummy Signer";
        internal const int E_NOTIMPL = -2147483647;
        internal const int E_OUTOFMEMORY = -2147024882;
        internal const int ERROR_CALL_NOT_IMPLEMENTED = 120;
        internal const int ERROR_CANCELLED = 0x4c7;
        internal const int ERROR_SUCCESS = 0;
        internal const uint EXPORT_PRIVATE_KEYS = 4;
        internal const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        internal const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        internal const uint HCCE_CURRENT_USER = 0;
        internal const uint HCCE_LOCAL_MACHINE = 1;
        internal const string KERNEL32 = "kernel32.dll";
        internal const uint LMEM_FIXED = 0;
        internal const uint LMEM_ZEROINIT = 0x40;
        internal const uint LPTR = 0x40;
        internal const string MS_DEF_DSS_DH_PROV = "Microsoft Base DSS and Diffie-Hellman Cryptographic Provider";
        internal const string MS_DEF_PROV = "Microsoft Base Cryptographic Provider v1.0";
        internal const string MS_ENH_DSS_DH_PROV = "Microsoft Enhanced DSS and Diffie-Hellman Cryptographic Provider";
        internal const string MS_ENHANCED_PROV = "Microsoft Enhanced Cryptographic Provider v1.0";
        internal const string MS_STRONG_PROV = "Microsoft Strong Cryptographic Provider";
        internal const int NTE_BAD_KEYSET = -2146893802;
        internal const int NTE_BAD_PUBLIC_KEY = -2146893803;
        internal const int NTE_NO_KEY = -2146893811;
        internal const byte OPAQUEKEYBLOB = 9;
        internal const uint PKCS_7_ASN_ENCODING = 0x10000;
        internal const uint PKCS_7_NDR_ENCODING = 0x20000;
        internal const uint PKCS_7_OR_X509_ASN_ENCODING = 0x10001;
        internal const uint PKCS_ATTRIBUTE = 0x16;
        internal const uint PKCS_RC2_CBC_PARAMETERS = 0x29;
        internal const uint PKCS_UTC_TIME = 0x11;
        internal const uint PKCS12_EXPORT_RESERVED_MASK = 0xffff0000;
        internal const uint PKCS7_SIGNER_INFO = 500;
        internal const byte PLAINTEXTKEYBLOB = 8;
        internal const uint PP_ENUMALGS_EX = 0x16;
        internal const byte PRIVATEKEYBLOB = 7;
        internal const uint PROV_DSS_DH = 13;
        internal const uint PROV_RSA_FULL = 1;
        internal const byte PUBLICKEYBLOB = 6;
        internal const byte PUBLICKEYBLOBEX = 10;
        internal const uint REPORT_NO_PRIVATE_KEY = 1;
        internal const uint REPORT_NOT_ABLE_TO_EXPORT_PRIVATE_KEY = 2;
        internal const uint RSA_CSP_PUBLICKEYBLOB = 0x13;
        internal const uint RSA_PRIV_MAGIC = 0x32415352;
        internal const uint RSA_PUB_MAGIC = 0x31415352;
        internal const int S_FALSE = 1;
        internal const int S_OK = 0;
        internal const byte SIMPLEBLOB = 1;
        internal const string SPC_COMMERCIAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.22";
        internal const string SPC_INDIVIDUAL_SP_KEY_PURPOSE_OBJID = "1.3.6.1.4.1.311.2.1.21";
        internal const byte SYMMETRICWRAPKEYBLOB = 11;
        internal const string szOID_AUTHORITY_INFO_ACCESS = "1.3.6.1.5.5.7.1.1";
        internal const string szOID_AUTHORITY_KEY_IDENTIFIER = "2.5.29.1";
        internal const string szOID_BASIC_CONSTRAINTS = "2.5.29.10";
        internal const string szOID_BASIC_CONSTRAINTS2 = "2.5.29.19";
        internal const string szOID_CAPICOM = "1.3.6.1.4.1.311.88";
        internal const string szOID_CAPICOM_attribute = "1.3.6.1.4.1.311.88.2";
        internal const string szOID_CAPICOM_documentDescription = "1.3.6.1.4.1.311.88.2.2";
        internal const string szOID_CAPICOM_documentName = "1.3.6.1.4.1.311.88.2.1";
        internal const string szOID_CAPICOM_encryptedContent = "1.3.6.1.4.1.311.88.3.1";
        internal const string szOID_CAPICOM_encryptedData = "1.3.6.1.4.1.311.88.3";
        internal const string szOID_CAPICOM_version = "1.3.6.1.4.1.311.88.1";
        internal const string szOID_CERT_POLICIES = "2.5.29.32";
        internal const string szOID_CERTIFICATE_TEMPLATE = "1.3.6.1.4.1.311.21.7";
        internal const string szOID_COMMON_NAME = "2.5.4.3";
        internal const string szOID_CRL_DIST_POINTS = "2.5.29.31";
        internal const string szOID_ENHANCED_KEY_USAGE = "2.5.29.37";
        internal const string szOID_ENROLL_CERTTYPE_EXTENSION = "1.3.6.1.4.1.311.20.2";
        internal const string szOID_ISSUER_ALT_NAME = "2.5.29.8";
        internal const string szOID_ISSUER_ALT_NAME2 = "2.5.29.18";
        internal const string szOID_KEY_USAGE = "2.5.29.15";
        internal const string szOID_KEY_USAGE_RESTRICTION = "2.5.29.4";
        internal const string szOID_KEYID_RDN = "1.3.6.1.4.1.311.10.7.1";
        internal const string szOID_NT_PRINCIPAL_NAME = "1.3.6.1.4.1.311.20.2.3";
        internal const string szOID_OIWSEC_desCBC = "1.3.14.3.2.7";
        internal const string szOID_OIWSEC_sha1 = "1.3.14.3.2.26";
        internal const string szOID_OIWSEC_sha1RSASign = "1.3.14.3.2.29";
        internal const string szOID_OIWSEC_SHA256 = "2.16.840.1.101.3.4.1";
        internal const string szOID_OIWSEC_SHA384 = "2.16.840.1.101.3.4.2";
        internal const string szOID_OIWSEC_SHA512 = "2.16.840.1.101.3.4.3";
        internal const string szOID_PKCS_1 = "1.2.840.113549.1.1";
        internal const string szOID_PKCS_10 = "1.2.840.113549.1.10";
        internal const string szOID_PKCS_12 = "1.2.840.113549.1.12";
        internal const string szOID_PKCS_2 = "1.2.840.113549.1.2";
        internal const string szOID_PKCS_3 = "1.2.840.113549.1.3";
        internal const string szOID_PKCS_4 = "1.2.840.113549.1.4";
        internal const string szOID_PKCS_5 = "1.2.840.113549.1.5";
        internal const string szOID_PKCS_6 = "1.2.840.113549.1.6";
        internal const string szOID_PKCS_7 = "1.2.840.113549.1.7";
        internal const string szOID_PKCS_8 = "1.2.840.113549.1.8";
        internal const string szOID_PKCS_9 = "1.2.840.113549.1.9";
        internal const string szOID_PKIX_KP_CLIENT_AUTH = "1.3.6.1.5.5.7.3.2";
        internal const string szOID_PKIX_KP_CODE_SIGNING = "1.3.6.1.5.5.7.3.3";
        internal const string szOID_PKIX_KP_EMAIL_PROTECTION = "1.3.6.1.5.5.7.3.4";
        internal const string szOID_PKIX_KP_SERVER_AUTH = "1.3.6.1.5.5.7.3.1";
        internal const string szOID_PKIX_NO_SIGNATURE = "1.3.6.1.5.5.7.6.2";
        internal const string szOID_RDN_DUMMY_SIGNER = "1.3.6.1.4.1.311.21.9";
        internal const string szOID_RSA_challengePwd = "1.2.840.113549.1.9.7";
        internal const string szOID_RSA_contentType = "1.2.840.113549.1.9.3";
        internal const string szOID_RSA_counterSign = "1.2.840.113549.1.9.6";
        internal const string szOID_RSA_data = "1.2.840.113549.1.7.1";
        internal const string szOID_RSA_DES_EDE3_CBC = "1.2.840.113549.3.7";
        internal const string szOID_RSA_digestedData = "1.2.840.113549.1.7.5";
        internal const string szOID_RSA_emailAddr = "1.2.840.113549.1.9.1";
        internal const string szOID_RSA_encryptedData = "1.2.840.113549.1.7.6";
        internal const string szOID_RSA_envelopedData = "1.2.840.113549.1.7.3";
        internal const string szOID_RSA_extCertAttrs = "1.2.840.113549.1.9.9";
        internal const string szOID_RSA_hashedData = "1.2.840.113549.1.7.5";
        internal const string szOID_RSA_MD5 = "1.2.840.113549.2.5";
        internal const string szOID_RSA_messageDigest = "1.2.840.113549.1.9.4";
        internal const string szOID_RSA_RC2CBC = "1.2.840.113549.3.2";
        internal const string szOID_RSA_RC4 = "1.2.840.113549.3.4";
        internal const string szOID_RSA_signedData = "1.2.840.113549.1.7.2";
        internal const string szOID_RSA_signEnvData = "1.2.840.113549.1.7.4";
        internal const string szOID_RSA_signingTime = "1.2.840.113549.1.9.5";
        internal const string szOID_RSA_SMIMEalg = "1.2.840.113549.1.9.16.3";
        internal const string szOID_RSA_SMIMEalgCMS3DESwrap = "1.2.840.113549.1.9.16.3.6";
        internal const string szOID_RSA_SMIMEalgCMSRC2wrap = "1.2.840.113549.1.9.16.3.7";
        internal const string szOID_RSA_SMIMEalgESDH = "1.2.840.113549.1.9.16.3.5";
        internal const string szOID_RSA_SMIMECapabilities = "1.2.840.113549.1.9.15";
        internal const string szOID_RSA_unstructAddr = "1.2.840.113549.1.9.8";
        internal const string szOID_RSA_unstructName = "1.2.840.113549.1.9.2";
        internal const string szOID_SUBJECT_ALT_NAME = "2.5.29.7";
        internal const string szOID_SUBJECT_ALT_NAME2 = "2.5.29.17";
        internal const string szOID_SUBJECT_KEY_IDENTIFIER = "2.5.29.14";
        internal const string szOID_X957_DSA = "1.2.840.10040.4.1";
        internal const string szOID_X957_sha1DSA = "1.2.840.10040.4.3";
        internal const int TRUST_E_BASIC_CONSTRAINTS = -2146869223;
        internal const int TRUST_E_CERT_SIGNATURE = -2146869244;
        internal const int TRUST_E_FAIL = -2146762485;
        internal const uint USAGE_MATCH_TYPE_AND = 0;
        internal const uint USAGE_MATCH_TYPE_OR = 1;
        internal const uint VER_PLATFORM_WIN32_NT = 2;
        internal const uint VER_PLATFORM_WIN32_WINDOWS = 1;
        internal const uint VER_PLATFORM_WIN32s = 0;
        internal const uint VER_PLATFORM_WINCE = 3;
        internal const uint X509_ANY_STRING = 6;
        internal const uint X509_ASN_ENCODING = 1;
        internal const uint X509_AUTHORITY_KEY_ID = 9;
        internal const uint X509_BASIC_CONSTRAINTS = 13;
        internal const uint X509_BASIC_CONSTRAINTS2 = 15;
        internal const uint X509_BITS = 0x1a;
        internal const uint X509_CERT_POLICIES = 0x10;
        internal const uint X509_CERTIFICATE_TEMPLATE = 0x40;
        internal const uint X509_DSS_PARAMETERS = 0x27;
        internal const uint X509_DSS_PUBLICKEY = 0x26;
        internal const uint X509_DSS_SIGNATURE = 40;
        internal const uint X509_ENHANCED_KEY_USAGE = 0x24;
        internal const uint X509_EXTENSIONS = 5;
        internal const uint X509_KEY_USAGE = 14;
        internal const uint X509_KEY_USAGE_RESTRICTION = 11;
        internal const uint X509_MULTI_BYTE_UINT = 0x26;
        internal const uint X509_NAME = 7;
        internal const uint X509_NAME_VALUE = 6;
        internal const uint X509_NDR_ENCODING = 2;
        internal const uint X509_OCTET_STRING = 0x19;
        internal const uint X509_UNICODE_ANY_STRING = 0x18;
        internal const uint X509_UNICODE_NAME_VALUE = 0x18;

        protected CAPIBase()
        {
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct BLOBHEADER
        {
            internal byte bType;
            internal byte bVersion;
            internal short reserved;
            internal uint aiKeyAlg;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_ENTRY
        {
            internal uint dwAltNameChoice;
            internal CAPIBase.CERT_ALT_NAME_ENTRY_UNION Value;
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_ENTRY_UNION
        {
            [FieldOffset(0)]
            internal CAPIBase.CRYPTOAPI_BLOB DirectoryName;
            [FieldOffset(0)]
            internal CAPIBase.CRYPTOAPI_BLOB IPAddress;
            [FieldOffset(0)]
            internal IntPtr pOtherName;
            [FieldOffset(0)]
            internal IntPtr pszRegisteredID;
            [FieldOffset(0)]
            internal IntPtr pwszDNSName;
            [FieldOffset(0)]
            internal IntPtr pwszRfc822Name;
            [FieldOffset(0)]
            internal IntPtr pwszURL;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ALT_NAME_INFO
        {
            internal uint cAltEntry;
            internal IntPtr rgAltEntry;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_BASIC_CONSTRAINTS_INFO
        {
            internal CAPIBase.CRYPT_BIT_BLOB SubjectType;
            internal bool fPathLenConstraint;
            internal uint dwPathLenConstraint;
            internal uint cSubtreesConstraint;
            internal IntPtr rgSubtreesConstraint;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_BASIC_CONSTRAINTS2_INFO
        {
            internal int fCA;
            internal int fPathLenConstraint;
            internal uint dwPathLenConstraint;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_CONTEXT
        {
            internal uint cbSize;
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
            internal uint cChain;
            internal IntPtr rgpChain;
            internal uint cLowerQualityChainContext;
            internal IntPtr rgpLowerQualityChainContext;
            internal uint fHasRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
            internal CERT_CHAIN_CONTEXT(int size)
            {
                this.cbSize = (uint) size;
                this.dwErrorStatus = 0;
                this.dwInfoStatus = 0;
                this.cChain = 0;
                this.rgpChain = IntPtr.Zero;
                this.cLowerQualityChainContext = 0;
                this.rgpLowerQualityChainContext = IntPtr.Zero;
                this.fHasRevocationFreshnessTime = 0;
                this.dwRevocationFreshnessTime = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_ELEMENT
        {
            internal uint cbSize;
            internal IntPtr pCertContext;
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
            internal IntPtr pRevocationInfo;
            internal IntPtr pIssuanceUsage;
            internal IntPtr pApplicationUsage;
            internal IntPtr pwszExtendedErrorInfo;
            internal CERT_CHAIN_ELEMENT(int size)
            {
                this.cbSize = (uint) size;
                this.pCertContext = IntPtr.Zero;
                this.dwErrorStatus = 0;
                this.dwInfoStatus = 0;
                this.pRevocationInfo = IntPtr.Zero;
                this.pIssuanceUsage = IntPtr.Zero;
                this.pApplicationUsage = IntPtr.Zero;
                this.pwszExtendedErrorInfo = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_PARA
        {
            internal uint cbSize;
            internal CAPIBase.CERT_USAGE_MATCH RequestedUsage;
            internal CAPIBase.CERT_USAGE_MATCH RequestedIssuancePolicy;
            internal uint dwUrlRetrievalTimeout;
            internal bool fCheckRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_PARA
        {
            internal uint cbSize;
            internal uint dwFlags;
            internal IntPtr pvExtraPolicyPara;
            internal CERT_CHAIN_POLICY_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.dwFlags = 0;
                this.pvExtraPolicyPara = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CHAIN_POLICY_STATUS
        {
            internal uint cbSize;
            internal uint dwError;
            internal IntPtr lChainIndex;
            internal IntPtr lElementIndex;
            internal IntPtr pvExtraPolicyStatus;
            internal CERT_CHAIN_POLICY_STATUS(int size)
            {
                this.cbSize = (uint) size;
                this.dwError = 0;
                this.lChainIndex = IntPtr.Zero;
                this.lElementIndex = IntPtr.Zero;
                this.pvExtraPolicyStatus = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_CONTEXT
        {
            internal uint dwCertEncodingType;
            internal IntPtr pbCertEncoded;
            internal uint cbCertEncoded;
            internal IntPtr pCertInfo;
            internal IntPtr hCertStore;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_DSS_PARAMETERS
        {
            internal CAPIBase.CRYPTOAPI_BLOB p;
            internal CAPIBase.CRYPTOAPI_BLOB q;
            internal CAPIBase.CRYPTOAPI_BLOB g;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ENHKEY_USAGE
        {
            internal uint cUsageIdentifier;
            internal IntPtr rgpszUsageIdentifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_EXTENSION
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal bool fCritical;
            internal CAPIBase.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ID
        {
            internal uint dwIdChoice;
            internal CAPIBase.CERT_ID_UNION Value;
        }

        [StructLayout(LayoutKind.Explicit, CharSet=CharSet.Unicode)]
        internal struct CERT_ID_UNION
        {
            [FieldOffset(0)]
            internal CAPIBase.CRYPTOAPI_BLOB HashId;
            [FieldOffset(0)]
            internal CAPIBase.CERT_ISSUER_SERIAL_NUMBER IssuerSerialNumber;
            [FieldOffset(0)]
            internal CAPIBase.CRYPTOAPI_BLOB KeyId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_INFO
        {
            internal uint dwVersion;
            internal CAPIBase.CRYPTOAPI_BLOB SerialNumber;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
            internal CAPIBase.CRYPTOAPI_BLOB Issuer;
            internal System.Runtime.InteropServices.ComTypes.FILETIME NotBefore;
            internal System.Runtime.InteropServices.ComTypes.FILETIME NotAfter;
            internal CAPIBase.CRYPTOAPI_BLOB Subject;
            internal CAPIBase.CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
            internal CAPIBase.CRYPT_BIT_BLOB IssuerUniqueId;
            internal CAPIBase.CRYPT_BIT_BLOB SubjectUniqueId;
            internal uint cExtension;
            internal IntPtr rgExtension;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_ISSUER_SERIAL_NUMBER
        {
            internal CAPIBase.CRYPTOAPI_BLOB Issuer;
            internal CAPIBase.CRYPTOAPI_BLOB SerialNumber;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_KEY_USAGE_RESTRICTION_INFO
        {
            internal uint cCertPolicyId;
            internal IntPtr rgCertPolicyId;
            internal CAPIBase.CRYPT_BIT_BLOB RestrictedKeyUsage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_NAME_INFO
        {
            internal uint cRDN;
            internal IntPtr rgRDN;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_NAME_VALUE
        {
            internal uint dwValueType;
            internal CAPIBase.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_OTHER_NAME
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal CAPIBase.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICIES_INFO
        {
            internal uint cPolicyInfo;
            internal IntPtr rgPolicyInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_ID
        {
            internal uint cCertPolicyElementId;
            internal IntPtr rgpszCertPolicyElementId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszPolicyIdentifier;
            internal uint cPolicyQualifier;
            internal IntPtr rgPolicyQualifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_POLICY_QUALIFIER_INFO
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszPolicyQualifierId;
            private CAPIBase.CRYPTOAPI_BLOB Qualifier;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO
        {
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER Algorithm;
            internal CAPIBase.CRYPT_BIT_BLOB PublicKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_PUBLIC_KEY_INFO2
        {
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER2 Algorithm;
            internal CAPIBase.CRYPT_BIT_BLOB PublicKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_RDN
        {
            internal uint cRDNAttr;
            internal IntPtr rgRDNAttr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_RDN_ATTR
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal uint dwValueType;
            internal CAPIBase.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_SIMPLE_CHAIN
        {
            internal uint cbSize;
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
            internal uint cElement;
            internal IntPtr rgpElement;
            internal IntPtr pTrustListInfo;
            internal uint fHasRevocationFreshnessTime;
            internal uint dwRevocationFreshnessTime;
            internal CERT_SIMPLE_CHAIN(int size)
            {
                this.cbSize = (uint) size;
                this.dwErrorStatus = 0;
                this.dwInfoStatus = 0;
                this.cElement = 0;
                this.rgpElement = IntPtr.Zero;
                this.pTrustListInfo = IntPtr.Zero;
                this.fHasRevocationFreshnessTime = 0;
                this.dwRevocationFreshnessTime = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_TEMPLATE_EXT
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal uint dwMajorVersion;
            private bool fMinorVersion;
            private uint dwMinorVersion;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_TRUST_STATUS
        {
            internal uint dwErrorStatus;
            internal uint dwInfoStatus;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CERT_USAGE_MATCH
        {
            internal uint dwType;
            internal CAPIBase.CERT_ENHKEY_USAGE Usage;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CMS_RECIPIENT_INFO
        {
            internal uint dwRecipientChoice;
            internal IntPtr pRecipientInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CMS_SIGNER_INFO
        {
            internal uint dwVersion;
            internal CAPIBase.CERT_ID SignerId;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal CAPIBase.CRYPTOAPI_BLOB EncryptedHash;
            internal CAPIBase.CRYPT_ATTRIBUTES AuthAttrs;
            internal CAPIBase.CRYPT_ATTRIBUTES UnauthAttrs;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA
        {
            internal uint cbSize;
            internal uint dwSignerIndex;
            internal CAPIBase.CRYPTOAPI_BLOB blob;
            internal CMSG_CTRL_ADD_SIGNER_UNAUTH_ATTR_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.dwSignerIndex = 0;
                this.blob = new CAPIBase.CRYPTOAPI_BLOB();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_DECRYPT_PARA
        {
            internal uint cbSize;
            internal IntPtr hCryptProv;
            internal uint dwKeySpec;
            internal uint dwRecipientIndex;
            internal CMSG_CTRL_DECRYPT_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.hCryptProv = IntPtr.Zero;
                this.dwKeySpec = 0;
                this.dwRecipientIndex = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA
        {
            internal uint cbSize;
            internal uint dwSignerIndex;
            internal uint dwUnauthAttrIndex;
            internal CMSG_CTRL_DEL_SIGNER_UNAUTH_ATTR_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.dwSignerIndex = 0;
                this.dwUnauthAttrIndex = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_KEY_AGREE_DECRYPT_PARA
        {
            internal uint cbSize;
            internal IntPtr hCryptProv;
            internal uint dwKeySpec;
            internal IntPtr pKeyAgree;
            internal uint dwRecipientIndex;
            internal uint dwRecipientEncryptedKeyIndex;
            internal CAPIBase.CRYPT_BIT_BLOB OriginatorPublicKey;
            internal CMSG_CTRL_KEY_AGREE_DECRYPT_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.hCryptProv = IntPtr.Zero;
                this.dwKeySpec = 0;
                this.pKeyAgree = IntPtr.Zero;
                this.dwRecipientIndex = 0;
                this.dwRecipientEncryptedKeyIndex = 0;
                this.OriginatorPublicKey = new CAPIBase.CRYPT_BIT_BLOB();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_KEY_TRANS_DECRYPT_PARA
        {
            internal uint cbSize;
            internal SafeCryptProvHandle hCryptProv;
            internal uint dwKeySpec;
            internal IntPtr pKeyTrans;
            internal uint dwRecipientIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA
        {
            internal uint cbSize;
            internal IntPtr hCryptProv;
            internal uint dwSignerIndex;
            internal uint dwSignerType;
            internal IntPtr pvSigner;
            internal CMSG_CTRL_VERIFY_SIGNATURE_EX_PARA(int size)
            {
                this.cbSize = (uint) size;
                this.hCryptProv = IntPtr.Zero;
                this.dwSignerIndex = 0;
                this.dwSignerType = 0;
                this.pvSigner = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_ENVELOPED_ENCODE_INFO
        {
            internal uint cbSize;
            internal IntPtr hCryptProv;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER ContentEncryptionAlgorithm;
            internal IntPtr pvEncryptionAuxInfo;
            internal uint cRecipients;
            internal IntPtr rgpRecipients;
            internal IntPtr rgCmsRecipients;
            internal uint cCertEncoded;
            internal IntPtr rgCertEncoded;
            internal uint cCrlEncoded;
            internal IntPtr rgCrlEncoded;
            internal uint cAttrCertEncoded;
            internal IntPtr rgAttrCertEncoded;
            internal uint cUnprotectedAttr;
            internal IntPtr rgUnprotectedAttr;
            internal CMSG_ENVELOPED_ENCODE_INFO(int size)
            {
                this.cbSize = (uint) size;
                this.hCryptProv = IntPtr.Zero;
                this.ContentEncryptionAlgorithm = new CAPIBase.CRYPT_ALGORITHM_IDENTIFIER();
                this.pvEncryptionAuxInfo = IntPtr.Zero;
                this.cRecipients = 0;
                this.rgpRecipients = IntPtr.Zero;
                this.rgCmsRecipients = IntPtr.Zero;
                this.cCertEncoded = 0;
                this.rgCertEncoded = IntPtr.Zero;
                this.cCrlEncoded = 0;
                this.rgCrlEncoded = IntPtr.Zero;
                this.cAttrCertEncoded = 0;
                this.rgAttrCertEncoded = IntPtr.Zero;
                this.cUnprotectedAttr = 0;
                this.rgUnprotectedAttr = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_CERT_ID_RECIPIENT_INFO
        {
            internal uint dwVersion;
            internal uint dwOriginatorChoice;
            internal CAPIBase.CERT_ID OriginatorCertId;
            internal IntPtr Padding;
            internal CAPIBase.CRYPTOAPI_BLOB UserKeyingMaterial;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal uint cRecipientEncryptedKeys;
            internal IntPtr rgpRecipientEncryptedKeys;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_PUBLIC_KEY_RECIPIENT_INFO
        {
            internal uint dwVersion;
            internal uint dwOriginatorChoice;
            internal CAPIBase.CERT_PUBLIC_KEY_INFO OriginatorPublicKeyInfo;
            internal CAPIBase.CRYPTOAPI_BLOB UserKeyingMaterial;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal uint cRecipientEncryptedKeys;
            internal IntPtr rgpRecipientEncryptedKeys;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_RECIPIENT_ENCODE_INFO
        {
            internal uint cbSize;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal IntPtr pvKeyEncryptionAuxInfo;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyWrapAlgorithm;
            internal IntPtr pvKeyWrapAuxInfo;
            internal IntPtr hCryptProv;
            internal uint dwKeySpec;
            internal uint dwKeyChoice;
            internal IntPtr pEphemeralAlgorithmOrSenderId;
            internal CAPIBase.CRYPTOAPI_BLOB UserKeyingMaterial;
            internal uint cRecipientEncryptedKeys;
            internal IntPtr rgpRecipientEncryptedKeys;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_AGREE_RECIPIENT_INFO
        {
            internal uint dwVersion;
            internal uint dwOriginatorChoice;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_TRANS_RECIPIENT_ENCODE_INFO
        {
            internal uint cbSize;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal IntPtr pvKeyEncryptionAuxInfo;
            internal IntPtr hCryptProv;
            internal CAPIBase.CRYPT_BIT_BLOB RecipientPublicKey;
            internal CAPIBase.CERT_ID RecipientId;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_KEY_TRANS_RECIPIENT_INFO
        {
            internal uint dwVersion;
            internal CAPIBase.CERT_ID RecipientId;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER KeyEncryptionAlgorithm;
            internal CAPIBase.CRYPTOAPI_BLOB EncryptedKey;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RC2_AUX_INFO
        {
            internal uint cbSize;
            internal uint dwBitLen;
            internal CMSG_RC2_AUX_INFO(int size)
            {
                this.cbSize = (uint) size;
                this.dwBitLen = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct CMSG_RECIPIENT_ENCODE_INFO
        {
            internal uint dwRecipientChoice;
            internal IntPtr pRecipientInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RECIPIENT_ENCRYPTED_KEY_ENCODE_INFO
        {
            internal uint cbSize;
            internal CAPIBase.CRYPT_BIT_BLOB RecipientPublicKey;
            internal CAPIBase.CERT_ID RecipientId;
            internal System.Runtime.InteropServices.ComTypes.FILETIME Date;
            internal IntPtr pOtherAttr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_RECIPIENT_ENCRYPTED_KEY_INFO
        {
            internal CAPIBase.CERT_ID RecipientId;
            internal CAPIBase.CRYPTOAPI_BLOB EncryptedKey;
            internal System.Runtime.InteropServices.ComTypes.FILETIME Date;
            internal IntPtr pOtherAttr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNED_ENCODE_INFO
        {
            internal uint cbSize;
            internal uint cSigners;
            internal IntPtr rgSigners;
            internal uint cCertEncoded;
            internal IntPtr rgCertEncoded;
            internal uint cCrlEncoded;
            internal IntPtr rgCrlEncoded;
            internal uint cAttrCertEncoded;
            internal IntPtr rgAttrCertEncoded;
            internal CMSG_SIGNED_ENCODE_INFO(int size)
            {
                this.cbSize = (uint) size;
                this.cSigners = 0;
                this.rgSigners = IntPtr.Zero;
                this.cCertEncoded = 0;
                this.rgCertEncoded = IntPtr.Zero;
                this.cCrlEncoded = 0;
                this.rgCrlEncoded = IntPtr.Zero;
                this.cAttrCertEncoded = 0;
                this.rgAttrCertEncoded = IntPtr.Zero;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNER_ENCODE_INFO
        {
            internal uint cbSize;
            internal IntPtr pCertInfo;
            internal IntPtr hCryptProv;
            internal uint dwKeySpec;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal IntPtr pvHashAuxInfo;
            internal uint cAuthAttr;
            internal IntPtr rgAuthAttr;
            internal uint cUnauthAttr;
            internal IntPtr rgUnauthAttr;
            internal CAPIBase.CERT_ID SignerId;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal IntPtr pvHashEncryptionAuxInfo;
            [DllImport("kernel32.dll", SetLastError=true)]
            internal static extern IntPtr LocalFree(IntPtr hMem);
            [DllImport("advapi32.dll", SetLastError=true)]
            internal static extern bool CryptReleaseContext([In] IntPtr hProv, [In] uint dwFlags);
            internal CMSG_SIGNER_ENCODE_INFO(int size)
            {
                this.cbSize = (uint) size;
                this.pCertInfo = IntPtr.Zero;
                this.hCryptProv = IntPtr.Zero;
                this.dwKeySpec = 0;
                this.HashAlgorithm = new CAPIBase.CRYPT_ALGORITHM_IDENTIFIER();
                this.pvHashAuxInfo = IntPtr.Zero;
                this.cAuthAttr = 0;
                this.rgAuthAttr = IntPtr.Zero;
                this.cUnauthAttr = 0;
                this.rgUnauthAttr = IntPtr.Zero;
                this.SignerId = new CAPIBase.CERT_ID();
                this.HashEncryptionAlgorithm = new CAPIBase.CRYPT_ALGORITHM_IDENTIFIER();
                this.pvHashEncryptionAuxInfo = IntPtr.Zero;
            }

            internal void Dispose()
            {
                if (this.hCryptProv != IntPtr.Zero)
                {
                    CryptReleaseContext(this.hCryptProv, 0);
                }
                if (this.SignerId.Value.KeyId.pbData != IntPtr.Zero)
                {
                    LocalFree(this.SignerId.Value.KeyId.pbData);
                }
                if (this.rgAuthAttr != IntPtr.Zero)
                {
                    LocalFree(this.rgAuthAttr);
                }
                if (this.rgUnauthAttr != IntPtr.Zero)
                {
                    LocalFree(this.rgUnauthAttr);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CMSG_SIGNER_INFO
        {
            internal uint dwVersion;
            internal CAPIBase.CRYPTOAPI_BLOB Issuer;
            internal CAPIBase.CRYPTOAPI_BLOB SerialNumber;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
            internal CAPIBase.CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
            internal CAPIBase.CRYPTOAPI_BLOB EncryptedHash;
            internal CAPIBase.CRYPT_ATTRIBUTES AuthAttrs;
            internal CAPIBase.CRYPT_ATTRIBUTES UnauthAttrs;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal class CMSG_STREAM_INFO
        {
            internal uint cbContent;
            internal CAPIBase.PFN_CMSG_STREAM_OUTPUT pfnStreamOutput;
            internal IntPtr pvArg;
            internal CMSG_STREAM_INFO(uint cbContent, CAPIBase.PFN_CMSG_STREAM_OUTPUT pfnStreamOutput, IntPtr pvArg)
            {
                this.cbContent = cbContent;
                this.pfnStreamOutput = pfnStreamOutput;
                this.pvArg = pvArg;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal CAPIBase.CRYPTOAPI_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ALGORITHM_IDENTIFIER2
        {
            internal IntPtr pszObjId;
            internal CAPIBase.CRYPTOAPI_BLOB Parameters;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTE
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal uint cValue;
            internal IntPtr rgValue;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTE_TYPE_VALUE
        {
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszObjId;
            internal CAPIBase.CRYPTOAPI_BLOB Value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_ATTRIBUTES
        {
            internal uint cAttr;
            internal IntPtr rgAttr;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_BIT_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
            internal uint cUnusedBits;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_KEY_PROV_INFO
        {
            internal string pwszContainerName;
            internal string pwszProvName;
            internal uint dwProvType;
            internal uint dwFlags;
            internal uint cProvParam;
            internal IntPtr rgProvParam;
            internal uint dwKeySpec;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_OID_INFO
        {
            internal uint cbSize;
            [MarshalAs(UnmanagedType.LPStr)]
            internal string pszOID;
            internal string pwszName;
            internal uint dwGroupId;
            internal uint Algid;
            internal CAPIBase.CRYPTOAPI_BLOB ExtraInfo;
            internal CRYPT_OID_INFO(int size)
            {
                this.cbSize = (uint) size;
                this.pszOID = null;
                this.pwszName = null;
                this.dwGroupId = 0;
                this.Algid = 0;
                this.ExtraInfo = new CAPIBase.CRYPTOAPI_BLOB();
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPT_RC2_CBC_PARAMETERS
        {
            internal uint dwVersion;
            internal bool fIV;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
            internal byte[] rgbIV;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct CRYPTOAPI_BLOB
        {
            internal uint cbData;
            internal IntPtr pbData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct DSSPUBKEY
        {
            internal uint magic;
            internal uint bitlen;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct KEY_USAGE_STRUCT
        {
            internal string pwszKeyUsage;
            internal uint dwKeyUsageBit;
            internal KEY_USAGE_STRUCT(string pwszKeyUsage, uint dwKeyUsageBit)
            {
                this.pwszKeyUsage = pwszKeyUsage;
                this.dwKeyUsageBit = dwKeyUsageBit;
            }
        }

        internal delegate bool PFN_CMSG_STREAM_OUTPUT(IntPtr pvArg, IntPtr pbData, uint cbData, bool fFinal);

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct PROV_ENUMALGS_EX
        {
            internal uint aiAlgid;
            internal uint dwDefaultLen;
            internal uint dwMinLen;
            internal uint dwMaxLen;
            internal uint dwProtocols;
            internal uint dwNameLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
            internal byte[] szName;
            internal uint dwLongNameLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst=40)]
            internal byte[] szLongName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct RSAPUBKEY
        {
            internal uint magic;
            internal uint bitlen;
            internal uint pubexp;
        }
    }
}

