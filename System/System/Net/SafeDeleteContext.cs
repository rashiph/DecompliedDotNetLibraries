namespace System.Net
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal abstract class SafeDeleteContext : SafeHandle
    {
        protected SafeFreeCredentials _EffectiveCredential;
        internal SSPIHandle _handle;
        private static readonly byte[] dummyBytes = new byte[1];
        private const string dummyStr = " ";

        protected SafeDeleteContext() : base(IntPtr.Zero, true)
        {
            this._handle = new SSPIHandle();
        }

        internal static unsafe int AcceptSecurityContext(SecurDll dll, ref SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, ContextFlags inFlags, Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref ContextFlags outFlags)
        {
            if (inCredentials == null)
            {
                throw new ArgumentNullException("inCredentials");
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
            bool flag = (inFlags & ContextFlags.AllocateMemory) != ContextFlags.Zero;
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
                        goto Label_00A5;
                    }
                    fixed (IntPtr* ptrRef = structArray3)
                    {
                    Label_00A5:
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
                                goto Label_01CB;
                            }
                            fixed (IntPtr* ptrRef2 = structArray4)
                            {
                            Label_01CB:
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
                                    handleTemplate = SafeFreeContextBuffer.CreateEmptyHandle(dll);
                                }
                                switch (dll)
                                {
                                    case SecurDll.SECURITY:
                                        if ((refContext == null) || refContext.IsInvalid)
                                        {
                                            refContext = new SafeDeleteContext_SECURITY();
                                        }
                                        num = MustRunAcceptSecurityContext_SECURITY(ref inCredentials, handle.IsZero ? null : ((void*) &handle), inputBuffer, inFlags, endianness, refContext, outputBuffer, ref outFlags, handleTemplate);
                                        break;

                                    case SecurDll.SECUR32:
                                        if ((refContext == null) || refContext.IsInvalid)
                                        {
                                            refContext = new SafeDeleteContext_SECUR32();
                                        }
                                        num = MustRunAcceptSecurityContext_SECUR32(ref inCredentials, handle.IsZero ? null : ((void*) &handle), inputBuffer, inFlags, endianness, refContext, outputBuffer, ref outFlags, handleTemplate);
                                        break;

                                    case SecurDll.SCHANNEL:
                                        if ((refContext == null) || refContext.IsInvalid)
                                        {
                                            refContext = new SafeDeleteContext_SCHANNEL();
                                        }
                                        num = MustRunAcceptSecurityContext_SCHANNEL(ref inCredentials, handle.IsZero ? null : ((void*) &handle), inputBuffer, inFlags, endianness, refContext, outputBuffer, ref outFlags, handleTemplate);
                                        break;

                                    default:
                                        throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
                                }
                                outSecBuffer.size = structArray2[0].count;
                                outSecBuffer.type = structArray2[0].type;
                                if (outSecBuffer.size > 0)
                                {
                                    outSecBuffer.token = new byte[outSecBuffer.size];
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

        internal static unsafe int CompleteAuthToken(SecurDll dll, ref SafeDeleteContext refContext, SecurityBuffer[] inSecBuffers)
        {
            SecurityBufferStruct[] structArray2;
            SecurityBufferDescriptor inputBuffers = new SecurityBufferDescriptor(inSecBuffers.Length);
            int num = -2146893055;
            GCHandle[] handleArray = null;
            SecurityBufferStruct[] structArray = new SecurityBufferStruct[inputBuffers.Count];
            if (((structArray2 = structArray) != null) && (structArray2.Length != 0))
            {
                goto Label_002F;
            }
            fixed (IntPtr* ptrRef = null)
            {
                goto Label_0039;
            Label_002F:
                ptrRef = structArray2;
            Label_0039:
                inputBuffers.UnmanagedPointer = (void*) ptrRef;
                handleArray = new GCHandle[inputBuffers.Count];
                for (int i = 0; i < inputBuffers.Count; i++)
                {
                    SecurityBuffer buffer = inSecBuffers[i];
                    if (buffer != null)
                    {
                        structArray[i].count = buffer.size;
                        structArray[i].type = buffer.type;
                        if (buffer.unmanagedToken != null)
                        {
                            structArray[i].token = buffer.unmanagedToken.DangerousGetHandle();
                        }
                        else if ((buffer.token == null) || (buffer.token.Length == 0))
                        {
                            structArray[i].token = IntPtr.Zero;
                        }
                        else
                        {
                            handleArray[i] = GCHandle.Alloc(buffer.token, GCHandleType.Pinned);
                            structArray[i].token = Marshal.UnsafeAddrOfPinnedArrayElement(buffer.token, buffer.offset);
                        }
                    }
                }
                SSPIHandle handle = new SSPIHandle();
                if (refContext != null)
                {
                    handle = refContext._handle;
                }
                try
                {
                    if (dll == SecurDll.SECURITY)
                    {
                        if ((refContext == null) || refContext.IsInvalid)
                        {
                            refContext = new SafeDeleteContext_SECURITY();
                        }
                        bool success = false;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            try
                            {
                                refContext.DangerousAddRef(ref success);
                            }
                            catch (Exception exception)
                            {
                                if (success)
                                {
                                    refContext.DangerousRelease();
                                    success = false;
                                }
                                if (!(exception is ObjectDisposedException))
                                {
                                    throw;
                                }
                            }
                            goto Label_0201;
                        }
                        finally
                        {
                            if (success)
                            {
                                num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.CompleteAuthToken(handle.IsZero ? null : ((void*) &handle), inputBuffers);
                                refContext.DangerousRelease();
                            }
                        }
                    }
                    throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
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
                }
            }
        Label_0201:;
            return num;
        }

        internal static unsafe int InitializeSecurityContext(SecurDll dll, ref SafeFreeCredentials inCredentials, ref SafeDeleteContext refContext, string targetName, ContextFlags inFlags, Endianness endianness, SecurityBuffer inSecBuffer, SecurityBuffer[] inSecBuffers, SecurityBuffer outSecBuffer, ref ContextFlags outFlags)
        {
            if (inCredentials == null)
            {
                throw new ArgumentNullException("inCredentials");
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
            bool flag = (inFlags & ContextFlags.AllocateMemory) != ContextFlags.Zero;
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
                        goto Label_00A6;
                    }
                    fixed (IntPtr* ptrRef = structArray3)
                    {
                    Label_00A6:
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
                                goto Label_01CC;
                            }
                            fixed (IntPtr* ptrRef2 = structArray4)
                            {
                                ref byte pinned numRef;
                                ref byte pinned numRef2;
                            Label_01CC:
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
                                    handleTemplate = SafeFreeContextBuffer.CreateEmptyHandle(dll);
                                }
                                switch (dll)
                                {
                                    case SecurDll.SECURITY:
                                        if ((refContext == null) || refContext.IsInvalid)
                                        {
                                            refContext = new SafeDeleteContext_SECURITY();
                                        }
                                        if ((targetName == null) || (targetName.Length == 0))
                                        {
                                            targetName = " ";
                                        }
                                        fixed (char* str = ((char*) targetName))
                                        {
                                            char* chPtr = str;
                                            num = MustRunInitializeSecurityContext_SECURITY(ref inCredentials, handle.IsZero ? null : ((void*) &handle), (targetName == " ") ? null : ((byte*) chPtr), inFlags, endianness, inputBuffer, refContext, outputBuffer, ref outFlags, handleTemplate);
                                            goto Label_044B;
                                        }
                                        break;

                                    case SecurDll.SECUR32:
                                        break;

                                    case SecurDll.SCHANNEL:
                                        goto Label_0381;

                                    default:
                                        goto Label_0423;
                                }
                                if ((refContext == null) || refContext.IsInvalid)
                                {
                                    refContext = new SafeDeleteContext_SECUR32();
                                }
                                byte[] dummyBytes = SafeDeleteContext.dummyBytes;
                                if ((targetName != null) && (targetName.Length != 0))
                                {
                                    dummyBytes = new byte[targetName.Length + 2];
                                    Encoding.Default.GetBytes(targetName, 0, targetName.Length, dummyBytes, 0);
                                }
                                try
                                {
                                    byte[] buffer5;
                                    if (((buffer5 = dummyBytes) == null) || (buffer5.Length == 0))
                                    {
                                        numRef = null;
                                    }
                                    else
                                    {
                                        numRef = buffer5;
                                    }
                                    num = MustRunInitializeSecurityContext_SECUR32(ref inCredentials, handle.IsZero ? null : ((void*) &handle), (dummyBytes == SafeDeleteContext.dummyBytes) ? null : numRef, inFlags, endianness, inputBuffer, refContext, outputBuffer, ref outFlags, handleTemplate);
                                    goto Label_044B;
                                }
                                finally
                                {
                                    numRef = null;
                                }
                            Label_0381:
                                if ((refContext == null) || refContext.IsInvalid)
                                {
                                    refContext = new SafeDeleteContext_SCHANNEL();
                                }
                                byte[] bytes = SafeDeleteContext.dummyBytes;
                                if ((targetName != null) && (targetName.Length != 0))
                                {
                                    bytes = new byte[targetName.Length + 2];
                                    Encoding.Default.GetBytes(targetName, 0, targetName.Length, bytes, 0);
                                }
                                try
                                {
                                    byte[] buffer6;
                                    if (((buffer6 = bytes) == null) || (buffer6.Length == 0))
                                    {
                                        numRef2 = null;
                                    }
                                    else
                                    {
                                        numRef2 = buffer6;
                                    }
                                    num = MustRunInitializeSecurityContext_SCHANNEL(ref inCredentials, handle.IsZero ? null : ((void*) &handle), (bytes == SafeDeleteContext.dummyBytes) ? null : numRef2, inFlags, endianness, inputBuffer, refContext, outputBuffer, ref outFlags, handleTemplate);
                                    goto Label_044B;
                                }
                                finally
                                {
                                    numRef2 = null;
                                }
                            Label_0423:;
                                throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "SecurDll" }), "Dll");
                            Label_044B:
                                outSecBuffer.size = structArray2[0].count;
                                outSecBuffer.type = structArray2[0].type;
                                if (outSecBuffer.size > 0)
                                {
                                    outSecBuffer.token = new byte[outSecBuffer.size];
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

        private static unsafe int MustRunAcceptSecurityContext_SCHANNEL(ref SafeFreeCredentials inCredentials, void* inContextPtr, SecurityBufferDescriptor inputBuffer, ContextFlags inFlags, Endianness endianness, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags outFlags, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.AcceptSecurityContext(ref credentialHandle, inContextPtr, inputBuffer, inFlags, endianness, ref outContext._handle, outputBuffer, ref outFlags, out num2);
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

        private static unsafe int MustRunAcceptSecurityContext_SECUR32(ref SafeFreeCredentials inCredentials, void* inContextPtr, SecurityBufferDescriptor inputBuffer, ContextFlags inFlags, Endianness endianness, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags outFlags, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.AcceptSecurityContext(ref credentialHandle, inContextPtr, inputBuffer, inFlags, endianness, ref outContext._handle, outputBuffer, ref outFlags, out num2);
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

        private static unsafe int MustRunAcceptSecurityContext_SECURITY(ref SafeFreeCredentials inCredentials, void* inContextPtr, SecurityBufferDescriptor inputBuffer, ContextFlags inFlags, Endianness endianness, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags outFlags, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (!success)
                {
                    inCredentials = null;
                }
                else if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.AcceptSecurityContext(ref credentialHandle, inContextPtr, inputBuffer, inFlags, endianness, ref outContext._handle, outputBuffer, ref outFlags, out num2);
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

        private static unsafe int MustRunInitializeSecurityContext_SCHANNEL(ref SafeFreeCredentials inCredentials, void* inContextPtr, byte* targetName, ContextFlags inFlags, Endianness endianness, SecurityBufferDescriptor inputBuffer, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags attributes, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SCHANNEL.InitializeSecurityContextA(ref credentialHandle, inContextPtr, targetName, inFlags, 0, endianness, inputBuffer, 0, ref outContext._handle, outputBuffer, ref attributes, out num2);
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

        private static unsafe int MustRunInitializeSecurityContext_SECUR32(ref SafeFreeCredentials inCredentials, void* inContextPtr, byte* targetName, ContextFlags inFlags, Endianness endianness, SecurityBufferDescriptor inputBuffer, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags attributes, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECUR32.InitializeSecurityContextA(ref credentialHandle, inContextPtr, targetName, inFlags, 0, endianness, inputBuffer, 0, ref outContext._handle, outputBuffer, ref attributes, out num2);
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

        private static unsafe int MustRunInitializeSecurityContext_SECURITY(ref SafeFreeCredentials inCredentials, void* inContextPtr, byte* targetName, ContextFlags inFlags, Endianness endianness, SecurityBufferDescriptor inputBuffer, SafeDeleteContext outContext, SecurityBufferDescriptor outputBuffer, ref ContextFlags attributes, SafeFreeContextBuffer handleTemplate)
        {
            int num = -2146893055;
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
                SSPIHandle credentialHandle = inCredentials._handle;
                if (!success)
                {
                    inCredentials = null;
                }
                else if (success && flag2)
                {
                    long num2;
                    num = UnsafeNclNativeMethods.SafeNetHandles_SECURITY.InitializeSecurityContextW(ref credentialHandle, inContextPtr, targetName, inFlags, 0, endianness, inputBuffer, 0, ref outContext._handle, outputBuffer, ref attributes, out num2);
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

        public override string ToString()
        {
            return this._handle.ToString();
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

