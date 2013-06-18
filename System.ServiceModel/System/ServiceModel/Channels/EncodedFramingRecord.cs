namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;
    using System.Text;

    internal abstract class EncodedFramingRecord
    {
        private byte[] encodedBytes;

        protected EncodedFramingRecord(byte[] encodedBytes)
        {
            this.encodedBytes = encodedBytes;
        }

        internal EncodedFramingRecord(FramingRecordType recordType, string value)
        {
            int byteCount = Encoding.UTF8.GetByteCount(value);
            int encodedSize = IntEncoder.GetEncodedSize(byteCount);
            this.encodedBytes = DiagnosticUtility.Utility.AllocateByteArray((1 + encodedSize) + byteCount);
            this.encodedBytes[0] = (byte) recordType;
            int offset = 1;
            offset += IntEncoder.Encode(byteCount, this.encodedBytes, offset);
            Encoding.UTF8.GetBytes(value, 0, value.Length, this.encodedBytes, offset);
            this.SetEncodedBytes(this.encodedBytes);
        }

        public override bool Equals(object o)
        {
            return ((o is EncodedFramingRecord) && this.Equals((EncodedFramingRecord) o));
        }

        public bool Equals(EncodedFramingRecord other)
        {
            if (other == null)
            {
                return false;
            }
            if (other != this)
            {
                byte[] encodedBytes = other.encodedBytes;
                if (this.encodedBytes.Length != encodedBytes.Length)
                {
                    return false;
                }
                for (int i = 0; i < this.encodedBytes.Length; i++)
                {
                    if (this.encodedBytes[i] != encodedBytes[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (((this.encodedBytes[0] << 0x10) | (this.encodedBytes[((int) this.encodedBytes.Length) / 2] << 8)) | this.encodedBytes[this.encodedBytes.Length - 1]);
        }

        protected void SetEncodedBytes(byte[] encodedBytes)
        {
            this.encodedBytes = encodedBytes;
        }

        public byte[] EncodedBytes
        {
            get
            {
                return this.encodedBytes;
            }
        }
    }
}

