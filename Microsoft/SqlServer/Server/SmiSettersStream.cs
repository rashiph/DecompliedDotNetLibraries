namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.IO;

    internal class SmiSettersStream : Stream
    {
        private long _lengthWritten;
        private SmiMetaData _metaData;
        private int _ordinal;
        private ITypedSettersV3 _setters;
        private SmiEventSink_Default _sink;

        internal SmiSettersStream(SmiEventSink_Default sink, ITypedSettersV3 setters, int ordinal, SmiMetaData metaData)
        {
            this._sink = sink;
            this._setters = setters;
            this._ordinal = ordinal;
            this._lengthWritten = 0L;
            this._metaData = metaData;
        }

        public override void Flush()
        {
            this._lengthWritten = ValueUtilsSmi.SetBytesLength(this._sink, this._setters, this._ordinal, this._metaData, this._lengthWritten);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw SQL.StreamReadNotSupported();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw SQL.StreamSeekNotSupported();
        }

        public override void SetLength(long value)
        {
            if (value < 0L)
            {
                throw ADP.ArgumentOutOfRange("value");
            }
            ValueUtilsSmi.SetBytesLength(this._sink, this._setters, this._ordinal, this._metaData, value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this._lengthWritten += ValueUtilsSmi.SetBytes(this._sink, this._setters, this._ordinal, this._metaData, this._lengthWritten, buffer, offset, count);
        }

        public override bool CanRead
        {
            get
            {
                return false;
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
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return this._lengthWritten;
            }
        }

        public override long Position
        {
            get
            {
                return this._lengthWritten;
            }
            set
            {
                throw SQL.StreamSeekNotSupported();
            }
        }
    }
}

