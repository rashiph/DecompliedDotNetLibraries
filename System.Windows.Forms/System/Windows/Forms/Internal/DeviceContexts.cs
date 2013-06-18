namespace System.Windows.Forms.Internal
{
    using System;
    using System.Windows.Forms;

    internal static class DeviceContexts
    {
        [ThreadStatic]
        private static System.Windows.Forms.ClientUtils.WeakRefCollection activeDeviceContexts;

        internal static void AddDeviceContext(DeviceContext dc)
        {
            if (activeDeviceContexts == null)
            {
                activeDeviceContexts = new System.Windows.Forms.ClientUtils.WeakRefCollection();
                activeDeviceContexts.RefCheckThreshold = 20;
            }
            if (!activeDeviceContexts.Contains(dc))
            {
                dc.Disposing += new EventHandler(DeviceContexts.OnDcDisposing);
                activeDeviceContexts.Add(dc);
            }
        }

        internal static bool IsFontInUse(WindowsFont wf)
        {
            if (wf != null)
            {
                for (int i = 0; i < activeDeviceContexts.Count; i++)
                {
                    DeviceContext context = activeDeviceContexts[i] as DeviceContext;
                    if ((context != null) && ((context.ActiveFont == wf) || context.IsFontOnContextStack(wf)))
                    {
                        return true;
                    }
                }
            }
            return false;
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

