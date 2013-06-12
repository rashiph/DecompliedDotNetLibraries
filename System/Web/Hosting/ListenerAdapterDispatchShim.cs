namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class ListenerAdapterDispatchShim : MarshalByRefObject, IRegisteredObject
    {
        internal IListenerChannelCallback MarshalComProxy(IListenerChannelCallback defaultDomainCallback)
        {
            IListenerChannelCallback objectForIUnknown = null;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(defaultDomainCallback);
            if (IntPtr.Zero == iUnknownForObject)
            {
                return null;
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                Guid gUID = typeof(IListenerChannelCallback).GUID;
                int errorCode = Marshal.QueryInterface(iUnknownForObject, ref gUID, out zero);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                objectForIUnknown = (IListenerChannelCallback) Marshal.GetObjectForIUnknown(zero);
            }
            finally
            {
                if (IntPtr.Zero != zero)
                {
                    Marshal.Release(zero);
                }
                if (IntPtr.Zero != iUnknownForObject)
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            return objectForIUnknown;
        }

        internal void StartListenerChannel(AppDomainProtocolHandler handler, IListenerChannelCallback listenerCallback)
        {
            IListenerChannelCallback listenerChannelCallback = this.MarshalComProxy(listenerCallback);
            if ((listenerChannelCallback != null) && (handler != null))
            {
                handler.StartListenerChannel(listenerChannelCallback);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }
    }
}

