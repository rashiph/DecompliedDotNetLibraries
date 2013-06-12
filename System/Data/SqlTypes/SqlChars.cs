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
    public sealed class SqlChars : INullable, IXmlSerializable, ISerializable
    {
        private long m_lCurLen;
        private IntPtr m_pchData;
        internal char[] m_rgchBuf;
        private char[] m_rgchWorkBuf;
        private SqlBytesCharsState m_state;
        internal SqlStreamChars m_stream;
        private const long x_lMaxLen = 0x7fffffffL;
        private const long x_lNull = -1L;

        public SqlChars()
        {
            this.SetNull();
        }

        public SqlChars(char[] buffer)
        {
            this.m_rgchBuf = buffer;
            this.m_stream = null;
            if (this.m_rgchBuf == null)
            {
                this.m_state = SqlBytesCharsState.Null;
                this.m_lCurLen = -1L;
            }
            else
            {
                this.m_state = SqlBytesCharsState.Buffer;
                this.m_lCurLen = this.m_rgchBuf.Length;
            }
            this.m_rgchWorkBuf = null;
        }

        internal SqlChars(SqlStreamChars s)
        {
            this.m_rgchBuf = null;
            this.m_lCurLen = -1L;
            this.m_stream = s;
            this.m_state = (s == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Stream;
            this.m_rgchWorkBuf = null;
        }

        public SqlChars(SqlString value) : this(value.IsNull ? null : value.Value.ToCharArray())
        {
        }

        private SqlChars(SerializationInfo info, StreamingContext context)
        {
            this.m_stream = null;
            this.m_rgchWorkBuf = null;
            if (info.GetBoolean("IsNull"))
            {
                this.m_state = SqlBytesCharsState.Null;
                this.m_rgchBuf = null;
            }
            else
            {
                this.m_state = SqlBytesCharsState.Buffer;
                this.m_rgchBuf = (char[]) info.GetValue("data", typeof(char[]));
                this.m_lCurLen = this.m_rgchBuf.Length;
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
                throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_BufferInsufficientMessage"));
            }
            if ((this.m_rgchBuf == null) || (this.m_rgchBuf.Length < length))
            {
                this.m_rgchBuf = new char[length];
            }
            if (this.m_stream.Position != 0L)
            {
                this.m_stream.Seek(0L, SeekOrigin.Begin);
            }
            this.m_stream.Read(this.m_rgchBuf, 0, (int) length);
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
            return new XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema");
        }

        public static explicit operator SqlString(SqlChars value)
        {
            return value.ToSqlString();
        }

        public static explicit operator SqlChars(SqlString value)
        {
            return new SqlChars(value);
        }

        public long Read(long offset, char[] buffer, int offsetInBuffer, int count)
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
                    Array.Copy(this.m_rgchBuf, offset, buffer, (long) offsetInBuffer, (long) count);
                }
            }
            return (long) count;
        }

        private void SetBuffer(char[] buffer)
        {
            this.m_rgchBuf = buffer;
            this.m_lCurLen = (this.m_rgchBuf == null) ? -1L : ((long) this.m_rgchBuf.Length);
            this.m_stream = null;
            this.m_state = (this.m_rgchBuf == null) ? SqlBytesCharsState.Null : SqlBytesCharsState.Buffer;
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
                if (this.m_rgchBuf == null)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_NoBufferMessage"));
                }
                if (value > this.m_rgchBuf.Length)
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
            info.AddValue("data", this.m_rgchBuf);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader r)
        {
            char[] buffer = null;
            string attribute = r.GetAttribute("nil", "http://www.w3.org/2001/XMLSchema-instance");
            if ((attribute != null) && XmlConvert.ToBoolean(attribute))
            {
                r.ReadElementString();
                this.SetNull();
            }
            else
            {
                buffer = r.ReadElementString().ToCharArray();
                this.SetBuffer(buffer);
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.IsNull)
            {
                writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else
            {
                char[] buffer = this.Buffer;
                writer.WriteString(new string(buffer, 0, (int) this.Length));
            }
        }

        public SqlString ToSqlString()
        {
            if (!this.IsNull)
            {
                return new string(this.Value);
            }
            return SqlString.Null;
        }

        public void Write(long offset, char[] buffer, int offsetInBuffer, int count)
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
                if (this.m_rgchBuf == null)
                {
                    throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_NoBufferMessage"));
                }
                if (offset < 0L)
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (offset > this.m_rgchBuf.Length)
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
                if (count > (this.m_rgchBuf.Length - offset))
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
                    Array.Copy(buffer, (long) offsetInBuffer, this.m_rgchBuf, offset, (long) count);
                    if (this.m_lCurLen < (offset + count))
                    {
                        this.m_lCurLen = offset + count;
                    }
                }
            }
        }

        public char[] Buffer
        {
            get
            {
                if (this.FStream())
                {
                    this.CopyStreamToBuffer();
                }
                return this.m_rgchBuf;
            }
        }

        public bool IsNull
        {
            get
            {
                return (this.m_state == SqlBytesCharsState.Null);
            }
        }

        public char this[long offset]
        {
            get
            {
                if ((offset < 0L) || (offset >= this.Length))
                {
                    throw new ArgumentOutOfRangeException("offset");
                }
                if (this.m_rgchWorkBuf == null)
                {
                    this.m_rgchWorkBuf = new char[1];
                }
                this.Read(offset, this.m_rgchWorkBuf, 0, 1);
                return this.m_rgchWorkBuf[0];
            }
            set
            {
                if (this.m_rgchWorkBuf == null)
                {
                    this.m_rgchWorkBuf = new char[1];
                }
                this.m_rgchWorkBuf[0] = value;
                this.Write(offset, this.m_rgchWorkBuf, 0, 1);
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
                if ((this.m_state != SqlBytesCharsState.Stream) && (this.m_rgchBuf != null))
                {
                    return (long) this.m_rgchBuf.Length;
                }
                return -1L;
            }
        }

        public static SqlChars Null
        {
            get
            {
                return new SqlChars(null);
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

        internal SqlStreamChars Stream
        {
            get
            {
                if (!this.FStream())
                {
                    return new StreamOnSqlChars(this);
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

        public char[] Value
        {
            get
            {
                char[] chArray;
                switch (this.m_state)
                {
                    case SqlBytesCharsState.Null:
                        throw new SqlNullValueException();

                    case SqlBytesCharsState.Stream:
                        if (this.m_stream.Length > 0x7fffffffL)
                        {
                            throw new SqlTypeException(System.Data.Res.GetString("SqlMisc_BufferInsufficientMessage"));
                        }
                        chArray = new char[this.m_stream.Length];
                        if (this.m_stream.Position != 0L)
                        {
                            this.m_stream.Seek(0L, SeekOrigin.Begin);
                        }
                        this.m_stream.Read(chArray, 0, (int) this.m_stream.Length);
                        return chArray;
                }
                chArray = new char[this.m_lCurLen];
                Array.Copy(this.m_rgchBuf, chArray, (int) this.m_lCurLen);
                return chArray;
            }
        }
    }
}

