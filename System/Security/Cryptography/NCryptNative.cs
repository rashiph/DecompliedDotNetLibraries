namespace System.Security.Cryptography
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class NCryptNative
    {
        private static bool? s_ncryptSupported;

        internal static byte[] BuildEccPublicBlob(string algorithm, BigInteger x, BigInteger y)
        {
            BCryptNative.KeyBlobMagicNumber number;
            int num;
            BCryptNative.MapAlgorithmIdToMagic(algorithm, out number, out num);
            byte[] src = ReverseBytes(FillKeyParameter(x.ToByteArray(), num));
            byte[] buffer2 = ReverseBytes(FillKeyParameter(y.ToByteArray(), num));
            byte[] dst = new byte[(8 + src.Length) + buffer2.Length];
            Buffer.BlockCopy(BitConverter.GetBytes((int) number), 0, dst, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(src.Length), 0, dst, 4, 4);
            Buffer.BlockCopy(src, 0, dst, 8, src.Length);
            Buffer.BlockCopy(buffer2, 0, dst, 8 + src.Length, buffer2.Length);
            return dst;
        }

        [SecurityCritical]
        internal static SafeNCryptKeyHandle CreatePersistedKey(SafeNCryptProviderHandle provider, string algorithm, string name, CngKeyCreationOptions options)
        {
            SafeNCryptKeyHandle phKey = null;
            ErrorCode code = UnsafeNativeMethods.NCryptCreatePersistedKey(provider, out phKey, algorithm, name, 0, options);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return phKey;
        }

        [SecurityCritical]
        internal static void DeleteKey(SafeNCryptKeyHandle key)
        {
            ErrorCode code = UnsafeNativeMethods.NCryptDeleteKey(key, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            key.Dispose();
        }

        [SecurityCritical]
        private static unsafe byte[] DeriveKeyMaterial(SafeNCryptSecretHandle secretAgreement, string kdf, NCryptBuffer[] parameters, SecretAgreementFlags flags)
        {
            fixed (NCryptBuffer* bufferRef = parameters)
            {
                NCryptBufferDesc pParameterList = new NCryptBufferDesc {
                    ulVersion = 0,
                    cBuffers = parameters.Length,
                    pBuffers = new IntPtr((void*) bufferRef)
                };
                int pcbResult = 0;
                ErrorCode code = UnsafeNativeMethods.NCryptDeriveKey(secretAgreement, kdf, ref pParameterList, null, 0, out pcbResult, flags);
                if ((code != ErrorCode.Success) && (code != ErrorCode.BufferTooSmall))
                {
                    throw new CryptographicException((int) code);
                }
                byte[] pbDerivedKey = new byte[pcbResult];
                code = UnsafeNativeMethods.NCryptDeriveKey(secretAgreement, kdf, ref pParameterList, pbDerivedKey, pbDerivedKey.Length, out pcbResult, flags);
                if (code != ErrorCode.Success)
                {
                    throw new CryptographicException((int) code);
                }
                return pbDerivedKey;
            }
        }

        [SecurityCritical]
        private static unsafe byte[] DeriveKeyMaterial(SafeNCryptSecretHandle secretAgreement, string kdf, string hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend, SecretAgreementFlags flags)
        {
            byte[] buffer5;
            List<NCryptBuffer> list = new List<NCryptBuffer>();
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    zero = Marshal.StringToCoTaskMemUni(hashAlgorithm);
                }
                NCryptBuffer item = new NCryptBuffer {
                    cbBuffer = (hashAlgorithm.Length + 1) * 2,
                    BufferType = BufferType.KdfHashAlgorithm,
                    pvBuffer = zero
                };
                list.Add(item);
                try
                {
                    fixed (byte* numRef = hmacKey)
                    {
                        fixed (byte* numRef2 = secretPrepend)
                        {
                            fixed (byte* numRef3 = secretAppend)
                            {
                                if (numRef != IntPtr.Zero)
                                {
                                    NCryptBuffer buffer2 = new NCryptBuffer {
                                        cbBuffer = hmacKey.Length,
                                        BufferType = BufferType.KdfHmacKey,
                                        pvBuffer = new IntPtr((void*) numRef)
                                    };
                                    list.Add(buffer2);
                                }
                                if (numRef2 != IntPtr.Zero)
                                {
                                    NCryptBuffer buffer3 = new NCryptBuffer {
                                        cbBuffer = secretPrepend.Length,
                                        BufferType = BufferType.KdfSecretPrepend,
                                        pvBuffer = new IntPtr((void*) numRef2)
                                    };
                                    list.Add(buffer3);
                                }
                                if (numRef3 != IntPtr.Zero)
                                {
                                    NCryptBuffer buffer4 = new NCryptBuffer {
                                        cbBuffer = secretAppend.Length,
                                        BufferType = BufferType.KdfSecretAppend,
                                        pvBuffer = new IntPtr((void*) numRef3)
                                    };
                                    list.Add(buffer4);
                                }
                                return DeriveKeyMaterial(secretAgreement, kdf, list.ToArray(), flags);
                            }
                        }
                    }
                }
                finally
                {
                    numRef = null;
                    numRef2 = null;
                    numRef3 = null;
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
            }
            return buffer5;
        }

        [SecurityCritical]
        internal static byte[] DeriveKeyMaterialHash(SafeNCryptSecretHandle secretAgreement, string hashAlgorithm, byte[] secretPrepend, byte[] secretAppend, SecretAgreementFlags flags)
        {
            return DeriveKeyMaterial(secretAgreement, "HASH", hashAlgorithm, null, secretPrepend, secretAppend, flags);
        }

        [SecurityCritical]
        internal static byte[] DeriveKeyMaterialHmac(SafeNCryptSecretHandle secretAgreement, string hashAlgorithm, byte[] hmacKey, byte[] secretPrepend, byte[] secretAppend, SecretAgreementFlags flags)
        {
            return DeriveKeyMaterial(secretAgreement, "HMAC", hashAlgorithm, hmacKey, secretPrepend, secretAppend, flags);
        }

        [SecurityCritical]
        internal static unsafe byte[] DeriveKeyMaterialTls(SafeNCryptSecretHandle secretAgreement, byte[] label, byte[] seed, SecretAgreementFlags flags)
        {
            NCryptBuffer[] parameters = new NCryptBuffer[2];
            fixed (byte* numRef = label)
            {
                fixed (byte* numRef2 = seed)
                {
                    NCryptBuffer buffer = new NCryptBuffer {
                        cbBuffer = label.Length,
                        BufferType = BufferType.KdfTlsLabel,
                        pvBuffer = new IntPtr((void*) numRef)
                    };
                    parameters[0] = buffer;
                    NCryptBuffer buffer2 = new NCryptBuffer {
                        cbBuffer = seed.Length,
                        BufferType = BufferType.KdfTlsSeed,
                        pvBuffer = new IntPtr((void*) numRef2)
                    };
                    parameters[1] = buffer2;
                    return DeriveKeyMaterial(secretAgreement, "TLS_PRF", parameters, flags);
                }
            }
        }

        [SecurityCritical]
        internal static SafeNCryptSecretHandle DeriveSecretAgreement(SafeNCryptKeyHandle privateKey, SafeNCryptKeyHandle otherPartyPublicKey)
        {
            SafeNCryptSecretHandle handle;
            ErrorCode code = UnsafeNativeMethods.NCryptSecretAgreement(privateKey, otherPartyPublicKey, out handle, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return handle;
        }

        [SecurityCritical]
        internal static byte[] ExportKey(SafeNCryptKeyHandle key, string format)
        {
            int pcbResult = 0;
            ErrorCode code = UnsafeNativeMethods.NCryptExportKey(key, IntPtr.Zero, format, IntPtr.Zero, null, 0, out pcbResult, 0);
            if ((code != ErrorCode.Success) && (code != ErrorCode.BufferTooSmall))
            {
                throw new CryptographicException((int) code);
            }
            byte[] pbOutput = new byte[pcbResult];
            code = UnsafeNativeMethods.NCryptExportKey(key, IntPtr.Zero, format, IntPtr.Zero, pbOutput, pbOutput.Length, out pcbResult, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return pbOutput;
        }

        private static byte[] FillKeyParameter(byte[] key, int keySize)
        {
            int num = (keySize / 8) + (((keySize % 8) == 0) ? 0 : 1);
            if (key.Length == num)
            {
                return key;
            }
            byte[] dst = new byte[num];
            Buffer.BlockCopy(key, 0, dst, 0, Math.Min(key.Length, dst.Length));
            return dst;
        }

        [SecurityCritical]
        internal static void FinalizeKey(SafeNCryptKeyHandle key)
        {
            ErrorCode code = UnsafeNativeMethods.NCryptFinalizeKey(key, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
        }

        [SecurityCritical]
        internal static byte[] GetProperty(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions, out bool foundProperty)
        {
            int pcbResult = 0;
            ErrorCode code = UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, (byte[]) null, 0, out pcbResult, propertyOptions);
            if (((code != ErrorCode.Success) && (code != ErrorCode.BufferTooSmall)) && (code != ErrorCode.NotFound))
            {
                throw new CryptographicException((int) code);
            }
            foundProperty = code != ErrorCode.NotFound;
            byte[] pbOutput = null;
            if ((code != ErrorCode.NotFound) && (pcbResult > 0))
            {
                pbOutput = new byte[pcbResult];
                code = UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, pbOutput, pbOutput.Length, out pcbResult, propertyOptions);
                if (code != ErrorCode.Success)
                {
                    throw new CryptographicException((int) code);
                }
                foundProperty = true;
            }
            return pbOutput;
        }

        [SecurityCritical]
        internal static int GetPropertyAsDWord(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
        {
            bool flag;
            byte[] buffer = GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
            if (flag && (buffer != null))
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            return 0;
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static IntPtr GetPropertyAsIntPtr(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
        {
            int size = IntPtr.Size;
            IntPtr zero = IntPtr.Zero;
            ErrorCode code = UnsafeNativeMethods.NCryptGetProperty(ncryptObject, propertyName, out zero, IntPtr.Size, out size, propertyOptions);
            if (code == ErrorCode.NotFound)
            {
                return IntPtr.Zero;
            }
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return zero;
        }

        [SecurityCritical]
        internal static unsafe string GetPropertyAsString(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions)
        {
            bool flag;
            byte[] buffer = GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
            if (!flag || (buffer == null))
            {
                return null;
            }
            if (buffer.Length == 0)
            {
                return string.Empty;
            }
            fixed (byte* numRef = buffer)
            {
                return Marshal.PtrToStringUni(new IntPtr((void*) numRef));
            }
        }

        [SecurityCritical]
        internal static unsafe T GetPropertyAsStruct<T>(SafeNCryptHandle ncryptObject, string propertyName, CngPropertyOptions propertyOptions) where T: struct
        {
            bool flag;
            byte[] buffer = GetProperty(ncryptObject, propertyName, propertyOptions, out flag);
            if (!flag || (buffer == null))
            {
                return default(T);
            }
            fixed (byte* numRef = buffer)
            {
                return (T) Marshal.PtrToStructure(new IntPtr((void*) numRef), typeof(T));
            }
        }

        [SecurityCritical]
        internal static SafeNCryptKeyHandle ImportKey(SafeNCryptProviderHandle provider, byte[] keyBlob, string format)
        {
            SafeNCryptKeyHandle phKey = null;
            ErrorCode code = UnsafeNativeMethods.NCryptImportKey(provider, IntPtr.Zero, format, IntPtr.Zero, out phKey, keyBlob, keyBlob.Length, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return phKey;
        }

        [SecurityCritical]
        internal static SafeNCryptKeyHandle OpenKey(SafeNCryptProviderHandle provider, string name, CngKeyOpenOptions options)
        {
            SafeNCryptKeyHandle phKey = null;
            ErrorCode code = UnsafeNativeMethods.NCryptOpenKey(provider, out phKey, name, 0, options);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return phKey;
        }

        [SecurityCritical]
        internal static SafeNCryptProviderHandle OpenStorageProvider(string providerName)
        {
            SafeNCryptProviderHandle phProvider = null;
            ErrorCode code = UnsafeNativeMethods.NCryptOpenStorageProvider(out phProvider, providerName, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return phProvider;
        }

        private static byte[] ReverseBytes(byte[] buffer)
        {
            return ReverseBytes(buffer, 0, buffer.Length, false);
        }

        private static byte[] ReverseBytes(byte[] buffer, int offset, int count)
        {
            return ReverseBytes(buffer, offset, count, false);
        }

        private static byte[] ReverseBytes(byte[] buffer, int offset, int count, bool padWithZeroByte)
        {
            byte[] buffer2;
            if (padWithZeroByte)
            {
                buffer2 = new byte[count + 1];
            }
            else
            {
                buffer2 = new byte[count];
            }
            int num = (offset + count) - 1;
            for (int i = 0; i < count; i++)
            {
                buffer2[i] = buffer[num - i];
            }
            return buffer2;
        }

        [SecurityCritical]
        internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, int value, CngPropertyOptions propertyOptions)
        {
            SetProperty(ncryptObject, propertyName, BitConverter.GetBytes(value), propertyOptions);
        }

        [SecurityCritical]
        internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, string value, CngPropertyOptions propertyOptions)
        {
            ErrorCode code = UnsafeNativeMethods.NCryptSetProperty(ncryptObject, propertyName, value, (value.Length + 1) * 2, propertyOptions);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
        }

        [SecurityCritical]
        internal static void SetProperty(SafeNCryptHandle ncryptObject, string propertyName, byte[] value, CngPropertyOptions propertyOptions)
        {
            ErrorCode code = UnsafeNativeMethods.NCryptSetProperty(ncryptObject, propertyName, value, (value != null) ? value.Length : 0, propertyOptions);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
        }

        [SecurityCritical]
        internal static unsafe void SetProperty<T>(SafeNCryptHandle ncryptObject, string propertyName, T value, CngPropertyOptions propertyOptions) where T: struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            fixed (byte* numRef = buffer)
            {
                bool flag = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        Marshal.StructureToPtr(value, new IntPtr((void*) numRef), false);
                        flag = true;
                    }
                    SetProperty(ncryptObject, propertyName, buffer, propertyOptions);
                }
                finally
                {
                    if (flag)
                    {
                        Marshal.DestroyStructure(new IntPtr((void*) numRef), typeof(T));
                    }
                }
            }
        }

        [SecurityCritical]
        internal static byte[] SignHash(SafeNCryptKeyHandle key, byte[] hash)
        {
            int pcbResult = 0;
            ErrorCode code = UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, null, 0, out pcbResult, 0);
            if ((code != ErrorCode.Success) && (code != ErrorCode.BufferTooSmall))
            {
                throw new CryptographicException((int) code);
            }
            byte[] pbSignature = new byte[pcbResult];
            code = UnsafeNativeMethods.NCryptSignHash(key, IntPtr.Zero, hash, hash.Length, pbSignature, pbSignature.Length, out pcbResult, 0);
            if (code != ErrorCode.Success)
            {
                throw new CryptographicException((int) code);
            }
            return pbSignature;
        }

        internal static void UnpackEccPublicBlob(byte[] blob, out BigInteger x, out BigInteger y)
        {
            int count = BitConverter.ToInt32(blob, 4);
            x = new BigInteger(ReverseBytes(blob, 8, count, true));
            y = new BigInteger(ReverseBytes(blob, 8 + count, count, true));
        }

        [SecurityCritical]
        internal static bool VerifySignature(SafeNCryptKeyHandle key, byte[] hash, byte[] signature)
        {
            ErrorCode code = UnsafeNativeMethods.NCryptVerifySignature(key, IntPtr.Zero, hash, hash.Length, signature, signature.Length, 0);
            if ((code != ErrorCode.Success) && (code != ErrorCode.BadSignature))
            {
                throw new CryptographicException((int) code);
            }
            return (code == ErrorCode.Success);
        }

        internal static bool NCryptSupported
        {
            [SecurityCritical]
            get
            {
                if (!s_ncryptSupported.HasValue)
                {
                    using (Microsoft.Win32.SafeLibraryHandle handle = Microsoft.Win32.UnsafeNativeMethods.LoadLibraryEx("ncrypt", IntPtr.Zero, 0))
                    {
                        s_ncryptSupported = new bool?(!handle.IsInvalid);
                    }
                }
                return s_ncryptSupported.Value;
            }
        }

        internal enum BufferType
        {
            KdfHashAlgorithm,
            KdfSecretPrepend,
            KdfSecretAppend,
            KdfHmacKey,
            KdfTlsLabel,
            KdfTlsSeed
        }

        internal enum ErrorCode
        {
            BadSignature = -2146893818,
            BufferTooSmall = -2146893784,
            KeyDoesNotExist = -2146893802,
            NotFound = -2146893807,
            Success = 0
        }

        internal static class KeyPropertyName
        {
            internal const string Algorithm = "Algorithm Name";
            internal const string AlgorithmGroup = "Algorithm Group";
            internal const string ClrIsEphemeral = "CLR IsEphemeral";
            internal const string ExportPolicy = "Export Policy";
            internal const string KeyType = "Key Type";
            internal const string KeyUsage = "Key Usage";
            internal const string Length = "Length";
            internal const string Name = "Name";
            internal const string ParentWindowHandle = "HWND Handle";
            internal const string ProviderHandle = "Provider Handle";
            internal const string UIPolicy = "UI Policy";
            internal const string UniqueName = "Unique Name";
            internal const string UseContext = "Use Context";
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NCRYPT_UI_POLICY
        {
            public int dwVersion;
            public CngUIProtectionLevels dwFlags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCreationTitle;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszFriendlyName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszDescription;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NCryptBuffer
        {
            public int cbBuffer;
            public System.Security.Cryptography.NCryptNative.BufferType BufferType;
            public IntPtr pvBuffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NCryptBufferDesc
        {
            public int ulVersion;
            public int cBuffers;
            public IntPtr pBuffers;
        }

        internal static class ProviderPropertyName
        {
            internal const string Name = "Name";
        }

        [Flags]
        internal enum SecretAgreementFlags
        {
            None,
            UseSecretAsHmacKey
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class UnsafeNativeMethods
        {
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptCreatePersistedKey(SafeNCryptProviderHandle hProvider, out SafeNCryptKeyHandle phKey, string pszAlgId, string pszKeyName, int dwLegacyKeySpec, CngKeyCreationOptions dwFlags);
            [DllImport("ncrypt.dll")]
            internal static extern NCryptNative.ErrorCode NCryptDeleteKey(SafeNCryptKeyHandle hKey, int flags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptDeriveKey(SafeNCryptSecretHandle hSharedSecret, string pwszKDF, [In] ref NCryptNative.NCryptBufferDesc pParameterList, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbDerivedKey, int cbDerivedKey, out int pcbResult, NCryptNative.SecretAgreementFlags dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptExportKey(SafeNCryptKeyHandle hKey, IntPtr hExportKey, string pszBlobType, IntPtr pParameterList, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput, int cbOutput, out int pcbResult, int dwFlags);
            [DllImport("ncrypt.dll")]
            internal static extern NCryptNative.ErrorCode NCryptFinalizeKey(SafeNCryptKeyHandle hKey, int dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbOutput, int cbOutput, out int pcbResult, CngPropertyOptions dwFlags);
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptGetProperty(SafeNCryptHandle hObject, string pszProperty, out IntPtr pbOutput, int cbOutput, out int pcbResult, CngPropertyOptions dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptImportKey(SafeNCryptProviderHandle hProvider, IntPtr hImportKey, string pszBlobType, IntPtr pParameterList, out SafeNCryptKeyHandle phKey, [MarshalAs(UnmanagedType.LPArray)] byte[] pbData, int cbData, int dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptOpenKey(SafeNCryptProviderHandle hProvider, out SafeNCryptKeyHandle phKey, string pszKeyName, int dwLegacyKeySpec, CngKeyOpenOptions dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptOpenStorageProvider(out SafeNCryptProviderHandle phProvider, string pszProviderName, int dwFlags);
            [DllImport("ncrypt.dll")]
            internal static extern NCryptNative.ErrorCode NCryptSecretAgreement(SafeNCryptKeyHandle hPrivKey, SafeNCryptKeyHandle hPubKey, out SafeNCryptSecretHandle phSecret, int dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, [MarshalAs(UnmanagedType.LPArray)] byte[] pbInput, int cbInput, CngPropertyOptions dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, IntPtr pbInput, int cbInput, CngPropertyOptions dwFlags);
            [DllImport("ncrypt.dll", CharSet=CharSet.Unicode)]
            internal static extern NCryptNative.ErrorCode NCryptSetProperty(SafeNCryptHandle hObject, string pszProperty, string pbInput, int cbInput, CngPropertyOptions dwFlags);
            [DllImport("ncrypt.dll")]
            internal static extern NCryptNative.ErrorCode NCryptSignHash(SafeNCryptKeyHandle hKey, IntPtr pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] byte[] pbSignature, int cbSignature, out int pcbResult, int dwFlags);
            [DllImport("ncrypt.dll")]
            internal static extern NCryptNative.ErrorCode NCryptVerifySignature(SafeNCryptKeyHandle hKey, IntPtr pPaddingInfo, [MarshalAs(UnmanagedType.LPArray)] byte[] pbHashValue, int cbHashValue, [MarshalAs(UnmanagedType.LPArray)] byte[] pbSignature, int cbSignature, int dwFlags);
        }
    }
}

