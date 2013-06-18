namespace System.Runtime.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Interop;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    internal class DiagnosticsEventProvider : IDisposable
    {
        private long allKeywordMask;
        private long anyKeywordMask;
        private const int basicTypeAllocationBufferSize = 0x10;
        private byte currentTraceLevel;
        [ThreadStatic]
        private static WriteEventErrorCode errorCode;
        private const int etwAPIMaxStringCount = 8;
        [SecurityCritical]
        private System.Runtime.Interop.UnsafeNativeMethods.EtwEnableCallback etwCallback;
        private const int etwMaxNumberArguments = 0x20;
        private int isDisposed;
        private bool isProviderEnabled;
        private const int maxEventDataDescriptors = 0x80;
        private Guid providerId;
        private const int traceEventMaximumSize = 0xffca;
        private const int traceEventMaximumStringSize = 0x7fd4;
        private long traceRegistrationHandle;
        private const int WindowsVistaMajorNumber = 6;

        [SecurityCritical, PermissionSet(SecurityAction.Demand, Unrestricted=true)]
        protected DiagnosticsEventProvider(Guid providerGuid)
        {
            this.providerId = providerGuid;
            this.EtwRegister();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public virtual void Close()
        {
            this.Dispose();
        }

        [SecurityCritical]
        public static Guid CreateActivityId()
        {
            Guid activityId = new Guid();
            System.Runtime.Interop.UnsafeNativeMethods.EventActivityIdControl(3, ref activityId);
            return activityId;
        }

        [SecurityCritical]
        private void Deregister()
        {
            if (this.traceRegistrationHandle != 0L)
            {
                System.Runtime.Interop.UnsafeNativeMethods.EventUnregister(this.traceRegistrationHandle);
                this.traceRegistrationHandle = 0L;
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
            if ((this.isDisposed != 1) && (Interlocked.Exchange(ref this.isDisposed, 1) == 0))
            {
                this.isProviderEnabled = false;
                this.Deregister();
            }
        }

        [SecurityCritical]
        private static unsafe string EncodeObject(ref object data, System.Runtime.Interop.UnsafeNativeMethods.EventData* dataDescriptor, byte* dataBuffer)
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
            this.isProviderEnabled = isEnabled != 0;
            this.currentTraceLevel = setLevel;
            this.anyKeywordMask = anyKeyword;
            this.allKeywordMask = allKeyword;
            this.OnControllerCommand();
        }

        [SecurityCritical]
        private void EtwRegister()
        {
            this.etwCallback = new System.Runtime.Interop.UnsafeNativeMethods.EtwEnableCallback(this.EtwEnableCallBack);
            uint num = System.Runtime.Interop.UnsafeNativeMethods.EventRegister(ref this.providerId, this.etwCallback, null, ref this.traceRegistrationHandle);
            if (num != 0)
            {
                throw new InvalidOperationException(SRCore.EtwRegistrationFailed(num.ToString("x", CultureInfo.CurrentCulture)));
            }
        }

        ~DiagnosticsEventProvider()
        {
            this.Dispose(false);
        }

        [SecurityCritical]
        private static Guid GetActivityId()
        {
            object activityId = Trace.CorrelationManager.ActivityId;
            if (activityId != null)
            {
                return (Guid) activityId;
            }
            return Guid.Empty;
        }

        public static WriteEventErrorCode GetLastWriteEventError()
        {
            return errorCode;
        }

        public bool IsEnabled()
        {
            return this.isProviderEnabled;
        }

        public bool IsEnabled(byte level, long keywords)
        {
            return ((this.isProviderEnabled && ((level <= this.currentTraceLevel) || (this.currentTraceLevel == 0))) && ((keywords == 0L) || (((keywords & this.anyKeywordMask) != 0L) && ((keywords & this.allKeywordMask) == this.allKeywordMask))));
        }

        protected virtual void OnControllerCommand()
        {
        }

        [SecurityCritical]
        public static void SetActivityId(ref Guid id)
        {
            System.Runtime.Interop.UnsafeNativeMethods.EventActivityIdControl(2, ref id);
        }

        private static void SetLastError(int error)
        {
            switch (error)
            {
                case 8:
                    errorCode = WriteEventErrorCode.NoFreeBuffers;
                    break;

                case 0xea:
                case 0x216:
                    errorCode = WriteEventErrorCode.EventTooBig;
                    break;
            }
        }

        [SecurityCritical]
        public unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, params object[] eventPayload)
        {
            uint num = 0;
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                int length = 0;
                SetActivityId(ref GetActivityId());
                if (((eventPayload == null) || (eventPayload.Length == 0)) || (eventPayload.Length == 1))
                {
                    System.Runtime.Interop.UnsafeNativeMethods.EventData data;
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
                        errorCode = WriteEventErrorCode.EventTooBig;
                        return false;
                    }
                    if (str != null)
                    {
                        fixed (char* str3 = ((char*) str))
                        {
                            char* chPtr = str3;
                            data.DataPointer = (ulong) chPtr;
                            num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint) length, &data);
                        }
                    }
                    else if (length == 0)
                    {
                        num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 0, null);
                    }
                    else
                    {
                        num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint) length, &data);
                    }
                }
                else
                {
                    length = eventPayload.Length;
                    if (length > 0x20)
                    {
                        throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", SRCore.EtwMaxNumberArgumentsExceeded(0x20)));
                    }
                    uint num3 = 0;
                    int index = 0;
                    int[] numArray = new int[8];
                    string[] strArray = new string[8];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* userData = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) stackalloc byte[(((IntPtr) length) * sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData))];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataDescriptor = userData;
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
                                    throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", SRCore.EtwAPIMaxStringCountExceeded(8)));
                                }
                                strArray[index] = str2;
                                numArray[index] = i;
                                index++;
                            }
                        }
                    }
                    if (num3 > 0xffca)
                    {
                        errorCode = WriteEventErrorCode.EventTooBig;
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
                                                        dataDescriptor[numArray[0]].DataPointer = (ulong) chPtr2;
                                                    }
                                                    if (strArray[1] != null)
                                                    {
                                                        dataDescriptor[numArray[1]].DataPointer = (ulong) chPtr3;
                                                    }
                                                    if (strArray[2] != null)
                                                    {
                                                        dataDescriptor[numArray[2]].DataPointer = (ulong) chPtr4;
                                                    }
                                                    if (strArray[3] != null)
                                                    {
                                                        dataDescriptor[numArray[3]].DataPointer = (ulong) chPtr5;
                                                    }
                                                    if (strArray[4] != null)
                                                    {
                                                        dataDescriptor[numArray[4]].DataPointer = (ulong) chPtr6;
                                                    }
                                                    if (strArray[5] != null)
                                                    {
                                                        dataDescriptor[numArray[5]].DataPointer = (ulong) chPtr7;
                                                    }
                                                    if (strArray[6] != null)
                                                    {
                                                        dataDescriptor[numArray[6]].DataPointer = (ulong) chPtr8;
                                                    }
                                                    if (strArray[7] != null)
                                                    {
                                                        dataDescriptor[numArray[7]].DataPointer = (ulong) chPtr9;
                                                    }
                                                    num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint) length, userData);
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
        public unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string data)
        {
            uint num = 0;
            data = data ?? string.Empty;
            if (this.IsEnabled(eventDescriptor.Level, eventDescriptor.Keywords))
            {
                System.Runtime.Interop.UnsafeNativeMethods.EventData data2;
                if (data.Length > 0x7fd4)
                {
                    errorCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                SetActivityId(ref GetActivityId());
                data2.Size = (uint) ((data.Length + 1) * 2);
                data2.Reserved = 0;
                fixed (char* str = ((char*) data))
                {
                    char* chPtr = str;
                    data2.DataPointer = (ulong) chPtr;
                    num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, 1, &data2);
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
        protected internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, int dataCount, IntPtr data)
        {
            uint num = 0;
            SetActivityId(ref GetActivityId());
            num = System.Runtime.Interop.UnsafeNativeMethods.EventWrite(this.traceRegistrationHandle, ref eventDescriptor, (uint) dataCount, (System.Runtime.Interop.UnsafeNativeMethods.EventData*) data);
            if (num != 0)
            {
                SetLastError((int) num);
                return false;
            }
            return true;
        }

        [SecurityCritical]
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
                throw Fx.Exception.AsError(new ArgumentNullException("eventMessage"));
            }
            if (this.IsEnabled(eventLevel, eventKeywords))
            {
                if (eventMessage.Length > 0x7fd4)
                {
                    errorCode = WriteEventErrorCode.EventTooBig;
                    return false;
                }
                fixed (char* str = ((char*) eventMessage))
                {
                    char* message = str;
                    error = (int) System.Runtime.Interop.UnsafeNativeMethods.EventWriteString(this.traceRegistrationHandle, eventLevel, eventKeywords, message);
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
        public unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, params object[] eventPayload)
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
                        throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", SRCore.EtwMaxNumberArgumentsExceeded(0x20)));
                    }
                    uint num3 = 0;
                    int index = 0;
                    int[] numArray = new int[8];
                    string[] strArray = new string[8];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* userData = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) stackalloc byte[(((IntPtr) length) * sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData))];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataDescriptor = userData;
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
                                    throw Fx.Exception.AsError(new ArgumentOutOfRangeException("eventPayload", SRCore.EtwAPIMaxStringCountExceeded(8)));
                                }
                                strArray[index] = str;
                                numArray[index] = i;
                                index++;
                            }
                        }
                    }
                    if (num3 > 0xffca)
                    {
                        errorCode = WriteEventErrorCode.EventTooBig;
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
                                                    num = System.Runtime.Interop.UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref activityId, ref relatedActivityId, (uint) length, userData);
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
                    num = System.Runtime.Interop.UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref activityId, ref relatedActivityId, 0, null);
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
        protected unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, int dataCount, IntPtr data)
        {
            uint num = 0;
            Guid activityId = GetActivityId();
            num = System.Runtime.Interop.UnsafeNativeMethods.EventWriteTransfer(this.traceRegistrationHandle, ref eventDescriptor, ref activityId, ref relatedActivityId, (uint) dataCount, (System.Runtime.Interop.UnsafeNativeMethods.EventData*) data);
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

        public enum WriteEventErrorCode
        {
            NoError,
            NoFreeBuffers,
            EventTooBig
        }
    }
}

