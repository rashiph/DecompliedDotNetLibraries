namespace System.Runtime.InteropServices
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [SecurityCritical]
    public abstract class SafeBuffer : SafeHandleZeroOrMinusOneIsInvalid
    {
        private UIntPtr _numBytes;
        private static readonly UIntPtr Uninitialized = ((UIntPtr.Size == 4) ? ((UIntPtr) (-1)) : ((UIntPtr) (-1L)));

        protected SafeBuffer(bool ownsHandle) : base(ownsHandle)
        {
            this._numBytes = Uninitialized;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), CLSCompliant(false)]
        public unsafe void AcquirePointer(ref byte* pointer)
        {
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            pointer = (byte*) IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                bool success = false;
                base.DangerousAddRef(ref success);
                pointer = (byte*) base.handle;
            }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void GenericPtrToStructure<T>(byte* ptr, out T structure, uint sizeofT) where T: struct
        {
            structure = default(T);
            PtrToStructureNative(ptr, __makeref(structure), sizeofT);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void GenericStructureToPtr<T>(ref T structure, byte* ptr, uint sizeofT) where T: struct
        {
            StructureToPtrNative(__makeref(structure), ptr, sizeofT);
        }

        [CLSCompliant(false)]
        public void Initialize<T>(uint numElements) where T: struct
        {
            this.Initialize(numElements, Marshal.AlignedSizeOf<T>());
        }

        [CLSCompliant(false)]
        public void Initialize(ulong numBytes)
        {
            if (numBytes < 0L)
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((IntPtr.Size == 4) && (numBytes > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_AddressSpace"));
            }
            if (numBytes >= ((ulong) Uninitialized))
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_UIntPtrMax-1"));
            }
            this._numBytes = (UIntPtr) numBytes;
        }

        [CLSCompliant(false)]
        public void Initialize(uint numElements, uint sizeOfEachElement)
        {
            if (numElements < 0)
            {
                throw new ArgumentOutOfRangeException("numElements", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (sizeOfEachElement < 0)
            {
                throw new ArgumentOutOfRangeException("sizeOfEachElement", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((IntPtr.Size == 4) && ((numElements * sizeOfEachElement) > uint.MaxValue))
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_AddressSpace"));
            }
            if ((numElements * sizeOfEachElement) >= ((ulong) Uninitialized))
            {
                throw new ArgumentOutOfRangeException("numElements", Environment.GetResourceString("ArgumentOutOfRange_UIntPtrMax-1"));
            }
            this._numBytes = (UIntPtr) (numElements * sizeOfEachElement);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static void NotEnoughRoom()
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_BufferTooSmall"));
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static InvalidOperationException NotInitialized()
        {
            return new InvalidOperationException(Environment.GetResourceString("InvalidOperation_MustCallInitialize"));
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern unsafe void PtrToStructureNative(byte* ptr, TypedReference structure, uint sizeofT);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), CLSCompliant(false)]
        public unsafe T Read<T>(ulong byteOffset) where T: struct
        {
            T local;
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            uint sizeofT = Marshal.SizeOf<T>();
            byte* ptr = (byte*) (((void*) base.handle) + byteOffset);
            this.SpaceCheck(ptr, (ulong) sizeofT);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                GenericPtrToStructure<T>(ptr, out local, sizeofT);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
            return local;
        }

        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public unsafe void ReadArray<T>(ulong byteOffset, T[] array, int index, int count) where T: struct
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            uint sizeofT = Marshal.SizeOf<T>();
            uint num2 = Marshal.AlignedSizeOf<T>();
            byte* ptr = (byte*) (((void*) base.handle) + byteOffset);
            this.SpaceCheck(ptr, (ulong) (num2 * count));
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                for (int i = 0; i < count; i++)
                {
                    GenericPtrToStructure<T>(ptr + ((byte*) (num2 * i)), out array[i + index], sizeofT);
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

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public void ReleasePointer()
        {
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            base.DangerousRelease();
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private unsafe void SpaceCheck(byte* ptr, ulong sizeInBytes)
        {
            if (((ulong) this._numBytes) < sizeInBytes)
            {
                NotEnoughRoom();
            }
            if (((long) ((ptr - ((void*) base.handle)) / 1)) > (((ulong) this._numBytes) - sizeInBytes))
            {
                NotEnoughRoom();
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static extern unsafe void StructureToPtrNative(TypedReference structure, byte* ptr, uint sizeofT);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), CLSCompliant(false)]
        public unsafe void Write<T>(ulong byteOffset, T value) where T: struct
        {
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            uint sizeofT = Marshal.SizeOf<T>();
            byte* ptr = (byte*) (((void*) base.handle) + byteOffset);
            this.SpaceCheck(ptr, (ulong) sizeofT);
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                GenericStructureToPtr<T>(ref value, ptr, sizeofT);
            }
            finally
            {
                if (success)
                {
                    base.DangerousRelease();
                }
            }
        }

        [CLSCompliant(false), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public unsafe void WriteArray<T>(ulong byteOffset, T[] array, int index, int count) where T: struct
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - index) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._numBytes == Uninitialized)
            {
                throw NotInitialized();
            }
            uint sizeofT = Marshal.SizeOf<T>();
            uint num2 = Marshal.AlignedSizeOf<T>();
            byte* ptr = (byte*) (((void*) base.handle) + byteOffset);
            this.SpaceCheck(ptr, (ulong) (num2 * count));
            bool success = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                base.DangerousAddRef(ref success);
                for (int i = 0; i < count; i++)
                {
                    GenericStructureToPtr<T>(ref array[i + index], ptr + ((byte*) (num2 * i)), sizeofT);
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

        [CLSCompliant(false)]
        public ulong ByteLength
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            get
            {
                if (this._numBytes == Uninitialized)
                {
                    throw NotInitialized();
                }
                return (ulong) this._numBytes;
            }
        }
    }
}

