namespace System.Runtime.Diagnostics
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Runtime;
    using System.Runtime.Interop;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class EtwProvider : DiagnosticsEventProvider
    {
        private Action invokeControllerCallback;

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries"), PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        internal EtwProvider(Guid id) : base(id)
        {
        }

        protected override void OnControllerCommand()
        {
            if (this.invokeControllerCallback != null)
            {
                this.invokeControllerCallback();
            }
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, int value1)
        {
            byte* numPtr = stackalloc byte[(IntPtr) sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 4;
            return base.WriteEvent(ref eventDescriptor, 1, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, long value1)
        {
            byte* numPtr = stackalloc byte[(IntPtr) sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 8;
            return base.WriteEvent(ref eventDescriptor, 1, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, int value1, int value2)
        {
            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 2)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 4;
            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
            dataPtr[1].Size = 4;
            return base.WriteEvent(ref eventDescriptor, 2, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, long value1, long value2)
        {
            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 2)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 8;
            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
            dataPtr[1].Size = 8;
            return base.WriteEvent(ref eventDescriptor, 2, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 2)];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                    dataPtr->DataPointer = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                    dataPtr[1].DataPointer = (ulong) chPtr2;
                    dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                    flag = base.WriteEvent(ref eventDescriptor, 2, (IntPtr) numPtr);
                }
            }
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, string value2, string value3)
        {
            bool flag = true;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            fixed (char* str = ((char*) value2))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value3))
                {
                    char* chPtr2 = str2;
                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 3)];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                    dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                    dataPtr->Size = (uint) sizeof(Guid);
                    dataPtr[1].DataPointer = (ulong) chPtr;
                    dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                    dataPtr[2].DataPointer = (ulong) chPtr2;
                    dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                    flag = base.WriteEvent(ref eventDescriptor, 3, (IntPtr) numPtr);
                }
            }
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, int value1, int value2, int value3)
        {
            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 3)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 4;
            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
            dataPtr[1].Size = 4;
            dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
            dataPtr[2].Size = 4;
            return base.WriteEvent(ref eventDescriptor, 3, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, long value1, long value2, long value3)
        {
            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 3)];
            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
            dataPtr->Size = 8;
            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
            dataPtr[1].Size = 8;
            dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
            dataPtr[2].Size = 8;
            return base.WriteEvent(ref eventDescriptor, 3, (IntPtr) numPtr);
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 3)];
                        System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                        dataPtr->DataPointer = (ulong) chPtr;
                        dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                        dataPtr[1].DataPointer = (ulong) chPtr2;
                        dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                        dataPtr[2].DataPointer = (ulong) chPtr3;
                        dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                        flag = base.WriteEvent(ref eventDescriptor, 3, (IntPtr) numPtr);
                    }
                }
            }
            str3 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, long value2, string value3, string value4)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value3))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value4))
                    {
                        char* chPtr3 = str3;
                        byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 4)];
                        System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                        dataPtr->DataPointer = (ulong) chPtr;
                        dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                        dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                        dataPtr[1].Size = 8;
                        dataPtr[2].DataPointer = (ulong) chPtr2;
                        dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                        dataPtr[3].DataPointer = (ulong) chPtr3;
                        dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                        flag = base.WriteEvent(ref eventDescriptor, 4, (IntPtr) numPtr);
                    }
                }
            }
            str3 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 4)];
                            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                            dataPtr->DataPointer = (ulong) chPtr;
                            dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                            dataPtr[1].DataPointer = (ulong) chPtr2;
                            dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                            dataPtr[2].DataPointer = (ulong) chPtr3;
                            dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                            dataPtr[3].DataPointer = (ulong) chPtr4;
                            dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                            flag = base.WriteEvent(ref eventDescriptor, 4, (IntPtr) numPtr);
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 5)];
                                System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                dataPtr->DataPointer = (ulong) chPtr;
                                dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                dataPtr[1].DataPointer = (ulong) chPtr2;
                                dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                dataPtr[2].DataPointer = (ulong) chPtr3;
                                dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                dataPtr[3].DataPointer = (ulong) chPtr4;
                                dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                dataPtr[4].DataPointer = (ulong) chPtr5;
                                dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                flag = base.WriteEvent(ref eventDescriptor, 5, (IntPtr) numPtr);
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 6)];
                                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                    dataPtr->DataPointer = (ulong) chPtr;
                                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                    dataPtr[1].DataPointer = (ulong) chPtr2;
                                    dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                    dataPtr[2].DataPointer = (ulong) chPtr3;
                                    dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                    dataPtr[3].DataPointer = (ulong) chPtr4;
                                    dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                    dataPtr[4].DataPointer = (ulong) chPtr5;
                                    dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                    dataPtr[5].DataPointer = (ulong) chPtr6;
                                    dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                    flag = base.WriteEvent(ref eventDescriptor, 6, (IntPtr) numPtr);
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 7)];
                                        System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                        dataPtr->DataPointer = (ulong) chPtr;
                                        dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                        dataPtr[1].DataPointer = (ulong) chPtr2;
                                        dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                        dataPtr[2].DataPointer = (ulong) chPtr3;
                                        dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                        dataPtr[3].DataPointer = (ulong) chPtr4;
                                        dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                        dataPtr[4].DataPointer = (ulong) chPtr5;
                                        dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                        dataPtr[5].DataPointer = (ulong) chPtr6;
                                        dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                        dataPtr[6].DataPointer = (ulong) chPtr7;
                                        dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                        flag = base.WriteEvent(ref eventDescriptor, 7, (IntPtr) numPtr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 8)];
                                            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                            dataPtr->DataPointer = (ulong) chPtr;
                                            dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                            dataPtr[1].DataPointer = (ulong) chPtr2;
                                            dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                            dataPtr[2].DataPointer = (ulong) chPtr3;
                                            dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                            dataPtr[3].DataPointer = (ulong) chPtr4;
                                            dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                            dataPtr[4].DataPointer = (ulong) chPtr5;
                                            dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                            dataPtr[5].DataPointer = (ulong) chPtr6;
                                            dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                            dataPtr[6].DataPointer = (ulong) chPtr7;
                                            dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                            dataPtr[7].DataPointer = (ulong) chPtr8;
                                            dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                            flag = base.WriteEvent(ref eventDescriptor, 8, (IntPtr) numPtr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 9)];
                                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                    dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                    dataPtr->Size = (uint) sizeof(Guid);
                                    dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                    dataPtr[1].Size = 8;
                                    dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                    dataPtr[2].Size = 8;
                                    dataPtr[3].DataPointer = (ulong) chPtr;
                                    dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                    dataPtr[4].DataPointer = (ulong) chPtr2;
                                    dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                    dataPtr[5].DataPointer = (ulong) chPtr3;
                                    dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                    dataPtr[6].DataPointer = (ulong) chPtr4;
                                    dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                    dataPtr[7].DataPointer = (ulong) chPtr5;
                                    dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                    dataPtr[8].DataPointer = (ulong) chPtr6;
                                    dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                    flag = base.WriteEvent(ref eventDescriptor, 9, (IntPtr) numPtr);
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value9))
                                            {
                                                char* chPtr9 = str9;
                                                byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 9)];
                                                System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                dataPtr->DataPointer = (ulong) chPtr;
                                                dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                                dataPtr[1].DataPointer = (ulong) chPtr2;
                                                dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                                dataPtr[2].DataPointer = (ulong) chPtr3;
                                                dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                                dataPtr[3].DataPointer = (ulong) chPtr4;
                                                dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                dataPtr[4].DataPointer = (ulong) chPtr5;
                                                dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                dataPtr[5].DataPointer = (ulong) chPtr6;
                                                dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                dataPtr[6].DataPointer = (ulong) chPtr7;
                                                dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                dataPtr[7].DataPointer = (ulong) chPtr8;
                                                dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                dataPtr[8].DataPointer = (ulong) chPtr9;
                                                dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                flag = base.WriteEvent(ref eventDescriptor, 9, (IntPtr) numPtr);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value9))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value10))
                                                {
                                                    char* chPtr10 = str10;
                                                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 10)];
                                                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                    dataPtr->DataPointer = (ulong) chPtr;
                                                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                                    dataPtr[1].DataPointer = (ulong) chPtr2;
                                                    dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                                    dataPtr[2].DataPointer = (ulong) chPtr3;
                                                    dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                                    dataPtr[3].DataPointer = (ulong) chPtr4;
                                                    dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                    dataPtr[4].DataPointer = (ulong) chPtr5;
                                                    dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                    dataPtr[5].DataPointer = (ulong) chPtr6;
                                                    dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                    dataPtr[6].DataPointer = (ulong) chPtr7;
                                                    dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                    dataPtr[7].DataPointer = (ulong) chPtr8;
                                                    dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                    dataPtr[8].DataPointer = (ulong) chPtr9;
                                                    dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                    dataPtr[9].DataPointer = (ulong) chPtr10;
                                                    dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                    flag = base.WriteEvent(ref eventDescriptor, 10, (IntPtr) numPtr);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value10))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value11))
                                        {
                                            char* chPtr8 = str8;
                                            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 11)];
                                            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                            dataPtr->Size = (uint) sizeof(Guid);
                                            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                            dataPtr[1].Size = 8;
                                            dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                            dataPtr[2].Size = 8;
                                            dataPtr[3].DataPointer = (ulong) chPtr;
                                            dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                            dataPtr[4].DataPointer = (ulong) chPtr2;
                                            dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                            dataPtr[5].DataPointer = (ulong) chPtr3;
                                            dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                            dataPtr[6].DataPointer = (ulong) chPtr4;
                                            dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                            dataPtr[7].DataPointer = (ulong) chPtr5;
                                            dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                            dataPtr[8].DataPointer = (ulong) chPtr6;
                                            dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                            dataPtr[9].DataPointer = (ulong) chPtr7;
                                            dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                            dataPtr[10].DataPointer = (ulong) chPtr8;
                                            dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                            flag = base.WriteEvent(ref eventDescriptor, 11, (IntPtr) numPtr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value9))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value10))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value11))
                                                    {
                                                        char* chPtr11 = str11;
                                                        byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 11)];
                                                        System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                        dataPtr->DataPointer = (ulong) chPtr;
                                                        dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                                        dataPtr[1].DataPointer = (ulong) chPtr2;
                                                        dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                                        dataPtr[2].DataPointer = (ulong) chPtr3;
                                                        dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                                        dataPtr[3].DataPointer = (ulong) chPtr4;
                                                        dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                        dataPtr[4].DataPointer = (ulong) chPtr5;
                                                        dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                        dataPtr[5].DataPointer = (ulong) chPtr6;
                                                        dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                        dataPtr[6].DataPointer = (ulong) chPtr7;
                                                        dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                        dataPtr[7].DataPointer = (ulong) chPtr8;
                                                        dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                        dataPtr[8].DataPointer = (ulong) chPtr9;
                                                        dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                        dataPtr[9].DataPointer = (ulong) chPtr10;
                                                        dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                        dataPtr[10].DataPointer = (ulong) chPtr11;
                                                        dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                        flag = base.WriteEvent(ref eventDescriptor, 11, (IntPtr) numPtr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value9))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value10))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value11))
                                                    {
                                                        char* chPtr11 = str11;
                                                        fixed (char* str12 = ((char*) value12))
                                                        {
                                                            char* chPtr12 = str12;
                                                            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 12)];
                                                            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                            dataPtr->DataPointer = (ulong) chPtr;
                                                            dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                                            dataPtr[1].DataPointer = (ulong) chPtr2;
                                                            dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                                            dataPtr[2].DataPointer = (ulong) chPtr3;
                                                            dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                                            dataPtr[3].DataPointer = (ulong) chPtr4;
                                                            dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                            dataPtr[4].DataPointer = (ulong) chPtr5;
                                                            dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                            dataPtr[5].DataPointer = (ulong) chPtr6;
                                                            dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                            dataPtr[6].DataPointer = (ulong) chPtr7;
                                                            dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                            dataPtr[7].DataPointer = (ulong) chPtr8;
                                                            dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                            dataPtr[8].DataPointer = (ulong) chPtr9;
                                                            dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                            dataPtr[9].DataPointer = (ulong) chPtr10;
                                                            dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                            dataPtr[10].DataPointer = (ulong) chPtr11;
                                                            dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                            dataPtr[11].DataPointer = (ulong) chPtr12;
                                                            dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                            flag = base.WriteEvent(ref eventDescriptor, 12, (IntPtr) numPtr);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            str12 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, Guid value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value13 = value13 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value6))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value7))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value8))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value9))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value10))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value11))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value12))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value13))
                                            {
                                                char* chPtr9 = str9;
                                                byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 13)];
                                                System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                                dataPtr->Size = (uint) sizeof(Guid);
                                                dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                                dataPtr[1].Size = 8;
                                                dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                                dataPtr[2].Size = 8;
                                                dataPtr[3].DataPointer = (ulong) chPtr;
                                                dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                dataPtr[4].DataPointer = (ulong) ((IntPtr) &value5);
                                                dataPtr[4].Size = (uint) sizeof(Guid);
                                                dataPtr[5].DataPointer = (ulong) chPtr2;
                                                dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                dataPtr[6].DataPointer = (ulong) chPtr3;
                                                dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                dataPtr[7].DataPointer = (ulong) chPtr4;
                                                dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                dataPtr[8].DataPointer = (ulong) chPtr5;
                                                dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                dataPtr[9].DataPointer = (ulong) chPtr6;
                                                dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                dataPtr[10].DataPointer = (ulong) chPtr7;
                                                dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                dataPtr[11].DataPointer = (ulong) chPtr8;
                                                dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                dataPtr[12].DataPointer = (ulong) chPtr9;
                                                dataPtr[12].Size = (uint) ((value13.Length + 1) * 2);
                                                flag = base.WriteEvent(ref eventDescriptor, 13, (IntPtr) numPtr);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value13 = value13 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value10))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value11))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value12))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value13))
                                                {
                                                    char* chPtr10 = str10;
                                                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 13)];
                                                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                    dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                                    dataPtr->Size = (uint) sizeof(Guid);
                                                    dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                                    dataPtr[1].Size = 8;
                                                    dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                                    dataPtr[2].Size = 8;
                                                    dataPtr[3].DataPointer = (ulong) chPtr;
                                                    dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                    dataPtr[4].DataPointer = (ulong) chPtr2;
                                                    dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                    dataPtr[5].DataPointer = (ulong) chPtr3;
                                                    dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                    dataPtr[6].DataPointer = (ulong) chPtr4;
                                                    dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                    dataPtr[7].DataPointer = (ulong) chPtr5;
                                                    dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                    dataPtr[8].DataPointer = (ulong) chPtr6;
                                                    dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                    dataPtr[9].DataPointer = (ulong) chPtr7;
                                                    dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                    dataPtr[10].DataPointer = (ulong) chPtr8;
                                                    dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                    dataPtr[11].DataPointer = (ulong) chPtr9;
                                                    dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                    dataPtr[12].DataPointer = (ulong) chPtr10;
                                                    dataPtr[12].Size = (uint) ((value13.Length + 1) * 2);
                                                    flag = base.WriteEvent(ref eventDescriptor, 13, (IntPtr) numPtr);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, string value1, string value2, string value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            value3 = value3 ?? string.Empty;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value13 = value13 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value3))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value4))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value5))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value6))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value7))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value8))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value9))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value10))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value11))
                                                    {
                                                        char* chPtr11 = str11;
                                                        fixed (char* str12 = ((char*) value12))
                                                        {
                                                            char* chPtr12 = str12;
                                                            fixed (char* str13 = ((char*) value13))
                                                            {
                                                                char* chPtr13 = str13;
                                                                byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 13)];
                                                                System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                                dataPtr->DataPointer = (ulong) chPtr;
                                                                dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                                                                dataPtr[1].DataPointer = (ulong) chPtr2;
                                                                dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                                                                dataPtr[2].DataPointer = (ulong) chPtr3;
                                                                dataPtr[2].Size = (uint) ((value3.Length + 1) * 2);
                                                                dataPtr[3].DataPointer = (ulong) chPtr4;
                                                                dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                                dataPtr[4].DataPointer = (ulong) chPtr5;
                                                                dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                                dataPtr[5].DataPointer = (ulong) chPtr6;
                                                                dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                                dataPtr[6].DataPointer = (ulong) chPtr7;
                                                                dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                                dataPtr[7].DataPointer = (ulong) chPtr8;
                                                                dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                                dataPtr[8].DataPointer = (ulong) chPtr9;
                                                                dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                                dataPtr[9].DataPointer = (ulong) chPtr10;
                                                                dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                                dataPtr[10].DataPointer = (ulong) chPtr11;
                                                                dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                                dataPtr[11].DataPointer = (ulong) chPtr12;
                                                                dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                                dataPtr[12].DataPointer = (ulong) chPtr13;
                                                                dataPtr[12].Size = (uint) ((value13.Length + 1) * 2);
                                                                flag = base.WriteEvent(ref eventDescriptor, 13, (IntPtr) numPtr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            str12 = null;
            str13 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13, string value14)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value13 = value13 ?? string.Empty;
            value14 = value14 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value10))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value11))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value12))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value13))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value14))
                                                    {
                                                        char* chPtr11 = str11;
                                                        byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 14)];
                                                        System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                        dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                                        dataPtr->Size = (uint) sizeof(Guid);
                                                        dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                                        dataPtr[1].Size = 8;
                                                        dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                                        dataPtr[2].Size = 8;
                                                        dataPtr[3].DataPointer = (ulong) chPtr;
                                                        dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                        dataPtr[4].DataPointer = (ulong) chPtr2;
                                                        dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                        dataPtr[5].DataPointer = (ulong) chPtr3;
                                                        dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                        dataPtr[6].DataPointer = (ulong) chPtr4;
                                                        dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                        dataPtr[7].DataPointer = (ulong) chPtr5;
                                                        dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                        dataPtr[8].DataPointer = (ulong) chPtr6;
                                                        dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                        dataPtr[9].DataPointer = (ulong) chPtr7;
                                                        dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                        dataPtr[10].DataPointer = (ulong) chPtr8;
                                                        dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                        dataPtr[11].DataPointer = (ulong) chPtr9;
                                                        dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                        dataPtr[12].DataPointer = (ulong) chPtr10;
                                                        dataPtr[12].Size = (uint) ((value13.Length + 1) * 2);
                                                        dataPtr[13].DataPointer = (ulong) chPtr11;
                                                        dataPtr[13].Size = (uint) ((value14.Length + 1) * 2);
                                                        flag = base.WriteEvent(ref eventDescriptor, 14, (IntPtr) numPtr);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, string value13, string value14, string value15)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value13 = value13 ?? string.Empty;
            value14 = value14 ?? string.Empty;
            value15 = value15 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value10))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value11))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value12))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value13))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value14))
                                                    {
                                                        char* chPtr11 = str11;
                                                        fixed (char* str12 = ((char*) value15))
                                                        {
                                                            char* chPtr12 = str12;
                                                            byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 15)];
                                                            System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                            dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                                            dataPtr->Size = (uint) sizeof(Guid);
                                                            dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                                            dataPtr[1].Size = 8;
                                                            dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                                            dataPtr[2].Size = 8;
                                                            dataPtr[3].DataPointer = (ulong) chPtr;
                                                            dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                            dataPtr[4].DataPointer = (ulong) chPtr2;
                                                            dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                            dataPtr[5].DataPointer = (ulong) chPtr3;
                                                            dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                            dataPtr[6].DataPointer = (ulong) chPtr4;
                                                            dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                            dataPtr[7].DataPointer = (ulong) chPtr5;
                                                            dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                            dataPtr[8].DataPointer = (ulong) chPtr6;
                                                            dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                            dataPtr[9].DataPointer = (ulong) chPtr7;
                                                            dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                            dataPtr[10].DataPointer = (ulong) chPtr8;
                                                            dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                            dataPtr[11].DataPointer = (ulong) chPtr9;
                                                            dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                            dataPtr[12].DataPointer = (ulong) chPtr10;
                                                            dataPtr[12].Size = (uint) ((value13.Length + 1) * 2);
                                                            dataPtr[13].DataPointer = (ulong) chPtr11;
                                                            dataPtr[13].Size = (uint) ((value14.Length + 1) * 2);
                                                            dataPtr[14].DataPointer = (ulong) chPtr12;
                                                            dataPtr[14].Size = (uint) ((value15.Length + 1) * 2);
                                                            flag = base.WriteEvent(ref eventDescriptor, 15, (IntPtr) numPtr);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            str12 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteEvent(ref EventDescriptor eventDescriptor, Guid value1, long value2, long value3, string value4, string value5, string value6, string value7, string value8, string value9, string value10, string value11, string value12, bool value13, string value14, string value15, string value16, string value17)
        {
            bool flag = true;
            value4 = value4 ?? string.Empty;
            value5 = value5 ?? string.Empty;
            value6 = value6 ?? string.Empty;
            value7 = value7 ?? string.Empty;
            value8 = value8 ?? string.Empty;
            value9 = value9 ?? string.Empty;
            value10 = value10 ?? string.Empty;
            value11 = value11 ?? string.Empty;
            value12 = value12 ?? string.Empty;
            value14 = value14 ?? string.Empty;
            value15 = value15 ?? string.Empty;
            value16 = value16 ?? string.Empty;
            value17 = value17 ?? string.Empty;
            fixed (char* str = ((char*) value4))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value5))
                {
                    char* chPtr2 = str2;
                    fixed (char* str3 = ((char*) value6))
                    {
                        char* chPtr3 = str3;
                        fixed (char* str4 = ((char*) value7))
                        {
                            char* chPtr4 = str4;
                            fixed (char* str5 = ((char*) value8))
                            {
                                char* chPtr5 = str5;
                                fixed (char* str6 = ((char*) value9))
                                {
                                    char* chPtr6 = str6;
                                    fixed (char* str7 = ((char*) value10))
                                    {
                                        char* chPtr7 = str7;
                                        fixed (char* str8 = ((char*) value11))
                                        {
                                            char* chPtr8 = str8;
                                            fixed (char* str9 = ((char*) value12))
                                            {
                                                char* chPtr9 = str9;
                                                fixed (char* str10 = ((char*) value14))
                                                {
                                                    char* chPtr10 = str10;
                                                    fixed (char* str11 = ((char*) value15))
                                                    {
                                                        char* chPtr11 = str11;
                                                        fixed (char* str12 = ((char*) value16))
                                                        {
                                                            char* chPtr12 = str12;
                                                            fixed (char* str13 = ((char*) value17))
                                                            {
                                                                char* chPtr13 = str13;
                                                                byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 0x11)];
                                                                System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                                                                dataPtr->DataPointer = (ulong) ((IntPtr) &value1);
                                                                dataPtr->Size = (uint) sizeof(Guid);
                                                                dataPtr[1].DataPointer = (ulong) ((IntPtr) &value2);
                                                                dataPtr[1].Size = 8;
                                                                dataPtr[2].DataPointer = (ulong) ((IntPtr) &value3);
                                                                dataPtr[2].Size = 8;
                                                                dataPtr[3].DataPointer = (ulong) chPtr;
                                                                dataPtr[3].Size = (uint) ((value4.Length + 1) * 2);
                                                                dataPtr[4].DataPointer = (ulong) chPtr2;
                                                                dataPtr[4].Size = (uint) ((value5.Length + 1) * 2);
                                                                dataPtr[5].DataPointer = (ulong) chPtr3;
                                                                dataPtr[5].Size = (uint) ((value6.Length + 1) * 2);
                                                                dataPtr[6].DataPointer = (ulong) chPtr4;
                                                                dataPtr[6].Size = (uint) ((value7.Length + 1) * 2);
                                                                dataPtr[7].DataPointer = (ulong) chPtr5;
                                                                dataPtr[7].Size = (uint) ((value8.Length + 1) * 2);
                                                                dataPtr[8].DataPointer = (ulong) chPtr6;
                                                                dataPtr[8].Size = (uint) ((value9.Length + 1) * 2);
                                                                dataPtr[9].DataPointer = (ulong) chPtr7;
                                                                dataPtr[9].Size = (uint) ((value10.Length + 1) * 2);
                                                                dataPtr[10].DataPointer = (ulong) chPtr8;
                                                                dataPtr[10].Size = (uint) ((value11.Length + 1) * 2);
                                                                dataPtr[11].DataPointer = (ulong) chPtr9;
                                                                dataPtr[11].Size = (uint) ((value12.Length + 1) * 2);
                                                                dataPtr[12].DataPointer = (ulong) ((IntPtr) &value13);
                                                                dataPtr[12].Size = 1;
                                                                dataPtr[13].DataPointer = (ulong) chPtr10;
                                                                dataPtr[13].Size = (uint) ((value14.Length + 1) * 2);
                                                                dataPtr[14].DataPointer = (ulong) chPtr11;
                                                                dataPtr[14].Size = (uint) ((value15.Length + 1) * 2);
                                                                dataPtr[15].DataPointer = (ulong) chPtr12;
                                                                dataPtr[15].Size = (uint) ((value16.Length + 1) * 2);
                                                                dataPtr[0x10].DataPointer = (ulong) chPtr13;
                                                                dataPtr[0x10].Size = (uint) ((value17.Length + 1) * 2);
                                                                flag = base.WriteEvent(ref eventDescriptor, 0x11, (IntPtr) numPtr);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            str3 = null;
            str4 = null;
            str5 = null;
            str6 = null;
            str7 = null;
            str8 = null;
            str9 = null;
            str10 = null;
            str11 = null;
            str12 = null;
            str13 = null;
            return flag;
        }

        [SecurityCritical]
        internal unsafe bool WriteTransferEvent(ref EventDescriptor eventDescriptor, Guid relatedActivityId, string value1, string value2)
        {
            bool flag = true;
            value1 = value1 ?? string.Empty;
            value2 = value2 ?? string.Empty;
            fixed (char* str = ((char*) value1))
            {
                char* chPtr = str;
                fixed (char* str2 = ((char*) value2))
                {
                    char* chPtr2 = str2;
                    byte* numPtr = stackalloc byte[(IntPtr) (sizeof(System.Runtime.Interop.UnsafeNativeMethods.EventData) * 2)];
                    System.Runtime.Interop.UnsafeNativeMethods.EventData* dataPtr = (System.Runtime.Interop.UnsafeNativeMethods.EventData*) numPtr;
                    dataPtr->DataPointer = (ulong) chPtr;
                    dataPtr->Size = (uint) ((value1.Length + 1) * 2);
                    dataPtr[1].DataPointer = (ulong) chPtr2;
                    dataPtr[1].Size = (uint) ((value2.Length + 1) * 2);
                    flag = base.WriteTransferEvent(ref eventDescriptor, relatedActivityId, 2, (IntPtr) numPtr);
                }
            }
            return flag;
        }

        internal Action ControllerCallBack
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.invokeControllerCallback;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.invokeControllerCallback = value;
            }
        }
    }
}

