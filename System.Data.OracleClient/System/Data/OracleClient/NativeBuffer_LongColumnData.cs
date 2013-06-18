namespace System.Data.OracleClient
{
    using System;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;

    internal sealed class NativeBuffer_LongColumnData : NativeBuffer
    {
        private int _chunkCount;
        private IntPtr _currentChunk;
        private static readonly int AllocationSize = 0x1ff8;
        private const int ChunkIsFree = -2;
        private const int ChunkToBeFilled = -1;
        private static readonly int LengthOrIndicatorOffset = IntPtr.Size;
        internal static readonly int MaxChunkSize = (AllocationSize - ReservedSize);
        private static readonly OutOfMemoryException OutOfMemory = new OutOfMemoryException();
        private static readonly int ReservedSize = (2 * IntPtr.Size);

        internal NativeBuffer_LongColumnData() : base(ReservedSize)
        {
            this._currentChunk = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                this._currentChunk = base.handle;
                Marshal.WriteIntPtr(this._currentChunk, 0, IntPtr.Zero);
                Marshal.WriteInt32(this._currentChunk, LengthOrIndicatorOffset, -2);
            }
        }

        internal static void CopyOutOfLineBytes(IntPtr longBuffer, int cbSourceOffset, byte[] destinationBuffer, int cbDestinationOffset, int cbCount)
        {
            if (IntPtr.Zero == longBuffer)
            {
                throw System.Data.Common.ADP.ArgumentNull("longBuffer");
            }
            int num4 = 0;
            int num3 = cbCount;
            while (num3 > 0)
            {
                longBuffer = Marshal.ReadIntPtr(longBuffer);
                if (IntPtr.Zero == longBuffer)
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                }
                int num = Marshal.ReadInt32(longBuffer, LengthOrIndicatorOffset);
                if ((num <= 0) || (num > MaxChunkSize))
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                }
                int num5 = cbSourceOffset - num4;
                if (num5 < num)
                {
                    int length = Math.Min(num3, (num4 + num) - cbSourceOffset);
                    Marshal.Copy(System.Data.Common.ADP.IntPtrOffset(longBuffer, num5 + ReservedSize), destinationBuffer, cbDestinationOffset, length);
                    cbSourceOffset += length;
                    cbDestinationOffset += length;
                    num3 -= length;
                }
                num4 += num;
            }
        }

        internal static void CopyOutOfLineChars(IntPtr longBuffer, int cchSourceOffset, char[] destinationBuffer, int cchDestinationOffset, int cchCount)
        {
            if (IntPtr.Zero == longBuffer)
            {
                throw System.Data.Common.ADP.ArgumentNull("longBuffer");
            }
            int num4 = 0;
            int num3 = cchCount;
            while (num3 > 0)
            {
                longBuffer = Marshal.ReadIntPtr(longBuffer);
                if (IntPtr.Zero == longBuffer)
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                }
                int num2 = Marshal.ReadInt32(longBuffer, LengthOrIndicatorOffset);
                if ((num2 <= 0) || (num2 > MaxChunkSize))
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                }
                if ((num2 & 1) != 0)
                {
                    throw System.Data.Common.ADP.InvalidCast();
                }
                int num5 = num2 / 2;
                int num6 = cchSourceOffset - num4;
                if (num6 < num5)
                {
                    int length = Math.Min(num3, (num4 + num5) - cchSourceOffset);
                    Marshal.Copy(System.Data.Common.ADP.IntPtrOffset(longBuffer, (num6 * System.Data.Common.ADP.CharSize) + ReservedSize), destinationBuffer, cchDestinationOffset, length);
                    cchSourceOffset += length;
                    cchDestinationOffset += length;
                    num3 -= length;
                }
                num4 += num5;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal IntPtr GetChunk(out IntPtr lengthPtr)
        {
            IntPtr ptr = Marshal.ReadIntPtr(this._currentChunk);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (IntPtr.Zero == ptr)
                {
                    ptr = System.Data.Common.SafeNativeMethods.LocalAlloc(0, (IntPtr) AllocationSize);
                    if (IntPtr.Zero != ptr)
                    {
                        Marshal.WriteIntPtr(ptr, IntPtr.Zero);
                        Marshal.WriteIntPtr(this._currentChunk, ptr);
                    }
                }
                if (IntPtr.Zero != ptr)
                {
                    Marshal.WriteInt32(ptr, LengthOrIndicatorOffset, -1);
                    this._currentChunk = ptr;
                    this._chunkCount++;
                }
            }
            if (IntPtr.Zero == ptr)
            {
                throw new OutOfMemoryException();
            }
            lengthPtr = System.Data.Common.ADP.IntPtrOffset(ptr, LengthOrIndicatorOffset);
            return System.Data.Common.ADP.IntPtrOffset(ptr, ReservedSize);
        }

        protected override bool ReleaseHandle()
        {
            IntPtr ptr2;
            for (IntPtr ptr = base.handle; IntPtr.Zero != ptr; ptr = ptr2)
            {
                ptr2 = Marshal.ReadIntPtr(ptr);
                System.Data.Common.SafeNativeMethods.LocalFree(ptr);
            }
            return true;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal void Reset()
        {
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    IntPtr ptr2;
                    for (IntPtr ptr = this._currentChunk = base.handle; IntPtr.Zero != ptr; ptr = ptr2)
                    {
                        ptr2 = Marshal.ReadIntPtr(ptr);
                        Marshal.WriteInt32(ptr, LengthOrIndicatorOffset, -2);
                    }
                    this._chunkCount = 0;
                }
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        internal int TotalLengthInBytes
        {
            get
            {
                IntPtr handle = base.handle;
                int num3 = 0;
                for (int i = 0; i < this._chunkCount; i++)
                {
                    handle = Marshal.ReadIntPtr(handle);
                    if (handle == IntPtr.Zero)
                    {
                        throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                    }
                    int num = Marshal.ReadInt32(handle, LengthOrIndicatorOffset);
                    if (num <= 0)
                    {
                        throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                    }
                    if (num > MaxChunkSize)
                    {
                        throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.InvalidLongBuffer);
                    }
                    num3 += num;
                }
                return num3;
            }
        }
    }
}

