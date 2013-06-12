namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class NtProcessInfoHelper
    {
        private static int GetNewBufferSize(int existingBufferSize, int requiredSize)
        {
            if (requiredSize == 0)
            {
                int num = existingBufferSize * 2;
                if (num < existingBufferSize)
                {
                    throw new OutOfMemoryException();
                }
                return num;
            }
            int num2 = requiredSize + 0x2800;
            if (num2 < requiredSize)
            {
                throw new OutOfMemoryException();
            }
            return num2;
        }

        public static ProcessInfo[] GetProcessInfos()
        {
            ProcessInfo[] processInfos;
            int size = 0x20000;
            int returnedSize = 0;
            GCHandle handle = new GCHandle();
            try
            {
                int num3;
                do
                {
                    byte[] buffer = new byte[size];
                    handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    num3 = Microsoft.Win32.NativeMethods.NtQuerySystemInformation(5, handle.AddrOfPinnedObject(), size, out returnedSize);
                    if (num3 == -1073741820)
                    {
                        if (handle.IsAllocated)
                        {
                            handle.Free();
                        }
                        size = GetNewBufferSize(size, returnedSize);
                    }
                }
                while (num3 == -1073741820);
                if (num3 < 0)
                {
                    throw new InvalidOperationException(SR.GetString("CouldntGetProcessInfos"), new Win32Exception(num3));
                }
                processInfos = GetProcessInfos(handle.AddrOfPinnedObject());
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            return processInfos;
        }

        private static ProcessInfo[] GetProcessInfos(IntPtr dataPtr)
        {
            IntPtr ptr;
            Hashtable hashtable = new Hashtable(60);
            long num = 0L;
        Label_000B:
            ptr = (IntPtr) (((long) dataPtr) + num);
            SystemProcessInformation structure = new SystemProcessInformation();
            Marshal.PtrToStructure(ptr, structure);
            ProcessInfo info = new ProcessInfo {
                processId = structure.UniqueProcessId.ToInt32(),
                handleCount = (int) structure.HandleCount,
                sessionId = (int) structure.SessionId,
                poolPagedBytes = (long) ((ulong) structure.QuotaPagedPoolUsage),
                poolNonpagedBytes = (long) ((ulong) structure.QuotaNonPagedPoolUsage),
                virtualBytes = (long) ((ulong) structure.VirtualSize),
                virtualBytesPeak = (long) ((ulong) structure.PeakVirtualSize),
                workingSetPeak = (long) ((ulong) structure.PeakWorkingSetSize),
                workingSet = (long) ((ulong) structure.WorkingSetSize),
                pageFileBytesPeak = (long) ((ulong) structure.PeakPagefileUsage),
                pageFileBytes = (long) ((ulong) structure.PagefileUsage),
                privateBytes = (long) ((ulong) structure.PrivatePageCount),
                basePriority = structure.BasePriority
            };
            if (structure.NamePtr == IntPtr.Zero)
            {
                if (info.processId == NtProcessManager.SystemProcessID)
                {
                    info.processName = "System";
                }
                else if (info.processId == 0)
                {
                    info.processName = "Idle";
                }
                else
                {
                    info.processName = info.processId.ToString(CultureInfo.InvariantCulture);
                }
            }
            else
            {
                string processShortName = GetProcessShortName(Marshal.PtrToStringUni(structure.NamePtr, structure.NameLength / 2));
                if (ProcessManager.IsOSOlderThanXP && (processShortName.Length == 15))
                {
                    if (processShortName.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        processShortName = processShortName.Substring(0, 14);
                    }
                    else if (processShortName.EndsWith(".e", StringComparison.OrdinalIgnoreCase))
                    {
                        processShortName = processShortName.Substring(0, 13);
                    }
                    else if (processShortName.EndsWith(".ex", StringComparison.OrdinalIgnoreCase))
                    {
                        processShortName = processShortName.Substring(0, 12);
                    }
                }
                info.processName = processShortName;
            }
            hashtable[info.processId] = info;
            ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(structure));
            for (int i = 0; i < structure.NumberOfThreads; i++)
            {
                SystemThreadInformation information2 = new SystemThreadInformation();
                Marshal.PtrToStructure(ptr, information2);
                ThreadInfo info2 = new ThreadInfo {
                    processId = (int) information2.UniqueProcess,
                    threadId = (int) information2.UniqueThread,
                    basePriority = information2.BasePriority,
                    currentPriority = information2.Priority,
                    startAddress = information2.StartAddress,
                    threadState = (ThreadState) information2.ThreadState,
                    threadWaitReason = NtProcessManager.GetThreadWaitReason((int) information2.WaitReason)
                };
                info.threadInfoList.Add(info2);
                ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(information2));
            }
            if (structure.NextEntryOffset != 0)
            {
                num += structure.NextEntryOffset;
                goto Label_000B;
            }
            ProcessInfo[] array = new ProcessInfo[hashtable.Values.Count];
            hashtable.Values.CopyTo(array, 0);
            return array;
        }

        internal static string GetProcessShortName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            int startIndex = -1;
            int num2 = -1;
            for (int i = 0; i < name.Length; i++)
            {
                if (name[i] == '\\')
                {
                    startIndex = i;
                }
                else if (name[i] == '.')
                {
                    num2 = i;
                }
            }
            if (num2 == -1)
            {
                num2 = name.Length - 1;
            }
            else
            {
                string b = name.Substring(num2);
                if (string.Equals(".exe", b, StringComparison.OrdinalIgnoreCase))
                {
                    num2--;
                }
                else
                {
                    num2 = name.Length - 1;
                }
            }
            if (startIndex == -1)
            {
                startIndex = 0;
            }
            else
            {
                startIndex++;
            }
            return name.Substring(startIndex, (num2 - startIndex) + 1);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SystemProcessInformation
        {
            internal uint NextEntryOffset;
            internal uint NumberOfThreads;
            private long SpareLi1;
            private long SpareLi2;
            private long SpareLi3;
            private long CreateTime;
            private long UserTime;
            private long KernelTime;
            internal ushort NameLength;
            internal ushort MaximumNameLength;
            internal IntPtr NamePtr;
            internal int BasePriority;
            internal IntPtr UniqueProcessId;
            internal IntPtr InheritedFromUniqueProcessId;
            internal uint HandleCount;
            internal uint SessionId;
            internal UIntPtr PageDirectoryBase;
            internal UIntPtr PeakVirtualSize;
            internal UIntPtr VirtualSize;
            internal uint PageFaultCount;
            internal UIntPtr PeakWorkingSetSize;
            internal UIntPtr WorkingSetSize;
            internal UIntPtr QuotaPeakPagedPoolUsage;
            internal UIntPtr QuotaPagedPoolUsage;
            internal UIntPtr QuotaPeakNonPagedPoolUsage;
            internal UIntPtr QuotaNonPagedPoolUsage;
            internal UIntPtr PagefileUsage;
            internal UIntPtr PeakPagefileUsage;
            internal UIntPtr PrivatePageCount;
            private long ReadOperationCount;
            private long WriteOperationCount;
            private long OtherOperationCount;
            private long ReadTransferCount;
            private long WriteTransferCount;
            private long OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class SystemThreadInformation
        {
            private long KernelTime;
            private long UserTime;
            private long CreateTime;
            private uint WaitTime;
            internal IntPtr StartAddress;
            internal IntPtr UniqueProcess;
            internal IntPtr UniqueThread;
            internal int Priority;
            internal int BasePriority;
            internal uint ContextSwitches;
            internal uint ThreadState;
            internal uint WaitReason;
        }
    }
}

