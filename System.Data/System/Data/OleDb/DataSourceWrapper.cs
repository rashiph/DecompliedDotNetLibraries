namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class DataSourceWrapper : WrappedIUnknown
    {
        internal DataSourceWrapper()
        {
        }

        internal IDBInfoWrapper IDBInfo(OleDbConnectionInternal connection)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|datasource> %d#, IDBInfo\n", connection.ObjectID);
            return new IDBInfoWrapper(base.ComWrapper());
        }

        internal IDBPropertiesWrapper IDBProperties(OleDbConnectionInternal connection)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|datasource> %d#, IDBProperties\n", connection.ObjectID);
            return new IDBPropertiesWrapper(base.ComWrapper());
        }

        internal OleDbHResult InitializeAndCreateSession(OleDbConnectionString constr, ref SessionWrapper sessionWrapper)
        {
            OleDbHResult result;
            bool success = false;
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                IntPtr ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(base.handle, 0), 0);
                UnsafeNativeMethods.IUnknownQueryInterface dangerousDataSourceIUnknownQueryInterface = constr.DangerousDataSourceIUnknownQueryInterface;
                if ((dangerousDataSourceIUnknownQueryInterface == null) || (ptr != Marshal.GetFunctionPointerForDelegate(dangerousDataSourceIUnknownQueryInterface)))
                {
                    dangerousDataSourceIUnknownQueryInterface = (UnsafeNativeMethods.IUnknownQueryInterface) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IUnknownQueryInterface));
                    constr.DangerousDataSourceIUnknownQueryInterface = dangerousDataSourceIUnknownQueryInterface;
                }
                ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(base.handle, 0), 3 * IntPtr.Size);
                UnsafeNativeMethods.IDBInitializeInitialize dangerousIDBInitializeInitialize = constr.DangerousIDBInitializeInitialize;
                if ((dangerousIDBInitializeInitialize == null) || (ptr != Marshal.GetFunctionPointerForDelegate(dangerousIDBInitializeInitialize)))
                {
                    dangerousIDBInitializeInitialize = (UnsafeNativeMethods.IDBInitializeInitialize) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IDBInitializeInitialize));
                    constr.DangerousIDBInitializeInitialize = dangerousIDBInitializeInitialize;
                }
                result = dangerousIDBInitializeInitialize(base.handle);
                if ((OleDbHResult.S_OK > result) && (OleDbHResult.DB_E_ALREADYINITIALIZED != result))
                {
                    return result;
                }
                result = (OleDbHResult) dangerousDataSourceIUnknownQueryInterface(base.handle, ref ODB.IID_IDBCreateSession, ref zero);
                if ((OleDbHResult.S_OK > result) || !(IntPtr.Zero != zero))
                {
                    return result;
                }
                ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(zero, 0), 3 * IntPtr.Size);
                UnsafeNativeMethods.IDBCreateSessionCreateSession dangerousIDBCreateSessionCreateSession = constr.DangerousIDBCreateSessionCreateSession;
                if ((dangerousIDBCreateSessionCreateSession == null) || (ptr != Marshal.GetFunctionPointerForDelegate(dangerousIDBCreateSessionCreateSession)))
                {
                    dangerousIDBCreateSessionCreateSession = (UnsafeNativeMethods.IDBCreateSessionCreateSession) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IDBCreateSessionCreateSession));
                    constr.DangerousIDBCreateSessionCreateSession = dangerousIDBCreateSessionCreateSession;
                }
                if (constr.DangerousIDBCreateCommandCreateCommand != null)
                {
                    result = dangerousIDBCreateSessionCreateSession(zero, IntPtr.Zero, ref ODB.IID_IDBCreateCommand, ref sessionWrapper);
                    if ((OleDbHResult.S_OK <= result) && !sessionWrapper.IsInvalid)
                    {
                        sessionWrapper.VerifyIDBCreateCommand(constr);
                    }
                    return result;
                }
                result = dangerousIDBCreateSessionCreateSession(zero, IntPtr.Zero, ref ODB.IID_IUnknown, ref sessionWrapper);
                if ((OleDbHResult.S_OK <= result) && !sessionWrapper.IsInvalid)
                {
                    sessionWrapper.QueryInterfaceIDBCreateCommand(constr);
                }
            }
            finally
            {
                if (IntPtr.Zero != zero)
                {
                    Marshal.Release(zero);
                }
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return result;
        }
    }
}

