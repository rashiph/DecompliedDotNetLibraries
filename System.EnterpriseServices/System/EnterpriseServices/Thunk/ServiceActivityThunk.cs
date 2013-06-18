namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;

    internal class ServiceActivityThunk
    {
        public unsafe IServiceActivity* m_pSA;

        public unsafe ServiceActivityThunk(ServiceConfigThunk psct)
        {
            IServiceActivity* activityPtr;
            IUnknown* serviceConfigUnknown = psct.ServiceConfigUnknown;
            this.m_pSA = null;
            **(((int*) serviceConfigUnknown))[8](serviceConfigUnknown);
            Marshal.ThrowExceptionForHR(*ServiceDomainThunk.CoCreateActivity(serviceConfigUnknown, &IID_IServiceActivity, &activityPtr));
            this.m_pSA = activityPtr;
        }

        public void {dtor}()
        {
            GC.SuppressFinalize(this);
            this.Finalize();
        }

        [HandleProcessCorruptedStateExceptions]
        public unsafe void AsynchronousCall(object pObj)
        {
            IUnknown* unknownPtr = null;
            IServiceCall* callPtr = null;
            try
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(pObj);
                IntPtr ptr2 = iUnknownForObject;
                unknownPtr = (IUnknown*) iUnknownForObject;
                Marshal.ThrowExceptionForHR(**(*((int*) unknownPtr))(unknownPtr, &IID_IServiceCall, &callPtr));
                IServiceActivity* pSA = this.m_pSA;
                Marshal.ThrowExceptionForHR(**(((int*) pSA))[0x10](pSA, callPtr));
            }
            finally
            {
                if (callPtr != null)
                {
                    **(((int*) callPtr))[8](callPtr);
                }
                if (unknownPtr != null)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
            }
        }

        public unsafe void BindToCurrentThread()
        {
            Marshal.ThrowExceptionForHR(**(((int*) this.m_pSA))[20](this.m_pSA));
        }

        protected override unsafe void Finalize()
        {
            IServiceActivity* pSA = this.m_pSA;
            if (pSA != null)
            {
                **(((int*) pSA))[8](pSA);
                this.m_pSA = null;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        public unsafe void SynchronousCall(object pObj)
        {
            IUnknown* unknownPtr = null;
            IServiceCall* callPtr = null;
            try
            {
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(pObj);
                IntPtr ptr2 = iUnknownForObject;
                unknownPtr = (IUnknown*) iUnknownForObject;
                Marshal.ThrowExceptionForHR(**(*((int*) unknownPtr))(unknownPtr, &IID_IServiceCall, &callPtr));
                IServiceActivity* pSA = this.m_pSA;
                Marshal.ThrowExceptionForHR(**(((int*) pSA))[12](pSA, callPtr));
            }
            finally
            {
                if (callPtr != null)
                {
                    **(((int*) callPtr))[8](callPtr);
                }
                if (unknownPtr != null)
                {
                    **(((int*) unknownPtr))[8](unknownPtr);
                }
            }
        }

        public unsafe void UnbindFromThread()
        {
            Marshal.ThrowExceptionForHR(**(((int*) this.m_pSA))[0x18](this.m_pSA));
        }
    }
}

