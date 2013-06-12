namespace System
{
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal class SafeTypeNameParserHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeTypeNameParserHandle() : base(true)
        {
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _ReleaseTypeNameParser(IntPtr pTypeNameParser);
        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            _ReleaseTypeNameParser(base.handle);
            base.handle = IntPtr.Zero;
            return true;
        }
    }
}

