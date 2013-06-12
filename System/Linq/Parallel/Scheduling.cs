namespace System.Linq.Parallel
{
    using System;
    using System.Runtime.InteropServices;

    internal static class Scheduling
    {
        internal const int DEFAULT_BOUNDED_BUFFER_CAPACITY = 0x200;
        internal const int DEFAULT_BYTES_PER_CHUNK = 0x200;
        internal static int DefaultDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 0x3f);
        internal const bool DefaultPreserveOrder = false;
        internal const int MAX_SUPPORTED_DOP = 0x3f;
        internal const int ZOMBIED_PRODUCER_TIMEOUT = -1;

        internal static int GetDefaultChunkSize<T>()
        {
            if (typeof(T).IsValueType)
            {
                if (typeof(T).StructLayoutAttribute.Value == LayoutKind.Explicit)
                {
                    return Math.Max(1, 0x200 / Marshal.SizeOf(typeof(T)));
                }
                return 0x80;
            }
            return (0x200 / IntPtr.Size);
        }

        internal static int GetDefaultDegreeOfParallelism()
        {
            return DefaultDegreeOfParallelism;
        }
    }
}

