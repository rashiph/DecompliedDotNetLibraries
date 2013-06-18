namespace System.Web
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal delegate void NativeFileChangeNotification(FileAction action, [In, MarshalAs(UnmanagedType.LPWStr)] string fileName, long ticks);
}

