namespace System.DirectoryServices.Interop
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal class SafeNativeMethods
    {
        public const int ERROR_MORE_DATA = 0xea;
        public const int ERROR_SUCCESS = 0;
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;

        [DllImport("activeds.dll", CharSet=CharSet.Unicode)]
        public static extern int ADsGetLastError(out int error, StringBuilder errorBuffer, int errorBufferLength, StringBuilder nameBuffer, int nameBufferLength);
        [DllImport("activeds.dll", CharSet=CharSet.Unicode)]
        public static extern int ADsSetLastError(int error, string errorString, string provider);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        public static extern int FormatMessageW(int dwFlags, int lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, int arguments);
        [DllImport("activeds.dll")]
        public static extern bool FreeADsMem(IntPtr pVoid);
        [DllImport("oleaut32.dll", PreserveSig=false)]
        public static extern void VariantClear(IntPtr pObject);
        [DllImport("oleaut32.dll")]
        public static extern void VariantInit(IntPtr pObject);

        [ComVisible(false)]
        public class EnumVariant
        {
            private object currentValue = NoMoreValues;
            private SafeNativeMethods.IEnumVariant enumerator;
            private static readonly object NoMoreValues = new object();

            public EnumVariant(SafeNativeMethods.IEnumVariant en)
            {
                if (en == null)
                {
                    throw new ArgumentNullException("en");
                }
                this.enumerator = en;
            }

            private void Advance()
            {
                this.currentValue = NoMoreValues;
                IntPtr pObject = Marshal.AllocCoTaskMem(0x10);
                try
                {
                    int[] pceltFetched = new int[1];
                    SafeNativeMethods.VariantInit(pObject);
                    this.enumerator.Next(1, pObject, pceltFetched);
                    try
                    {
                        if (pceltFetched[0] > 0)
                        {
                            this.currentValue = Marshal.GetObjectForNativeVariant(pObject);
                        }
                    }
                    finally
                    {
                        SafeNativeMethods.VariantClear(pObject);
                    }
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pObject);
                }
            }

            public bool GetNext()
            {
                this.Advance();
                return (this.currentValue != NoMoreValues);
            }

            public object GetValue()
            {
                if (this.currentValue == NoMoreValues)
                {
                    throw new InvalidOperationException(Res.GetString("DSEnumerator"));
                }
                return this.currentValue;
            }

            public void Reset()
            {
                this.enumerator.Reset();
                this.currentValue = NoMoreValues;
            }
        }

        [ComImport, Guid("00020404-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IEnumVariant
        {
            [SuppressUnmanagedCodeSecurity]
            void Next([In, MarshalAs(UnmanagedType.U4)] int celt, [In, Out] IntPtr rgvar, [Out, MarshalAs(UnmanagedType.LPArray)] int[] pceltFetched);
            [SuppressUnmanagedCodeSecurity]
            void Skip([In, MarshalAs(UnmanagedType.U4)] int celt);
            [SuppressUnmanagedCodeSecurity]
            void Reset();
            [SuppressUnmanagedCodeSecurity]
            void Clone([Out, MarshalAs(UnmanagedType.LPArray)] SafeNativeMethods.IEnumVariant[] ppenum);
        }
    }
}

