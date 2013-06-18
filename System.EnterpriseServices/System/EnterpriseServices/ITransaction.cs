namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("0FB15084-AF41-11CE-BD2B-204C4F4F5020"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITransaction
    {
        void Commit(int fRetaining, int grfTC, int grfRM);
        void Abort(ref BOID pboidReason, int fRetaining, int fAsync);
        void GetTransactionInfo(out XACTTRANSINFO pinfo);
    }
}

