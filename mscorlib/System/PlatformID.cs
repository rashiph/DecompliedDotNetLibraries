namespace System
{
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum PlatformID
    {
        Win32S,
        Win32Windows,
        Win32NT,
        WinCE,
        Unix,
        Xbox,
        MacOSX
    }
}

