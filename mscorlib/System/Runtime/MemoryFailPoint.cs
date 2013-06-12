namespace System.Runtime
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    public sealed class MemoryFailPoint : CriticalFinalizerObject, IDisposable
    {
        private bool _mustSubtractReservation;
        private ulong _reservedMemory;
        private const int CheckThreshold = 0x2710;
        private static readonly uint GCSegmentSize;
        private static long LastKnownFreeAddressSpace = 0L;
        private static long LastTimeCheckingAddressSpace = 0L;
        private const int LowMemoryFudgeFactor = 0x1000000;
        private static readonly ulong TopOfMemory;

        [SecuritySafeCritical]
        static MemoryFailPoint()
        {
            GetMemorySettings(out GCSegmentSize, out TopOfMemory);
        }

        [SecurityCritical]
        public unsafe MemoryFailPoint(int sizeInMegabytes)
        {
            if (sizeInMegabytes <= 0)
            {
                throw new ArgumentOutOfRangeException("sizeInMegabytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            ulong num = ((ulong) sizeInMegabytes) << 20;
            this._reservedMemory = num;
            ulong size = (ulong) (Math.Ceiling((double) (((float) num) / ((float) GCSegmentSize))) * GCSegmentSize);
            if (size >= TopOfMemory)
            {
                throw new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_TooBig"));
            }
            ulong availPageFile = 0L;
            ulong totalAddressSpaceFree = 0L;
            for (int i = 0; i < 3; i++)
            {
                CheckForAvailableMemory(out availPageFile, out totalAddressSpaceFree);
                ulong memoryFailPointReservedMemory = SharedStatics.MemoryFailPointReservedMemory;
                ulong num7 = size + memoryFailPointReservedMemory;
                bool flag = (num7 < size) || (num7 < memoryFailPointReservedMemory);
                bool flag2 = (availPageFile < (num7 + ((ulong) 0x1000000L))) || flag;
                bool flag3 = (totalAddressSpaceFree < num7) || flag;
                long tickCount = Environment.TickCount;
                if (((tickCount > (LastTimeCheckingAddressSpace + 0x2710L)) || (tickCount < LastTimeCheckingAddressSpace)) || (LastKnownFreeAddressSpace < size))
                {
                    CheckForFreeAddressSpace(size, false);
                }
                bool flag4 = LastKnownFreeAddressSpace < size;
                if ((!flag2 && !flag3) && !flag4)
                {
                    break;
                }
                switch (i)
                {
                    case 0:
                    {
                        GC.Collect();
                        continue;
                    }
                    case 1:
                        if (!flag2)
                        {
                            continue;
                        }
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            continue;
                        }
                        finally
                        {
                            UIntPtr numBytes = new UIntPtr(size);
                            void* address = Win32Native.VirtualAlloc(null, numBytes, 0x1000, 4);
                            if ((address != null) && !Win32Native.VirtualFree(address, UIntPtr.Zero, 0x8000))
                            {
                                __Error.WinIOError();
                            }
                        }
                        break;

                    case 2:
                        break;

                    default:
                    {
                        continue;
                    }
                }
                if (flag2 || flag3)
                {
                    InsufficientMemoryException exception = new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint"));
                    throw exception;
                }
                if (flag4)
                {
                    InsufficientMemoryException exception2 = new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_VAFrag"));
                    throw exception2;
                }
            }
            Interlocked.Add(ref LastKnownFreeAddressSpace, (long) -num);
            if (LastKnownFreeAddressSpace < 0L)
            {
                CheckForFreeAddressSpace(size, true);
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                SharedStatics.AddMemoryFailPointReservation((long) num);
                this._mustSubtractReservation = true;
            }
        }

        [SecurityCritical]
        private static void CheckForAvailableMemory(out ulong availPageFile, out ulong totalAddressSpaceFree)
        {
            Win32Native.MEMORYSTATUSEX buffer = new Win32Native.MEMORYSTATUSEX();
            if (!Win32Native.GlobalMemoryStatusEx(buffer))
            {
                __Error.WinIOError();
            }
            availPageFile = buffer.availPageFile;
            totalAddressSpaceFree = buffer.availVirtual;
        }

        [SecurityCritical]
        private static bool CheckForFreeAddressSpace(ulong size, bool shouldThrow)
        {
            ulong num = MemFreeAfterAddress(null, size);
            LastKnownFreeAddressSpace = (long) num;
            LastTimeCheckingAddressSpace = Environment.TickCount;
            if ((num < size) && shouldThrow)
            {
                throw new InsufficientMemoryException(Environment.GetResourceString("InsufficientMemory_MemFailPoint_VAFrag"));
            }
            return (num >= size);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private void Dispose(bool disposing)
        {
            if (this._mustSubtractReservation)
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    SharedStatics.AddMemoryFailPointReservation((long) -this._reservedMemory);
                    this._mustSubtractReservation = false;
                }
            }
        }

        [SecuritySafeCritical]
        ~MemoryFailPoint()
        {
            this.Dispose(false);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetMemorySettings(out uint maxGCSegmentSize, out ulong topOfMemory);
        [SecurityCritical]
        private static unsafe ulong MemFreeAfterAddress(void* address, ulong size)
        {
            if (size >= TopOfMemory)
            {
                return 0L;
            }
            ulong num = 0L;
            Win32Native.MEMORY_BASIC_INFORMATION structure = new Win32Native.MEMORY_BASIC_INFORMATION();
            IntPtr sizeOfBuffer = (IntPtr) Marshal.SizeOf(structure);
            while ((((ulong) address) + size) < TopOfMemory)
            {
                if (Win32Native.VirtualQuery(address, ref structure, sizeOfBuffer) == IntPtr.Zero)
                {
                    __Error.WinIOError();
                }
                ulong num2 = structure.RegionSize.ToUInt64();
                if (structure.State == 0x10000)
                {
                    if (num2 >= size)
                    {
                        return num2;
                    }
                    num = Math.Max(num, num2);
                }
                address = (void*) (((ulong) address) + num2);
            }
            return num;
        }
    }
}

