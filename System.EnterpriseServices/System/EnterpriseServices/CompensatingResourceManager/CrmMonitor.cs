namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class CrmMonitor
    {
        private unsafe ICrmMonitor* _pMon;

        public unsafe CrmMonitor()
        {
            ICrmMonitor* monitorPtr;
            int modopt(IsLong) errorCode = CoCreateInstance(&CLSID_CRMRecoveryClerk, null, 0x15, &IID_ICrmMonitor, (void**) &monitorPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            this._pMon = monitorPtr;
        }

        public unsafe void AddRef()
        {
            **(((int*) this._pMon))[4](this._pMon);
        }

        public unsafe object GetClerks()
        {
            ICrmMonitorClerks* clerksPtr;
            ICrmMonitor* monitorPtr = this._pMon;
            int modopt(IsLong) errorCode = **(((int*) monitorPtr))[12](monitorPtr, &clerksPtr);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            object objectForIUnknown = null;
            try
            {
                IntPtr pUnk = new IntPtr((void*) clerksPtr);
                objectForIUnknown = Marshal.GetObjectForIUnknown(pUnk);
            }
            finally
            {
                **(((int*) clerksPtr))[8](clerksPtr);
            }
            return objectForIUnknown;
        }

        public unsafe CrmLogControl HoldClerk(object idx)
        {
            CrmLogControl control = null;
            tagVARIANT gvariant;
            tagVARIANT gvariant2;
            IntPtr pDstNativeVariant = new IntPtr((int) &gvariant);
            VariantInit(&gvariant);
            VariantInit(&gvariant2);
            Marshal.GetNativeVariantForObject(idx, pDstNativeVariant);
            ICrmMonitor* monitorPtr = this._pMon;
            int modopt(IsLong) errorCode = **(((int*) monitorPtr))[0x10](monitorPtr, gvariant, &gvariant2);
            VariantClear(&gvariant);
            if (errorCode < 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            IUnknown* unknownPtr = *((IUnknown**) (&gvariant2 + 8));
            if (*(((int*) (&gvariant2 + 8))) != 0)
            {
                try
                {
                    IntPtr p = new IntPtr((int) unknownPtr);
                    control = new CrmLogControl(p);
                }
                finally
                {
                    VariantClear(&gvariant2);
                }
            }
            return control;
        }

        public unsafe void Release()
        {
            **(((int*) this._pMon))[8](this._pMon);
        }
    }
}

