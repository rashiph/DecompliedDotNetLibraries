namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum WindowsAccountType
    {
        Normal,
        Guest,
        System,
        Anonymous
    }
}

