namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Xml;

    internal sealed class SqlStream : Stream
    {
        private bool _advanceReader;
        private int _bom;
        private byte[] _bufferedData;
        private long _bytesCol;
        private int _columnOrdinal;
        private bool _endOfColumn;
        private bool _processAllRows;
        private SqlDataReader _reader;
        private bool _readFirstRow;

        internal SqlStream(SqlDataReader reader, bool addByteOrderMark, bool processAllRows) : this(0, reader, addByteOrderMark, processAllRows, true)
        {
        }

        internal SqlStream(int columnOrdinal, SqlDataReader reader, bool addByteOrderMark, bool processAllRows, bool advanceReader)
        {
            this._columnOrdinal = columnOrdinal;
            this._reader = reader;
            this._bom = addByteOrderMark ? 0xfeff : 0;
            this._processAllRows = processAllRows;
            this._advanceReader = advanceReader;
        }

        private static bool AdvanceToNextRow(SqlDataReader reader)
        {
            do
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            while (reader.NextResult());
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((disposing && this._advanceReader) && ((this._reader != null) && !this._reader.IsClosed))
                {
                    this._reader.Close();
                }
                this._reader = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush()
        {
            throw ADP.NotSupported();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = 0;
            int num2 = 0;
            if (this._reader == null)
            {
                throw ADP.StreamClosed("Read");
            }
            if (buffer == null)
            {
                throw ADP.ArgumentNull("buffer");
            }
            if ((offset < 0) || (count < 0))
            {
                throw ADP.ArgumentOutOfRange(string.Empty, (offset < 0) ? "offset" : "count");
            }
            if ((buffer.Length - offset) < count)
            {
                throw ADP.ArgumentOutOfRange("count");
            }
            if (this._bom > 0)
            {
                this._bufferedData = new byte[2];
                num2 = this.ReadBytes(this._bufferedData, 0, 2);
                if ((num2 < 2) || ((this._bufferedData[0] == 0xdf) && (this._bufferedData[1] == 0xff)))
                {
                    this._bom = 0;
                }
                while (count > 0)
                {
                    if (this._bom <= 0)
                    {
                        break;
                    }
                    buffer[offset] = (byte) this._bom;
                    this._bom = this._bom >> 8;
                    offset++;
                    count--;
                    num++;
                }
            }
            if (num2 > 0)
            {
                while (count > 0)
                {
                    buffer[offset++] = this._bufferedData[0];
                    num++;
                    count--;
                    if ((num2 > 1) && (count > 0))
                    {
                        buffer[offset++] = this._bufferedData[1];
                        num++;
                        count--;
                        break;
                    }
                }
                this._bufferedData = null;
            }
            return (num + this.ReadBytes(buffer, offset, count));
        }

        private int ReadBytes(byte[] buffer, int offset, int count)
        {
            bool flag = true;
            int num2 = 0;
            int num = 0;
            if (this._reader.IsClosed || this._endOfColumn)
            {
                return 0;
            }
            try
            {
                while (count > 0)
                {
                    if (this._advanceReader && (0L == this._bytesCol))
                    {
                        flag = false;
                        if ((!this._readFirstRow || this._processAllRows) && AdvanceToNextRow(this._reader))
                        {
                            this._readFirstRow = true;
                            if (this._reader.IsDBNull(this._columnOrdinal))
                            {
                                continue;
                            }
                            flag = true;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                    num = (int) this._reader.GetBytesInternal(this._columnOrdinal, this._bytesCol, buffer, offset, count);
                    if (num < count)
                    {
                        this._bytesCol = 0L;
                        flag = false;
                        if (!this._advanceReader)
                        {
                            this._endOfColumn = true;
                        }
                    }
                    else
                    {
                        this._bytesCol += num;
                    }
                    count -= num;
                    offset += num;
                    num2 += num;
                }
                if (!flag && this._advanceReader)
                {
                    this._reader.Close();
                }
            }
            catch (Exception exception)
            {
                if (this._advanceReader && ADP.IsCatchableExceptionType(exception))
                {
                    this._reader.Close();
                }
                throw;
            }
            return num2;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw ADP.NotSupported();
        }

        public override void SetLength(long value)
        {
            throw ADP.NotSupported();
        }

        internal XmlReader ToXmlReader()
        {
            XmlReader reader;
            XmlReaderSettings settings = new XmlReaderSettings {
                ConformanceLevel = ConformanceLevel.Fragment,
                CloseInput = true
            };
            MethodInfo method = typeof(XmlReader).GetMethod("CreateSqlReader", BindingFlags.NonPublic | BindingFlags.Static);
            object[] objArray = new object[3];
            objArray[0] = this;
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw ADP.NotSupported();
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override long Length
        {
            get
            {
                throw ADP.NotSupported();
            }
        }

        public override long Position
        {
            get
            {
                throw ADP.NotSupported();
            }
            set
            {
                throw ADP.NotSupported();
            }
        }
    }
}

