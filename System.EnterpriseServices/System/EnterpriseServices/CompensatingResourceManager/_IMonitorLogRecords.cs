namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("70C8E441-C7ED-11D1-82FB-00A0C91EEDE9")]
    internal interface _IMonitorLogRecords
    {
        int Count { get; }
        System.EnterpriseServices.CompensatingResourceManager.TransactionState TransactionState { get; }
        bool StructuredRecords { [return: MarshalAs(UnmanagedType.VariantBool)] get; }
        void GetLogRecord([In] int dwIndex, [In, Out, MarshalAs(UnmanagedType.LPStruct)] ref _LogRecord pRecord);
        object GetLogRecordVariants([In] object IndexNumber);
    }
}

