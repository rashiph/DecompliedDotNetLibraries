namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;

    internal class MainWindowFinder
    {
        private IntPtr bestHandle;
        private int processId;

        private bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            int num;
            Microsoft.Win32.NativeMethods.GetWindowThreadProcessId(new HandleRef(this, handle), out num);
            if ((num == this.processId) && this.IsMainWindow(handle))
            {
                this.bestHandle = handle;
                return false;
            }
            return true;
        }

        public IntPtr FindMainWindow(int processId)
        {
            this.bestHandle = IntPtr.Zero;
            this.processId = processId;
            Microsoft.Win32.NativeMethods.EnumThreadWindowsCallback callback = new Microsoft.Win32.NativeMethods.EnumThreadWindowsCallback(this.EnumWindowsCallback);
            Microsoft.Win32.NativeMethods.EnumWindows(callback, IntPtr.Zero);
            GC.KeepAlive(callback);
            return this.bestHandle;
        }

        private bool IsMainWindow(IntPtr handle)
        {
            return (!(Microsoft.Win32.NativeMethods.GetWindow(new HandleRef(this, handle), 4) != IntPtr.Zero) && Microsoft.Win32.NativeMethods.IsWindowVisible(new HandleRef(this, handle)));
        }
    }
}

