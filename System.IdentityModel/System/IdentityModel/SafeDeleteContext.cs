namespace System.IdentityModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class SafeDeleteContext : SafeHandle
    {
        private SafeFreeCredentials _EffectiveCredential;
        internal SSPIHandle _handle;
        private static readonly byte[] dummyBytes = new byte[1];
        private const string dummyStr = " ";
        private const string SECURITY = "security.Dll";

        protected SafeDeleteContext() : base(IntPtr.Zero, true)
        {
            this._handle = new SSPIHandle();
        }

        internal static unsafe int AcceptSecurityContext(SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, SspiContextFlags inFlags, Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref SspiContextFlags outFlags)
        {
            if (inCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inCredentials");
            }
            SecurityBufferDescriptor inputBuffer = null;
            if (inSecBuffer != null)
            {
                inputBuffer = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers != null)
            {
                inputBuffer = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outputBuffer = new SecurityBufferDescriptor(1);
            bool flag = (inFlags & SspiContextFlags.AllocateMemory) != SspiContextFlags.Zero;
            int num = -1;
            SSPIHandle handle = new SSPIHandle();
            if (refContext != null)
            {
                handle = refContext._handle;
            }
            GCHandle[] handleArray = null;
            GCHandle handle2 = new GCHandle();
            SafeFreeContextBuffer handleTemplate = null;
            try
            {
                handle2 = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
                SecurityBufferStruct[] structArray = new SecurityBufferStruct[(inputBuffer == null) ? 1 : inputBuffer.Count];
                try
                {
                    SecurityBufferStruct[] structArray3;
                    if (((structArray3 = structArray) == null) || (structArray3.Length == 0))
                    {
                        ptrRef = null;
                        goto Label_00A9;
                    }
                    fixed (IntPtr* ptrRef = structArray3)
                    {
                    Label_00A9:
                        if (inputBuffer != null)
                        {
                            inputBuffer.UnmanagedPointer = (void*) ptrRef;
                            handleArray = new GCHandle[inputBuffer.Count];
                            for (int i = 0; i < inputBuffer.Count; i++)
                            {
                                SecurityBuffer buffer2 = (inSecBuffer != null) ? inSecBuffer : inSecBuffers[i];
                                if (buffer2 != null)
                                {
                                    structArray[i].count = buffer2.size;
                                    structArray[i].type = buffer2.type;
                                    if (buffer2.unmanagedToken != null)
                                    {
                                        structArray[i].token = buffer2.unmanagedToken.DangerousGetHandle();
                                    }
                                    else if ((buffer2.token == null) || (buffer2.token.Length == 0))
                                    {
                                        structArray[i].token = IntPtr.Zero;
                                    }
                                    else
                                    {
                                        handleArray[i] = GCHandle.Alloc(buffer2.token, GCHandleType.Pinned);
                                        structArray[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(buffer2.token, buffer2.offset);
                                    }
                                }
                            }
                        }
                        SecurityBufferStruct[] structArray2 = new SecurityBufferStruct[1];
                        try
                        {
                            SecurityBufferStruct[] structArray4;
                            if (((structArray4 = structArray2) == null) || (structArray4.Length == 0))
                            {
                                ptrRef2 = null;
                                goto Label_01CF;
                            }
                            fixed (IntPtr* ptrRef2 = structArray4)
                            {
                            Label_01CF:
                                outputBuffer.UnmanagedPointer = (void*) ptrRef2;
                                structArray2[0].count = outSecBuffer.size;
                                structArray2[0].type = outSecBuffer.type;
                                if ((outSecBuffer.token == null) || (outSecBuffer.token.Length == 0))
                                {
                                    structArray2[0].token = IntPtr.Zero;
                                }
                                else
                                {
                                    structArray2[0].token = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
                                }
                                if (flag)
                                {
                                    handleTemplate = SafeFreeContextBuffer.CreateEmptyHandle();
                                }
                                if ((refContext == null) || refContext.IsInvalid)
                                {
                                    refContext = new SafeDeleteContext();
                                }
                                num = MustRunAcceptSecurityContext(inCredentials, handle.IsZero ? null : ((void*) &handle), inputBuffer, inFlags, endianness, refContext, outputBuffer, ref outFlags, handleTemplate);
                                outSecBuffer.size = structArray2[0].count;
                                outSecBuffer.type = structArray2[0].type;
                                if (outSecBuffer.size > 0)
                                {
                                    outSecBuffer.token = DiagnosticUtility.Utility.AllocateByteArray(outSecBuffer.size);
                                    Marshal.Copy(structArray2[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                                    return num;
                                }
                                outSecBuffer.token = null;
                                return num;
                            }
                        }
                        finally
                        {
                            ptrRef2 = null;
                        }
                        return num;
                    }
                }
                finally
                {
                    ptrRef = null;
                }
            }
            finally
            {
                if (handleArray != null)
                {
                    for (int j = 0; j < handleArray.Length; j++)
                    {
                        if (handleArray[j].IsAllocated)
                        {
                            handleArray[j].Free();
                        }
                    }
                }
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
                if (handleTemplate != null)
                {
                    handleTemplate.Close();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int AcceptSecurityContext(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] SecurityBufferDescriptor inputBuffer, [In] SspiContextFlags inFlags, [In] Endianness endianness, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref SspiContextFlags attributes, out long timestamp);
        public static unsafe int DecryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            uint qualityOfProtection = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = DecryptMessage(ref context._handle, inputOutput, sequenceNumber, &qualityOfProtection);
                    context.DangerousRelease();
                }
            }
            if ((num == 0) && (qualityOfProtection == 0x80000001))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.IdentityModel.SR.GetString("SspiPayloadNotEncrypted")));
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int DecryptMessage(ref SSPIHandle contextHandle, [In, Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber, uint* qualityOfProtection);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SuppressUnmanagedCodeSecurity, DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int DeleteSecurityContext(ref SSPIHandle handlePtr);
        public static int EncryptMessage(SafeDeleteContext context, SecurityBufferDescriptor inputOutput, uint sequenceNumber)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = EncryptMessage(ref context._handle, 0, inputOutput, sequenceNumber);
                    context.DangerousRelease();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int EncryptMessage(ref SSPIHandle contextHandle, [In] uint qualityOfProtection, [In, Out] SecurityBufferDescriptor inputOutput, [In] uint sequenceNumber);
        internal int GetSecurityContextToken(out SafeCloseHandle safeHandle)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    base.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = QuerySecurityContextToken(ref this._handle, out safeHandle);
                    base.DangerousRelease();
                }
                else
                {
                    safeHandle = new SafeCloseHandle(IntPtr.Zero, false);
                }
            }
            return num;
        }

        public static int ImpersonateSecurityContext(SafeDeleteContext context)
        {
            int num = -2146893055;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                context.DangerousAddRef(ref success);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    context.DangerousRelease();
                    success = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (success)
                {
                    num = ImpersonateSecurityContext(ref context._handle);
                    context.DangerousRelease();
                }
            }
            return num;
        }

        [DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern int ImpersonateSecurityContext(ref SSPIHandle handlePtr);
        internal static unsafe int InitializeSecurityContext(SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, string targetName, SspiContextFlags inFlags, Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref SspiContextFlags outFlags)
        {
            if (inCredentials == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inCredentials");
            }
            SecurityBufferDescriptor inputBuffer = null;
            if (inSecBuffer != null)
            {
                inputBuffer = new SecurityBufferDescriptor(1);
            }
            else if (inSecBuffers != null)
            {
                inputBuffer = new SecurityBufferDescriptor(inSecBuffers.Length);
            }
            SecurityBufferDescriptor outputBuffer = new SecurityBufferDescriptor(1);
            bool flag = (inFlags & SspiContextFlags.AllocateMemory) != SspiContextFlags.Zero;
            int num = -1;
            SSPIHandle handle = new SSPIHandle();
            if (refContext != null)
            {
                handle = refContext._handle;
            }
            GCHandle[] handleArray = null;
            GCHandle handle2 = new GCHandle();
            SafeFreeContextBuffer handleTemplate = null;
            try
            {
                handle2 = GCHandle.Alloc(outSecBuffer.token, GCHandleType.Pinned);
                SecurityBufferStruct[] structArray = new SecurityBufferStruct[(inputBuffer == null) ? 1 : inputBuffer.Count];
                try
                {
                    SecurityBufferStruct[] structArray3;
                    if (((structArray3 = structArray) == null) || (structArray3.Length == 0))
                    {
                        ptrRef = null;
                        goto Label_00A9;
                    }
                    fixed (IntPtr* ptrRef = structArray3)
                    {
                    Label_00A9:
                        if (inputBuffer != null)
                        {
                            inputBuffer.UnmanagedPointer = (void*) ptrRef;
                            handleArray = new GCHandle[inputBuffer.Count];
                            for (int i = 0; i < inputBuffer.Count; i++)
                            {
                                SecurityBuffer buffer2 = (inSecBuffer != null) ? inSecBuffer : inSecBuffers[i];
                                if (buffer2 != null)
                                {
                                    structArray[i].count = buffer2.size;
                                    structArray[i].type = buffer2.type;
                                    if (buffer2.unmanagedToken != null)
                                    {
                                        structArray[i].token = buffer2.unmanagedToken.DangerousGetHandle();
                                    }
                                    else if ((buffer2.token == null) || (buffer2.token.Length == 0))
                                    {
                                        structArray[i].token = IntPtr.Zero;
                                    }
                                    else
                                    {
                                        handleArray[i] = GCHandle.Alloc(buffer2.token, GCHandleType.Pinned);
                                        structArray[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(buffer2.token, buffer2.offset);
                                    }
                                }
                            }
                        }
                        SecurityBufferStruct[] structArray2 = new SecurityBufferStruct[1];
                        try
                        {
                            SecurityBufferStruct[] structArray4;
                            if (((structArray4 = structArray2) == null) || (structArray4.Length == 0))
                            {
                                ptrRef2 = null;
                                goto Label_01CF;
                            }
                            fixed (IntPtr* ptrRef2 = structArray4)
                            {
                            Label_01CF:
                                outputBuffer.UnmanagedPointer = (void*) ptrRef2;
                                structArray2[0].count = outSecBuffer.size;
                                structArray2[0].type = outSecBuffer.type;
                                if ((outSecBuffer.token == null) || (outSecBuffer.token.Length == 0))
                                {
                                    structArray2[0].token = IntPtr.Zero;
                                }
                                else
                                {
                                    structArray2[0].token = Marshal.UnsafeAddrOfPinnedArrayElement(outSecBuffer.token, outSecBuffer.offset);
                                }
                                if (flag)
                                {
                                    handleTemplate = SafeFreeContextBuffer.CreateEmptyHandle();
                                }
                                if ((refContext == null) || refContext.IsInvalid)
                                {
                                    refContext = new SafeDeleteContext();
                                }
                                if ((targetName == null) || (targetName.Length == 0))
                                {
                                    targetName = " ";
                                }
                                fixed (char* str = ((char*) targetName))
                                {
                                    char* chPtr = str;
                                    num = MustRunInitializeSecurityContext(inCredentials, handle.IsZero ? null : ((void*) &handle), (targetName == " ") ? null : ((byte*) chPtr), inFlags, endianness, inputBuffer, refContext, outputBuffer, ref outFlags, handleTemplate);
                                }
                                outSecBuffer.size = structArray2[0].count;
                                outSecBuffer.type = structArray2[0].type;
                                if (outSecBuffer.size > 0)
                                {
                                    outSecBuffer.token = DiagnosticUtility.Utility.AllocateByteArray(outSecBuffer.size);
                                    Marshal.Copy(structArray2[0].token, outSecBuffer.token, 0, outSecBuffer.size);
                                    return num;
                                }
                                outSecBuffer.token = null;
                                return num;
                            }
                        }
                        finally
                        {
                            ptrRef2 = null;
                        }
                        return num;
                    }
                }
                finally
                {
                    ptrRef = null;
                }
            }
            finally
            {
                if (handleArray != null)
                {
                    for (int j = 0; j < handleArray.Length; j++)
                    {
                        if (handleArray[j].IsAllocated)
                        {
                            handleArray[j].Free();
                        }
                    }
                }
                if (handle2.IsAllocated)
                {
                    handle2.Free();
                }
                if (handleTemplate != null)
                {
                    handleTemplate.Close();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        internal static extern unsafe int InitializeSecurityContextW(ref SSPIHandle credentialHandle, [In] void* inContextPtr, [In] byte* targetName, [In] SspiContextFlags inFlags, [In] int reservedI, [In] Endianness endianness, [In] SecurityBufferDescriptor inputBuffer, [In] int reservedII, ref SSPIHandle outContextPtr, [In, Out] SecurityBufferDescriptor outputBuffer, [In, Out] ref SspiContextFlags attributes, out long timestamp);
        private static unsafe int MustRunAcceptSecurityContext(SafeFreeCredentials inCredentials, void* inContextPtr, SecurityBufferDescriptor inputBuffer, SspiContextFlags inFlags, Endianness endianness, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref SspiContextFlags outFlags, SafeFreeContextBuffer handleTemplate)
        {
            int num = -1;
            bool success = false;
            bool flag2 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                inCredentials.DangerousAddRef(ref success);
                outContext.DangerousAddRef(ref flag2);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    inCredentials.DangerousRelease();
                    success = false;
                }
                if (flag2)
                {
                    outContext.DangerousRelease();
                    flag2 = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (!success)
                {
                    inCredentials = null;
                }
                else if (success && flag2)
                {
                    long num2;
                    num = AcceptSecurityContext(ref inCredentials._handle, inContextPtr, inputBuffer, inFlags, endianness, ref outContext._handle, outputBuffer, ref outFlags, out num2);
                    if ((outContext._EffectiveCredential != inCredentials) && ((num & 0x80000000L) == 0L))
                    {
                        if (outContext._EffectiveCredential != null)
                        {
                            outContext._EffectiveCredential.DangerousRelease();
                        }
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }
                    outContext.DangerousRelease();
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(outputBuffer.UnmanagedPointer.token);
                        if (handleTemplate.IsInvalid)
                        {
                            handleTemplate.SetHandleAsInvalid();
                        }
                    }
                    if ((inContextPtr == null) && ((num & 0x80000000L) != 0L))
                    {
                        outContext._handle.SetToInvalid();
                    }
                }
            }
            return num;
        }

        private static unsafe int MustRunInitializeSecurityContext(SafeFreeCredentials inCredentials, void* inContextPtr, byte* targetName, SspiContextFlags inFlags, Endianness endianness, SecurityBufferDescriptor inputBuffer, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref SspiContextFlags attributes, SafeFreeContextBuffer handleTemplate)
        {
            int num = -1;
            bool success = false;
            bool flag2 = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                inCredentials.DangerousAddRef(ref success);
                outContext.DangerousAddRef(ref flag2);
            }
            catch (Exception exception)
            {
                if (success)
                {
                    inCredentials.DangerousRelease();
                    success = false;
                }
                if (flag2)
                {
                    outContext.DangerousRelease();
                    flag2 = false;
                }
                if (!(exception is ObjectDisposedException))
                {
                    throw;
                }
            }
            finally
            {
                if (!success)
                {
                    inCredentials = null;
                }
                else if (success && flag2)
                {
                    long num2;
                    num = InitializeSecurityContextW(ref inCredentials._handle, inContextPtr, targetName, inFlags, 0, endianness, inputBuffer, 0, ref outContext._handle, outputBuffer, ref attributes, out num2);
                    if ((outContext._EffectiveCredential != inCredentials) && ((num & 0x80000000L) == 0L))
                    {
                        if (outContext._EffectiveCredential != null)
                        {
                            outContext._EffectiveCredential.DangerousRelease();
                        }
                        outContext._EffectiveCredential = inCredentials;
                    }
                    else
                    {
                        inCredentials.DangerousRelease();
                    }
                    outContext.DangerousRelease();
                    if (handleTemplate != null)
                    {
                        handleTemplate.Set(outputBuffer.UnmanagedPointer.token);
                        if (handleTemplate.IsInvalid)
                        {
                            handleTemplate.SetHandleAsInvalid();
                        }
                    }
                }
                if ((inContextPtr == null) && ((num & 0x80000000L) != 0L))
                {
                    outContext._handle.SetToInvalid();
                }
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("security.Dll", SetLastError=true, ExactSpelling=true)]
        private static extern int QuerySecurityContextToken(ref SSPIHandle phContext, out SafeCloseHandle handle);
        protected override bool ReleaseHandle()
        {
            if (this._EffectiveCredential != null)
            {
                this._EffectiveCredential.DangerousRelease();
            }
            return (DeleteSecurityContext(ref this._handle) == 0);
        }

        public override bool IsInvalid
        {
            get
            {
                if (!base.IsClosed)
                {
                    return this._handle.IsZero;
                }
                return true;
            }
        }
    }
}

