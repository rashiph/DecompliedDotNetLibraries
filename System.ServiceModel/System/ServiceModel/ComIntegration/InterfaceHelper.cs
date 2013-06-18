namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal static class InterfaceHelper
    {
        internal static IntPtr GetInterfacePtrForObject(Guid iid, object obj)
        {
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(obj);
            if (IntPtr.Zero == iUnknownForObject)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("UnableToRetrievepUnk")));
            }
            IntPtr zero = IntPtr.Zero;
            int num = Marshal.QueryInterface(iUnknownForObject, ref iid, out zero);
            Marshal.Release(iUnknownForObject);
            if (num != HR.S_OK)
            {
                throw Fx.AssertAndThrow("QueryInterface should succeed");
            }
            return zero;
        }
    }
}

