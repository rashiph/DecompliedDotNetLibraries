namespace System.IdentityModel.Selectors
{
    using Microsoft.InfoCards;
    using Microsoft.InfoCards.Diagnostics;
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text.RegularExpressions;

    internal class CardSpaceShim
    {
        public CsV2CloseCryptoHandle m_csShimCloseCryptoHandle;
        public CsV2Decrypt m_csShimDecrypt;
        public CsV2Encrypt m_csShimEncrypt;
        public CsV2FreeToken m_csShimFreeToken;
        public CsV2GenerateDerivedKey m_csShimGenerateDerivedKey;
        public CsV2GetCryptoTransform m_csShimGetCryptoTransform;
        public CsV2GetKeyedHash m_csShimGetKeyedHash;
        public CsV2GetToken m_csShimGetToken;
        public CsV2HashCore m_csShimHashCore;
        public CsV2HashFinal m_csShimHashFinal;
        public CsV2ImportInformationCard m_csShimImportInformationCard;
        public CsV2ManageCardSpace m_csShimManageCardSpace;
        public CsV2SignHash m_csShimSignHash;
        public CsV2TransformBlock m_csShimTransformBlock;
        public CsV2TransformFinalBlock m_csShimTransformFinalBlock;
        public CsV2VerifyHash m_csShimVerifyHash;
        private System.IdentityModel.Selectors.SafeLibraryHandle m_implementationDll;
        private bool m_isInitialized;
        private object m_syncRoot = new object();
        private const string REDIRECT_DLL_CARDSPACE_V1 = "infocardapi";
        private const string REDIRECT_DLL_IMPLEMENTATION_VALUE = "ImplementationDLL";
        private const string REDIRECT_DLL_IMPLEMENTATION_VALUE_DEFAULT = "infocardapi2";
        private const string REDIRECT_DLL_REG_KEY = @"software\microsoft\cardspace\v1";

        private string GetCardSpaceImplementationDll()
        {
            string path = this.GetV2ImplementationDllPath();
            if (!File.Exists(path))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "infocardapi.dll");
                if (!File.Exists(path))
                {
                    throw InfoCardTrace.ThrowHelperError(new CardSpaceException(Microsoft.InfoCards.SR.GetString("ClientAPIServiceNotInstalledError")));
                }
            }
            return path;
        }

        private string GetV2ImplementationDllPath()
        {
            string str = string.Empty;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"software\microsoft\cardspace\v1"))
            {
                if (key != null)
                {
                    str = (string) key.GetValue("ImplementationDLL");
                    if (!string.IsNullOrEmpty(str))
                    {
                        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), str + ".dll");
                        if (!this.IsSafeFile(str) || !File.Exists(path))
                        {
                            str = string.Empty;
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(str))
            {
                str = "infocardapi2";
            }
            InfoCardTrace.Assert(!string.IsNullOrEmpty(str), "v2AndAboveImplementationDll should not be empty", new object[0]);
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), str + ".dll");
        }

        public void InitializeIfNecessary()
        {
            if (!this.m_isInitialized)
            {
                lock (this.m_syncRoot)
                {
                    if (!this.m_isInitialized)
                    {
                        string cardSpaceImplementationDll = this.GetCardSpaceImplementationDll();
                        this.m_implementationDll = System.IdentityModel.Selectors.SafeLibraryHandle.LoadLibraryW(cardSpaceImplementationDll);
                        if (this.m_implementationDll.IsInvalid)
                        {
                            throw System.IdentityModel.Selectors.NativeMethods.ThrowWin32ExceptionWithContext(new Win32Exception(), cardSpaceImplementationDll);
                        }
                        try
                        {
                            IntPtr procAddressWrapper = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "CloseCryptoHandle");
                            this.m_csShimCloseCryptoHandle = (CsV2CloseCryptoHandle) Marshal.GetDelegateForFunctionPointer(procAddressWrapper, typeof(CsV2CloseCryptoHandle));
                            IntPtr ptr = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "Decrypt");
                            this.m_csShimDecrypt = (CsV2Decrypt) Marshal.GetDelegateForFunctionPointer(ptr, typeof(CsV2Decrypt));
                            IntPtr ptr3 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "Encrypt");
                            this.m_csShimEncrypt = (CsV2Encrypt) Marshal.GetDelegateForFunctionPointer(ptr3, typeof(CsV2Encrypt));
                            IntPtr ptr4 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "FreeToken");
                            this.m_csShimFreeToken = (CsV2FreeToken) Marshal.GetDelegateForFunctionPointer(ptr4, typeof(CsV2FreeToken));
                            IntPtr ptr5 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "GenerateDerivedKey");
                            this.m_csShimGenerateDerivedKey = (CsV2GenerateDerivedKey) Marshal.GetDelegateForFunctionPointer(ptr5, typeof(CsV2GenerateDerivedKey));
                            IntPtr ptr6 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "GetCryptoTransform");
                            this.m_csShimGetCryptoTransform = (CsV2GetCryptoTransform) Marshal.GetDelegateForFunctionPointer(ptr6, typeof(CsV2GetCryptoTransform));
                            IntPtr ptr7 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "GetKeyedHash");
                            this.m_csShimGetKeyedHash = (CsV2GetKeyedHash) Marshal.GetDelegateForFunctionPointer(ptr7, typeof(CsV2GetKeyedHash));
                            IntPtr ptr8 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "GetToken");
                            this.m_csShimGetToken = (CsV2GetToken) Marshal.GetDelegateForFunctionPointer(ptr8, typeof(CsV2GetToken));
                            IntPtr ptr9 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "HashCore");
                            this.m_csShimHashCore = (CsV2HashCore) Marshal.GetDelegateForFunctionPointer(ptr9, typeof(CsV2HashCore));
                            IntPtr ptr10 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "HashFinal");
                            this.m_csShimHashFinal = (CsV2HashFinal) Marshal.GetDelegateForFunctionPointer(ptr10, typeof(CsV2HashFinal));
                            IntPtr ptr11 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "ImportInformationCard");
                            this.m_csShimImportInformationCard = (CsV2ImportInformationCard) Marshal.GetDelegateForFunctionPointer(ptr11, typeof(CsV2ImportInformationCard));
                            IntPtr ptr12 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "ManageCardSpace");
                            this.m_csShimManageCardSpace = (CsV2ManageCardSpace) Marshal.GetDelegateForFunctionPointer(ptr12, typeof(CsV2ManageCardSpace));
                            IntPtr ptr13 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "SignHash");
                            this.m_csShimSignHash = (CsV2SignHash) Marshal.GetDelegateForFunctionPointer(ptr13, typeof(CsV2SignHash));
                            IntPtr ptr14 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "TransformBlock");
                            this.m_csShimTransformBlock = (CsV2TransformBlock) Marshal.GetDelegateForFunctionPointer(ptr14, typeof(CsV2TransformBlock));
                            IntPtr ptr15 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "TransformFinalBlock");
                            this.m_csShimTransformFinalBlock = (CsV2TransformFinalBlock) Marshal.GetDelegateForFunctionPointer(ptr15, typeof(CsV2TransformFinalBlock));
                            IntPtr ptr16 = System.IdentityModel.Selectors.NativeMethods.GetProcAddressWrapper(this.m_implementationDll, "VerifyHash");
                            this.m_csShimVerifyHash = (CsV2VerifyHash) Marshal.GetDelegateForFunctionPointer(ptr16, typeof(CsV2VerifyHash));
                        }
                        catch (Win32Exception)
                        {
                            InfoCardTrace.Assert(!this.m_isInitialized, "If an exception occurred, we expect this to be false", new object[0]);
                            throw;
                        }
                        this.m_isInitialized = true;
                    }
                }
            }
        }

        private bool IsSafeFile(string fileName)
        {
            return Regex.IsMatch(fileName, "^[A-Za-z0-9]+$");
        }

        [SuppressUnmanagedCodeSecurity]
        internal delegate bool CsV2CloseCryptoHandle([In] IntPtr hKey);

        internal delegate int CsV2Decrypt(InternalRefCountedHandle nativeCryptoHandle, bool fOAEP, [MarshalAs(UnmanagedType.U4)] int cbInData, SafeHandle pInData, [MarshalAs(UnmanagedType.U4)] out int pcbOutData, out GlobalAllocSafeHandle pOutData);

        internal delegate int CsV2Encrypt(InternalRefCountedHandle nativeCryptoHandle, bool fOAEP, [MarshalAs(UnmanagedType.U4)] int cbInData, SafeHandle pInData, [MarshalAs(UnmanagedType.U4)] out int pcbOutData, out GlobalAllocSafeHandle pOutData);

        [SuppressUnmanagedCodeSecurity]
        internal delegate int CsV2FreeToken([In] IntPtr token);

        internal delegate int CsV2GenerateDerivedKey(InternalRefCountedHandle nativeCryptoHandle, int cbLabel, SafeHandle pLabel, int cbNonce, SafeHandle pNonce, int derivedKeyLength, int offset, [MarshalAs(UnmanagedType.LPWStr)] string derivationAlgUri, out int cbDerivedKey, out GlobalAllocSafeHandle pDerivedKey);

        internal delegate int CsV2GetCryptoTransform(InternalRefCountedHandle nativeCryptoHandle, int mode, int padding, int feedbackSize, int direction, int cbIV, SafeHandle pIV, out InternalRefCountedHandle nativeTransformHandle);

        internal delegate int CsV2GetKeyedHash(InternalRefCountedHandle nativeCryptoHandle, out InternalRefCountedHandle nativeHashHandle);

        internal delegate int CsV2GetToken(int cPolicyChain, SafeHandle pPolicyChain, out SafeTokenHandle securityToken, out InternalRefCountedHandle pCryptoHandle);

        internal delegate int CsV2HashCore(InternalRefCountedHandle nativeCryptoHandle, int cbInData, SafeHandle pInData);

        internal delegate int CsV2HashFinal(InternalRefCountedHandle nativeCryptoHandle, int cbInData, SafeHandle pInData, out int cbOutData, out GlobalAllocSafeHandle pOutData);

        internal delegate int CsV2ImportInformationCard([MarshalAs(UnmanagedType.LPWStr)] string nativeFileName);

        internal delegate int CsV2ManageCardSpace();

        internal delegate int CsV2SignHash(InternalRefCountedHandle nativeCryptoHandle, [MarshalAs(UnmanagedType.U4)] int cbHash, SafeHandle pInData, SafeHandle pHashAlgOid, [MarshalAs(UnmanagedType.U4)] out int pcbSig, out GlobalAllocSafeHandle pSig);

        internal delegate int CsV2TransformBlock(InternalRefCountedHandle nativeCryptoHandle, int cbInData, SafeHandle pInData, out int cbOutData, out GlobalAllocSafeHandle pOutData);

        internal delegate int CsV2TransformFinalBlock(InternalRefCountedHandle nativeCryptoHandle, int cbInData, SafeHandle pInData, out int cbOutData, out GlobalAllocSafeHandle pOutData);

        internal delegate int CsV2VerifyHash(InternalRefCountedHandle nativeCryptoHandle, [MarshalAs(UnmanagedType.U4)] int cbHash, SafeHandle pInData, SafeHandle pHashAlgOid, [MarshalAs(UnmanagedType.U4)] int pcbSig, SafeHandle pSig, out bool verified);
    }
}

