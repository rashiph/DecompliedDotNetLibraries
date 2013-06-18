namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class CrmLogControl
    {
        private unsafe ICrmLogControl* _pCtrl;

        public unsafe CrmLogControl()
        {
            ICrmLogControl* controlPtr;
            this._pCtrl = null;
            int modopt(IsLong) errorCode = CoCreateInstance(&CLSID_CRMClerk, null, 0x15, &IID_ICrmLogControl, (void**) &controlPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            this._pCtrl = controlPtr;
        }

        public unsafe CrmLogControl(IntPtr p)
        {
            ICrmLogControl* controlPtr;
            IUnknown* unknownPtr = (IUnknown*) p.ToInt32();
            if (unknownPtr == null)
            {
                throw new NullReferenceException();
            }
            int modopt(IsLong) errorCode = **(*((int*) unknownPtr))(unknownPtr, &IID_ICrmLogControl, &controlPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            this._pCtrl = controlPtr;
        }

        public unsafe void Dispose()
        {
            ICrmLogControl* controlPtr = this._pCtrl;
            if (controlPtr != null)
            {
                **(((int*) controlPtr))[8](controlPtr);
                this._pCtrl = null;
            }
        }

        public unsafe void ForceLog()
        {
            int modopt(IsLong) errorCode = **(((int*) this._pCtrl))[0x18](this._pCtrl);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public unsafe void ForceTransactionToAbort()
        {
            int modopt(IsLong) errorCode = **(((int*) this._pCtrl))[0x20](this._pCtrl);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public unsafe void ForgetLogRecord()
        {
            int modopt(IsLong) errorCode = **(((int*) this._pCtrl))[0x1c](this._pCtrl);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
        }

        public unsafe CrmMonitorLogRecords GetMonitor()
        {
            return new CrmMonitorLogRecords(new IntPtr((int) this._pCtrl));
        }

        public unsafe string GetTransactionUOW()
        {
            char* chPtr;
            ICrmLogControl* controlPtr = this._pCtrl;
            int modopt(IsLong) errorCode = **(((int*) controlPtr))[12](controlPtr, &chPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            IntPtr ptr = new IntPtr((void*) chPtr);
            SysFreeString(chPtr);
            return Marshal.PtrToStringBSTR(ptr);
        }

        public unsafe void RegisterCompensator(string progid, string desc, int modopt(IsLong) flags)
        {
            char* chPtr4 = null;
            char* chPtr3 = null;
            try
            {
                char* chPtr2 = (char*) Marshal.StringToCoTaskMemUni(progid).ToInt32();
                char* chPtr = (char*) Marshal.StringToCoTaskMemUni(desc).ToInt32();
                ICrmLogControl* controlPtr = this._pCtrl;
                int modopt(IsLong) errorCode = **(((int*) controlPtr))[0x10](controlPtr, chPtr2, chPtr, flags);
                int modopt(IsLong) num2 = errorCode;
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
            }
            finally
            {
                CoTaskMemFree((void*) chPtr4);
                CoTaskMemFree((void*) chPtr3);
            }
        }

        public unsafe void WriteLogRecord(byte[] b)
        {
            tagBLOB gblob;
            *((int*) &gblob) = b.Length;
            fixed (byte* numRef = b)
            {
                *((int*) (&gblob + 4)) = numRef;
                ICrmLogControl* controlPtr = this._pCtrl;
                int modopt(IsLong) errorCode = **(((int*) controlPtr))[0x24](controlPtr, &gblob, 1);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                return;
            }
        }
    }
}

