namespace System.IdentityModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal static class SspiWrapper
    {
        private const int SECPKG_FLAG_NEGOTIABLE2 = 0x200000;
        private static SecurityPackageInfoClass[] securityPackages;

        internal static int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext refContext, SspiContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(credential, ref refContext, inFlags, datarep, inputBuffer, null, outputBuffer, ref outFlags);
        }

        internal static int AcceptSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext refContext, SspiContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.AcceptSecurityContext(credential, ref refContext, inFlags, datarep, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(string package, CredentialUse intent, ref AuthIdentityEx authdata)
        {
            SafeFreeCredentials outCredential = null;
            int error = SafeFreeCredentials.AcquireCredentialsHandle(package, intent, ref authdata, out outCredential);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return outCredential;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(string package, CredentialUse intent, SecureCredential scc)
        {
            SafeFreeCredentials outCredential = null;
            int error = SafeFreeCredentials.AcquireCredentialsHandle(package, intent, ref scc, out outCredential);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return outCredential;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(string package, CredentialUse intent, ref IntPtr ppAuthIdentity)
        {
            SafeFreeCredentials outCredential = null;
            int error = SafeFreeCredentials.AcquireCredentialsHandle(package, intent, ref ppAuthIdentity, out outCredential);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return outCredential;
        }

        public static SafeFreeCredentials AcquireDefaultCredential(string package, CredentialUse intent, params string[] additionalPackages)
        {
            SafeFreeCredentials outCredential = null;
            AuthIdentityEx authIdentity = new AuthIdentityEx(null, null, null, additionalPackages);
            int error = SafeFreeCredentials.AcquireDefaultCredential(package, intent, ref authIdentity, out outCredential);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return outCredential;
        }

        public static int DecryptMessage(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber, bool isGssBlob)
        {
            return EncryptDecryptHelper(context, input, sequenceNumber, false, isGssBlob);
        }

        public static unsafe int EncryptDecryptHelper(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber, bool encrypt, bool isGssBlob)
        {
            int num6;
            SecurityBufferStruct[] structArray2;
            SecurityBufferDescriptor inputOutput = new SecurityBufferDescriptor(input.Length);
            SecurityBufferStruct[] structArray = new SecurityBufferStruct[input.Length];
            byte[][] bufferArray = new byte[input.Length][];
            if (((structArray2 = structArray) == null) || (structArray2.Length == 0))
            {
                fixed (IntPtr* ptrRef = null)
                {
                }
            }
            inputOutput.UnmanagedPointer = (void*) ptrRef;
            GCHandle[] handleArray = new GCHandle[input.Length];
            try
            {
                int num2;
                for (int i = 0; i < input.Length; i++)
                {
                    SecurityBuffer buffer = input[i];
                    structArray[i].count = buffer.size;
                    structArray[i].type = buffer.type;
                    if ((buffer.token == null) || (buffer.token.Length == 0))
                    {
                        structArray[i].token = IntPtr.Zero;
                    }
                    else
                    {
                        handleArray[i] = GCHandle.Alloc(buffer.token, GCHandleType.Pinned);
                        structArray[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(buffer.token, buffer.offset);
                        bufferArray[i] = buffer.token;
                    }
                }
                if (encrypt)
                {
                    num2 = SafeDeleteContext.EncryptMessage(context, inputOutput, sequenceNumber);
                }
                else
                {
                    num2 = SafeDeleteContext.DecryptMessage(context, inputOutput, sequenceNumber);
                }
                for (int j = 0; j < input.Length; j++)
                {
                    SecurityBuffer buffer2 = input[j];
                    buffer2.size = structArray[j].count;
                    buffer2.type = structArray[j].type;
                    if (buffer2.size == 0)
                    {
                        buffer2.offset = 0;
                        buffer2.token = null;
                        continue;
                    }
                    if ((isGssBlob && !encrypt) && (buffer2.type == BufferType.Data))
                    {
                        buffer2.token = DiagnosticUtility.Utility.AllocateByteArray(buffer2.size);
                        Marshal.Copy(structArray[j].token, buffer2.token, 0, buffer2.size);
                        continue;
                    }
                    int index = 0;
                    while (index < input.Length)
                    {
                        if (bufferArray[index] != null)
                        {
                            byte* numPtr = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(bufferArray[index], 0);
                            if ((((void*) structArray[j].token) >= numPtr) && ((((void*) structArray[j].token) + buffer2.size) <= (numPtr + bufferArray[index].Length)))
                            {
                                buffer2.offset = (int) ((long) ((((void*) structArray[j].token) - numPtr) / 1));
                                buffer2.token = bufferArray[index];
                                break;
                            }
                        }
                        index++;
                    }
                    if (index >= input.Length)
                    {
                        buffer2.size = 0;
                        buffer2.offset = 0;
                        buffer2.token = null;
                    }
                    if ((buffer2.offset < 0) || (buffer2.offset > ((buffer2.token == null) ? 0 : buffer2.token.Length)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SspiWrapperEncryptDecryptAssert1", new object[] { buffer2.offset })));
                    }
                    if ((buffer2.size < 0) || (buffer2.size > ((buffer2.token == null) ? 0 : (buffer2.token.Length - buffer2.offset))))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SspiWrapperEncryptDecryptAssert2", new object[] { buffer2.size })));
                    }
                }
                num6 = num2;
            }
            finally
            {
                for (int k = 0; k < handleArray.Length; k++)
                {
                    if (handleArray[k].IsAllocated)
                    {
                        handleArray[k].Free();
                    }
                }
            }
            return num6;
        }

        public static int EncryptMessage(SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(context, input, sequenceNumber, true, false);
        }

        private static SecurityPackageInfoClass[] EnumerateSecurityPackages()
        {
            if (SecurityPackages == null)
            {
                int pkgnum = 0;
                SafeFreeContextBuffer pkgArray = null;
                try
                {
                    int error = SafeFreeContextBuffer.EnumeratePackages(out pkgnum, out pkgArray);
                    if (error != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    SecurityPackageInfoClass[] classArray = new SecurityPackageInfoClass[pkgnum];
                    for (int i = 0; i < pkgnum; i++)
                    {
                        classArray[i] = new SecurityPackageInfoClass(pkgArray, i);
                    }
                    SecurityPackages = classArray;
                }
                finally
                {
                    if (pkgArray != null)
                    {
                        pkgArray.Close();
                    }
                }
            }
            return SecurityPackages;
        }

        public static SecurityPackageInfoClass GetVerifyPackageInfo(string packageName)
        {
            SecurityPackageInfoClass[] classArray = EnumerateSecurityPackages();
            if (classArray != null)
            {
                for (int i = 0; i < classArray.Length; i++)
                {
                    if (string.Compare(classArray[i].Name, packageName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return classArray[i];
                    }
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("SSPIPackageNotSupported", new object[] { packageName })));
        }

        public static void ImpersonateSecurityContext(SafeDeleteContext context)
        {
            int error = SafeDeleteContext.ImpersonateSecurityContext(context);
            if (error != 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        internal static int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, SspiContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, inputBuffer, null, outputBuffer, ref outFlags);
        }

        internal static int InitializeSecurityContext(SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, SspiContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref SspiContextFlags outFlags)
        {
            return SafeDeleteContext.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, null, inputBuffers, outputBuffer, ref outFlags);
        }

        public static bool IsNegotiateExPackagePresent()
        {
            SecurityPackageInfoClass[] classArray = EnumerateSecurityPackages();
            if (classArray != null)
            {
                int num = 0x200000;
                for (int i = 0; i < classArray.Length; i++)
                {
                    if ((classArray[i].Capabilities & num) != 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsSspiPromptingNeeded(uint ErrorOrNtStatus)
        {
            return System.IdentityModel.NativeMethods.SspiIsPromptingNeeded(ErrorOrNtStatus);
        }

        public static unsafe object QueryContextAttributes(SafeDeleteContext securityContext, ContextAttribute contextAttribute)
        {
            int size = IntPtr.Size;
            Type handleType = null;
            switch (contextAttribute)
            {
                case ContextAttribute.Sizes:
                    size = SecSizes.SizeOf;
                    break;

                case ContextAttribute.Names:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;

                case ContextAttribute.Lifespan:
                    size = LifeSpan_Struct.Size;
                    break;

                case ContextAttribute.StreamSizes:
                    size = StreamSizes.SizeOf;
                    break;

                case ContextAttribute.SessionKey:
                    handleType = typeof(SafeFreeContextBuffer);
                    size = SecPkgContext_SessionKey.Size;
                    break;

                case ContextAttribute.PackageInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;

                case ContextAttribute.NegotiationInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    size = Marshal.SizeOf(typeof(NegotiationInfo));
                    break;

                case ContextAttribute.Flags:
                    break;

                case ContextAttribute.RemoteCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;

                case ContextAttribute.LocalCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;

                case ContextAttribute.ConnectionInfo:
                    size = Marshal.SizeOf(typeof(SslConnectionInfo));
                    break;

                default:
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("contextAttribute", (int) contextAttribute, typeof(ContextAttribute)));
            }
            SafeHandle refHandle = null;
            object obj2 = null;
            try
            {
                byte[] buffer = new byte[size];
                int error = QueryContextAttributes(securityContext, contextAttribute, buffer, handleType, out refHandle);
                if (error != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
                switch (contextAttribute)
                {
                    case ContextAttribute.Sizes:
                        break;

                    case ContextAttribute.Names:
                        return Marshal.PtrToStringUni(refHandle.DangerousGetHandle());

                    case ContextAttribute.Lifespan:
                        return new LifeSpan(buffer);

                    case ContextAttribute.DceInfo:
                    case (ContextAttribute.StreamSizes | ContextAttribute.Names):
                    case ContextAttribute.Authority:
                    case (ContextAttribute.Authority | ContextAttribute.Names):
                    case ((ContextAttribute) 8):
                    case (ContextAttribute.PackageInfo | ContextAttribute.Names):
                    case (ContextAttribute.NegotiationInfo | ContextAttribute.Names):
                        return obj2;

                    case ContextAttribute.StreamSizes:
                        return new StreamSizes(buffer);

                    case ContextAttribute.SessionKey:
                        try
                        {
                            byte[] buffer4;
                            if (((buffer4 = buffer) == null) || (buffer4.Length == 0))
                            {
                                fixed (IntPtr* ptrRef2 = null)
                                {
                                }
                            }
                            obj2 = new SecuritySessionKeyClass(refHandle, Marshal.ReadInt32(new IntPtr((void*) ptrRef2)));
                        }
                        finally
                        {
                            ptrRef2 = null;
                        }
                        return obj2;

                    case ContextAttribute.PackageInfo:
                        return new SecurityPackageInfoClass(refHandle, 0);

                    case ContextAttribute.NegotiationInfo:
                        try
                        {
                            byte[] buffer3;
                            if (((buffer3 = buffer) == null) || (buffer3.Length == 0))
                            {
                                fixed (IntPtr* ptrRef = null)
                                {
                                }
                            }
                            return new NegotiationInfoClass(refHandle, Marshal.ReadInt32(new IntPtr((void*) ptrRef), NegotiationInfo.NegotiationStateOffset));
                        }
                        finally
                        {
                            ptrRef = null;
                        }
                        goto Label_026A;

                    case ContextAttribute.Flags:
                        try
                        {
                            fixed (byte* numRef = buffer)
                            {
                                return Marshal.ReadInt32(new IntPtr((void*) numRef));
                            }
                        }
                        finally
                        {
                            numRef = null;
                        }
                        break;

                    case ContextAttribute.RemoteCertificate:
                    case ContextAttribute.LocalCertificate:
                        goto Label_026A;

                    case ContextAttribute.ConnectionInfo:
                        return new SslConnectionInfo(buffer);

                    default:
                        return obj2;
                }
                return new SecSizes(buffer);
            Label_026A:
                obj2 = refHandle;
                refHandle = null;
                return obj2;
            }
            finally
            {
                if (refHandle != null)
                {
                    refHandle.Close();
                }
            }
            return obj2;
        }

        private static unsafe int QueryContextAttributes(SafeDeleteContext phContext, ContextAttribute attribute, byte[] buffer, Type handleType, out SafeHandle refHandle)
        {
            refHandle = null;
            if (handleType != null)
            {
                if (handleType != typeof(SafeFreeContextBuffer))
                {
                    if (handleType != typeof(SafeFreeCertContext))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("handleType", System.IdentityModel.SR.GetString("ValueMustBeOf2Types", new object[] { typeof(SafeFreeContextBuffer).ToString(), typeof(SafeFreeCertContext).ToString() })));
                    }
                    refHandle = new SafeFreeCertContext();
                }
                else
                {
                    refHandle = SafeFreeContextBuffer.CreateEmptyHandle();
                }
            }
            fixed (byte* numRef = buffer)
            {
                return SafeFreeContextBuffer.QueryContextAttributes(phContext, attribute, numRef, refHandle);
            }
        }

        public static int QuerySecurityContextToken(SafeDeleteContext context, out SafeCloseHandle token)
        {
            return context.GetSecurityContextToken(out token);
        }

        public static int QuerySpecifiedTarget(SafeDeleteContext securityContext, out string specifiedTarget)
        {
            int num2;
            int size = IntPtr.Size;
            Type handleType = typeof(SafeFreeContextBuffer);
            SafeHandle refHandle = null;
            specifiedTarget = null;
            try
            {
                byte[] buffer = new byte[size];
                num2 = QueryContextAttributes(securityContext, ContextAttribute.SpecifiedTarget, buffer, handleType, out refHandle);
                if (num2 != 0)
                {
                    return num2;
                }
                specifiedTarget = Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
            }
            finally
            {
                if (refHandle != null)
                {
                    refHandle.Close();
                }
            }
            return num2;
        }

        public static uint SspiPromptForCredential(string targetName, string packageName, out IntPtr ppAuthIdentity, ref bool saveCredentials)
        {
            CREDUI_INFO pUiInfo = new CREDUI_INFO {
                cbSize = Marshal.SizeOf(typeof(CREDUI_INFO)),
                pszCaptionText = System.IdentityModel.SR.GetString("SspiLoginPromptHeaderMessage"),
                pszMessageText = ""
            };
            return System.IdentityModel.NativeMethods.SspiPromptForCredentials(targetName, ref pUiInfo, 0, packageName, IntPtr.Zero, out ppAuthIdentity, ref saveCredentials, 0);
        }

        public static SecurityPackageInfoClass[] SecurityPackages
        {
            get
            {
                return securityPackages;
            }
            set
            {
                securityPackages = value;
            }
        }
    }
}

