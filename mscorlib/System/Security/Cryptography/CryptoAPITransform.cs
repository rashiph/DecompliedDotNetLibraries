namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true)]
    public sealed class CryptoAPITransform : ICryptoTransform, IDisposable
    {
        private byte[] _depadBuffer;
        private byte[] _rgbKey;
        [SecurityCritical]
        private SafeKeyHandle _safeKeyHandle;
        [SecurityCritical]
        private SafeProvHandle _safeProvHandle;
        private int BlockSizeValue;
        private CryptoAPITransformMode encryptOrDecrypt;
        private byte[] IVValue;
        private CipherMode ModeValue;
        private PaddingMode PaddingValue;

        private CryptoAPITransform()
        {
        }

        [SecurityCritical]
        internal CryptoAPITransform(int algid, int cArgs, int[] rgArgIds, object[] rgArgValues, byte[] rgbKey, PaddingMode padding, CipherMode cipherChainingMode, int blockSize, int feedbackSize, bool useSalt, CryptoAPITransformMode encDecMode)
        {
            this.BlockSizeValue = blockSize;
            this.ModeValue = cipherChainingMode;
            this.PaddingValue = padding;
            this.encryptOrDecrypt = encDecMode;
            int[] destinationArray = new int[rgArgIds.Length];
            Array.Copy(rgArgIds, destinationArray, rgArgIds.Length);
            this._rgbKey = new byte[rgbKey.Length];
            Array.Copy(rgbKey, this._rgbKey, rgbKey.Length);
            object[] objArray = new object[rgArgValues.Length];
            for (int i = 0; i < rgArgValues.Length; i++)
            {
                if (rgArgValues[i] is byte[])
                {
                    byte[] sourceArray = (byte[]) rgArgValues[i];
                    byte[] buffer3 = new byte[sourceArray.Length];
                    Array.Copy(sourceArray, buffer3, sourceArray.Length);
                    objArray[i] = buffer3;
                }
                else if (rgArgValues[i] is int)
                {
                    objArray[i] = (int) rgArgValues[i];
                }
                else if (rgArgValues[i] is CipherMode)
                {
                    objArray[i] = (int) rgArgValues[i];
                }
            }
            this._safeProvHandle = Utils.AcquireProvHandle(new CspParameters(Utils.DefaultRsaProviderType));
            SafeKeyHandle invalidHandle = SafeKeyHandle.InvalidHandle;
            Utils._ImportBulkKey(this._safeProvHandle, algid, useSalt, this._rgbKey, ref invalidHandle);
            this._safeKeyHandle = invalidHandle;
            for (int j = 0; j < cArgs; j++)
            {
                int num;
                switch (rgArgIds[j])
                {
                    case 1:
                    {
                        this.IVValue = (byte[]) objArray[j];
                        byte[] iVValue = this.IVValue;
                        Utils.SetKeyParamRgb(this._safeKeyHandle, destinationArray[j], iVValue, iVValue.Length);
                        continue;
                    }
                    case 4:
                        this.ModeValue = (CipherMode) objArray[j];
                        num = (int) objArray[j];
                        break;

                    case 5:
                        num = (int) objArray[j];
                        break;

                    case 0x13:
                        num = (int) objArray[j];
                        break;

                    default:
                        throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidKeyParameter"), "_rgArgIds[i]");
                }
                Utils.SetKeyParamDw(this._safeKeyHandle, destinationArray[j], num);
            }
        }

        [SecuritySafeCritical]
        public void Clear()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.Clear();
        }

        [SecurityCritical]
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._rgbKey != null)
                {
                    Array.Clear(this._rgbKey, 0, this._rgbKey.Length);
                    this._rgbKey = null;
                }
                if (this.IVValue != null)
                {
                    Array.Clear(this.IVValue, 0, this.IVValue.Length);
                    this.IVValue = null;
                }
                if (this._depadBuffer != null)
                {
                    Array.Clear(this._depadBuffer, 0, this._depadBuffer.Length);
                    this._depadBuffer = null;
                }
                if ((this._safeKeyHandle != null) && !this._safeKeyHandle.IsClosed)
                {
                    this._safeKeyHandle.Dispose();
                }
                if ((this._safeProvHandle != null) && !this._safeProvHandle.IsClosed)
                {
                    this._safeProvHandle.Dispose();
                }
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void Reset()
        {
            this._depadBuffer = null;
            byte[] outputBuffer = null;
            Utils._EncryptData(this._safeKeyHandle, new byte[0], 0, 0, ref outputBuffer, 0, this.PaddingValue, true);
        }

        [SecuritySafeCritical]
        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (outputBuffer == null)
            {
                throw new ArgumentNullException("outputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (((inputCount <= 0) || ((inputCount % this.InputBlockSize) != 0)) || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"));
            }
            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this.encryptOrDecrypt == CryptoAPITransformMode.Encrypt)
            {
                return Utils._EncryptData(this._safeKeyHandle, inputBuffer, inputOffset, inputCount, ref outputBuffer, outputOffset, this.PaddingValue, false);
            }
            if ((this.PaddingValue == PaddingMode.Zeros) || (this.PaddingValue == PaddingMode.None))
            {
                return Utils._DecryptData(this._safeKeyHandle, inputBuffer, inputOffset, inputCount, ref outputBuffer, outputOffset, this.PaddingValue, false);
            }
            if (this._depadBuffer == null)
            {
                this._depadBuffer = new byte[this.InputBlockSize];
                int num = inputCount - this.InputBlockSize;
                Buffer.InternalBlockCopy(inputBuffer, inputOffset + num, this._depadBuffer, 0, this.InputBlockSize);
                return Utils._DecryptData(this._safeKeyHandle, inputBuffer, inputOffset, num, ref outputBuffer, outputOffset, this.PaddingValue, false);
            }
            int num2 = Utils._DecryptData(this._safeKeyHandle, this._depadBuffer, 0, this._depadBuffer.Length, ref outputBuffer, outputOffset, this.PaddingValue, false);
            outputOffset += this.OutputBlockSize;
            int cb = inputCount - this.InputBlockSize;
            Buffer.InternalBlockCopy(inputBuffer, inputOffset + cb, this._depadBuffer, 0, this.InputBlockSize);
            num2 = Utils._DecryptData(this._safeKeyHandle, inputBuffer, inputOffset, cb, ref outputBuffer, outputOffset, this.PaddingValue, false);
            return (this.OutputBlockSize + num2);
        }

        [SecuritySafeCritical]
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
            if (this.encryptOrDecrypt == CryptoAPITransformMode.Encrypt)
            {
                byte[] buffer = null;
                Utils._EncryptData(this._safeKeyHandle, inputBuffer, inputOffset, inputCount, ref buffer, 0, this.PaddingValue, true);
                this.Reset();
                return buffer;
            }
            if ((inputCount % this.InputBlockSize) != 0)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_SSD_InvalidDataSize"));
            }
            if (this._depadBuffer == null)
            {
                byte[] buffer2 = null;
                Utils._DecryptData(this._safeKeyHandle, inputBuffer, inputOffset, inputCount, ref buffer2, 0, this.PaddingValue, true);
                this.Reset();
                return buffer2;
            }
            byte[] dst = new byte[this._depadBuffer.Length + inputCount];
            Buffer.InternalBlockCopy(this._depadBuffer, 0, dst, 0, this._depadBuffer.Length);
            Buffer.InternalBlockCopy(inputBuffer, inputOffset, dst, this._depadBuffer.Length, inputCount);
            byte[] outputBuffer = null;
            Utils._DecryptData(this._safeKeyHandle, dst, 0, dst.Length, ref outputBuffer, 0, this.PaddingValue, true);
            this.Reset();
            return outputBuffer;
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
                return (this.BlockSizeValue / 8);
            }
        }

        public IntPtr KeyHandle
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this._safeKeyHandle.DangerousGetHandle();
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return (this.BlockSizeValue / 8);
            }
        }
    }
}

