namespace System.EnterpriseServices.Internal
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("ecabafd2-7f19-11d2-978e-0000f8757e2a")]
    public interface IClrObjectFactory
    {
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [DispId(1)]
        object CreateFromAssembly(string assembly, string type, string mode);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [DispId(2)]
        object CreateFromVroot(string VrootUrl, string Mode);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [DispId(3)]
        object CreateFromWsdl(string WsdlUrl, string Mode);
        [return: MarshalAs(UnmanagedType.IDispatch)]
        [DispId(4)]
        object CreateFromMailbox(string Mailbox, string Mode);
    }
}

