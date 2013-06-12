namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class OleDbServicesWrapper : WrappedIUnknown
    {
        private UnsafeNativeMethods.IDataInitializeGetDataSource DangerousIDataInitializeGetDataSource;

        internal OleDbServicesWrapper(object unknown)
        {
            if (unknown != null)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    base.handle = Marshal.GetComInterfaceForObject(unknown, typeof(UnsafeNativeMethods.IDataInitialize));
                }
                IntPtr ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(base.handle, 0), 3 * IntPtr.Size);
                this.DangerousIDataInitializeGetDataSource = (UnsafeNativeMethods.IDataInitializeGetDataSource) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IDataInitializeGetDataSource));
            }
        }

        internal void GetDataSource(OleDbConnectionString constr, ref DataSourceWrapper datasrcWrapper)
        {
            OleDbHResult result;
            UnsafeNativeMethods.IDataInitializeGetDataSource dangerousIDataInitializeGetDataSource = this.DangerousIDataInitializeGetDataSource;
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                string actualConnectionString = constr.ActualConnectionString;
                result = dangerousIDataInitializeGetDataSource(base.handle, IntPtr.Zero, 0x17, actualConnectionString, ref ODB.IID_IDBInitialize, ref datasrcWrapper);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            if (result < OleDbHResult.S_OK)
            {
                if (OleDbHResult.REGDB_E_CLASSNOTREG == result)
                {
                    throw ODB.ProviderUnavailable(constr.Provider, null);
                }
                throw OleDbConnection.ProcessResults(result, null, null);
            }
            if (datasrcWrapper.IsInvalid)
            {
                SafeNativeMethods.Wrapper.ClearErrorInfo();
                throw ODB.ProviderUnavailable(constr.Provider, null);
            }
        }
    }
}

