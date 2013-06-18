namespace System.Management
{
    using System;
    using System.Runtime.InteropServices;

    internal class WbemErrorInfo
    {
        public static IWbemClassObjectFreeThreaded GetErrorInfo()
        {
            IErrorInfo errorInfo = GetErrorInfo(0);
            if (errorInfo != null)
            {
                IntPtr ptr2;
                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(errorInfo);
                Marshal.QueryInterface(iUnknownForObject, ref IWbemClassObjectFreeThreaded.IID_IWbemClassObject, out ptr2);
                Marshal.Release(iUnknownForObject);
                if (ptr2 != IntPtr.Zero)
                {
                    return new IWbemClassObjectFreeThreaded(ptr2);
                }
            }
            return null;
        }

        [DllImport("oleaut32.dll", PreserveSig=false)]
        private static extern IErrorInfo GetErrorInfo(int reserved);
    }
}

