namespace System.Diagnostics
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, SharedState=true)]
    internal sealed class SharedPerformanceCounter
    {
        private long baseAddress;
        private CategoryData categoryData;
        private static Hashtable categoryDataTable = new Hashtable(StringComparer.Ordinal);
        private static readonly int CategoryEntrySize = Marshal.SizeOf(typeof(CategoryEntry));
        private string categoryName;
        private int categoryNameHashCode;
        private unsafe CounterEntry* counterEntryPointer;
        private static readonly int CounterEntrySize = Marshal.SizeOf(typeof(CounterEntry));
        internal const int DefaultCountersFileMappingSize = 0x80000;
        internal const string DefaultFileMappingName = "netfxcustomperfcounters.1.0";
        internal int InitialOffset;
        private static readonly int InstanceEntrySize = Marshal.SizeOf(typeof(InstanceEntry));
        private static long InstanceLifetimeSweepWindow = 0x11e1a300L;
        internal const int InstanceNameMaxLength = 0x7f;
        internal const int InstanceNameSlotSize = 0x100;
        private static long LastInstanceLifetimeSweepTick;
        internal const int MaxCountersFileMappingSize = 0x2000000;
        private const int MaxSpinCount = 0x1388;
        internal const int MinCountersFileMappingSize = 0x8000;
        private static System.Diagnostics.ProcessData procData;
        private static readonly int ProcessLifetimeEntrySize = Marshal.SizeOf(typeof(ProcessLifetimeEntry));
        internal static readonly int SingleInstanceHashCode = GetWstrHashCode("systemdiagnosticssharedsingleinstance");
        internal const string SingleInstanceName = "systemdiagnosticssharedsingleinstance";
        private int thisInstanceOffset;

        internal SharedPerformanceCounter(string catName, string counterName, string instanceName) : this(catName, counterName, instanceName, PerformanceCounterInstanceLifetime.Global)
        {
        }

        internal unsafe SharedPerformanceCounter(string catName, string counterName, string instanceName, PerformanceCounterInstanceLifetime lifetime)
        {
            this.InitialOffset = 4;
            this.thisInstanceOffset = -1;
            this.categoryName = catName;
            this.categoryNameHashCode = GetWstrHashCode(this.categoryName);
            this.categoryData = this.GetCategoryData();
            if (this.categoryData.UseUniqueSharedMemory)
            {
                if ((instanceName != null) && (instanceName.Length > 0x7f))
                {
                    throw new InvalidOperationException(SR.GetString("InstanceNameTooLong"));
                }
            }
            else if (lifetime != PerformanceCounterInstanceLifetime.Global)
            {
                throw new InvalidOperationException(SR.GetString("ProcessLifetimeNotValidInGlobal"));
            }
            if (((counterName != null) && (instanceName != null)) && this.categoryData.CounterNames.Contains(counterName))
            {
                this.counterEntryPointer = this.GetCounter(counterName, instanceName, this.categoryData.EnableReuse, lifetime);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static unsafe long AddToValue(CounterEntry* counterEntry, long addend)
        {
            if (IsMisaligned(counterEntry))
            {
                CounterEntryMisaligned* misalignedPtr = (CounterEntryMisaligned*) counterEntry;
                ulong num = (ulong) misalignedPtr->Value_hi;
                num = num << 0x20;
                num |= (ulong) misalignedPtr->Value_lo;
                num += (ulong) addend;
                misalignedPtr->Value_hi = (int) (num >> 0x20);
                misalignedPtr->Value_lo = (int) (num & 0xffffffffL);
                return (long) num;
            }
            return Interlocked.Add(ref counterEntry.Value, addend);
        }

        private unsafe int CalculateAndAllocateMemory(int totalSize, out int alignmentAdjustment)
        {
            int num;
            int num2;
            alignmentAdjustment = 0;
            do
            {
                num2 = *((int*) this.baseAddress);
                this.ResolveOffset(num2, 0);
                num = this.CalculateMemory(num2, totalSize, out alignmentAdjustment);
                int num3 = (((int) this.baseAddress) + num) & 7;
                int num4 = (8 - num3) & 7;
                num += num4;
            }
            while (Microsoft.Win32.SafeNativeMethods.InterlockedCompareExchange((IntPtr) this.baseAddress, num, num2) != num2);
            return num2;
        }

        private int CalculateMemory(int oldOffset, int totalSize, out int alignmentAdjustment)
        {
            int num = this.CalculateMemoryNoBoundsCheck(oldOffset, totalSize, out alignmentAdjustment);
            if ((num > this.FileView.FileMappingSize) || (num < 0))
            {
                throw new InvalidOperationException(SR.GetString("CountersOOM"));
            }
            return num;
        }

        private int CalculateMemoryNoBoundsCheck(int oldOffset, int totalSize, out int alignmentAdjustment)
        {
            int num = totalSize;
            Thread.MemoryBarrier();
            int num2 = (((int) this.baseAddress) + oldOffset) & 7;
            alignmentAdjustment = (8 - num2) & 7;
            num += alignmentAdjustment;
            return (oldOffset + num);
        }

        private unsafe void ClearCounterValues(InstanceEntry* instancePointer)
        {
            CounterEntry* counterEntry = null;
            if (instancePointer.FirstCounterOffset != 0)
            {
                counterEntry = (CounterEntry*) this.ResolveOffset(instancePointer.FirstCounterOffset, CounterEntrySize);
            }
            while (counterEntry != null)
            {
                SetValue(counterEntry, 0L);
                if (counterEntry->NextCounterOffset != 0)
                {
                    counterEntry = (CounterEntry*) this.ResolveOffset(counterEntry->NextCounterOffset, CounterEntrySize);
                }
                else
                {
                    counterEntry = null;
                }
            }
        }

        private unsafe int CreateCategory(CategoryEntry* lastCategoryPointer, int instanceNameHashCode, string instanceName, PerformanceCounterInstanceLifetime lifetime)
        {
            int num2;
            int num3;
            int num4;
            CategoryEntry* entryPtr;
            InstanceEntry* entryPtr2;
            int num5 = 0;
            int num = (this.categoryName.Length + 1) * 2;
            int totalSize = ((CategoryEntrySize + InstanceEntrySize) + (CounterEntrySize * this.categoryData.CounterNames.Count)) + num;
            for (int i = 0; i < this.categoryData.CounterNames.Count; i++)
            {
                totalSize += (((string) this.categoryData.CounterNames[i]).Length + 1) * 2;
            }
            if (this.categoryData.UseUniqueSharedMemory)
            {
                num2 = 0x100;
                totalSize += ProcessLifetimeEntrySize + num2;
                num4 = *((int*) this.baseAddress);
                num5 = this.CalculateMemory(num4, totalSize, out num3);
                if (num4 == this.InitialOffset)
                {
                    lastCategoryPointer.IsConsistent = 0;
                }
            }
            else
            {
                num2 = (instanceName.Length + 1) * 2;
                totalSize += num2;
                num4 = this.CalculateAndAllocateMemory(totalSize, out num3);
            }
            long num8 = this.ResolveOffset(num4, totalSize + num3);
            if (num4 == this.InitialOffset)
            {
                entryPtr = (CategoryEntry*) num8;
                num8 += CategoryEntrySize + num3;
                entryPtr2 = (InstanceEntry*) num8;
            }
            else
            {
                num8 += num3;
                entryPtr = (CategoryEntry*) num8;
                num8 += CategoryEntrySize;
                entryPtr2 = (InstanceEntry*) num8;
            }
            num8 += InstanceEntrySize;
            CounterEntry* counterEntry = (CounterEntry*) num8;
            num8 += CounterEntrySize * this.categoryData.CounterNames.Count;
            if (this.categoryData.UseUniqueSharedMemory)
            {
                ProcessLifetimeEntry* lifetimeEntry = (ProcessLifetimeEntry*) num8;
                num8 += ProcessLifetimeEntrySize;
                counterEntry->LifetimeOffset = (int) (((ulong) lifetimeEntry) - this.baseAddress);
                PopulateLifetimeEntry(lifetimeEntry, lifetime);
            }
            entryPtr->CategoryNameHashCode = this.categoryNameHashCode;
            entryPtr->NextCategoryOffset = 0;
            entryPtr->FirstInstanceOffset = (int) (((ulong) entryPtr2) - this.baseAddress);
            entryPtr->CategoryNameOffset = (int) (num8 - this.baseAddress);
            SafeMarshalCopy(this.categoryName, (IntPtr) num8);
            num8 += num;
            entryPtr2->InstanceNameHashCode = instanceNameHashCode;
            entryPtr2->NextInstanceOffset = 0;
            entryPtr2->FirstCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
            entryPtr2->RefCount = 1;
            entryPtr2->InstanceNameOffset = (int) (num8 - this.baseAddress);
            SafeMarshalCopy(instanceName, (IntPtr) num8);
            num8 += num2;
            string wstr = (string) this.categoryData.CounterNames[0];
            counterEntry->CounterNameHashCode = GetWstrHashCode(wstr);
            SetValue(counterEntry, 0L);
            counterEntry->CounterNameOffset = (int) (num8 - this.baseAddress);
            SafeMarshalCopy(wstr, (IntPtr) num8);
            num8 += (wstr.Length + 1) * 2;
            for (int j = 1; j < this.categoryData.CounterNames.Count; j++)
            {
                CounterEntry* entryPtr5 = counterEntry;
                wstr = (string) this.categoryData.CounterNames[j];
                counterEntry++;
                counterEntry->CounterNameHashCode = GetWstrHashCode(wstr);
                SetValue(counterEntry, 0L);
                counterEntry->CounterNameOffset = (int) (num8 - this.baseAddress);
                SafeMarshalCopy(wstr, (IntPtr) num8);
                num8 += (wstr.Length + 1) * 2;
                entryPtr5->NextCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
            }
            int num10 = (int) (((ulong) entryPtr) - this.baseAddress);
            lastCategoryPointer.IsConsistent = 0;
            if (num10 != this.InitialOffset)
            {
                lastCategoryPointer.NextCategoryOffset = num10;
            }
            if (this.categoryData.UseUniqueSharedMemory)
            {
                this.baseAddress[0] = num5;
                lastCategoryPointer.IsConsistent = 1;
            }
            return num10;
        }

        private unsafe int CreateCounter(CounterEntry* lastCounterPointer, int counterNameHashCode, string counterName)
        {
            int num3;
            int num = (counterName.Length + 1) * 2;
            int totalSize = sizeof(CounterEntry) + num;
            int offset = this.CalculateAndAllocateMemory(totalSize, out num3) + num3;
            long num5 = this.ResolveOffset(offset, totalSize);
            CounterEntry* counterEntry = (CounterEntry*) num5;
            num5 += sizeof(CounterEntry);
            counterEntry->CounterNameOffset = (int) (num5 - this.baseAddress);
            counterEntry->CounterNameHashCode = counterNameHashCode;
            counterEntry->NextCounterOffset = 0;
            SetValue(counterEntry, 0L);
            SafeMarshalCopy(counterName, (IntPtr) num5);
            lastCounterPointer.NextCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
            return offset;
        }

        private unsafe int CreateInstance(CategoryEntry* categoryPointer, int instanceNameHashCode, string instanceName, PerformanceCounterInstanceLifetime lifetime)
        {
            int num;
            int num3;
            int num4;
            int totalSize = InstanceEntrySize + (CounterEntrySize * this.categoryData.CounterNames.Count);
            int num5 = 0;
            if (this.categoryData.UseUniqueSharedMemory)
            {
                num = 0x100;
                totalSize += ProcessLifetimeEntrySize + num;
                num4 = *((int*) this.baseAddress);
                num5 = this.CalculateMemory(num4, totalSize, out num3);
            }
            else
            {
                num = (instanceName.Length + 1) * 2;
                totalSize += num;
                for (int i = 0; i < this.categoryData.CounterNames.Count; i++)
                {
                    totalSize += (((string) this.categoryData.CounterNames[i]).Length + 1) * 2;
                }
                num4 = this.CalculateAndAllocateMemory(totalSize, out num3);
            }
            num4 += num3;
            long num7 = this.ResolveOffset(num4, totalSize);
            InstanceEntry* entryPtr = (InstanceEntry*) num7;
            num7 += InstanceEntrySize;
            CounterEntry* counterEntry = (CounterEntry*) num7;
            num7 += CounterEntrySize * this.categoryData.CounterNames.Count;
            if (this.categoryData.UseUniqueSharedMemory)
            {
                ProcessLifetimeEntry* lifetimeEntry = (ProcessLifetimeEntry*) num7;
                num7 += ProcessLifetimeEntrySize;
                counterEntry->LifetimeOffset = (int) (((ulong) lifetimeEntry) - this.baseAddress);
                PopulateLifetimeEntry(lifetimeEntry, lifetime);
            }
            entryPtr->InstanceNameHashCode = instanceNameHashCode;
            entryPtr->NextInstanceOffset = 0;
            entryPtr->FirstCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
            entryPtr->RefCount = 1;
            entryPtr->InstanceNameOffset = (int) (num7 - this.baseAddress);
            SafeMarshalCopy(instanceName, (IntPtr) num7);
            num7 += num;
            if (this.categoryData.UseUniqueSharedMemory)
            {
                InstanceEntry* entryPtr4 = (InstanceEntry*) this.ResolveOffset(categoryPointer.FirstInstanceOffset, InstanceEntrySize);
                CounterEntry* entryPtr5 = (CounterEntry*) this.ResolveOffset(entryPtr4->FirstCounterOffset, CounterEntrySize);
                counterEntry->CounterNameHashCode = entryPtr5->CounterNameHashCode;
                SetValue(counterEntry, 0L);
                counterEntry->CounterNameOffset = entryPtr5->CounterNameOffset;
                for (int j = 1; j < this.categoryData.CounterNames.Count; j++)
                {
                    CounterEntry* entryPtr6 = counterEntry;
                    counterEntry++;
                    entryPtr5 = (CounterEntry*) this.ResolveOffset(entryPtr5->NextCounterOffset, CounterEntrySize);
                    counterEntry->CounterNameHashCode = entryPtr5->CounterNameHashCode;
                    SetValue(counterEntry, 0L);
                    counterEntry->CounterNameOffset = entryPtr5->CounterNameOffset;
                    entryPtr6->NextCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
                }
            }
            else
            {
                CounterEntry* entryPtr7 = null;
                for (int k = 0; k < this.categoryData.CounterNames.Count; k++)
                {
                    string wstr = (string) this.categoryData.CounterNames[k];
                    counterEntry->CounterNameHashCode = GetWstrHashCode(wstr);
                    counterEntry->CounterNameOffset = (int) (num7 - this.baseAddress);
                    SafeMarshalCopy(wstr, (IntPtr) num7);
                    num7 += (wstr.Length + 1) * 2;
                    SetValue(counterEntry, 0L);
                    if (k != 0)
                    {
                        entryPtr7->NextCounterOffset = (int) (((ulong) counterEntry) - this.baseAddress);
                    }
                    entryPtr7 = counterEntry;
                    counterEntry++;
                }
            }
            int num10 = (int) (((ulong) entryPtr) - this.baseAddress);
            categoryPointer.IsConsistent = 0;
            entryPtr->NextInstanceOffset = categoryPointer.FirstInstanceOffset;
            categoryPointer.FirstInstanceOffset = num10;
            if (this.categoryData.UseUniqueSharedMemory)
            {
                this.baseAddress[0] = num5;
                categoryPointer.IsConsistent = 1;
            }
            return num4;
        }

        internal unsafe long Decrement()
        {
            if (this.counterEntryPointer == null)
            {
                return 0L;
            }
            return DecrementUnaligned(this.counterEntryPointer);
        }

        private static unsafe long DecrementUnaligned(CounterEntry* counterEntry)
        {
            if (IsMisaligned(counterEntry))
            {
                return AddToValue(counterEntry, -1L);
            }
            return Interlocked.Decrement(ref counterEntry.Value);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static unsafe void ExitCriticalSection(int* spinLockPointer)
        {
            spinLockPointer[0] = 0;
        }

        private unsafe bool FindCategory(CategoryEntry** returnCategoryPointerReference)
        {
            CategoryEntry* entryPtr = (CategoryEntry*) this.ResolveOffset(this.InitialOffset, CategoryEntrySize);
            CategoryEntry* currentCategoryPointer = entryPtr;
            CategoryEntry* entryPtr3 = entryPtr;
        Label_0017:
            if (currentCategoryPointer->IsConsistent == 0)
            {
                this.Verify(currentCategoryPointer);
            }
            if ((currentCategoryPointer->CategoryNameHashCode == this.categoryNameHashCode) && this.StringEquals(this.categoryName, currentCategoryPointer->CategoryNameOffset))
            {
                *((IntPtr*) returnCategoryPointerReference) = currentCategoryPointer;
                return true;
            }
            entryPtr3 = currentCategoryPointer;
            if (currentCategoryPointer->NextCategoryOffset != 0)
            {
                currentCategoryPointer = (CategoryEntry*) this.ResolveOffset(currentCategoryPointer->NextCategoryOffset, CategoryEntrySize);
                goto Label_0017;
            }
            *((IntPtr*) returnCategoryPointerReference) = entryPtr3;
            return false;
        }

        private unsafe bool FindCounter(int counterNameHashCode, string counterName, InstanceEntry* instancePointer, CounterEntry** returnCounterPointerReference)
        {
            CounterEntry* entryPtr = (CounterEntry*) this.ResolveOffset(instancePointer.FirstCounterOffset, CounterEntrySize);
            CounterEntry* entryPtr2 = entryPtr;
        Label_0015:
            if ((entryPtr->CounterNameHashCode == counterNameHashCode) && this.StringEquals(counterName, entryPtr->CounterNameOffset))
            {
                *((IntPtr*) returnCounterPointerReference) = entryPtr;
                return true;
            }
            entryPtr2 = entryPtr;
            if (entryPtr->NextCounterOffset != 0)
            {
                entryPtr = (CounterEntry*) this.ResolveOffset(entryPtr->NextCounterOffset, CounterEntrySize);
                goto Label_0015;
            }
            *((IntPtr*) returnCounterPointerReference) = entryPtr2;
            return false;
        }

        private unsafe bool FindInstance(int instanceNameHashCode, string instanceName, CategoryEntry* categoryPointer, InstanceEntry** returnInstancePointerReference, bool activateUnusedInstances, PerformanceCounterInstanceLifetime lifetime, out bool foundFreeInstance)
        {
            bool flag3;
            InstanceEntry* currentInstancePointer = (InstanceEntry*) this.ResolveOffset(categoryPointer.FirstInstanceOffset, InstanceEntrySize);
            InstanceEntry* entryPtr2 = currentInstancePointer;
            foundFreeInstance = false;
            if (currentInstancePointer->InstanceNameHashCode == SingleInstanceHashCode)
            {
                if (!this.StringEquals("systemdiagnosticssharedsingleinstance", currentInstancePointer->InstanceNameOffset))
                {
                    if (instanceName == "systemdiagnosticssharedsingleinstance")
                    {
                        throw new InvalidOperationException(SR.GetString("MultiInstanceOnly", new object[] { this.categoryName }));
                    }
                }
                else if (instanceName != "systemdiagnosticssharedsingleinstance")
                {
                    throw new InvalidOperationException(SR.GetString("SingleInstanceOnly", new object[] { this.categoryName }));
                }
            }
            else if (instanceName == "systemdiagnosticssharedsingleinstance")
            {
                throw new InvalidOperationException(SR.GetString("MultiInstanceOnly", new object[] { this.categoryName }));
            }
            bool flag = activateUnusedInstances;
            if (activateUnusedInstances)
            {
                int num3;
                int totalSize = ((InstanceEntrySize + ProcessLifetimeEntrySize) + 0x100) + (CounterEntrySize * this.categoryData.CounterNames.Count);
                int oldOffset = *((int*) this.baseAddress);
                int num4 = this.CalculateMemoryNoBoundsCheck(oldOffset, totalSize, out num3);
                if ((num4 <= this.FileView.FileMappingSize) && (num4 >= 0))
                {
                    long num5 = DateTime.Now.Ticks - LastInstanceLifetimeSweepTick;
                    if (num5 < InstanceLifetimeSweepWindow)
                    {
                        flag = false;
                    }
                }
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                bool flag2;
            Label_0156:
                flag2 = false;
                if (flag && (currentInstancePointer->RefCount != 0))
                {
                    flag2 = true;
                    this.VerifyLifetime(currentInstancePointer);
                }
                if ((currentInstancePointer->InstanceNameHashCode == instanceNameHashCode) && this.StringEquals(instanceName, currentInstancePointer->InstanceNameOffset))
                {
                    ProcessLifetimeEntry* entryPtr4;
                    *((IntPtr*) returnInstancePointerReference) = currentInstancePointer;
                    CounterEntry* entryPtr3 = (CounterEntry*) this.ResolveOffset(currentInstancePointer->FirstCounterOffset, CounterEntrySize);
                    if (this.categoryData.UseUniqueSharedMemory)
                    {
                        entryPtr4 = (ProcessLifetimeEntry*) this.ResolveOffset(entryPtr3->LifetimeOffset, ProcessLifetimeEntrySize);
                    }
                    else
                    {
                        entryPtr4 = null;
                    }
                    if (!flag2 && (currentInstancePointer->RefCount != 0))
                    {
                        this.VerifyLifetime(currentInstancePointer);
                    }
                    if (currentInstancePointer->RefCount != 0)
                    {
                        if ((entryPtr4 != null) && (entryPtr4->ProcessId != 0))
                        {
                            if (lifetime != PerformanceCounterInstanceLifetime.Process)
                            {
                                throw new InvalidOperationException(SR.GetString("CantConvertProcessToGlobal"));
                            }
                            if (ProcessData.ProcessId != entryPtr4->ProcessId)
                            {
                                throw new InvalidOperationException(SR.GetString("InstanceAlreadyExists", new object[] { instanceName }));
                            }
                            if (((entryPtr4->StartupTime != -1L) && (ProcessData.StartupTime != -1L)) && (ProcessData.StartupTime != entryPtr4->StartupTime))
                            {
                                throw new InvalidOperationException(SR.GetString("InstanceAlreadyExists", new object[] { instanceName }));
                            }
                        }
                        else if (lifetime == PerformanceCounterInstanceLifetime.Process)
                        {
                            throw new InvalidOperationException(SR.GetString("CantConvertGlobalToProcess"));
                        }
                        return true;
                    }
                    if (activateUnusedInstances)
                    {
                        Mutex mutex = null;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            SharedUtils.EnterMutexWithoutGlobal(this.categoryData.MutexName, ref mutex);
                            this.ClearCounterValues(currentInstancePointer);
                            if (entryPtr4 != null)
                            {
                                PopulateLifetimeEntry(entryPtr4, lifetime);
                            }
                            currentInstancePointer->RefCount = 1;
                            return true;
                        }
                        finally
                        {
                            if (mutex != null)
                            {
                                mutex.ReleaseMutex();
                                mutex.Close();
                            }
                        }
                    }
                    return false;
                }
                if (currentInstancePointer->RefCount == 0)
                {
                    foundFreeInstance = true;
                }
                entryPtr2 = currentInstancePointer;
                if (currentInstancePointer->NextInstanceOffset != 0)
                {
                    currentInstancePointer = (InstanceEntry*) this.ResolveOffset(currentInstancePointer->NextInstanceOffset, InstanceEntrySize);
                    goto Label_0156;
                }
                *((IntPtr*) returnInstancePointerReference) = entryPtr2;
                flag3 = false;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
                if (flag)
                {
                    LastInstanceLifetimeSweepTick = DateTime.Now.Ticks;
                }
            }
            return flag3;
        }

        private unsafe CategoryData GetCategoryData()
        {
            CategoryData data = (CategoryData) categoryDataTable[this.categoryName];
            if (data == null)
            {
                lock (categoryDataTable)
                {
                    data = (CategoryData) categoryDataTable[this.categoryName];
                    if (data == null)
                    {
                        data = new CategoryData {
                            FileMappingName = "netfxcustomperfcounters.1.0",
                            MutexName = this.categoryName
                        };
                        new RegistryPermission(PermissionState.Unrestricted).Assert();
                        RegistryKey key = null;
                        try
                        {
                            int fileMappingSizeFromConfig;
                            key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + this.categoryName + @"\Performance");
                            object obj2 = key.GetValue("CategoryOptions");
                            if (obj2 != null)
                            {
                                int num = (int) obj2;
                                data.EnableReuse = (num & 1) != 0;
                                if ((num & 2) != 0)
                                {
                                    data.UseUniqueSharedMemory = true;
                                    this.InitialOffset = 8;
                                    data.FileMappingName = "netfxcustomperfcounters.1.0" + this.categoryName;
                                }
                            }
                            object obj3 = key.GetValue("FileMappingSize");
                            if ((obj3 != null) && data.UseUniqueSharedMemory)
                            {
                                fileMappingSizeFromConfig = (int) obj3;
                                if (fileMappingSizeFromConfig < 0x8000)
                                {
                                    fileMappingSizeFromConfig = 0x8000;
                                }
                                if (fileMappingSizeFromConfig > 0x2000000)
                                {
                                    fileMappingSizeFromConfig = 0x2000000;
                                }
                            }
                            else
                            {
                                fileMappingSizeFromConfig = GetFileMappingSizeFromConfig();
                                if (data.UseUniqueSharedMemory)
                                {
                                    fileMappingSizeFromConfig = fileMappingSizeFromConfig >> 2;
                                }
                            }
                            object obj4 = key.GetValue("Counter Names");
                            byte[] buffer = obj4 as byte[];
                            if (buffer != null)
                            {
                                ArrayList list = new ArrayList();
                                try
                                {
                                    fixed (byte* numRef = buffer)
                                    {
                                        int startIndex = 0;
                                        for (int i = 0; i < (buffer.Length - 1); i += 2)
                                        {
                                            if (((buffer[i] == 0) && (buffer[i + 1] == 0)) && (startIndex != i))
                                            {
                                                list.Add(new string((sbyte*) numRef, startIndex, i - startIndex, Encoding.Unicode).ToLowerInvariant());
                                                startIndex = i + 2;
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    numRef = null;
                                }
                                data.CounterNames = list;
                            }
                            else
                            {
                                string[] c = (string[]) obj4;
                                for (int j = 0; j < c.Length; j++)
                                {
                                    c[j] = c[j].ToLowerInvariant();
                                }
                                data.CounterNames = new ArrayList(c);
                            }
                            if (SharedUtils.CurrentEnvironment == 1)
                            {
                                data.FileMappingName = @"Global\" + data.FileMappingName;
                                data.MutexName = @"Global\" + this.categoryName;
                            }
                            data.FileMapping = new FileMapping(data.FileMappingName, fileMappingSizeFromConfig, this.InitialOffset);
                            categoryDataTable[this.categoryName] = data;
                        }
                        finally
                        {
                            if (key != null)
                            {
                                key.Close();
                            }
                            CodeAccessPermission.RevertAssert();
                        }
                    }
                }
            }
            this.baseAddress = (long) data.FileMapping.FileViewAddress;
            if (data.UseUniqueSharedMemory)
            {
                this.InitialOffset = 8;
            }
            return data;
        }

        private unsafe CounterEntry* GetCounter(string counterName, string instanceName, bool enableReuse, PerformanceCounterInstanceLifetime lifetime)
        {
            int singleInstanceHashCode;
            CounterEntry* entryPtr5;
            int wstrHashCode = GetWstrHashCode(counterName);
            if ((instanceName != null) && (instanceName.Length != 0))
            {
                singleInstanceHashCode = GetWstrHashCode(instanceName);
            }
            else
            {
                singleInstanceHashCode = SingleInstanceHashCode;
                instanceName = "systemdiagnosticssharedsingleinstance";
            }
            Mutex mutex = null;
            CounterEntry* returnCounterPointerReference = null;
            InstanceEntry* instancePointer = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                CategoryEntry* entryPtr3;
                bool flag2;
                bool flag5;
                SharedUtils.EnterMutexWithoutGlobal(this.categoryData.MutexName, ref mutex);
                while (!this.FindCategory(&entryPtr3))
                {
                    bool flag;
                    if (this.categoryData.UseUniqueSharedMemory)
                    {
                        flag = true;
                    }
                    else
                    {
                        WaitAndEnterCriticalSection(&entryPtr3->SpinLock, out flag);
                    }
                    if (flag)
                    {
                        int num3;
                        try
                        {
                            num3 = this.CreateCategory(entryPtr3, singleInstanceHashCode, instanceName, lifetime);
                        }
                        finally
                        {
                            if (!this.categoryData.UseUniqueSharedMemory)
                            {
                                ExitCriticalSection(&entryPtr3->SpinLock);
                            }
                        }
                        entryPtr3 = (CategoryEntry*) this.ResolveOffset(num3, CategoryEntrySize);
                        instancePointer = (InstanceEntry*) this.ResolveOffset(entryPtr3->FirstInstanceOffset, InstanceEntrySize);
                        this.FindCounter(wstrHashCode, counterName, instancePointer, &returnCounterPointerReference);
                        return returnCounterPointerReference;
                    }
                }
                while (!this.FindInstance(singleInstanceHashCode, instanceName, entryPtr3, &instancePointer, true, lifetime, out flag2))
                {
                    bool flag3;
                    InstanceEntry* lockInstancePointer = instancePointer;
                    if (this.categoryData.UseUniqueSharedMemory)
                    {
                        flag3 = true;
                    }
                    else
                    {
                        WaitAndEnterCriticalSection(&lockInstancePointer->SpinLock, out flag3);
                    }
                    if (flag3)
                    {
                        try
                        {
                            bool flag4 = false;
                            if (enableReuse && flag2)
                            {
                                flag4 = this.TryReuseInstance(singleInstanceHashCode, instanceName, entryPtr3, &instancePointer, lifetime, lockInstancePointer);
                            }
                            if (!flag4)
                            {
                                int offset = this.CreateInstance(entryPtr3, singleInstanceHashCode, instanceName, lifetime);
                                instancePointer = (InstanceEntry*) this.ResolveOffset(offset, InstanceEntrySize);
                                this.FindCounter(wstrHashCode, counterName, instancePointer, &returnCounterPointerReference);
                                return returnCounterPointerReference;
                            }
                            continue;
                        }
                        finally
                        {
                            if (!this.categoryData.UseUniqueSharedMemory)
                            {
                                ExitCriticalSection(&lockInstancePointer->SpinLock);
                            }
                        }
                    }
                }
                if (!this.categoryData.UseUniqueSharedMemory)
                {
                    goto Label_01FC;
                }
                this.FindCounter(wstrHashCode, counterName, instancePointer, &returnCounterPointerReference);
                return returnCounterPointerReference;
            Label_01C0:
                WaitAndEnterCriticalSection(&returnCounterPointerReference->SpinLock, out flag5);
                if (flag5)
                {
                    try
                    {
                        int num5 = this.CreateCounter(returnCounterPointerReference, wstrHashCode, counterName);
                        return (CounterEntry*) this.ResolveOffset(num5, CounterEntrySize);
                    }
                    finally
                    {
                        ExitCriticalSection(&returnCounterPointerReference->SpinLock);
                    }
                }
            Label_01FC:
                if (!this.FindCounter(wstrHashCode, counterName, instancePointer, &returnCounterPointerReference))
                {
                    goto Label_01C0;
                }
                entryPtr5 = returnCounterPointerReference;
            }
            finally
            {
                try
                {
                    if ((returnCounterPointerReference != null) && (instancePointer != null))
                    {
                        this.thisInstanceOffset = this.ResolveAddress((long) ((ulong) instancePointer), InstanceEntrySize);
                    }
                }
                catch (InvalidOperationException)
                {
                    this.thisInstanceOffset = -1;
                }
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
            return entryPtr5;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetFileMappingSizeFromConfig()
        {
            return DiagnosticsConfiguration.PerfomanceCountersFileMappingSize;
        }

        private unsafe int GetStringLength(char* startChar)
        {
            char* chPtr = startChar;
            ulong num = (ulong) (this.baseAddress + this.FileView.FileMappingSize);
            while (((ulong) chPtr) < (num - ((ulong) 2L)))
            {
                if (chPtr[0] == '\0')
                {
                    return (int) ((long) ((chPtr - startChar) / 2));
                }
                chPtr++;
            }
            throw new InvalidOperationException(SR.GetString("MappingCorrupted"));
        }

        private static unsafe long GetValue(CounterEntry* counterEntry)
        {
            if (IsMisaligned(counterEntry))
            {
                CounterEntryMisaligned* misalignedPtr = (CounterEntryMisaligned*) counterEntry;
                ulong num = (ulong) misalignedPtr->Value_hi;
                num = num << 0x20;
                num |= (ulong) misalignedPtr->Value_lo;
                return (long) num;
            }
            return counterEntry.Value;
        }

        internal static int GetWstrHashCode(string wstr)
        {
            uint num = 0x1505;
            for (uint i = 0; i < wstr.Length; i++)
            {
                num = ((num << 5) + num) ^ wstr[(int) i];
            }
            return (int) num;
        }

        internal unsafe long Increment()
        {
            if (this.counterEntryPointer == null)
            {
                return 0L;
            }
            return IncrementUnaligned(this.counterEntryPointer);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal unsafe long IncrementBy(long value)
        {
            if (this.counterEntryPointer == null)
            {
                return 0L;
            }
            return AddToValue(this.counterEntryPointer, value);
        }

        private static unsafe long IncrementUnaligned(CounterEntry* counterEntry)
        {
            if (IsMisaligned(counterEntry))
            {
                return AddToValue(counterEntry, 1L);
            }
            return Interlocked.Increment(ref counterEntry.Value);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static unsafe bool IsMisaligned(CounterEntry* counterEntry)
        {
            return ((((ulong) counterEntry) & 7L) != 0L);
        }

        private static unsafe void PopulateLifetimeEntry(ProcessLifetimeEntry* lifetimeEntry, PerformanceCounterInstanceLifetime lifetime)
        {
            if (lifetime == PerformanceCounterInstanceLifetime.Process)
            {
                lifetimeEntry.LifetimeType = 1;
                lifetimeEntry.ProcessId = ProcessData.ProcessId;
                lifetimeEntry.StartupTime = ProcessData.StartupTime;
            }
            else
            {
                lifetimeEntry.ProcessId = 0;
                lifetimeEntry.StartupTime = 0L;
            }
        }

        private unsafe void RemoveAllInstances()
        {
            CategoryEntry* entryPtr;
            if (this.FindCategory(&entryPtr))
            {
                InstanceEntry* instancePointer = (InstanceEntry*) this.ResolveOffset(entryPtr->FirstInstanceOffset, InstanceEntrySize);
                Mutex mutex = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    SharedUtils.EnterMutexWithoutGlobal(this.categoryData.MutexName, ref mutex);
                    while (true)
                    {
                        this.RemoveOneInstance(instancePointer, true);
                        if (instancePointer->NextInstanceOffset == 0)
                        {
                            return;
                        }
                        instancePointer = (InstanceEntry*) this.ResolveOffset(instancePointer->NextInstanceOffset, InstanceEntrySize);
                    }
                }
                finally
                {
                    if (mutex != null)
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                }
            }
        }

        internal static void RemoveAllInstances(string categoryName)
        {
            new SharedPerformanceCounter(categoryName, null, null).RemoveAllInstances();
            RemoveCategoryData(categoryName);
        }

        private static void RemoveCategoryData(string categoryName)
        {
            lock (categoryDataTable)
            {
                categoryDataTable.Remove(categoryName);
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal unsafe void RemoveInstance(string instanceName, PerformanceCounterInstanceLifetime instanceLifetime)
        {
            if ((instanceName != null) && (instanceName.Length != 0))
            {
                CategoryEntry* entryPtr;
                int wstrHashCode = GetWstrHashCode(instanceName);
                if (this.FindCategory(&entryPtr))
                {
                    InstanceEntry* returnInstancePointerReference = null;
                    bool flag = false;
                    Mutex mutex = null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        bool flag2;
                        SharedUtils.EnterMutexWithoutGlobal(this.categoryData.MutexName, ref mutex);
                        if (this.thisInstanceOffset != -1)
                        {
                            try
                            {
                                returnInstancePointerReference = (InstanceEntry*) this.ResolveOffset(this.thisInstanceOffset, InstanceEntrySize);
                                if ((returnInstancePointerReference->InstanceNameHashCode == wstrHashCode) && this.StringEquals(instanceName, returnInstancePointerReference->InstanceNameOffset))
                                {
                                    flag = true;
                                    CounterEntry* entryPtr3 = (CounterEntry*) this.ResolveOffset(returnInstancePointerReference->FirstCounterOffset, CounterEntrySize);
                                    if (this.categoryData.UseUniqueSharedMemory)
                                    {
                                        ProcessLifetimeEntry* entryPtr4 = (ProcessLifetimeEntry*) this.ResolveOffset(entryPtr3->LifetimeOffset, ProcessLifetimeEntrySize);
                                        if (((entryPtr4 != null) && (entryPtr4->LifetimeType == 1)) && (entryPtr4->ProcessId != 0))
                                        {
                                            flag &= instanceLifetime == PerformanceCounterInstanceLifetime.Process;
                                            flag &= ProcessData.ProcessId == entryPtr4->ProcessId;
                                            if ((entryPtr4->StartupTime != -1L) && (ProcessData.StartupTime != -1L))
                                            {
                                                flag &= ProcessData.StartupTime == entryPtr4->StartupTime;
                                            }
                                        }
                                        else
                                        {
                                            flag &= instanceLifetime != PerformanceCounterInstanceLifetime.Process;
                                        }
                                    }
                                }
                            }
                            catch (InvalidOperationException)
                            {
                                flag = false;
                            }
                            if (!flag)
                            {
                                this.thisInstanceOffset = -1;
                            }
                        }
                        if ((flag || this.FindInstance(wstrHashCode, instanceName, entryPtr, &returnInstancePointerReference, false, instanceLifetime, out flag2)) && (returnInstancePointerReference != null))
                        {
                            this.RemoveOneInstance(returnInstancePointerReference, false);
                        }
                    }
                    finally
                    {
                        if (mutex != null)
                        {
                            mutex.ReleaseMutex();
                            mutex.Close();
                        }
                    }
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private unsafe void RemoveOneInstance(InstanceEntry* instancePointer, bool clearValue)
        {
            bool taken = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                if (!this.categoryData.UseUniqueSharedMemory)
                {
                    while (!taken)
                    {
                        WaitAndEnterCriticalSection(&instancePointer.SpinLock, out taken);
                    }
                }
                instancePointer.RefCount = 0;
                if (clearValue)
                {
                    this.ClearCounterValues(instancePointer);
                }
            }
            finally
            {
                if (taken)
                {
                    ExitCriticalSection(&instancePointer.SpinLock);
                }
            }
        }

        private int ResolveAddress(long address, int sizeToRead)
        {
            int num = (int) (address - this.baseAddress);
            if ((num > (this.FileView.FileMappingSize - sizeToRead)) || (num < 0))
            {
                throw new InvalidOperationException(SR.GetString("MappingCorrupted"));
            }
            return num;
        }

        private long ResolveOffset(int offset, int sizeToRead)
        {
            if ((offset > (this.FileView.FileMappingSize - sizeToRead)) || (offset < 0))
            {
                throw new InvalidOperationException(SR.GetString("MappingCorrupted"));
            }
            return (this.baseAddress + offset);
        }

        private static void SafeMarshalCopy(string str, IntPtr nativePointer)
        {
            char[] destination = new char[str.Length + 1];
            str.CopyTo(0, destination, 0, str.Length);
            destination[str.Length] = '\0';
            Marshal.Copy(destination, 0, nativePointer, destination.Length);
        }

        private static unsafe void SetValue(CounterEntry* counterEntry, long value)
        {
            if (IsMisaligned(counterEntry))
            {
                CounterEntryMisaligned* misalignedPtr = (CounterEntryMisaligned*) counterEntry;
                misalignedPtr->Value_lo = (int) (((ulong) value) & 0xffffffffL);
                misalignedPtr->Value_hi = (int) (value >> 0x20);
            }
            else
            {
                counterEntry.Value = value;
            }
        }

        private unsafe bool StringEquals(string stringA, int offset)
        {
            char* chPtr = (char*) this.ResolveOffset(offset, 0);
            ulong num = (ulong) (this.baseAddress + this.FileView.FileMappingSize);
            int index = 0;
            while (index < stringA.Length)
            {
                if (((ulong) (chPtr + index)) > (num - ((ulong) 2L)))
                {
                    throw new InvalidOperationException(SR.GetString("MappingCorrupted"));
                }
                if (stringA[index] != chPtr[index])
                {
                    return false;
                }
                index++;
            }
            if (((ulong) (chPtr + index)) > (num - ((ulong) 2L)))
            {
                throw new InvalidOperationException(SR.GetString("MappingCorrupted"));
            }
            return (chPtr[index] == '\0');
        }

        private unsafe bool TryReuseInstance(int instanceNameHashCode, string instanceName, CategoryEntry* categoryPointer, InstanceEntry** returnInstancePointerReference, PerformanceCounterInstanceLifetime lifetime, InstanceEntry* lockInstancePointer)
        {
            InstanceEntry* entryPtr = (InstanceEntry*) this.ResolveOffset(categoryPointer.FirstInstanceOffset, InstanceEntrySize);
            InstanceEntry* entryPtr2 = entryPtr;
        Label_0015:
            if (entryPtr->RefCount == 0)
            {
                bool flag;
                long num;
                if (this.categoryData.UseUniqueSharedMemory)
                {
                    num = this.ResolveOffset(entryPtr->InstanceNameOffset, 0x100);
                    flag = true;
                }
                else
                {
                    num = this.ResolveOffset(entryPtr->InstanceNameOffset, 0);
                    flag = this.GetStringLength((char*) num) == instanceName.Length;
                }
                bool flag2 = (lockInstancePointer == entryPtr) || this.categoryData.UseUniqueSharedMemory;
                if (flag)
                {
                    bool flag3;
                    if (flag2)
                    {
                        flag3 = true;
                    }
                    else
                    {
                        WaitAndEnterCriticalSection(&entryPtr->SpinLock, out flag3);
                    }
                    if (flag3)
                    {
                        try
                        {
                            SafeMarshalCopy(instanceName, (IntPtr) num);
                            entryPtr->InstanceNameHashCode = instanceNameHashCode;
                            *((IntPtr*) returnInstancePointerReference) = entryPtr;
                            this.ClearCounterValues(returnInstancePointerReference[0]);
                            if (this.categoryData.UseUniqueSharedMemory)
                            {
                                CounterEntry* entryPtr3 = (CounterEntry*) this.ResolveOffset(entryPtr->FirstCounterOffset, CounterEntrySize);
                                ProcessLifetimeEntry* lifetimeEntry = (ProcessLifetimeEntry*) this.ResolveOffset(entryPtr3->LifetimeOffset, ProcessLifetimeEntrySize);
                                PopulateLifetimeEntry(lifetimeEntry, lifetime);
                            }
                            *(((IntPtr*) returnInstancePointerReference)).RefCount = 1;
                            return true;
                        }
                        finally
                        {
                            if (!flag2)
                            {
                                ExitCriticalSection(&entryPtr->SpinLock);
                            }
                        }
                    }
                }
            }
            entryPtr2 = entryPtr;
            if (entryPtr->NextInstanceOffset != 0)
            {
                entryPtr = (InstanceEntry*) this.ResolveOffset(entryPtr->NextInstanceOffset, InstanceEntrySize);
                goto Label_0015;
            }
            *((IntPtr*) returnInstancePointerReference) = entryPtr2;
            return false;
        }

        private unsafe void Verify(CategoryEntry* currentCategoryPointer)
        {
            if (this.categoryData.UseUniqueSharedMemory)
            {
                Mutex mutex = null;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    SharedUtils.EnterMutexWithoutGlobal(this.categoryData.MutexName, ref mutex);
                    this.VerifyCategory(currentCategoryPointer);
                }
                finally
                {
                    if (mutex != null)
                    {
                        mutex.ReleaseMutex();
                        mutex.Close();
                    }
                }
            }
        }

        private unsafe void VerifyCategory(CategoryEntry* currentCategoryPointer)
        {
            int offset = *((int*) this.baseAddress);
            this.ResolveOffset(offset, 0);
            if (this.ResolveAddress((long) ((ulong) currentCategoryPointer), CategoryEntrySize) >= offset)
            {
                currentCategoryPointer.SpinLock = 0;
                currentCategoryPointer.CategoryNameHashCode = 0;
                currentCategoryPointer.CategoryNameOffset = 0;
                currentCategoryPointer.FirstInstanceOffset = 0;
                currentCategoryPointer.NextCategoryOffset = 0;
                currentCategoryPointer.IsConsistent = 0;
            }
            else
            {
                if (currentCategoryPointer.NextCategoryOffset > offset)
                {
                    currentCategoryPointer.NextCategoryOffset = 0;
                }
                else if (currentCategoryPointer.NextCategoryOffset != 0)
                {
                    this.VerifyCategory((CategoryEntry*) this.ResolveOffset(currentCategoryPointer.NextCategoryOffset, CategoryEntrySize));
                }
                if (currentCategoryPointer.FirstInstanceOffset != 0)
                {
                    if (currentCategoryPointer.FirstInstanceOffset > offset)
                    {
                        InstanceEntry* entryPtr = (InstanceEntry*) this.ResolveOffset(currentCategoryPointer.FirstInstanceOffset, InstanceEntrySize);
                        currentCategoryPointer.FirstInstanceOffset = entryPtr->NextInstanceOffset;
                        if (currentCategoryPointer.FirstInstanceOffset > offset)
                        {
                            currentCategoryPointer.FirstInstanceOffset = 0;
                        }
                    }
                    if (currentCategoryPointer.FirstInstanceOffset != 0)
                    {
                        this.VerifyInstance((InstanceEntry*) this.ResolveOffset(currentCategoryPointer.FirstInstanceOffset, InstanceEntrySize));
                    }
                }
                currentCategoryPointer.IsConsistent = 1;
            }
        }

        private unsafe void VerifyInstance(InstanceEntry* currentInstancePointer)
        {
            int offset = *((int*) this.baseAddress);
            this.ResolveOffset(offset, 0);
            if (currentInstancePointer.NextInstanceOffset > offset)
            {
                currentInstancePointer.NextInstanceOffset = 0;
            }
            else if (currentInstancePointer.NextInstanceOffset != 0)
            {
                this.VerifyInstance((InstanceEntry*) this.ResolveOffset(currentInstancePointer.NextInstanceOffset, InstanceEntrySize));
            }
        }

        private unsafe void VerifyLifetime(InstanceEntry* currentInstancePointer)
        {
            CounterEntry* entryPtr = (CounterEntry*) this.ResolveOffset(currentInstancePointer.FirstCounterOffset, CounterEntrySize);
            if (entryPtr->LifetimeOffset != 0)
            {
                ProcessLifetimeEntry* entryPtr2 = (ProcessLifetimeEntry*) this.ResolveOffset(entryPtr->LifetimeOffset, ProcessLifetimeEntrySize);
                if (entryPtr2->LifetimeType == 1)
                {
                    int processId = entryPtr2->ProcessId;
                    long startupTime = entryPtr2->StartupTime;
                    if (processId != 0)
                    {
                        if (processId == ProcessData.ProcessId)
                        {
                            if (((ProcessData.StartupTime != -1L) && (startupTime != -1L)) && (ProcessData.StartupTime != startupTime))
                            {
                                currentInstancePointer.RefCount = 0;
                            }
                        }
                        else
                        {
                            using (Microsoft.Win32.SafeHandles.SafeProcessHandle handle = Microsoft.Win32.SafeHandles.SafeProcessHandle.OpenProcess(0x400, false, processId))
                            {
                                long num3;
                                long num5;
                                if ((Marshal.GetLastWin32Error() == 0x57) && handle.IsInvalid)
                                {
                                    currentInstancePointer.RefCount = 0;
                                    return;
                                }
                                if ((!handle.IsInvalid && (startupTime != -1L)) && (Microsoft.Win32.NativeMethods.GetProcessTimes(handle, out num3, out num5, out num5, out num5) && (num3 != startupTime)))
                                {
                                    currentInstancePointer.RefCount = 0;
                                    return;
                                }
                            }
                            using (Microsoft.Win32.SafeHandles.SafeProcessHandle handle2 = Microsoft.Win32.SafeHandles.SafeProcessHandle.OpenProcess(0x100000, false, processId))
                            {
                                if (!handle2.IsInvalid)
                                {
                                    using (ProcessWaitHandle handle3 = new ProcessWaitHandle(handle2))
                                    {
                                        if (handle3.WaitOne(0, false))
                                        {
                                            currentInstancePointer.RefCount = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static unsafe void WaitAndEnterCriticalSection(int* spinLockPointer, out bool taken)
        {
            WaitForCriticalSection(spinLockPointer);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                int num = Interlocked.CompareExchange(ref (int) ref spinLockPointer, 1, 0);
                taken = num == 0;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private static unsafe void WaitForCriticalSection(int* spinLockPointer)
        {
            int num = 0x1388;
            while ((num > 0) && (spinLockPointer[0] != 0))
            {
                if (spinLockPointer[0] != 0)
                {
                    Thread.Sleep(1);
                }
                num--;
            }
            if ((num == 0) && (spinLockPointer[0] != 0))
            {
                spinLockPointer[0] = 0;
            }
        }

        private FileMapping FileView
        {
            get
            {
                return this.categoryData.FileMapping;
            }
        }

        private static System.Diagnostics.ProcessData ProcessData
        {
            get
            {
                if (procData == null)
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    try
                    {
                        int currentProcessId = Microsoft.Win32.NativeMethods.GetCurrentProcessId();
                        long creation = -1L;
                        using (Microsoft.Win32.SafeHandles.SafeProcessHandle handle = Microsoft.Win32.SafeHandles.SafeProcessHandle.OpenProcess(0x400, false, currentProcessId))
                        {
                            if (!handle.IsInvalid)
                            {
                                long num3;
                                Microsoft.Win32.NativeMethods.GetProcessTimes(handle, out creation, out num3, out num3, out num3);
                            }
                        }
                        procData = new System.Diagnostics.ProcessData(currentProcessId, creation);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return procData;
            }
        }

        internal long Value
        {
            get
            {
                if (this.counterEntryPointer == null)
                {
                    return 0L;
                }
                return GetValue(this.counterEntryPointer);
            }
            set
            {
                if (this.counterEntryPointer != null)
                {
                    SetValue(this.counterEntryPointer, value);
                }
            }
        }

        private class CategoryData
        {
            public ArrayList CounterNames;
            public bool EnableReuse;
            public System.Diagnostics.SharedPerformanceCounter.FileMapping FileMapping;
            public string FileMappingName;
            public string MutexName;
            public bool UseUniqueSharedMemory;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CategoryEntry
        {
            public int SpinLock;
            public int CategoryNameHashCode;
            public int CategoryNameOffset;
            public int FirstInstanceOffset;
            public int NextCategoryOffset;
            public int IsConsistent;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CounterEntry
        {
            public int SpinLock;
            public int CounterNameHashCode;
            public int CounterNameOffset;
            public int LifetimeOffset;
            public long Value;
            public int NextCounterOffset;
            public int padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CounterEntryMisaligned
        {
            public int SpinLock;
            public int CounterNameHashCode;
            public int CounterNameOffset;
            public int LifetimeOffset;
            public int Value_lo;
            public int Value_hi;
            public int NextCounterOffset;
            public int padding2;
        }

        private class FileMapping
        {
            private Microsoft.Win32.SafeHandles.SafeFileMappingHandle fileMappingHandle;
            internal int FileMappingSize;
            private SafeFileMapViewHandle fileViewAddress;

            public FileMapping(string fileMappingName, int fileMappingSize, int initialOffset)
            {
                this.Initialize(fileMappingName, fileMappingSize, initialOffset);
            }

            private void Initialize(string fileMappingName, int fileMappingSize, int initialOffset)
            {
                string lpName = fileMappingName;
                SharedUtils.CheckEnvironment();
                SafeLocalMemHandle pSecurityDescriptor = null;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    string stringSecurityDescriptor = "D:(A;OICI;FRFWGRGW;;;AU)(A;OICI;FRFWGRGW;;;S-1-5-33)";
                    if (!SafeLocalMemHandle.ConvertStringSecurityDescriptorToSecurityDescriptor(stringSecurityDescriptor, 1, out pSecurityDescriptor, IntPtr.Zero))
                    {
                        throw new InvalidOperationException(SR.GetString("SetSecurityDescriptorFailed"));
                    }
                    Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES lpFileMappingAttributes = new Microsoft.Win32.NativeMethods.SECURITY_ATTRIBUTES {
                        lpSecurityDescriptor = pSecurityDescriptor,
                        bInheritHandle = false
                    };
                    int num = 14;
                    int millisecondsTimeout = 0;
                    bool flag = false;
                    while (!flag && (num > 0))
                    {
                        this.fileMappingHandle = Microsoft.Win32.NativeMethods.CreateFileMapping((IntPtr) (-1), lpFileMappingAttributes, 4, 0, fileMappingSize, lpName);
                        if ((Marshal.GetLastWin32Error() != 5) || !this.fileMappingHandle.IsInvalid)
                        {
                            flag = true;
                        }
                        else
                        {
                            this.fileMappingHandle.SetHandleAsInvalid();
                            this.fileMappingHandle = Microsoft.Win32.NativeMethods.OpenFileMapping(2, false, lpName);
                            if ((Marshal.GetLastWin32Error() != 2) || !this.fileMappingHandle.IsInvalid)
                            {
                                flag = true;
                                continue;
                            }
                            num--;
                            if (millisecondsTimeout == 0)
                            {
                                millisecondsTimeout = 10;
                            }
                            else
                            {
                                Thread.Sleep(millisecondsTimeout);
                                millisecondsTimeout *= 2;
                            }
                        }
                    }
                    if (this.fileMappingHandle.IsInvalid)
                    {
                        throw new InvalidOperationException(SR.GetString("CantCreateFileMapping"));
                    }
                    this.fileViewAddress = SafeFileMapViewHandle.MapViewOfFile(this.fileMappingHandle, 2, 0, 0, UIntPtr.Zero);
                    if (this.fileViewAddress.IsInvalid)
                    {
                        throw new InvalidOperationException(SR.GetString("CantMapFileView"));
                    }
                    Microsoft.Win32.NativeMethods.MEMORY_BASIC_INFORMATION buffer = new Microsoft.Win32.NativeMethods.MEMORY_BASIC_INFORMATION();
                    if (Microsoft.Win32.NativeMethods.VirtualQuery(this.fileViewAddress, ref buffer, (IntPtr) sizeof(Microsoft.Win32.NativeMethods.MEMORY_BASIC_INFORMATION)) == IntPtr.Zero)
                    {
                        throw new InvalidOperationException(SR.GetString("CantGetMappingSize"));
                    }
                    this.FileMappingSize = (int) ((uint) buffer.RegionSize);
                }
                finally
                {
                    if (pSecurityDescriptor != null)
                    {
                        pSecurityDescriptor.Close();
                    }
                    CodeAccessPermission.RevertAssert();
                }
                Microsoft.Win32.SafeNativeMethods.InterlockedCompareExchange(this.fileViewAddress.DangerousGetHandle(), initialOffset, 0);
            }

            internal IntPtr FileViewAddress
            {
                get
                {
                    if (this.fileViewAddress.IsInvalid)
                    {
                        throw new InvalidOperationException(SR.GetString("SharedMemoryGhosted"));
                    }
                    return this.fileViewAddress.DangerousGetHandle();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct InstanceEntry
        {
            public int SpinLock;
            public int InstanceNameHashCode;
            public int InstanceNameOffset;
            public int RefCount;
            public int FirstCounterOffset;
            public int NextInstanceOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ProcessLifetimeEntry
        {
            public int LifetimeType;
            public int ProcessId;
            public long StartupTime;
        }
    }
}

