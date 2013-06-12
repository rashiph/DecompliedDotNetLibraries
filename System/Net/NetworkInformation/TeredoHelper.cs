namespace System.Net.NetworkInformation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net.Sockets;
    using System.Security;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal class TeredoHelper
    {
        private readonly Action<object> callback;
        private SafeCancelMibChangeNotify cancelHandle;
        private static bool impendingAppDomainUnload;
        private readonly StableUnicastIpAddressTableDelegate onStabilizedDelegate;
        private static List<TeredoHelper> pendingNotifications = new List<TeredoHelper>();
        private bool runCallbackCalled;
        private readonly object state;

        static TeredoHelper()
        {
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(TeredoHelper.OnAppDomainUnload);
        }

        private TeredoHelper(Action<object> callback, object state)
        {
            this.callback = callback;
            this.state = state;
            this.onStabilizedDelegate = new StableUnicastIpAddressTableDelegate(this.OnStabilized);
            this.runCallbackCalled = false;
        }

        private static void OnAppDomainUnload(object sender, EventArgs args)
        {
            lock (pendingNotifications)
            {
                impendingAppDomainUnload = true;
                foreach (TeredoHelper helper in pendingNotifications)
                {
                    helper.cancelHandle.Dispose();
                }
            }
        }

        private void OnStabilized(IntPtr context, IntPtr table)
        {
            UnsafeNetInfoNativeMethods.FreeMibTable(table);
            if (!this.runCallbackCalled)
            {
                lock (this)
                {
                    if (!this.runCallbackCalled)
                    {
                        this.runCallbackCalled = true;
                        ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.RunCallback), null);
                    }
                }
            }
        }

        private void RunCallback(object o)
        {
            lock (pendingNotifications)
            {
                if (impendingAppDomainUnload)
                {
                    return;
                }
                pendingNotifications.Remove(this);
                this.cancelHandle.Dispose();
            }
            this.callback(this.state);
        }

        public static bool UnsafeNotifyStableUnicastIpAddressTable(Action<object> callback, object state)
        {
            TeredoHelper item = new TeredoHelper(callback, state);
            uint num = 0;
            SafeFreeMibTable table = null;
            lock (pendingNotifications)
            {
                if (impendingAppDomainUnload)
                {
                    return false;
                }
                num = UnsafeNetInfoNativeMethods.NotifyStableUnicastIpAddressTable(AddressFamily.Unspecified, out table, item.onStabilizedDelegate, IntPtr.Zero, out item.cancelHandle);
                if (table != null)
                {
                    table.Dispose();
                }
                if (num == 0x3e5)
                {
                    pendingNotifications.Add(item);
                    return false;
                }
            }
            if (num != 0)
            {
                throw new Win32Exception((int) num);
            }
            return true;
        }
    }
}

