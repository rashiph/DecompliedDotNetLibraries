namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal static class NtProcessManager
    {
        internal const int IdleProcessID = 0;
        private const string PerfCounterQueryString = "230 232";
        private const int ProcessPerfCounterId = 230;
        private const int ThreadPerfCounterId = 0xe8;
        private static Hashtable valueIds = new Hashtable();

        static NtProcessManager()
        {
            valueIds.Add("Handle Count", ValueId.HandleCount);
            valueIds.Add("Pool Paged Bytes", ValueId.PoolPagedBytes);
            valueIds.Add("Pool Nonpaged Bytes", ValueId.PoolNonpagedBytes);
            valueIds.Add("Elapsed Time", ValueId.ElapsedTime);
            valueIds.Add("Virtual Bytes Peak", ValueId.VirtualBytesPeak);
            valueIds.Add("Virtual Bytes", ValueId.VirtualBytes);
            valueIds.Add("Private Bytes", ValueId.PrivateBytes);
            valueIds.Add("Page File Bytes", ValueId.PageFileBytes);
            valueIds.Add("Page File Bytes Peak", ValueId.PageFileBytesPeak);
            valueIds.Add("Working Set Peak", ValueId.WorkingSetPeak);
            valueIds.Add("Working Set", ValueId.WorkingSet);
            valueIds.Add("ID Thread", ValueId.ThreadId);
            valueIds.Add("ID Process", ValueId.ProcessId);
            valueIds.Add("Priority Base", ValueId.BasePriority);
            valueIds.Add("Priority Current", ValueId.CurrentPriority);
            valueIds.Add("% User Time", ValueId.UserTime);
            valueIds.Add("% Privileged Time", ValueId.PrivilegedTime);
            valueIds.Add("Start Address", ValueId.StartAddress);
            valueIds.Add("Thread State", ValueId.ThreadState);
            valueIds.Add("Thread Wait Reason", ValueId.ThreadWaitReason);
        }

        public static ModuleInfo GetFirstModuleInfo(int processId)
        {
            ModuleInfo[] moduleInfos = GetModuleInfos(processId, true);
            if (moduleInfos.Length == 0)
            {
                return null;
            }
            return moduleInfos[0];
        }

        public static ModuleInfo[] GetModuleInfos(int processId)
        {
            return GetModuleInfos(processId, false);
        }

        private static ModuleInfo[] GetModuleInfos(int processId, bool firstModuleOnly)
        {
            ModuleInfo[] infoArray2;
            if ((processId == SystemProcessID) || (processId == 0))
            {
                throw new Win32Exception(-2147467259, SR.GetString("EnumProcessModuleFailed"));
            }
            Microsoft.Win32.SafeHandles.SafeProcessHandle invalidHandle = Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
            try
            {
                bool flag;
                invalidHandle = ProcessManager.OpenProcess(processId, 0x410, true);
                IntPtr[] ptrArray = new IntPtr[0x40];
                GCHandle handle2 = new GCHandle();
                int needed = 0;
            Label_0045:
                flag = false;
                try
                {
                    handle2 = GCHandle.Alloc(ptrArray, GCHandleType.Pinned);
                    flag = Microsoft.Win32.NativeMethods.EnumProcessModules(invalidHandle, handle2.AddrOfPinnedObject(), ptrArray.Length * IntPtr.Size, ref needed);
                    if (!flag)
                    {
                        bool flag2 = false;
                        bool flag3 = false;
                        if (!ProcessManager.IsOSOlderThanXP)
                        {
                            Microsoft.Win32.SafeHandles.SafeProcessHandle hProcess = Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle;
                            try
                            {
                                hProcess = ProcessManager.OpenProcess(Microsoft.Win32.NativeMethods.GetCurrentProcessId(), 0x400, true);
                                if (!Microsoft.Win32.SafeNativeMethods.IsWow64Process(hProcess, ref flag2))
                                {
                                    throw new Win32Exception();
                                }
                                if (!Microsoft.Win32.SafeNativeMethods.IsWow64Process(invalidHandle, ref flag3))
                                {
                                    throw new Win32Exception();
                                }
                                if (flag2 && !flag3)
                                {
                                    throw new Win32Exception(0x12b, SR.GetString("EnumProcessModuleFailedDueToWow"));
                                }
                            }
                            finally
                            {
                                if (hProcess != Microsoft.Win32.SafeHandles.SafeProcessHandle.InvalidHandle)
                                {
                                    hProcess.Close();
                                }
                            }
                        }
                        for (int j = 0; j < 50; j++)
                        {
                            flag = Microsoft.Win32.NativeMethods.EnumProcessModules(invalidHandle, handle2.AddrOfPinnedObject(), ptrArray.Length * IntPtr.Size, ref needed);
                            if (flag)
                            {
                                goto Label_012F;
                            }
                            Thread.Sleep(1);
                        }
                    }
                }
                finally
                {
                    handle2.Free();
                }
            Label_012F:
                if (!flag)
                {
                    throw new Win32Exception();
                }
                needed /= IntPtr.Size;
                if (needed > ptrArray.Length)
                {
                    ptrArray = new IntPtr[ptrArray.Length * 2];
                    goto Label_0045;
                }
                ArrayList list = new ArrayList();
                for (int i = 0; i < needed; i++)
                {
                    ModuleInfo info = new ModuleInfo();
                    IntPtr handle = ptrArray[i];
                    Microsoft.Win32.NativeMethods.NtModuleInfo ntModuleInfo = new Microsoft.Win32.NativeMethods.NtModuleInfo();
                    if (!Microsoft.Win32.NativeMethods.GetModuleInformation(invalidHandle, new HandleRef(null, handle), ntModuleInfo, Marshal.SizeOf(ntModuleInfo)))
                    {
                        throw new Win32Exception();
                    }
                    info.sizeOfImage = ntModuleInfo.SizeOfImage;
                    info.entryPoint = ntModuleInfo.EntryPoint;
                    info.baseOfDll = ntModuleInfo.BaseOfDll;
                    StringBuilder baseName = new StringBuilder(0x400);
                    if (Microsoft.Win32.NativeMethods.GetModuleBaseName(invalidHandle, new HandleRef(null, handle), baseName, baseName.Capacity * 2) == 0)
                    {
                        throw new Win32Exception();
                    }
                    info.baseName = baseName.ToString();
                    StringBuilder builder2 = new StringBuilder(0x400);
                    if (Microsoft.Win32.NativeMethods.GetModuleFileNameEx(invalidHandle, new HandleRef(null, handle), builder2, builder2.Capacity * 2) == 0)
                    {
                        throw new Win32Exception();
                    }
                    info.fileName = builder2.ToString();
                    if (string.Compare(info.fileName, @"\SystemRoot\System32\smss.exe", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        info.fileName = Path.Combine(Environment.SystemDirectory, "smss.exe");
                    }
                    if (((info.fileName != null) && (info.fileName.Length >= 4)) && info.fileName.StartsWith(@"\\?\", StringComparison.Ordinal))
                    {
                        info.fileName = info.fileName.Substring(4);
                    }
                    list.Add(info);
                    if (firstModuleOnly)
                    {
                        break;
                    }
                }
                ModuleInfo[] array = new ModuleInfo[list.Count];
                list.CopyTo(array, 0);
                infoArray2 = array;
            }
            finally
            {
                if (!invalidHandle.IsInvalid)
                {
                    invalidHandle.Close();
                }
            }
            return infoArray2;
        }

        public static int GetProcessIdFromHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle)
        {
            Microsoft.Win32.NativeMethods.NtProcessBasicInfo info = new Microsoft.Win32.NativeMethods.NtProcessBasicInfo();
            int error = Microsoft.Win32.NativeMethods.NtQueryInformationProcess(processHandle, 0, info, Marshal.SizeOf(info), null);
            if (error != 0)
            {
                throw new InvalidOperationException(SR.GetString("CantGetProcessId"), new Win32Exception(error));
            }
            return info.UniqueProcessId.ToInt32();
        }

        public static int[] GetProcessIds()
        {
            int num;
            int[] processIds = new int[0x100];
        Label_000B:
            if (!Microsoft.Win32.NativeMethods.EnumProcesses(processIds, processIds.Length * 4, out num))
            {
                throw new Win32Exception();
            }
            if (num == (processIds.Length * 4))
            {
                processIds = new int[processIds.Length * 2];
                goto Label_000B;
            }
            int[] destinationArray = new int[num / 4];
            Array.Copy(processIds, destinationArray, destinationArray.Length);
            return destinationArray;
        }

        public static int[] GetProcessIds(string machineName, bool isRemoteMachine)
        {
            ProcessInfo[] processInfos = GetProcessInfos(machineName, isRemoteMachine);
            int[] numArray = new int[processInfos.Length];
            for (int i = 0; i < processInfos.Length; i++)
            {
                numArray[i] = processInfos[i].processId;
            }
            return numArray;
        }

        private static ProcessInfo GetProcessInfo(Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE type, IntPtr instancePtr, Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION[] counters)
        {
            ProcessInfo info = new ProcessInfo();
            for (int i = 0; i < counters.Length; i++)
            {
                Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = counters[i];
                long num2 = ReadCounterValue(perf_counter_definition.CounterType, (IntPtr) (((long) instancePtr) + perf_counter_definition.CounterOffset));
                switch (perf_counter_definition.CounterNameTitlePtr)
                {
                    case 0:
                        info.handleCount = (int) num2;
                        break;

                    case 1:
                        info.poolPagedBytes = (int) num2;
                        break;

                    case 2:
                        info.poolNonpagedBytes = (int) num2;
                        break;

                    case 4:
                        info.virtualBytesPeak = (int) num2;
                        break;

                    case 5:
                        info.virtualBytes = (int) num2;
                        break;

                    case 6:
                        info.privateBytes = (int) num2;
                        break;

                    case 7:
                        info.pageFileBytes = (int) num2;
                        break;

                    case 8:
                        info.pageFileBytesPeak = (int) num2;
                        break;

                    case 9:
                        info.workingSetPeak = (int) num2;
                        break;

                    case 10:
                        info.workingSet = (int) num2;
                        break;

                    case 12:
                        info.processId = (int) num2;
                        break;

                    case 13:
                        info.basePriority = (int) num2;
                        break;
                }
            }
            return info;
        }

        private static ProcessInfo[] GetProcessInfos(PerformanceCounterLib library)
        {
            ProcessInfo[] infoArray = new ProcessInfo[0];
            byte[] data = null;
            for (int i = 5; (infoArray.Length == 0) && (i != 0); i--)
            {
                try
                {
                    data = library.GetPerformanceData("230 232");
                    infoArray = GetProcessInfos(library, 230, 0xe8, data);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(SR.GetString("CouldntGetProcessInfos"), exception);
                }
            }
            if (infoArray.Length == 0)
            {
                throw new InvalidOperationException(SR.GetString("ProcessDisabled"));
            }
            return infoArray;
        }

        public static ProcessInfo[] GetProcessInfos(string machineName, bool isRemoteMachine)
        {
            ProcessInfo[] processInfos;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            try
            {
                processInfos = GetProcessInfos(PerformanceCounterLib.GetPerformanceCounterLib(machineName, new CultureInfo(9)));
            }
            catch (Exception exception)
            {
                if (isRemoteMachine)
                {
                    throw new InvalidOperationException(SR.GetString("CouldntConnectToRemoteMachine"), exception);
                }
                throw exception;
            }
            return processInfos;
        }

        private static ProcessInfo[] GetProcessInfos(PerformanceCounterLib library, int processIndex, int threadIndex, byte[] data)
        {
            Hashtable hashtable = new Hashtable();
            ArrayList list = new ArrayList();
            GCHandle handle = new GCHandle();
            try
            {
                handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                IntPtr ptr = handle.AddrOfPinnedObject();
                Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK structure = new Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK();
                Marshal.PtrToStructure(ptr, structure);
                IntPtr ptr2 = (IntPtr) (((long) ptr) + structure.HeaderLength);
                Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION perf_instance_definition = new Microsoft.Win32.NativeMethods.PERF_INSTANCE_DEFINITION();
                Microsoft.Win32.NativeMethods.PERF_COUNTER_BLOCK perf_counter_block = new Microsoft.Win32.NativeMethods.PERF_COUNTER_BLOCK();
                for (int j = 0; j < structure.NumObjectTypes; j++)
                {
                    Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE perf_object_type = new Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE();
                    Marshal.PtrToStructure(ptr2, perf_object_type);
                    IntPtr ptr3 = (IntPtr) (((long) ptr2) + perf_object_type.DefinitionLength);
                    IntPtr ptr4 = (IntPtr) (((long) ptr2) + perf_object_type.HeaderLength);
                    ArrayList list2 = new ArrayList();
                    for (int k = 0; k < perf_object_type.NumCounters; k++)
                    {
                        Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = new Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION();
                        Marshal.PtrToStructure(ptr4, perf_counter_definition);
                        string counterName = library.GetCounterName(perf_counter_definition.CounterNameTitleIndex);
                        if (perf_object_type.ObjectNameTitleIndex == processIndex)
                        {
                            perf_counter_definition.CounterNameTitlePtr = (int) GetValueId(counterName);
                        }
                        else if (perf_object_type.ObjectNameTitleIndex == threadIndex)
                        {
                            perf_counter_definition.CounterNameTitlePtr = (int) GetValueId(counterName);
                        }
                        list2.Add(perf_counter_definition);
                        ptr4 = (IntPtr) (((long) ptr4) + perf_counter_definition.ByteLength);
                    }
                    Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION[] perf_counter_definitionArray = new Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION[list2.Count];
                    list2.CopyTo(perf_counter_definitionArray, 0);
                    for (int m = 0; m < perf_object_type.NumInstances; m++)
                    {
                        Marshal.PtrToStructure(ptr3, perf_instance_definition);
                        IntPtr ptr5 = (IntPtr) (((long) ptr3) + perf_instance_definition.NameOffset);
                        string strA = Marshal.PtrToStringUni(ptr5);
                        if (!strA.Equals("_Total"))
                        {
                            IntPtr ptr6 = (IntPtr) (((long) ptr3) + perf_instance_definition.ByteLength);
                            Marshal.PtrToStructure(ptr6, perf_counter_block);
                            if (perf_object_type.ObjectNameTitleIndex == processIndex)
                            {
                                ProcessInfo info = GetProcessInfo(perf_object_type, (IntPtr) (((long) ptr3) + perf_instance_definition.ByteLength), perf_counter_definitionArray);
                                if (((info.processId != 0) || (string.Compare(strA, "Idle", StringComparison.OrdinalIgnoreCase) == 0)) && (hashtable[info.processId] == null))
                                {
                                    string str3 = strA;
                                    if (str3.Length == 15)
                                    {
                                        if (strA.EndsWith(".", StringComparison.Ordinal))
                                        {
                                            str3 = strA.Substring(0, 14);
                                        }
                                        else if (strA.EndsWith(".e", StringComparison.Ordinal))
                                        {
                                            str3 = strA.Substring(0, 13);
                                        }
                                        else if (strA.EndsWith(".ex", StringComparison.Ordinal))
                                        {
                                            str3 = strA.Substring(0, 12);
                                        }
                                    }
                                    info.processName = str3;
                                    hashtable.Add(info.processId, info);
                                }
                            }
                            else if (perf_object_type.ObjectNameTitleIndex == threadIndex)
                            {
                                ThreadInfo info2 = GetThreadInfo(perf_object_type, (IntPtr) (((long) ptr3) + perf_instance_definition.ByteLength), perf_counter_definitionArray);
                                if (info2.threadId != 0)
                                {
                                    list.Add(info2);
                                }
                            }
                            ptr3 = (IntPtr) ((((long) ptr3) + perf_instance_definition.ByteLength) + perf_counter_block.ByteLength);
                        }
                    }
                    ptr2 = (IntPtr) (((long) ptr2) + perf_object_type.TotalByteLength);
                }
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                ThreadInfo info3 = (ThreadInfo) list[i];
                ProcessInfo info4 = (ProcessInfo) hashtable[info3.processId];
                if (info4 != null)
                {
                    info4.threadInfoList.Add(info3);
                }
            }
            ProcessInfo[] array = new ProcessInfo[hashtable.Values.Count];
            hashtable.Values.CopyTo(array, 0);
            return array;
        }

        private static ThreadInfo GetThreadInfo(Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE type, IntPtr instancePtr, Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION[] counters)
        {
            ThreadInfo info = new ThreadInfo();
            for (int i = 0; i < counters.Length; i++)
            {
                Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = counters[i];
                long num2 = ReadCounterValue(perf_counter_definition.CounterType, (IntPtr) (((long) instancePtr) + perf_counter_definition.CounterOffset));
                switch (perf_counter_definition.CounterNameTitlePtr)
                {
                    case 11:
                        info.threadId = (int) num2;
                        break;

                    case 12:
                        info.processId = (int) num2;
                        break;

                    case 13:
                        info.basePriority = (int) num2;
                        break;

                    case 14:
                        info.currentPriority = (int) num2;
                        break;

                    case 0x11:
                        info.startAddress = (IntPtr) num2;
                        break;

                    case 0x12:
                        info.threadState = (System.Diagnostics.ThreadState) ((int) num2);
                        break;

                    case 0x13:
                        info.threadWaitReason = GetThreadWaitReason((int) num2);
                        break;
                }
            }
            return info;
        }

        internal static ThreadWaitReason GetThreadWaitReason(int value)
        {
            switch (value)
            {
                case 0:
                case 7:
                    return ThreadWaitReason.Executive;

                case 1:
                case 8:
                    return ThreadWaitReason.FreePage;

                case 2:
                case 9:
                    return ThreadWaitReason.PageIn;

                case 3:
                case 10:
                    return ThreadWaitReason.SystemAllocation;

                case 4:
                case 11:
                    return ThreadWaitReason.ExecutionDelay;

                case 5:
                case 12:
                    return ThreadWaitReason.Suspended;

                case 6:
                case 13:
                    return ThreadWaitReason.UserRequest;

                case 14:
                    return ThreadWaitReason.EventPairHigh;

                case 15:
                    return ThreadWaitReason.EventPairLow;

                case 0x10:
                    return ThreadWaitReason.LpcReceive;

                case 0x11:
                    return ThreadWaitReason.LpcReply;

                case 0x12:
                    return ThreadWaitReason.VirtualMemory;

                case 0x13:
                    return ThreadWaitReason.PageOut;
            }
            return ThreadWaitReason.Unknown;
        }

        private static ValueId GetValueId(string counterName)
        {
            if (counterName != null)
            {
                object obj2 = valueIds[counterName];
                if (obj2 != null)
                {
                    return (ValueId) obj2;
                }
            }
            return ValueId.Unknown;
        }

        private static long ReadCounterValue(int counterType, IntPtr dataPtr)
        {
            if ((counterType & 0x100) != 0)
            {
                return Marshal.ReadInt64(dataPtr);
            }
            return (long) Marshal.ReadInt32(dataPtr);
        }

        internal static int SystemProcessID
        {
            get
            {
                if (ProcessManager.IsOSOlderThanXP)
                {
                    return 8;
                }
                return 4;
            }
        }

        private enum ValueId
        {
            BasePriority = 13,
            CurrentPriority = 14,
            ElapsedTime = 3,
            HandleCount = 0,
            PageFileBytes = 7,
            PageFileBytesPeak = 8,
            PoolNonpagedBytes = 2,
            PoolPagedBytes = 1,
            PrivateBytes = 6,
            PrivilegedTime = 0x10,
            ProcessId = 12,
            StartAddress = 0x11,
            ThreadId = 11,
            ThreadState = 0x12,
            ThreadWaitReason = 0x13,
            Unknown = -1,
            UserTime = 15,
            VirtualBytes = 5,
            VirtualBytesPeak = 4,
            WorkingSet = 10,
            WorkingSetPeak = 9
        }
    }
}

