namespace System.Windows.Forms
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class ComponentManagerBroker : MarshalByRefObject
    {
        private static ComponentManagerBroker _broker;
        [ThreadStatic]
        private ComponentManagerProxy _proxy;
        private static string _remoteObjectName;
        private static object _syncObject;

        static ComponentManagerBroker()
        {
            int currentProcessId = SafeNativeMethods.GetCurrentProcessId();
            _syncObject = new object();
            _remoteObjectName = string.Format(CultureInfo.CurrentCulture, "ComponentManagerBroker.{0}.{1:X}", new object[] { Application.WindowsFormsVersion, currentProcessId });
        }

        public ComponentManagerBroker()
        {
            if (_broker == null)
            {
                _broker = this;
            }
        }

        internal void ClearComponentManager()
        {
            this._proxy = null;
        }

        internal static UnsafeNativeMethods.IMsoComponentManager GetComponentManager(IntPtr pOriginal)
        {
            lock (_syncObject)
            {
                if (_broker == null)
                {
                    object obj2;
                    ((UnsafeNativeMethods.ICorRuntimeHost) RuntimeEnvironment.GetRuntimeInterfaceAsObject(typeof(UnsafeNativeMethods.CorRuntimeHost).GUID, typeof(UnsafeNativeMethods.ICorRuntimeHost).GUID)).GetDefaultDomain(out obj2);
                    AppDomain currentDomain = obj2 as AppDomain;
                    if (currentDomain == null)
                    {
                        currentDomain = AppDomain.CurrentDomain;
                    }
                    if (currentDomain == AppDomain.CurrentDomain)
                    {
                        _broker = new ComponentManagerBroker();
                    }
                    else
                    {
                        _broker = GetRemotedComponentManagerBroker(currentDomain);
                    }
                }
            }
            return _broker.GetProxy((long) pOriginal);
        }

        public UnsafeNativeMethods.IMsoComponentManager GetProxy(long pCM)
        {
            if (this._proxy == null)
            {
                UnsafeNativeMethods.IMsoComponentManager objectForIUnknown = (UnsafeNativeMethods.IMsoComponentManager) Marshal.GetObjectForIUnknown((IntPtr) pCM);
                this._proxy = new ComponentManagerProxy(this, objectForIUnknown);
            }
            return this._proxy;
        }

        private static ComponentManagerBroker GetRemotedComponentManagerBroker(AppDomain domain)
        {
            System.Type type = typeof(ComponentManagerBroker);
            ComponentManagerBroker broker = (ComponentManagerBroker) domain.CreateInstanceAndUnwrap(type.Assembly.FullName, type.FullName);
            return broker.Singleton;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        internal ComponentManagerBroker Singleton
        {
            get
            {
                return _broker;
            }
        }
    }
}

