namespace System.Diagnostics.Eventing.Reader
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    internal class NativeWrapper
    {
        private static bool s_platformNotSupported = (Environment.OSVersion.Version.Major < 6);

        [SecurityCritical]
        public static DateTime ConvertFileTimeToDateTime(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 0x11)
            {
                throw new EventLogInvalidDataException();
            }
            return DateTime.FromFileTime((long) val.FileTime);
        }

        [SecurityCritical]
        public static string ConvertToAnsiString(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 2)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.AnsiString == IntPtr.Zero)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringAuto(val.AnsiString);
        }

        [SecurityCritical]
        public static byte[] ConvertToBinaryArray(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 14)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.Binary == IntPtr.Zero)
            {
                return new byte[0];
            }
            IntPtr binary = val.Binary;
            byte[] destination = new byte[val.Count];
            Marshal.Copy(binary, destination, 0, (int) val.Count);
            return destination;
        }

        [SecurityCritical]
        public static Guid ConvertToGuid(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 15)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.GuidReference == IntPtr.Zero)
            {
                return Guid.Empty;
            }
            return (Guid) Marshal.PtrToStructure(val.GuidReference, typeof(Guid));
        }

        [SecurityCritical]
        public static int[] ConvertToIntArray(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 0x88)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.Reference == IntPtr.Zero)
            {
                return new int[0];
            }
            IntPtr reference = val.Reference;
            int[] destination = new int[val.Count];
            Marshal.Copy(reference, destination, 0, (int) val.Count);
            return destination;
        }

        [SecurityCritical]
        private static object ConvertToObject(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            switch (val.Type)
            {
                case 0:
                    return null;

                case 1:
                    return ConvertToString(val);

                case 2:
                    return ConvertToAnsiString(val);

                case 3:
                    return val.SByte;

                case 4:
                    return val.UInt8;

                case 5:
                    return val.SByte;

                case 6:
                    return val.UShort;

                case 7:
                    return val.Integer;

                case 8:
                    return val.UInteger;

                case 9:
                    return val.Long;

                case 10:
                    return val.ULong;

                case 12:
                    return val.Double;

                case 13:
                    if (val.Bool == 0)
                    {
                        return false;
                    }
                    return true;

                case 14:
                    return ConvertToBinaryArray(val);

                case 15:
                    return ConvertToGuid(val);

                case 0x11:
                    return ConvertFileTimeToDateTime(val);

                case 0x13:
                    return ConvertToSid(val);

                case 20:
                    return val.Integer;

                case 0x15:
                    return val.ULong;

                case 0x20:
                    return ConvertToSafeHandle(val);

                case 0x81:
                    return ConvertToStringArray(val);

                case 0x88:
                    return ConvertToIntArray(val);
            }
            throw new EventLogInvalidDataException();
        }

        [SecurityCritical]
        public static object ConvertToObject(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType desiredType)
        {
            if (val.Type == 0)
            {
                return null;
            }
            if (val.Type != ((long) desiredType))
            {
                throw new EventLogInvalidDataException();
            }
            return ConvertToObject(val);
        }

        [SecurityCritical]
        public static EventLogHandle ConvertToSafeHandle(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 0x20)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.Handle == IntPtr.Zero)
            {
                return EventLogHandle.Zero;
            }
            return new EventLogHandle(val.Handle, true);
        }

        [SecurityCritical]
        public static SecurityIdentifier ConvertToSid(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 0x13)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.SidVal == IntPtr.Zero)
            {
                return null;
            }
            return new SecurityIdentifier(val.SidVal);
        }

        [SecurityCritical]
        public static string ConvertToString(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 1)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.StringVal == IntPtr.Zero)
            {
                return string.Empty;
            }
            return Marshal.PtrToStringAuto(val.StringVal);
        }

        [SecurityCritical]
        public static string[] ConvertToStringArray(Microsoft.Win32.UnsafeNativeMethods.EvtVariant val)
        {
            if (val.Type != 0x81)
            {
                throw new EventLogInvalidDataException();
            }
            if (val.Reference == IntPtr.Zero)
            {
                return new string[0];
            }
            IntPtr reference = val.Reference;
            IntPtr[] destination = new IntPtr[val.Count];
            Marshal.Copy(reference, destination, 0, (int) val.Count);
            string[] strArray = new string[val.Count];
            for (int i = 0; i < val.Count; i++)
            {
                strArray[i] = Marshal.PtrToStringAuto(destination[i]);
            }
            return strArray;
        }

        [SecuritySafeCritical]
        public static void EvtArchiveExportedLog(EventLogHandle session, string logFilePath, int locale, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtArchiveExportedLog(session, logFilePath, locale, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [SecuritySafeCritical]
        public static void EvtCancel(EventLogHandle handle)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            if (!Microsoft.Win32.UnsafeNativeMethods.EvtCancel(handle))
            {
                EventLogException.Throw(Marshal.GetLastWin32Error());
            }
        }

        [SecuritySafeCritical]
        public static void EvtClearLog(EventLogHandle session, string channelPath, string targetFilePath, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtClearLog(session, channelPath, targetFilePath, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [SecurityCritical]
        public static void EvtClose(IntPtr handle)
        {
            Microsoft.Win32.UnsafeNativeMethods.EvtClose(handle);
        }

        [SecurityCritical]
        public static EventLogHandle EvtCreateBookmark(string bookmarkXml)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtCreateBookmark(bookmarkXml);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtCreateRenderContext(int valuePathsCount, string[] valuePaths, Microsoft.Win32.UnsafeNativeMethods.EvtRenderContextFlags flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtCreateRenderContext(valuePathsCount, valuePaths, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecuritySafeCritical]
        public static void EvtExportLog(EventLogHandle session, string channelPath, string query, string targetFilePath, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtExportLog(session, channelPath, query, targetFilePath, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [SecurityCritical]
        public static string EvtFormatMessage(EventLogHandle handle, uint msgId)
        {
            int num;
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            StringBuilder buffer = new StringBuilder(null);
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(handle, EventLogHandle.Zero, msgId, 0, null, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageId, 0, buffer, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag && (errorCode != 0x3ab5))
            {
                if (errorCode == 0x3ab3)
                {
                    return null;
                }
                if (errorCode != 0x7a)
                {
                    EventLogException.Throw(errorCode);
                }
            }
            buffer.EnsureCapacity(num);
            flag = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(handle, EventLogHandle.Zero, msgId, 0, null, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageId, num, buffer, out num);
            errorCode = Marshal.GetLastWin32Error();
            if (!flag && (errorCode != 0x3ab5))
            {
                if (errorCode == 0x3ab3)
                {
                    return null;
                }
                if (errorCode == 0x3ab5)
                {
                    return null;
                }
                EventLogException.Throw(errorCode);
            }
            return buffer.ToString();
        }

        [SecuritySafeCritical]
        public static string EvtFormatMessageFormatDescription(EventLogHandle handle, EventLogHandle eventHandle, string[] values)
        {
            int num;
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            Microsoft.Win32.UnsafeNativeMethods.EvtStringVariant[] variantArray = new Microsoft.Win32.UnsafeNativeMethods.EvtStringVariant[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                variantArray[i].Type = 1;
                variantArray[i].StringVal = values[i];
            }
            StringBuilder buffer = new StringBuilder(null);
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(handle, eventHandle, uint.MaxValue, values.Length, variantArray, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageEvent, 0, buffer, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag && (errorCode != 0x3ab5))
            {
                switch (errorCode)
                {
                    case 0x3ab9:
                    case 0x3afc:
                    case 0x3ab3:
                    case 0x3ab4:
                    case 0x717:
                        return null;
                }
                if (errorCode != 0x7a)
                {
                    EventLogException.Throw(errorCode);
                }
            }
            buffer.EnsureCapacity(num);
            flag = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(handle, eventHandle, uint.MaxValue, values.Length, variantArray, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags.EvtFormatMessageEvent, num, buffer, out num);
            errorCode = Marshal.GetLastWin32Error();
            if (!flag && (errorCode != 0x3ab5))
            {
                if (errorCode == 0x3ab3)
                {
                    return null;
                }
                EventLogException.Throw(errorCode);
            }
            return buffer.ToString();
        }

        [SecuritySafeCritical]
        public static IEnumerable<string> EvtFormatMessageRenderKeywords(EventLogHandle pmHandle, EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags flag)
        {
            IEnumerable<string> enumerable;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            IntPtr zero = IntPtr.Zero;
            try
            {
                int num;
                List<string> list = new List<string>();
                bool flag2 = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageBuffer(pmHandle, eventHandle, 0, 0, IntPtr.Zero, flag, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag2)
                {
                    switch (errorCode)
                    {
                        case 0x3ab9:
                        case 0x3afc:
                        case 0x3ab3:
                        case 0x3ab4:
                        case 0x717:
                            return list.AsReadOnly();
                    }
                    if (errorCode != 0x7a)
                    {
                        EventLogException.Throw(errorCode);
                    }
                }
                zero = Marshal.AllocHGlobal((int) (num * 2));
                flag2 = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageBuffer(pmHandle, eventHandle, 0, 0, IntPtr.Zero, flag, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag2)
                {
                    switch (errorCode)
                    {
                        case 0x3ab9:
                        case 0x3afc:
                            return list;

                        case 0x3ab3:
                        case 0x3ab4:
                            return list;

                        case 0x717:
                            return list;
                    }
                    EventLogException.Throw(errorCode);
                }
                IntPtr ptr = zero;
                while (true)
                {
                    string str = Marshal.PtrToStringAuto(ptr);
                    if (string.IsNullOrEmpty(str))
                    {
                        break;
                    }
                    list.Add(str);
                    ptr = new IntPtr((((long) ptr) + (str.Length * 2)) + 2L);
                }
                enumerable = list.AsReadOnly();
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return enumerable;
        }

        [SecuritySafeCritical]
        public static string EvtFormatMessageRenderName(EventLogHandle pmHandle, EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessageFlags flag)
        {
            int num;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            StringBuilder buffer = new StringBuilder(null);
            bool flag2 = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(pmHandle, eventHandle, 0, 0, null, flag, 0, buffer, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag2 && (errorCode != 0x3ab5))
            {
                switch (errorCode)
                {
                    case 0x3ab9:
                    case 0x3afc:
                    case 0x3ab3:
                    case 0x3ab4:
                    case 0x717:
                        return null;
                }
                if (errorCode != 0x7a)
                {
                    EventLogException.Throw(errorCode);
                }
            }
            buffer.EnsureCapacity(num);
            flag2 = Microsoft.Win32.UnsafeNativeMethods.EvtFormatMessage(pmHandle, eventHandle, 0, 0, null, flag, num, buffer, out num);
            errorCode = Marshal.GetLastWin32Error();
            if (!flag2 && (errorCode != 0x3ab5))
            {
                switch (errorCode)
                {
                    case 0x3ab9:
                    case 0x3afc:
                    case 0x3ab3:
                    case 0x3ab4:
                    case 0x717:
                        return null;
                }
                EventLogException.Throw(errorCode);
            }
            return buffer.ToString();
        }

        [SecuritySafeCritical]
        public static object EvtGetChannelConfigProperty(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId enumType)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetChannelConfigProperty(handle, enumType, 0, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetChannelConfigProperty(handle, enumType, 0, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecuritySafeCritical]
        public static object EvtGetEventInfo(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtEventPropertyId enumType)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetEventInfo(handle, enumType, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if ((!flag && (errorCode != 0)) && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetEventInfo(handle, enumType, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        public static object EvtGetEventMetadataProperty(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtEventMetadataPropertyId enumType)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetEventMetadataProperty(handle, enumType, 0, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetEventMetadataProperty(handle, enumType, 0, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        public static object EvtGetLogInfo(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtLogPropertyId enumType)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetLogInfo(handle, enumType, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetLogInfo(handle, enumType, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        public static object EvtGetObjectArrayProperty(EventLogHandle objArrayHandle, int index, int thePropertyId)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetObjectArrayProperty(objArrayHandle, thePropertyId, index, 0, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetObjectArrayProperty(objArrayHandle, thePropertyId, index, 0, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        public static int EvtGetObjectArraySize(EventLogHandle objectArray)
        {
            int num;
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetObjectArraySize(objectArray, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
            return num;
        }

        [SecuritySafeCritical]
        public static object EvtGetPublisherMetadataProperty(EventLogHandle pmHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId thePropertyId)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetPublisherMetadataProperty(pmHandle, thePropertyId, 0, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetPublisherMetadataProperty(pmHandle, thePropertyId, 0, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        internal static EventLogHandle EvtGetPublisherMetadataPropertyHandle(EventLogHandle pmHandle, Microsoft.Win32.UnsafeNativeMethods.EvtPublisherMetadataPropertyId thePropertyId)
        {
            EventLogHandle handle;
            IntPtr zero = IntPtr.Zero;
            try
            {
                int num;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetPublisherMetadataProperty(pmHandle, thePropertyId, 0, 0, IntPtr.Zero, out num);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetPublisherMetadataProperty(pmHandle, thePropertyId, 0, num, zero, out num);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                handle = ConvertToSafeHandle(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return handle;
        }

        [SecurityCritical]
        public static object EvtGetQueryInfo(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtQueryPropertyId enumType)
        {
            object obj2;
            IntPtr zero = IntPtr.Zero;
            int bufferRequired = 0;
            try
            {
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetQueryInfo(handle, enumType, 0, IntPtr.Zero, ref bufferRequired);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(bufferRequired);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtGetQueryInfo(handle, enumType, bufferRequired, zero, ref bufferRequired);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(zero, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                obj2 = ConvertToObject(val);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return obj2;
        }

        [SecurityCritical]
        public static bool EvtNext(EventLogHandle queryHandle, int eventSize, IntPtr[] events, int timeout, int flags, ref int returned)
        {
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtNext(queryHandle, eventSize, events, timeout, flags, ref returned);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag && (errorCode != 0x103))
            {
                EventLogException.Throw(errorCode);
            }
            return (errorCode == 0);
        }

        [SecurityCritical]
        public static string EvtNextChannelPath(EventLogHandle handle, ref bool finish)
        {
            int num;
            StringBuilder channelPathBuffer = new StringBuilder(null);
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtNextChannelPath(handle, 0, channelPathBuffer, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                if (errorCode == 0x103)
                {
                    finish = true;
                    return null;
                }
                if (errorCode != 0x7a)
                {
                    EventLogException.Throw(errorCode);
                }
            }
            channelPathBuffer.EnsureCapacity(num);
            flag = Microsoft.Win32.UnsafeNativeMethods.EvtNextChannelPath(handle, num, channelPathBuffer, out num);
            errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
            return channelPathBuffer.ToString();
        }

        [SecurityCritical]
        public static EventLogHandle EvtNextEventMetadata(EventLogHandle eventMetadataEnum, int flags)
        {
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtNextEventMetadata(eventMetadataEnum, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!handle.IsInvalid)
            {
                return handle;
            }
            if (errorCode != 0x103)
            {
                EventLogException.Throw(errorCode);
            }
            return null;
        }

        [SecurityCritical]
        public static string EvtNextPublisherId(EventLogHandle handle, ref bool finish)
        {
            int num;
            StringBuilder publisherIdBuffer = new StringBuilder(null);
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtNextPublisherId(handle, 0, publisherIdBuffer, out num);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                if (errorCode == 0x103)
                {
                    finish = true;
                    return null;
                }
                if (errorCode != 0x7a)
                {
                    EventLogException.Throw(errorCode);
                }
            }
            publisherIdBuffer.EnsureCapacity(num);
            flag = Microsoft.Win32.UnsafeNativeMethods.EvtNextPublisherId(handle, num, publisherIdBuffer, out num);
            errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
            return publisherIdBuffer.ToString();
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenChannelConfig(EventLogHandle session, string channelPath, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenChannelConfig(session, channelPath, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenChannelEnum(EventLogHandle session, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenChannelEnum(session, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenEventMetadataEnum(EventLogHandle ProviderMetadata, int flags)
        {
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenEventMetadataEnum(ProviderMetadata, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenLog(EventLogHandle session, string path, PathType flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenLog(session, path, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenProviderEnum(EventLogHandle session, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenPublisherEnum(session, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenProviderMetadata(EventLogHandle session, string ProviderId, string logFilePath, int locale, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenPublisherMetadata(session, ProviderId, logFilePath, locale, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtOpenSession(Microsoft.Win32.UnsafeNativeMethods.EvtLoginClass loginClass, ref Microsoft.Win32.UnsafeNativeMethods.EvtRpcLogin login, int timeout, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtOpenSession(loginClass, ref login, timeout, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static EventLogHandle EvtQuery(EventLogHandle session, string path, string query, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtQuery(session, path, query, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static void EvtRender(EventLogHandle context, EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags flags, StringBuilder buffer)
        {
            int num;
            int num2;
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtRender(context, eventHandle, flags, buffer.Capacity, buffer, out num, out num2);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                if (errorCode == 0x7a)
                {
                    buffer.Capacity = num;
                    flag = Microsoft.Win32.UnsafeNativeMethods.EvtRender(context, eventHandle, flags, buffer.Capacity, buffer, out num, out num2);
                    errorCode = Marshal.GetLastWin32Error();
                }
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
            }
        }

        [SecurityCritical]
        public static string EvtRenderBookmark(EventLogHandle eventHandle)
        {
            string str;
            IntPtr zero = IntPtr.Zero;
            Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags evtRenderBookmark = Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags.EvtRenderBookmark;
            try
            {
                int num;
                int num2;
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtRender(EventLogHandle.Zero, eventHandle, evtRenderBookmark, 0, IntPtr.Zero, out num, out num2);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag && (errorCode != 0x7a))
                {
                    EventLogException.Throw(errorCode);
                }
                zero = Marshal.AllocHGlobal(num);
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtRender(EventLogHandle.Zero, eventHandle, evtRenderBookmark, num, zero, out num, out num2);
                errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                str = Marshal.PtrToStringAuto(zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return str;
        }

        [SecuritySafeCritical]
        public static void EvtRenderBufferWithContextSystem(EventLogHandle contextHandle, EventLogHandle eventHandle, Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags flag, SystemProperties systemProperties, int SYSTEM_PROPERTY_COUNT)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                int num;
                int num2;
                if (!Microsoft.Win32.UnsafeNativeMethods.EvtRender(contextHandle, eventHandle, flag, 0, IntPtr.Zero, out num, out num2))
                {
                    int num3 = Marshal.GetLastWin32Error();
                    if (num3 != 0x7a)
                    {
                        EventLogException.Throw(num3);
                    }
                }
                zero = Marshal.AllocHGlobal(num);
                bool flag2 = Microsoft.Win32.UnsafeNativeMethods.EvtRender(contextHandle, eventHandle, flag, num, zero, out num, out num2);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag2)
                {
                    EventLogException.Throw(errorCode);
                }
                if (num2 != SYSTEM_PROPERTY_COUNT)
                {
                    throw new InvalidOperationException("We do not have " + SYSTEM_PROPERTY_COUNT + " variants given for the  UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues flag. (System Properties)");
                }
                ptr = zero;
                for (int i = 0; i < num2; i++)
                {
                    Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(ptr, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                    switch (i)
                    {
                        case 0:
                            systemProperties.ProviderName = (string) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeString);
                            break;

                        case 1:
                            systemProperties.ProviderId = (Guid?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeGuid);
                            break;

                        case 2:
                            systemProperties.Id = (ushort?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt16);
                            break;

                        case 3:
                            systemProperties.Qualifiers = (ushort?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt16);
                            break;

                        case 4:
                            systemProperties.Level = (byte?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeByte);
                            break;

                        case 5:
                            systemProperties.Task = (ushort?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt16);
                            break;

                        case 6:
                            systemProperties.Opcode = (byte?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeByte);
                            break;

                        case 7:
                            systemProperties.Keywords = (ulong?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeHexInt64);
                            break;

                        case 8:
                            systemProperties.TimeCreated = (DateTime?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeFileTime);
                            break;

                        case 9:
                            systemProperties.RecordId = (ulong?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt64);
                            break;

                        case 10:
                            systemProperties.ActivityId = (Guid?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeGuid);
                            break;

                        case 11:
                            systemProperties.RelatedActivityId = (Guid?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeGuid);
                            break;

                        case 12:
                            systemProperties.ProcessId = (uint?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt32);
                            break;

                        case 13:
                            systemProperties.ThreadId = (uint?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeUInt32);
                            break;

                        case 14:
                            systemProperties.ChannelName = (string) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeString);
                            break;

                        case 15:
                            systemProperties.ComputerName = (string) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeString);
                            break;

                        case 0x10:
                            systemProperties.UserId = (SecurityIdentifier) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeSid);
                            break;

                        case 0x11:
                            systemProperties.Version = (byte?) ConvertToObject(val, Microsoft.Win32.UnsafeNativeMethods.EvtVariantType.EvtVarTypeByte);
                            break;
                    }
                    ptr = new IntPtr(((long) ptr) + Marshal.SizeOf(val));
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
        }

        [SecuritySafeCritical]
        public static IList<object> EvtRenderBufferWithContextUserOrValues(EventLogHandle contextHandle, EventLogHandle eventHandle)
        {
            IList<object> list2;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags evtRenderEventValues = Microsoft.Win32.UnsafeNativeMethods.EvtRenderFlags.EvtRenderEventValues;
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            try
            {
                int num;
                int num2;
                if (!Microsoft.Win32.UnsafeNativeMethods.EvtRender(contextHandle, eventHandle, evtRenderEventValues, 0, IntPtr.Zero, out num, out num2))
                {
                    int num3 = Marshal.GetLastWin32Error();
                    if (num3 != 0x7a)
                    {
                        EventLogException.Throw(num3);
                    }
                }
                zero = Marshal.AllocHGlobal(num);
                bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtRender(contextHandle, eventHandle, evtRenderEventValues, num, zero, out num, out num2);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
                List<object> list = new List<object>(num2);
                if (num2 > 0)
                {
                    ptr = zero;
                    for (int i = 0; i < num2; i++)
                    {
                        Microsoft.Win32.UnsafeNativeMethods.EvtVariant val = (Microsoft.Win32.UnsafeNativeMethods.EvtVariant) Marshal.PtrToStructure(ptr, typeof(Microsoft.Win32.UnsafeNativeMethods.EvtVariant));
                        list.Add(ConvertToObject(val));
                        ptr = new IntPtr(((long) ptr) + Marshal.SizeOf(val));
                    }
                }
                list2 = list;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
            }
            return list2;
        }

        [SecuritySafeCritical]
        public static void EvtSaveChannelConfig(EventLogHandle channelConfig, int flags)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtSaveChannelConfig(channelConfig, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [SecurityCritical]
        public static void EvtSeek(EventLogHandle resultSet, long position, EventLogHandle bookmark, int timeout, Microsoft.Win32.UnsafeNativeMethods.EvtSeekFlags flags)
        {
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtSeek(resultSet, position, bookmark, timeout, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [SecuritySafeCritical]
        public static void EvtSetChannelConfigProperty(EventLogHandle handle, Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId enumType, object val)
        {
            EventLogPermissionHolder.GetEventLogPermission().Demand();
            Microsoft.Win32.UnsafeNativeMethods.EvtVariant propertyValue = new Microsoft.Win32.UnsafeNativeMethods.EvtVariant();
            CoTaskMemSafeHandle handle2 = new CoTaskMemSafeHandle();
            using (handle2)
            {
                bool flag;
                if (val == null)
                {
                    goto Label_017B;
                }
                switch (enumType)
                {
                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigEnabled:
                        propertyValue.Type = 13;
                        if (!((bool) val))
                        {
                            break;
                        }
                        propertyValue.Bool = 1;
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelConfigAccess:
                        propertyValue.Type = 1;
                        handle2.SetMemory(Marshal.StringToCoTaskMemAuto((string) val));
                        propertyValue.StringVal = handle2.GetMemory();
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigRetention:
                        propertyValue.Type = 13;
                        if (!((bool) val))
                        {
                            goto Label_0146;
                        }
                        propertyValue.Bool = 1;
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigAutoBackup:
                        propertyValue.Type = 13;
                        if (!((bool) val))
                        {
                            goto Label_016B;
                        }
                        propertyValue.Bool = 1;
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigMaxSize:
                        propertyValue.Type = 10;
                        propertyValue.ULong = (ulong) ((long) val);
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelLoggingConfigLogFilePath:
                        propertyValue.Type = 1;
                        handle2.SetMemory(Marshal.StringToCoTaskMemAuto((string) val));
                        propertyValue.StringVal = handle2.GetMemory();
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigLevel:
                        propertyValue.Type = 8;
                        propertyValue.UInteger = (uint) ((int) val);
                        goto Label_0183;

                    case Microsoft.Win32.UnsafeNativeMethods.EvtChannelConfigPropertyId.EvtChannelPublishingConfigKeywords:
                        propertyValue.Type = 10;
                        propertyValue.ULong = (ulong) ((long) val);
                        goto Label_0183;

                    default:
                        throw new InvalidOperationException();
                }
                propertyValue.Bool = 0;
                goto Label_0183;
            Label_0146:
                propertyValue.Bool = 0;
                goto Label_0183;
            Label_016B:
                propertyValue.Bool = 0;
                goto Label_0183;
            Label_017B:
                propertyValue.Type = 0;
            Label_0183:
                flag = Microsoft.Win32.UnsafeNativeMethods.EvtSetChannelConfigProperty(handle, enumType, 0, ref propertyValue);
                int errorCode = Marshal.GetLastWin32Error();
                if (!flag)
                {
                    EventLogException.Throw(errorCode);
                }
            }
        }

        [SecurityCritical]
        public static EventLogHandle EvtSubscribe(EventLogHandle session, SafeWaitHandle signalEvent, string path, string query, EventLogHandle bookmark, IntPtr context, IntPtr callback, int flags)
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException();
            }
            EventLogHandle handle = Microsoft.Win32.UnsafeNativeMethods.EvtSubscribe(session, signalEvent, path, query, bookmark, context, callback, flags);
            int errorCode = Marshal.GetLastWin32Error();
            if (handle.IsInvalid)
            {
                EventLogException.Throw(errorCode);
            }
            return handle;
        }

        [SecurityCritical]
        public static void EvtUpdateBookmark(EventLogHandle bookmark, EventLogHandle eventHandle)
        {
            bool flag = Microsoft.Win32.UnsafeNativeMethods.EvtUpdateBookmark(bookmark, eventHandle);
            int errorCode = Marshal.GetLastWin32Error();
            if (!flag)
            {
                EventLogException.Throw(errorCode);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
        public class SystemProperties
        {
            public Guid? ActivityId = null;
            public string ChannelName;
            public string ComputerName;
            public bool filled;
            public ushort? Id = null;
            public ulong? Keywords = null;
            public byte? Level = null;
            public byte? Opcode = null;
            public uint? ProcessId = null;
            public Guid? ProviderId = null;
            public string ProviderName;
            public ushort? Qualifiers = null;
            public ulong? RecordId = null;
            public Guid? RelatedActivityId = null;
            public ushort? Task = null;
            public uint? ThreadId = null;
            public DateTime? TimeCreated = null;
            public SecurityIdentifier UserId;
            public byte? Version = null;
        }
    }
}

