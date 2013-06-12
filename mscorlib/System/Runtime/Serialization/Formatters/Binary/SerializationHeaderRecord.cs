namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Security;

    internal sealed class SerializationHeaderRecord : IStreamable
    {
        internal int binaryFormatterMajorVersion;
        internal int binaryFormatterMinorVersion;
        internal BinaryHeaderEnum binaryHeaderEnum;
        internal int headerId;
        internal int majorVersion;
        internal int minorVersion;
        internal int topId;

        internal SerializationHeaderRecord()
        {
            this.binaryFormatterMajorVersion = 1;
        }

        internal SerializationHeaderRecord(BinaryHeaderEnum binaryHeaderEnum, int topId, int headerId, int majorVersion, int minorVersion)
        {
            this.binaryFormatterMajorVersion = 1;
            this.binaryHeaderEnum = binaryHeaderEnum;
            this.topId = topId;
            this.headerId = headerId;
            this.majorVersion = majorVersion;
            this.minorVersion = minorVersion;
        }

        public void Dump()
        {
        }

        [Conditional("_LOGGING")]
        private void DumpInternal()
        {
            BCLDebug.CheckEnabled("BINARY");
        }

        private static int GetInt32(byte[] buffer, int index)
        {
            return (((buffer[index] | (buffer[index + 1] << 8)) | (buffer[index + 2] << 0x10)) | (buffer[index + 3] << 0x18));
        }

        [SecurityCritical]
        public void Read(__BinaryParser input)
        {
            byte[] buffer = input.ReadBytes(0x11);
            if (buffer.Length < 0x11)
            {
                __Error.EndOfFile();
            }
            this.majorVersion = GetInt32(buffer, 9);
            if (this.majorVersion > this.binaryFormatterMajorVersion)
            {
                throw new SerializationException(Environment.GetResourceString("Serialization_InvalidFormat", new object[] { BitConverter.ToString(buffer) }));
            }
            this.binaryHeaderEnum = (BinaryHeaderEnum) buffer[0];
            this.topId = GetInt32(buffer, 1);
            this.headerId = GetInt32(buffer, 5);
            this.minorVersion = GetInt32(buffer, 13);
        }

        public void Write(__BinaryWriter sout)
        {
            this.majorVersion = this.binaryFormatterMajorVersion;
            this.minorVersion = this.binaryFormatterMinorVersion;
            sout.WriteByte((byte) this.binaryHeaderEnum);
            sout.WriteInt32(this.topId);
            sout.WriteInt32(this.headerId);
            sout.WriteInt32(this.binaryFormatterMajorVersion);
            sout.WriteInt32(this.binaryFormatterMinorVersion);
        }
    }
}

