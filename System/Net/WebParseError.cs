namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct WebParseError
    {
        public WebParseErrorSection Section;
        public WebParseErrorCode Code;
    }
}

