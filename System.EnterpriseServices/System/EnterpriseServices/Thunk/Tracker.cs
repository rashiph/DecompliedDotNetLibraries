namespace System.EnterpriseServices.Thunk
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class Tracker
    {
        private unsafe ISendMethodEvents* _pTracker;

        internal unsafe Tracker(ISendMethodEvents* pTracker)
        {
            this._pTracker = pTracker;
            **(((int*) pTracker))[4](pTracker);
        }

        public unsafe void Release()
        {
            ISendMethodEvents* eventsPtr = this._pTracker;
            if (eventsPtr != null)
            {
                **(((int*) eventsPtr))[8](eventsPtr);
                this._pTracker = null;
            }
        }

        public unsafe void SendMethodCall(IntPtr pIdentity, MethodBase method)
        {
            if (this._pTracker != null)
            {
                _GUID _guid;
                Guid guid = Marshal.GenerateGuidForType(method.ReflectedType);
                memcpy(&_guid, ((int) &guid), 0x10);
                int comSlotForMethodInfo = 4;
                if (method.ReflectedType.IsInterface)
                {
                    comSlotForMethodInfo = Marshal.GetComSlotForMethodInfo(method);
                }
                int num2 = *(((int*) this._pTracker)) + 12;
                *num2[0](this._pTracker, (void*) pIdentity, &_guid, comSlotForMethodInfo);
            }
        }

        public unsafe void SendMethodReturn(IntPtr pIdentity, MethodBase method, Exception except)
        {
            if (this._pTracker != null)
            {
                _GUID _guid;
                Guid guid = Marshal.GenerateGuidForType(method.ReflectedType);
                memcpy(&_guid, ((int) &guid), 0x10);
                int comSlotForMethodInfo = 4;
                if (method.ReflectedType.IsInterface)
                {
                    comSlotForMethodInfo = Marshal.GetComSlotForMethodInfo(method);
                }
                int modopt(IsLong) hRForException = 0;
                if (except != null)
                {
                    hRForException = Marshal.GetHRForException(except);
                }
                int num3 = *(((int*) this._pTracker)) + 0x10;
                *num3[0](this._pTracker, (void*) pIdentity, &_guid, comSlotForMethodInfo, 0, hRForException);
            }
        }
    }
}

