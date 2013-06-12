namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    [ComVisible(true)]
    public class ToBase64Transform : ICryptoTransform, IDisposable
    {
        public void Clear()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            this.Clear();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        ~ToBase64Transform()
        {
            this.Dispose(false);
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((inputCount < 0) || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            char[] outArray = new char[4];
            Convert.ToBase64CharArray(inputBuffer, inputOffset, 3, outArray, 0);
            byte[] bytes = Encoding.ASCII.GetBytes(outArray);
            if (bytes.Length != 4)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_SSE_InvalidDataSize"));
            }
            Buffer.BlockCopy(bytes, 0, outputBuffer, outputOffset, bytes.Length);
            return bytes.Length;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((inputCount < 0) || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (inputCount == 0)
            {
                return new byte[0];
            }
            char[] outArray = new char[4];
            Convert.ToBase64CharArray(inputBuffer, inputOffset, inputCount, outArray, 0);
            return Encoding.ASCII.GetBytes(outArray);
        }

        public virtual bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return false;
            }
        }

        public int InputBlockSize
        {
            get
            {
                return 3;
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return 4;
            }
        }
    }
}

