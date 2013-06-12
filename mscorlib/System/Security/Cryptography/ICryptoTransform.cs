namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public interface ICryptoTransform : IDisposable
    {
        int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset);
        byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);

        bool CanReuseTransform { get; }

        bool CanTransformMultipleBlocks { get; }

        int InputBlockSize { get; }

        int OutputBlockSize { get; }
    }
}

