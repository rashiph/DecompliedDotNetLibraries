namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Proxies;

    internal class Callback
    {
        private static ContextCallbackFunction _cb = new ContextCallbackFunction(Callback.CallbackFunction);
        private static ContextCallbackFunction _cbMarshal = new ContextCallbackFunction(Callback.MarshalCallback);
        private static int modopt(IsLong) modopt(CallConvStdcall) *(tagComCallData*) _pfn = ((int modopt(IsLong) modopt(CallConvStdcall) *(tagComCallData*)) Marshal.GetFunctionPointerForDelegate(_cb).ToPointer());
        private static int modopt(IsLong) modopt(CallConvStdcall) *(tagComCallData*) _pfnMarshal = ((int modopt(IsLong) modopt(CallConvStdcall) *(tagComCallData*)) Marshal.GetFunctionPointerForDelegate(_cbMarshal).ToPointer());

        private static unsafe int modopt(IsLong) CallbackFunction(tagComCallData* pData)
        {
            UserCallData data = null;
            uint num;
            bool flag = false;
            try
            {
                IntPtr pinned = new IntPtr(*((int*) (pData + 8)));
                data = UserCallData.Get(pinned);
                data.msg = ((IProxyInvoke) RemotingServices.GetRealProxy(data.otp)).LocalInvoke(data.msg);
            }
            catch (Exception exception)
            {
                flag = true;
                if (data != null)
                {
                    data.except = exception;
                }
            }
            catch
            {
                flag = true;
            }
            IMethodReturnMessage msg = data.msg as IMethodReturnMessage;
            if (((msg != null) && (msg.Exception != null)) || flag)
            {
                if ((data != null) && data.fIsAutoDone)
                {
                    IUnknown* pDestCtx = data.pDestCtx;
                    IObjectContext* contextPtr = null;
                    if (**(*(((int*) pDestCtx)))(pDestCtx, &IID_IObjectContext, &contextPtr) >= 0)
                    {
                        **(((int*) contextPtr))[20](contextPtr);
                        **(((int*) contextPtr))[8](contextPtr);
                    }
                }
                num = 0x80131500;
            }
            else
            {
                num = 0;
            }
            return (int modopt(IsLong)) num;
        }

        public unsafe IMessage DoCallback(object otp, IMessage msg, IntPtr ctx, [MarshalAs(UnmanagedType.U1)] bool fIsAutoDone, MemberInfo mb, [MarshalAs(UnmanagedType.U1)] bool bHasGit)
        {
            tagComCallData2 data2;
            Proxy.Init();
            IUnknown* unknownPtr = null;
            IContextCallback* callbackPtr = null;
            IMessage message = null;
            UserCallData data = null;
            *((int*) &data2) = 0;
            *((int*) (&data2 + 4)) = 0;
            *((int*) (&data2 + 8)) = 0;
            *((int*) (&data2 + 12)) = _pfn;
            try
            {
                RealProxy realProxy = RemotingServices.GetRealProxy(otp);
                if (bHasGit)
                {
                    unknownPtr = (IUnknown*) realProxy.GetCOMIUnknown(false).ToInt32();
                }
                IUnknown* unknownPtr2 = (IUnknown*) ctx.ToInt32();
                int modopt(IsLong) errorCode = **(*((int*) unknownPtr2))(unknownPtr2, &IID_IContextCallback, &callbackPtr);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                int comSlotForMethodInfo = fIsAutoDone ? 7 : 8;
                _GUID _guid = IID_IRemoteDispatch;
                Type reflectedType = mb.ReflectedType;
                if (reflectedType.IsInterface)
                {
                    Guid guid = Marshal.GenerateGuidForType(reflectedType);
                    memcpy(&_guid, ((int) &guid), 0x10);
                    comSlotForMethodInfo = Marshal.GetComSlotForMethodInfo(mb);
                }
                data = new UserCallData(otp, msg, ctx, fIsAutoDone, mb);
                *((int*) (&data2 + 8)) = data.Pin().ToInt32();
                int modopt(IsLong) num = **(((int*) callbackPtr))[12](callbackPtr, __unep@?FilteringCallbackFunction@Thunk@EnterpriseServices@System@@$$FYGJPAUtagComCallData@123@@Z, &data2, &_guid, comSlotForMethodInfo, unknownPtr);
                message = data.msg;
                object except = data.except;
                if (except != null)
                {
                    throw except;
                }
                if ((num < 0) && (num != -2146233088))
                {
                    Marshal.ThrowExceptionForHR(num);
                }
            }
            finally
            {
                if (*(((int*) (&data2 + 8))) != 0)
                {
                    IntPtr pinned = new IntPtr(*((void**) (&data2 + 8)));
                    data.Unpin(pinned);
                }
                if (unknownPtr != null)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
                if (callbackPtr != null)
                {
                    **(((int*) callbackPtr))[8](callbackPtr);
                }
            }
            return message;
        }

        private static unsafe int modopt(IsLong) MarshalCallback(tagComCallData* pData)
        {
            IntPtr pinned = new IntPtr(*((int*) (pData + 8)));
            UserMarshalData data = UserMarshalData.Get(pinned);
            uint modopt(IsLong) num2 = 0;
            IUnknown* unknownPtr = (IUnknown*) data.pUnk.ToInt32();
            int modopt(IsLong) num = CoGetMarshalSizeMax(&num2, &IID_IUnknown, unknownPtr, 3, null, 0);
            if (num >= 0)
            {
                num2 += 4;
                try
                {
                    data.buffer = new byte[num2];
                }
                catch (OutOfMemoryException)
                {
                    num = -2147024882;
                }
                if (num < 0)
                {
                    return num;
                }
                fixed (byte* numRef = data.buffer)
                {
                    try
                    {
                        num = MarshalInterface(numRef, num2, unknownPtr, 3, 0);
                    }
                    fault
                    {
                        numRef = null;
                    }
                }
            }
            return num;
        }

        public unsafe byte[] SwitchMarshal(IntPtr ctx, IntPtr pUnk)
        {
            tagComCallData2 data2;
            Proxy.Init();
            byte[] buffer = null;
            IUnknown* unknownPtr2 = (IUnknown*) pUnk.ToInt32();
            IContextCallback* callbackPtr = null;
            UserMarshalData data = null;
            *((int*) &data2) = 0;
            *((int*) (&data2 + 4)) = 0;
            *((int*) (&data2 + 8)) = 0;
            *((int*) (&data2 + 12)) = _pfnMarshal;
            try
            {
                IUnknown* unknownPtr = (IUnknown*) ctx.ToInt32();
                int modopt(IsLong) errorCode = **(*((int*) unknownPtr))(unknownPtr, &IID_IContextCallback, &callbackPtr);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                data = new UserMarshalData(pUnk);
                *((int*) (&data2 + 8)) = data.Pin().ToInt32();
                int modopt(IsLong) num = **(((int*) callbackPtr))[12](callbackPtr, __unep@?FilteringCallbackFunction@Thunk@EnterpriseServices@System@@$$FYGJPAUtagComCallData@123@@Z, &data2, &IID_IUnknown, 2, unknownPtr2);
                if (num < 0)
                {
                    Marshal.ThrowExceptionForHR(num);
                }
                buffer = data.buffer;
            }
            finally
            {
                if (*(((int*) (&data2 + 8))) != 0)
                {
                    IntPtr pinned = new IntPtr(*((void**) (&data2 + 8)));
                    data.Unpin(pinned);
                }
                if (callbackPtr != null)
                {
                    **(((int*) callbackPtr))[8](callbackPtr);
                }
            }
            return buffer;
        }
    }
}

