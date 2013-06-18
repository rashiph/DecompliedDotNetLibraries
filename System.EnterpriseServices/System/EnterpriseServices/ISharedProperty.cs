namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("2A005C01-A5DE-11CF-9E66-00AA00A3F464")]
    internal interface ISharedProperty
    {
        object Value { get; set; }
    }
}

