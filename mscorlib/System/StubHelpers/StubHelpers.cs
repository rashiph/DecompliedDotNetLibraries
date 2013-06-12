namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical, SuppressUnmanagedCodeSecurity, ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class StubHelpers
    {
        [ThreadStatic]
        private static CopyCtorStubDesc s_copyCtorStubDesc;

        [ForceTokenStabilization, SecurityCritical]
        internal static IntPtr AddToCleanupList(ref CleanupWorkList pCleanupWorkList, SafeHandle handle)
        {
            if (pCleanupWorkList == null)
            {
                pCleanupWorkList = new CleanupWorkList();
            }
            CleanupWorkListElement elem = new CleanupWorkListElement(handle);
            pCleanupWorkList.Add(elem);
            return SafeHandleAddRef(handle, ref elem.m_owned);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern object AllocateInternal(IntPtr typeHandle);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern uint CalcVaListSize(IntPtr va_list);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void CheckCollectedDelegateMDA(IntPtr pEntryThunk);
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries"), ForceTokenStabilization]
        internal static void CheckStringLength(int length)
        {
            CheckStringLength((uint) length);
        }

        [ForceTokenStabilization, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal static void CheckStringLength(uint length)
        {
            if (length > 0x7ffffff0)
            {
                throw new MarshalDirectiveException(Environment.GetResourceString("Marshaler_StringTooLong"));
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr CreateCustomMarshalerHelper(IntPtr pMD, int paramToken, IntPtr hndManagedType);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void DebuggerTraceCall(IntPtr pSecretParam);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void DecimalCanonicalizeInternal(ref decimal dec);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void DemandPermission(IntPtr pNMD);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical, ForceTokenStabilization]
        internal static void DestroyCleanupList(ref CleanupWorkList pCleanupWorkList)
        {
            if (pCleanupWorkList != null)
            {
                pCleanupWorkList.Destroy();
                pCleanupWorkList = null;
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern unsafe void FmtClassUpdateCLRInternal(object obj, byte* pNative);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern unsafe void FmtClassUpdateNativeInternal(object obj, byte* pNative, ref CleanupWorkList pCleanupWorkList);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetCLRToCOMTarget(IntPtr pUnk, IntPtr pCPCMD);
        [ForceTokenStabilization]
        internal static Exception GetCOMHRExceptionObject(int hr, IntPtr pCPCMD, object pThis)
        {
            Exception exception = InternalGetCOMHRExceptionObject(hr, pCPCMD, pThis);
            exception.InternalPreserveStackTrace();
            return exception;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetCOMIPFromRCW(object objSrc, IntPtr pCPCMD, out bool pfNeedsRelease);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetDelegateTarget(Delegate pThis);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetFinalStubTarget(IntPtr pStubArg, IntPtr pUnmngThis, int dwStubFlags);
        [ForceTokenStabilization]
        internal static Exception GetHRExceptionObject(int hr)
        {
            Exception hRExceptionObject = InternalGetHRExceptionObject(hr);
            hRExceptionObject.InternalPreserveStackTrace();
            return hRExceptionObject;
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetNDirectTarget(IntPtr pMD);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr GetStubContext();
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void InitDeclaringType(IntPtr pMD);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern Exception InternalGetCOMHRExceptionObject(int hr, IntPtr pCPCMD, object pThis);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern Exception InternalGetHRExceptionObject(int hr);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern bool IsQCall(IntPtr pMD);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern unsafe void LayoutDestroyNativeInternal(byte* pNative, IntPtr pMT);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void MarshalToManagedVaListInternal(IntPtr va_list, IntPtr pArgIterator);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void MarshalToUnmanagedVaListInternal(IntPtr va_list, uint vaListSize, IntPtr pArgIterator);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr ProfilerBeginTransitionCallback(IntPtr pSecretParam, IntPtr pThread, object pThis);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ProfilerEndTransitionCallback(IntPtr pMD, IntPtr pThread);
        [SecurityCritical, ForceTokenStabilization]
        internal static IntPtr SafeHandleAddRef(SafeHandle pHandle, ref bool success)
        {
            if (pHandle == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_SafeHandle"));
            }
            pHandle.DangerousAddRef(ref success);
            if (!success)
            {
                return IntPtr.Zero;
            }
            return pHandle.DangerousGetHandle();
        }

        [ForceTokenStabilization, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecurityCritical]
        internal static void SafeHandleRelease(SafeHandle pHandle)
        {
            if (pHandle == null)
            {
                throw new ArgumentNullException(Environment.GetResourceString("ArgumentNull_SafeHandle"));
            }
            try
            {
                pHandle.DangerousRelease();
            }
            catch (Exception exception)
            {
                Mda.ReportErrorSafeHandleRelease(exception);
            }
        }

        [ForceTokenStabilization]
        internal static void SetCopyCtorCookieChain(IntPtr pStubArg, IntPtr pUnmngThis, int dwStubFlags, IntPtr pCookie)
        {
            s_copyCtorStubDesc.m_pCookie = pCookie;
            s_copyCtorStubDesc.m_pTarget = GetFinalStubTarget(pStubArg, pUnmngThis, dwStubFlags);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void SetLastError();
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern unsafe int strlen(sbyte* ptr);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void StubRegisterRCW(object pThis, IntPtr pThread);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void StubUnregisterRCW(object pThis, IntPtr pThread);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ThrowDeferredException();
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ThrowInteropParamException(int resID, int paramIdx);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void TriggerGCForMDA();
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ValidateByref(IntPtr byref, IntPtr pMD, object pThis);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ValidateObject(object obj, IntPtr pMD, object pThis);
    }
}

