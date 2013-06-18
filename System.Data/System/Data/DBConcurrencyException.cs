namespace System.Data
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public sealed class DBConcurrencyException : SystemException
    {
        private DataRow[] _dataRows;

        public DBConcurrencyException() : this(Res.GetString("ADP_DBConcurrencyExceptionMessage"), null)
        {
        }

        public DBConcurrencyException(string message) : this(message, null)
        {
        }

        private DBConcurrencyException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public DBConcurrencyException(string message, Exception inner) : base(message, inner)
        {
            base.HResult = -2146232011;
        }

        public DBConcurrencyException(string message, Exception inner, DataRow[] dataRows) : base(message, inner)
        {
            base.HResult = -2146232011;
            this._dataRows = dataRows;
        }

        public void CopyToRows(DataRow[] array)
        {
            this.CopyToRows(array, 0);
        }

        public void CopyToRows(DataRow[] array, int arrayIndex)
        {
            DataRow[] rowArray = this._dataRows;
            if (rowArray != null)
            {
                rowArray.CopyTo(array, arrayIndex);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo si, StreamingContext context)
        {
            if (si == null)
            {
                throw new ArgumentNullException("si");
            }
            base.GetObjectData(si, context);
        }

        public DataRow Row
        {
            get
            {
                DataRow[] rowArray = this._dataRows;
                if ((rowArray != null) && (0 < rowArray.Length))
                {
                    return rowArray[0];
                }
                return null;
            }
            set
            {
                this._dataRows = new DataRow[] { value };
            }
        }

        public int RowCount
        {
            get
            {
                DataRow[] rowArray = this._dataRows;
                if (rowArray == null)
                {
                    return 0;
                }
                return rowArray.Length;
            }
        }
    }
}

