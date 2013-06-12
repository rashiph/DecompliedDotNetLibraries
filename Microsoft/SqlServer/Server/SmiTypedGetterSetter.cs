namespace Microsoft.SqlServer.Server
{
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;

    internal abstract class SmiTypedGetterSetter : ITypedGettersV3, ITypedSettersV3
    {
        protected SmiTypedGetterSetter()
        {
        }

        internal virtual void EndElements(SmiEventSink sink)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual bool GetBoolean(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual byte GetByte(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int GetBytes(SmiEventSink sink, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetBytesLength(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int GetChars(SmiEventSink sink, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetCharsLength(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual DateTime GetDateTime(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual DateTimeOffset GetDateTimeOffset(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual double GetDouble(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual Guid GetGuid(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual short GetInt16(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int GetInt32(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual long GetInt64(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual float GetSingle(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SqlDecimal GetSqlDecimal(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual string GetString(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual TimeSpan GetTimeSpan(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual SmiTypedGetterSetter GetTypedGetterSetter(SmiEventSink sink, int ordinal)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual SmiMetaData GetVariantType(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual bool IsDBNull(SmiEventSink sink, int ordinal)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual void NewElement(SmiEventSink sink)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal virtual bool NextElement(SmiEventSink sink)
        {
            if (!this.CanGet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetBoolean(SmiEventSink sink, int ordinal, bool value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetByte(SmiEventSink sink, int ordinal, byte value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int SetBytes(SmiEventSink sink, int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetBytesLength(SmiEventSink sink, int ordinal, long length)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual int SetChars(SmiEventSink sink, int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetCharsLength(SmiEventSink sink, int ordinal, long length)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDateTime(SmiEventSink sink, int ordinal, DateTime value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDateTimeOffset(SmiEventSink sink, int ordinal, DateTimeOffset value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDBNull(SmiEventSink sink, int ordinal)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetDouble(SmiEventSink sink, int ordinal, double value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetGuid(SmiEventSink sink, int ordinal, Guid value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt16(SmiEventSink sink, int ordinal, short value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt32(SmiEventSink sink, int ordinal, int value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetInt64(SmiEventSink sink, int ordinal, long value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSingle(SmiEventSink sink, int ordinal, float value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetSqlDecimal(SmiEventSink sink, int ordinal, SqlDecimal value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetString(SmiEventSink sink, int ordinal, string value, int offset, int length)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetTimeSpan(SmiEventSink sink, int ordinal, TimeSpan value)
        {
            if (!this.CanSet)
            {
                throw ADP.InternalError(ADP.InternalErrorCode.InvalidSmiCall);
            }
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        public virtual void SetVariantMetaData(SmiEventSink sink, int ordinal, SmiMetaData metaData)
        {
            throw ADP.InternalError(ADP.InternalErrorCode.UnimplementedSMIMethod);
        }

        internal abstract bool CanGet { get; }

        internal abstract bool CanSet { get; }
    }
}

