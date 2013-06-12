namespace System.Net.Security
{
    using System;
    using System.Collections;
    using System.Net;
    using System.Runtime.InteropServices;

    internal static class SslSessionsCache
    {
        private const int c_CheckExpiredModulo = 0x20;
        private static Hashtable s_CachedCreds = new Hashtable(0x20);

        internal static void CacheCredential(SafeFreeCredentials creds, byte[] thumbPrint, SchProtocols allowedProtocols, EncryptionPolicy encryptionPolicy)
        {
            if (!creds.IsInvalid)
            {
                object obj2 = new SslCredKey(thumbPrint, allowedProtocols, encryptionPolicy);
                SafeCredentialReference reference = s_CachedCreds[obj2] as SafeCredentialReference;
                if (((reference == null) || reference.IsClosed) || reference._Target.IsInvalid)
                {
                    lock (s_CachedCreds)
                    {
                        reference = s_CachedCreds[obj2] as SafeCredentialReference;
                        if ((reference == null) || reference.IsClosed)
                        {
                            reference = SafeCredentialReference.CreateReference(creds);
                            if (reference != null)
                            {
                                s_CachedCreds[obj2] = reference;
                                if ((s_CachedCreds.Count % 0x20) == 0)
                                {
                                    DictionaryEntry[] array = new DictionaryEntry[s_CachedCreds.Count];
                                    s_CachedCreds.CopyTo(array, 0);
                                    for (int i = 0; i < array.Length; i++)
                                    {
                                        reference = array[i].Value as SafeCredentialReference;
                                        if (reference != null)
                                        {
                                            creds = reference._Target;
                                            reference.Close();
                                            if ((!creds.IsClosed && !creds.IsInvalid) && ((reference = SafeCredentialReference.CreateReference(creds)) != null))
                                            {
                                                s_CachedCreds[array[i].Key] = reference;
                                            }
                                            else
                                            {
                                                s_CachedCreds.Remove(array[i].Key);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static SafeFreeCredentials TryCachedCredential(byte[] thumbPrint, SchProtocols allowedProtocols, EncryptionPolicy encryptionPolicy)
        {
            if (s_CachedCreds.Count != 0)
            {
                object obj2 = new SslCredKey(thumbPrint, allowedProtocols, encryptionPolicy);
                SafeCredentialReference reference = s_CachedCreds[obj2] as SafeCredentialReference;
                if (((reference != null) && !reference.IsClosed) && !reference._Target.IsInvalid)
                {
                    return reference._Target;
                }
            }
            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SslCredKey
        {
            private static readonly byte[] s_EmptyArray;
            private byte[] _CertThumbPrint;
            private SchProtocols _AllowedProtocols;
            private EncryptionPolicy _EncryptionPolicy;
            private int _HashCode;
            internal SslCredKey(byte[] thumbPrint, SchProtocols allowedProtocols, EncryptionPolicy encryptionPolicy)
            {
                this._CertThumbPrint = (thumbPrint == null) ? s_EmptyArray : thumbPrint;
                this._HashCode = 0;
                if (thumbPrint != null)
                {
                    this._HashCode ^= this._CertThumbPrint[0];
                    if (1 < this._CertThumbPrint.Length)
                    {
                        this._HashCode ^= this._CertThumbPrint[1] << 8;
                    }
                    if (2 < this._CertThumbPrint.Length)
                    {
                        this._HashCode ^= this._CertThumbPrint[2] << 0x10;
                    }
                    if (3 < this._CertThumbPrint.Length)
                    {
                        this._HashCode ^= this._CertThumbPrint[3] << 0x18;
                    }
                }
                this._HashCode ^= allowedProtocols;
                this._HashCode ^= encryptionPolicy;
                this._AllowedProtocols = allowedProtocols;
                this._EncryptionPolicy = encryptionPolicy;
            }

            public override int GetHashCode()
            {
                return this._HashCode;
            }

            public static bool operator ==(SslSessionsCache.SslCredKey sslCredKey1, SslSessionsCache.SslCredKey sslCredKey2)
            {
                return ((sslCredKey1 == sslCredKey2) || (((sslCredKey1 != 0) && (sslCredKey2 != 0)) && sslCredKey1.Equals(sslCredKey2)));
            }

            public static bool operator !=(SslSessionsCache.SslCredKey sslCredKey1, SslSessionsCache.SslCredKey sslCredKey2)
            {
                if (sslCredKey1 == sslCredKey2)
                {
                    return false;
                }
                if ((sslCredKey1 != 0) && (sslCredKey2 != 0))
                {
                    return !sslCredKey1.Equals(sslCredKey2);
                }
                return true;
            }

            public override bool Equals(object y)
            {
                SslSessionsCache.SslCredKey key = (SslSessionsCache.SslCredKey) y;
                if (this._CertThumbPrint.Length != key._CertThumbPrint.Length)
                {
                    return false;
                }
                if (this._HashCode != key._HashCode)
                {
                    return false;
                }
                if (this._EncryptionPolicy != key._EncryptionPolicy)
                {
                    return false;
                }
                if (this._AllowedProtocols != key._AllowedProtocols)
                {
                    return false;
                }
                for (int i = 0; i < this._CertThumbPrint.Length; i++)
                {
                    if (this._CertThumbPrint[i] != key._CertThumbPrint[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            static SslCredKey()
            {
                s_EmptyArray = new byte[0];
            }
        }
    }
}

