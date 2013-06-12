namespace System.Data.SqlClient
{
    using Microsoft.SqlServer.Server;
    using System;
    using System.Data.SqlTypes;
    using System.Diagnostics;

    internal class TdsRecordBufferSetter : SmiRecordBuffer
    {
        private TdsValueSetter[] _fieldSetters;
        private SmiMetaData _metaData;
        private TdsParserStateObject _stateObj;

        internal TdsRecordBufferSetter(TdsParserStateObject stateObj, SmiMetaData md)
        {
            this._fieldSetters = new TdsValueSetter[md.FieldMetaData.Count];
            for (int i = 0; i < md.FieldMetaData.Count; i++)
            {
                this._fieldSetters[i] = new TdsValueSetter(stateObj, md.FieldMetaData[i]);
            }
            this._stateObj = stateObj;
            this._metaData = md;
        }

        [Conditional("DEBUG")]
        internal void CheckSettingColumn(int ordinal)
        {
        }

        [Conditional("DEBUG")]
        private void CheckWritingToColumn(int ordinal)
        {
        }

        public override void Close(SmiEventSink eventSink)
        {
        }

        internal override void EndElements(SmiEventSink sink)
        {
            this._stateObj.Parser.WriteByte(0, this._stateObj);
        }

        internal override void NewElement(SmiEventSink sink)
        {
            this._stateObj.Parser.WriteByte(1, this._stateObj);
        }

        public override void SetBoolean(SmiEventSink sink, int ordinal, bool value)
        {
            this._fieldSetters[ordinal].SetBoolean(value);
        }

        public override void SetByte(SmiEventSink sink, int ordinal, byte value)
        {
            this._fieldSetters[ordinal].SetByte(value);
        }

        public override int SetBytes(SmiEventSink sink, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            return this._fieldSetters[ordinal].SetBytes(fieldOffset, buffer, bufferOffset, length);
        }

        public override void SetBytesLength(SmiEventSink sink, int ordinal, long length)
        {
            this._fieldSetters[ordinal].SetBytesLength(length);
        }

        public override int SetChars(SmiEventSink sink, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            return this._fieldSetters[ordinal].SetChars(fieldOffset, buffer, bufferOffset, length);
        }

        public override void SetCharsLength(SmiEventSink sink, int ordinal, long length)
        {
            this._fieldSetters[ordinal].SetCharsLength(length);
        }

        public override void SetDateTime(SmiEventSink sink, int ordinal, DateTime value)
        {
            this._fieldSetters[ordinal].SetDateTime(value);
        }

        public override void SetDateTimeOffset(SmiEventSink sink, int ordinal, DateTimeOffset value)
        {
            this._fieldSetters[ordinal].SetDateTimeOffset(value);
        }

        public override void SetDBNull(SmiEventSink sink, int ordinal)
        {
            this._fieldSetters[ordinal].SetDBNull();
        }

        public override void SetDouble(SmiEventSink sink, int ordinal, double value)
        {
            this._fieldSetters[ordinal].SetDouble(value);
        }

        public override void SetGuid(SmiEventSink sink, int ordinal, Guid value)
        {
            this._fieldSetters[ordinal].SetGuid(value);
        }

        public override void SetInt16(SmiEventSink sink, int ordinal, short value)
        {
            this._fieldSetters[ordinal].SetInt16(value);
        }

        public override void SetInt32(SmiEventSink sink, int ordinal, int value)
        {
            this._fieldSetters[ordinal].SetInt32(value);
        }

        public override void SetInt64(SmiEventSink sink, int ordinal, long value)
        {
            this._fieldSetters[ordinal].SetInt64(value);
        }

        public override void SetSingle(SmiEventSink sink, int ordinal, float value)
        {
            this._fieldSetters[ordinal].SetSingle(value);
        }

        public override void SetSqlDecimal(SmiEventSink sink, int ordinal, SqlDecimal value)
        {
            this._fieldSetters[ordinal].SetSqlDecimal(value);
        }

        public override void SetString(SmiEventSink sink, int ordinal, string value, int offset, int length)
        {
            this._fieldSetters[ordinal].SetString(value, offset, length);
        }

        public override void SetTimeSpan(SmiEventSink sink, int ordinal, TimeSpan value)
        {
            this._fieldSetters[ordinal].SetTimeSpan(value);
        }

        public override void SetVariantMetaData(SmiEventSink sink, int ordinal, SmiMetaData metaData)
        {
            this._fieldSetters[ordinal].SetVariantType(metaData);
        }

        [Conditional("DEBUG")]
        private void SkipPossibleDefaultedColumns(int targetColumn)
        {
        }

        internal override bool CanGet
        {
            get
            {
                return false;
            }
        }

        internal override bool CanSet
        {
            get
            {
                return true;
            }
        }
    }
}

