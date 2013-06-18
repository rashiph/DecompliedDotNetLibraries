namespace System.Data.OleDb
{
    using System;

    internal sealed class MetaData : IComparable
    {
        internal string baseCatalogName;
        internal string baseColumnName;
        internal string baseSchemaName;
        internal string baseTableName;
        internal Bindings bindings;
        internal ColumnBinding columnBinding;
        internal string columnName;
        internal int flags;
        internal Guid guid;
        internal string idname;
        internal bool isAutoIncrement;
        internal bool isHidden;
        internal bool isKeyColumn;
        internal bool isUnique;
        internal int kind;
        internal IntPtr ordinal;
        internal byte precision;
        internal IntPtr propid;
        internal byte scale;
        internal int size;
        internal NativeDBType type;

        internal MetaData()
        {
        }

        int IComparable.CompareTo(object obj)
        {
            if (this.isHidden == (obj as MetaData).isHidden)
            {
                return (((int) this.ordinal) - ((int) (obj as MetaData).ordinal));
            }
            if (!this.isHidden)
            {
                return -1;
            }
            return 1;
        }
    }
}

