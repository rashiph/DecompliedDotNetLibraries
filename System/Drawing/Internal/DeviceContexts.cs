namespace System.Drawing.Internal
{
    using System;
    using System.Drawing;

    internal static class DeviceContexts
    {
        [ThreadStatic]
        private static System.Drawing.ClientUtils.WeakRefCollection activeDeviceContexts;

        internal static void AddDeviceContext(DeviceContext dc)
        {
            if (activeDeviceContexts == null)
            {
                activeDeviceContexts = new System.Drawing.ClientUtils.WeakRefCollection();
                activeDeviceContexts.RefCheckThreshold = 20;
            }
            if (!activeDeviceContexts.Contains(dc))
            {
                dc.Disposing += new EventHandler(DeviceContexts.OnDcDisposing);
                activeDeviceContexts.Add(dc);
            }
        }

        private static void OnDcDisposing(object sender, EventArgs e)
        {
            DeviceContext dc = sender as DeviceContext;
            if (dc != null)
            {
                dc.Disposing -= new EventHandler(DeviceContexts.OnDcDisposing);
                RemoveDeviceContext(dc);
            }
        }

        internal static void RemoveDeviceContext(DeviceContext dc)
        {
            if (activeDeviceContexts != null)
            {
                activeDeviceContexts.RemoveByHashCode(dc);
            }
        }
    }
}

