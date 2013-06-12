namespace System.ComponentModel.Design
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public enum ViewTechnology
    {
        Default = 2,
        [Obsolete("This value has been deprecated. Use ViewTechnology.Default instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        Passthrough = 0,
        [Obsolete("This value has been deprecated. Use ViewTechnology.Default instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        WindowsForms = 1
    }
}

