namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.SqlClient;
    using System.IO;

    internal class SmiGettersStream : Stream
    {
        private ITypedGettersV3 _getters;
        private SmiMetaData _metaData;
        private int _ordinal;
        private long _readPosition;
        private SmiEventSink_Default _sink;

        internal SmiGettersStream(SmiEventSink_Default sink, ITypedGettersV3 getters, int ordinal, SmiMetaData metaData)
        {
            this._sink = sink;
            this._getters = getters;
            this._ordinal = ordinal;
            this._readPosition = 0L;
            this._metaData = metaData;
        }

        public override void Flush()
        {
            throw SQL.StreamWriteNotSupported();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long num = ValueUtilsSmi.GetBytesInternal(this._sink, this._getters, this._ordinal, this._metaData, this._readPosition, buffer, offset, count, false);
            this._readPosition += num;
            return (int) num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw SQL.StreamSeekNotSupported();
        }

        public override void SetLength(long value)
        {
            throw SQL.StreamWriteNotSupported();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw SQL.StreamWriteNotSupported();
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
                return ValueUtilsSmi.GetBytesInternal(this._sink, this._getters, this._ordinal, this._metaData, 0L, null, 0, 0, false);
            }
        }

        public override long Position
        {
            get
            {
                return this._readPosition;
            }
            set
            {
                throw SQL.StreamSeekNotSupported();
            }
        }
    }
}

