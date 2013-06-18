namespace System.Data.Design
{
    using System;

    internal class DataAccessor : DataSourceComponent
    {
        internal const string DEFAULT_BASE_CLASS = "System.ComponentModel.Component";
        internal const string DEFAULT_NAME_POSTFIX = "TableAdapter";
        private System.Data.Design.DesignTable designTable;

        public DataAccessor(System.Data.Design.DesignTable designTable)
        {
            if (designTable == null)
            {
                throw new ArgumentNullException("DesignTable");
            }
            this.designTable = designTable;
        }

        internal System.Data.Design.DesignTable DesignTable
        {
            get
            {
                return this.designTable;
            }
        }
    }
}

