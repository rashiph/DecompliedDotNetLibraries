namespace System.Data.Common
{
    using System;
    using System.ComponentModel;
    using System.Data;

    public abstract class DbParameter : MarshalByRefObject, IDbDataParameter, IDataParameter
    {
        protected DbParameter()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public abstract void ResetDbType();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), RefreshProperties(RefreshProperties.All), ResDescription("DbParameter_DbType"), ResCategory("DataCategory_Data")]
        public abstract System.Data.DbType DbType { get; set; }

        [ResCategory("DataCategory_Data"), RefreshProperties(RefreshProperties.All), ResDescription("DbParameter_Direction"), DefaultValue(1)]
        public abstract ParameterDirection Direction { get; set; }

        [DesignOnly(true), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public abstract bool IsNullable { get; set; }

        [ResCategory("DataCategory_Data"), DefaultValue(""), ResDescription("DbParameter_ParameterName")]
        public abstract string ParameterName { get; set; }

        [ResDescription("DbParameter_Size"), ResCategory("DataCategory_Data")]
        public abstract int Size { get; set; }

        [ResDescription("DbParameter_SourceColumn"), ResCategory("DataCategory_Update"), DefaultValue("")]
        public abstract string SourceColumn { get; set; }

        [ResCategory("DataCategory_Update"), RefreshProperties(RefreshProperties.All), ResDescription("DbParameter_SourceColumnNullMapping"), EditorBrowsable(EditorBrowsableState.Advanced), DefaultValue(false)]
        public abstract bool SourceColumnNullMapping { get; set; }

        [ResCategory("DataCategory_Update"), ResDescription("DbParameter_SourceVersion"), DefaultValue(0x200)]
        public abstract DataRowVersion SourceVersion { get; set; }

        byte IDbDataParameter.Precision
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        byte IDbDataParameter.Scale
        {
            get
            {
                return 0;
            }
            set
            {
            }
        }

        [DefaultValue((string) null), ResCategory("DataCategory_Data"), ResDescription("DbParameter_Value"), RefreshProperties(RefreshProperties.All)]
        public abstract object Value { get; set; }
    }
}

