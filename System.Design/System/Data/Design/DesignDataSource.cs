namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    [DataSourceXmlClass("DataSource")]
    internal class DesignDataSource : DataSourceComponent, IDataSourceNamedObject, INamedObject, IDataSourceCommandTarget
    {
        private System.Data.DataSet dataSet;
        private int defaultConnectionIndex;
        private DesignConnectionCollection designConnections;
        private DesignRelationCollection designRelations;
        private DesignTableCollection designTables;
        private const string EXTPROPNAME_ENABLE_TABLEADAPTERMANAGER = "EnableTableAdapterManager";
        internal static string EXTPROPNAME_GENERATOR_DATASETNAME = "Generator_DataSetName";
        internal static string EXTPROPNAME_USER_DATASETNAME = "Generator_UserDSName";
        private string functionsComponentName;
        private string generatorFunctionsComponentClassName;
        private TypeAttributes modifier = TypeAttributes.Public;
        private StringCollection namingPropNames = new StringCollection();
        private System.Data.SchemaSerializationMode schemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
        private DataSourceXmlSerializer serializer;
        private SourceCollection sources;
        private string userFunctionsComponentName;

        private bool CanRemoveChild(object child)
        {
            bool flag = false;
            Type c = child.GetType();
            if (typeof(DesignTable).IsAssignableFrom(c))
            {
                return this.DesignTables.Contains((DesignTable) child);
            }
            if (typeof(DesignRelation).IsAssignableFrom(c))
            {
                return this.DesignRelations.Contains((DesignRelation) child);
            }
            if (typeof(IDesignConnection).IsAssignableFrom(c))
            {
                return this.DesignConnections.Contains((IDesignConnection) child);
            }
            if (typeof(Source).IsAssignableFrom(c))
            {
                flag = this.Sources.Contains((Source) child);
            }
            return flag;
        }

        internal ArrayList GetRelatedRelations(ICollection tableList)
        {
            ArrayList list = new ArrayList();
            foreach (DesignRelation relation in this.DesignRelations)
            {
                DesignTable parentDesignTable = relation.ParentDesignTable;
                DesignTable childDesignTable = relation.ChildDesignTable;
                foreach (object obj2 in tableList)
                {
                    if ((parentDesignTable == obj2) || (childDesignTable == obj2))
                    {
                        list.Add(relation);
                        break;
                    }
                }
            }
            return list;
        }

        internal void ReadDataSourceExtraInformation(XmlTextReader xmlTextReader)
        {
            XmlDocument document = new XmlDocument();
            XmlNode newChild = document.ReadNode(xmlTextReader);
            document.AppendChild(newChild);
            if (this.serializer != null)
            {
                this.serializer.DeserializeBody((XmlElement) newChild, this);
            }
        }

        private void ReadXmlSchema(DataSourceXmlTextReader xmlReader)
        {
            this.designConnections = new DesignConnectionCollection(this);
            this.designTables = new DesignTableCollection(this);
            this.designRelations = new DesignRelationCollection(this);
            this.sources = new SourceCollection(this);
            this.serializer = new DataSourceXmlSerializer();
            this.dataSet = new System.Data.DataSet();
            this.dataSet.Locale = CultureInfo.InvariantCulture;
            System.Data.DataSet set = new System.Data.DataSet {
                Locale = CultureInfo.InvariantCulture
            };
            set.ReadXmlSchema(xmlReader);
            this.dataSet = set;
            foreach (DataTable table in this.dataSet.Tables)
            {
                DesignTable table2 = this.designTables[table.TableName];
                if (table2 == null)
                {
                    this.designTables.Add(new DesignTable(table, TableType.DataTable));
                }
                else
                {
                    table2.DataTable = table;
                }
                foreach (Constraint constraint in table.Constraints)
                {
                    ForeignKeyConstraint foreignKeyConstraint = constraint as ForeignKeyConstraint;
                    if (foreignKeyConstraint != null)
                    {
                        this.designRelations.Add(new DesignRelation(foreignKeyConstraint));
                    }
                }
            }
            foreach (DataRelation relation in this.dataSet.Relations)
            {
                DesignRelation relation2 = this.designRelations[relation.ChildKeyConstraint];
                if (relation2 != null)
                {
                    relation2.DataRelation = relation;
                }
                else
                {
                    this.designRelations.Add(new DesignRelation(relation));
                }
            }
            foreach (Source source in this.Sources)
            {
                this.SetConnectionProperty(source);
            }
            foreach (DesignTable table3 in this.DesignTables)
            {
                this.SetConnectionProperty(table3.MainSource);
                foreach (Source source2 in table3.Sources)
                {
                    this.SetConnectionProperty(source2);
                }
            }
            this.serializer.InitializeObjects();
        }

        public void ReadXmlSchema(Stream stream, string baseURI)
        {
            DataSourceXmlTextReader xmlReader = new DataSourceXmlTextReader(this, stream, baseURI);
            this.ReadXmlSchema(xmlReader);
        }

        public void ReadXmlSchema(TextReader textReader, string baseURI)
        {
            DataSourceXmlTextReader xmlReader = new DataSourceXmlTextReader(this, textReader, baseURI);
            this.ReadXmlSchema(xmlReader);
        }

        private void RemoveChild(object child)
        {
            Type c = child.GetType();
            if (typeof(DesignTable).IsAssignableFrom(c))
            {
                this.DesignTables.Remove((DesignTable) child);
            }
            else if (typeof(DesignRelation).IsAssignableFrom(c))
            {
                this.DesignRelations.Remove((DesignRelation) child);
            }
            else if (typeof(IDesignConnection).IsAssignableFrom(c))
            {
                this.DesignConnections.Remove((IDesignConnection) child);
            }
            else if (typeof(Source).IsAssignableFrom(c))
            {
                this.Sources.Remove((Source) child);
            }
        }

        private void SetConnectionProperty(Source source)
        {
            DbSource source2 = source as DbSource;
            if (source2 != null)
            {
                string connectionRef = source2.ConnectionRef;
                if ((connectionRef != null) && (connectionRef.Length != 0))
                {
                    IDesignConnection connection = this.DesignConnections.Get(connectionRef);
                    if (connection != null)
                    {
                        source2.Connection = connection;
                    }
                }
            }
        }

        void IDataSourceCommandTarget.AddChild(object child, bool fixName)
        {
            Type c = child.GetType();
            if (typeof(DesignTable).IsAssignableFrom(c))
            {
                this.DesignTables.Add((DesignTable) child);
            }
            else if (typeof(DesignRelation).IsAssignableFrom(c))
            {
                this.DesignRelations.Add((DesignRelation) child);
            }
            else if (typeof(IDesignConnection).IsAssignableFrom(c))
            {
                this.DesignConnections.Add((IDesignConnection) child);
            }
            else if (typeof(Source).IsAssignableFrom(c))
            {
                this.Sources.Add((Source) child);
            }
        }

        bool IDataSourceCommandTarget.CanAddChildOfType(Type childType)
        {
            return (((typeof(DesignTable).IsAssignableFrom(childType) || typeof(IDesignConnection).IsAssignableFrom(childType)) || typeof(Source).IsAssignableFrom(childType)) || (typeof(DesignRelation).IsAssignableFrom(childType) && (this.DesignTables.Count > 0)));
        }

        bool IDataSourceCommandTarget.CanInsertChildOfType(Type childType, object refChild)
        {
            if (typeof(Source).IsAssignableFrom(childType))
            {
                return (refChild is Source);
            }
            if (typeof(IDesignConnection).IsAssignableFrom(childType))
            {
                return (refChild is IDesignConnection);
            }
            return typeof(DesignTable).IsAssignableFrom(childType);
        }

        bool IDataSourceCommandTarget.CanRemoveChildren(ICollection children)
        {
            foreach (object obj2 in children)
            {
                if (!this.CanRemoveChild(obj2))
                {
                    return false;
                }
            }
            return true;
        }

        object IDataSourceCommandTarget.GetObject(int index, bool getSiblingIfOutOfRange)
        {
            throw new NotImplementedException();
        }

        int IDataSourceCommandTarget.IndexOf(object child)
        {
            throw new NotImplementedException();
        }

        void IDataSourceCommandTarget.InsertChild(object child, object refChild)
        {
            if (child is DesignTable)
            {
                this.DesignTables.InsertBefore(child, refChild);
            }
            else if (child is DesignRelation)
            {
                this.DesignRelations.InsertBefore(child, refChild);
            }
            else if (child is Source)
            {
                this.Sources.InsertBefore(child, refChild);
            }
            else if (child is IDesignConnection)
            {
                this.DesignConnections.InsertBefore(child, refChild);
            }
        }

        void IDataSourceCommandTarget.RemoveChildren(ICollection children)
        {
            SortedList list = new SortedList();
            foreach (object obj2 in children)
            {
                if (obj2 is DesignTable)
                {
                    list.Add(-this.DesignTables.IndexOf((DesignTable) obj2), obj2);
                }
                else
                {
                    this.RemoveChild(obj2);
                }
            }
            foreach (DesignRelation relation in this.GetRelatedRelations(children))
            {
                this.RemoveChild(relation);
            }
            foreach (object obj3 in list.Values)
            {
                if (obj3 is DesignTable)
                {
                    this.RemoveChild(obj3);
                }
            }
        }

        internal System.Data.DataSet DataSet
        {
            get
            {
                if (this.dataSet == null)
                {
                    this.dataSet = new System.Data.DataSet();
                    this.dataSet.Locale = CultureInfo.InvariantCulture;
                    this.dataSet.EnforceConstraints = false;
                }
                return this.dataSet;
            }
        }

        [DisplayName("DefaultConnection")]
        public DesignConnection DefaultConnection
        {
            get
            {
                if (((this.DesignConnections.Count > 0) && (this.defaultConnectionIndex >= 0)) && (this.defaultConnectionIndex < this.DesignConnections.Count))
                {
                    return (this.DesignConnections[this.defaultConnectionIndex] as DesignConnection);
                }
                return null;
            }
        }

        [DataSourceXmlSubItem(Name="Connections", ItemType=typeof(DesignConnection)), DisplayName("Connections"), Browsable(false)]
        public DesignConnectionCollection DesignConnections
        {
            get
            {
                if (this.designConnections == null)
                {
                    this.designConnections = new DesignConnectionCollection(this);
                }
                return this.designConnections;
            }
        }

        [Browsable(false)]
        public DesignRelationCollection DesignRelations
        {
            get
            {
                if (this.designRelations == null)
                {
                    this.designRelations = new DesignRelationCollection(this);
                }
                return this.designRelations;
            }
        }

        [Browsable(false), DataSourceXmlSubItem(Name="Tables", ItemType=typeof(DesignConnection))]
        public DesignTableCollection DesignTables
        {
            get
            {
                if (this.designTables == null)
                {
                    this.designTables = new DesignTableCollection(this);
                }
                return this.designTables;
            }
        }

        [DefaultValue(true)]
        public bool EnableTableAdapterManager
        {
            get
            {
                bool result = false;
                bool.TryParse(this.DataSet.ExtendedProperties["EnableTableAdapterManager"] as string, out result);
                return result;
            }
            set
            {
                this.DataSet.ExtendedProperties["EnableTableAdapterManager"] = value.ToString();
            }
        }

        [Browsable(false), DefaultValue((string) null), DataSourceXmlAttribute]
        public string FunctionsComponentName
        {
            get
            {
                return this.functionsComponentName;
            }
            set
            {
                this.functionsComponentName = value;
            }
        }

        internal string GeneratorDataSetName
        {
            get
            {
                return (this.DataSet.ExtendedProperties[EXTPROPNAME_GENERATOR_DATASETNAME] as string);
            }
            set
            {
                this.DataSet.ExtendedProperties[EXTPROPNAME_GENERATOR_DATASETNAME] = value;
            }
        }

        [DataSourceXmlAttribute, Browsable(false), DefaultValue((string) null)]
        public string GeneratorFunctionsComponentClassName
        {
            get
            {
                return this.generatorFunctionsComponentClassName;
            }
            set
            {
                this.generatorFunctionsComponentClassName = value;
            }
        }

        [DefaultValue(1), DataSourceXmlAttribute]
        public TypeAttributes Modifier
        {
            get
            {
                return this.modifier;
            }
            set
            {
                this.modifier = value;
            }
        }

        [MergableProperty(false), DefaultValue("")]
        public string Name
        {
            get
            {
                return this.DataSet.DataSetName;
            }
            set
            {
                this.DataSet.DataSetName = value;
            }
        }

        internal override StringCollection NamingPropertyNames
        {
            get
            {
                return this.namingPropNames;
            }
        }

        [Browsable(false)]
        public string PublicTypeName
        {
            get
            {
                return "DataSet";
            }
        }

        [DataSourceXmlAttribute]
        public System.Data.SchemaSerializationMode SchemaSerializationMode
        {
            get
            {
                return this.schemaSerializationMode;
            }
            set
            {
                this.schemaSerializationMode = value;
            }
        }

        [DataSourceXmlSubItem(typeof(Source)), Browsable(false)]
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

        internal string UserDataSetName
        {
            get
            {
                return (this.DataSet.ExtendedProperties[EXTPROPNAME_USER_DATASETNAME] as string);
            }
            set
            {
                this.DataSet.ExtendedProperties[EXTPROPNAME_USER_DATASETNAME] = value;
            }
        }

        [DataSourceXmlAttribute, DefaultValue((string) null), Browsable(false)]
        public string UserFunctionsComponentName
        {
            get
            {
                return this.userFunctionsComponentName;
            }
            set
            {
                this.userFunctionsComponentName = value;
            }
        }
    }
}

