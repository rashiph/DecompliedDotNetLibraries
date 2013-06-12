namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum PEFileKinds
    {
        ConsoleApplication = 2,
        Dll = 1,
        WindowApplication = 3
    }
}

