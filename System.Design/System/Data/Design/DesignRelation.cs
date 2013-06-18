namespace System.Data.Design
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;

    internal class DesignRelation : DataSourceComponent, IDataSourceNamedObject, INamedObject
    {
        private System.Data.ForeignKeyConstraint dataForeignKeyConstraint;
        private System.Data.DataRelation dataRelation;
        private const string EXTPROPNAME_GENERATOR_CHILDPROPNAME = "Generator_ChildPropName";
        private const string EXTPROPNAME_GENERATOR_PARENTPROPNAME = "Generator_ParentPropName";
        private const string EXTPROPNAME_GENERATOR_RELATIONVARNAME = "Generator_RelationVarName";
        private const string EXTPROPNAME_USER_CHILDTABLE = "Generator_UserChildTable";
        private const string EXTPROPNAME_USER_PARENTTABLE = "Generator_UserParentTable";
        private const string EXTPROPNAME_USER_RELATIONNAME = "Generator_UserRelationName";
        internal const string NAMEROOT = "Relation";
        private DesignDataSource owner;

        public DesignRelation(System.Data.DataRelation dataRelation)
        {
            this.DataRelation = dataRelation;
        }

        public DesignRelation(System.Data.ForeignKeyConstraint foreignKeyConstraint)
        {
            this.DataRelation = null;
            this.dataForeignKeyConstraint = foreignKeyConstraint;
        }

        internal DataColumn[] ChildDataColumns
        {
            get
            {
                if (this.dataRelation != null)
                {
                    return this.dataRelation.ChildColumns;
                }
                if (this.dataForeignKeyConstraint != null)
                {
                    return this.dataForeignKeyConstraint.Columns;
                }
                return new DataColumn[0];
            }
        }

        internal DesignTable ChildDesignTable
        {
            get
            {
                DataTable childTable = null;
                if (this.dataRelation != null)
                {
                    childTable = this.dataRelation.ChildTable;
                }
                else if (this.dataForeignKeyConstraint != null)
                {
                    childTable = this.dataForeignKeyConstraint.Table;
                }
                if ((childTable != null) && (this.Owner != null))
                {
                    return this.Owner.DesignTables[childTable];
                }
                return null;
            }
        }

        internal System.Data.DataRelation DataRelation
        {
            get
            {
                return this.dataRelation;
            }
            set
            {
                this.dataRelation = value;
                if (this.dataRelation != null)
                {
                    this.dataForeignKeyConstraint = null;
                }
            }
        }

        internal System.Data.ForeignKeyConstraint ForeignKeyConstraint
        {
            get
            {
                if ((this.dataRelation != null) && (this.dataRelation.ChildKeyConstraint != null))
                {
                    return this.dataRelation.ChildKeyConstraint;
                }
                return this.dataForeignKeyConstraint;
            }
            set
            {
                this.dataForeignKeyConstraint = value;
            }
        }

        internal string GeneratorChildPropName
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_ChildPropName"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_ChildPropName"] = value;
            }
        }

        internal string GeneratorParentPropName
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_ParentPropName"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_ParentPropName"] = value;
            }
        }

        internal string GeneratorRelationVarName
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_RelationVarName"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_RelationVarName"] = value;
            }
        }

        [MergableProperty(false), DefaultValue("")]
        public string Name
        {
            get
            {
                if (this.dataRelation != null)
                {
                    return this.dataRelation.RelationName;
                }
                if (this.dataForeignKeyConstraint != null)
                {
                    return this.dataForeignKeyConstraint.ConstraintName;
                }
                return string.Empty;
            }
            set
            {
                if (!StringUtil.EqualValue(this.Name, value))
                {
                    if (this.CollectionParent != null)
                    {
                        this.CollectionParent.ValidateUniqueName(this, value);
                    }
                    if (this.dataRelation != null)
                    {
                        this.dataRelation.RelationName = value;
                    }
                    if (this.dataForeignKeyConstraint != null)
                    {
                        this.dataForeignKeyConstraint.ConstraintName = value;
                    }
                }
            }
        }

        internal override StringCollection NamingPropertyNames
        {
            get
            {
                StringCollection strings = new StringCollection();
                strings.AddRange(new string[] { "typedParent", "typedChildren" });
                return strings;
            }
        }

        internal DesignDataSource Owner
        {
            get
            {
                return this.owner;
            }
            set
            {
                this.owner = value;
            }
        }

        internal DataColumn[] ParentDataColumns
        {
            get
            {
                if (this.dataRelation != null)
                {
                    return this.dataRelation.ParentColumns;
                }
                if (this.dataForeignKeyConstraint != null)
                {
                    return this.dataForeignKeyConstraint.RelatedColumns;
                }
                return new DataColumn[0];
            }
        }

        internal DesignTable ParentDesignTable
        {
            get
            {
                DataTable parentTable = null;
                if (this.dataRelation != null)
                {
                    parentTable = this.dataRelation.ParentTable;
                }
                else if (this.dataForeignKeyConstraint != null)
                {
                    parentTable = this.dataForeignKeyConstraint.RelatedTable;
                }
                if ((parentTable != null) && (this.Owner != null))
                {
                    return this.Owner.DesignTables[parentTable];
                }
                return null;
            }
        }

        [Browsable(false)]
        public string PublicTypeName
        {
            get
            {
                return "Relation";
            }
        }

        internal string UserChildTable
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_UserChildTable"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_UserChildTable"] = value;
            }
        }

        internal string UserParentTable
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_UserParentTable"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_UserParentTable"] = value;
            }
        }

        internal string UserRelationName
        {
            get
            {
                return (this.dataRelation.ExtendedProperties["Generator_UserRelationName"] as string);
            }
            set
            {
                this.dataRelation.ExtendedProperties["Generator_UserRelationName"] = value;
            }
        }

        [Flags]
        public enum CompareOption
        {
            Columns,
            Tables,
            ForeignKeyConstraints
        }
    }
}

