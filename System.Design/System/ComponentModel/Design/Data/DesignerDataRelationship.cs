namespace System.ComponentModel.Design.Data
{
    using System;
    using System.Collections;

    public sealed class DesignerDataRelationship
    {
        private ICollection _childColumns;
        private DesignerDataTable _childTable;
        private string _name;
        private ICollection _parentColumns;

        public DesignerDataRelationship(string name, ICollection parentColumns, DesignerDataTable childTable, ICollection childColumns)
        {
            this._childColumns = childColumns;
            this._childTable = childTable;
            this._name = name;
            this._parentColumns = parentColumns;
        }

        public ICollection ChildColumns
        {
            get
            {
                return this._childColumns;
            }
        }

        public DesignerDataTable ChildTable
        {
            get
            {
                return this._childTable;
            }
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public ICollection ParentColumns
        {
            get
            {
                return this._parentColumns;
            }
        }
    }
}

