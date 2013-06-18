namespace System.Data.SqlClient
{
    using System;
    using System.Reflection;

    internal sealed class Row
    {
        private object[] _dataFields;

        internal Row(int rowCount)
        {
            this._dataFields = new object[rowCount];
        }

        internal object[] DataFields
        {
            get
            {
                return this._dataFields;
            }
        }

        internal object this[int index]
        {
            get
            {
                return this._dataFields[index];
            }
        }
    }
}

