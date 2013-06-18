namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Xml;

    internal class DesignTable : DataSourceComponent, IDataSourceNamedObject, INamedObject, IDataSourceXmlSerializable, IDataSourceXmlSpecialOwner, IDataSourceInitAfterLoading, IDataSourceCommandTarget
    {
        private string baseClass;
        private CodeGenPropertyCache codeGenPropertyCache;
        private System.Data.Design.DataAccessor dataAccessor;
        private TypeAttributes dataAccessorModifier;
        private string dataAccessorName;
        private System.Data.DataTable dataTable;
        private const string DATATABLE_NAMEROOT = "DataTable";
        private DesignColumnCollection designColumns;
        internal static string EXTPROPNAME_GENERATOR_ROWCHANGEDNAME = "Generator_RowChangedName";
        internal static string EXTPROPNAME_GENERATOR_ROWCHANGINGNAME = "Generator_RowChangingName";
        internal static string EXTPROPNAME_GENERATOR_ROWCLASSNAME = "Generator_RowClassName";
        internal static string EXTPROPNAME_GENERATOR_ROWDELETEDNAME = "Generator_RowDeletedName";
        internal static string EXTPROPNAME_GENERATOR_ROWDELETINGNAME = "Generator_RowDeletingName";
        internal static string EXTPROPNAME_GENERATOR_ROWEVARGNAME = "Generator_RowEvArgName";
        internal static string EXTPROPNAME_GENERATOR_ROWEVHANDLERNAME = "Generator_RowEvHandlerName";
        internal static string EXTPROPNAME_GENERATOR_TABLECLASSNAME = "Generator_TableClassName";
        internal static string EXTPROPNAME_GENERATOR_TABLEPROPNAME = "Generator_TablePropName";
        internal static string EXTPROPNAME_GENERATOR_TABLEVARNAME = "Generator_TableVarName";
        internal static string EXTPROPNAME_USER_TABLENAME = "Generator_UserTableName";
        private string generatorDataComponentClassName;
        private string generatorRunFillName;
        private bool inAccessConstraints;
        private const string KEY_NAMEROOT = "Key";
        private Source mainSource;
        private const string MAINSOURCE_NAME = "Fill";
        internal const string MAINSOURCE_PROPERTY = "MainSource";
        private DataColumnMappingCollection mappings;
        internal const string NAME_PROPERTY = "Name";
        private StringCollection namingPropNames;
        private DesignDataSource owner;
        private const string PRIMARYKEY_PROPERTY = "PrimaryKey";
        private string provider;
        private const string RADTABLE_NAMEROOT = "DataTable";
        private SourceCollection sources;
        private System.Data.Design.TableType tableType;
        private string userDataComponentName;
        private bool webServiceAttribute;
        private string webServiceDescription;
        private string webServiceNamespace;

        internal event EventHandler ConstraintChanged
        {
            add
            {
                this.constraintsChanged += value;
            }
            remove
            {
                this.constraintsChanged -= value;
            }
        }

        private event EventHandler constraintsChanged;

        private event EventHandler dataAccessorChanged;

        internal event EventHandler DataAccessorChanged
        {
            add
            {
                this.dataAccessorChanged += value;
            }
            remove
            {
                this.dataAccessorChanged -= value;
            }
        }

        private event EventHandler dataAccessorChanging;

        internal event EventHandler DataAccessorChanging
        {
            add
            {
                this.dataAccessorChanging += value;
            }
            remove
            {
                this.dataAccessorChanging -= value;
            }
        }

        private event EventHandler tableTypeChanged;

        internal event EventHandler TableTypeChanged
        {
            add
            {
                this.tableTypeChanged += value;
            }
            remove
            {
                this.tableTypeChanged -= value;
            }
        }

        public DesignTable() : this(null, System.Data.Design.TableType.DataTable)
        {
        }

        public DesignTable(System.Data.DataTable dataTable) : this(dataTable, System.Data.Design.TableType.DataTable)
        {
        }

        public DesignTable(System.Data.DataTable dataTable, System.Data.Design.TableType tableType)
        {
            this.dataAccessorModifier = TypeAttributes.Public;
            this.namingPropNames = new StringCollection();
            if (dataTable == null)
            {
                this.dataTable = new System.Data.DataTable();
                this.dataTable.Locale = CultureInfo.InvariantCulture;
            }
            else
            {
                this.dataTable = dataTable;
            }
            this.TableType = tableType;
            this.AddRemoveConstraintMonitor(true);
            this.namingPropNames.AddRange(new string[] { "typedPlural", "typedName" });
        }

        public DesignTable(System.Data.DataTable dataTable, System.Data.Design.TableType tableType, DataColumnMappingCollection mappings) : this(dataTable, tableType)
        {
            this.mappings = mappings;
        }

        private bool AddPrimaryKeyFromSchemaTable(System.Data.DataTable schemaTable)
        {
            if ((schemaTable.PrimaryKey.Length <= 0) || (this.DataTable.PrimaryKey.Length != 0))
            {
                return false;
            }
            DataColumn[] columnArray = new DataColumn[schemaTable.PrimaryKey.Length];
            for (int i = 0; i < schemaTable.PrimaryKey.Length; i++)
            {
                DataColumn column = schemaTable.PrimaryKey[i];
                if (!this.Mappings.Contains(column.ColumnName))
                {
                    return false;
                }
                string dataSetColumn = this.Mappings[column.ColumnName].DataSetColumn;
                if (!this.DataTable.Columns.Contains(dataSetColumn))
                {
                    return false;
                }
                columnArray[i] = this.DataTable.Columns[dataSetColumn];
            }
            this.PrimaryKeyColumns = columnArray;
            return true;
        }

        private void AddRemoveConstraintMonitor(bool addEventHandler)
        {
            if (addEventHandler)
            {
                if (this.DataTable != null)
                {
                    this.DataTable.Constraints.CollectionChanged += new CollectionChangeEventHandler(this.OnConstraintCollectionChanged);
                }
            }
            else if (this.DataTable != null)
            {
                this.DataTable.Constraints.CollectionChanged -= new CollectionChangeEventHandler(this.OnConstraintCollectionChanged);
            }
        }

        internal void ConvertTableTypeTo(System.Data.Design.TableType newTableType)
        {
            if (newTableType != this.tableType)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.OnComponentChanging(this, null);
                }
                try
                {
                    this.TableType = newTableType;
                    this.mainSource = null;
                    this.sources = null;
                    this.mappings = null;
                    this.provider = string.Empty;
                    this.OnTableTypeChanged();
                }
                finally
                {
                    if (service != null)
                    {
                        service.OnComponentChanged(this, null, null, null);
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.AddRemoveConstraintMonitor(false);
            }
            base.Dispose(disposing);
        }

        private DbSource EnsureDbSource()
        {
            if (this.tableType != System.Data.Design.TableType.RadTable)
            {
                throw new InternalException(null, "Operation invalid. Table gets data from something else than a database.", 0x4e27, false, false);
            }
            if (this.MainSource == null)
            {
                this.MainSource = new DbSource();
            }
            DbSource mainSource = this.mainSource as DbSource;
            if (mainSource == null)
            {
                throw new InternalException(null, "Operation invalid. Table gets data from something else than a database.", 0x4e27, false, false);
            }
            if ((mainSource.DeleteCommand != null) && StringUtil.EmptyOrSpace(mainSource.DeleteCommand.Name))
            {
                mainSource.DeleteCommand.Name = "(DeleteCommand)";
            }
            if ((mainSource.UpdateCommand != null) && StringUtil.EmptyOrSpace(mainSource.UpdateCommand.Name))
            {
                mainSource.UpdateCommand.Name = "(UpdateCommand)";
            }
            if ((mainSource.SelectCommand != null) && StringUtil.EmptyOrSpace(mainSource.SelectCommand.Name))
            {
                mainSource.SelectCommand.Name = "(SelectCommand)";
            }
            if ((mainSource.InsertCommand != null) && StringUtil.EmptyOrSpace(mainSource.InsertCommand.Name))
            {
                mainSource.InsertCommand.Name = "(InsertCommand)";
            }
            return mainSource;
        }

        private DataColumn FindSharedColumn(ICollection dataColumns, ICollection designColumns)
        {
            foreach (DataColumn column in dataColumns)
            {
                foreach (object obj2 in designColumns)
                {
                    DesignColumn column2 = obj2 as DesignColumn;
                    if ((column2 != null) && (column2.DataColumn == column))
                    {
                        return column;
                    }
                }
            }
            return null;
        }

        internal ArrayList GetRelatedDataConstraints(ICollection columns, bool uniqueOnly)
        {
            ArrayList list = new ArrayList();
            foreach (Constraint constraint in this.dataTable.Constraints)
            {
                DataColumn[] columnArray = null;
                if (constraint is UniqueConstraint)
                {
                    columnArray = ((UniqueConstraint) constraint).Columns;
                }
                else if (!uniqueOnly && (constraint is ForeignKeyConstraint))
                {
                    columnArray = ((ForeignKeyConstraint) constraint).Columns;
                }
                if (columnArray != null)
                {
                    foreach (object obj2 in columns)
                    {
                        if (obj2 is DesignColumn)
                        {
                            DesignColumn column = obj2 as DesignColumn;
                            if (columnArray.Contains(column.DataColumn))
                            {
                                list.Add(constraint);
                                break;
                            }
                        }
                    }
                }
            }
            return list;
        }

        internal string GetUniqueRelationName(string proposedName)
        {
            return this.GetUniqueRelationName(proposedName, true, 1);
        }

        internal string GetUniqueRelationName(string proposedName, int startSuffix)
        {
            return this.GetUniqueRelationName(proposedName, false, startSuffix);
        }

        internal string GetUniqueRelationName(string proposedName, bool firstTryProposedName, int startSuffix)
        {
            if (this.Owner == null)
            {
                throw new InternalException("Need have DataSource");
            }
            SimpleNamedObjectCollection container = new SimpleNamedObjectCollection();
            foreach (DesignRelation relation in this.Owner.DesignRelations)
            {
                container.Add(new SimpleNamedObject(relation.Name));
            }
            foreach (Constraint constraint in this.DataTable.Constraints)
            {
                container.Add(new SimpleNamedObject(constraint.ConstraintName));
            }
            INameService nameService = container.GetNameService();
            if (firstTryProposedName)
            {
                return nameService.CreateUniqueName(container, proposedName);
            }
            return nameService.CreateUniqueName(container, proposedName, startSuffix);
        }

        internal bool IsForeignKeyConstraint(DataColumn column)
        {
            foreach (Constraint constraint in this.dataTable.Constraints)
            {
                DataColumn[] columns = null;
                if (constraint is ForeignKeyConstraint)
                {
                    columns = ((ForeignKeyConstraint) constraint).Columns;
                }
                if ((columns != null) && columns.Contains(column))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsInConstraintCollection(Constraint constraint)
        {
            return ((this.DataTable != null) && (this.DataTable.Constraints[constraint.ConstraintName] == constraint));
        }

        private void OnConstraintChanged()
        {
            if (this.constraintsChanged != null)
            {
                this.constraintsChanged(this, new EventArgs());
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.OnComponentChanged(this, null, null, null);
                }
            }
        }

        private void OnConstraintCollectionChanged(object sender, CollectionChangeEventArgs ccevent)
        {
            if (!this.inAccessConstraints)
            {
                this.OnConstraintChanged();
            }
        }

        internal void OnTableTypeChanged()
        {
            if (this.tableTypeChanged != null)
            {
                this.tableTypeChanged(this, EventArgs.Empty);
            }
        }

        internal void RemoveColumnMapping(string columnName)
        {
        }

        private void RemoveColumnsFromSource(Source source, string[] colsToRemove)
        {
        }

        internal void RemoveConstraint(Constraint constraint)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.OnComponentChanging(this, null);
            }
            try
            {
                this.inAccessConstraints = true;
                if (this.dataTable.Constraints.CanRemove(constraint))
                {
                    this.dataTable.Constraints.Remove(constraint);
                }
                else if (this.dataTable.Constraints.Count == 1)
                {
                    if (this.dataTable.Constraints[0] == constraint)
                    {
                        this.dataTable.Constraints.Clear();
                    }
                }
                else
                {
                    Constraint[] constraints = new Constraint[this.dataTable.Constraints.Count - 1];
                    ArrayList list = new ArrayList();
                    int num = 0;
                    foreach (Constraint constraint2 in this.dataTable.Constraints)
                    {
                        if (constraint2 != constraint)
                        {
                            constraints[num++] = constraint2;
                        }
                    }
                    if (this.Owner != null)
                    {
                        foreach (DataRelation relation in this.Owner.DataSet.Relations)
                        {
                            if (relation.ChildTable == this.dataTable)
                            {
                                list.Add(relation);
                            }
                        }
                        foreach (DataRelation relation2 in list)
                        {
                            this.Owner.DataSet.Relations.Remove(relation2);
                        }
                    }
                    this.dataTable.Constraints.Clear();
                    this.dataTable.Constraints.AddRange(constraints);
                    if (this.Owner != null)
                    {
                        foreach (DataRelation relation3 in list)
                        {
                            this.Owner.DataSet.Relations.Add(relation3);
                        }
                    }
                }
            }
            finally
            {
                this.inAccessConstraints = false;
                this.OnConstraintChanged();
            }
        }

        internal void RemoveKey(UniqueConstraint constraint)
        {
            ArrayList list = new ArrayList();
            foreach (DesignRelation relation in this.owner.DesignRelations)
            {
                DataRelation dataRelation = relation.DataRelation;
                if ((dataRelation != null) && (dataRelation.ParentKeyConstraint == constraint))
                {
                    list.Add(relation);
                }
            }
            foreach (DesignRelation relation3 in list)
            {
                this.owner.DesignRelations.Remove(relation3);
            }
            this.RemoveConstraint(constraint);
        }

        internal void SetTypeForUndo(System.Data.Design.TableType newType)
        {
            this.tableType = newType;
        }

        private bool ShouldSerializeMappings()
        {
            return ((this.mappings != null) && (this.mappings.Count > 0));
        }

        private bool ShouldSerializeParameters()
        {
            if (this.TableType != System.Data.Design.TableType.RadTable)
            {
                return false;
            }
            DbSourceParameterCollection parameters = this.Parameters;
            return ((parameters != null) && (0 < parameters.Count));
        }

        void IDataSourceCommandTarget.AddChild(object child, bool fixName)
        {
            if (child is DesignColumn)
            {
                this.DesignColumns.Add((DesignColumn) child);
            }
            else if (child is Source)
            {
                if (child is DbSource)
                {
                    ((DbSource) child).Connection = this.Connection;
                    if (this.Connection != null)
                    {
                        ((DbSource) child).ConnectionRef = this.Connection.Name;
                    }
                }
                this.Sources.Add((Source) child);
            }
        }

        bool IDataSourceCommandTarget.CanAddChildOfType(Type childType)
        {
            return ((typeof(DesignColumn).IsAssignableFrom(childType) || ((this.TableType != System.Data.Design.TableType.DataTable) && typeof(Source).IsAssignableFrom(childType))) || (typeof(DesignRelation).IsAssignableFrom(childType) && (this.DesignColumns.Count > 0)));
        }

        bool IDataSourceCommandTarget.CanInsertChildOfType(Type childType, object refChild)
        {
            if (typeof(DesignColumn).IsAssignableFrom(childType))
            {
                return (refChild is DesignColumn);
            }
            if (!typeof(Source).IsAssignableFrom(childType))
            {
                return false;
            }
            return ((this.TableType != System.Data.Design.TableType.DataTable) && (refChild is Source));
        }

        bool IDataSourceCommandTarget.CanRemoveChildren(ICollection children)
        {
            foreach (object obj2 in children)
            {
                if (obj2 is DesignColumn)
                {
                    if (((DesignColumn) obj2).DesignTable != this)
                    {
                        return false;
                    }
                }
                else if (obj2 is Source)
                {
                    if (!this.Sources.Contains((Source) obj2))
                    {
                        return false;
                    }
                }
                else if (obj2 is System.Data.Design.DataAccessor)
                {
                    if (((System.Data.Design.DataAccessor) obj2).DesignTable != this)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        object IDataSourceCommandTarget.GetObject(int index, bool getSiblingIfOutOfRange)
        {
            int count = this.DesignColumns.Count;
            int num2 = (this.TableType == System.Data.Design.TableType.DataTable) ? 0 : this.Sources.Count;
            int num3 = (this.TableType == System.Data.Design.TableType.DataTable) ? count : ((count + num2) + 1);
            if (num3 > 0)
            {
                if (!getSiblingIfOutOfRange && ((index < 0) || (index >= num3)))
                {
                    return null;
                }
                if (index >= num3)
                {
                    index = num3 - 1;
                }
                IList sources = this.Sources;
                if (index < 0)
                {
                    if (count > 0)
                    {
                        return this.DesignColumns[0];
                    }
                    if (this.mainSource != null)
                    {
                        return this.mainSource;
                    }
                    if (num2 > 0)
                    {
                        return sources[0];
                    }
                    return null;
                }
                if (index < count)
                {
                    return this.DesignColumns[index];
                }
                if (this.TableType != System.Data.Design.TableType.DataTable)
                {
                    index -= count;
                    if (index == 0)
                    {
                        return this.MainSource;
                    }
                    index--;
                    if (index < num2)
                    {
                        return sources[index];
                    }
                }
            }
            return null;
        }

        int IDataSourceCommandTarget.IndexOf(object child)
        {
            if (child is DesignColumn)
            {
                return this.DesignColumns.IndexOf((DesignColumn) child);
            }
            if ((child is Source) && (this.TableType != System.Data.Design.TableType.DataTable))
            {
                if (child == this.mainSource)
                {
                    return this.DesignColumns.Count;
                }
                int index = this.Sources.IndexOf((Source) child);
                if (index >= 0)
                {
                    return ((this.DesignColumns.Count + index) + 1);
                }
            }
            return -1;
        }

        void IDataSourceCommandTarget.InsertChild(object child, object refChild)
        {
            if (refChild == null)
            {
                ((IDataSourceCommandTarget) this).AddChild(child, true);
            }
            else if (child is DesignColumn)
            {
                this.DesignColumns.InsertBefore(child, refChild);
            }
            else if ((this.TableType != System.Data.Design.TableType.DataTable) && (child is Source))
            {
                this.Sources.InsertBefore(child, refChild);
            }
        }

        void IDataSourceCommandTarget.RemoveChildren(ICollection children)
        {
            if (this.owner != null)
            {
                ArrayList relatedRelations = this.owner.GetRelatedRelations(new DesignTable[] { this });
                if (relatedRelations.Count > 0)
                {
                    int num = 0;
                    ArrayList list2 = new ArrayList();
                    foreach (DesignRelation relation in relatedRelations)
                    {
                        if ((relation.ParentDesignTable == this) && (this.FindSharedColumn(relation.ParentDataColumns, children) != null))
                        {
                            num++;
                            list2.Add(relation);
                        }
                        else if ((relation.ChildDesignTable == this) && (this.FindSharedColumn(relation.ChildDataColumns, children) != null))
                        {
                            num++;
                            list2.Add(relation);
                        }
                    }
                    if (num > 0)
                    {
                        foreach (DesignRelation relation2 in list2)
                        {
                            if (relation2.Owner != null)
                            {
                                relation2.Owner.DesignRelations.Remove(relation2);
                            }
                        }
                    }
                }
            }
            foreach (UniqueConstraint constraint in this.GetRelatedDataConstraints(children, true))
            {
                if (constraint.IsPrimaryKey)
                {
                    this.PrimaryKeyColumns = null;
                }
                else
                {
                    this.RemoveConstraint(constraint);
                }
            }
            foreach (Constraint constraint2 in this.GetRelatedDataConstraints(children, false))
            {
                this.RemoveConstraint(constraint2);
            }
            ArrayList list4 = new ArrayList();
            foreach (object obj2 in children)
            {
                if (obj2 is DesignColumn)
                {
                    DesignColumn column3 = (DesignColumn) obj2;
                    string[] strArray = DataDesignUtil.MapColumnNames(this.Mappings, new string[] { column3.Name }, DataDesignUtil.MappingDirection.DataSetToSource);
                    list4.Add(strArray[0]);
                    this.DesignColumns.Remove((DesignColumn) obj2);
                    this.RemoveColumnMapping(column3.Name);
                }
                else if (obj2 is Source)
                {
                    this.Sources.Remove((Source) obj2);
                }
                else if (obj2 is System.Data.Design.DataAccessor)
                {
                    this.ConvertTableTypeTo(System.Data.Design.TableType.DataTable);
                }
            }
            if (list4.Count > 0)
            {
                string[] colsToRemove = (string[]) list4.ToArray(typeof(string));
                this.RemoveColumnsFromSource(this.MainSource, colsToRemove);
                foreach (Source source in this.Sources)
                {
                    this.RemoveColumnsFromSource(source, colsToRemove);
                }
            }
        }

        void IDataSourceInitAfterLoading.InitializeAfterLoading()
        {
            if ((this.Name == null) || (this.Name.Length == 0))
            {
                throw new DataSourceSerializationException(System.Design.SR.GetString("DTDS_NameIsRequired", new object[] { "RadTable" }));
            }
            if (this.dataTable.DataSet != this.Owner.DataSet)
            {
                throw new DataSourceSerializationException(System.Design.SR.GetString("DTDS_TableNotMatch", new object[] { this.Name }));
            }
        }

        void IDataSourceXmlSerializable.ReadXml(XmlElement xmlElement, DataSourceXmlSerializer serializer)
        {
            if ((xmlElement.LocalName == "TableAdapter") || (xmlElement.LocalName == "DbTable"))
            {
                this.TableType = System.Data.Design.TableType.RadTable;
                serializer.DeserializeBody(xmlElement, this);
            }
        }

        void IDataSourceXmlSerializable.WriteXml(XmlWriter xmlWriter, DataSourceXmlSerializer serializer)
        {
            switch (this.TableType)
            {
                case System.Data.Design.TableType.DataTable:
                    break;

                case System.Data.Design.TableType.RadTable:
                    xmlWriter.WriteStartElement(string.Empty, "TableAdapter", "urn:schemas-microsoft-com:xml-msdatasource");
                    serializer.SerializeBody(xmlWriter, this);
                    xmlWriter.WriteFullEndElement();
                    break;

                default:
                    return;
            }
        }

        void IDataSourceXmlSpecialOwner.ReadSpecialItem(string propertyName, XmlNode xmlNode, DataSourceXmlSerializer serializer)
        {
            if (propertyName == "Mappings")
            {
                string sourceColumn = string.Empty;
                string dataSetColumn = string.Empty;
                XmlElement element = xmlNode as XmlElement;
                if (element != null)
                {
                    foreach (XmlNode node in element.ChildNodes)
                    {
                        XmlElement element2 = node as XmlElement;
                        if ((element2 != null) && (element2.LocalName == "Mapping"))
                        {
                            XmlAttribute attribute = element2.Attributes["SourceColumn"];
                            if (attribute != null)
                            {
                                sourceColumn = attribute.InnerText;
                            }
                            attribute = element2.Attributes["DataSetColumn"];
                            if (attribute != null)
                            {
                                dataSetColumn = attribute.InnerText;
                            }
                            DataColumnMapping mapping = new DataColumnMapping(sourceColumn, dataSetColumn);
                            this.Mappings.Add(mapping);
                        }
                    }
                }
            }
        }

        void IDataSourceXmlSpecialOwner.WriteSpecialItem(string propertyName, XmlWriter writer, DataSourceXmlSerializer serializer)
        {
            if (propertyName == "Mappings")
            {
                foreach (DataColumnMapping mapping in this.Mappings)
                {
                    writer.WriteStartElement(string.Empty, "Mapping", "urn:schemas-microsoft-com:xml-msdatasource");
                    writer.WriteAttributeString("SourceColumn", mapping.SourceColumn);
                    writer.WriteAttributeString("DataSetColumn", mapping.DataSetColumn);
                    writer.WriteEndElement();
                }
            }
        }

        internal void UpdateColumnMappingDataSetColumnName(string oldName, string newName)
        {
        }

        internal void UpdateColumnMappingSourceColumnName(string dataSetColumn, string newSourceColumn)
        {
        }

        [Browsable(false), DataSourceXmlAttribute]
        public string BaseClass
        {
            get
            {
                if (StringUtil.NotEmptyAfterTrim(this.baseClass))
                {
                    return this.baseClass;
                }
                return "System.ComponentModel.Component";
            }
            set
            {
                this.baseClass = value;
            }
        }

        public IDesignConnection Connection
        {
            get
            {
                if (this.TableType == System.Data.Design.TableType.RadTable)
                {
                    return this.EnsureDbSource().Connection;
                }
                return null;
            }
            set
            {
                if (this.TableType == System.Data.Design.TableType.RadTable)
                {
                    this.EnsureDbSource().Connection = value;
                }
            }
        }

        internal System.Data.Design.DataAccessor DataAccessor
        {
            get
            {
                return this.dataAccessor;
            }
            set
            {
                if (this.dataAccessorChanging != null)
                {
                    this.dataAccessorChanging(this, new EventArgs());
                }
                this.dataAccessor = value;
                if (this.dataAccessorChanged != null)
                {
                    this.dataAccessorChanged(this, new EventArgs());
                }
            }
        }

        [DataSourceXmlAttribute, DefaultValue(1)]
        public TypeAttributes DataAccessorModifier
        {
            get
            {
                return this.dataAccessorModifier;
            }
            set
            {
                this.dataAccessorModifier = value;
            }
        }

        [Browsable(false), DataSourceXmlAttribute]
        public string DataAccessorName
        {
            get
            {
                if (StringUtil.NotEmptyAfterTrim(this.dataAccessorName))
                {
                    return this.dataAccessorName;
                }
                return (this.Name + "TableAdapter");
            }
            set
            {
                this.dataAccessorName = value;
            }
        }

        [Browsable(false)]
        public System.Data.DataTable DataTable
        {
            get
            {
                return this.dataTable;
            }
            set
            {
                if (this.dataTable != value)
                {
                    if (this.dataTable != null)
                    {
                        this.AddRemoveConstraintMonitor(false);
                    }
                    this.dataTable = value;
                    if (this.dataTable != null)
                    {
                        this.AddRemoveConstraintMonitor(true);
                    }
                }
            }
        }

        [DefaultValue((string) null)]
        public DbSourceCommand DeleteCommand
        {
            get
            {
                return this.EnsureDbSource().DeleteCommand;
            }
            set
            {
                this.EnsureDbSource().DeleteCommand = value;
            }
        }

        [Browsable(false)]
        public DesignColumnCollection DesignColumns
        {
            get
            {
                if (this.designColumns == null)
                {
                    this.designColumns = new DesignColumnCollection(this);
                }
                return this.designColumns;
            }
        }

        protected override object ExternalPropertyHost
        {
            get
            {
                return this.dataTable;
            }
        }

        [Browsable(false), DefaultValue((string) null), DataSourceXmlAttribute]
        public string GeneratorDataComponentClassName
        {
            get
            {
                return this.generatorDataComponentClassName;
            }
            set
            {
                this.generatorDataComponentClassName = value;
            }
        }

        [Browsable(false)]
        public override string GeneratorName
        {
            get
            {
                return this.GeneratorTablePropName;
            }
        }

        internal string GeneratorRowChangedName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCHANGEDNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCHANGEDNAME] = value;
            }
        }

        internal string GeneratorRowChangingName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCHANGINGNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCHANGINGNAME] = value;
            }
        }

        internal string GeneratorRowClassName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCLASSNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWCLASSNAME] = value;
            }
        }

        internal string GeneratorRowDeletedName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWDELETEDNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWDELETEDNAME] = value;
            }
        }

        internal string GeneratorRowDeletingName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWDELETINGNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWDELETINGNAME] = value;
            }
        }

        internal string GeneratorRowEvArgName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWEVARGNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWEVARGNAME] = value;
            }
        }

        internal string GeneratorRowEvHandlerName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWEVHANDLERNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_ROWEVHANDLERNAME] = value;
            }
        }

        internal string GeneratorRunFillName
        {
            get
            {
                return this.generatorRunFillName;
            }
            set
            {
                this.generatorRunFillName = value;
            }
        }

        internal string GeneratorTableClassName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLECLASSNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLECLASSNAME] = value;
            }
        }

        internal string GeneratorTablePropName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLEPROPNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLEPROPNAME] = value;
            }
        }

        internal string GeneratorTableVarName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLEVARNAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_GENERATOR_TABLEVARNAME] = value;
            }
        }

        internal bool HasAnyExpressionColumn
        {
            get
            {
                foreach (DataColumn column in this.DataTable.Columns)
                {
                    if ((column.Expression != null) && (column.Expression.Length > 0))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        internal bool HasAnyUpdateCommand
        {
            get
            {
                if ((((this.TableType != System.Data.Design.TableType.RadTable) || (this.MainSource == null)) || (!(this.MainSource is DbSource) || (((DbSource) this.MainSource).CommandOperation != CommandOperation.Select))) || (((this.DeleteCommand == null) && (this.InsertCommand == null)) && (this.UpdateCommand == null)))
                {
                    return false;
                }
                return true;
            }
        }

        [DefaultValue((string) null)]
        public DbSourceCommand InsertCommand
        {
            get
            {
                return this.EnsureDbSource().InsertCommand;
            }
            set
            {
                this.EnsureDbSource().InsertCommand = value;
            }
        }

        [DataSourceXmlSubItem(Name="MainSource", ItemType=typeof(Source)), Browsable(false)]
        public Source MainSource
        {
            get
            {
                if (this.mainSource == null)
                {
                    DbSource source = new DbSource();
                    if (this.Owner != null)
                    {
                        source.Connection = this.Owner.DefaultConnection;
                    }
                    this.MainSource = source;
                }
                return this.mainSource;
            }
            set
            {
                if (this.mainSource != null)
                {
                    this.mainSource.Owner = null;
                }
                this.mainSource = value;
                if (value != null)
                {
                    this.mainSource.Owner = this;
                    if (StringUtil.EmptyOrSpace(this.mainSource.Name))
                    {
                        this.mainSource.Name = "Fill";
                    }
                }
            }
        }

        [Browsable(false), DataSourceXmlElement(Name="Mappings", SpecialWay=true)]
        public DataColumnMappingCollection Mappings
        {
            get
            {
                if (this.mappings == null)
                {
                    this.mappings = new DataColumnMappingCollection();
                }
                return this.mappings;
            }
            set
            {
                this.mappings = value;
            }
        }

        [DataSourceXmlAttribute, MergableProperty(false), DefaultValue("")]
        public string Name
        {
            get
            {
                return this.dataTable.TableName;
            }
            set
            {
                if (this.dataTable.TableName != value)
                {
                    if (this.CollectionParent != null)
                    {
                        this.CollectionParent.ValidateUniqueName(this, value);
                    }
                    this.dataTable.TableName = value;
                }
            }
        }

        internal override StringCollection NamingPropertyNames
        {
            get
            {
                return this.namingPropNames;
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
                if (this.owner != value)
                {
                    if (this.owner != null)
                    {
                        string text1 = this.owner.DataSet.Namespace;
                    }
                    this.owner = value;
                }
            }
        }

        public DbSourceParameterCollection Parameters
        {
            get
            {
                DbSource mainSource = this.MainSource as DbSource;
                if ((mainSource != null) && (mainSource.SelectCommand != null))
                {
                    return mainSource.SelectCommand.Parameters;
                }
                return null;
            }
        }

        [Browsable(false)]
        public DataColumn[] PrimaryKeyColumns
        {
            get
            {
                return this.DataTable.PrimaryKey;
            }
            set
            {
                this.AddRemoveConstraintMonitor(false);
                try
                {
                    base.SetPropertyValue("PrimaryKey", value);
                    this.OnConstraintChanged();
                }
                finally
                {
                    this.AddRemoveConstraintMonitor(true);
                }
            }
        }

        internal CodeGenPropertyCache PropertyCache
        {
            get
            {
                return this.codeGenPropertyCache;
            }
            set
            {
                this.codeGenPropertyCache = value;
            }
        }

        [DataSourceXmlAttribute, Browsable(false), DefaultValue((string) null)]
        public string Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        [Browsable(false)]
        public string PublicTypeName
        {
            get
            {
                switch (this.tableType)
                {
                    case System.Data.Design.TableType.DataTable:
                        return "DataTable";

                    case System.Data.Design.TableType.RadTable:
                        return "DataTable";
                }
                return null;
            }
        }

        [Browsable(false)]
        public DbSourceCommand SelectCommand
        {
            get
            {
                return this.EnsureDbSource().SelectCommand;
            }
            set
            {
                this.EnsureDbSource().SelectCommand = value;
            }
        }

        [Browsable(false), DataSourceXmlSubItem(typeof(Source))]
        public SourceCollection Sources
        {
            get
            {
                if (this.sources == null)
                {
                    this.sources = new SourceCollection(this);
                }
                return this.sources;
            }
        }

        [Browsable(false)]
        public System.Data.Design.TableType TableType
        {
            get
            {
                return this.tableType;
            }
            set
            {
                this.tableType = value;
                if (this.tableType == System.Data.Design.TableType.RadTable)
                {
                    this.DataAccessor = new System.Data.Design.DataAccessor(this);
                }
                else
                {
                    this.DataAccessor = null;
                }
            }
        }

        [DefaultValue((string) null)]
        public DbSourceCommand UpdateCommand
        {
            get
            {
                return this.EnsureDbSource().UpdateCommand;
            }
            set
            {
                this.EnsureDbSource().UpdateCommand = value;
            }
        }

        [DataSourceXmlAttribute, Browsable(false), DefaultValue((string) null)]
        public string UserDataComponentName
        {
            get
            {
                return this.userDataComponentName;
            }
            set
            {
                this.userDataComponentName = value;
            }
        }

        internal string UserTableName
        {
            get
            {
                return (this.dataTable.ExtendedProperties[EXTPROPNAME_USER_TABLENAME] as string);
            }
            set
            {
                this.dataTable.ExtendedProperties[EXTPROPNAME_USER_TABLENAME] = value;
            }
        }

        [DataSourceXmlAttribute(ItemType=typeof(bool)), Browsable(false), DefaultValue(false)]
        public bool WebServiceAttribute
        {
            get
            {
                return this.webServiceAttribute;
            }
            set
            {
                this.webServiceAttribute = value;
            }
        }

        [DataSourceXmlAttribute, Browsable(false)]
        public string WebServiceDescription
        {
            get
            {
                return this.webServiceDescription;
            }
            set
            {
                this.webServiceDescription = value;
            }
        }

        [Browsable(false), DataSourceXmlAttribute]
        public string WebServiceNamespace
        {
            get
            {
                return this.webServiceNamespace;
            }
            set
            {
                this.webServiceNamespace = value;
            }
        }

        internal class CodeGenPropertyCache
        {
            private Type adapterType;
            private Type connectionType;
            private DesignTable designTable;
            private string tamAdapterPropName;
            private string tamAdapterVarName;
            private Type transactionType;

            internal CodeGenPropertyCache(DesignTable designTable)
            {
                this.designTable = designTable;
            }

            internal Type AdapterType
            {
                get
                {
                    if (this.adapterType == null)
                    {
                        if (((this.designTable == null) || (this.designTable.Connection == null)) || (this.designTable.Connection.Provider == null))
                        {
                            return null;
                        }
                        DbProviderFactory factory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                        if (factory != null)
                        {
                            DataAdapter adapter = factory.CreateDataAdapter();
                            if (adapter != null)
                            {
                                this.adapterType = adapter.GetType();
                            }
                        }
                    }
                    return this.adapterType;
                }
            }

            internal Type ConnectionType
            {
                get
                {
                    if (((this.connectionType == null) && (this.designTable != null)) && (this.designTable.Connection != null))
                    {
                        IDbConnection connection = this.designTable.Connection.CreateEmptyDbConnection();
                        if (connection != null)
                        {
                            this.connectionType = connection.GetType();
                        }
                    }
                    return this.connectionType;
                }
            }

            internal string TAMAdapterPropName
            {
                get
                {
                    return this.tamAdapterPropName;
                }
                set
                {
                    this.tamAdapterPropName = value;
                }
            }

            internal string TAMAdapterVarName
            {
                get
                {
                    return this.tamAdapterVarName;
                }
                set
                {
                    this.tamAdapterVarName = value;
                }
            }

            internal Type TransactionType
            {
                get
                {
                    if (this.transactionType == null)
                    {
                        if (((this.designTable == null) || (this.designTable.Connection == null)) || (this.designTable.Connection.Provider == null))
                        {
                            return null;
                        }
                        DbProviderFactory factory = ProviderManager.GetFactory(this.designTable.Connection.Provider);
                        if (factory != null)
                        {
                            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(factory.CreateCommand().GetType()))
                            {
                                if (StringUtil.EqualValue(descriptor.Name, "Transaction"))
                                {
                                    this.transactionType = descriptor.PropertyType;
                                    break;
                                }
                            }
                        }
                        if (this.transactionType == null)
                        {
                            this.transactionType = typeof(IDbTransaction);
                        }
                    }
                    return this.transactionType;
                }
            }
        }
    }
}

