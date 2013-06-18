namespace System.Data.Design
{
    using System;
    using System.Data;
    using System.Reflection;

    internal class DesignRelationCollection : DataSourceCollectionBase
    {
        private DesignDataSource dataSource;

        public DesignRelationCollection(DesignDataSource dataSource) : base(dataSource)
        {
            this.dataSource = dataSource;
        }

        public int Add(DesignRelation rel)
        {
            return base.List.Add(rel);
        }

        public bool Contains(DesignRelation rel)
        {
            return base.List.Contains(rel);
        }

        protected override void OnInsert(int index, object value)
        {
            base.ValidateType(value);
            DesignRelation relation = (DesignRelation) value;
            if ((this.dataSource == null) || (relation.Owner != this.dataSource))
            {
                if ((this.dataSource != null) && (relation.Owner != null))
                {
                    throw new InternalException("This relation belongs to another DataSource already", 0x4e2a);
                }
                if ((relation.Name == null) || (relation.Name.Length == 0))
                {
                    relation.Name = this.CreateUniqueName(relation);
                }
                this.ValidateName(relation);
                System.Data.DataSet dataSet = this.DataSet;
                if (dataSet != null)
                {
                    if (relation.ForeignKeyConstraint != null)
                    {
                        ForeignKeyConstraint foreignKeyConstraint = relation.ForeignKeyConstraint;
                        if (foreignKeyConstraint.Columns.Length > 0)
                        {
                            DataTable table = foreignKeyConstraint.Columns[0].Table;
                            if ((table != null) && !table.Constraints.Contains(foreignKeyConstraint.ConstraintName))
                            {
                                table.Constraints.Add(foreignKeyConstraint);
                            }
                        }
                    }
                    if ((relation.DataRelation != null) && !dataSet.Relations.Contains(relation.DataRelation.RelationName))
                    {
                        dataSet.Relations.Add(relation.DataRelation);
                    }
                }
                base.OnInsert(index, value);
                relation.Owner = this.dataSource;
            }
        }

        public void Remove(DesignRelation rel)
        {
            base.List.Remove(rel);
        }

        private System.Data.DataSet DataSet
        {
            get
            {
                if (this.dataSource != null)
                {
                    return this.dataSource.DataSet;
                }
                return null;
            }
        }

        internal DesignRelation this[ForeignKeyConstraint constraint]
        {
            get
            {
                if (constraint != null)
                {
                    foreach (DesignRelation relation in this)
                    {
                        if (relation.ForeignKeyConstraint == constraint)
                        {
                            return relation;
                        }
                    }
                }
                return null;
            }
        }

        internal DesignRelation this[string name]
        {
            get
            {
                return (DesignRelation) this.FindObject(name);
            }
        }

        protected override Type ItemType
        {
            get
            {
                return typeof(DesignRelation);
            }
        }

        protected override INameService NameService
        {
            get
            {
                return DataSetNameService.DefaultInstance;
            }
        }
    }
}

