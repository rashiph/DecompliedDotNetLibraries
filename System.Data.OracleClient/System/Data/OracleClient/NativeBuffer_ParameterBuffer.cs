namespace System.Data.OracleClient
{
    using System;

    internal sealed class NativeBuffer_ParameterBuffer : NativeBuffer
    {
        internal NativeBuffer_ParameterBuffer(int initialSize) : base(initialSize, true)
        {
        }
    }
}

