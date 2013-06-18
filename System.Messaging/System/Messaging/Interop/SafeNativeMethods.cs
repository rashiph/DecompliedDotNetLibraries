namespace System.Messaging.Interop
{
    using System;
    using System.Messaging;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity, ComVisible(false)]
    internal static class SafeNativeMethods
    {
        public const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        public const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        public const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        public const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        public const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        public const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        public const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;

        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr arguments);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        public static extern bool GetComputerName(StringBuilder lpBuffer, int[] nSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true, ExactSpelling=true)]
        public static extern bool GetHandleInformation(SafeHandle handle, out int handleInformation);
        [DllImport("mqrt.dll", EntryPoint="MQBeginTransaction", CharSet=CharSet.Unicode)]
        public static extern int IntMQBeginTransaction(out ITransaction refTransaction);
        [DllImport("mqrt.dll", EntryPoint="MQInstanceToFormatName", CharSet=CharSet.Unicode)]
        public static extern int IntMQInstanceToFormatName(byte[] id, StringBuilder formatName, ref int count);
        [DllImport("mqrt.dll", EntryPoint="MQPathNameToFormatName", CharSet=CharSet.Unicode)]
        private static extern int IntMQPathNameToFormatName(string pathName, StringBuilder formatName, ref int count);
        [DllImport("kernel32.dll")]
        public static extern IntPtr LocalFree(IntPtr hMem);
        public static int MQBeginTransaction(out ITransaction refTransaction)
        {
            int num;
            try
            {
                num = IntMQBeginTransaction(out refTransaction);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQCloseCursor(IntPtr cursorHandle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQCloseQueue(IntPtr handle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQCreateCursor(MessageQueueHandle handle, out CursorHandle cursorHandle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern void MQFreeMemory(IntPtr memory);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern void MQFreeSecurityContext(IntPtr handle);
        public static int MQInstanceToFormatName(byte[] id, StringBuilder formatName, ref int count)
        {
            int num;
            try
            {
                num = IntMQInstanceToFormatName(id, formatName, ref count);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQLocateEnd(IntPtr enumHandle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQLocateNext(LocatorHandle enumHandle, ref int propertyCount, [Out] MQPROPVARIANTS[] variantArray);
        public static int MQPathNameToFormatName(string pathName, StringBuilder formatName, ref int count)
        {
            int num;
            try
            {
                num = IntMQPathNameToFormatName(pathName, formatName, ref count);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [DllImport("advapi32.dll")]
        public static extern int SetEntriesInAclW(int count, IntPtr entries, IntPtr oldacl, out IntPtr newAcl);

        public unsafe delegate void ReceiveCallback(int result, IntPtr handle, int timeout, int action, IntPtr propertiesPointer, NativeOverlapped* overlappedPointer, IntPtr cursorHandle);
    }
}

