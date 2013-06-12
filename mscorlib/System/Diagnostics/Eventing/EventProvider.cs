namespace System.Diagnostics.Eventing
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal class EventProvider : IDisposable
    {
        internal const string ADVAPI32 = "advapi32.dll";
        private long m_allKeywordMask;
        private long m_anyKeywordMask;
        [SecuritySafeCritical]
        private ClassicEtw.ControlCallback m_classicControlCallback;
        private unsafe ClassicEtw.EVENT_HEADER* m_classicEventHeader;
        private long m_classicSessionHandle;
        private int m_disposed;
        private int m_enabled;
        [SecuritySafeCritical]
        private ManifestEtw.EtwEnableCallback m_etwCallback;
        private bool m_isClassic;
        private byte m_level;
        private Guid m_providerId;
        private long m_regHandle;
        private const int s_basicTypeAllocationBufferSize = 0x10;
        private const int s_etwAPIMaxStringCount = 8;
        private const int s_etwMaxMumberArguments = 0x20;
        private static bool s_isClassic;
        private const int s_maxEventDataDescriptors = 0x80;
        [ThreadStatic]
        private static WriteEventErrorCode s_returnCode;
        private const int s_traceEventMaximumSize = 0xffca;
        private const int s_traceEventMaximumStringSize = 0x7fd4;

        internal EventProvider()
        {
            s_isClassic = this.m_isClassic = true;
        }

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
        protected EventProvider(Guid providerGuid)
        {
            this.m_providerId = providerGuid;
            s_isClassic = this.m_isClassic = true;
            this.Register(providerGuid);
        }

        [SecurityCritical]
        private unsafe uint ClassicControlCallback(ClassicEtw.WMIDPREQUESTCODE requestCode, IntPtr requestContext, IntPtr reserved, ClassicEtw.WNODE_HEADER* data)
        {
            int traceEnableFlags = ClassicEtw.GetTraceEnableFlags(data.HistoricalContext);
            byte traceEnableLevel = ClassicEtw.GetTraceEnableLevel(data.HistoricalContext);
            int isEnabled = 0;
            if (requestCode == ClassicEtw.WMIDPREQUESTCODE.EnableEvents)
            {
                this.m_classicSessionHandle = ClassicEtw.GetTraceLoggerHandle(data);
                isEnabled = 1;
            }
            else if (requestCode == ClassicEtw.WMIDPREQUESTCODE.DisableEvents)
            {
                this.m_classicSessionHandle = 0L;
                isEnabled = 0;
            }
            this.m_etwCallback(ref this.m_providerId, isEnabled, traceEnableLevel, (long) traceEnableFlags, 0L, null, null);
            return 0;
        }

        private static uint ClassicShimEventActivityIdControl(int controlCode, ref Guid activityId)
        {
            throw new NotImplementedException();
        }

        [SecurityCritical]
        private unsafe uint ClassicShimEventWrite(ref EventDescriptorInternal eventDescriptor, uint userDataCount, EventData* userData)
        {
            this.m_classicEventHeader.Header.ClientContext = 0;
            this.m_classicEventHeader.Header.Flags = 0x120000;
            this.m_classicEventHeader.Header.Guid = GenTaskGuidFromProviderGuid(this.m_providerId, (ushort) eventDescriptor.Task);
            this.m_classicEventHeader.Header.Level = eventDescriptor.Level;
            this.m_classicEventHeader.Header.Type = eventDescriptor.Opcode;
            this.m_classicEventHeader.Header.Version = eventDescriptor.Version;
            EventData* dataPtr = &this.m_classicEventHeader.Data;
            if (userDataCount > 0x10)
            {
                throw new Exception();
            }
            this.m_classicEventHeader.Header.Size = (ushort) (((ulong) 0x30L) + (userDataCount * sizeof(EventData)));
            for (int i = 0; i < userDataCount; i++)
            {
                dataPtr[i].Ptr = userData[i].Ptr;
                dataPtr[i].Size = userData[i].Size;
            }
            return ClassicEtw.TraceEvent(this.m_classicSessionHandle, this.m_classicEventHeader);
        }

        [SecurityCritical]
        private unsafe uint ClassicShimEventWriteString(byte level, long keywords, char* message)
        {
            EventDescriptorInternal eventDescriptor = new EventDescriptorInternal(0, 0, 0, 0, 0, 0, 0L);
            char* chPtr = message;
            while (chPtr[0] != '\0')
            {
                chPtr++;
            }
            EventData userData = new EventData {
                Ptr = (ulong) message,
                Size = ((uint) ((long) ((chPtr - message) / 2))) + 1,
                Reserved = 0
            };
            return this.ClassicShimEventWrite(ref eventDescriptor, 1, &userData);
        }

        [SecurityCritical]
        private unsafe uint ClassicShimEventWriteTransfer(ref EventDescriptorInternal eventDescriptor, ref Guid activityId, ref Guid relatedActivityId, uint userDataCount, EventData* userData)
        {
            throw new NotImplementedException();
        }

        [SecuritySafeCritical]
        private unsafe uint ClassicShimRegister(Guid providerId, ManifestEtw.EtwEnableCallback enableCallback)
        {
            ClassicEtw.TRACE_GUID_REGISTRATION trace_guid_registration;
            if (this.m_regHandle != 0L)
            {
                throw new Exception();
            }
            this.m_classicEventHeader = (ClassicEtw.EVENT_HEADER*) Marshal.AllocHGlobal(sizeof(ClassicEtw.EVENT_HEADER));
            ZeroMemory((IntPtr) this.m_classicEventHeader, sizeof(ClassicEtw.EVENT_HEADER));
            trace_guid_registration.RegHandle = null;
            trace_guid_registration.Guid = &providerId;
            this.m_classicControlCallback = new ClassicEtw.ControlCallback(this.ClassicControlCallback);
            return ClassicEtw.RegisterTraceGuidsW(this.m_classicControlCallback, null, ref providerId, 1, &trace_guid_registration, null, null, out this.m_regHandle);
        }

        [SecuritySafeCritical]
        private unsafe uint ClassicShimUnregister()
        {
            uint num = ClassicEtw.UnregisterTraceGuids(this.m_regHandle);
            this.m_regHandle = 0L;
            this.m_classicControlCallback = null;
            this.m_classicSessionHandle = 0L;
            if (this.m_classicEventHeader != null)
            {
                Marshal.FreeHGlobal((IntPtr) this.m_classicEventHeader);
                this.m_classicEventHeader = null;
            }
            return num;
        }

        public virtual void Close()
        {
            this.Dispose();
        }

        [SecurityCritical]
        public static Guid CreateActivityId()
        {
            Guid activityId = new Guid();
            EventActivityIdControl(3, ref activityId);
            return activityId;
        }

        [SecurityCritical]
        private void Deregister()
        {
            if (this.m_regHandle != 0L)
            {
                this.EventUnregister();
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
            if (data is IntPtr)
            {
                dataDescriptor.Size = (uint) sizeof(IntPtr);
                IntPtr* ptrPtr = (IntPtr*) dataBuffer;
                ptrPtr[0] = (IntPtr) data;
                dataDescriptor.Ptr = (ulong) ptrPtr;
            }
            else if (data is int)
            {
                dataDescriptor.Size = 4;
                int* numPtr = (int*) dataBuffer;
                numPtr[0] = (int) data;
                dataDescriptor.Ptr = (ulong) numPtr;
            }
            else if (data is long)
            {
                dataDescriptor.Size = 8;
                long* numPtr2 = (long*) dataBuffer;
                numPtr2[0] = (long) data;
                dataDescriptor.Ptr = (ulong) numPtr2;
            }
            else if (data is uint)
            {
                dataDescriptor.Size = 4;
                uint* numPtr3 = (uint*) dataBuffer;
                numPtr3[0] = (uint) data;
                dataDescriptor.Ptr = (ulong) numPtr3;
            }
            else if (data is ulong)
            {
                dataDescriptor.Size = 8;
                ulong* numPtr4 = (ulong*) dataBuffer;
                numPtr4[0] = (ulong) data;
                dataDescriptor.Ptr = (ulong) numPtr4;
            }
            else if (data is char)
            {
                dataDescriptor.Size = 2;
                char* chPtr = (char*) dataBuffer;
                chPtr[0] = (char) data;
                dataDescriptor.Ptr = (ulong) chPtr;
            }
            else if (data is byte)
            {
                dataDescriptor.Size = 1;
                byte* numPtr5 = dataBuffer;
                numPtr5[0] = (byte) data;
                dataDescriptor.Ptr = (ulong) numPtr5;
            }
            else if (data is short)
            {
                dataDescriptor.Size = 2;
                short* numPtr6 = (short*) dataBuffer;
                numPtr6[0] = (short) data;
                dataDescriptor.Ptr = (ulong) numPtr6;
            }
            else if (data is sbyte)
            {
                dataDescriptor.Size = 1;
                sbyte* numPtr7 = (sbyte*) dataBuffer;
                numPtr7[0] = (sbyte) data;
                dataDescriptor.Ptr = (ulong) numPtr7;
            }
            else if (data is ushort)
            {
                dataDescriptor.Size = 2;
                ushort* numPtr8 = (ushort*) dataBuffer;
                numPtr8[0] = (ushort) data;
                dataDescriptor.Ptr = (ulong) numPtr8;
            }
            else if (data is float)
            {
                dataDescriptor.Size = 4;
                float* numPtr9 = (float*) dataBuffer;
                numPtr9[0] = (float) data;
                dataDescriptor.Ptr = (ulong) numPtr9;
            }
            else if (data is double)
            {
                dataDescriptor.Size = 8;
                double* numPtr10 = (double*) dataBuffer;
                numPtr10[0] = (double) data;
                dataDescriptor.Ptr = (ulong) numPtr10;
            }
            else if (data is bool)
            {
                dataDescriptor.Size = 1;
                bool* flagPtr = (bool*) dataBuffer;
                *((sbyte*) flagPtr) = (bool) data;
                dataDescriptor.Ptr = (ulong) flagPtr;
            }
            else if (data is Guid)
            {
                dataDescriptor.Size = (uint) sizeof(Guid);
                Guid* guidPtr = (Guid*) dataBuffer;
                guidPtr[0] = (Guid) data;
                dataDescriptor.Ptr = (ulong) guidPtr;
            }
            else if (data is decimal)
            {
                dataDescriptor.Size = 0x10;
                decimal* numPtr11 = (decimal*) dataBuffer;
                numPtr11[0] = (decimal) data;
                dataDescriptor.Ptr = (ulong) numPtr11;
            }
            else if (data is bool)
            {
                dataDescriptor.Size = 1;
                bool* flagPtr2 = (bool*) dataBuffer;
                *((sbyte*) flagPtr2) = (bool) data;
                dataDescriptor.Ptr = (ulong) flagPtr2;
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
        private unsafe void EtwEnableCallBack([In] ref Guid sourceId, [In] int isEnabled, [In] byte setLevel, [In] long anyKeyword, [In] long allKeyword, [In] ManifestEtw.EVENT_FILTER_DESCRIPTOR* filterData, [In] void* callbackContext)
        {
            byte[] buffer;
            int num;
            this.m_enabled = isEnabled;
            this.m_level = setLevel;
            this.m_anyKeywordMask = anyKeyword;
            this.m_allKeywordMask = allKeyword;
            ControllerCommand update = ControllerCommand.Update;
            IDictionary<string, string> arguments = null;
            if (this.GetDataFromController(filterData, out update, out buffer, out num))
            {
                arguments = new Dictionary<string, string>(4);
                while (num < buffer.Length)
                {
                    int num2 = FindNull(buffer, num);
                    int idx = num2 + 1;
                    int num4 = FindNull(buffer, idx);
                    if (num4 < buffer.Length)
                    {
                        string str = Encoding.UTF8.GetString(buffer, num, num2 - num);
                        string str2 = Encoding.UTF8.GetString(buffer, idx, num4 - idx);
                        arguments[str] = str2;
                    }
                    num = num4 + 1;
                }
            }
            this.OnControllerCommand(update, arguments);
        }

        [SecuritySafeCritical]
        private static uint EventActivityIdControl(int controlCode, ref Guid activityId)
        {
            if (!s_isClassic)
            {
                return ManifestEtw.EventActivityIdControl(controlCode, ref activityId);
            }
            return ClassicShimEventActivityIdControl(controlCode, ref activityId);
        }

        [SecuritySafeCritical]
        private uint EventRegister(ref Guid providerId, ManifestEtw.EtwEnableCallback enableCallback)
        {
            s_isClassic = this.m_isClassic = Environment.OSVersion.Version.Major < 6;
            this.m_providerId = providerId;
            this.m_etwCallback = enableCallback;
            if (!this.m_isClassic)
            {
                return ManifestEtw.EventRegister(ref providerId, enableCallback, null, ref this.m_regHandle);
            }
            return this.ClassicShimRegister(providerId, enableCallback);
        }

        [SecuritySafeCritical]
        private uint EventUnregister()
        {
            uint num;
            if (!this.m_isClassic)
            {
                num = ManifestEtw.EventUnregister(this.m_regHandle);
            }
            else
            {
                num = this.ClassicShimUnregister();
            }
            this.m_regHandle = 0L;
            return num;
        }

        [SecuritySafeCritical]
        private unsafe uint EventWrite(ref EventDescriptorInternal eventDescriptor, uint userDataCount, EventData* userData)
        {
            if (!this.m_isClassic)
            {
                return ManifestEtw.EventWrite(this.m_regHandle, ref eventDescriptor, userDataCount, userData);
            }
            return this.ClassicShimEventWrite(ref eventDescriptor, userDataCount, userData);
        }

        [SecuritySafeCritical]
        private unsafe uint EventWriteString(byte level, long keywords, char* message)
        {
            if (!this.m_isClassic)
            {
                return ManifestEtw.EventWriteString(this.m_regHandle, level, keywords, message);
            }
            return this.ClassicShimEventWriteString(level, keywords, message);
        }

        [SecuritySafeCritical]
        private unsafe uint EventWriteTransfer(ref EventDescriptorInternal eventDescriptor, ref Guid activityId, ref Guid relatedActivityId, uint userDataCount, EventData* userData)
        {
            if (!this.m_isClassic)
            {
                return ManifestEtw.EventWriteTransfer(this.m_regHandle, ref eventDescriptor, ref activityId, ref relatedActivityId, userDataCount, userData);
            }
            return this.ClassicShimEventWriteTransfer(ref eventDescriptor, ref activityId, ref relatedActivityId, userDataCount, userData);
        }

        ~EventProvider()
        {
            this.Dispose(false);
        }

        private static int FindNull(byte[] buffer, int idx)
        {
            while ((idx < buffer.Length) && (buffer[idx] != 0))
            {
                idx++;
            }
            return idx;
        }

        internal static Guid GenTaskGuidFromProviderGuid(Guid providerGuid, ushort taskNumber)
        {
            byte[] b = providerGuid.ToByteArray();
            b[15] = (byte) (b[15] + ((byte) taskNumber));
            b[14] = (byte) (b[14] + ((byte) (taskNumber >> 8)));
            return new Guid(b);
        }

        [SecurityCritical]
        private static Guid GetActivityId()
        {
            Guid activityId = new Guid();
            EventActivityIdControl(1, ref activityId);
            return activityId;
        }

        [SecurityCritical]
        private unsafe bool GetDataFromController(ManifestEtw.EVENT_FILTER_DESCRIPTOR* filterData, out ControllerCommand command, out byte[] data, out int dataStart)
        {
            data = null;
            if (filterData == null)
            {
                string keyName = @"\Microsoft\Windows\CurrentVersion\Winevt\Publishers\{" + this.m_providerId + "}";
                if (Marshal.SizeOf(typeof(IntPtr)) == 8)
                {
                    keyName = @"HKEY_LOCAL_MACHINE\Software\Wow6432Node" + keyName;
                }
                else
                {
                    keyName = @"HKEY_LOCAL_MACHINE\Software" + keyName;
                }
                data = Registry.GetValue(keyName, "ControllerData", null) as byte[];
                if ((data != null) && (data.Length >= 4))
                {
                    command = (ControllerCommand) (((data[3] << ((8 + data[2]) & 0x1f)) << ((8 + data[1]) & 0x1f)) << (8 + data[0]));
                    dataStart = 4;
                    return true;
                }
            }
            else
            {
                if (((filterData.Ptr != 0L) && (0 < filterData.Size)) && (filterData.Size <= 0x400))
                {
                    data = new byte[filterData.Size];
                    Marshal.Copy((IntPtr) filterData.Ptr, data, 0, data.Length);
                }
                command = (ControllerCommand) filterData.Type;
                dataStart = 0;
                return true;
            }
            dataStart = 0;
            command = ControllerCommand.Update;
            return false;
        }

        public static WriteEventErrorCode GetLastWriteEventError()
        {
            return s_returnCode;
        }

        internal static ushort GetTaskFromTaskGuid(Guid taskGuid, Guid providerGuid)
        {
            byte[] buffer = taskGuid.ToByteArray();
            byte[] buffer2 = providerGuid.ToByteArray();
            return (ushort) (((buffer[1] - buffer2[14]) << 8) + (buffer[15] - buffer2[15]));
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

        protected virtual void OnControllerCommand(ControllerCommand command, IDictionary<string, string> arguments)
        {
        }

        [SecurityCritical]
        internal void Register(Guid providerGuid)
        {
            this.m_providerId = providerGuid;
            this.m_etwCallback = new ManifestEtw.EtwEnableCallback(this.EtwEnableCallBack);
            uint num = this.EventRegister(ref this.m_providerId, this.m_etwCallback);
            if (num != 0)
            {
                throw new ArgumentException(Win32Native.GetMessage((int) num));
            }
        }

        [SecurityCritical]
        public static void SetActivityId(ref Guid id)
        {
            EventActivityIdControl(2, ref id);
        }

        private static void SetLastError(int error)
        {
            switch (error)
            {
                case 8:
                    s_returnCode = WriteEventErrorCode.NoFreeBuffers;
                    break;

                case 0xea:
                case 0x216:
                    s_returnCode = WriteEventErrorCode.EventTooBig;
                    break;
            }
        }

        [SecuritySafeCritical]
        public unsafe bool WriteEvent(ref EventDescriptorInternal eventDescriptor, params object[] eventPayload)
        {
            uint num = 0;
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                int length = 0;
                if (((eventPayload == null) || (eventPayload.Length == 0)) || (eventPayload.Length == 1))
                {
                    EventData data;
                    string str = null;
                    byte* dataBuffer = stackalloc byte[0x10];
                    data.Size = 0;
                    if ((eventPayload != null) && (eventPayload.Length != 0))
                    {
                        str = EncodeObject(ref eventPayload[0], &data, dataBuffer);
                        length = 1;
                    }
                    if (data.Size > 0xffca)
                    {
                        s_returnCode = WriteEventErrorCode.EventTooBig;
                        return false;
                    }
                    if (str != null)
                    {
                        fixed (char* str3 = ((char*) str))
                        {
                            char* chPtr = str3;
                            data.Ptr = (ulong) chPtr;
                            num = this.EventWrite(ref eventDescriptor, (uint) length, &data);
                        }
                    }
                    else if (length == 0)
                    {
                        num = this.EventWrite(ref eventDescriptor, 0, null);
                    }
                    else
                    {
                        num = this.EventWrite(ref eventDescriptor, (uint) length, &data);
                    }
                }
                else
                {
                    length = eventPayload.Length;
                    if (length > 0x20)
                    {
                        throw new ArgumentOutOfRangeException("eventPayload", SRETW.GetString("ArgumentOutOfRange_MaxArgExceeded", new object[] { 0x20 }));
                    }
                    uint num3 = 0;
                    int index = 0;
                    int[] numArray = new int[8];
                    string[] strArray = new string[8];
                    EventData* userData = (EventData*) stackalloc byte[(((IntPtr) length) * sizeof(EventData))];
                    EventData* dataDescriptor = userData;
                    byte* numPtr3 = stackalloc byte[(IntPtr) (0x10 * length)];
                    for (int i = 0; i < eventPayload.Length; i++)
                    {
                        if (eventPayload[i] != null)
                        {
                            string str2 = EncodeObject(ref eventPayload[i], dataDescriptor, numPtr3);
                            numPtr3 += 0x10;
                            num3 += dataDescriptor->Size;
                            dataDescriptor++;
                            if (str2 != null)
                            {
                                if (index >= 8)
                                {
                                    throw new ArgumentOutOfRangeException("eventPayload", SRETW.GetString("ArgumentOutOfRange_MaxStringsExceeded", new object[] { 8 }));
                                }
                                strArray[index] = str2;
                                numArray[index] = i;
                                index++;
                            }
                        }
                    }
                    if (num3 > 0xffca)
                    {
                        s_returnCode = WriteEventErrorCode.EventTooBig;
                        return false;
                    }
                    fixed (char* str4 = ((char*) strArray[0]))
                    {
                        char* chPtr2 = str4;
                        fixed (char* str5 = ((char*) strArray[1]))
                        {
                            char* chPtr3 = str5;
                            fixed (char* str6 = ((char*) strArray[2]))
                            {
                                char* chPtr4 = str6;
                                fixed (char* str7 = ((char*) strArray[3]))
                                {
                                    char* chPtr5 = str7;
                                    fixed (char* str8 = ((char*) strArray[4]))
                                    {
                                        char* chPtr6 = str8;
                                        fixed (char* str9 = ((char*) strArray[5]))
                                        {
                                            char* chPtr7 = str9;
                                            fixed (char* str10 = ((char*) strArray[6]))
                                            {
                                                char* chPtr8 = str10;
                                                fixed (char* str11 = ((char*) strArray[7]))
                                                {
                                                    char* chPtr9 = str11;
                                                    dataDescriptor = userData;
                                                    if (strArray[0] != null)
                                                    {
                                                        dataDescriptor[numArray[0]].Ptr = (ulong) chPtr2;
                                                    }
                                                    if (strArray[1] != null)
                                                    {
                                                        dataDescriptor[numArray[1]].Ptr = (ulong) chPtr3;
                                                    }
                                                    if (strArray[2] != null)
                                                    {
                                                        dataDescriptor[numArray[2]].Ptr = (ulong) chPtr4;
                                                    }
                                                    if (strArray[3] != null)
                                                    {
                                                        dataDescriptor[numArray[3]].Ptr = (ulong) chPtr5;
                                                    }
                                                    if (strArray[4] != null)
                                                    {
                                                        dataDescriptor[numArray[4]].Ptr = (ulong) chPtr6;
                                                    }
                                                    if (strArray[5] != null)
                                                    {
                                                        dataDescriptor[numArray[5]].Ptr = (ulong) chPtr7;
                                                    }
                                                    if (strArray[6] != null)
                                                    {
                                                        dataDescriptor[numArray[6]].Ptr = (ulong) chPtr8;
                                                    }
                                                    if (strArray[7] != null)
                                                    {
                                                        dataDescriptor[numArray[7]].Ptr = (ulong) chPtr9;
                                                    }
                                                    num = this.EventWrite(ref eventDescriptor, (uint) length, userData);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    str6 = null;
                    str7 = null;
                    str8 = null;
                    str9 = null;
                    str10 = null;
                    str11 = null;
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
        public unsafe bool WriteEvent(ref EventDescriptorInternal eventDescriptor, string data)
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
                    s_returnCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                data2.Size = (uint) ((data.Length + 1) * 2);
                data2.Reserved = 0;
                fixed (char* str = ((char*) data))
                {
                    char* chPtr = str;
                    data2.Ptr = (ulong) chPtr;
                    num = this.EventWrite(ref eventDescriptor, 1, &data2);
                }
            }
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        protected internal unsafe bool WriteEvent(ref EventDescriptorInternal eventDescriptor, int dataCount, IntPtr data)
        {
            uint num = 0;
            num = this.EventWrite(ref eventDescriptor, (uint) dataCount, (EventData*) data);
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

        [SecuritySafeCritical]
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
                    s_returnCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                fixed (char* str = ((char*) eventMessage))
                {
                    char* message = str;
                    error = (int) this.EventWriteString(eventLevel, eventKeywords, message);
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
        public unsafe bool WriteTransferEvent(ref EventDescriptorInternal eventDescriptor, Guid relatedActivityId, params object[] eventPayload)
        {
            uint num = 0;
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                Guid activityId = GetActivityId();
                if ((eventPayload != null) && (eventPayload.Length != 0))
                {
                    int length = eventPayload.Length;
                    if (length > 0x20)
                    {
                        throw new ArgumentOutOfRangeException("eventPayload", SRETW.GetString("ArgumentOutOfRange_MaxArgExceeded", new object[] { 0x20 }));
                    }
                    uint num3 = 0;
                    int index = 0;
                    int[] numArray = new int[8];
                    string[] strArray = new string[8];
                    EventData* userData = (EventData*) stackalloc byte[(((IntPtr) length) * sizeof(EventData))];
                    EventData* dataDescriptor = userData;
                    byte* dataBuffer = stackalloc byte[(IntPtr) (0x10 * length)];
                    for (int i = 0; i < eventPayload.Length; i++)
                    {
                        if (eventPayload[i] != null)
                        {
                            string str = EncodeObject(ref eventPayload[i], dataDescriptor, dataBuffer);
                            dataBuffer += 0x10;
                            num3 += dataDescriptor->Size;
                            dataDescriptor++;
                            if (str != null)
                            {
                                if (index >= 8)
                                {
                                    throw new ArgumentOutOfRangeException("eventPayload", SRETW.GetString("ArgumentOutOfRange_MaxStringsExceeded", new object[] { 8 }));
                                }
                                strArray[index] = str;
                                numArray[index] = i;
                                index++;
                            }
                        }
                    }
                    if (num3 > 0xffca)
                    {
                        s_returnCode = WriteEventErrorCode.EventTooBig;
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
                                                    dataDescriptor = userData;
                                                    if (strArray[0] != null)
                                                    {
                                                        dataDescriptor[numArray[0]].Ptr = (ulong) chPtr;
                                                    }
                                                    if (strArray[1] != null)
                                                    {
                                                        dataDescriptor[numArray[1]].Ptr = (ulong) chPtr2;
                                                    }
                                                    if (strArray[2] != null)
                                                    {
                                                        dataDescriptor[numArray[2]].Ptr = (ulong) chPtr3;
                                                    }
                                                    if (strArray[3] != null)
                                                    {
                                                        dataDescriptor[numArray[3]].Ptr = (ulong) chPtr4;
                                                    }
                                                    if (strArray[4] != null)
                                                    {
                                                        dataDescriptor[numArray[4]].Ptr = (ulong) chPtr5;
                                                    }
                                                    if (strArray[5] != null)
                                                    {
                                                        dataDescriptor[numArray[5]].Ptr = (ulong) chPtr6;
                                                    }
                                                    if (strArray[6] != null)
                                                    {
                                                        dataDescriptor[numArray[6]].Ptr = (ulong) chPtr7;
                                                    }
                                                    if (strArray[7] != null)
                                                    {
                                                        dataDescriptor[numArray[7]].Ptr = (ulong) chPtr8;
                                                    }
                                                    num = this.EventWriteTransfer(ref eventDescriptor, ref activityId, ref relatedActivityId, (uint) length, userData);
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
                else
                {
                    num = this.EventWriteTransfer(ref eventDescriptor, ref activityId, ref relatedActivityId, 0, null);
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
        protected unsafe bool WriteTransferEvent(ref EventDescriptorInternal eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
        {
            uint num = 0;
            Guid activityId = GetActivityId();
            num = this.EventWriteTransfer(ref eventDescriptor, ref activityId, ref relatedActivityId, (uint) dataCount, (EventData*) data);
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("kernel32.dll", SetLastError=true)]
        internal static extern void ZeroMemory(IntPtr handle, int length);

        protected EventLevel Level
        {
            get
            {
                return (EventLevel) this.m_level;
            }
            set
            {
                this.m_level = (byte) value;
            }
        }

        protected EventKeywords MatchAllKeyword
        {
            get
            {
                return (EventKeywords) this.m_allKeywordMask;
            }
            set
            {
                this.m_allKeywordMask = (long) value;
            }
        }

        protected EventKeywords MatchAnyKeyword
        {
            get
            {
                return (EventKeywords) this.m_anyKeywordMask;
            }
            set
            {
                this.m_anyKeywordMask = (long) value;
            }
        }

        private enum ActivityControl : uint
        {
            EVENT_ACTIVITY_CTRL_CREATE_ID = 3,
            EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5,
            EVENT_ACTIVITY_CTRL_GET_ID = 1,
            EVENT_ACTIVITY_CTRL_GET_SET_ID = 4,
            EVENT_ACTIVITY_CTRL_SET_ID = 2
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class ClassicEtw
        {
            internal const int MAX_MOF_FIELDS = 0x10;
            internal const int WNODE_FLAG_TRACED_GUID = 0x20000;
            internal const int WNODE_FLAG_USE_MOF_PTR = 0x100000;

            [SecurityCritical, DllImport("advapi32.dll")]
            internal static extern int GetTraceEnableFlags(ulong traceHandle);
            [SecurityCritical, DllImport("advapi32.dll")]
            internal static extern byte GetTraceEnableLevel(ulong traceHandle);
            [SecurityCritical, DllImport("advapi32.dll")]
            internal static extern unsafe long GetTraceLoggerHandle(WNODE_HEADER* data);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode)]
            internal static extern unsafe uint RegisterTraceGuidsW([In] ControlCallback cbFunc, [In] void* context, [In] ref Guid providerGuid, [In] int taskGuidCount, [In, Out] TRACE_GUID_REGISTRATION* taskGuids, [In] string mofImagePath, [In] string mofResourceName, out long regHandle);
            [SecurityCritical, DllImport("advapi32.dll")]
            internal static extern unsafe uint TraceEvent(long traceHandle, EVENT_HEADER* header);
            [SecurityCritical, DllImport("advapi32.dll")]
            internal static extern uint UnregisterTraceGuids(long regHandle);

            [SecurityCritical]
            internal unsafe delegate uint ControlCallback(EventProvider.ClassicEtw.WMIDPREQUESTCODE requestCode, IntPtr requestContext, IntPtr reserved, EventProvider.ClassicEtw.WNODE_HEADER* data);

            [StructLayout(LayoutKind.Explicit, Size=0x130)]
            internal struct EVENT_HEADER
            {
                [FieldOffset(0x30)]
                public EventProvider.EventData Data;
                [FieldOffset(0)]
                public EventProvider.ClassicEtw.EVENT_TRACE_HEADER Header;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct EVENT_TRACE_HEADER
            {
                public ushort Size;
                public ushort FieldTypeFlags;
                public byte Type;
                public byte Level;
                public ushort Version;
                public int ThreadId;
                public int ProcessId;
                public long TimeStamp;
                public System.Guid Guid;
                public uint ClientContext;
                public uint Flags;
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct TRACE_GUID_REGISTRATION
            {
                internal unsafe System.Guid* Guid;
                internal unsafe void* RegHandle;
            }

            internal enum WMIDPREQUESTCODE
            {
                GetAllData,
                GetSingleInstance,
                SetSingleInstance,
                SetSingleItem,
                EnableEvents,
                DisableEvents,
                EnableCollection,
                DisableCollection,
                RegInfo,
                ExecuteMethod
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct WNODE_HEADER
            {
                public uint BufferSize;
                public uint ProviderId;
                public ulong HistoricalContext;
                public ulong TimeStamp;
                public System.Guid Guid;
                public uint ClientContext;
                public uint Flags;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct EventData
        {
            internal ulong Ptr;
            internal uint Size;
            internal uint Reserved;
        }

        [SuppressUnmanagedCodeSecurity]
        internal static class ManifestEtw
        {
            internal const int ERROR_ARITHMETIC_OVERFLOW = 0x216;
            internal const int ERROR_MORE_DATA = 0xea;
            internal const int ERROR_NOT_ENOUGH_MEMORY = 8;

            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern uint EventActivityIdControl([In] int ControlCode, [In, Out] ref Guid ActivityId);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern unsafe uint EventRegister([In] ref Guid providerId, [In] EtwEnableCallback enableCallback, [In] void* callbackContext, [In, Out] ref long registrationHandle);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern uint EventUnregister([In] long registrationHandle);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern unsafe uint EventWrite([In] long registrationHandle, [In] ref EventDescriptorInternal eventDescriptor, [In] uint userDataCount, [In] EventProvider.EventData* userData);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern unsafe uint EventWriteString([In] long registrationHandle, [In] byte level, [In] long keywords, [In] char* message);
            [SecurityCritical, DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern unsafe uint EventWriteTransfer([In] long registrationHandle, [In] ref EventDescriptorInternal eventDescriptor, [In] ref Guid activityId, [In] ref Guid relatedActivityId, [In] uint userDataCount, [In] EventProvider.EventData* userData);

            [SecuritySafeCritical]
            internal unsafe delegate void EtwEnableCallback([In] ref Guid sourceId, [In] int isEnabled, [In] byte level, [In] long matchAnyKeywords, [In] long matchAllKeywords, [In] EventProvider.ManifestEtw.EVENT_FILTER_DESCRIPTOR* filterData, [In] void* callbackContext);

            [StructLayout(LayoutKind.Sequential)]
            internal struct EVENT_FILTER_DESCRIPTOR
            {
                public long Ptr;
                public int Size;
                public int Type;
            }
        }

        public enum WriteEventErrorCode
        {
            NoError,
            NoFreeBuffers,
            EventTooBig
        }
    }
}

