namespace System.Data.OleDb
{
    using System;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class SessionWrapper : WrappedIUnknown
    {
        private UnsafeNativeMethods.IDBCreateCommandCreateCommand DangerousIDBCreateCommandCreateCommand;

        internal SessionWrapper()
        {
        }

        internal OleDbHResult CreateCommand(ref object icommandText)
        {
            OleDbHResult result = OleDbHResult.E_NOINTERFACE;
            UnsafeNativeMethods.IDBCreateCommandCreateCommand dangerousIDBCreateCommandCreateCommand = this.DangerousIDBCreateCommandCreateCommand;
            if (dangerousIDBCreateCommandCreateCommand != null)
            {
                bool success = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    base.DangerousAddRef(ref success);
                    result = dangerousIDBCreateCommandCreateCommand(base.handle, IntPtr.Zero, ref ODB.IID_ICommandText, ref icommandText);
                }
                finally
                {
                    if (success)
                    {
                        base.DangerousRelease();
                    }
                }
            }
            return result;
        }

        internal IDBSchemaRowsetWrapper IDBSchemaRowset(OleDbConnectionInternal connection)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|session> %d#, IDBSchemaRowset\n", connection.ObjectID);
            return new IDBSchemaRowsetWrapper(base.ComWrapper());
        }

        internal IOpenRowsetWrapper IOpenRowset(OleDbConnectionInternal connection)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|session> %d#, IOpenRowset\n", connection.ObjectID);
            return new IOpenRowsetWrapper(base.ComWrapper());
        }

        internal ITransactionJoinWrapper ITransactionJoin(OleDbConnectionInternal connection)
        {
            Bid.Trace("<oledb.IUnknown.QueryInterface|API|OLEDB|session> %d#, ITransactionJoin\n", connection.ObjectID);
            return new ITransactionJoinWrapper(base.ComWrapper());
        }

        internal void QueryInterfaceIDBCreateCommand(OleDbConnectionString constr)
        {
            if (!constr.HaveQueriedForCreateCommand || (constr.DangerousIDBCreateCommandCreateCommand != null))
            {
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    UnsafeNativeMethods.IUnknownQueryInterface delegateForFunctionPointer = (UnsafeNativeMethods.IUnknownQueryInterface) Marshal.GetDelegateForFunctionPointer(Marshal.ReadIntPtr(Marshal.ReadIntPtr(base.handle, 0), 0), typeof(UnsafeNativeMethods.IUnknownQueryInterface));
                    int num = delegateForFunctionPointer(base.handle, ref ODB.IID_IDBCreateCommand, ref zero);
                    if ((0 <= num) && (IntPtr.Zero != zero))
                    {
                        IntPtr ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(zero, 0), 3 * IntPtr.Size);
                        this.DangerousIDBCreateCommandCreateCommand = (UnsafeNativeMethods.IDBCreateCommandCreateCommand) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IDBCreateCommandCreateCommand));
                        constr.DangerousIDBCreateCommandCreateCommand = this.DangerousIDBCreateCommandCreateCommand;
                    }
                    constr.HaveQueriedForCreateCommand = true;
                }
                finally
                {
                    if (IntPtr.Zero != zero)
                    {
                        IntPtr handle = base.handle;
                        base.handle = zero;
                        Marshal.Release(handle);
                    }
                }
            }
        }

        internal void VerifyIDBCreateCommand(OleDbConnectionString constr)
        {
            IntPtr ptr = Marshal.ReadIntPtr(Marshal.ReadIntPtr(base.handle, 0), 3 * IntPtr.Size);
            UnsafeNativeMethods.IDBCreateCommandCreateCommand dangerousIDBCreateCommandCreateCommand = constr.DangerousIDBCreateCommandCreateCommand;
            if ((dangerousIDBCreateCommandCreateCommand == null) || (ptr != Marshal.GetFunctionPointerForDelegate(dangerousIDBCreateCommandCreateCommand)))
            {
                dangerousIDBCreateCommandCreateCommand = (UnsafeNativeMethods.IDBCreateCommandCreateCommand) Marshal.GetDelegateForFunctionPointer(ptr, typeof(UnsafeNativeMethods.IDBCreateCommandCreateCommand));
                constr.DangerousIDBCreateCommandCreateCommand = dangerousIDBCreateCommandCreateCommand;
            }
            this.DangerousIDBCreateCommandCreateCommand = dangerousIDBCreateCommandCreateCommand;
        }
    }
}

