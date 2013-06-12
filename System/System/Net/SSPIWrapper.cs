namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class SSPIWrapper
    {
        internal static int AcceptSecurityContext(SSPIInterface SecModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, ContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "AcceptSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", inFlags = ", inFlags, ")" }));
            }
            int num = SecModule.AcceptSecurityContext(ref credential, ref context, inputBuffer, inFlags, datarep, outputBuffer, ref outFlags);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffer", new object[] { "AcceptSecurityContext", (inputBuffer == null) ? 0 : inputBuffer.size, outputBuffer.size, (SecurityStatus) num }));
            }
            return num;
        }

        internal static int AcceptSecurityContext(SSPIInterface SecModule, SafeFreeCredentials credential, ref SafeDeleteContext context, ContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "AcceptSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", inFlags = ", inFlags, ")" }));
            }
            int num = SecModule.AcceptSecurityContext(credential, ref context, inputBuffers, inFlags, datarep, outputBuffer, ref outFlags);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffers", new object[] { "AcceptSecurityContext", (inputBuffers == null) ? 0 : inputBuffers.Length, outputBuffer.size, (SecurityStatus) num }));
            }
            return num;
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface SecModule, string package, CredentialUse intent, ref AuthIdentity authdata)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "AcquireCredentialsHandle(package  = ", package, ", intent   = ", intent, ", authdata = ", (AuthIdentity) authdata, ")" }));
            }
            SafeFreeCredentials outCredential = null;
            int error = SecModule.AcquireCredentialsHandle(package, intent, ref authdata, out outCredential);
            if (error == 0)
            {
                return outCredential;
            }
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", new object[] { "AcquireCredentialsHandle()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", new object[] { error }) }));
            }
            throw new Win32Exception(error);
        }

        public static SafeFreeCredentials AcquireCredentialsHandle(SSPIInterface SecModule, string package, CredentialUse intent, SecureCredential scc)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "AcquireCredentialsHandle(package = ", package, ", intent  = ", intent, ", scc     = ", scc, ")" }));
            }
            SafeFreeCredentials outCredential = null;
            int error = SecModule.AcquireCredentialsHandle(package, intent, ref scc, out outCredential);
            if (error == 0)
            {
                return outCredential;
            }
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", new object[] { "AcquireCredentialsHandle()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", new object[] { error }) }));
            }
            throw new Win32Exception(error);
        }

        public static SafeFreeCredentials AcquireDefaultCredential(SSPIInterface SecModule, string package, CredentialUse intent)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "AcquireDefaultCredential(package = ", package, ", intent  = ", intent, ")" }));
            }
            SafeFreeCredentials outCredential = null;
            int error = SecModule.AcquireDefaultCredential(package, intent, out outCredential);
            if (error == 0)
            {
                return outCredential;
            }
            if (Logging.On)
            {
                Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", new object[] { "AcquireDefaultCredential()", string.Format(CultureInfo.CurrentCulture, "0X{0:X}", new object[] { error }) }));
            }
            throw new Win32Exception(error);
        }

        internal static int CompleteAuthToken(SSPIInterface SecModule, ref SafeDeleteContext context, SecurityBuffer[] inputBuffers)
        {
            int num = SecModule.CompleteAuthToken(ref context, inputBuffers);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_operation_returned_something", new object[] { "CompleteAuthToken()", (SecurityStatus) num }));
            }
            return num;
        }

        public static int DecryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(OP.Decrypt, secModule, context, input, sequenceNumber);
        }

        private static unsafe int EncryptDecryptHelper(OP op, SSPIInterface SecModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            SecurityBufferDescriptor inputOutput = new SecurityBufferDescriptor(input.Length);
            SecurityBufferStruct[] structArray = new SecurityBufferStruct[input.Length];
            fixed (SecurityBufferStruct* structRef = structArray)
            {
                int num6;
                inputOutput.UnmanagedPointer = (void*) structRef;
                GCHandle[] handleArray = new GCHandle[input.Length];
                byte[][] bufferArray = new byte[input.Length][];
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
                    switch (op)
                    {
                        case OP.Encrypt:
                            num2 = SecModule.EncryptMessage(context, inputOutput, sequenceNumber);
                            break;

                        case OP.Decrypt:
                            num2 = SecModule.DecryptMessage(context, inputOutput, sequenceNumber);
                            break;

                        case OP.MakeSignature:
                            num2 = SecModule.MakeSignature(context, inputOutput, sequenceNumber);
                            break;

                        case OP.VerifySignature:
                            num2 = SecModule.VerifySignature(context, inputOutput, sequenceNumber);
                            break;

                        default:
                            throw ExceptionHelper.MethodNotImplementedException;
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
                        }
                        else
                        {
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
                        }
                    }
                    if ((num2 != 0) && Logging.On)
                    {
                        if (num2 == 0x90321)
                        {
                            Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_returned_something", new object[] { op, "SEC_I_RENEGOTIATE" }));
                        }
                        else
                        {
                            Logging.PrintError(Logging.Web, SR.GetString("net_log_operation_failed_with_error", new object[] { op, string.Format(CultureInfo.CurrentCulture, "0X{0:X}", new object[] { num2 }) }));
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
        }

        public static int EncryptMessage(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(OP.Encrypt, secModule, context, input, sequenceNumber);
        }

        internal static SecurityPackageInfoClass[] EnumerateSecurityPackages(SSPIInterface SecModule)
        {
            if (SecModule.SecurityPackages == null)
            {
                lock (SecModule)
                {
                    if (SecModule.SecurityPackages == null)
                    {
                        int pkgnum = 0;
                        SafeFreeContextBuffer pkgArray = null;
                        try
                        {
                            int error = SecModule.EnumerateSecurityPackages(out pkgnum, out pkgArray);
                            if (error != 0)
                            {
                                throw new Win32Exception(error);
                            }
                            SecurityPackageInfoClass[] classArray = new SecurityPackageInfoClass[pkgnum];
                            if (Logging.On)
                            {
                                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_enumerating_security_packages"));
                            }
                            for (int i = 0; i < pkgnum; i++)
                            {
                                classArray[i] = new SecurityPackageInfoClass(pkgArray, i);
                                if (Logging.On)
                                {
                                    Logging.PrintInfo(Logging.Web, "    " + classArray[i].Name);
                                }
                            }
                            SecModule.SecurityPackages = classArray;
                        }
                        finally
                        {
                            if (pkgArray != null)
                            {
                                pkgArray.Close();
                            }
                        }
                    }
                }
            }
            return SecModule.SecurityPackages;
        }

        public static string ErrorDescription(int errorCode)
        {
            if (errorCode == -1)
            {
                return "An exception when invoking Win32 API";
            }
            switch (((SecurityStatus) errorCode))
            {
                case SecurityStatus.InvalidHandle:
                    return "Invalid handle";

                case SecurityStatus.TargetUnknown:
                    return "Target unknown";

                case SecurityStatus.PackageNotFound:
                    return "Package not found";

                case SecurityStatus.InvalidToken:
                    return "Invalid token";

                case SecurityStatus.MessageAltered:
                    return "Message altered";

                case SecurityStatus.BufferNotEnough:
                    return "Buffer not enough";

                case SecurityStatus.WrongPrincipal:
                    return "Wrong principal";

                case SecurityStatus.UntrustedRoot:
                    return "Untrusted root";

                case SecurityStatus.ContinueNeeded:
                    return "Continue needed";

                case SecurityStatus.IncompleteMessage:
                    return "Message incomplete";
            }
            return ("0x" + errorCode.ToString("x", NumberFormatInfo.InvariantInfo));
        }

        internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName)
        {
            return GetVerifyPackageInfo(secModule, packageName, false);
        }

        internal static SecurityPackageInfoClass GetVerifyPackageInfo(SSPIInterface secModule, string packageName, bool throwIfMissing)
        {
            SecurityPackageInfoClass[] classArray = EnumerateSecurityPackages(secModule);
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
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_package_not_found", new object[] { packageName }));
            }
            if (throwIfMissing)
            {
                throw new NotSupportedException(SR.GetString("net_securitypackagesupport"));
            }
            return null;
        }

        internal static int InitializeSecurityContext(SSPIInterface SecModule, ref SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness datarep, SecurityBuffer inputBuffer, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "InitializeSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", targetName = ", targetName, ", inFlags = ", inFlags, ")" }));
            }
            int num = SecModule.InitializeSecurityContext(ref credential, ref context, targetName, inFlags, datarep, inputBuffer, outputBuffer, ref outFlags);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffer", new object[] { "InitializeSecurityContext", (inputBuffer == null) ? 0 : inputBuffer.size, outputBuffer.size, (SecurityStatus) num }));
            }
            return num;
        }

        internal static int InitializeSecurityContext(SSPIInterface SecModule, SafeFreeCredentials credential, ref SafeDeleteContext context, string targetName, ContextFlags inFlags, Endianness datarep, SecurityBuffer[] inputBuffers, SecurityBuffer outputBuffer, ref ContextFlags outFlags)
        {
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, string.Concat(new object[] { "InitializeSecurityContext(credential = ", credential.ToString(), ", context = ", ValidationHelper.ToString(context), ", targetName = ", targetName, ", inFlags = ", inFlags, ")" }));
            }
            int num = SecModule.InitializeSecurityContext(credential, ref context, targetName, inFlags, datarep, inputBuffers, outputBuffer, ref outFlags);
            if (Logging.On)
            {
                Logging.PrintInfo(Logging.Web, SR.GetString("net_log_sspi_security_context_input_buffers", new object[] { "InitializeSecurityContext", (inputBuffers == null) ? 0 : inputBuffers.Length, outputBuffer.size, (SecurityStatus) num }));
            }
            return num;
        }

        internal static int MakeSignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(OP.MakeSignature, secModule, context, input, sequenceNumber);
        }

        public static object QueryContextAttributes(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute)
        {
            int num;
            return QueryContextAttributes(SecModule, securityContext, contextAttribute, out num);
        }

        public static unsafe object QueryContextAttributes(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute, out int errorCode)
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

                case ContextAttribute.StreamSizes:
                    size = StreamSizes.SizeOf;
                    break;

                case ContextAttribute.PackageInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;

                case ContextAttribute.NegotiationInfo:
                    handleType = typeof(SafeFreeContextBuffer);
                    size = Marshal.SizeOf(typeof(NegotiationInfo));
                    break;

                case ContextAttribute.RemoteCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;

                case ContextAttribute.LocalCertificate:
                    handleType = typeof(SafeFreeCertContext);
                    break;

                case ContextAttribute.ClientSpecifiedSpn:
                    handleType = typeof(SafeFreeContextBuffer);
                    break;

                case ContextAttribute.IssuerListInfoEx:
                    size = Marshal.SizeOf(typeof(IssuerListInfoEx));
                    handleType = typeof(SafeFreeContextBuffer);
                    break;

                case ContextAttribute.ConnectionInfo:
                    size = Marshal.SizeOf(typeof(SslConnectionInfo));
                    break;

                default:
                    throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "ContextAttribute" }), "contextAttribute");
            }
            SafeHandle refHandle = null;
            object obj2 = null;
            try
            {
                byte[] buffer = new byte[size];
                errorCode = SecModule.QueryContextAttributes(securityContext, contextAttribute, buffer, handleType, out refHandle);
                if (errorCode != 0)
                {
                    return null;
                }
                ContextAttribute attribute2 = contextAttribute;
                if (attribute2 <= ContextAttribute.NegotiationInfo)
                {
                    switch (attribute2)
                    {
                        case ContextAttribute.Sizes:
                            return new SecSizes(buffer);

                        case ContextAttribute.Names:
                            if (!ComNetOS.IsWin9x)
                            {
                                return Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
                            }
                            return Marshal.PtrToStringAnsi(refHandle.DangerousGetHandle());

                        case ContextAttribute.Lifespan:
                        case ContextAttribute.DceInfo:
                            return obj2;

                        case ContextAttribute.StreamSizes:
                            return new StreamSizes(buffer);

                        case ContextAttribute.PackageInfo:
                            return new SecurityPackageInfoClass(refHandle, 0);

                        case (ContextAttribute.PackageInfo | ContextAttribute.Names):
                            return obj2;

                        case ContextAttribute.NegotiationInfo:
                            goto Label_0229;
                    }
                    return obj2;
                }
                switch (attribute2)
                {
                    case ContextAttribute.RemoteCertificate:
                    case ContextAttribute.LocalCertificate:
                        obj2 = refHandle;
                        refHandle = null;
                        return obj2;

                    case ContextAttribute.ClientSpecifiedSpn:
                        goto Label_0266;

                    case ContextAttribute.IssuerListInfoEx:
                        obj2 = new IssuerListInfoEx(refHandle, buffer);
                        refHandle = null;
                        return obj2;

                    case ContextAttribute.ConnectionInfo:
                        return new SslConnectionInfo(buffer);

                    default:
                        return obj2;
                }
            Label_0229:
                try
                {
                    byte[] buffer2;
                    if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                    {
                        fixed (IntPtr* ptrRef = null)
                        {
                        }
                    }
                    return new NegotiationInfoClass(refHandle, Marshal.ReadInt32(new IntPtr((void*) ptrRef), NegotiationInfo.NegotiationStateOffest));
                }
                finally
                {
                    ptrRef = null;
                }
            Label_0266:
                return Marshal.PtrToStringUni(refHandle.DangerousGetHandle());
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

        public static SafeFreeContextBufferChannelBinding QueryContextChannelBinding(SSPIInterface SecModule, SafeDeleteContext securityContext, ContextAttribute contextAttribute)
        {
            SafeFreeContextBufferChannelBinding binding;
            if (SecModule.QueryContextChannelBinding(securityContext, contextAttribute, out binding) != 0)
            {
                return null;
            }
            return binding;
        }

        public static int QuerySecurityContextToken(SSPIInterface SecModule, SafeDeleteContext context, out SafeCloseHandle token)
        {
            return SecModule.QuerySecurityContextToken(context, out token);
        }

        public static int VerifySignature(SSPIInterface secModule, SafeDeleteContext context, SecurityBuffer[] input, uint sequenceNumber)
        {
            return EncryptDecryptHelper(OP.VerifySignature, secModule, context, input, sequenceNumber);
        }

        private enum OP
        {
            Decrypt = 2,
            Encrypt = 1,
            MakeSignature = 3,
            VerifySignature = 4
        }
    }
}

