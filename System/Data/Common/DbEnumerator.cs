namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;
    using System.Data.ProviderBase;

    public class DbEnumerator : IEnumerator
    {
        internal IDataRecord _current;
        internal PropertyDescriptorCollection _descriptors;
        private FieldNameLookup _fieldNameLookup;
        internal IDataReader _reader;
        internal SchemaInfo[] _schemaInfo;
        private bool closeReader;

        public DbEnumerator(IDataReader reader)
        {
            if (reader == null)
            {
                throw ADP.ArgumentNull("reader");
            }
            this._reader = reader;
        }

        public DbEnumerator(IDataReader reader, bool closeReader)
        {
            if (reader == null)
            {
                throw ADP.ArgumentNull("reader");
            }
            this._reader = reader;
            this.closeReader = closeReader;
        }

        private void BuildSchemaInfo()
        {
            int fieldCount = this._reader.FieldCount;
            string[] columnNameArray = new string[fieldCount];
            for (int i = 0; i < fieldCount; i++)
            {
                columnNameArray[i] = this._reader.GetName(i);
            }
            ADP.BuildSchemaTableInfoTableNames(columnNameArray);
            SchemaInfo[] infoArray = new SchemaInfo[fieldCount];
            PropertyDescriptor[] properties = new PropertyDescriptor[this._reader.FieldCount];
            for (int j = 0; j < infoArray.Length; j++)
            {
                SchemaInfo info = new SchemaInfo {
                    name = this._reader.GetName(j),
                    type = this._reader.GetFieldType(j),
                    typeName = this._reader.GetDataTypeName(j)
                };
                properties[j] = new DbColumnDescriptor(j, columnNameArray[j], info.type);
                infoArray[j] = info;
            }
            this._schemaInfo = infoArray;
            this._fieldNameLookup = new FieldNameLookup(this._reader, -1);
            this._descriptors = new PropertyDescriptorCollection(properties);
        }

        public bool MoveNext()
        {
            if (this._schemaInfo == null)
            {
                this.BuildSchemaInfo();
            }
            this._current = null;
            if (this._reader.Read())
            {
                object[] values = new object[this._schemaInfo.Length];
                this._reader.GetValues(values);
                this._current = new DataRecordInternal(this._schemaInfo, values, this._descriptors, this._fieldNameLookup);
                return true;
            }
            if (this.closeReader)
            {
                this._reader.Close();
            }
            return false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Reset()
        {
            throw ADP.NotSupported();
        }

        public object Current
        {
            get
            {
                return this._current;
            }
        }

        private sealed class DbColumnDescriptor : PropertyDescriptor
        {
            private int _ordinal;
            private Type _type;

            internal DbColumnDescriptor(int ordinal, string name, Type type) : base(name, null)
            {
                this._ordinal = ordinal;
                this._type = type;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override object GetValue(object component)
            {
                return ((IDataRecord) component)[this._ordinal];
            }

            public override void ResetValue(object component)
            {
                throw ADP.NotSupported();
            }

            public override void SetValue(object component, object value)
            {
                throw ADP.NotSupported();
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get
                {
                    return typeof(IDataRecord);
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this._type;
                }
            }
        }
    }
}

