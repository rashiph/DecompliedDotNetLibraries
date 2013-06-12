namespace System.Data.SqlTypes
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [Serializable, XmlSchemaProvider("GetXsdType")]
    public sealed class SqlBytes : INullable, IXmlSerializable, ISerializable
    {
        private long m_lCurLen;
        private IntPtr m_pbData;
        internal byte[] m_rgbBuf;
        private byte[] m_rgbWorkBuf;
        private SqlBytesCharsState m_state;
        internal System.IO.Stream m_stream;
        private const long x_lMaxLen = 0x7fffffffL;
        private const long x_lNull = -1L;

        public SqlBytes()
        {
            this.SetNull();
        }

        public SqlBytes(byte[] buffer)
        {
            this.m_rgbBuf = buffer;
            this.m_stream = null;
            if (this.m_rgbBuf == null)
            {
                this.m_state = SqlBytesCharsState.Null;
                this.m_lCurLen = -1L;
            }
            else
            {
                this.m_state = SqlBytesCharsState.Buffer;
                this.m_lCurLen = this.m_rgbBuf.Length;
            }
            this.m_rgbWorkBuf = null;
        }

        public SqlBytes(SqlBinary value) : this(value.IsNull ? null : value.Value)
        {
        }

        public SqlBytes(System.IO.Stream s)
        {
            this.m_rgbBuf = null;
            this.m_lCurLen = -1L;
            this.m_stream = s;
            this.m_state = (s == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Stream;
            this.m_rgbWorkBuf = null;
        }

        private SqlBytes(SerializationInfo info, StreamingContext context)
        {
            this.m_stream = null;
            this.m_rgbWorkBuf = null;
            if (info.GetBoolean("IsNull"))
            {
                this.m_state = SqlBytesCharsState.Null;
                this.m_rgbBuf = null;
            }
            else
            {
                this.m_state = SqlBytesCharsState.Buffer;
                this.m_rgbBuf = (byte[]) info.GetValue("data", typeof(byte[]));
                this.m_lCurLen = this.m_rgbBuf.Length;
            }
        }

        [Conditional("DEBUG")]
        private void AssertValid()
        {
            bool isNull = this.IsNull;
        }

        private void CopyStreamToBuffer()
        {
            long length = this.m_stream.Length;
            if (length >= 0x7fffffffL)
            {
                throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_WriteOffsetLargerThanLenMessage"));
            }
            if ((this.m_rgbBuf == null) || (this.m_rgbBuf.Length < length))
            {
                this.m_rgbBuf = new byte[length];
            }
            if (this.m_stream.Position != 0L)
            {
                this.m_stream.Seek(0L, SeekOrigin.Begin);
            }
            this.m_stream.Read(this.m_rgbBuf, 0, (int) length);
            this.m_stream = null;
            this.m_lCurLen = length;
            this.m_state = SqlBytesCharsState.Buffer;
        }

        internal bool FStream()
        {
            return (this.m_state == SqlBytesCharsState.Stream);
        }

        public static XmlQualifiedName GetXsdType(XmlSchemaSet schemaSet)
        {
            return new XmlQualifiedName("base64Binary", "http://www.w3.org/2001/XMLSchema");
        }

        public static explicit operator SqlBytes(SqlBinary value)
        {
            return new SqlBytes(value);
        }

        public static explicit operator SqlBinary(SqlBytes value)
        {
            return value.ToSqlBinary();
        }

        public long Read(long offset, byte[] buffer, int offsetInBuffer, int count)
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if ((offset > this.Length) || (offset < 0L))
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if ((offsetInBuffer > buffer.Length) || (offsetInBuffer < 0))
            {
                throw new ArgumentOutOfRangeException("offsetInBuffer");
            }
            if ((count < 0) || (count > (buffer.Length - offsetInBuffer)))
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (count > (this.Length - offset))
            {
                count = (int) (this.Length - offset);
            }
            if (count != 0)
            {
                if (this.m_state == SqlBytesCharsState.Stream)
                {
                    if (this.m_stream.Position != offset)
                    {
                        this.m_stream.Seek(offset, SeekOrigin.Begin);
                    }
                    this.m_stream.Read(buffer, offsetInBuffer, count);
                }
                else
                {
                    Array.Copy(this.m_rgbBuf, offset, buffer, (long) offsetInBuffer, (long) count);
                }
            }
            return (long) count;
        }

        private void SetBuffer(byte[] buffer)
        {
            this.m_rgbBuf = buffer;
            this.m_lCurLen = (this.m_rgbBuf == null) ? -1L : ((long) this.m_rgbBuf.Length);
            this.m_stream = null;
            this.m_state = (this.m_rgbBuf == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Buffer;
        }

        public void SetLength(long value)
        {
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            if (this.FStream())
            {
                this.m_stream.SetLength(value);
            }
            else
            {
                if (this.m_rgbBuf == null)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_NoBufferMessage"));
                }
                if (value > this.m_rgbBuf.Length)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.IsNull)
                {
                    this.m_state = SqlBytesCharsState.Buffer;
                }
                this.m_lCurLen = value;
            }
        }

        public void SetNull()
        {
            this.m_lCurLen = -1L;
            this.m_stream = null;
            this.m_state = SqlBytesCharsState.Null;
        }

        [SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter=true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            switch (this.m_state)
            {
                case SqlBytesCharsState.Buffer:
                    break;

                case SqlBytesCharsState.Stream:
                    this.CopyStreamToBuffer();
                    break;

                default:
                    info.AddValue("IsNull", true);
                    return;
            }
            info.AddValue("IsNull", false);
            info.AddValue("data", this.m_rgbBuf);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader r)
        {
            byte[] buffer = null;
            string attribute = r.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                r.ReadElementString();
                this.SetNull();
            }
            else
            {
                string s = r.ReadElementString();
                if (s == null)
                {
                    buffer = new byte[0];
                }
                else
                {
                    s = s.Trim();
                    if (s.Length == 0)
                    {
                        buffer = new byte[0];
                    }
                    else
                    {
                        buffer = Convert.FromBase64String(s);
                    }
                }
            }
            this.SetBuffer(buffer);
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                byte[] inArray = this.Buffer;
                writer.WriteString(Convert.ToBase64String(inArray, 0, (int) this.Length));
            }
        }

        public SqlBinary ToSqlBinary()
        {
            if (!this.IsNull)
            {
                return new SqlBinary(this.Value);
            }
            return SqlBinary.Null;
        }

        public void Write(long offset, byte[] buffer, int offsetInBuffer, int count)
        {
            if (this.FStream())
            {
                if (this.m_stream.Position != offset)
                {
                    this.m_stream.Seek(offset, SeekOrigin.Begin);
                }
                this.m_stream.Write(buffer, offsetInBuffer, count);
            }
            else
            {
                if (buffer == null)
                {
                    throw new ArgumentNullException("buffer");
                }
                if (this.m_rgbBuf == null)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_NoBufferMessage"));
                }
                if (offset < 0L)
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (offset > this.m_rgbBuf.Length)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_BufferInsufficientMessage"));
                }
                if ((offsetInBuffer < 0) || (offsetInBuffer > buffer.Length))
                {
                    throw new ArgumentOutOfRangeException("offsetInBuffer");
                }
                if ((count < 0) || (count > (buffer.Length - offsetInBuffer)))
                {
                    throw new ArgumentOutOfRangeException("count");
                }
                if (count > (this.m_rgbBuf.Length - offset))
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_BufferInsufficientMessage"));
                }
                if (this.IsNull)
                {
                    if (offset != 0L)
                    {
                        throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_WriteNonZeroOffsetOnNullMessage"));
                    }
                    this.m_lCurLen = 0L;
                    this.m_state = SqlBytesCharsState.Buffer;
                }
                else if (offset > this.m_lCurLen)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_WriteOffsetLargerThanLenMessage"));
                }
                if (count != 0)
                {
                    Array.Copy(buffer, (long) offsetInBuffer, this.m_rgbBuf, offset, (long) count);
                    if (this.m_lCurLen < (offset + count))
                    {
                        this.m_lCurLen = offset + count;
                    }
                }
            }
        }

        public byte[] Buffer
        {
            get
            {
                if (this.FStream())
                {
                    this.CopyStreamToBuffer();
                }
                return this.m_rgbBuf;
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.m_state == SqlBytesCharsState.Null);
            }
        }

        public byte this[long offset]
        {
            get
            {
                if ((offset < 0L) || (offset >= this.Length))
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (this.m_rgbWorkBuf == null)
                {
                    this.m_rgbWorkBuf = new byte[1];
                }
                this.Read(offset, this.m_rgbWorkBuf, 0, 1);
                return this.m_rgbWorkBuf[0];
            }
            set
            {
                if (this.m_rgbWorkBuf == null)
                {
                    this.m_rgbWorkBuf = new byte[1];
                }
                this.m_rgbWorkBuf[0] = value;
                this.Write(offset, this.m_rgbWorkBuf, 0, 1);
            }
        }

        public long Length
        {
            get
            {
                SqlBytesCharsState state = this.m_state;
                if (state == SqlBytesCharsState.Null)
                {
                    throw new SqlNullValueException();
                }
                if (state != SqlBytesCharsState.Stream)
                {
                    return this.m_lCurLen;
                }
                return this.m_stream.Length;
            }
        }

        public long MaxLength
        {
            get
            {
                if ((this.m_state != SqlBytesCharsState.Stream) && (this.m_rgbBuf != null))
                {
                    return (long) this.m_rgbBuf.Length;
                }
                return -1L;
            }
        }

        public static SqlBytes Null
        {
            get
            {
                return new SqlBytes(null);
            }
        }

        public StorageState Storage
        {
            get
            {
                switch (this.m_state)
                {
                    case SqlBytesCharsState.Null:
                        throw new SqlNullValueException();

                    case SqlBytesCharsState.Buffer:
                        return StorageState.Buffer;

                    case SqlBytesCharsState.Stream:
                        return StorageState.Stream;
                }
                return StorageState.UnmanagedBuffer;
            }
        }

        public System.IO.Stream Stream
        {
            get
            {
                if (!this.FStream())
                {
                    return new StreamOnSqlBytes(this);
                }
                return this.m_stream;
            }
            set
            {
                this.m_lCurLen = -1L;
                this.m_stream = value;
                this.m_state = (value == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Stream;
            }
        }

        public byte[] Value
        {
            get
            {
                byte[] buffer;
                switch (this.m_state)
                {
                    case SqlBytesCharsState.Null:
                        throw new SqlNullValueException();

                    case SqlBytesCharsState.Stream:
                        if (this.m_stream.Length > 0x7fffffffL)
                        {
                            throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_BufferInsufficientMessage"));
                        }
                        buffer = new byte[this.m_stream.Length];
                        if (this.m_stream.Position != 0L)
                        {
                            this.m_stream.Seek(0L, SeekOrigin.Begin);
                        }
                        this.m_stream.Read(buffer, 0, (int) this.m_stream.Length);
                        return buffer;
                }
                buffer = new byte[this.m_lCurLen];
                Array.Copy(this.m_rgbBuf, buffer, (int) this.m_lCurLen);
                return buffer;
            }
        }
    }
}

