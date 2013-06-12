namespace System.Net
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal sealed class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private uint resClose;

        private SafeRegistryHandle() : base(true)
        {
        }

        internal uint QueryValue(string name, out object data)
        {
            uint num2;
            uint num3;
            data = null;
            byte[] buffer = null;
            uint size = 0;
            while (true)
            {
                num3 = UnsafeNclNativeMethods.RegistryHelper.RegQueryValueEx(this, name, IntPtr.Zero, out num2, buffer, ref size);
                if ((num3 != 0xea) && ((buffer != null) || (num3 != 0)))
                {
                    break;
                }
                buffer = new byte[size];
            }
            if (num3 != 0)
            {
                return num3;
            }
            switch (num2)
            {
                case 3:
                    return 50;
            }
            if (size != buffer.Length)
            {
                byte[] src = buffer;
                buffer = new byte[size];
                Buffer.BlockCopy(src, 0, buffer, 0, (int) size);
            }
            data = buffer;
            return 0;
        }

        internal uint RegCloseKey()
        {
            base.Close();
            return this.resClose;
        }

        internal uint RegNotifyChangeKeyValue(bool watchSubTree, uint notifyFilter, SafeWaitHandle regEvent, bool async)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegNotifyChangeKeyValue(this, watchSubTree, notifyFilter, regEvent, async);
        }

        internal static uint RegOpenCurrentUser(uint samDesired, out System.Net.SafeRegistryHandle resultKey)
        {
            if (ComNetOS.IsWin9x)
            {
                return UnsafeNclNativeMethods.RegistryHelper.RegOpenKeyEx(UnsafeNclNativeMethods.RegistryHelper.HKEY_CURRENT_USER, null, 0, samDesired, out resultKey);
            }
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenCurrentUser(samDesired, out resultKey);
        }

        internal uint RegOpenKeyEx(string subKey, uint ulOptions, uint samDesired, out System.Net.SafeRegistryHandle resultSubKey)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenKeyEx(this, subKey, ulOptions, samDesired, out resultSubKey);
        }

        internal static uint RegOpenKeyEx(IntPtr key, string subKey, uint ulOptions, uint samDesired, out System.Net.SafeRegistryHandle resultSubKey)
        {
            return UnsafeNclNativeMethods.RegistryHelper.RegOpenKeyEx(key, subKey, ulOptions, samDesired, out resultSubKey);
        }

        protected override bool ReleaseHandle()
        {
            if (!this.IsInvalid)
            {
                this.resClose = UnsafeNclNativeMethods.RegistryHelper.RegCloseKey(base.handle);
            }
            base.SetHandleAsInvalid();
            return true;
        }
    }
}

