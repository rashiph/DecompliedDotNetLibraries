namespace System.Net
{
    using System;
    using System.Globalization;

    internal class FrameHeader
    {
        private int _MajorV;
        private int _MessageId;
        private int _MinorV;
        private int _PayloadSize;
        public const int DefaultMajorV = 1;
        public const int DefaultMinorV = 0;
        public const int HandshakeDoneId = 20;
        public const int HandshakeErrId = 0x15;
        public const int HandshakeId = 0x16;
        public const int IgnoreValue = -1;

        public FrameHeader()
        {
            this._MessageId = 0x16;
            this._MajorV = 1;
            this._MinorV = 0;
            this._PayloadSize = -1;
        }

        public FrameHeader(int messageId, int majorV, int minorV)
        {
            this._MessageId = messageId;
            this._MajorV = majorV;
            this._MinorV = minorV;
            this._PayloadSize = -1;
        }

        public void CopyFrom(byte[] bytes, int start, FrameHeader verifier)
        {
            this._MessageId = bytes[start++];
            this._MajorV = bytes[start++];
            this._MinorV = bytes[start++];
            this._PayloadSize = (bytes[start++] << 8) | bytes[start];
            if ((verifier.MessageId != -1) && (this.MessageId != verifier.MessageId))
            {
                throw new InvalidOperationException(SR.GetString("net_io_header_id", new object[] { "MessageId", this.MessageId, verifier.MessageId }));
            }
            if ((verifier.MajorV != -1) && (this.MajorV != verifier.MajorV))
            {
                throw new InvalidOperationException(SR.GetString("net_io_header_id", new object[] { "MajorV", this.MajorV, verifier.MajorV }));
            }
            if ((verifier.MinorV != -1) && (this.MinorV != verifier.MinorV))
            {
                throw new InvalidOperationException(SR.GetString("net_io_header_id", new object[] { "MinorV", this.MinorV, verifier.MinorV }));
            }
        }

        public void CopyTo(byte[] dest, int start)
        {
            dest[start++] = (byte) this._MessageId;
            dest[start++] = (byte) this._MajorV;
            dest[start++] = (byte) this._MinorV;
            dest[start++] = (byte) ((this._PayloadSize >> 8) & 0xff);
            dest[start] = (byte) (this._PayloadSize & 0xff);
        }

        public int MajorV
        {
            get
            {
                return this._MajorV;
            }
        }

        public int MaxMessageSize
        {
            get
            {
                return 0xffff;
            }
        }

        public int MessageId
        {
            get
            {
                return this._MessageId;
            }
            set
            {
                this._MessageId = value;
            }
        }

        public int MinorV
        {
            get
            {
                return this._MinorV;
            }
        }

        public int PayloadSize
        {
            get
            {
                return this._PayloadSize;
            }
            set
            {
                if (value > this.MaxMessageSize)
                {
                    throw new ArgumentException(SR.GetString("net_frame_max_size", new object[] { this.MaxMessageSize.ToString(NumberFormatInfo.InvariantInfo), value.ToString(NumberFormatInfo.InvariantInfo) }), "PayloadSize");
                }
                this._PayloadSize = value;
            }
        }

        public int Size
        {
            get
            {
                return 5;
            }
        }
    }
}

