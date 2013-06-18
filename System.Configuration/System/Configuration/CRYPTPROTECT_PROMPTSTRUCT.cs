namespace System.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CRYPTPROTECT_PROMPTSTRUCT : IDisposable
    {
        public int cbSize;
        public int dwPromptFlags;
        public IntPtr hwndApp;
        public string szPrompt;
        void IDisposable.Dispose()
        {
            this.hwndApp = IntPtr.Zero;
        }
    }
}

