namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class ContextThunk
    {
        private ContextThunk()
        {
        }

        public static unsafe void DisableCommit()
        {
            IObjectContext* contextPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContext, (void**) &contextPtr);
            if ((context >= 0) && (null != contextPtr))
            {
                context = **(((int*) contextPtr))[0x1c](contextPtr);
                **(((int*) contextPtr))[8](contextPtr);
                if (context == 0)
                {
                    return;
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
        }

        public static unsafe void EnableCommit()
        {
            IObjectContext* contextPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContext, (void**) &contextPtr);
            if ((context >= 0) && (null != contextPtr))
            {
                context = **(((int*) contextPtr))[0x18](contextPtr);
                **(((int*) contextPtr))[8](contextPtr);
                if (context == 0)
                {
                    return;
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool GetDeactivateOnReturn()
        {
            IContextState* statePtr = null;
            int modopt(IsLong) context = GetContext(&IID_IContextState, (void**) &statePtr);
            if ((context >= 0) && (null != statePtr))
            {
                short num2;
                context = **(((int*) statePtr))[0x10](statePtr, &num2);
                **(((int*) statePtr))[8](statePtr);
                if (context >= 0)
                {
                    return (-1 == num2);
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
            return false;
        }

        public static unsafe int GetMyTransactionVote()
        {
            int num2;
            IContextState* statePtr = null;
            int modopt(IsLong) context = GetContext(&IID_IContextState, (void**) &statePtr);
            if ((context >= 0) && (null != statePtr))
            {
                context = **(((int*) statePtr))[0x18](statePtr, &num2);
                **(((int*) statePtr))[8](statePtr);
                if (context >= 0)
                {
                    return num2;
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
            return num2;
        }

        public static unsafe object GetTransaction()
        {
            object objectForIUnknown;
            IUnknown* unknownPtr = null;
            IObjectContextInfo* infoPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContextInfo, (void**) &infoPtr);
            if ((context >= 0) && (null != infoPtr))
            {
                context = **(((int*) infoPtr))[0x10](infoPtr, &unknownPtr);
                **(((int*) infoPtr))[8](infoPtr);
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            else if (context >= 0)
            {
                goto Label_004E;
            }
            Marshal.ThrowExceptionForHR(context);
        Label_004E:
            objectForIUnknown = null;
            if (unknownPtr != null)
            {
                try
                {
                    IntPtr pUnk = new IntPtr((void*) unknownPtr);
                    objectForIUnknown = Marshal.GetObjectForIUnknown(pUnk);
                }
                finally
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
            }
            return objectForIUnknown;
        }

        public static unsafe Guid GetTransactionId()
        {
            Guid guid2 = new Guid();
            IObjectContextInfo* infoPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContextInfo, (void**) &infoPtr);
            if ((context >= 0) && (null != infoPtr))
            {
                _GUID _guid;
                context = **(((int*) infoPtr))[20](infoPtr, &_guid);
                **(((int*) infoPtr))[8](infoPtr);
                if (context == 0)
                {
                    return new Guid(*((uint*) &_guid), *((ushort*) (&_guid + 4)), *((ushort*) (&_guid + 6)), *((byte*) (&_guid + 8)), *((byte*) (&_guid + 9)), *((byte*) (&_guid + 10)), *((byte*) (&_guid + 11)), *((byte*) (&_guid + 12)), *((byte*) (&_guid + 13)), *((byte*) (&_guid + 14)), *((byte*) (&_guid + 15)));
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
            return guid2;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool GetTransactionProxyOrTransaction(ref object ppTx, TxInfo pTxInfo)
        {
            IObjectContext* contextPtr = null;
            pTxInfo.isDtcTransaction = false;
            bool flag = false;
            ppTx = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContext, (void**) &contextPtr);
            if ((context < 0) || (null == contextPtr))
            {
                if (context == -2147467262)
                {
                    return flag;
                }
                goto Label_019C;
            }
            if (**(((int*) contextPtr))[0x20](contextPtr) != 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
                goto Label_0186;
            }
            IContextTransactionInfoPrivate* privatePtr = null;
            context = **(*((int*) contextPtr))(contextPtr, &_GUID_7d40fcc8_f81e_462e_bba1_8a99ebdc826c, &privatePtr);
            switch (context)
            {
                case 0:
                    try
                    {
                        IUnknown* unknownPtr = null;
                        context = **(((int*) privatePtr))[12](privatePtr, &unknownPtr);
                        if (context >= 0)
                        {
                            if (unknownPtr == null)
                            {
                                uint modopt(IsLong) num2;
                                int modopt(IsLong) num3;
                                pTxInfo.isDtcTransaction = false;
                                context = **(((int*) privatePtr))[20](privatePtr, &num3, &num2);
                                if (context >= 0)
                                {
                                    pTxInfo.IsolationLevel = num3;
                                    pTxInfo.timeout = num2;
                                }
                                break;
                            }
                            IUnknown* unknownPtr3 = null;
                            context = **(*((int*) unknownPtr))(unknownPtr, &_GUID_0fb15084_af41_11ce_bd2b_204c4f4f5020, &unknownPtr3);
                            switch (context)
                            {
                                case 0:
                                    pTxInfo.isDtcTransaction = true;
                                    **(((int*) unknownPtr3))[8](unknownPtr3);
                                    break;

                                case -2147467262:
                                    context = 0;
                                    break;
                            }
                            try
                            {
                                if (context >= 0)
                                {
                                    IntPtr pUnk = new IntPtr((void*) unknownPtr);
                                    ppTx = Marshal.GetObjectForIUnknown(pUnk);
                                }
                            }
                            finally
                            {
                                **(((int*) unknownPtr))[8](unknownPtr);
                            }
                        }
                        break;
                    }
                    finally
                    {
                        **(((int*) privatePtr))[8](privatePtr);
                    }
                    break;

                case -2147467262:
                {
                    IObjectContextInfo* infoPtr = null;
                    IUnknown* unknownPtr2 = null;
                    context = **(*((int*) contextPtr))(contextPtr, &IID_IObjectContextInfo, &infoPtr);
                    if (context >= 0)
                    {
                        try
                        {
                            context = **(((int*) infoPtr))[0x10](infoPtr, &unknownPtr2);
                            if (context >= 0)
                            {
                                pTxInfo.isDtcTransaction = true;
                                try
                                {
                                    IntPtr ptr = new IntPtr((void*) unknownPtr2);
                                    ppTx = Marshal.GetObjectForIUnknown(ptr);
                                }
                                finally
                                {
                                    **(((int*) unknownPtr2))[8](unknownPtr2);
                                }
                            }
                        }
                        finally
                        {
                            **(((int*) infoPtr))[8](infoPtr);
                        }
                    }
                    break;
                }
            }
        Label_0186:
            **(((int*) contextPtr))[8](contextPtr);
        Label_019C:
            if (context < 0)
            {
                Marshal.ThrowExceptionForHR(context);
            }
            return flag;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static bool IsDefaultContext()
        {
            byte num = (IsDefaultContext() != 0) ? ((byte) 1) : ((byte) 0);
            return (bool) num;
        }

        [return: MarshalAs(UnmanagedType.U1)]
        public static unsafe bool IsInTransaction()
        {
            IObjectContext* contextPtr = null;
            if ((GetContext(&IID_IObjectContext, (void**) &contextPtr) >= 0) && (null != contextPtr))
            {
                bool flag = **(((int*) contextPtr))[0x20](contextPtr) != 0;
                **(((int*) contextPtr))[8](contextPtr);
                return flag;
            }
            return false;
        }

        public static unsafe Guid RegisterTransactionProxy(object pTransactionProxy)
        {
            IContextTransactionInfoPrivate* privatePtr = null;
            int modopt(IsLong) context = GetContext(&_GUID_7d40fcc8_f81e_462e_bba1_8a99ebdc826c, (void**) &privatePtr);
            if (context >= 0)
            {
                try
                {
                    IUnknown* unknownPtr = (IUnknown*) Marshal.GetIUnknownForObject(pTransactionProxy).ToPointer();
                    ITransactionProxyPrivate* privatePtr2 = null;
                    context = **(*((int*) unknownPtr))(unknownPtr, &_GUID_02558374_df2e_4dae_bd6b_1d5c994f9bdc, &privatePtr2);
                    **(((int*) unknownPtr))[8](unknownPtr);
                    if (context >= 0)
                    {
                        _GUID _guid;
                        context = **(((int*) privatePtr))[0x10](privatePtr, privatePtr2, &_guid);
                        **(((int*) privatePtr2))[8](privatePtr2);
                        if (context >= 0)
                        {
                            return new Guid(*((uint*) &_guid), *((ushort*) (&_guid + 4)), *((ushort*) (&_guid + 6)), *((byte*) (&_guid + 8)), *((byte*) (&_guid + 9)), *((byte*) (&_guid + 10)), *((byte*) (&_guid + 11)), *((byte*) (&_guid + 12)), *((byte*) (&_guid + 13)), *((byte*) (&_guid + 14)), *((byte*) (&_guid + 15)));
                        }
                    }
                }
                finally
                {
                    **(((int*) privatePtr))[8](privatePtr);
                }
                if (context >= 0)
                {
                    goto Label_00CB;
                }
            }
            Marshal.ThrowExceptionForHR(context);
        Label_00CB:
            return Guid.Empty;
        }

        public static unsafe void SetAbort()
        {
            IObjectContext* contextPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContext, (void**) &contextPtr);
            if ((context >= 0) && (null != contextPtr))
            {
                context = **(((int*) contextPtr))[20](contextPtr);
                **(((int*) contextPtr))[8](contextPtr);
                if (context == 0)
                {
                    return;
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
        }

        public static unsafe void SetComplete()
        {
            IObjectContext* contextPtr = null;
            int modopt(IsLong) context = GetContext(&IID_IObjectContext, (void**) &contextPtr);
            if ((context >= 0) && (null != contextPtr))
            {
                context = **(((int*) contextPtr))[0x10](contextPtr);
                **(((int*) contextPtr))[8](contextPtr);
                if (context == 0)
                {
                    return;
                }
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            Marshal.ThrowExceptionForHR(context);
        }

        public static unsafe void SetDeactivateOnReturn([MarshalAs(UnmanagedType.U1)] bool deactivateOnReturn)
        {
            IContextState* statePtr = null;
            int modopt(IsLong) context = GetContext(&IID_IContextState, (void**) &statePtr);
            if ((context >= 0) && (null != statePtr))
            {
                short num2 = deactivateOnReturn ? ((short) (-1)) : ((short) 0);
                context = **(((int*) statePtr))[12](statePtr, num2);
                **(((int*) statePtr))[8](statePtr);
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            else if (context >= 0)
            {
                return;
            }
            Marshal.ThrowExceptionForHR(context);
        }

        public static unsafe void SetMyTransactionVote(int vote)
        {
            IContextState* statePtr = null;
            int modopt(IsLong) context = GetContext(&IID_IContextState, (void**) &statePtr);
            if ((context >= 0) && (null != statePtr))
            {
                context = **(((int*) statePtr))[20](statePtr, vote);
                **(((int*) statePtr))[8](statePtr);
            }
            if (context == -2147467262)
            {
                context = -2147164156;
            }
            else if (context >= 0)
            {
                return;
            }
            Marshal.ThrowExceptionForHR(context);
        }
    }
}

