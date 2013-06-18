namespace System.EnterpriseServices.CompensatingResourceManager
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("70C8E442-C7ED-11D1-82FB-00A0C91EEDE9")]
    internal interface _IMonitorClerks
    {
        object Item(object index);
        [return: MarshalAs(UnmanagedType.Interface)]
        object _NewEnum();
        int Count();
        object ProgIdCompensator(object index);
        object Description(object index);
        object TransactionUOW(object index);
        object ActivityId(object index);
    }
}

