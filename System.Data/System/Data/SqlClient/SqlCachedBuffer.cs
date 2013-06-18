namespace System.Data.SqlClient
{
    using System;
    using System.Collections;
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal sealed class SqlCachedBuffer : INullable
    {
        private ArrayList _cachedBytes;
        private const int _maxChunkSize = 0x800;
        public static readonly SqlCachedBuffer Null = new SqlCachedBuffer();

        private SqlCachedBuffer()
        {
        }

        internal SqlCachedBuffer(SqlDataReader dataRdr, int columnOrdinal, long startPosition)
        {
            int num;
            this._cachedBytes = new ArrayList();
            long dataIndex = startPosition;
            do
            {
                byte[] buffer = new byte[0x800];
                num = (int) dataRdr.GetBytesInternal(columnOrdinal, dataIndex, buffer, 0, 0x800);
                dataIndex += num;
                if (this._cachedBytes.Count == 0)
                {
                    this.AddByteOrderMark(buffer, num);
                }
                if (0 < num)
                {
                    if (num < buffer.Length)
                    {
                        byte[] dst = new byte[num];
                        Buffer.BlockCopy(buffer, 0, dst, 0, num);
                        buffer = dst;
                    }
                    this._cachedBytes.Add(buffer);
                }
            }
            while (0 < num);
        }

        internal SqlCachedBuffer(SqlMetaDataPriv metadata, TdsParser parser, TdsParserStateObject stateObj)
        {
            int len = 0;
            this._cachedBytes = new ArrayList();
            ulong num = parser.PlpBytesLeft(stateObj);
            do
            {
                if (num == 0L)
                {
                    return;
                }
                do
                {
                    len = (num > 0x800L) ? 0x800 : ((int) num);
                    byte[] buff = new byte[len];
                    len = stateObj.ReadPlpBytes(ref buff, 0, len);
                    if (this._cachedBytes.Count == 0)
                    {
                        this.AddByteOrderMark(buff);
                    }
                    this._cachedBytes.Add(buff);
                    num -= len;
                }
                while (num > 0L);
                num = parser.PlpBytesLeft(stateObj);
            }
            while (num > 0L);
        }

        private void AddByteOrderMark(byte[] byteArr)
        {
            this.AddByteOrderMark(byteArr, byteArr.Length);
        }

        private void AddByteOrderMark(byte[] byteArr, int length)
        {
            int num = 0xfeff;
            if (((length >= 2) && (byteArr[0] == 0xdf)) && (byteArr[1] == 0xff))
            {
                num = 0;
            }
            if (num != 0)
            {
                byte[] buffer = new byte[2];
                buffer[0] = (byte) num;
                num = num >> 8;
                buffer[1] = (byte) num;
                this._cachedBytes.Add(buffer);
            }
        }

        internal SqlString ToSqlString()
        {
            if (this.IsNull)
            {
                return SqlString.Null;
            }
            return new SqlString(this.ToString());
        }

        internal SqlXml ToSqlXml()
        {
            return new SqlXml(new SqlCachedStream(this));
        }

        public override string ToString()
        {
            if (this.IsNull)
            {
                throw new SqlNullValueException();
            }
            if (this._cachedBytes.Count == 0)
            {
                return string.Empty;
            }
            SqlCachedStream stream = new SqlCachedStream(this);
            SqlXml xml = new SqlXml(stream);
            return xml.Value;
        }

        internal XmlReader ToXmlReader()
        {
            XmlReader reader;
            SqlCachedStream stream = new SqlCachedStream(this);
            XmlReaderSettings settings = new XmlReaderSettings {
                ConformanceLevel = ConformanceLevel.Fragment
            };
            MethodInfo method = typeof(XmlReader).GetMethod("CreateSqlReader", BindingFlags.NonPublic | BindingFlags.Static);
            object[] objArray = new object[3];
            objArray[0] = stream;
            objArray[1] = settings;
            object[] parameters = objArray;
            new ReflectionPermission(ReflectionPermissionFlag.MemberAccess).Assert();
            try
            {
                reader = (XmlReader) method.Invoke(null, parameters);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return reader;
        }

        internal ArrayList CachedBytes
        {
            get
            {
                return this._cachedBytes;
            }
        }

        public bool IsNull
        {
            get
            {
                if (this._cachedBytes != null)
                {
                    return false;
                }
                return true;
            }
        }
    }
}

