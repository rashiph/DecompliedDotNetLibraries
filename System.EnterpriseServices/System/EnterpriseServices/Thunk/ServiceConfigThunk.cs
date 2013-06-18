namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;

    internal class ServiceConfigThunk
    {
        private IntPtr m_pTrackerAppName;
        private IntPtr m_pTrackerCtxName;
        private unsafe IUnknown* m_pUnkSC;
        private int m_tracker;

        public unsafe ServiceConfigThunk()
        {
            IUnknown* unknownPtr;
            this.m_pUnkSC = null;
            this.m_tracker = 0;
            int modopt(IsLong) errorCode = CoCreateInstance(&CLSID_CServiceConfig, null, 1, &IID_IUnknown, (void**) &unknownPtr);
            if (errorCode == -2147221008)
            {
                int modopt(IsLong) num3 = CoInitializeEx(null, 0);
                if (num3 < 0)
                {
                    Marshal.ThrowExceptionForHR(num3);
                }
                int modopt(IsLong) num2 = CoCreateInstance(&CLSID_CServiceConfig, null, 1, &IID_IUnknown, (void**) &unknownPtr);
                if (num2 < 0)
                {
                    Marshal.ThrowExceptionForHR(num2);
                }
            }
            else if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            IntPtr ptr2 = Marshal.StringToCoTaskMemUni("");
            this.m_pTrackerAppName = ptr2;
            IntPtr ptr = Marshal.StringToCoTaskMemUni("");
            this.m_pTrackerCtxName = ptr;
            this.m_pUnkSC = unknownPtr;
        }

        public void {dtor}()
        {
            GC.SuppressFinalize(this);
            this.Finalize();
        }

        protected override unsafe void Finalize()
        {
            IUnknown* pUnkSC = this.m_pUnkSC;
            if (pUnkSC != null)
            {
                **(((int*) pUnkSC))[8](pUnkSC);
                this.m_pUnkSC = null;
            }
            if (this.m_pTrackerAppName != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.m_pTrackerAppName);
            }
            if (this.m_pTrackerCtxName != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(this.m_pTrackerCtxName);
            }
        }

        public int Binding
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceThreadPoolConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceThreadPoolConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[0x10](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public object Byot
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IUnknown* unknownPtr = null;
                ITransaction* transactionPtr = null;
                IServiceTransactionConfig* configPtr = null;
                try
                {
                    if (value != null)
                    {
                        IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(value);
                        IntPtr ptr2 = iUnknownForObject;
                        unknownPtr = (IUnknown*) iUnknownForObject;
                        Marshal.ThrowExceptionForHR(**(*((int*) unknownPtr))(unknownPtr, &_GUID_0fb15084_af41_11ce_bd2b_204c4f4f5020, &transactionPtr));
                    }
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[0x20](configPtr, transactionPtr));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                    if (unknownPtr != null)
                    {
                        **(((int*) unknownPtr))[8](unknownPtr);
                    }
                    if (transactionPtr != null)
                    {
                        **(((int*) transactionPtr))[8](transactionPtr);
                    }
                }
            }
        }

        public object ByotSysTxn
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IUnknown* unknownPtr = null;
                IUnknown* unknownPtr2 = null;
                IServiceSysTxnConfigInternal* internalPtr = null;
                try
                {
                    if (value != null)
                    {
                        IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(value);
                        IntPtr ptr2 = iUnknownForObject;
                        unknownPtr = (IUnknown*) iUnknownForObject;
                        Marshal.ThrowExceptionForHR(**(*((int*) unknownPtr))(unknownPtr, &_GUID_02558374_df2e_4dae_bd6b_1d5c994f9bdc, &unknownPtr2));
                    }
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &_GUID_33caf1a1_fcb8_472b_b45e_967448ded6d8, &internalPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) internalPtr))[0x24](internalPtr, unknownPtr2));
                }
                finally
                {
                    if (internalPtr != null)
                    {
                        **(((int*) internalPtr))[8](internalPtr);
                    }
                    if (unknownPtr != null)
                    {
                        **(((int*) unknownPtr))[8](unknownPtr);
                    }
                    if (unknownPtr2 != null)
                    {
                        **(((int*) unknownPtr2))[8](unknownPtr2);
                    }
                }
            }
        }

        public bool COMTIIntrinsics
        {
            [param: MarshalAs(UnmanagedType.U1)]
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceComTIIntrinsicsConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceComTIIntrinsicsConfig, &configPtr));
                    int num2 = value ? 1 : 0;
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, num2));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public bool IISIntrinsics
        {
            [param: MarshalAs(UnmanagedType.U1)]
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceIISIntrinsicsConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceIISIntrinsicsConfig, &configPtr));
                    int num2 = value ? 1 : 0;
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, num2));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int Inheritance
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceInheritanceConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceInheritanceConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int Partition
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServicePartitionConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServicePartitionConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public Guid PartitionId
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServicePartitionConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServicePartitionConfig, &configPtr));
                    ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(value));
                    Marshal.StructureToPtr(value, ptr, false);
                    int num2 = *(((int*) configPtr)) + 0x10;
                    Marshal.ThrowExceptionForHR(*num2[0](configPtr, (void*) ptr));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public IUnknown* ServiceConfigUnknown
        {
            get
            {
                IUnknown* pUnkSC = this.m_pUnkSC;
                if (pUnkSC != null)
                {
                    **(((int*) pUnkSC))[4](pUnkSC);
                }
                return this.m_pUnkSC;
            }
        }

        public bool SupportsSysTxn
        {
            [return: MarshalAs(UnmanagedType.U1)]
            get
            {
                IUnknown* unknownPtr2 = null;
                if (!?A0x2b051ab0.?fInitialized@?1??get_SupportsSysTxn@ServiceConfigThunk@Thunk@EnterpriseServices@System@@Q$AAM_NXZ@4_NA)
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    ?A0x2b051ab0.?fSupportsSysTxn@?1??get_SupportsSysTxn@ServiceConfigThunk@Thunk@EnterpriseServices@System@@Q$AAM_NXZ@4_NA = **(*(((int*) pUnkSC)))(pUnkSC, &_GUID_33caf1a1_fcb8_472b_b45e_967448ded6d8, &unknownPtr2) >= 0;
                    ?A0x2b051ab0.?fInitialized@?1??get_SupportsSysTxn@ServiceConfigThunk@Thunk@EnterpriseServices@System@@Q$AAM_NXZ@4_NA = true;
                    if (unknownPtr2 != null)
                    {
                        **(((int*) unknownPtr2))[8](unknownPtr2);
                    }
                }
                return ?A0x2b051ab0.?fSupportsSysTxn@?1??get_SupportsSysTxn@ServiceConfigThunk@Thunk@EnterpriseServices@System@@Q$AAM_NXZ@4_NA;
            }
        }

        public int Sxs
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceSxsConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceSxsConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public string SxsDirectory
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceSxsConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceSxsConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 20;
                    Marshal.ThrowExceptionForHR(*num2[0](configPtr, (void*) ptr));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public string SxsName
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceSxsConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceSxsConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 0x10;
                    Marshal.ThrowExceptionForHR(*num2[0](configPtr, (void*) ptr));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int Synchronization
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceSynchronizationConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceSynchronizationConfig, &configPtr));
                    if (value > 0)
                    {
                        value--;
                    }
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int ThreadPool
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceThreadPoolConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceThreadPoolConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public string TipUrl
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTransactionConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 0x18;
                    Marshal.ThrowExceptionForHR(*num2[0](configPtr, (void*) ptr));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public bool Tracker
        {
            [param: MarshalAs(UnmanagedType.U1)]
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTrackerConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTrackerConfig, &configPtr));
                    int num4 = value ? 1 : 0;
                    int num3 = *(((int*) configPtr)) + 12;
                    Marshal.ThrowExceptionForHR(*num3[0](configPtr, num4, (void*) this.m_pTrackerAppName, (void*) this.m_pTrackerCtxName));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
                int num2 = value ? 1 : 0;
                this.m_tracker = num2;
            }
        }

        public string TrackerAppName
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTrackerConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTrackerConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 12;
                    int modopt(IsLong) errorCode = *num2[0](configPtr, this.m_tracker, (void*) ptr, (void*) this.m_pTrackerCtxName);
                    if (errorCode < 0)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                        ptr = Marshal.StringToCoTaskMemUni("");
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                finally
                {
                    if (this.m_pTrackerAppName != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(this.m_pTrackerAppName);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                    this.m_pTrackerAppName = ptr;
                }
            }
        }

        public string TrackerCtxName
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTrackerConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTrackerConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 12;
                    int modopt(IsLong) errorCode = *num2[0](configPtr, this.m_tracker, (void*) this.m_pTrackerAppName, (void*) ptr);
                    if (errorCode < 0)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                        ptr = Marshal.StringToCoTaskMemUni("");
                        Marshal.ThrowExceptionForHR(errorCode);
                    }
                }
                finally
                {
                    if (this.m_pTrackerCtxName != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(this.m_pTrackerCtxName);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                    this.m_pTrackerCtxName = ptr;
                }
            }
        }

        public int Transaction
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTransactionConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    if (value > 0)
                    {
                        value--;
                    }
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[12](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public string TxDesc
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTransactionConfig* configPtr = null;
                IntPtr ptr = new IntPtr();
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    ptr = Marshal.StringToCoTaskMemUni(value);
                    int num2 = *(((int*) configPtr)) + 0x1c;
                    Marshal.ThrowExceptionForHR(*num2[0](configPtr, (void*) ptr));
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeCoTaskMem(ptr);
                    }
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int TxIsolationLevel
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTransactionConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[0x10](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }

        public int TxTimeout
        {
            [HandleProcessCorruptedStateExceptions]
            set
            {
                IServiceTransactionConfig* configPtr = null;
                try
                {
                    IUnknown* pUnkSC = this.m_pUnkSC;
                    Marshal.ThrowExceptionForHR(**(*((int*) pUnkSC))(pUnkSC, &IID_IServiceTransactionConfig, &configPtr));
                    Marshal.ThrowExceptionForHR(**(((int*) configPtr))[20](configPtr, value));
                }
                finally
                {
                    if (configPtr != null)
                    {
                        **(((int*) configPtr))[8](configPtr);
                    }
                }
            }
        }
    }
}

