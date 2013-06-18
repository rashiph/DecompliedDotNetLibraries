namespace System.ServiceModel.Channels
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.Text;

    internal abstract class StringDecoder
    {
        private int bytesNeeded;
        private State currentState;
        private byte[] encodedBytes;
        private int encodedSize;
        private IntDecoder sizeDecoder;
        private int sizeQuota;
        private string value;
        private int valueLengthInBytes;

        public StringDecoder(int sizeQuota)
        {
            this.sizeQuota = sizeQuota;
            this.sizeDecoder = new IntDecoder();
            this.currentState = State.ReadingSize;
            this.Reset();
        }

        private static bool CompareBuffers(byte[] buffer1, byte[] buffer2, int offset)
        {
            for (int i = 0; i < buffer1.Length; i++)
            {
                if (buffer1[i] != buffer2[i + offset])
                {
                    return false;
                }
            }
            return true;
        }

        public int Decode(byte[] buffer, int offset, int size)
        {
            int bytesNeeded;
            DecoderHelper.ValidateSize(size);
            switch (this.currentState)
            {
                case State.ReadingSize:
                    bytesNeeded = this.sizeDecoder.Decode(buffer, offset, size);
                    if (this.sizeDecoder.IsValueDecoded)
                    {
                        this.encodedSize = this.sizeDecoder.Value;
                        if (this.encodedSize > this.sizeQuota)
                        {
                            Exception exception = this.OnSizeQuotaExceeded(this.encodedSize);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                        }
                        if ((this.encodedBytes == null) || (this.encodedBytes.Length < this.encodedSize))
                        {
                            this.encodedBytes = DiagnosticUtility.Utility.AllocateByteArray(this.encodedSize);
                            this.value = null;
                        }
                        this.currentState = State.ReadingBytes;
                        this.bytesNeeded = this.encodedSize;
                        return bytesNeeded;
                    }
                    return bytesNeeded;

                case State.ReadingBytes:
                    if ((((this.value == null) || (this.valueLengthInBytes != this.encodedSize)) || ((this.bytesNeeded != this.encodedSize) || (size < this.encodedSize))) || !CompareBuffers(this.encodedBytes, buffer, offset))
                    {
                        bytesNeeded = this.bytesNeeded;
                        if (size < this.bytesNeeded)
                        {
                            bytesNeeded = size;
                        }
                        Buffer.BlockCopy(buffer, offset, this.encodedBytes, this.encodedSize - this.bytesNeeded, bytesNeeded);
                        this.bytesNeeded -= bytesNeeded;
                        if (this.bytesNeeded == 0)
                        {
                            this.value = Encoding.UTF8.GetString(this.encodedBytes, 0, this.encodedSize);
                            this.valueLengthInBytes = this.encodedSize;
                            this.OnComplete(this.value);
                        }
                        return bytesNeeded;
                    }
                    bytesNeeded = this.bytesNeeded;
                    this.OnComplete(this.value);
                    return bytesNeeded;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataException(System.ServiceModel.SR.GetString("InvalidDecoderStateMachine")));
        }

        protected virtual void OnComplete(string value)
        {
            this.currentState = State.Done;
        }

        protected abstract Exception OnSizeQuotaExceeded(int size);
        public void Reset()
        {
            this.currentState = State.ReadingSize;
            this.sizeDecoder.Reset();
        }

        public bool IsValueDecoded
        {
            get
            {
                return (this.currentState == State.Done);
            }
        }

        public string Value
        {
            get
            {
                if (this.currentState != State.Done)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("FramingValueNotAvailable")));
                }
                return this.value;
            }
        }

        private enum State
        {
            ReadingSize,
            ReadingBytes,
            Done
        }
    }
}

