namespace System.Security.Cryptography
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class BCryptNative
    {
        private static bool? s_bcryptSupported;

        [SecurityCritical]
        internal static int GetInt32Property<T>(T algorithm, string property) where T: SafeHandle
        {
            return BitConverter.ToInt32(GetProperty<T>(algorithm, property), 0);
        }

        [SecurityCritical]
        internal static byte[] GetProperty<T>(T algorithm, string property) where T: SafeHandle
        {
            BCryptPropertyGetter<T> getter = null;
            if (typeof(T) == typeof(SafeBCryptAlgorithmHandle))
            {
                getter = new BCryptPropertyGetter<SafeBCryptAlgorithmHandle>(UnsafeNativeMethods.BCryptGetAlgorithmProperty) as BCryptPropertyGetter<T>;
            }
            else if (typeof(T) == typeof(SafeBCryptHashHandle))
            {
                getter = new BCryptPropertyGetter<SafeBCryptHashHandle>(UnsafeNativeMethods.BCryptGetHashProperty) as BCryptPropertyGetter<T>;
            }
            int pcbResult = 0;
            ErrorCode code = getter(algorithm, property, null, 0, ref pcbResult, 0);
            if ((code != ErrorCode.BufferToSmall) && (code != ErrorCode.Success))
            {
                throw new CryptographicException((int) code);
            }
            byte[] pbOutput = new byte[pcbResult];
            code = getter(algorithm, property, pbOutput, pbOutput.Length, ref pcbResult, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return pbOutput;
        }

        internal static void MapAlgorithmIdToMagic(string algorithm, out KeyBlobMagicNumber algorithmMagic, out int keySize)
        {
            switch (algorithm)
            {
                case "ECDH_P256":
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP256;
                    keySize = 0x100;
                    return;

                case "ECDH_P384":
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP384;
                    keySize = 0x180;
                    return;

                case "ECDH_P521":
                    algorithmMagic = KeyBlobMagicNumber.ECDHPublicP521;
                    keySize = 0x209;
                    return;

                case "ECDSA_P256":
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP256;
                    keySize = 0x100;
                    return;

                case "ECDSA_P384":
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP384;
                    keySize = 0x180;
                    return;

                case "ECDSA_P521":
                    algorithmMagic = KeyBlobMagicNumber.ECDsaPublicP521;
                    keySize = 0x209;
                    return;
            }
            throw new ArgumentException(System.SR.GetString("Cryptography_UnknownEllipticCurveAlgorithm"));
        }

        [SecurityCritical]
        internal static SafeBCryptAlgorithmHandle OpenAlgorithm(string algorithm, string implementation)
        {
            SafeBCryptAlgorithmHandle phAlgorithm = null;
            ErrorCode code = UnsafeNativeMethods.BCryptOpenAlgorithmProvider(out phAlgorithm, algorithm, implementation, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return phAlgorithm;
        }

        internal static bool BCryptSupported
        {
            [SecurityCritical]
            get
            {
                if (!s_bcryptSupported.HasValue)
                {
                    using (Microsoft.Win32.SafeLibraryHandle handle = Microsoft.Win32.UnsafeNativeMethods.LoadLibraryEx("bcrypt", IntPtr.Zero, 0))
                    {
                        s_bcryptSupported = new bool?(!handle.IsInvalid);
                    }
                }
                return s_bcryptSupported.Value;
            }
        }

        internal static class AlgorithmName
        {
            public const string ECDHP256 = "ECDH_P256";
            public const string ECDHP384 = "ECDH_P384";
            public const string ECDHP521 = "ECDH_P521";
            public const string ECDsaP256 = "ECDSA_P256";
            public const string ECDsaP384 = "ECDSA_P384";
            public const string ECDsaP521 = "ECDSA_P521";
            public const string MD5 = "MD5";
            public const string Sha1 = "SHA1";
            public const string Sha256 = "SHA256";
            public const string Sha384 = "SHA384";
            public const string Sha512 = "SHA512";
        }

        private delegate BCryptNative.ErrorCode BCryptPropertyGetter<T>(T hObject, string pszProperty, byte[] pbOutput, int cbOutput, ref int pcbResult, int dwFlags) where T: SafeHandle;

        internal enum ErrorCode
        {
            BufferToSmall = -1073741789,
            ObjectNameNotFound = -1073741772,
            Success = 0
        }

        internal static class HashPropertyName
        {
            public const string HashLength = "HashDigestLength";
        }

        internal enum KeyBlobMagicNumber
        {
            ECDHPublicP256 = 0x314b4345,
            ECDHPublicP384 = 0x334b4345,
            ECDHPublicP521 = 0x354b4345,
            ECDsaPublicP256 = 0x31534345,
            ECDsaPublicP384 = 0x33534345,
            ECDsaPublicP521 = 0x35534345
        }

        internal static class KeyDerivationFunction
        {
            public const string Hash = "HASH";
            public const string Hmac = "HMAC";
            public const string Tls = "TLS_PRF";
        }

        internal static class ObjectPropertyName
        {
            public const string ObjectLength = "ObjectLength";
        }

        internal static class ProviderName
        {
            public const string MicrosoftPrimitiveProvider = "Microsoft Primitive Provider";
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical(SecurityCriticalScope.Everything)]
        internal static class UnsafeNativeMethods
        {
            [DllImport("bcrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern BCryptNative.ErrorCode BCryptCreateHash(SafeBCryptAlgorithmHandle hAlgorithm, out SafeBCryptHashHandle phHash, IntPtr pbHashObject, int cbHashObject, IntPtr pbSecret, int cbSecret, int dwFlags);
            [DllImport("bcrypt.dll")]
            internal static extern BCryptNative.ErrorCode BCryptFinishHash(SafeBCryptHashHandle hHash, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, int dwFlags);
            [DllImport("bcrypt.dll", EntryPoint="BCryptGetProperty", CharSet=CharSet.Unicode)]
            internal static extern BCryptNative.ErrorCode BCryptGetAlgorithmProperty(SafeBCryptAlgorithmHandle hObject, string pszProperty, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput, int cbOutput, [In, Out] ref int pcbResult, int flags);
            [DllImport("bcrypt.dll", EntryPoint="BCryptGetProperty", CharSet=CharSet.Unicode)]
            internal static extern BCryptNative.ErrorCode BCryptGetHashProperty(SafeBCryptHashHandle hObject, string pszProperty, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput, int cbOutput, [In, Out] ref int pcbResult, int flags);
            [DllImport("bcrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern BCryptNative.ErrorCode BCryptGetProperty(SafeBCryptAlgorithmHandle hObject, string pszProperty, [In, Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput, int cbOutput, [In, Out] ref int pcbResult, int flags);
            [DllImport("bcrypt.dll")]
            internal static extern BCryptNative.ErrorCode BCryptHashData(SafeBCryptHashHandle hHash, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, int dwFlags);
            [DllImport("bcrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern BCryptNative.ErrorCode BCryptOpenAlgorithmProvider(out SafeBCryptAlgorithmHandle phAlgorithm, string pszAlgId, string pszImplementation, int dwFlags);
        }
    }
}

