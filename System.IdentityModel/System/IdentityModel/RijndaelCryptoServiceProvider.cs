namespace System.IdentityModel
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class RijndaelCryptoServiceProvider : Rijndael
    {
        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbKey");
            }
            if (rgbIV == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbIV");
            }
            if (base.ModeValue != CipherMode.CBC)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AESCipherModeNotSupported", new object[] { base.ModeValue })));
            }
            return new RijndaelCryptoTransform(rgbKey, rgbIV, base.PaddingValue, base.BlockSizeValue, false);
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgbIV)
        {
            if (rgbKey == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbKey");
            }
            if (rgbIV == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rgbIV");
            }
            if (base.ModeValue != CipherMode.CBC)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AESCipherModeNotSupported", new object[] { base.ModeValue })));
            }
            return new RijndaelCryptoTransform(rgbKey, rgbIV, base.PaddingValue, base.BlockSizeValue, true);
        }

        public override void GenerateIV()
        {
            base.IVValue = new byte[base.BlockSizeValue / 8];
            CryptoHelper.RandomNumberGenerator.GetBytes(base.IVValue);
        }

        public override void GenerateKey()
        {
            base.KeyValue = new byte[base.KeySizeValue / 8];
            CryptoHelper.RandomNumberGenerator.GetBytes(base.KeyValue);
        }

        private class RijndaelCryptoTransform : ICryptoTransform, IDisposable
        {
            private int blockSize;
            private byte[] depadBuffer;
            private bool encrypt;
            private System.IdentityModel.SafeKeyHandle keyHandle = System.IdentityModel.SafeKeyHandle.InvalidHandle;
            private PaddingMode paddingMode;
            private System.IdentityModel.SafeProvHandle provHandle = System.IdentityModel.SafeProvHandle.InvalidHandle;

            public unsafe RijndaelCryptoTransform(byte[] rgbKey, byte[] rgbIV, PaddingMode paddingMode, int blockSizeBits, bool encrypt)
            {
                if (((rgbKey.Length != 0x10) && (rgbKey.Length != 0x18)) && (rgbKey.Length != 0x20))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AESKeyLengthNotSupported", new object[] { rgbKey.Length * 8 })));
                }
                if (rgbIV.Length != 0x10)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AESIVLengthNotSupported", new object[] { rgbIV.Length * 8 })));
                }
                if ((paddingMode != PaddingMode.PKCS7) && (paddingMode != PaddingMode.ISO10126))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.IdentityModel.SR.GetString("AESPaddingModeNotSupported", new object[] { paddingMode })));
                }
                this.paddingMode = paddingMode;
                this.blockSize = blockSizeBits / 8;
                this.encrypt = encrypt;
                System.IdentityModel.SafeProvHandle phProv = null;
                System.IdentityModel.SafeKeyHandle phKey = null;
                try
                {
                    ThrowIfFalse("AESCryptAcquireContextFailed", System.IdentityModel.NativeMethods.CryptAcquireContextW(out phProv, null, null, 0x18, 0xf0000000));
                    int cbData = PLAINTEXTKEYBLOBHEADER.SizeOf + rgbKey.Length;
                    byte[] dst = new byte[cbData];
                    Buffer.BlockCopy(rgbKey, 0, dst, PLAINTEXTKEYBLOBHEADER.SizeOf, rgbKey.Length);
                    fixed (IntPtr* ptrRef = dst)
                    {
                        PLAINTEXTKEYBLOBHEADER* plaintextkeyblobheaderPtr = (PLAINTEXTKEYBLOBHEADER*) ptrRef;
                        plaintextkeyblobheaderPtr->bType = 8;
                        plaintextkeyblobheaderPtr->bVersion = 2;
                        plaintextkeyblobheaderPtr->reserved = 0;
                        if (rgbKey.Length == 0x10)
                        {
                            plaintextkeyblobheaderPtr->aiKeyAlg = 0x660e;
                        }
                        else if (rgbKey.Length == 0x18)
                        {
                            plaintextkeyblobheaderPtr->aiKeyAlg = 0x660f;
                        }
                        else
                        {
                            plaintextkeyblobheaderPtr->aiKeyAlg = 0x6610;
                        }
                        plaintextkeyblobheaderPtr->keyLength = rgbKey.Length;
                        phKey = System.IdentityModel.SafeKeyHandle.SafeCryptImportKey(phProv, (void*) ptrRef, cbData);
                    }
                    fixed (IntPtr* ptrRef2 = rgbIV)
                    {
                        ThrowIfFalse("AESCryptSetKeyParamFailed", System.IdentityModel.NativeMethods.CryptSetKeyParam(phKey, 1, (void*) ptrRef2, 0));
                    }
                    this.keyHandle = phKey;
                    this.provHandle = phProv;
                    phKey = null;
                    phProv = null;
                }
                finally
                {
                    if (phKey != null)
                    {
                        phKey.Close();
                    }
                    if (phProv != null)
                    {
                        phProv.Close();
                    }
                }
            }

            private unsafe int DecryptData(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, bool final)
            {
                bool flag = final && (this.paddingMode == PaddingMode.PKCS7);
                int dwDataLen = inputCount;
                if (dwDataLen > 0)
                {
                    Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
                    fixed (IntPtr* ptrRef = ((IntPtr*) &(outputBuffer[outputOffset])))
                    {
                        ThrowIfFalse("AESCryptDecryptFailed", System.IdentityModel.NativeMethods.CryptDecrypt(this.keyHandle, IntPtr.Zero, flag, 0, (void*) ptrRef, ref dwDataLen));
                    }
                }
                if (!flag && final)
                {
                    byte num2 = outputBuffer[(outputOffset + dwDataLen) - 1];
                    dwDataLen -= num2;
                }
                return dwDataLen;
            }

            public void Dispose()
            {
                try
                {
                    this.keyHandle.Close();
                }
                finally
                {
                    this.provHandle.Close();
                }
            }

            private void DoPadding(ref byte[] tempBuffer, ref int tempOffset, ref int dwCount)
            {
                int num = dwCount % this.blockSize;
                int count = this.blockSize - num;
                byte[] data = new byte[count];
                CryptoHelper.RandomNumberGenerator.GetBytes(data);
                data[count - 1] = (byte) count;
                int num3 = (dwCount + count) + this.blockSize;
                if (tempBuffer.Length >= (tempOffset + num3))
                {
                    Buffer.BlockCopy(data, 0, tempBuffer, tempOffset + dwCount, count);
                }
                else
                {
                    byte[] dst = new byte[num3];
                    Buffer.BlockCopy(tempBuffer, tempOffset, dst, 0, dwCount);
                    Buffer.BlockCopy(data, 0, dst, dwCount, count);
                    Array.Clear(tempBuffer, tempOffset, dwCount);
                    tempBuffer = dst;
                    tempOffset = 0;
                }
                dwCount += count;
            }

            private unsafe int EncryptData(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset, bool final)
            {
                if ((outputBuffer.Length - outputOffset) < inputCount)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputBuffer", System.IdentityModel.SR.GetString("AESInsufficientOutputBuffer", new object[] { outputBuffer.Length - outputOffset, inputCount })));
                }
                bool flag = final && (this.paddingMode == PaddingMode.ISO10126);
                byte[] dst = outputBuffer;
                int dstOffset = outputOffset;
                int dwCount = inputCount;
                bool flag2 = true;
                Buffer.BlockCopy(inputBuffer, inputOffset, dst, dstOffset, inputCount);
                try
                {
                    if (flag)
                    {
                        this.DoPadding(ref dst, ref dstOffset, ref dwCount);
                    }
                    fixed (IntPtr* ptrRef = ((IntPtr*) &(dst[dstOffset])))
                    {
                        ThrowIfFalse("AESCryptEncryptFailed", System.IdentityModel.NativeMethods.CryptEncrypt(this.keyHandle, IntPtr.Zero, final, 0, (void*) ptrRef, ref dwCount, dst.Length - dstOffset));
                    }
                    flag2 = false;
                }
                finally
                {
                    if (flag2)
                    {
                        Array.Clear(dst, dstOffset, inputCount);
                    }
                }
                if (flag)
                {
                    dwCount -= this.blockSize;
                }
                if (dst != outputBuffer)
                {
                    Buffer.BlockCopy(dst, dstOffset, outputBuffer, outputOffset, dwCount);
                }
                return dwCount;
            }

            private static void ThrowIfFalse(string sr, bool ret)
            {
                if (!ret)
                {
                    int error = Marshal.GetLastWin32Error();
                    string str = (error != 0) ? new Win32Exception(error).Message : string.Empty;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CryptographicException(System.IdentityModel.SR.GetString(sr, new object[] { str })));
                }
            }

            public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                if (inputBuffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputBuffer");
                }
                if (outputBuffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("outputBuffer");
                }
                if (inputOffset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (inputCount <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputCount", System.IdentityModel.SR.GetString("ValueMustBeGreaterThanZero")));
                }
                if (outputOffset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputOffset", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if ((inputCount % this.blockSize) != 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.IdentityModel.SR.GetString("AESInvalidInputBlockSize", new object[] { inputCount, this.blockSize })));
                }
                if ((inputBuffer.Length - inputCount) < inputOffset)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, (inputBuffer.Length - inputCount) - 1 })));
                }
                if (outputBuffer.Length < outputOffset)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("outputOffset", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, outputBuffer.Length - 1 })));
                }
                if (this.encrypt)
                {
                    return this.EncryptData(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset, false);
                }
                if (this.paddingMode == PaddingMode.PKCS7)
                {
                    return this.DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset, false);
                }
                if (this.depadBuffer == null)
                {
                    this.depadBuffer = new byte[this.blockSize];
                    int num = inputCount - this.blockSize;
                    Buffer.BlockCopy(inputBuffer, inputOffset + num, this.depadBuffer, 0, this.blockSize);
                    if (num > 0)
                    {
                        return this.DecryptData(inputBuffer, inputOffset, num, outputBuffer, outputOffset, false);
                    }
                    return 0;
                }
                int num2 = this.DecryptData(this.depadBuffer, 0, this.depadBuffer.Length, outputBuffer, outputOffset, false);
                outputOffset += num2;
                int num3 = inputCount - this.blockSize;
                Buffer.BlockCopy(inputBuffer, inputOffset + num3, this.depadBuffer, 0, this.blockSize);
                return (num2 + ((num3 <= 0) ? 0 : this.DecryptData(inputBuffer, inputOffset, num3, outputBuffer, outputOffset, false)));
            }

            public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
            {
                if (inputBuffer == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("inputBuffer");
                }
                if (inputOffset < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if (inputCount < 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputCount", System.IdentityModel.SR.GetString("ValueMustBeNonNegative")));
                }
                if ((inputBuffer.Length - inputCount) < inputOffset)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("inputOffset", System.IdentityModel.SR.GetString("ValueMustBeInRange", new object[] { 0, (inputBuffer.Length - inputCount) - 1 })));
                }
                if (this.encrypt)
                {
                    int num = this.blockSize - (inputCount % this.blockSize);
                    int num2 = inputCount + num;
                    if (this.paddingMode == PaddingMode.ISO10126)
                    {
                        num2 += this.blockSize;
                    }
                    byte[] buffer = new byte[num2];
                    int len = this.EncryptData(inputBuffer, inputOffset, inputCount, buffer, 0, true);
                    return this.TruncateBuffer(buffer, len);
                }
                if (this.paddingMode == PaddingMode.PKCS7)
                {
                    byte[] buffer2 = new byte[inputCount];
                    int num4 = this.DecryptData(inputBuffer, inputOffset, inputCount, buffer2, 0, true);
                    return this.TruncateBuffer(buffer2, num4);
                }
                if (this.depadBuffer == null)
                {
                    byte[] buffer3 = new byte[inputCount];
                    int num5 = this.DecryptData(inputBuffer, inputOffset, inputCount, buffer3, 0, true);
                    return this.TruncateBuffer(buffer3, num5);
                }
                byte[] outputBuffer = new byte[this.depadBuffer.Length + inputCount];
                int outputOffset = this.DecryptData(this.depadBuffer, 0, this.depadBuffer.Length, outputBuffer, 0, false);
                outputOffset += this.DecryptData(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset, true);
                return this.TruncateBuffer(outputBuffer, outputOffset);
            }

            private byte[] TruncateBuffer(byte[] buffer, int len)
            {
                if (len == buffer.Length)
                {
                    return buffer;
                }
                byte[] dst = new byte[len];
                Buffer.BlockCopy(buffer, 0, dst, 0, len);
                if (!this.encrypt)
                {
                    Array.Clear(buffer, 0, buffer.Length);
                }
                return dst;
            }

            public bool CanReuseTransform
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
                    return true;
                }
            }

            public int InputBlockSize
            {
                get
                {
                    return this.blockSize;
                }
            }

            public int OutputBlockSize
            {
                get
                {
                    return this.blockSize;
                }
            }
        }
    }
}

