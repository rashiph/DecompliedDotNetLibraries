namespace Microsoft.Win32
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    internal static class Fusion
    {
        [SecuritySafeCritical]
        private static unsafe string GetDisplayName(IAssemblyName aName, uint dwDisplayFlags)
        {
            uint pccDisplayName = 0;
            string str = null;
            aName.GetDisplayName(IntPtr.Zero, ref pccDisplayName, dwDisplayFlags);
            if (pccDisplayName > 0)
            {
                IntPtr zero = IntPtr.Zero;
                byte[] buffer = new byte[(pccDisplayName + 1) * 2];
                fixed (byte* numRef = buffer)
                {
                    zero = new IntPtr((void*) numRef);
                    aName.GetDisplayName(zero, ref pccDisplayName, dwDisplayFlags);
                    str = Marshal.PtrToStringUni(zero);
                }
            }
            return str;
        }

        [SecurityCritical]
        public static void ReadCache(ArrayList alAssems, string name, uint nFlag)
        {
            IAssemblyEnum ppEnum = null;
            IAssemblyName ppName = null;
            IAssemblyName name3 = null;
            IApplicationContext pAppCtx = null;
            int num;
            if (name != null)
            {
                num = Win32Native.CreateAssemblyNameObject(out name3, name, 1, IntPtr.Zero);
                if (num != 0)
                {
                    Marshal.ThrowExceptionForHR(num);
                }
            }
            num = Win32Native.CreateAssemblyEnum(out ppEnum, pAppCtx, name3, nFlag, IntPtr.Zero);
            if (num != 0)
            {
                Marshal.ThrowExceptionForHR(num);
            }
        Label_0042:
            num = ppEnum.GetNextAssembly(out pAppCtx, out ppName, 0);
            if (num == 0)
            {
                string displayName = GetDisplayName(ppName, 0);
                if (displayName != null)
                {
                    alAssems.Add(displayName);
                }
                goto Label_0042;
            }
            if (num < 0)
            {
                Marshal.ThrowExceptionForHR(num);
            }
        }
    }
}

