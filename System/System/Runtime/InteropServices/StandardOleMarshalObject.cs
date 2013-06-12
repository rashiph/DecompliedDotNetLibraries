namespace System.Runtime.InteropServices
{
    using Microsoft.Win32;
    using System;
    using System.Security.Permissions;

    [ComVisible(true)]
    public class StandardOleMarshalObject : MarshalByRefObject, Microsoft.Win32.UnsafeNativeMethods.IMarshal
    {
        protected StandardOleMarshalObject()
        {
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private IntPtr GetStdMarshaller(ref Guid riid, int dwDestContext, int mshlflags)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(this);
            if (iUnknownForObject != IntPtr.Zero)
            {
                try
                {
                    if (Microsoft.Win32.UnsafeNativeMethods.CoGetStandardMarshal(ref riid, iUnknownForObject, dwDestContext, IntPtr.Zero, mshlflags, out zero) == 0)
                    {
                        return zero;
                    }
                }
                finally
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            throw new InvalidOperationException(SR.GetString("StandardOleMarshalObjectGetMarshalerFailed", new object[] { riid.ToString() }));
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.DisconnectObject(int dwReserved)
        {
            return -2147467263;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.GetMarshalSizeMax(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out int pSize)
        {
            int num;
            Guid guid = riid;
            IntPtr ptr = this.GetStdMarshaller(ref guid, dwDestContext, mshlflags);
            try
            {
                num = Microsoft.Win32.UnsafeNativeMethods.CoGetMarshalSizeMax(out pSize, ref guid, ptr, dwDestContext, pvDestContext, mshlflags);
            }
            finally
            {
                Marshal.Release(ptr);
            }
            return num;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.GetUnmarshalClass(ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags, out Guid pCid)
        {
            pCid = typeof(Microsoft.Win32.UnsafeNativeMethods.IStdMarshal).GUID;
            return 0;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.MarshalInterface(object pStm, ref Guid riid, IntPtr pv, int dwDestContext, IntPtr pvDestContext, int mshlflags)
        {
            int num;
            IntPtr ptr = this.GetStdMarshaller(ref riid, dwDestContext, mshlflags);
            try
            {
                num = Microsoft.Win32.UnsafeNativeMethods.CoMarshalInterface(pStm, ref riid, ptr, dwDestContext, pvDestContext, mshlflags);
            }
            finally
            {
                Marshal.Release(ptr);
                if (pStm != null)
                {
                    Marshal.ReleaseComObject(pStm);
                }
            }
            return num;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.ReleaseMarshalData(object pStm)
        {
            if (pStm != null)
            {
                Marshal.ReleaseComObject(pStm);
            }
            return -2147467263;
        }

        [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        int Microsoft.Win32.UnsafeNativeMethods.IMarshal.UnmarshalInterface(object pStm, ref Guid riid, out IntPtr ppv)
        {
            ppv = IntPtr.Zero;
            if (pStm != null)
            {
                Marshal.ReleaseComObject(pStm);
            }
            return -2147467263;
        }
    }
}

