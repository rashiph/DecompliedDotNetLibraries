namespace System.Messaging.Interop
{
    using System;
    using System.Messaging;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [ComVisible(false), SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool GetSecurityDescriptorDacl(IntPtr pSD, out bool daclPresent, out IntPtr pDacl, out bool daclDefaulted);
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool InitializeSecurityDescriptor(System.Messaging.Interop.NativeMethods.SECURITY_DESCRIPTOR SD, int revision);
        [DllImport("mqrt.dll", EntryPoint="MQCreateQueue", CharSet=CharSet.Unicode)]
        private static extern int IntMQCreateQueue(IntPtr securityDescriptor, MessagePropertyVariants.MQPROPS queueProperties, StringBuilder formatName, ref int formatNameLength);
        [DllImport("mqrt.dll", EntryPoint="MQDeleteQueue", CharSet=CharSet.Unicode)]
        private static extern int IntMQDeleteQueue(string formatName);
        [DllImport("mqrt.dll", EntryPoint="MQGetMachineProperties", CharSet=CharSet.Unicode)]
        private static extern int IntMQGetMachineProperties(string machineName, IntPtr machineIdPointer, MessagePropertyVariants.MQPROPS machineProperties);
        [DllImport("mqrt.dll", EntryPoint="MQGetQueueProperties", CharSet=CharSet.Unicode)]
        private static extern int IntMQGetQueueProperties(string formatName, MessagePropertyVariants.MQPROPS queueProperties);
        [DllImport("mqrt.dll", EntryPoint="MQLocateBegin", CharSet=CharSet.Unicode)]
        private static extern int IntMQLocateBegin(string context, Restrictions.MQRESTRICTION Restriction, Columns.MQCOLUMNSET columnSet, IntPtr sortSet, out LocatorHandle enumHandle);
        [DllImport("mqrt.dll", EntryPoint="MQMgmtGetInfo", CharSet=CharSet.Unicode)]
        private static extern int IntMQMgmtGetInfo(string machineName, string objectName, MessagePropertyVariants.MQPROPS queueProperties);
        [DllImport("mqrt.dll", EntryPoint="MQOpenQueue", CharSet=CharSet.Unicode)]
        private static extern int IntMQOpenQueue(string formatName, int access, int shareMode, out MessageQueueHandle handle);
        [DllImport("mqrt.dll", EntryPoint="MQReceiveMessageByLookupId", CharSet=CharSet.Unicode)]
        private static extern unsafe int IntMQReceiveMessageByLookupId(MessageQueueHandle handle, long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, IntPtr transaction);
        [DllImport("mqrt.dll", EntryPoint="MQReceiveMessageByLookupId", CharSet=CharSet.Unicode)]
        private static extern unsafe int IntMQReceiveMessageByLookupId(MessageQueueHandle handle, long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, ITransaction transaction);
        [DllImport("mqrt.dll", EntryPoint="MQSetQueueProperties", CharSet=CharSet.Unicode)]
        private static extern int IntMQSetQueueProperties(string formatName, MessagePropertyVariants.MQPROPS queueProperties);
        [DllImport("advapi32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr sid, ref int sidSize, StringBuilder DomainName, ref int DomainSize, out int pUse);
        public static int MQCreateQueue(IntPtr securityDescriptor, MessagePropertyVariants.MQPROPS queueProperties, StringBuilder formatName, ref int formatNameLength)
        {
            int num;
            try
            {
                num = IntMQCreateQueue(securityDescriptor, queueProperties, formatName, ref formatNameLength);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        public static int MQDeleteQueue(string formatName)
        {
            int num;
            try
            {
                num = IntMQDeleteQueue(formatName);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        public static int MQGetMachineProperties(string machineName, IntPtr machineIdPointer, MessagePropertyVariants.MQPROPS machineProperties)
        {
            int num;
            try
            {
                num = IntMQGetMachineProperties(machineName, machineIdPointer, machineProperties);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        public static int MQGetQueueProperties(string formatName, MessagePropertyVariants.MQPROPS queueProperties)
        {
            int num;
            try
            {
                num = IntMQGetQueueProperties(formatName, queueProperties);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [DllImport("mqrt.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int MQGetQueueSecurity(string formatName, int SecurityInformation, IntPtr SecurityDescriptor, int length, out int lengthNeeded);
        public static int MQLocateBegin(string context, Restrictions.MQRESTRICTION Restriction, Columns.MQCOLUMNSET columnSet, out LocatorHandle enumHandle)
        {
            int num;
            try
            {
                num = IntMQLocateBegin(context, Restriction, columnSet, IntPtr.Zero, out enumHandle);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        public static int MQMgmtGetInfo(string machineName, string objectName, MessagePropertyVariants.MQPROPS queueProperties)
        {
            int num;
            try
            {
                num = IntMQMgmtGetInfo(machineName, objectName, queueProperties);
            }
            catch (EntryPointNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQInfoNotSupported"));
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        public static int MQOpenQueue(string formatName, int access, int shareMode, out MessageQueueHandle handle)
        {
            int num;
            try
            {
                num = IntMQOpenQueue(formatName, access, shareMode, out handle);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQPurgeQueue(MessageQueueHandle handle);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(MessageQueueHandle handle, uint timeout, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, CursorHandle cursorHandle, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern unsafe int MQReceiveMessage(MessageQueueHandle handle, uint timeout, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, CursorHandle cursorHandle, ITransaction transaction);
        public static unsafe int MQReceiveMessageByLookupId(MessageQueueHandle handle, long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, IntPtr transaction)
        {
            int num;
            try
            {
                num = IntMQReceiveMessageByLookupId(handle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(Res.GetString("PlatformNotSupported"));
            }
            return num;
        }

        public static unsafe int MQReceiveMessageByLookupId(MessageQueueHandle handle, long lookupId, int action, MessagePropertyVariants.MQPROPS properties, NativeOverlapped* overlapped, SafeNativeMethods.ReceiveCallback receiveCallback, ITransaction transaction)
        {
            int num;
            try
            {
                num = IntMQReceiveMessageByLookupId(handle, lookupId, action, properties, overlapped, receiveCallback, transaction);
            }
            catch (EntryPointNotFoundException)
            {
                throw new PlatformNotSupportedException(Res.GetString("PlatformNotSupported"));
            }
            return num;
        }

        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQSendMessage(MessageQueueHandle handle, MessagePropertyVariants.MQPROPS properties, IntPtr transaction);
        [DllImport("mqrt.dll", CharSet=CharSet.Unicode)]
        public static extern int MQSendMessage(MessageQueueHandle handle, MessagePropertyVariants.MQPROPS properties, ITransaction transaction);
        public static int MQSetQueueProperties(string formatName, MessagePropertyVariants.MQPROPS queueProperties)
        {
            int num;
            try
            {
                num = IntMQSetQueueProperties(formatName, queueProperties);
            }
            catch (DllNotFoundException)
            {
                throw new InvalidOperationException(Res.GetString("MSMQNotInstalled"));
            }
            return num;
        }

        [DllImport("mqrt.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int MQSetQueueSecurity(string formatName, int SecurityInformation, System.Messaging.Interop.NativeMethods.SECURITY_DESCRIPTOR SecurityDescriptor);
        [DllImport("advapi32.dll", SetLastError=true)]
        public static extern bool SetSecurityDescriptorDacl(System.Messaging.Interop.NativeMethods.SECURITY_DESCRIPTOR pSD, bool daclPresent, IntPtr pDacl, bool daclDefaulted);
    }
}

