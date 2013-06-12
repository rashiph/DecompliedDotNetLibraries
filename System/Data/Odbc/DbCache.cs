namespace System.Data.Odbc
{
    using System;
    using System.Data;
    using System.Reflection;

    internal sealed class DbCache
    {
        internal int _count;
        private bool[] _isBadValue;
        internal bool _randomaccess = true;
        private OdbcDataReader _record;
        private DbSchemaInfo[] _schema;
        private object[] _values;

        internal DbCache(OdbcDataReader record, int count)
        {
            this._count = count;
            this._record = record;
            this._randomaccess = !record.IsBehavior(CommandBehavior.SequentialAccess);
            this._values = new object[count];
            this._isBadValue = new bool[count];
        }

        internal object AccessIndex(int i)
        {
            object[] values = this.Values;
            if (this._randomaccess)
            {
                for (int j = 0; j < i; j++)
                {
                    if (values[j] == null)
                    {
                        values[j] = this._record.GetValue(j);
                    }
                }
            }
            return values[i];
        }

        internal void FlushValues()
        {
            int length = this._values.Length;
            for (int i = 0; i < length; i++)
            {
                this._values[i] = null;
            }
        }

        internal DbSchemaInfo GetSchema(int i)
        {
            if (this._schema == null)
            {
                this._schema = new DbSchemaInfo[this.Count];
            }
            if (this._schema[i] == null)
            {
                this._schema[i] = new DbSchemaInfo();
            }
            return this._schema[i];
        }

        internal void InvalidateValue(int i)
        {
            this._isBadValue[i] = true;
        }

        internal int Count
        {
            get
            {
                return this._count;
            }
        }

        internal object this[int i]
        {
            get
            {
                if (this._isBadValue[i])
                {
                    OverflowException innerException = (OverflowException) this.Values[i];
                    throw new OverflowException(innerException.Message, innerException);
                }
                return this.Values[i];
            }
            set
            {
                this.Values[i] = value;
                this._isBadValue[i] = false;
            }
        }

        internal object[] Values
        {
            get
            {
                return this._values;
            }
        }
    }
}

