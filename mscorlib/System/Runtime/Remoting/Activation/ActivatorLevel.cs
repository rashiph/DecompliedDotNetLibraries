namespace System.Runtime.Remoting.Activation
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum ActivatorLevel
    {
        AppDomain = 12,
        Construction = 4,
        Context = 8,
        Machine = 20,
        Process = 0x10
    }
}

