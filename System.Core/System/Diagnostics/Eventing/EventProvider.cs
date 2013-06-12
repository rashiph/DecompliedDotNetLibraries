namespace System.Diagnostics.Eventing
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class EventProvider : IDisposable
    {
        private long m_allKeywordMask;
        private long m_anyKeywordMask;
        private int m_disposed;
        private int m_enabled;
        private Microsoft.Win32.UnsafeNativeMethods.EtwEnableCallback m_etwCallback;
        private byte m_level;
        private Guid m_providerId;
        private long m_regHandle;
        private const int s_basicTypeAllocationBufferSize = 0x10;
        private const int s_etwAPIMaxStringCount = 8;
        private const int s_etwMaxMumberArguments = 0x20;
        private const int s_maxEventDataDescriptors = 0x80;
        private static bool s_platformNotSupported = (Environment.OSVersion.Version.Major < 6);
        private static bool s_preWin7 = ((Environment.OSVersion.Version.Major < 6) || ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor < 1)));
        private static LocalDataStoreSlot s_returnCodeSlot;
        private const int s_traceEventMaximumSize = 0xffca;
        private const int s_traceEventMaximumStringSize = 0x7fd4;

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
        public EventProvider(Guid providerGuid)
        {
            this.m_providerId = providerGuid;
            s_returnCodeSlot = Thread.AllocateDataSlot();
            Thread.SetData(s_returnCodeSlot, 0);
            this.EtwRegister();
        }

        public virtual void Close()
        {
            this.Dispose();
        }

        [SecurityCritical]
        public static Guid CreateActivityId()
        {
            Guid activityId = new Guid();
            Microsoft.Win32.UnsafeNativeMethods.EventActivityIdControl(3, ref activityId);
            return activityId;
        }

        [SecurityCritical]
        private void Deregister()
        {
            if (this.m_regHandle != 0L)
            {
                Microsoft.Win32.UnsafeNativeMethods.EventUnregister(this.m_regHandle);
                this.m_regHandle = 0L;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            if ((this.m_disposed != 1) && (Interlocked.Exchange(ref this.m_disposed, 1) == 0))
            {
                this.m_enabled = 0;
                this.Deregister();
            }
        }

        [SecurityCritical]
        private static unsafe string EncodeObject(ref object data, EventData* dataDescriptor, byte* dataBuffer)
        {
            dataDescriptor.Reserved = 0;
            string str = data as string;
            if (str != null)
            {
                dataDescriptor.Size = (uint) ((str.Length + 1) * 2);
                return str;
            }
            if (data == null)
            {
                dataDescriptor.Size = 0;
                dataDescriptor.DataPointer = 0L;
            }
            else if (data is IntPtr)
            {
                dataDescriptor.Size = (uint) sizeof(IntPtr);
                IntPtr* ptrPtr = (IntPtr*) dataBuffer;
                ptrPtr[0] = (IntPtr) data;
                dataDescriptor.DataPointer = (ulong) ptrPtr;
            }
            else if (data is int)
            {
                dataDescriptor.Size = 4;
                int* numPtr = (int*) dataBuffer;
                numPtr[0] = (int) data;
                dataDescriptor.DataPointer = (ulong) numPtr;
            }
            else if (data is long)
            {
                dataDescriptor.Size = 8;
                long* numPtr2 = (long*) dataBuffer;
                numPtr2[0] = (long) data;
                dataDescriptor.DataPointer = (ulong) numPtr2;
            }
            else if (data is uint)
            {
                dataDescriptor.Size = 4;
                uint* numPtr3 = (uint*) dataBuffer;
                numPtr3[0] = (uint) data;
                dataDescriptor.DataPointer = (ulong) numPtr3;
            }
            else if (data is ulong)
            {
                dataDescriptor.Size = 8;
                ulong* numPtr4 = (ulong*) dataBuffer;
                numPtr4[0] = (ulong) data;
                dataDescriptor.DataPointer = (ulong) numPtr4;
            }
            else if (data is char)
            {
                dataDescriptor.Size = 2;
                char* chPtr = (char*) dataBuffer;
                chPtr[0] = (char) data;
                dataDescriptor.DataPointer = (ulong) chPtr;
            }
            else if (data is byte)
            {
                dataDescriptor.Size = 1;
                byte* numPtr5 = dataBuffer;
                numPtr5[0] = (byte) data;
                dataDescriptor.DataPointer = (ulong) numPtr5;
            }
            else if (data is short)
            {
                dataDescriptor.Size = 2;
                short* numPtr6 = (short*) dataBuffer;
                numPtr6[0] = (short) data;
                dataDescriptor.DataPointer = (ulong) numPtr6;
            }
            else if (data is sbyte)
            {
                dataDescriptor.Size = 1;
                sbyte* numPtr7 = (sbyte*) dataBuffer;
                numPtr7[0] = (sbyte) data;
                dataDescriptor.DataPointer = (ulong) numPtr7;
            }
            else if (data is ushort)
            {
                dataDescriptor.Size = 2;
                ushort* numPtr8 = (ushort*) dataBuffer;
                numPtr8[0] = (ushort) data;
                dataDescriptor.DataPointer = (ulong) numPtr8;
            }
            else if (data is float)
            {
                dataDescriptor.Size = 4;
                float* numPtr9 = (float*) dataBuffer;
                numPtr9[0] = (float) data;
                dataDescriptor.DataPointer = (ulong) numPtr9;
            }
            else if (data is double)
            {
                dataDescriptor.Size = 8;
                double* numPtr10 = (double*) dataBuffer;
                numPtr10[0] = (double) data;
                dataDescriptor.DataPointer = (ulong) numPtr10;
            }
            else if (data is bool)
            {
                dataDescriptor.Size = 1;
                bool* flagPtr = (bool*) dataBuffer;
                *((sbyte*) flagPtr) = (bool) data;
                dataDescriptor.DataPointer = (ulong) flagPtr;
            }
            else if (data is Guid)
            {
                dataDescriptor.Size = (uint) sizeof(Guid);
                Guid* guidPtr = (Guid*) dataBuffer;
                guidPtr[0] = (Guid) data;
                dataDescriptor.DataPointer = (ulong) guidPtr;
            }
            else if (data is decimal)
            {
                dataDescriptor.Size = 0x10;
                decimal* numPtr11 = (decimal*) dataBuffer;
                numPtr11[0] = (decimal) data;
                dataDescriptor.DataPointer = (ulong) numPtr11;
            }
            else if (data is bool)
            {
                dataDescriptor.Size = 1;
                bool* flagPtr2 = (bool*) dataBuffer;
                *((sbyte*) flagPtr2) = (bool) data;
                dataDescriptor.DataPointer = (ulong) flagPtr2;
            }
            else
            {
                str = data.ToString();
                dataDescriptor.Size = (uint) ((str.Length + 1) * 2);
                return str;
            }
            return null;
        }

        [SecurityCritical]
        private unsafe void EtwEnableCallBack([In] ref Guid sourceId, [In] int isEnabled, [In] byte setLevel, [In] long anyKeyword, [In] long allKeyword, [In] void* filterData, [In] void* callbackContext)
        {
            this.m_enabled = isEnabled;
            this.m_level = setLevel;
            this.m_anyKeywordMask = anyKeyword;
            this.m_allKeywordMask = allKeyword;
        }

        [SecurityCritical]
        private void EtwRegister()
        {
            if (s_platformNotSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("NotSupported_DownLevelVista"));
            }
            this.m_etwCallback = new Microsoft.Win32.UnsafeNativeMethods.EtwEnableCallback(this.EtwEnableCallBack);
            uint num = Microsoft.Win32.UnsafeNativeMethods.EventRegister(ref this.m_providerId, this.m_etwCallback, null, ref this.m_regHandle);
            if (num != 0)
            {
                throw new Win32Exception((int) num);
            }
        }

        ~EventProvider()
        {
            this.Dispose(false);
        }

        [SecurityCritical]
        private static Guid GetActivityId()
        {
            return Trace.CorrelationManager.ActivityId;
        }

        public static WriteEventErrorCode GetLastWriteEventError()
        {
            object data = Thread.GetData(s_returnCodeSlot);
            if (data == null)
            {
                return WriteEventErrorCode.NoError;
            }
            return (WriteEventErrorCode) ((int) data);
        }

        public bool IsEnabled()
        {
            if (this.m_enabled == 0)
            {
                return false;
            }
            return true;
        }

        public bool IsEnabled(byte level, long keywords)
        {
            if (this.m_enabled == 0)
            {
                return false;
            }
            return (((level <= this.m_level) || (this.m_level == 0)) && ((keywords == 0L) || (((keywords & this.m_anyKeywordMask) != 0L) && ((keywords & this.m_allKeywordMask) == this.m_allKeywordMask))));
        }

        [SecurityCritical]
        public static void SetActivityId(ref Guid id)
        {
            Trace.CorrelationManager.ActivityId = id;
            Microsoft.Win32.UnsafeNativeMethods.EventActivityIdControl(2, ref id);
        }

        private static void SetLastError(int error)
        {
            switch (error)
            {
                case 8:
                    Thread.SetData(s_returnCodeSlot, 1);
                    break;

                case 0xea:
                case 0x216:
                    Thread.SetData(s_returnCodeSlot, 2);
                    break;
            }
        }

        public bool WriteEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, params object[] eventPayload)
        {
            return this.WriteTransferEvent(ref eventDescriptor, Guid.Empty, eventPayload);
        }

        [SecurityCritical]
        public unsafe bool WriteEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, string data)
        {
            uint num = 0;
            if (data == null)
            {
                throw new ArgumentNullException("dataString");
            }
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                EventData data2;
                if (data.Length > 0x7fd4)
                {
                    Thread.SetData(s_returnCodeSlot, 2);
                    return false;
                }
                data2.Size = (uint) ((data.Length + 1) * 2);
                data2.Reserved = 0;
                fixed (char* str = ((char*) data))
                {
                    char* chPtr = str;
                    Guid activityId = GetActivityId();
                    data2.DataPointer = (ulong) chPtr;
                    if (s_preWin7)
                    {
                        num = Microsoft.Win32.UnsafeNativeMethods.EventWrite(this.m_regHandle, ref eventDescriptor, 1, (void*) &data2);
                    }
                    else
                    {
                        num = Microsoft.Win32.UnsafeNativeMethods.EventWriteTransfer(this.m_regHandle, ref eventDescriptor, (activityId == Guid.Empty) ? null : &activityId, null, 1, (void*) &data2);
                    }
                }
            }
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        [SecurityCritical]
        protected unsafe bool WriteEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, int dataCount, IntPtr data)
        {
            uint num = 0;
            if (s_preWin7)
            {
                num = Microsoft.Win32.UnsafeNativeMethods.EventWrite(this.m_regHandle, ref eventDescriptor, (uint) dataCount, (void*) data);
            }
            else
            {
                Guid activityId = GetActivityId();
                num = Microsoft.Win32.UnsafeNativeMethods.EventWriteTransfer(this.m_regHandle, ref eventDescriptor, (activityId == Guid.Empty) ? null : &activityId, null, (uint) dataCount, (void*) data);
            }
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        public bool WriteMessageEvent(string eventMessage)
        {
            return this.WriteMessageEvent(eventMessage, 0, 0L);
        }

        [SecurityCritical]
        public unsafe bool WriteMessageEvent(string eventMessage, byte eventLevel, long eventKeywords)
        {
            int error = 0;
            if (eventMessage == null)
            {
                throw new ArgumentNullException("eventMessage");
            }
            if (this.IsEnabled(eventLevel, eventKeywords))
            {
                if (eventMessage.Length > 0x7fd4)
                {
                    Thread.SetData(s_returnCodeSlot, 2);
                    return false;
                }
                fixed (char* str = ((char*) eventMessage))
                {
                    char* message = str;
                    error = (int) Microsoft.Win32.UnsafeNativeMethods.EventWriteString(this.m_regHandle, eventLevel, eventKeywords, message);
                }
                if (error != 0)
                {
                    SetLastError(error);
                    return false;
                }
            }
            return true;
        }

        [SecurityCritical]
        public unsafe bool WriteTransferEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, Guid relatedActivityId, params object[] eventPayload)
        {
            uint num = 0;
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                Guid activityId = GetActivityId();
                int length = 0;
                EventData* dataDescriptor = null;
                if ((eventPayload != null) && (eventPayload.Length != 0))
                {
                    length = eventPayload.Length;
                    if (length > 0x20)
                    {
                        throw new ArgumentOutOfRangeException("eventPayload", System.SR.GetString("ArgumentOutOfRange_MaxArgExceeded", new object[] { 0x20 }));
                    }
                    uint num3 = 0;
                    int index = 0;
                    int[] numArray = new int[8];
                    string[] strArray = new string[8];
                    EventData* dataPtr2 = (EventData*) stackalloc byte[(((IntPtr) length) * sizeof(EventData))];
                    dataDescriptor = dataPtr2;
                    byte* dataBuffer = stackalloc byte[(IntPtr) (0x10 * length)];
                    for (int i = 0; i < eventPayload.Length; i++)
                    {
                        string str = EncodeObject(ref eventPayload[i], dataDescriptor, dataBuffer);
                        dataBuffer += 0x10;
                        num3 += dataDescriptor->Size;
                        dataDescriptor++;
                        if (str != null)
                        {
                            if (index >= 8)
                            {
                                throw new ArgumentOutOfRangeException("eventPayload", System.SR.GetString("ArgumentOutOfRange_MaxStringsExceeded", new object[] { 8 }));
                            }
                            strArray[index] = str;
                            numArray[index] = i;
                            index++;
                        }
                    }
                    if (num3 > 0xffca)
                    {
                        Thread.SetData(s_returnCodeSlot, 2);
                        return false;
                    }
                    fixed (char* str2 = ((char*) strArray[0]))
                    {
                        char* chPtr = str2;
                        fixed (char* str3 = ((char*) strArray[1]))
                        {
                            char* chPtr2 = str3;
                            fixed (char* str4 = ((char*) strArray[2]))
                            {
                                char* chPtr3 = str4;
                                fixed (char* str5 = ((char*) strArray[3]))
                                {
                                    char* chPtr4 = str5;
                                    fixed (char* str6 = ((char*) strArray[4]))
                                    {
                                        char* chPtr5 = str6;
                                        fixed (char* str7 = ((char*) strArray[5]))
                                        {
                                            char* chPtr6 = str7;
                                            fixed (char* str8 = ((char*) strArray[6]))
                                            {
                                                char* chPtr7 = str8;
                                                fixed (char* str9 = ((char*) strArray[7]))
                                                {
                                                    char* chPtr8 = str9;
                                                    dataDescriptor = dataPtr2;
                                                    if (strArray[0] != null)
                                                    {
                                                        dataDescriptor[numArray[0]].DataPointer = (ulong) chPtr;
                                                    }
                                                    if (strArray[1] != null)
                                                    {
                                                        dataDescriptor[numArray[1]].DataPointer = (ulong) chPtr2;
                                                    }
                                                    if (strArray[2] != null)
                                                    {
                                                        dataDescriptor[numArray[2]].DataPointer = (ulong) chPtr3;
                                                    }
                                                    if (strArray[3] != null)
                                                    {
                                                        dataDescriptor[numArray[3]].DataPointer = (ulong) chPtr4;
                                                    }
                                                    if (strArray[4] != null)
                                                    {
                                                        dataDescriptor[numArray[4]].DataPointer = (ulong) chPtr5;
                                                    }
                                                    if (strArray[5] != null)
                                                    {
                                                        dataDescriptor[numArray[5]].DataPointer = (ulong) chPtr6;
                                                    }
                                                    if (strArray[6] != null)
                                                    {
                                                        dataDescriptor[numArray[6]].DataPointer = (ulong) chPtr7;
                                                    }
                                                    if (strArray[7] != null)
                                                    {
                                                        dataDescriptor[numArray[7]].DataPointer = (ulong) chPtr8;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    str4 = null;
                    str5 = null;
                    str6 = null;
                    str7 = null;
                    str8 = null;
                    str9 = null;
                }
                if ((relatedActivityId == Guid.Empty) && s_preWin7)
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.EventWrite(this.m_regHandle, ref eventDescriptor, (uint) length, (void*) dataDescriptor);
                }
                else
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.EventWriteTransfer(this.m_regHandle, ref eventDescriptor, (activityId == Guid.Empty) ? null : &activityId, ((relatedActivityId == Guid.Empty) && !s_preWin7) ? null : &relatedActivityId, (uint) length, (void*) dataDescriptor);
                }
            }
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        [SecurityCritical]
        protected unsafe bool WriteTransferEvent(ref System.Diagnostics.Eventing.EventDescriptor eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
        {
            uint num = 0;
            Guid activityId = GetActivityId();
            num = Microsoft.Win32.UnsafeNativeMethods.EventWriteTransfer(this.m_regHandle, ref eventDescriptor, (activityId == Guid.Empty) ? null : &activityId, &relatedActivityId, (uint) dataCount, (void*) data);
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        private enum ActivityControl : uint
        {
            EVENT_ACTIVITY_CTRL_CREATE_ID = 3,
            EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5,
            EVENT_ACTIVITY_CTRL_GET_ID = 1,
            EVENT_ACTIVITY_CTRL_GET_SET_ID = 4,
            EVENT_ACTIVITY_CTRL_SET_ID = 2
        }

        [StructLayout(LayoutKind.Explicit, Size=0x10)]
        private struct EventData
        {
            [FieldOffset(0)]
            internal ulong DataPointer;
            [FieldOffset(12)]
            internal int Reserved;
            [FieldOffset(8)]
            internal uint Size;
        }

        public enum WriteEventErrorCode
        {
            NoError,
            NoFreeBuffers,
            EventTooBig
        }
    }
}

